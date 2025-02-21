﻿using AppManager.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AppManager.Controllers
{
    public class SessionCreateParams(string hardwareFingerprint,string appSecret,string email,string password)
    {
        public string HardwareFingerprint { get; init; } = hardwareFingerprint;
        public string AppSecret { get; init; } = appSecret;
        public string Email { get; init; } = email;
        public string Password { get; init; } = password;
    }
    public class SessionController(MyDbContext context,ILogger<Session> logger) : ControllerBase
    {
        private readonly MyDbContext _context = context;
        private readonly ILogger<Session> _logger = logger;


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
        [HttpPost]
        public async Task<ActionResult<Session>> CreateSessionAsync(SessionCreateParams @params)
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


                string accessToken = "";
                string refreshToken = "";

                var session = new Session
                {
                    AppId = app.AppId,
                    HardwareFingerprint = @params.HardwareFingerprint,
                    AcesToken = accessToken,
                    RefreshToken = refreshToken
                };

                _context.Sessions.Add(session);
                
                await _context.SaveChangesAsync();
                _logger?.LogInformation("Successfully created a new session");
                return session;

            }
            catch(Exception e)
            {
                _logger?.LogError(e, "An exception occured while creating a session");
                throw;
            }
        }

    }
}
