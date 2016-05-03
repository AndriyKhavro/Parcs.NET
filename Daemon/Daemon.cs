using System;
using System.Net;
using System.Net.Sockets;
using Parcs;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.ServiceProcess;

namespace DaemonPr
{
    public class Daemon: ServiceBase
    {
        TcpListener _listener;
        private readonly object _locker = new object();
        private readonly ConcurrentDictionary<int, int> _jobDictionary = new ConcurrentDictionary<int, int>();
        static HostInfo _server;

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(Run);
        }

        protected override void OnStop()
        {
            _listener.Stop();
        }

        public void Run()
        {
            IPAddress ip;
            try
            {
                ip = HostInfo.LocalIP;
            }

            catch (Exception)
            {
                Console.WriteLine("Cannot get local IP. Please, enter your IP:");
                string ipStr = Console.ReadLine();
                ip = IPAddress.Parse(ipStr);
            }

            int port = (int)Ports.DaemonPort;
            _listener = new TcpListener(ip, port);
            Console.WriteLine("Accepting connections from clients, IP: {0}, port: {1}", ip.ToString(), port);
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
                    Console.WriteLine("Unknown error, stopping the listener...");
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
                    channel = new Channel(reader, writer, true);

                    while (true)
                    {
                        try
                        {
                            signal = channel.ReadData(typeof(byte));

                            switch (signal)
                            {
                                case ((byte)Constants.RecieveTask):

                                    currentJob = (Job)channel.ReadObject();
                                    //Job.AddUniqueJob(currentJob);
                                    _jobDictionary.AddOrUpdate(currentJob.Number, 1, (key, oldvalue) => oldvalue + 1);
                                    pointNumber = channel.ReadData(typeof(int));
                                    continue;

                                case ((byte)Constants.ExecuteClass):

                                    if (currentJob != null)
                                    {

                                        var executor = new ModuleExecutor(channel, currentJob, pointNumber);
                                        executor.Run();
                                        
                                        DeletePoint(currentJob.Number, pointNumber);
 
                                        if (_jobDictionary[currentJob.Number] == 0)
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
                                        string ip = channel.ReadData(typeof(string));
                                        if (_server == null || _server.IpAddress.ToString() != ip)
                                        {
                                            _server = new HostInfo(ip, (int)Ports.ServerPort);
                                        }

                                        continue;
                                    }

                                default:
                                    Console.WriteLine("Unknown signal received, stopping the application...");
                                    return;
                            }
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
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
            _jobDictionary.AddOrUpdate(jobNum, 0, (key, oldvalue) => oldvalue - 1);

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
                    daemon.Run();
                }
            }

        }
    }

}
