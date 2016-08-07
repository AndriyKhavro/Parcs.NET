using System;
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

        public void RunModule(string[] args = null, BaseModuleOptions options = null)
        {
            if (options != null && args != null)
            {
                if (!CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    throw new ArgumentException($@"Cannot parse the arguments. Possible usages:
{options.GetUsage()}");
                }

                if (!string.IsNullOrEmpty(options.ServerIp))
                {
                    Job.SetServerIp(options.ServerIp);
                }
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
