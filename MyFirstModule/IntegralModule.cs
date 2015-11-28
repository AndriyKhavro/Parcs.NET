using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parcs;

namespace FirstModule
{
   // private static Func<double, double> func = x => Math.c
    public class IntegralModule: IModule
    {
        private static double Integral(double a, double b, double h, Func<double, double> func)
        {
            int N = (int)((b - a) / h);
            double res = 0;
            for (int j = 1; j <= N; ++j)
            {
                double x = a + (2 * j - 1) * h / 2;
                res += func(x);
            }

            return res * h;
        }

        public void Run(ModuleInfo info)
        {
            double a = info.Parent.ReadData(typeof(double));
            double b = info.Parent.ReadData(typeof(double));
            double h = info.Parent.ReadData(typeof(double));
            var func = new Func<double, double>(x => Math.Cos(x));

            double res = Integral(a, b, h, func);
            info.Parent.WriteData(res);
        }
    }
}
