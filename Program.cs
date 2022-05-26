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
                a.Use(async (context, next) =>
                {
                    Log.Logger.Error(DateTime.Now.ToString());

                    await next();
                });
            })
            .Build();
}
