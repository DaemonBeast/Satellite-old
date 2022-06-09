using Satellite.Core;
using Satellite.Core.Interfaces;
using Satellite.Core.Settings;
using Satellite.Core.Utilities;
using Satellite.S3.Extensions;
using Satellite.S3.Settings;
using Serilog;
using Serilog.Events;

namespace Satellite;

public static class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting {Name} v{Version}", DotnetUtilities.Name, DotnetUtilities.Version);

            ConfigureWebApplication(BuildWebApplication(args)).Run();

            return 0;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "{Name} terminated unexpectedly", DotnetUtilities.Name);

            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static WebApplication BuildWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        builder.Configuration.AddJsonFile("config.json");

        builder.Services.AddControllers();

        builder.Services.AddHttpClient();

        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddSwaggerGen();

        builder.Services
            .Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.Section))
            .AddSingleton<ModResolver>();

        var s3Settings = builder.Configuration
            .GetSection(S3Settings.Section)
            .Get<S3Settings>() ?? new S3Settings();

        if (s3Settings.Enabled)
        {
            builder.Host.AddSatelliteS3();
        }
        else
        {
            builder.Services.AddSingleton<IStorage, LocalStorage>();
        }

        builder.Services.AddSingleton<FileResolver>();

        return builder.Build();
    }

    private static WebApplication ConfigureWebApplication(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        
        app.MapControllers();
        
        return app;
    }
}