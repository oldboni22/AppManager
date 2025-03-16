using System.Security.Cryptography;
using System.Text;
using AppManager.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppManager.Controllers;

public record struct LoginCreateParams
    (string HardwareId,string AppSecret,string Email,string Password);

[Route("api/Login")]
[ApiController]
public class LoginController(MyDbContext context, ILogger<Login> logger) : ControllerBase
{

    private readonly MyDbContext _context = context;
    private readonly ILogger<Login> _logger = logger;


    Task<App?> TryGetAppAsync(string appSecret) =>
        _context.Apps.SingleOrDefaultAsync(app => app.Secret == appSecret);

    Task<User?> TryGetUserAsync(string email) =>
        _context.Users.SingleOrDefaultAsync(user => user.Email == email);
    
    
    bool IsPasswordValid(User user, string input)
    {
        string result;
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(user.PasswordSalt + input);
            result = Convert.ToBase64String(sha.ComputeHash(bytes));
        }
        return result == user.PasswordHash;
    }

    string GenerateSalt()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes);
    }

    string GenerateDeviceHash(string deviceId, string salt)
    {
        string hash;
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(salt + deviceId);
            hash = Convert.ToBase64String(sha.ComputeHash(bytes));
        }
        
        return hash.Substring(0, 16);
    }

    bool IsDeviceHashLegit(Login login,string input)
    {
        string hash;
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(login.DeviceSalt + input);
            hash = Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        return hash.Substring(0,16) == login.DeviceHash;
    }
    
    [HttpGet("{hardwareId},{userId}")]
    public async Task<ActionResult<Login>> TryGetLoginAsync(string hardwareId, int userId)
    {
        try
        {
            var logins = _context.Logins.Where(log => log.UserId == userId);
            foreach (var login in logins)
            {
                var token = login.UpdateToken;
                if (JwtAuth.IsUpdateTokenLegit(token, $"{userId}")
                    && IsDeviceHashLegit(login, hardwareId))
                {
                    return login;
                }
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e,"An error occured while trying to fetch a login!");
            throw;
        }

        return NotFound();
    }
    
    
    
    [HttpPost]
    public async Task<ActionResult<Login>> CreateLogin(LoginCreateParams @params)
    {
        try
        {
            App? app = await TryGetAppAsync(@params.AppSecret);
            if (app == null)
            {
                _logger?.LogWarning("The provided app secret is invalid!");
                return NotFound();
            }
            User? user = await TryGetUserAsync(@params.Email);
            if(user == null)
            {
                _logger?.LogWarning("There's no user associated with the provided email!");
                return NotFound();
            }
            if(IsPasswordValid(user,@params.Password) is false)
            {
                _logger?.LogWarning("The given password is incorect!");
                return Forbid();
            }

            string updateToken = JwtAuth.GenerateUpdateToken(user.UserId);
            var salt = GenerateSalt();
            
            var login = new Login()
            {
                DeviceSalt = salt,
                DeviceHash = GenerateDeviceHash(@params.HardwareId,salt),
                UpdateToken = updateToken,
                UserId = user.UserId,
            };

            await _context.Logins.AddAsync(login);
            await _context.SaveChangesAsync();
            _logger?.LogInformation("Successfully created a login " + login.ToString());

            return login;
        }
        catch (Exception e)
        {
            _logger?.LogError(e,"An error occured while creating a login");
            throw;
        }
    }

}