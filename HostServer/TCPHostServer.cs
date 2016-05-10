using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Parcs;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using HostServer.WebApi;
using log4net;
using Microsoft.Owin.Hosting;

namespace HostServer
{
    public class TCPHostServer : ServiceBase
    {
        private static Server _hostServer;
        private TcpListener _listener;
        private const int WEB_API_PORT = 1236;
        private IDisposable _webApi;

        private static readonly ILog Log = LogManager.GetLogger(typeof(TCPHostServer));

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(Run);
        }

        protected override void OnStop()
        {
            _listener?.Stop();
            _webApi?.Dispose();
        }

        public void Run()
        {
            _webApi = WebApp.Start<Startup>($"http://localhost:{WEB_API_PORT}");

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

            int port = (int)Ports.ServerPort;
            _listener = new TcpListener(ip, port);
            _hostServer = Server.Instance; //make it a singleton and use in self-hosted WebAPI
            Log.Info($"Accepting connections from clients, IP: {ip}, port: {port}");
            RunListener();
        }

        private void RunListener()
        {
            _listener.Start();

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var clientStream = client.GetStream();

                var clientTask = new Task(() => RunClient(clientStream));
                clientTask.Start();
                clientTask.ContinueWith(_ =>
                {
                    client.Close();
                    clientStream.Dispose();
                });
            }
        }

        private void RunClient(NetworkStream clientStream)
        {
            int jobNumber = 0;

            using (var writer = new BinaryWriter(clientStream))
            {
                using (var reader = new BinaryReader(clientStream))
                {
                    while (true)
                    {
                        try
                        {
                            byte signal = reader.ReadByte();

                            switch (signal)
                            {
                                case ((byte)Constants.PointCreated):
                                    jobNumber = reader.ReadInt32();
                                    int parentNumber = reader.ReadInt32();
                                    IPointInfo point = _hostServer.CreatePoint(jobNumber, parentNumber);
                                    if (point == null)
                                    {
                                        return;
                                    }
                                    writer.Write(point.Number);
                                    writer.Write(point.Host.IpAddress.ToString()); //provide client with point and daemon IP adress
                                    break;
                                case ((byte)Constants.PointDeleted):
                                    {
                                        jobNumber = reader.ReadInt32();
                                        var pointNumber = reader.ReadInt32();
                                        _hostServer.DeletePoint(jobNumber, pointNumber);
                                    }
                                    break;
                                case ((byte)Constants.BeginJob):
                                    {
                                        jobNumber = _hostServer.BeginJob();
                                        writer.Write(jobNumber);
                                    }
                                    break;
                                case ((byte)Constants.FinishJob):
                                    jobNumber = reader.ReadInt32();
                                    _hostServer.EndJob(jobNumber);
                                    return;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (jobNumber != 0)
                            {
                                _hostServer.EndJob(jobNumber);
                            }

                            Console.WriteLine(ex.Message);
                            _hostServer.UpdateHostList();
                            return;
                        }
                    }
                }
            }
        }
        
        private static void ListenToKeyboard()
        {
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey();
                        if (keyInfo.Key == ConsoleKey.U && ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0))
                        {
                            UpdateHostList();
                        }
                    }
                });
        }

        private static void UpdateHostList()
        {
            Log.Debug("Updating host list...");
            _hostServer.UpdateHostList();
        }
        
        static void Main(string[] args)
        {
            using (var service = new TCPHostServer())
            {
                if (!Environment.UserInteractive)
                {
                    // running as service
                    ServiceBase.Run(service);
                }

                else
                {
                    // running as console app
                    ListenToKeyboard();
                    service.Run();
                }
            }
        }
    }
}
