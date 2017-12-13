using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerBi.Converters
{
    public class SectionEntry
    {
        public string Name { get; set; }
        public Guid Key { get; set; }
    }

    public class MetadataConverter : Converter
    {
        public override Stream RawToVcs(Stream b)
        {
            var outputStream = new MemoryStream();

            var section1 = new List<SectionEntry>();
            var section2 = new List<SectionEntry>();
            var section3 = new List<string>();

            var binaryReader = new BinaryReader(b);
            binaryReader.ReadBytes(8);
            var section1Length = binaryReader.ReadInt32();

            for (var i = 0; i < section1Length; i++)
            {
                var size = (short)binaryReader.ReadByte();
                var text = new string(binaryReader.ReadChars(size));
                var size2 = (short)binaryReader.ReadByte();
                var text2 = new string(binaryReader.ReadChars(size2));
                section1.Add(new SectionEntry { Name = text, Key = Guid.Parse(text2)});
            }

            var section2Length = binaryReader.ReadInt32();

            for (var i = 0; i < section2Length; i++)
            {
                var size = (short)binaryReader.ReadByte();
                var text = new string(binaryReader.ReadChars(size));
                var size2 = (short)binaryReader.ReadByte();
                var text2 = new string(binaryReader.ReadChars(size2));
                section2.Add(new SectionEntry { Name = text2, Key = Guid.Parse(text) });
            }

            binaryReader.ReadByte();
            var section3Length = (int)binaryReader.ReadByte();
            for (var i = 0; i < section3Length; i++){
                var size = (short)binaryReader.ReadByte();
                var text = new string(binaryReader.ReadChars(size));
                section3.Add(text);
            }

            var streamWriter = new StreamWriter(outputStream);
            streamWriter.WriteLine("Section1");
            foreach (var entry in section1)
            {
                streamWriter.WriteLine($"{entry.Name}: {entry.Key}");
            }
            streamWriter.WriteLine("Section2");
            foreach (var entry in section1)
            {
                streamWriter.WriteLine($"{entry.Name}: {entry.Key}");
            }
            streamWriter.WriteLine("Section3");
            foreach (var entry in section3)
            {
                streamWriter.WriteLine(entry);
            }

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