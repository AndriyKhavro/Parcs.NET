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

           // AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            return job;
        }

        private void FinishJob(IJob job)
        {
            job.FinishJob();
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

        //private static IJob job;

        //private static void OnProcessExit(object sender, EventArgs e)
        //{
        //    if (job != null && !job.IsFinished)
        //    {
        //        job.FinishJob();
        //    }
        //}

    }
}
