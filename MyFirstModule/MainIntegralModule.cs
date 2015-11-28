﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Parcs;

namespace FirstModule
{
    class MainIntegralModule: IModule
    {
        public static void Main(string[] args)
        {
            
            var job = new Job();
            if (!job.AddFile(Assembly.GetExecutingAssembly().Location/*"MyFirstModule.exe"*/))
            {
                Console.WriteLine("File doesn't exist");
                return;
            }

            (new MainIntegralModule()).Run(new ModuleInfo(job, null));
            //fucker.FinishFucker();
            Console.ReadKey();
        }

        public void Run(ModuleInfo info)
        {
            double a = 0;
            double b = Math.PI/2;
            double h = 0.00000001;
            //var func = new Func<double, double>(x => Math.Cos(x * x * x));
            const int pointsNum = 2;
            var points = new IPoint[pointsNum];
            var channels = new IChannel[pointsNum];
            for (int i = 0; i < pointsNum; ++i)
            {
                points[i] = info.CreatePoint();
                channels[i] = points[i].CreateChannel();
                points[i].ExecuteClass("FirstModule.IntegralModule");
            }

            double y = a;
            for (int i = 0; i < pointsNum; ++i)
            {
                channels[i].WriteData(y);
                channels[i].WriteData(y + (b - a) / pointsNum);
                channels[i].WriteData(h);
                //channels[i].WriteObject(func);
                y += (b - a) / pointsNum;
            }
            DateTime time = DateTime.Now;            
            Console.WriteLine("Waiting for result...");

            double res = 0;
            for (int i = pointsNum - 1; i >= 0; --i)
            {
                res += channels[i].ReadData(typeof(double));
            }

            Console.WriteLine("Result found: res = {0}, time = {1}", res, Math.Round((DateTime.Now - time).TotalSeconds,3));
            
        }
    }
}
