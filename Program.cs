using Microsoft.AspNetCore;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Display;

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
                        s.TextFormatter = new MessageTemplateTextFormatter("[{CorrelationId}] {Message}", null);
                    }))

            .UseSentry(_=>_.Dsn ="https://e8d57cabda394366b25b57bba7c204a6@o1027677.ingest.sentry.io/5994502")

            .Configure(a =>
            {
                a.Use(async (context, next) =>
                {
                    using (LogContext.PushProperty("CorrelationId", "myCorrelationId"))
                    {
                        Log.Logger.Error(DateTime.Now.ToString());

                        await next();
                    }
                });
            })
            .Build();
}
