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
        public int Number { get; set; }
        public int ParentNumber { get; private set; }
        public HostInfo Host { get; private set; }
        public bool IsFinished { get; set; }

        public PointInfo(HostInfo host, int parentNumber)
        {
            Host = host;
            ParentNumber = parentNumber;
        }
    }
}
