using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerBi;

namespace PowerBiVcs
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileSystem = new FileSystem();
            var extractor = new pbivcs(fileSystem);

            extractor.ExtractPbit("Files\\Template.pbit", "Out1", true);
            extractor.ExtractPbit("Files\\Report.pbix", "Out2", true);

        }
    }
}
