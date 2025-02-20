using System.Security.Cryptography;
using AppManager.DataModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppManager.Controllers;


[Route("api/App")]
[ApiController]
public class AppClient(MyDbContext context, ILogger<App>? logger) : ControllerBase
{
    private readonly MyDbContext _context = context;
    private readonly ILogger<App>? _logger = logger;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<App>> ReadAppAsync(int id)
    {
        try
        {
            var result = await _context.Apps.SingleOrDefaultAsync(app => app.AppId == id);
            if (result == null)
            {
                _logger?.LogWarning($"An app with id = {id} was not found!");
                return NotFound();
            }
            else
            {
                _logger?.LogInformation($"Successfully fetched an app with id = {id}");
                return result;    
            }
            
        }
        catch (Exception e)
        {
            _logger?.LogError(e,$"An exception occured while fetching an app with id = {id}!");
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
        string _adminSecretReal = "idk";
        
        try
        {
            if (_adminSecretReal != adminSecret)
            {
                _logger?.LogWarning("Unauthorized attempt to create an app!");
                return Unauthorized();
            }

            _context.Apps.Add(app with{Secret = GenerateAppSecret()});
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ReadAppAsync),new {id = app.AppId} ,app);
        }
        catch (Exception e)
        {
            _logger?.LogError(e,"An exception occured while creating an app!");
            throw;
        }
    }
    
}