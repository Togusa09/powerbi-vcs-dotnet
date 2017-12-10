using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerBi.Converters
{
    public class MetadataConverter : Converter
    {
        public override Stream RawToVcs(Stream b)
        {
            var streamReader = new StreamReader(b, Encoding.ASCII);
            var content = streamReader.ReadToEnd();
            var regex = new Regex("(\\\\x[0-9a-f]{2})([^\\\\x])");
            var splitContent = regex.Split(content);

            var outputStream = new MemoryStream();
            var streamWriter = new StreamWriter(outputStream, Encoding.ASCII);

            int i = 0;
            foreach (var t in splitContent)
            {
                if (i % 3 == 2)
                {
                    streamWriter.Write(Environment.NewLine);
                }
                streamWriter.Write(t);

                i++;
            }
            streamWriter.Flush();
            return outputStream;
        }

        public override Stream VcsToRaw(Stream b)
        {
            b.Seek(0, SeekOrigin.Begin);

            var streamReader = new StreamReader(b, Encoding.ASCII);
            var content = streamReader.ReadToEnd();

            var outputStream = new MemoryStream();
            var streamWriter = new StreamWriter(outputStream, Encoding.ASCII);

            streamWriter.Write(content.Replace("\r", "").Replace("\n", ""));

            streamWriter.Flush();
            return outputStream;
        }

        public override string RawToConsoleText(Stream b)
        {
            using (var stream = RawToVcs(b))
            using(var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public MetadataConverter(IFileSystem fileSystem) : base(fileSystem)
        {
            
        }
    }
}