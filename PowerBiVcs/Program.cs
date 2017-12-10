using System;
using System.Diagnostics;
using CommandLine;
using PowerBi;

namespace PowerBiVcs
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new CommandLineOptions();
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(x => options = x);

            if (options.Input == options.Output)
            {
                Console.WriteLine("Error! Input and output paths cannot be same");
                return;
            }

            if ((options.ExtractToVcs || options.CompressFromVcs) && string.IsNullOrEmpty(options.Output))
            {
                Console.WriteLine("Error! Output is required for extraction and compression");
                return;
            }

            var fileSystem = new FileSystem();
            var extractor = new PowerBiExtractor(fileSystem);

            if (options.ExtractToVcs)
            {
                extractor.ExtractPbit(options.Input, options.Output, options.Overwrite);
            }

            if (options.CompressFromVcs)
            {
                extractor.CompressPbit(options.Input, options.Output, options.Overwrite);
            }

            if (options.WriteToScreen)
            {
                extractor.WritePbitToScreen(options.Input);
                if (Debugger.IsAttached)
                {
                    Console.ReadLine();
                }
            }


            //extractor.ExtractPbit("Files\\Template.pbit", "Out1", true);
            //extractor.ExtractPbit("Files\\Report.pbix", "Out2", true);
        }
    }
}
