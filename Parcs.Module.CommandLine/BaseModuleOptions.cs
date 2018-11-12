using CommandLine;

namespace Parcs.Module.CommandLine
{
    public class BaseModuleOptions : IModuleOptions
    {
        [Option("priority", Required = false, HelpText = "Priority of Job used for job dispatching")]
        public int Priority { get; set; }
        [Option("user", Required = false, HelpText = "Username under which the job will be running")]
        public string Username { get; set; }
        [Option("serverip", Required = false, HelpText = "Host Server IP")]
        public string ServerIp { get; set; }

        /// <summary>
        /// Returns empty string for now due to breaking changes in CommandLine package
        /// </summary>
        /// <returns></returns>
        public string GetUsage()
        {
            return "";
        }
    }
}
