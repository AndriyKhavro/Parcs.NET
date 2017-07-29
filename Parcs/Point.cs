using System.IO;


namespace Parcs
{
    public class Point : IPoint
    {
        public HostInfo Host { get; private set; }
        public IChannel channel;
        public IJob job;
        public int Number { get; private set; }
        public int parentNumber;

        public Point(IJob job, int parentNumber)
        {
            this.job = job;
            this.parentNumber = parentNumber;

            Initialize();
        }

        protected virtual void Initialize()
        {
            HostInfo serv = Job.Server;
            string ipAddress = null;
            if (serv.IsConnected || serv.Connect())
            {
                serv.Writer.Write((byte) Constants.PointCreated);
                serv.Writer.Write(job.Number);
                serv.Writer.Write(parentNumber);

                ipAddress = ReadIPFromStream(serv.Reader);
                Host = new HostInfo(ipAddress, (int)Ports.DaemonPort);
                if (!Host.Connect())
                {
                    throw new ParcsException("Cannot connect to host");
                }
            }
        }

        protected virtual IChannel CreateNewChannel()
        {
            return new Channel(Host.Reader, Host.Writer, Host.IsConnected) { From = Number };
        }


        public virtual IChannel CreateChannel()
        {
            channel = CreateNewChannel();
            InitializeChannel();

            return channel;
        }

        public virtual void ExecuteClass(string className)
        {
            channel.WriteData((byte)Constants.ExecuteClass);
            channel.WriteData(className);
        }

        private void InitializeChannel()
        {
            channel.WriteData((byte)Constants.RecieveTask);
            channel.WriteObject(job);
            WriteNumberToChannel();
            string fileName = job.FileName;
            if (fileName != null)
            {
                channel.WriteData((byte)Constants.LoadFile);
                channel.WriteFile(fileName);
                //bool fileSent = channel.ReadData(typeof(bool));
                //if (!fileSent) return null;
            }
        }

        protected virtual void WriteNumberToChannel()
        {
            Host.Writer.Write(Number); //to wrap it with continuewith
        }

        private string ReadIPFromStream(BinaryReader binaryReader)
        {
            Number = binaryReader.ReadInt32();
            return binaryReader.ReadString();
        }

    }
}