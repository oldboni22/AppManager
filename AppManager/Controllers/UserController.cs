using System.Security.Cryptography;
using System.Text;
using AppManager.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppManager.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(ILogger<User>? logger, MyDbContext context) : ControllerBase
{
    private readonly ILogger<User>? _logger = logger;
    private readonly MyDbContext _context = context;
    
    async Task<App?> TryGetApp(string appSecret)
    {
        App? app = await _context.Apps.SingleOrDefaultAsync(a => a.Secret == appSecret);
        return app;
    }

    [HttpGet("{accessToken}")]
    public async Task<ActionResult<User?>> ReadUserAsync(string accessToken)
    {
        try
        {
            User? user = await _context.Users.SingleOrDefaultAsync();
            if (user == null)
            {
                _logger?.LogWarning("A user was not found!");
                return NotFound();
            }
            
            _logger?.LogInformation("Successfully fetched a user");
            return user;
        }
        catch (Exception e)
        {
            _logger?.LogError(e,"An error occured while trying to read a user!");
            throw;
        }
    }
    

    string GeneratePasswordHash(string salt, string password)
    {
        string hash;
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(salt + password);
            hash = Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        return hash;
    }

    [HttpPost("{email},{password},{appSecret}")]
    public async Task<ActionResult<User>> CreateUserAsync(string email,string password,string appSecret)
    {
        try
        {
            if (_context.Apps.Any(app => app.Secret == appSecret) is false)
            {
                _logger?.LogWarning("Unauthorized attempt to create a user!");
                return Unauthorized();
            }

            var salt = this.GenerateSalt();

            var newUser = new User()
            {
                Email = email,
                PasswordSalt = salt,
                PasswordHash = GeneratePasswordHash(salt,password)
            };
            
            await _context.Users.AddAsync(newUser);
            _logger?.LogInformation("Successfully created an app");

            return newUser;
        }
        catch (Exception e)
        {
            _logger?.LogError(e,"An error occured while trying to create a user!");
            throw;
        }
    }
}