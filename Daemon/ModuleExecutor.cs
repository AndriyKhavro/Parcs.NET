using System;
using Parcs;
using System.Reflection;
using System.IO;
using log4net;

namespace DaemonPr
{
    [Serializable]
    public class ModuleExecutor
    { 
        private IChannel _channel;
        private IJob _currentJob;
        private int _pointNum;
        private string _assemblyFullPath;
        private readonly ILog _log = LogManager.GetLogger(typeof(ModuleExecutor));
        
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
                _log.Error("Class "
                    + classname + " for point " + _pointNum +
                    " not found");
                return;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return;
            }

            _log.Info("Starting class " + module.GetType() +
                    " on point "
                    + _currentJob.Number + ":" + _pointNum + " ...");

            module.Run(new ModuleInfo(_currentJob, _channel));

            _log.Info("Calcutations finished on point "
                    + _currentJob.Number + ":" + _pointNum + " ...");	                           
        }
    }
}
