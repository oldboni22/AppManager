using System.Security.Cryptography;
using System.Text;
using AppManager.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppManager.Controllers;

public record struct LoginCreateParams
    (string HardwareId,string AppSecret,string Email,string Password);

[Route("api/[controller]")]
[ApiController]
public class AuthController(MyDbContext context, ILogger<Login>? logger) : ControllerBase
{

    private readonly MyDbContext _context = context;
    private readonly ILogger<Login>? _logger = logger;

    #region Utils
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

    string GenerateDeviceHash(string hardwareId, string salt)
    {
        string hash;
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(salt + hardwareId);
            hash = Convert.ToBase64String(sha.ComputeHash(bytes));
        }
        
        return hash.Substring(0, 16);
    }

    bool IsDeviceHashLegit(Login login,string input)
    {
        string hash;
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(login.HardwareSalt + input);
            hash = Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        return hash.Substring(0,16) == login.HardwareHash;
    }
    #endregion
    
    #region Login
    async Task<Login?> TryGetLoginAsync(string hardwareId, int userId)
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

        return null;
    }
    [HttpPost("Login/")]
    public async Task<ActionResult<Login>> CreateLoginAsync(LoginCreateParams @params)
    {
        try
        {
            App? app = await _context.TryGetAppAsync(@params.AppSecret);
            if (app == null)
            {
                _logger?.LogWarning("The provided app secret is invalid!");
                return NotFound();
            }
            User? user = await _context.TryGetUserAsync(@params.Email);
            if(user == null)
            {
                _logger?.LogWarning("There's no user associated with the provided email!");
                return NotFound();
            }
            if(IsPasswordValid(user,@params.Password) is false)
            {
                _logger?.LogWarning("The given password is incorrect!");
                return Forbid();
            }

            string updateToken = JwtAuth.GenerateUpdateToken(user.UserId);
            var salt = this.GenerateSalt();
            
            var login = new Login()
            {
                HardwareSalt = salt,
                HardwareHash = GenerateDeviceHash(@params.HardwareId,salt),
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
    #endregion

    #region Session

    [HttpPost("Session/{hardwareId},/{userId}")]
    public async Task<ActionResult<Session>> CreateSessionAsync(string hardwareId,int userId)
    {
        try
        {
            var login = await TryGetLoginAsync(hardwareId, userId);
            if (login == null)
            {
                _logger?.LogWarning("Unauthorized attempt to create a session");
                return Unauthorized();
            }

            var session = new Session()
            {
                AccessToken = JwtAuth.GenerateAccessToken(userId),
                Login = login,
                LoginId = login.LoginId
            };

            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();
            _logger?.LogInformation("Successfully created a session" + session.ToString());

            return session;
        }
        catch (Exception e)
        {
            _logger?.LogError(e,"An error occured while creating a session");
            throw;
        }
        
    }
    #endregion
    
    
}