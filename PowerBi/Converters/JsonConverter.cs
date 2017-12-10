using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PowerBi.Converters
{
    public class JsonConverter : Converter
    {
        private readonly Encoding _encoding;

        public JsonConverter(Encoding encoding, IFileSystem fileSystem) : base(fileSystem)
        {
            _encoding = encoding;
        }

        public override Stream RawToVcs(Stream b)
        {
            var streamReader = new StreamReader(b, _encoding);
            var reader = new JsonTextReader(streamReader);

            var serialiser = new JsonSerializer
            {
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Formatting = Formatting.Indented
            };
            var obj = serialiser.Deserialize(reader);

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            var writer = new JsonTextWriter(streamWriter);
            serialiser.Formatting = Formatting.Indented;
            serialiser.Serialize(writer, obj);

            writer.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        public override string RawToConsoleText(Stream b)
        {
            var streamReader = new StreamReader(b, _encoding);
            var reader = new JsonTextReader(streamReader);

            var settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Formatting = Formatting.Indented
            };

            var serialiser = new JsonSerializer
            {
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Formatting = Formatting.Indented
            };
            var obj = serialiser.Deserialize(reader);

            return JsonConvert.SerializeObject(obj, settings);
        }

        public override Stream VcsToRaw(Stream b)
        {
            var streamReader = new StreamReader(b, _encoding);
            var reader = new JsonTextReader(streamReader);

            var serialiser = new JsonSerializer
            {
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Formatting = Formatting.None
            };

            var obj = serialiser.Deserialize(reader);

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            var writer = new JsonTextWriter(streamWriter);
            
            serialiser.Serialize(writer, obj);

            writer.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
}