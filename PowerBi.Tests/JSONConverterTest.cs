using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using PowerBi.Converters;
using Shouldly;
using TestStack.BDDfy;
using TestStack.BDDfy.Scanners.StepScanners.Fluent;
using Xunit;

namespace PowerBi.Tests
{
    public class JSONConverterTest
    {
        private JsonConverter _jsonConverter;
        private string _input;
        private string _output;

        [Fact]
        public void ConvertUnformattedJsonToVcs()
        {
            this.Given(s => s.ANewJsonConverter())
                .And(s => s.UnformattedJsonInput())
                .When(s => s.ConvertedFromRawToVcs())
                .Then(s => s.TheJsonShouldBeFormatted())
                .BDDfy();
        }

        [Fact]
        public void ConvertFormattedJsonToRaw()
        {
            this.Given(s => s.ANewJsonConverter())
                .And(s => s.FormattedJsonInput())
                .When(s => s.ConvertedFromVcsToRaw())
                .Then(s => s.TheJsonShouldBeUnformatted())
                .BDDfy();
        }

        private void ConvertedFromRawToVcs()
        {
            using (var stream = new MemoryStream())
            using (var t = new StreamWriter(stream))
            {
                t.Write(_input);
                t.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                using (var result = _jsonConverter.RawToVcs(stream))
                using(var reader = new StreamReader(result))
                {
                    result.Seek(0, SeekOrigin.Begin);
                    _output = reader.ReadToEnd();
                }
            }
        }

        private void ConvertedFromVcsToRaw()
        {
            using (var stream = new MemoryStream())
            using (var t = new StreamWriter(stream))
            {
                t.Write(_input);
                t.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                using (var result = _jsonConverter.VcsToRaw(stream))
                using (var reader = new StreamReader(result))
                {
                    result.Seek(0, SeekOrigin.Begin);
                    _output = reader.ReadToEnd();
                }
            }
        }

        private void TheJsonShouldBeFormatted()
        {
            _output.ShouldBe("{\r\n  \"TestKey\": \"TestValue\",\r\n  \"lastUpdate\": \"2017-07-10T07:49:43.696667+12:00\"\r\n}");
        }

        private void TheJsonShouldBeUnformatted()
        {
            _output.ShouldBe("{\"TestKey\":\"TestValue\"}");
        }

        private void UnformattedJsonInput()
        {
            _input = "{\"TestKey\": \"TestValue\", \"lastUpdate\": \"2017-07-10T07:49:43.696667+12:00\"}";
        }

        private void FormattedJsonInput()
        {
            _input = "{\n\t\"TestKey\": \"TestValue\"\n}";
        }

        private void ANewJsonConverter()
        {
            var fileSystem = new MockFileSystem();
            _jsonConverter = new JsonConverter(Encoding.Default, fileSystem);
        }
    }
}
