using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using TestStack.BDDfy;
using TestStack.BDDfy.Scanners.StepScanners.Fluent;
using Xunit;

namespace PowerBi.Tests
{
    public class FileExtrationTest
    {
        private StubFileSystem _fileSystem;
        private PowerBiExtractor _extractor;

        [Fact]
        public void CanExtractResourcesFromAPBitFile()
        {
            this.Given(s => s.ANewPowerBiExtractor())
                .And(s => s.AFileThatExists("Template.pbit"))
                .When(s => s.TheExtractProcessIsRun())
                .Then(s => s.AllTheFilesAreCreated())
                .BDDfy();
        }

        private void AllTheFilesAreCreated()
        {
            var files = _fileSystem.ListFiles();
            files.ShouldNotBeEmpty();
        }

        private void TheExtractProcessIsRun()
        {
            _extractor.ExtractPbit("Template.pbit", "Output", true);
        }

        private void AFileThatExists(string templatePbit)
        {
            _fileSystem.AddEmbeddedFile("Template.pbit", "Template.pbit");
        }

        private void ANewPowerBiExtractor()
        {
            _fileSystem = new StubFileSystem();
            _extractor = new PowerBiExtractor(_fileSystem);
        }
    }
}
