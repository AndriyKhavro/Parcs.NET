using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parcs
{
    [Serializable]
    public class Job : IJob
    {
        private static HostInfo _server;
        public static HostInfo Server
        {
            get
            {
                if (_server == null)
                {
                    _server = ReadServerName();
                }

                return _server;
            }
        }

        private const string serverName = "server.txt";

        public string FileName
        {
            get;
            private set;
        }

        public int Number { get; private set; }

        public bool IsFinished { get; private set; }
        //private static IDictionary<int, IJob> _uniqueJobs = new Dictionary<int, IJob>();

        public Job()
        {
            if (!Server.IsConnected && !Server.Connect()) throw new Exception("Cannot connect to server...");
            var writer = Server.Writer;
            var reader = Server.Reader;

            writer.Write((byte)Constants.BeginJob);
            Number = Server.Reader.ReadInt32();
        }

        //public static void AddUniqueJob(IJob job) 
        //{
        //    if (!_uniqueJobs.Keys.Contains(job.Number))
        //        _uniqueJobs.Add(job.Number, job);
        //}

        private static HostInfo ReadServerName()
        {
            string serverIp;
            try
            {

                using (StreamReader reader = new StreamReader(serverName))
                {
                    serverIp = reader.ReadLine();
                    return new HostInfo(serverIp, (int)Ports.ServerPort);
                }
            }

            catch (FileNotFoundException)
            {
                Console.WriteLine("File " + serverName + " was not found");
                return null;
            }

            catch (IOException)
            {
                Console.WriteLine("Can not read from file " + serverName);
                return null;
            }
        }

        public void FinishJob()
        {
            if (!IsFinished)
            {
                Server.Writer.Write((byte)Constants.FinishJob);
                Server.Writer.Write(Number);
                IsFinished = true;
            }
        }

        public IPoint CreatePoint(int parentNumber)
        {
            return new ConcurrentPoint(this, parentNumber);
        }

        public bool AddFile(string fileName)
        {
            if (FileName == fileName) { return true; }
            if (File.Exists(fileName))
            {
                FileName = fileName;
                return true;
            }

            return false;
        }
    }
}
