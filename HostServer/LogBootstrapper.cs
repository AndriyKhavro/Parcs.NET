using System;
using Serilog;

namespace HostServer
{
    public static class LogConfigurator
    {
        public static void Configure()
        {
            string connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.LogConnectionString);
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.RollingFile("HostServer.log.txt");

            if (!String.IsNullOrEmpty(connectionString))
            {
                log.WriteTo.MSSqlServer(connectionString, "LogEntries", autoCreateSqlTable: true);
            }

            Log.Logger = log.CreateLogger();
        }
    }
}
