using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Parcs;
using System.IO;
using System.Threading.Tasks;

namespace HostServer
{
    class TCPHostServer
    {
        static Server hostServer;
        TcpListener _listener;
        IList<NetworkStream> _clientStreamList;
        IList<TcpClient> _clientList;

        public TCPHostServer()
        {
            _clientList = new List<TcpClient>();
            _clientStreamList = new List<NetworkStream>();
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
            hostServer = new Server();
            Console.WriteLine("Accepting connections from clients, IP: {0}, port: {1}", ip.ToString(), port);
            RunListener();

        }
        private void RunListener()
        {
            _listener.Start();

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                _clientList.Add(client);
                var clientStream = client.GetStream();
                _clientStreamList.Add(clientStream);

                var clientTask = new Task(() => RunClient(clientStream));
                clientTask.Start();
            }
        }

        private void RunClient(NetworkStream clientStream)
        {
            int jobNumber = 0;
            int pointNumber;

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
                                    IPointInfo point = hostServer.CreatePoint(jobNumber, parentNumber);
                                    if (point != null)
                                    {
                                        writer.Write(point.Number);
                                        writer.Write(point.Host.IpAddress.ToString());
                                    }

                                    break;
                                case ((byte)Constants.PointDeleted):
                                    {
                                        jobNumber = reader.ReadInt32();
                                        pointNumber = reader.ReadInt32();
                                        hostServer.DeletePoint(jobNumber, pointNumber);
                                    }
                                    break;
                                case ((byte)Constants.BeginJob):
                                    {
                                        jobNumber = hostServer.BeginJob();
                                        writer.Write(jobNumber);
                                    }
                                    break;
                                case ((byte)Constants.FinishJob):
                                    jobNumber = reader.ReadInt32();
                                    hostServer.EndJob(jobNumber);
                                    return;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (jobNumber != 0)
                            {
                                hostServer.EndJob(jobNumber);
                            }

                            Console.WriteLine(ex.Message);
                            hostServer.UpdateHostList();
                            return;
                        }
                    }
                }
            }
        }

        ~TCPHostServer()
        {
            Disconnect();
        }

        private void Disconnect()
        {
            _listener.Stop();
            foreach (var clientStream in _clientStreamList)
            {
                clientStream.Close();
            }
            foreach (var client in _clientList)
            { client.Close(); }
            //clientStream.Close();
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
            hostServer.UpdateHostList();
        }
    }
}
