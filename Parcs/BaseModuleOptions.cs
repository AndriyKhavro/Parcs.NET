using CommandLine;
using CommandLine.Text;

namespace Parcs
{
    public class BaseModuleOptions
    {
        [Option("priority", Required = false, HelpText = "Priority of Job used for job dispatching")]
        public int Priority { get; set; }
        [Option("user", Required = false, HelpText = "Username under which the job will be running")]
        public string Username { get; set; }
        [Option("serverip", Required = false, HelpText = "Path to the file with the sever IP")]
        public string ServerIp { get; set; }

        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
