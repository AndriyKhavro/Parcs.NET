using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parcs
{
    public class ConcurrentChannel : IChannel
    {
        volatile Channel channel;

        internal ConcurrentChannel(IPoint point, TaskQueue taskQueue)//: base(reader, writer, works)
        {
            _taskQueue = taskQueue;
            _taskQueue.StartNewTask(() => CreateChannel(point));
        }

        private void CreateChannel(IPoint point)
        {
            channel = new Channel(point.Host.Reader, point.Host.Writer, point.Host.IsConnected) { From = _from };
        }

        public void Close()
        {
            _taskQueue.StartNewTask(() => channel.Close());
        }

        public void WriteData(dynamic data)
        {
            _taskQueue.StartNewTask(() => channel.WriteData(data));
        }

        public void WriteFile(string fullPath)
        {
           _taskQueue.StartNewTask(() => channel.WriteFile(fullPath));
        }

        public void WriteObject(object obj)
        {
           _taskQueue.StartNewTask(() => channel.WriteObject(obj));
        }

        public dynamic ReadData(Type type)
        {
            _taskQueue.Wait();
            return channel.ReadData(type);
            //return _taskQueue.StartNewGenericTask<dynamic>(() => channel.ReadData(type));
        }

        public string ReadFile()
        {
            _taskQueue.Wait();
            return channel.ReadFile();
            //return StartNewGenericTask<string>(() => channel.ReadFile());
        }

        public object ReadObject()
        {
            _taskQueue.Wait();
            return channel.ReadObject();
            //return StartNewGenericTask<object>(() => channel.ReadObject());
        }

        public object ReadObject(Type type)
        {
            _taskQueue.Wait();
            return channel.ReadObject(type);
            //return StartNewGenericTask<object>(() => channel.ReadObject(type));
        }

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

        //protected T StartNewGenericTask<T>(Func<T> func)
        //{
        //    Task<T> t;
        //    if (_task == null)
        //    {
        //        t = Task.Factory.StartNew<T>(func);
        //    }

        //    else
        //    {
        //        t = _task.ContinueWith<T>((prevTask) => func());
        //    }

        //    _task = t;
        //    return t.Result;
        //}

        private TaskQueue _taskQueue;

        public bool Works
        {
            get { return true; }
        }

        public int From
        {
            get
            {
                if (channel == null)
                {
                    return _from;
                }

                return channel.From;
            }
            set
            {
                if (channel == null)
                {
                    _from = value;
                }

                else
                {
                    channel.From = value;
                }
            }
        }

        private int _from;
    }
}
