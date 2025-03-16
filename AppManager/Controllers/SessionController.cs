using AppManager.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AppManager.Controllers
{


    [Route("Api/Session")]
    public class SessionController(MyDbContext context,ILogger<Session> logger) : ControllerBase
    {
        private readonly MyDbContext _context = context;
        private readonly ILogger<Session> _logger = logger;
        

    }
}
