using AppManager.DataModels;
using Microsoft.EntityFrameworkCore;

namespace AppManager;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>().HasOne<Login>(s => s.Login);
    }

    public DbSet<App> Apps { get; init; }
    public DbSet<User> Users { get; init; }
    public DbSet<Session> Sessions { get; init; }
    public DbSet<Login> Logins { get; init; }
}