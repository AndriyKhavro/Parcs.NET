using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Parcs;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;
using Serilog;

namespace HostServer
{
    internal class Server
    {
        private static readonly Lazy<Server> _instance = new Lazy<Server>(() => new Server());
        private static readonly ILogger _log = Log.Logger.ForContext<Server>();

        public static Server Instance => _instance.Value;

        private Server()
        {
            ReadHostsFromFile();
        }

        public IList<HostInfo> HostList { get; private set; } = new List<HostInfo>();

        readonly ConcurrentDictionary<int, JobInfo> _taskDictionary = new ConcurrentDictionary<int, JobInfo>();

        private int _taskNumber;
        private const string fileName = "hosts.txt";
        private readonly object _syncRoot = new object();
        
        public void ReadHostsFromFile()
        {
            var hostsFileName = File.Exists(fileName)
                ? fileName
                : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
            try
            {
                using (var reader = new StreamReader(hostsFileName))
                {

                    while (!reader.EndOfStream)
                    {
                        AddDaemon(reader.ReadLine());
                    }
                }
            }

            catch (FileNotFoundException)            
            {
                _log.Warning("File " + fileName + " was not found!");
            }


            if (HostList.Count == 0)
            {
                _log.Warning("Host list is empty!");
            }
        }

        public void AddDaemon(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return;
            if (HostList.All(h => h.IpAddress.ToString() != ip))
            {
                HostList.Add(new HostInfo(ip, (int) Ports.DaemonPort));
            }
        }
        
        public void UpdateHostList()
        {
            ReadHostsFromFile();
            CheckHostNames();
        }

        public IPointInfo CreatePoint(int jobNumber, int parentNumber)
        {
            var jobInfo = _taskDictionary[jobNumber];            
            HostInfo target = null;
            bool targetChosen = false;
            while (!targetChosen)
            {
                if (jobInfo.IsCancelled)
                {
                    return null;
                }
                jobInfo.NeedsPoint = true;
                if (jobInfo == GetTheMostUrgentTask())
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
                jobInfo.AddPoint(p);
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
                _log.Warning($"Host {host.IpAddress} is not responding...");
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
            ti.RemovePoint(pointNum, ti.IsCancelled);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>task number</returns>
        public int BeginJob(int priority, string username)
        {
            _log.Information($"Starting a new job with priority {priority}. Username: {username}");
            var t = new JobInfo(++_taskNumber) {Priority = priority, Username = username};
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
                    _log.Warning("End job: task with such number doesn't exist");
                    return;
                }

                ti.IsFinished = true;

                foreach (var p in ti.PointDictionary.ToList())
                {
                    DeletePoint(ti, p.Value.Number);
                }
            }

            _log.Information($"Job N {number} has finished");
        }

        public void CancelJob(int number)
        {
            var jobToCancel = _taskDictionary[number];
            if (jobToCancel.IsFinished)
            {
                return;
            }
            foreach (var host in jobToCancel.PointDictionary.Values.Select(p => p.Host).Distinct())
            {
                _log.Information($"Cancelling job N {number} on host {host.IpAddress}...");
                host.Writer.Write((byte)Constants.CancelJob);
                host.Writer.Write(number);
            }
            lock (_syncRoot)
            {
                jobToCancel.NeedsPoint = false;
                jobToCancel.IsCancelled = true;
            }
        }
    }
}
