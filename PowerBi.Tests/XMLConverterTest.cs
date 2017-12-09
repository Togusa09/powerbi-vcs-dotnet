using System.IO;
using System.Text;
using Shouldly;
using TestStack.BDDfy;
using TestStack.BDDfy.Scanners.StepScanners.Fluent;
using Xunit;

namespace PowerBi.Tests
{
    public class XMLConverterTest
    {
        private XMLConverter _XMLConverter;
        private string _input;
        private string _output;
        private Encoding _outputEncoding;
        private Encoding _inputEncoding = Encoding.UTF8;

        [Fact]
        public void ConvertUnformattedXMLWithoutHeaderToVcs()
        {
            this.Given(s => s.ANewXMLConverter(Encoding.UTF8))
                .And(s => s.UnformattedXMLInputWithoutHeader())
                .When(s => s.ConvertedFromRawToVcs())
                .Then(s => s.TheXMLShouldBeFormattedAndContainTheCorrectEncoding())
                .BDDfy();
        }

        [Fact]
        public void ConvertFormattedXMLWithoutHeaderToRaw()
        {
            this.Given(s => s.ANewXMLConverter(Encoding.UTF8))
                .And(s => s.FormattedXMLInputWithoutHeader())
                .When(s => s.ConvertedFromVcsToRaw())
                .Then(s => s.TheXMLShouldBeUnformattedAndContainTheCorrectEncoding())
                .BDDfy();
        }

        [Theory]
        [InlineData("UTF-8")]
        [InlineData("UTF-16")]
        public void ConvertFormattedXMLWithHeaderEncodingToVcs(string encodingString)
        {
            var encoding = Encoding.GetEncoding(encodingString);
            _inputEncoding = encoding;

            this.Given(s => s.ANewXMLConverter(encoding))
                .And(s => s.UnformattedXMLInputWithHeader(encoding))
                .When(s => s.ConvertedFromRawToVcs())
                .Then(s => s.TheXMLShouldBeFormattedAndContainTheCorrectEncoding())
                .BDDfy();
        }

        [Theory]
        [InlineData("UTF-8")]
        [InlineData("UTF-16")]
        public void ConvertFormattedXMLWithHeaderEncodingToRaw(string encodingString)
        {
            var encoding = Encoding.GetEncoding(encodingString);
            _inputEncoding = encoding;

            this.Given(s => s.ANewXMLConverter(encoding))
                .And(s => s.FormattedXMLInputWithHeader(encoding))
                .When(s => s.ConvertedFromVcsToRaw())
                .Then(s => s.TheXMLShouldBeUnformattedAndContainTheCorrectEncoding())
                .BDDfy();
        }


        private void ConvertedFromRawToVcs()
        {
            using (var stream = new MemoryStream())
            using (var t = new StreamWriter(stream, _inputEncoding))
            {
                t.Write(_input);
                t.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                using (var result = _XMLConverter.RawToVcs(stream))
                using (var reader = new StreamReader(result))
                {
                    _output = reader.ReadToEnd();
                }
            }
        }

        private void ConvertedFromVcsToRaw()
        {
            using (var stream = new MemoryStream())
            using (var t = new StreamWriter(stream, _inputEncoding))
            {
                t.Write(_input);
                t.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                using (var result = _XMLConverter.VcsToRaw(stream))
                using (var reader = new StreamReader(result))
                {
                    _output = reader.ReadToEnd();
                }
            }
        }

        private void TheXMLShouldBeFormattedAndContainTheCorrectEncoding()
        {
            _output.ShouldBe($"<?xml version=\"1.0\" encoding=\"{_outputEncoding.HeaderName}\"?>\r\n<Entities>\r\n  <Entity Name=\"Test\" />\r\n</Entities>");
        }

        private void TheXMLShouldBeUnformattedAndContainTheCorrectEncoding()
        {
            _output.ShouldBe($"<?xml version=\"1.0\" encoding=\"{_outputEncoding.HeaderName}\"?><Entities><Entity Name=\"Test\" /></Entities>");
        }

        private void UnformattedXMLInputWithHeader(Encoding encoding)
        {
            _input = $"<?xml version=\"1.0\" encoding=\"{encoding.HeaderName}\"?><Entities><Entity Name=\"Test\" /></Entities>";
        }

        private void UnformattedXMLInputWithoutHeader()
        {
            _input = "<Entities><Entity Name=\"Test\" /></Entities>";
        }

        private void FormattedXMLInputWithHeader(Encoding encoding)
        {
            _input = $"<?xml version=\"1.0\" encoding=\"{encoding.HeaderName}\"?>\n<Entities>\n\t<Entity Name=\"Test\" />\n</Entities>";
        }

        private void FormattedXMLInputWithoutHeader()
        {
            _input = "<Entities>\n\t<Entity Name=\"Test\" />\n</Entities>";
        }

        private void ANewXMLConverter(Encoding encoding)
        {
            var fileSystem = new StubFileSystem();
            _outputEncoding = encoding;
            _XMLConverter = new XMLConverter(encoding, fileSystem);
        }
    }
}
