using System.IO;
using System.IO.Abstractions.TestingHelpers;
using PowerBi.Converters;
using Shouldly;
using TestStack.BDDfy;
using TestStack.BDDfy.Scanners.StepScanners.Fluent;
using Xunit;

namespace PowerBi.Tests
{
    
    public class NoopConverterTest
    {
        private NoopConverter _converter;
        private byte[] _input;
        private byte[] _output;

        [Fact]
        public void ConvertAnEmptyArrayFromRawToVcs()
        {
            this.Given(s => s.ANewNoopConverter())
                    .And(s => s.AnEmptyArray())
                .When(s => s.TheArrayIsConvertedFromRawToVcs())
                .Then(s => s.ItShouldOutputTheSameArray())
                    .And(s => s.TheArrayShouldBeEmpty())
                .BDDfy();
        }

        [Fact]
        public void ConvertAPopulatedArrayFromRawToVcs()
        {
            this.Given(s => s.ANewNoopConverter())
                    .And(s => s.APopulatedArray())
                .When(s => s.TheArrayIsConvertedFromRawToVcs())
                .Then(s => s.ItShouldOutputTheSameArray())
                .BDDfy();
        }

        [Fact]
        public void ConvertAnEmptyArrayFromVcsToRaw()
        {
            this.Given(s => s.ANewNoopConverter())
                .And(s => s.AnEmptyArray())
                .When(s => s.TheArrayIsConvertedFromVcsToRaw())
                .Then(s => s.ItShouldOutputTheSameArray())
                .And(s => s.TheArrayShouldBeEmpty())
                .BDDfy();
        }

        [Fact]
        public void ConvertAPopulatedArrayFromVcsToRaw()
        {
            this.Given(s => s.ANewNoopConverter())
                .And(s => s.APopulatedArray())
                .When(s => s.TheArrayIsConvertedFromVcsToRaw())
                .Then(s => s.ItShouldOutputTheSameArray())
                .BDDfy();
        }

        private void ItShouldOutputTheSameArray()
        {
            _output.ShouldNotBeNull();
            _output.ShouldBe(_input);
        }

        private void TheArrayShouldBeEmpty()
        {
            _output.ShouldBeEmpty();
        }

        private void TheArrayIsConvertedFromRawToVcs()
        {
            var inputStream = new MemoryStream(_input);

            inputStream.Flush();
            inputStream.Seek(0, SeekOrigin.Begin);

            var outputStream = _converter.RawToVcs(inputStream);
            
                var ms = outputStream as MemoryStream;
                _output = ms.ToArray();
            
        }
        private void TheArrayIsConvertedFromVcsToRaw()
        {
            var inputStream = new MemoryStream(_input);

            inputStream.Flush();
            inputStream.Seek(0, SeekOrigin.Begin);
            using (var outputStream = _converter.VcsToRaw(inputStream))
            {
                var ms = outputStream as MemoryStream;
                _output = ms.ToArray();
            }
        }

        private void AnEmptyArray()
        {
            _input = new byte[0];
        }

        private void APopulatedArray()
        {
            _input = new byte[] { 0x01, 0x02, 0x03, 0x4 };
        }

        private void ANewNoopConverter()
        {
            var fileSystem = new MockFileSystem();
            _converter = new NoopConverter(fileSystem);
        }
    }
}
