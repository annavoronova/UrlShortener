using System;
using System.IO;
using System.Web;
using Serilog;
using Serilog.Events;

namespace UrlShortener.Web
{
    public static class SerilogConfig
    {
        public static void Configure()
        {
            // Ensure Logs directory exists
            var logsPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Logs");
            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.File(
                    path: Path.Combine(logsPath, "log{Date}.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Serilog configured successfully");
        }
    }
}
