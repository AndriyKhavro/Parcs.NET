using Serilog;

namespace DaemonPr
{
    public static class LogConfigurator
    {
        public static void Configure()
        {
            const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}";

            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .WriteTo.Console(outputTemplate: outputTemplate)
                .WriteTo.RollingFile("Daemon.log.txt", outputTemplate: outputTemplate);

            Log.Logger = log.CreateLogger();
        }
    }
}
