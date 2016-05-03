using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Parcs;
using System.Reflection;
using System.IO;

namespace DaemonPr
{
    [Serializable]
    public class ModuleExecutor
    { 
        private IChannel _channel;
        private IJob _currentJob;
        private int _pointNum;
        private string _assemblyFullPath;
        
        public ModuleExecutor(IChannel chan, IJob curJob, int pointNum)
        {
            _assemblyFullPath = curJob.FileName;
            _channel = chan;
            _currentJob = curJob;
            _pointNum = pointNum;
        }

        public void Run()
        {
            IModule module = null;
            string classname = (string)_channel.ReadData(typeof(string));
            byte[] file = File.ReadAllBytes(_assemblyFullPath);
            Assembly assembly = Assembly.Load(file);

            try
            {
                Type type = assembly.GetType(classname);
                module = (IModule)Activator.CreateInstance(type);
            }

            catch (ArgumentException)
            {
                Console.WriteLine("Class "
                    + classname + " for point " + _pointNum +
                    " not found");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine("Starting class " + module.GetType().ToString() +
                    " on point "
                    + _currentJob.Number + ":" + _pointNum + " ...");

            module.Run(new ModuleInfo(_currentJob, _channel));
            
            Console.WriteLine("Calcutations finished on point "
                    + _currentJob.Number + ":" + _pointNum + " ...");	                           
        }
    }
}
