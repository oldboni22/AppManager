using AppManager.DataModels;
using Microsoft.EntityFrameworkCore;

namespace AppManager;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
        
    }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>().HasOne<App>(x => x.App);
    }

    public DbSet<App> Apps { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
}