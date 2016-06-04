using System.Linq;

namespace DataAccess.Logs
{
    public class LogEntryRepository : ILogEntryRepository
    {
        private readonly HostServerContext _context = new HostServerContext();
        public IQueryable<LogEntry> Get()
        {
            return _context.Logs;
        }
    }
}
