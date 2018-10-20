using System;
using Serilog;

namespace HostServer
{
    public static class LogConfigurator
    {
        public static void Configure()
        {
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.RollingFile("HostServer.log.txt");

            string connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.LogConnectionString);

            if (!String.IsNullOrWhiteSpace(connectionString))
            {
                log.WriteTo.MSSqlServer(connectionString, "LogEntries", autoCreateSqlTable: true);
            }

            string elasticSearchUrl = Environment.GetEnvironmentVariable(EnvironmentVariables.LogElasticSearchUrl);

            if (!String.IsNullOrWhiteSpace(elasticSearchUrl))
            {
                log.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticSearchUrl))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv5
                });
            }

            Log.Logger = log.CreateLogger();
        }
    }
}
