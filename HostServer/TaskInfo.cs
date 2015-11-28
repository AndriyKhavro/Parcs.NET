using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parcs;

namespace HostServer
{
    public class TaskInfo 
    {
        public int Number { get; private set; }
        private int lastPointNumber;
        public IDictionary<int, IPointInfo> PointDictionary { get; private set; }
        public bool NeedsPoint { get; set; }
        public int Priority { get; set; }
        
        public TaskInfo(IList<HostInfo> currentHostList, int number)
        {
            PointDictionary = new Dictionary<int, IPointInfo>();
            Number = number;
        }

        public int AddPoint(IPointInfo pointInfo)
        {
            pointInfo.Number = ++lastPointNumber;
            PointDictionary.Add(pointInfo.Number, pointInfo);
            ++pointInfo.Host.PointsNumber;
            NeedsPoint = false;
            return lastPointNumber;
        }

        public void RemovePoint(int pointNum)
        {
            IPointInfo pi;
            if (PointDictionary.TryGetValue(pointNum, out pi))
            {
                pi.Host.PointsNumber--;
                //Console.WriteLine("remove point: host.pointsnumber = {0}", PointDictionary[pointNum].Host.PointsNumber);
                PointDictionary.Remove(pointNum);
            }
        }

       
        
    }
}
