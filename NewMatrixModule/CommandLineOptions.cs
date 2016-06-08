using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewMatrixModule
{
    using CommandLine;
    using CommandLine.Text;

    public class CommandLineOptions
    {
        [Option("m1", Required = true, HelpText = "File path to the first matrix.")]
        public string File1 { get; set; }
        [Option("m2", Required = true, HelpText = "File path to the second matrix.")]
        public string File2 { get; set; }
        [Option("p", Required = true, HelpText = "Number of points.")]
        public int PointsNum { get; set; }
        [Option("serverip", Required = false, HelpText = "Path to the file with the sever IP")]
        public string ServerIp { get; set; }

        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
