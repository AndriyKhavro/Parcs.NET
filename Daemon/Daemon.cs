using System;
using System.Net;
using System.Net.Sockets;
using Parcs;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using log4net;

namespace DaemonPr
{
    public class Daemon : ServiceBase
    {
        TcpListener _listener;
        private readonly object _locker = new object();
        private readonly ConcurrentDictionary<int, List<int>> _jobPointNumberDictionary = new ConcurrentDictionary<int, List<int>>(); //stores numbers of points
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _cancellationDictionary = new ConcurrentDictionary<int, CancellationTokenSource>();
        static HostInfo _server;
        private readonly ILog _log = LogManager.GetLogger(typeof(Daemon));

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(() => Run(ExtractIpFromArgs(args), false));
        }

        protected override void OnStop()
        {
            _listener.Stop();
        }

        public void Run(string localIp, bool allowUserInput)
        {
            IPAddress ip = string.IsNullOrEmpty(localIp) ? HostInfo.GetLocalIpAddress(allowUserInput) : IPAddress.Parse(localIp);

            int port = (int)Ports.DaemonPort;
            _listener = new TcpListener(ip, port);
            _log.InfoFormat("Accepting connections from clients, IP: {0}, port: {1}", ip, port);
            RunListener();
        }
        
        private void RunListener()
        {
            _listener.Start();
            while (true)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    var clientStream = client.GetStream();

                    var clientTask = new Task(() => RunClient(clientStream));
                    clientTask.Start();

                }
                catch (SocketException)
                {
                    _log.Error("Unknown error, stopping the listener...");
                    _listener.Stop();
                    return;
                }
            }
        }

        private void RunClient(NetworkStream clientStream)
        {
            Channel channel;
            byte signal;
            Job currentJob = null;
            int pointNumber = 0;
            using (BinaryReader reader = new BinaryReader(clientStream))
            {
                using (BinaryWriter writer = new BinaryWriter(clientStream))
                {
                    channel = new Channel(reader, writer);

                    while (true)
                    {
                        try
                        {
                            signal = channel.ReadByte();

                            switch (signal)
                            {
                                case ((byte)Constants.RecieveTask):

                                    currentJob = (Job)channel.ReadObject();
                                    pointNumber = channel.ReadInt();
                                    _jobPointNumberDictionary.AddOrUpdate(currentJob.Number, new List<int> { pointNumber },
                                        (key, oldvalue) =>
                                        {
                                            oldvalue.Add(pointNumber);
                                            return oldvalue;
                                        });
                                    _cancellationDictionary.AddOrUpdate(currentJob.Number, new CancellationTokenSource(),
                                        (key, oldValue) => oldValue);
                                    continue;

                                case ((byte)Constants.ExecuteClass):

                                    if (currentJob != null)
                                    {
                                        var cancellationTokenSource = _cancellationDictionary[currentJob.Number];
                                        if (!_cancellationDictionary[currentJob.Number].IsCancellationRequested)
                                        {
                                            var executor = new ModuleExecutor(channel, currentJob, pointNumber);

                                            try
                                            {
                                                executor.Run(cancellationTokenSource.Token);
                                            }
                                            catch (OperationCanceledException)
                                            {
                                                _log.Info($"Point N {currentJob.Number}:{pointNumber} was cancelled");
                                            }

                                            DeletePoint(currentJob.Number, pointNumber);

                                            if (_jobPointNumberDictionary[currentJob.Number].Count == 0)
                                            {
                                                lock (_locker)
                                                {
                                                    if (File.Exists(currentJob.FileName))
                                                    {
                                                        File.Delete(currentJob.FileName);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    return;

                                case ((byte)Constants.LoadFile):
                                    {
                                        LoadFile(channel, currentJob);
                                        continue;
                                    }

                                case ((byte)Constants.ProcessorsCountRequest):
                                    {
                                        channel.WriteData(Environment.ProcessorCount);
                                        continue;
                                    }

                                case ((byte)Constants.LinpackRequest):
                                    {
                                        var linpack = new Linpack();
                                        linpack.RunBenchmark();
                                        channel.WriteData(linpack.MFlops);
                                        continue;
                                    }

                                case ((byte)Constants.ServerIP):
                                    {
                                        string ip = channel.ReadString();
                                        if (_server == null || _server.IpAddress.ToString() != ip)
                                        {
                                            _server = new HostInfo(ip, (int)Ports.ServerPort);
                                        }

                                        continue;
                                    }
                                case ((byte)Constants.CancelJob):
                                {
                                    int jobNumber = channel.ReadInt();
                                    CancellationTokenSource tokenSource;
                                    if (_cancellationDictionary.TryGetValue(jobNumber, out tokenSource))
                                    {
                                        tokenSource.Cancel();
                                        _log.Info($"Cancelling job N {jobNumber}");
                                    }
                                    else
                                    {
                                        _log.Info($"Job N {jobNumber} does not exist or does not have cancellation token");
                                    }
                                    continue;
                                }

                                default:
                                    _log.Error("Unknown signal received, stopping the application...");
                                    return;
                            }
                        }

                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            return;
                        }
                    }
                }
            }
        }

        private void LoadFile(IChannel channel, IJob curJob)
        {
            string fileName = channel.ReadFile();

            if (!curJob.AddFile(fileName))
            {
                throw new ParcsException("File was not sent");
            }
        }

        private void DeletePoint(int jobNum, int pointNum)
        {
            _jobPointNumberDictionary.AddOrUpdate(jobNum, new List<int>(), (key, oldvalue) =>
            {
                oldvalue.Remove(pointNum);
                return oldvalue;
            });

            if (_server != null)
            {
                if (_server.IsConnected || _server.Connect())
                {
                    _server.Writer.Write((byte)Constants.PointDeleted);
                    _server.Writer.Write(jobNum);
                    _server.Writer.Write(pointNum);
                }
            }
        }

        public static void Main(string[] args)
        {
            using (var daemon = new Daemon())
            {
                if (!Environment.UserInteractive)
                {
                    // running as service
                    ServiceBase.Run(daemon);
                }

                else
                {
                    // running as console app
                    daemon.Run(ExtractIpFromArgs(args), true);
                }
            }
        }

        private static string ExtractIpFromArgs(string[] args)
        {
            return args.Length > 0 ? args[0] : "";
        }
    }

}
