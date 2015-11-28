using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parcs;

namespace MatrixModule
{
    public class MultMatrix: IModule
    {
        public void Run(ModuleInfo info)
        {
            Matrix m = (Matrix)info.Parent.ReadObject();
            Matrix m1 = (Matrix)info.Parent.ReadObject();
            info.Parent.WriteObject(m.MultiplyBy(m1));
        }
    }
}
