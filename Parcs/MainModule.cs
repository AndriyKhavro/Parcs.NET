using System.Reflection;
using System.Threading;

namespace Parcs
{
    public abstract class MainModule: IModule
    {
        private IJob CreateJob(int priority, string username)
        {
            var job = new Job(priority, username);
            if (!job.AddFile(Assembly.GetEntryAssembly().Location))
            {
                throw new ParcsException();
            }
            
            return job;
        }

        public void RunModule(int priority = 0, string username = "")
        {
            IJob job = null;

            try
            {
                job = CreateJob(priority, username);
                Run(new ModuleInfo(job, null));
            }

            finally
            {
                if (job != null)
                {
                    job.FinishJob();
                }
            }
        }

        public abstract void Run(ModuleInfo info, CancellationToken token = default(CancellationToken));
    }
}
