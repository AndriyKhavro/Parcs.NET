using System;
using Parcs;
using System.Reflection;
using System.Threading;
using Serilog;

namespace DaemonPr
{
    [Serializable]
    public class ModuleExecutor
    { 
        private readonly IChannel _channel;
        private readonly IJob _currentJob;
        private readonly int _pointNum;
        private readonly ModuleFileStorage _moduleFileStorage;
        private static readonly ILogger _log = Log.Logger.ForContext<ModuleExecutor>();
        
        internal ModuleExecutor(IChannel chan, IJob curJob, int pointNum, ModuleFileStorage moduleFileStorage)
        {
            _channel = chan;
            _currentJob = curJob;
            _pointNum = pointNum;
            _moduleFileStorage = moduleFileStorage;
        }

        public void Run(CancellationToken token)
        {
            IModule module = null;
            string classname = _channel.ReadString();
            byte[] file = _moduleFileStorage.ReadFile(_currentJob.FileName);
            Assembly assembly = Assembly.Load(file);

            try
            {
                Type type = assembly.GetType(classname);
                module = (IModule)Activator.CreateInstance(type);
            }

            catch (ArgumentException ex)
            {
                _log.Error(ex, "Class "
                    + classname + " for point " + _pointNum +
                    " not found");
                return;
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
                return;
            }

            _log.Information("Starting class " + module.GetType() +
                    " on point "
                    + _currentJob.Number + ":" + _pointNum + " ...");

            module.Run(new ModuleInfo(_currentJob, _channel), token);

            _log.Information("Calcutations finished on point "
                    + _currentJob.Number + ":" + _pointNum + " ...");	                           
        }
    }
}
