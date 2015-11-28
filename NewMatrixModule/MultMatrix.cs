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
            Matrix m = (Matrix)info.Parent.ReadObject(typeof(Matrix));
            Matrix m1 = (Matrix)info.Parent.ReadObject(typeof(Matrix));
            info.Parent.WriteObject(m.MultiplyBy(m1));
        }
    }
}
