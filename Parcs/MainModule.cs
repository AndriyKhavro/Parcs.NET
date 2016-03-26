using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Parcs
{
    public abstract class MainModule: IModule
    {
        private IJob CreateJob()
        {
            var job = new Job();
            if (!job.AddFile(Assembly.GetEntryAssembly().Location))
            {
                throw new ParcsException();
            }
            
            return job;
        }

        public void RunModule()
        {
            IJob job = null;

            try
            {
                job = CreateJob();
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

        public abstract void Run(ModuleInfo info);
    }
}
