﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostServer
{
    public class TaskInfo 
    {
        public int Number { get; private set; }
        private int _lastPointNumber;
        public IDictionary<int, IPointInfo> PointDictionary { get; private set; }
        public bool NeedsPoint { get; set; }
        public int Priority { get; set; }
        
        public TaskInfo(int number)
        {
            PointDictionary = new Dictionary<int, IPointInfo>();
            Number = number;
        }

        public int AddPoint(IPointInfo pointInfo)
        {
            pointInfo.Number = ++_lastPointNumber;
            PointDictionary.Add(pointInfo.Number, pointInfo);
            ++pointInfo.Host.PointCount;
            NeedsPoint = false;
            return _lastPointNumber;
        }

        public void RemovePoint(int pointNum)
        {
            IPointInfo pi;
            if (PointDictionary.TryGetValue(pointNum, out pi))
            {
                pi.Host.PointCount--;
                PointDictionary.Remove(pointNum);
            }
        }

       
        
    }
}
