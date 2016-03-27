using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Parcs;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;

namespace HostServer
{
    class Server
    {
        private static readonly Lazy<Server> _instance = new Lazy<Server>(() => new Server());

        public static Server Instance
        {
            get { return _instance.Value; }
        }

        private Server()
        {
            ReadHostsFromFile();
        }

        public IList<HostInfo> HostList { get; private set; }

        readonly ConcurrentDictionary<int, JobInfo> _taskDictionary = new ConcurrentDictionary<int, JobInfo>();

        private int _taskNumber;
        private const string fileName = "hosts.txt";
        private readonly object _syncRoot = new object();
        
        public void ReadHostsFromFile()
        {
            HostList = new List<HostInfo>();
            StreamReader reader = null;
            var hostsFileName = File.Exists(fileName)
                ? fileName
                : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
            try
            {
                using (reader = new StreamReader(hostsFileName))
                {

                    while (!reader.EndOfStream)
                    {
                        string ip = reader.ReadLine();
                        if (string.IsNullOrEmpty(ip)) break;
                        HostList.Add(new HostInfo(ip, (int)Ports.DaemonPort));
                    }
                }
            }

            catch (FileNotFoundException)            
            {
                Console.WriteLine("File " + fileName + " was not found!");
            }


            if (HostList.Count == 0)
            {
                throw new ParcsException("Host list is empty!");
            }
        }


        public void UpdateHostList()
        {
            ReadHostsFromFile();
            CheckHostNames();
        }

        public IPointInfo CreatePoint(int jobNumber, int parentNumber)
        {
            var taskInfo = _taskDictionary[jobNumber];
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

            PointInfo p = new PointInfo(target, parentNumber);
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
            foreach (var host in HostList.OrderByDescending(host => host.LinpackResult))
            {
                if (host.PointCount < host.ProcessorCount)
                {
                    return host;
                }
            }

            return null;
        }

        public IEnumerable<JobInfo> GetCurrentJobs()
        {
            return _taskDictionary.Values;
        }

        private void CheckHostNames()
        {
            var listToRemove = HostList.Where(host => !host.IsConnected && !host.Connect()).ToList();
            foreach (var host in listToRemove)
            {
                Console.WriteLine("Host {0} is not responding...", host.IpAddress.ToString());
            }

            HostList = HostList.Except(listToRemove).ToList();

            foreach (var host in HostList)
            {
                host.SendLocalIp();
            }
        }

        private JobInfo GetTheMostUrgentTask()
        {
            return _taskDictionary.Values.Where(x => x.NeedsPoint).OrderByDescending(x => x.Priority).ThenBy(x => x.Number).FirstOrDefault();
        }

        public void DeletePoint(int jobsNum, int pointNum)
        {
            lock (_syncRoot)
            {
                JobInfo ti;
                if (_taskDictionary.TryGetValue(jobsNum, out ti))
                {
                    DeletePoint(ti, pointNum);
                }
            }
        }

        private void DeletePoint(JobInfo ti, int pointNum)
        {
            ti.RemovePoint(pointNum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>task number</returns>
        public int BeginJob()
        {
            var t = new JobInfo(++_taskNumber);
            _taskDictionary.AddOrUpdate(t.Number, t, (key, value) => value);
            CheckHostNames();
            return _taskNumber;
        }

        public void EndJob(int number)
        {
            lock (_syncRoot)
            {
                JobInfo ti;
                if (!_taskDictionary.TryGetValue(number, out ti))
                {
                    //Console.WriteLine("End job: task with such number doesn't exist");
                    return;
                }

                ti.IsFinished = true;

                foreach (var p in ti.PointDictionary.ToList())
                {
                    DeletePoint(ti, p.Value.Number);
                }
            }

            Console.WriteLine("Job N {0} has finished", number);
        }

    }
}
