using Sentry.Extensibility;

namespace Api
{
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(cfg =>
            {
                cfg.UseSentry(o =>
                {
                    o.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
                    o.MaxRequestBodySize = RequestSize.Always;
                    o.MinimumBreadcrumbLevel = LogLevel.Trace;
                    o.MinimumEventLevel = LogLevel.Warning;
                    o.FlushOnCompletedRequest = true;
                    o.TracesSampleRate = 1.0;
                    o.SendDefaultPii = true;
                    o.EnableTracing = true;
                    o.Debug = true;
                });

                cfg.UseStartup<Startup>();

            }).Build().Run();
        }
    }
}