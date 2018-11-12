using System.Reflection;
using System.Threading;

namespace Parcs
{
    public abstract class MainModule: IModule
    {
        private IJob CreateJob(int priority, string username)
        {
            var job = new Job(priority, username);
            job.AddFile(Assembly.GetEntryAssembly().Location);            
            return job;
        }

        public void RunModule(IModuleOptions options = null)
        {
            if (!string.IsNullOrEmpty(options?.ServerIp))
            {
                Job.SetServerIp(options.ServerIp);
            }

            IJob job = null;

            try
            {
                job = CreateJob(options?.Priority ?? 0, options?.Username ?? "");
                Run(new ModuleInfo(job, null));
            }

            finally
            {
                job?.FinishJob();
            }
        }

        public abstract void Run(ModuleInfo info, CancellationToken token = default(CancellationToken));
    }
}
