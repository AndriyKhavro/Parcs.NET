using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace HostServer.Persistance.Logs
{
    public class HostServerContext : DbContext
    {
        public DbSet<LogEntry> Logs { get; set; }

        public HostServerContext()
            : base("HostServerContext")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HostServerContext, Configuration>());
            Configuration.LazyLoadingEnabled = false;
        }
    }

    public class Configuration : DbMigrationsConfiguration<HostServerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }
    }
}
