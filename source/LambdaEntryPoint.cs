using Sentry.Extensibility;

namespace Api
{
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder.UseSentry(cfg =>
            {
                cfg.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
                cfg.MaxRequestBodySize = RequestSize.Always;
                cfg.MinimumBreadcrumbLevel = LogLevel.Trace;
                cfg.MinimumEventLevel = LogLevel.Warning;
                cfg.FlushOnCompletedRequest = true;
                cfg.TracesSampleRate = 1.0;
                cfg.SendDefaultPii = true;
                cfg.EnableTracing = true;
                cfg.Debug = true;
            });

            builder.UseStartup<Startup>();
        }

        protected override void Init(IHostBuilder builder)
        {
        }
    }
}