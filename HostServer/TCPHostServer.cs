using System;
using System.Net;
using System.Net.Sockets;
using Parcs;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using HostServer.WebApi;
using Microsoft.Owin.Hosting;
using Serilog;

namespace HostServer
{
    public class TCPHostServer : ServiceBase
    {
        private static Server _hostServer;
        private TcpListener _listener;
        private const int WEB_API_PORT = 1236;
        private IDisposable _webApi;

        private static readonly ILogger _log = Log.Logger.ForContext<TCPHostServer>();

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(() => Run(ExtractIpFromArgs(args), false));
        }

        protected override void OnStop()
        {
            _listener?.Stop();
            _webApi?.Dispose();
        }

        public void Run(string localIp, bool allowUserInput)
        {
            _webApi = WebApp.Start<Startup>($"http://*:{WEB_API_PORT}");

            IPAddress ip = string.IsNullOrEmpty(localIp) ? HostInfo.GetLocalIpAddress(allowUserInput) : IPAddress.Parse(localIp);

            int port = (int)Ports.ServerPort;
            _listener = new TcpListener(ip, port);
            _hostServer = Server.Instance; //make it a singleton and use in self-hosted WebAPI
            _log.Information($"Accepting connections from clients, IP: {ip}, port: {port}");
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
                                    continue;
                                case ((byte)Constants.PointDeleted):
                                    {
                                        jobNumber = reader.ReadInt32();
                                        var pointNumber = reader.ReadInt32();
                                        _hostServer.DeletePoint(jobNumber, pointNumber);
                                    }
                                    continue;
                                case ((byte)Constants.BeginJob):
                                    int priority = reader.ReadInt32();
                                    string username = reader.ReadString();
                                    jobNumber = _hostServer.BeginJob(priority, username);
                                    writer.Write(jobNumber);
                                    continue;
                                case ((byte)Constants.FinishJob):
                                    jobNumber = reader.ReadInt32();
                                    _hostServer.EndJob(jobNumber);
                                    return;
                                case (byte)Constants.IpAddress:
                                    string ip = reader.ReadString();
                                    _log.Information($"Adding new daemon. IP: {ip}");
                                    _hostServer.AddDaemon(ip);
                                    continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (jobNumber != 0)
                            {
                                _hostServer.EndJob(jobNumber);
                            }

                            _log.Error(ex, "An error occurred during job execution");
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
            _log.Debug("Updating host list...");
            _hostServer.UpdateHostList();
        }
        
        static void Main(string[] args)
        {
            LogConfigurator.Configure();

            using (var service = new TCPHostServer())
            {
                HostInfo.ExternalLocalIP = Environment.GetEnvironmentVariable(EnvironmentVariables.ExternalLocalIp);

                if (!Environment.UserInteractive && !args.Contains("--docker"))
                {
                    // running as service
                    ServiceBase.Run(service);
                }

                else
                {
                    // running as console app
                    ListenToKeyboard();
                    string envVariableValue = Environment.GetEnvironmentVariable(EnvironmentVariables.LocalIp);
                    string localIp = !string.IsNullOrWhiteSpace(envVariableValue)
                        ? envVariableValue
                        : ExtractIpFromArgs(args);
                    service.Run(localIp, true);
                }
            }
        }

        private static string ExtractIpFromArgs(string[] args)
        {
            return args.FirstOrDefault(a => !a.Contains("docker")) ?? string.Empty;
        }
    }
}
