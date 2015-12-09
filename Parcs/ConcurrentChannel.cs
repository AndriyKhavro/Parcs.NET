using System;

namespace Parcs
{
    public class ConcurrentChannel : IChannel
    {
        volatile Channel _channel;

        internal ConcurrentChannel(IPoint point, TaskQueue taskQueue)//: base(reader, writer, works)
        {
            _taskQueue = taskQueue;
            _taskQueue.StartNewTask(() => CreateChannel(point));
        }

        private void CreateChannel(IPoint point)
        {
            _channel = new Channel(point.Host.Reader, point.Host.Writer, point.Host.IsConnected) { From = _from };
        }
        

        public void Close()
        {
            _taskQueue.StartNewTask(() => _channel.Close());
        }

        public void WriteData(dynamic data)
        {
            _taskQueue.StartNewTask(() => _channel.WriteData(data));
        }

        public void WriteFile(string fullPath)
        {
           _taskQueue.StartNewTask(() => _channel.WriteFile(fullPath));
        }

        public void WriteObject(object obj)
        {
           _taskQueue.StartNewTask(() => _channel.WriteObject(obj));
        }

        public dynamic ReadData(Type type)
        {
            _taskQueue.Wait();
            return _channel.ReadData(type);
        }

        public string ReadFile()
        {
            _taskQueue.Wait();
            return _channel.ReadFile();
        }

        public object ReadObject()
        {
            _taskQueue.Wait();
            return _channel.ReadObject();
        }

        public object ReadObject(Type type)
        {
            _taskQueue.Wait();
            return _channel.ReadObject(type);
        }

        public T ReadObject<T>()
        {
            return (T) ReadObject(typeof (T));
        }

        private readonly TaskQueue _taskQueue;

        public bool Works
        {
            get { return true; }
        }

        public int From
        {
            get
            {
                if (_channel == null)
                {
                    return _from;
                }

                return _channel.From;
            }
            set
            {
                if (_channel == null)
                {
                    _from = value;
                }

                else
                {
                    _channel.From = value;
                }
            }
        }

        private int _from;
    }
}
