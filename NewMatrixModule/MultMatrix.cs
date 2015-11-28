using Parcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMatrixModule
{
    public class MultMatrix : IModule
    {
        public void Run(ModuleInfo info)
        {
            MyMatrix m = (MyMatrix)info.Parent.ReadObject(typeof(MyMatrix));
            MyMatrix m1 = (MyMatrix)info.Parent.ReadObject(typeof(MyMatrix));
            info.Parent.WriteObject(m.MultiplyBy(m1));
        }
    }
}
