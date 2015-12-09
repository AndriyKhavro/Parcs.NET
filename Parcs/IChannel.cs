using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Parcs
{
    public interface IChannel 
    {
        bool Works { get; }
        void WriteData(dynamic data);
        dynamic ReadData(Type type);
        void WriteObject(object obj);
        object ReadObject();
        void WriteFile(string filePath);
        string ReadFile();
        int From { get; set; }
        object ReadObject(Type type);
        T ReadObject<T>();
        void Close();
    }
}
