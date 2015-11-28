using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Parcs;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DaemonPr
{
    class Daemon
    {
        TcpListener _listener;
        Object _locker = new Object();
        ConcurrentDictionary<int, int> _jobDictionary = new ConcurrentDictionary<int, int>();
        static HostInfo _server;

        public Daemon()
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
            //Thread listenerThread = new Thread(RunListener);
            //listenerThread.Start();

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
                    //Console.WriteLine("Channel was created");

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
            //  try 
            //  {
            //int filesNum = channel.ReadData(typeof(int));
            //for (int i = 0; i < filesNum; i++) 
            //{
            string fileName = channel.ReadFile();
            //string filename = (string)chan.ReadData(typeof(string));
            if (!curJob.AddFile(fileName))
            {
                throw new ParcsException("File was not sent");
                //channel.WriteData(true);
            }
            //else
            //{
            //    channel.WriteData(false);
            //}
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

        //~Daemon() { Dispose(false); }

        //public void Dispose() { Dispose(true); }

        //private void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        GC.SuppressFinalize(this);
        //    }

        //    foreach (var stream in _clientStreamList)
        //    { stream.Close(); }
        //    foreach (var client in _clientList)
        //    { client.Close(); }

        //    _listener.Stop();
        //    foreach (var file in _filesToDelete)
        //    {
        //        try
        //        {
        //            File.Delete(file);
        //        }
        //        catch { }
        //    }


        //}


        public static void Main(string[] args)
        {
            Daemon daemon = new Daemon();
        }
    }

}
