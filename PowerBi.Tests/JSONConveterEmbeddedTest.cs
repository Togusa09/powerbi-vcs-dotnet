using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerBi.Converters;
using Shouldly;
using TestStack.BDDfy;
using TestStack.BDDfy.Scanners.StepScanners.Fluent;
using Xunit;

namespace PowerBi.Tests
{
    public class JSONConveterEmbeddedTest
    {
        private JsonConverter _jsonConverter;
        private string _input;
        private string _output;

        [Fact]
        public void ConvertFormattedJsonToRaw()
        {
            this.Given(s => s.ANewJsonConverter())
                .And(s => s.JSONStringWithEmbeddedJSON())
                .When(s => s.ConvertedFromVcsToRaw())
                .Then(s => s.TheJsonShouldBeUnformatted())
                .BDDfy();
        }

        private void JSONStringWithEmbeddedJSON()
        {
            _input =
                "{\"config\":{\"__powerbi-vcs-embedded-json__\":{\"name\":\"VisualContainer\",\"layouts\":[{\"id\":0,\"position\":{\"x\":10.24,\"y\":20.48,\"z\":1,\"width\":1268.48,\"height\":698.24}}]}}}";
        }

        private void ANewJsonConverter()
        {
            var fileSystem = new MockFileSystem();
            _jsonConverter = new JsonConverter(Encoding.Default, fileSystem, false);
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

        private void TheJsonShouldBeUnformatted()
        {
            _output.ShouldBe("{\"config\":\"{\\\"name\\\":\\\"VisualContainer\\\",\\\"layouts\\\":[{\\\"id\\\":0,\\\"position\\\":{\\\"x\\\":10.24,\\\"y\\\":20.48,\\\"z\\\":1,\\\"width\\\":1268.48,\\\"height\\\":698.24}}]}\"}");
        }
    }
}
