using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Parcs;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;

namespace HostServer
{
    class Server
    {
        IList<HostInfo> _hostList;

        public IList<HostInfo> HostList
        {
            get { return _hostList; }
            set { _hostList = value; }
        }

        //IDictionary<int, TaskInfo> _taskDictionary;
        ConcurrentDictionary<int, TaskInfo> _taskDictionary = new ConcurrentDictionary<int, TaskInfo>();

        int taskNumber;
        const string fileName = "hosts.txt";
        object _syncRoot = new object();

        public Server()
        {
            ReadHostsFromFile();
        }

        public Server(IList<HostInfo> hostList)
        {
            _hostList = hostList;
            //CheckHostNames();
        }

        public void ReadHostsFromFile()
        {
            _hostList = new List<HostInfo>();
            StreamReader reader = null;
            try
            {
                using (reader = new StreamReader(fileName))
                {

                    while (!reader.EndOfStream)
                    {
                        string ip = reader.ReadLine();
                        if (string.IsNullOrEmpty(ip)) break;
                        _hostList.Add(new HostInfo(ip, (int)Ports.DaemonPort));
                    }
                }
            }

            catch (FileNotFoundException)
            {
                Console.WriteLine("File " + fileName + " was not found!");
            }


            if (_hostList.Count == 0)
            {
                throw new ParcsException("Host list is empty!");
            }
        }

        public void CheckHostNames()//when to invoke?
        {
            var listToRemove = _hostList.Where(host => !host.IsConnected && !host.Connect()).ToList();
            foreach (var host in listToRemove)
            {
                Console.WriteLine("Host {0} is not responding...", host.IpAddress.ToString());
            }

            _hostList = _hostList.Except(listToRemove).ToList();

            foreach (var host in _hostList)
            {
                host.WriteIp();
            }
        }

        public void UpdateHostList()
        {
            ReadHostsFromFile();
            CheckHostNames();
        }

        public IPointInfo CreatePoint(int jobsNum, int parentNum)
        {
            var taskInfo = _taskDictionary[jobsNum];
            HostInfo target = null;
            bool targetChosen = false;
            while (!targetChosen)
            {
                taskInfo.NeedsPoint = true;
                if (taskInfo == GetTheMostUrgentTask())
                {
                    lock (_syncRoot)
                    {
                        target = GetTargetHost(); //may return null             
                    }
                }

                if (target == null) 
                {
                    Thread.Sleep(100);
                }

                else
                {
                    targetChosen = true;
                }
            }

            PointInfo p = new PointInfo(target, parentNum);
            lock (_syncRoot)
            {
                taskInfo.AddPoint(p);
            }

            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Target host. Returns null in case there is no free host.</returns>
        public HostInfo GetTargetHost()
        {
            foreach (var host in _hostList.OrderByDescending(host => host.LinpackResult))
            {
                if (host.PointsNumber < host.ProcessorCount)
                {
                    return host;
                }
            }

            return null;
        }

        private TaskInfo GetTheMostUrgentTask()
        {
            return _taskDictionary.Values.Where(x => x.NeedsPoint).OrderByDescending(x => x.Priority).ThenBy(x => x.Number).FirstOrDefault();
        }

        public void DeletePoint(int jobsNum, int pointNum)
        {
            TaskInfo ti;
            lock (_syncRoot)
            {
                if (_taskDictionary.TryGetValue(jobsNum, out ti))
                {
                    DeletePoint(ti, pointNum);
                }
            }
        }

        private void DeletePoint(TaskInfo ti, int pointNum)
        {
            ti.RemovePoint(pointNum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>task number</returns>
        public int BeginJob()
        {
            var t = new TaskInfo(HostList, ++taskNumber);
            _taskDictionary.AddOrUpdate(t.Number, t, (key, value) => value);
            CheckHostNames();
            return taskNumber;
        }

        public void EndJob(int number)
        {
            TaskInfo ti;
            lock (_syncRoot)
            {
                if (!_taskDictionary.TryRemove(number, out ti))
                {
                    //Console.WriteLine("End job: task with such number doesn't exist");
                    return;
                }

                foreach (var p in ti.PointDictionary.ToList())
                {
                    DeletePoint(ti, p.Value.Number);
                }
            }

            Console.WriteLine("Job N {0} has finished", number);
        }

    }
}
