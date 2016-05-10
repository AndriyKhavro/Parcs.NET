﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Parcs;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;
using log4net;

namespace HostServer
{
    class Server
    {
        private static readonly Lazy<Server> _instance = new Lazy<Server>(() => new Server());
        private static readonly ILog Log = LogManager.GetLogger(typeof(Server));

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
                Log.Warn("File " + fileName + " was not found!");
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
                Log.Warn($"Host {host.IpAddress} is not responding...");
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
                    Log.Warn("End job: task with such number doesn't exist");
                    return;
                }

                ti.IsFinished = true;

                foreach (var p in ti.PointDictionary.ToList())
                {
                    DeletePoint(ti, p.Value.Number);
                }
            }

            Log.Info($"Job N {number} has finished");
        }

        public void CancelJob(int number)
        {
            var jobToCancel = _taskDictionary[number];
            foreach (var host in jobToCancel.PointDictionary.Values.Select(p => p.Host).Distinct())
            {
                Log.Debug($"Cancelling job N {number} on host {host.IpAddress}...");
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
