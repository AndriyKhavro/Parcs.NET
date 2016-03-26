using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Parcs
{
    public class HostInfo
    {
        public IPAddress IpAddress
        {
            get;
            private set;
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; IsConnected = false; }
        }


        private TcpClient _tcpClient;
        private NetworkStream _stream;
        //public NetworkStream networkStream { get; private set; }
        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }

        public bool IsConnected { get; private set; }
        public int PointCount { get; set; }
        public int ProcessorCount
        {
            get
            {
                if (_processorCount == null)
                {
                    _processorCount = GetProcessorCount();
                }

                return _processorCount.Value;
            }
        }

        public double LinpackResult
        {
            get
            {
                if (_linpackResult == null)
                {
                    _linpackResult = GetLinpackResult();
                }

                return _linpackResult.Value;
            }
        }

        private int? _processorCount;
        private double? _linpackResult;

        public bool Connect()
        {
            var ipEndPoint = new IPEndPoint(IpAddress, Port);
            if (_tcpClient == null)
            {
                _tcpClient = new TcpClient();
            }

            //int attemptsNumber = 10;
            //int i = 0;
            while (!_tcpClient.Connected)
            {
                try
                {
                    _tcpClient.Connect(ipEndPoint);
                    _stream = _tcpClient.GetStream();
                    //_stream.ReadTimeout = 20;
                    //networkStream = tcpClient.GetStream();
                    Reader = new BinaryReader(_stream);
                    Writer = new BinaryWriter(_stream);
                    Console.WriteLine("Connection to host (IP: {0}) is established", IpAddress.ToString());
                    IsConnected = true;
                    return true;
                }

                catch (SocketException)
                {
                    Console.WriteLine("Cannot connect to host, IP: {0}", IpAddress.ToString());
                    IsConnected = false;
                    return false;
                }
            }

            return true;

        }

        public HostInfo(string ipAddress, int port)
            : this(IPAddress.Parse(ipAddress), port)
        {
        }


        public HostInfo(IPAddress ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
            _tcpClient = new TcpClient();
            IsConnected = false;
        }

        public void SendLocalIp()
        {
            Writer.Write((byte)Constants.ServerIP);
            Writer.Write(LocalIP.ToString());
        }

        private int GetProcessorCount()
        {
            Writer.Write((byte)Constants.ProcessorsCountRequest);
            return Reader.ReadInt32();
        }

        private double GetLinpackResult()
        {
            Writer.Write((byte)Constants.LinpackRequest);
            return Reader.ReadDouble();
        }

        public static IPAddress LocalIP //192.168 if necessary
        {
            get
            {
                string hostName = Dns.GetHostName();
                var ipHostEntry = Dns.GetHostEntry(hostName);

                // ProtocolFamily.InterNetwork.ToString
                return ipHostEntry.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).Single(x => !IPAddress.IsLoopback(x) && !x.ToString().StartsWith("192.168.56"));
                //return ipHostEntry.AddressList;
            }
        }

        public static string LocalName
        {
            get
            {
                return Dns.GetHostName();
            }
        }
    }
}
