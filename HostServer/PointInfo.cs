using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parcs;

namespace HostServer
{
    class PointInfo: IPointInfo
    {
        private bool _isFinished;
        public int Number { get; set; }
        public int ParentNumber { get; }
        public HostInfo Host { get; }
        public DateTime StartTimeUtc { get; }
        public DateTime? FinishTimeUtc { get; private set; }

        public bool IsFinished
        {
            get { return _isFinished; }
            set
            {
                _isFinished = value;
                if (value && !FinishTimeUtc.HasValue)
                {
                    FinishTimeUtc = DateTime.UtcNow;
                }
            }
        }


        public PointInfo(HostInfo host, int parentNumber)
        {
            Host = host;
            ParentNumber = parentNumber;
            StartTimeUtc = DateTime.UtcNow;
        }
    }
}
