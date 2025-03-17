using AppManager.DataModels;
using Microsoft.EntityFrameworkCore;

namespace AppManager;

public static class MyDbContextExtensions
{
    public static async Task<User?> TryGetUserAsync(this MyDbContext context,string email) =>
        await context.Users.SingleOrDefaultAsync(user => user.Email == email);
    
    public static async Task<App?> TryGetAppAsync(this MyDbContext context,string appSecret) =>
        await context.Apps.SingleOrDefaultAsync(app => app.Secret == appSecret);
    
}