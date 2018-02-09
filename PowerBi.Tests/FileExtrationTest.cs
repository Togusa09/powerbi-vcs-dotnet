using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Shouldly;
using TestStack.BDDfy;
using TestStack.BDDfy.Scanners.StepScanners.Fluent;
using Xunit;

namespace PowerBi.Tests
{
    public class FileExtrationTest
    {
        private MockFileSystem _fileSystem;
        private PowerBiExtractor _extractor;

        [Fact]
        public void CanExtractResourcesFromAPBitFile()
        {
            this.Given(s => s.ANewPowerBiExtractor())
                    .And(s => s.AFileThatExists("Template.pbit"))
                .When(s => s.TheExtractProcessIsRun("Template.pbit", "Output"))
                .Then(s => s.AllTheFilesAreCreated())
                .BDDfy();
        }

        [Fact]
        public void CanExtractAndRecompressAFile()
        {
            this.Given(s => s.ANewPowerBiExtractor())
                .And(s => s.AFileThatExists("Template.pbit"))
                .When(s => s.TheExtractProcessIsRun("Template.pbit", "Output"))
                    .And(s => s.TheCompressionProcessIsRun("Output", "Template2.pbit"))
                .Then(s => s.TheFileIsCreated("Template2.pbit"))
                    .And(s => s.TheFileMatchesTheOriginal())
                .BDDfy();
        }

        private void TheFileMatchesTheOriginal()
        {
            var originalFile = _fileSystem.FileInfo.FromFileName("Template.pbit");
            var newFile = _fileSystem.FileInfo.FromFileName("Template2.pbit");
            newFile.Length.ShouldBe(originalFile.Length);

            var originalFileHash = string.Empty;
            using (var stream = originalFile.Open(FileMode.Open))
            {
                originalFileHash = stream.HashFile();
            }

            var newFileHash = string.Empty;
            using (var stream = newFile.Open(FileMode.Open))
            {
                newFileHash = stream.HashFile();
            }
            newFileHash.ShouldBe(originalFileHash);
        }

        private void AllTheFilesAreCreated()
        {
            var files = _fileSystem.AllFiles;
            files.ShouldNotBeEmpty();
        }

        private void TheFileIsCreated(string filename)
        {
             _fileSystem.AllFiles.ShouldContain(@"C:\Test\" + filename);

            //var attributes = _fileSystem.FileInfo.FromFileName(@"C:\Test\" + filename);
            //attributes.Length
        }

        private void TheExtractProcessIsRun(string input, string output)
        {
            _extractor.ExtractPbit(input, output, true);
        }

        private void TheCompressionProcessIsRun(string input, string output)
        {
            _extractor.CompressPbit(input, output, true);
        }

        private void AFileThatExists(string templatePbit)
        {
            _fileSystem.AddFileFromEmbeddedResource("Template.pbit", Assembly.GetExecutingAssembly(), "PowerBi.Tests.Files.Template.pbit");
        }

        private void ANewPowerBiExtractor()
        {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), @"C:\Test\");
            _extractor = new PowerBiExtractor(_fileSystem);
        }
    }
}
