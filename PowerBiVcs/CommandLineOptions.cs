using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace PowerBiVcs
{
    class CommandLineOptions
    {
        //[Option('r', "read", Required = true, HelpText = "Input files to be processed.")]
        //public IEnumerable<string> InputFiles { get; set; }

        [Value(0, MetaName = "input", HelpText = "The input path", Required = true)]
        public string Input { get; set; }

        [Value(1, MetaName = "output", HelpText = "The output path")]
        public string Output { get; set; }

        [Option('x', Default = false, HelpText = "extract pbit at INPUT to VCS-friendly format at OUTPUT", SetName = "extract")]
        public bool ExtractToVcs { get; set; }
        [Option('c', Default = false, HelpText = "compress VCS-friendly format at INPUT to pbit at OUTPUT", SetName = "extract")]
        public bool CompressFromVcs { get; set; }
        [Option('s', Default = false, HelpText = "write content of file to screen", SetName = "extract")]
        public bool WriteToScreen { get; set; }


        [Option("over-write", HelpText = "if present, allow overwriting of OUTPUT. If not, will fail if OUTPUT exists")]
        public bool Overwrite { get; set; }
    }
}
