using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parcs
{
    public class ConcurrentPoint : Point
    {
        internal ConcurrentPoint(IJob job, int parentNumber) : base(job, parentNumber) { }

        protected override void Initialize()
        {
            _taskQueue.StartNewTask(LockInitialize);
        }

        private void LockInitialize()
        {
            lock (TaskQueue.syncRoot)
            {
                base.Initialize();
            }
        }

        protected override IChannel CreateNewChannel()
        {
            return new ConcurrentChannel(this, _taskQueue) { From = Number };
        }

        protected override void WriteNumberToChannel()
        {
            _taskQueue.StartNewTask(base.WriteNumberToChannel);
        }

        private TaskQueue _taskQueue = new TaskQueue();

        //protected void StartNewTask(Action action)
        //{
        //    if (_task == null)
        //    {
        //        _task = Task.Factory.StartNew(action);
        //    }

        //    else
        //    {
        //        _task = _task.ContinueWith((prevTask) => action);
        //    }
        //}

        //private Task _task;
    }
}
