using System.Security.Cryptography;
using System.Text;
using AppManager.DataModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppManager.Controllers;


[Route("api/App")]
[ApiController]
public class AppController(MyDbContext context, ILogger<App>? logger) : ControllerBase
{
    private readonly MyDbContext _context = context;
    private readonly ILogger<App>? _logger = logger;

    private string _adminSecret = " ";
    
    [HttpGet("{id},{secret}")]
    public async Task<ActionResult<App>> ReadAppAsync(int id,string secret)
    {
        try
        {
            var result = await _context.Apps.SingleOrDefaultAsync(app => app.AppId == id);
            if (result == null)
            {
                _logger?.LogWarning("An app was not found!");
                return NotFound();
            }
            else if (result.Secret != secret || secret != _adminSecret)
            {
                _logger?.LogWarning("An attempt to read app with a wrong secret!");
                return Unauthorized();
            }
            else
            {
                _logger?.LogInformation("Successfully fetched an app");
                return result;    
            }
            
        }
        catch (Exception e)
        {
            _logger?.LogError(e,"An exception occured while fetching an app");
            throw;
        }
    }

    string GenerateAppSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
    
    [HttpPost("{adminSecret}")]
    public async Task<ActionResult<App>> CreateApp(App app,string adminSecret)
    {
        try
        {
            if (_adminSecret != adminSecret)
            {
                _logger?.LogWarning("Unauthorized attempt to create an app!");
                return Unauthorized();
            }

            _context.Apps.Add(app with{Secret = GenerateAppSecret()});
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ReadAppAsync),new {id = app.AppId,secret = app.Secret} ,app);
        }
        catch (Exception e)
        {
            _logger?.LogError(e,"An exception occured while creating an app!");
            throw;
        }
    }

    [HttpDelete("{id},{adminSecret}")]
    public async Task<IActionResult> DeleteAppAsync(int id,string adminSecret)
    {
        try
        {
            if (adminSecret != _adminSecret)
            {
                _logger?.LogWarning("Unauthorized attempt to delete an app!");
                return Unauthorized();
            }
            
            App? app = await _context.Apps.SingleOrDefaultAsync(a => a.AppId == id);
            if (app == null)
            {
                _logger?.LogWarning("An app was not found!");
                return NotFound();
            }

            _context.Apps.Remove(app);
            await _context.SaveChangesAsync();
            
            _logger?.LogInformation("Successfully deleted an app");
            return NoContent();

        }
        catch (Exception e)
        {
            _logger?.LogError(e,"An exception occured while deleting an app !");
            throw;
        }
    }
    
}