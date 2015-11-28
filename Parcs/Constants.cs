using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parcs
{
    public enum Constants: byte
    {
        PointCreated,
        PointDeleted,
        RecieveTask,
        LoadFile,
        LoadClasses,
        ExecuteClass,
        ConnectPoint, 
        BeginJob,
        ProcessorsCountRequest,
        FinishJob,
        LinpackRequest,
        ServerIP
    }
    public enum Ports
    {
        ServerPort = 1234,
        DaemonPort = 2222
    }

 
}
