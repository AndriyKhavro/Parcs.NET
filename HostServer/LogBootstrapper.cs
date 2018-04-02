using System;
using System.Linq;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace HostServer
{
    public static class LogConfigurator
    {
        public static void Configure()
        {
            string connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.LogConnectionString);

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                SetConnectionString(connectionString);
            }
        }

        private static void SetConnectionString(string connectionString)
        {
            Hierarchy logHierarchy = log4net.LogManager.GetRepository() as Hierarchy;

            if (logHierarchy == null)
            {
                throw new InvalidOperationException
                    ("Can't set connection string as hierarchy is null.");
            }

            var appender = logHierarchy.GetAppenders()
                .OfType<AdoNetAppender>()
                .SingleOrDefault();

            if (appender == null)
            {
                throw new InvalidOperationException
                    ("Can't locate a database appender");
            }

            appender.ConnectionString = connectionString;
            appender.ActivateOptions();
        }
    }
}
