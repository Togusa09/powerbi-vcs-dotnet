using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PowerBi.Converters
{
    public class JsonConverter : Converter
    {
        private string EMBEDDED_JSON_KEY = "__powerbi-vcs-embedded-json__";
        private bool SORT_KEYS = false;

        private readonly Encoding _encoding;

        public JsonConverter(Encoding encoding, IFileSystem fileSystem) : base(fileSystem)
        {
            _encoding = encoding;
        }

        /// <summary>
        /// Some pbit json has embedded json strings. To aid readability and diffs etc., we make sure we load and format
        ///these too.To make sure we're aware of this, we follow the encoding:
        ///```
        ///x: "{\"y\": 1 }"
        ///```
        ///
        ///becomes
        ///
        ///```
        ///x: { EMBEDDED_JSON_KEY: { "y": 1 } }
        ///```
        /// </summary>
        /// <param name="v"></param>
        private void JsonifyEmbeddedJson(Object obj)
        {
            if (obj is JObject)
            {
                var jobj = obj as JObject;
                foreach (var t in jobj.Properties().ToList())
                {
                    JsonifyEmbeddedJson(t.Value);
                }
                
            }
            else if (obj is JToken)
            {
                var jtoken = obj as JToken;
                if (jtoken.Type == JTokenType.String)
                {
                    try
                    {
                        var value = JToken.Parse(jtoken.Value<string>());
                        if (value.Type == JTokenType.Array || value.Type == JTokenType.Object)
                        {
                            var parent = jtoken.Parent as JProperty;
                            var jobj = new JObject {{this.EMBEDDED_JSON_KEY, value}};
                            var prop = new JProperty(parent.Name, jobj);
                            parent.Replace(prop);
                            //prop1.Value.Replace(jobj);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                } else if (jtoken.Type == JTokenType.Array)
                {
                    foreach (var token in jtoken.Children().ToList())
                    {
                        JsonifyEmbeddedJson(token);
                    }
                }

            }
        }

        private void UndoJsonifyEmbeddedJson(string v)
        {
            
        }

        public override Stream RawToVcs(Stream b)
        {
            var streamReader = new StreamReader(b, _encoding);
            var reader = new JsonTextReader(streamReader);

            var serialiser = new JsonSerializer
            {
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Formatting = Formatting.Indented,
            };
            
            var obj = serialiser.Deserialize(reader) as JObject;

            this.JsonifyEmbeddedJson(obj);

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