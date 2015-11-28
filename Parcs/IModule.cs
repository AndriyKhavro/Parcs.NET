using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parcs
{
    public interface IModule
    {
        void Run(ModuleInfo info);
    }
}
