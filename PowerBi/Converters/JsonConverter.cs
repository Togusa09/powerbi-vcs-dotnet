using System;
using System.IO;
using System.IO.Abstractions;
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
        private bool _formatRaw;

        private readonly Encoding _encoding;

        public JsonConverter(Encoding encoding, IFileSystem fileSystem, bool formatRaw) : base(fileSystem)
        {
            _encoding = encoding;
            _formatRaw = formatRaw;
        }

        // So not threadsafe...
        private int _depth = 0;

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
        private void JsonifyEmbeddedJson(JToken obj)
        {
            _depth++;
            if (_depth > 1000)
            {
                throw new Exception("Max json depth exceeded");
            }

            try
            {
                if (obj is JObject)
                {
                    var jobj = obj as JObject;
                    foreach (var t in jobj.Properties().ToList())
                    {
                        JsonifyEmbeddedJson(t.Value);
                    }

                }
                else
                {
                    var jtoken = obj;
                    if (jtoken.Type == JTokenType.String)
                    {
                        try
                        {
                            var valueString = jtoken.Value<string>();
                            if (valueString.StartsWith("{") || valueString.StartsWith("["))
                            {
                                var value = JToken.Parse(valueString);
                                if (value.Type == JTokenType.Array || value.Type == JTokenType.Object)
                                {
                                    var parent = jtoken.Parent as JProperty;
                                    var jobj = new JObject { { this.EMBEDDED_JSON_KEY, value } };
                                    var prop = new JProperty(parent.Name, jobj);
                                    parent.Replace(prop);
                                    //prop1.Value.Replace(jobj);
                                }
                            }                         
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    else if (jtoken.Type == JTokenType.Array)
                    {
                        foreach (var token in jtoken.Children().ToList())
                        {
                            JsonifyEmbeddedJson(token);
                        }
                    }
                }
            }
            finally
            {
                _depth--;
            }
        }

        private void UndoJsonifyEmbeddedJson(JToken token)
        {
       if (token is JObject)
            {
                var jobj = (JObject)token;
                foreach (var property in jobj.Properties().ToList())
                {
                    if (property.Name == EMBEDDED_JSON_KEY)
                    {
                        var settings = new JsonSerializerSettings
                        {
                            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                            DateParseHandling = DateParseHandling.DateTimeOffset,
                            Formatting = Formatting.Indented
                        };

                        var serialised = JsonConvert.SerializeObject(property.First, settings);
                        var parent = property.Parent.Parent as JProperty;
                        parent.Replace(new JProperty(parent.Name, serialised));
                    }
                    else
                    {
                        UndoJsonifyEmbeddedJson(property.Value);
                    }
                }
            }
            else
            {
                 
                if (token.Type == JTokenType.Array)
                {
                    foreach (var childToken in token.Children().ToList())
                    {
                        UndoJsonifyEmbeddedJson(childToken);
                    }
                }
            }
        }

        public override Stream RawToVcs(Stream b)
        {
            using (var streamReader = new StreamReader(b, _encoding))
            using (var reader = new JsonTextReader(streamReader))
            {
                var serialiser = new JsonSerializer
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                    DateParseHandling = DateParseHandling.DateTimeOffset,
                    Formatting = Formatting.Indented,
                };

                var obj = serialiser.Deserialize(reader) as JObject;

                JsonifyEmbeddedJson(obj);

                var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, _encoding, 1024, true))
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    serialiser.Formatting = Formatting.Indented;
                    serialiser.Serialize(writer, obj);

                    writer.Flush();
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    return memoryStream;
                }
            }
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
            using(var streamReader = new StreamReader(b, _encoding))
            using(var reader = new JsonTextReader(streamReader))
            {
                var serialiser = new JsonSerializer
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                    DateParseHandling = DateParseHandling.None,
                    Formatting = Formatting.None
                };

                if (_formatRaw)
                {
                    serialiser.Formatting = Formatting.Indented;
                }

                var obj = serialiser.Deserialize(reader);

                UndoJsonifyEmbeddedJson(obj as JToken);

                var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, _encoding, 1024, true))
                using(var writer = new JsonTextWriter(streamWriter))
                {
                    serialiser.Serialize(writer, obj);

                    writer.Flush();
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    return memoryStream;
                }
            }
        }
    }
}