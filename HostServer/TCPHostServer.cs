using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Parcs;
using System.IO;
using System.Threading.Tasks;

namespace HostServer
{
    public class TCPHostServer
    {
        private static Server _hostServer;
        private readonly TcpListener _listener;

        public TCPHostServer()
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

            int port = (int)Ports.ServerPort;
            _listener = new TcpListener(ip, port);
            _hostServer = new Server(); //make it a singleton and use in self-hosted WebAPI
            Console.WriteLine("Accepting connections from clients, IP: {0}, port: {1}", ip.ToString(), port);
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

        static void Main(string[] args)
        {
            ListenToKeyboard();
            var tcpHostServer = new TCPHostServer();
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
            Console.WriteLine("Updating host list...");
            _hostServer.UpdateHostList();
        }
    }
}
