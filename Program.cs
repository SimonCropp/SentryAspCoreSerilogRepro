using Microsoft.AspNetCore;
using Serilog;
using Serilog.Events;

namespace Sentry.Samples.AspNetCore.Serilog;

public class Program
{
    public static void Main(string[] args) => BuildWebHost(args).Run();

    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)

            .UseSerilog((_, c) =>
                c.Enrich.FromLogContext()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.Sentry(s =>
                    {
                        s.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                        s.MinimumEventLevel = LogEventLevel.Error;
                    }))

            .UseSentry()

            .Configure(a =>
            {
                // An example ASP.NET Core middleware that throws an
                // exception when serving a request to path: /throw
                a.Use(async (context, next) =>
                {
                    // See MinimumBreadcrumbLevel set at the Serilog configuration above
                    Log.Logger.Debug("Static Serilog logger debug log stored as breadcrumbs.");

                    var log = context.RequestServices.GetService<ILoggerFactory>()
                        .CreateLogger<Program>();

                    log.LogInformation("Handling some request...");

                    // Sends an event which includes the info and debug messages above
                    Log.Logger.Error("Logging using static Serilog directly also goes to Sentry.");

                    var hub = context.RequestServices.GetService<IHub>();
                    hub.ConfigureScope(s =>
                    {
                        // More data can be added to the scope like this:
                        s.SetTag("Sample", "ASP.NET Core"); // indexed by Sentry
                        s.SetExtra("Extra!", "Some extra information");
                    });

                    // Logging through the ASP.NET Core `ILogger` while using Serilog
                    log.LogInformation("Logging info...");
                    log.LogWarning("Logging some warning!");

                    await next();
                });
            })
            .Build();
}
