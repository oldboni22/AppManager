using Serilog;

namespace AppManager;

public static class WebApplicationBuilderExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var cfg = new LoggerConfiguration();

        cfg.ReadFrom.Configuration(builder.Configuration);
        cfg.Enrich.FromLogContext();
        cfg.WriteTo.Console();
        cfg.WriteTo.File("logs/.log");

        Log.Logger = cfg.CreateLogger();
        builder.Host.UseSerilog();
    }
}