using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PowerBi.Converters;

namespace PowerBi
{
    public class DataMashupConverter : Converter
    {
        private readonly Dictionary<string, Converter> _converters;

        public DataMashupConverter(IFileSystem fileSystem) : base(fileSystem)
        {
            _converters = new Dictionary<string, Converter>()
            {
                {"[Content_Types].xml", new  XMLConverter(Encoding.UTF8, fileSystem)},
                {"Config/Package.xml", new  XMLConverter(Encoding.UTF8, fileSystem)},
                {"Formulas/Section1.m", new  NoopConverter(fileSystem)},
            };
        }

        public override void WriteRawToVcs(Stream zipStream, string vcsPath)
        {
            var memoryStream = new MemoryStream();
            zipStream.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();

            var len1 = BitConverter.ToInt32(bytes, 4);
            var start1 = 8;
            var zip1 = bytes.Skip(start1).Take(len1).ToArray();

            var start2 = start1 + len1 + 4;
            var len2 = BitConverter.ToInt32(bytes, start1 + len1);
            var xml1 = bytes.Skip(start2).Take(len2).ToArray();

            var b8 = bytes.Skip(start2 + len2).Take(8).ToArray();
            var start3 = start2 + len2 + 12;
            var len3 = BitConverter.ToInt32(bytes, start2 + len2 + 8);
            if (BitConverter.ToInt32(bytes, start2 + len2) - len3 != 34)
            {
                throw new Exception("TODO");
            }

            var xml2 = bytes.Skip(start3).Take(len3).ToArray();
            var extra = bytes.Skip(start3 + len3).ToArray();

            var zip1Stream = new MemoryStream(zip1);
            zip1Stream.Seek(0, SeekOrigin.Begin);

            var order = new List<string>();
            using (var zip = new ZipArchive(zip1Stream))
            {
                foreach (var zipArchiveEntry in zip.Entries)
                {
                    order.Add(zipArchiveEntry.FullName);
                    var outpath = Path.Combine(vcsPath, zipArchiveEntry.FullName);
                    var converter = FindConverter(zipArchiveEntry.FullName);

                    converter.WriteRawToVcs(zipArchiveEntry.Open(), outpath);
                }
            }

            using (var file = _fileSystem.CreateNewFile(Path.Combine(vcsPath, ".zo")))
            using (var writer = new StreamWriter(file))
            {
                writer.Write(string.Join("\n", order));
            }
            var xmlStream1 =  new MemoryStream(xml1);
            new XMLConverter(Encoding.UTF8, _fileSystem).WriteRawToVcs(xmlStream1, Path.Combine(vcsPath, "3.xml"));

            var xmlStream2 = new MemoryStream(xml2);
            new XMLConverter(Encoding.UTF8, _fileSystem).WriteRawToVcs(xmlStream2, Path.Combine(vcsPath, "6.xml"));

            var extraStream = new MemoryStream(extra);
            new NoopConverter(_fileSystem).WriteRawToVcs(extraStream, Path.Combine(vcsPath, "7.bytes"));
        }

        public override void WriteVcsToRaw(string vcsdir, ZipArchive zipFile)
        {
            //zip up the header bytes
            var stream = new MemoryStream();
            var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var order = File.ReadAllLines(Path.Combine(vcsdir, ".zo"));
                foreach (var name in order)
                {
                    var converter = FindConverter(name);
                    converter.WriteVcsToRaw(Path.Combine(vcsdir, name), zip );
                }
            }

            //Write header
            stream.Write(new byte[] { 0x00, 0x00, 0x00, 0x00},0, 4);
            var writer = new BinaryWriter(stream);

            //write zip
            zipStream.Flush();
            zipStream.Seek(0, SeekOrigin.Begin);
            zipStream.WriteTo(stream);

            var xmlb = new XMLConverter(Encoding.UTF8, _fileSystem).VcsToRaw(_fileSystem.OpenFile(Path.Combine(vcsdir, "3.xml")));
            var xmlStream1 = new MemoryStream();
            xmlb.CopyTo(xmlStream1);

            writer.Write((int)xmlStream1.Length);
            writer.Write(xmlStream1.ToArray());

            xmlb = new XMLConverter(Encoding.UTF8, _fileSystem).VcsToRaw(_fileSystem.OpenFile(Path.Combine(vcsdir, "6.xml")));
            var xmlStream2 = new MemoryStream();
            xmlb.CopyTo(xmlStream2);

            writer.Write((int)xmlStream2.Length + 34);
            writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            writer.Write(xmlStream2.ToArray());

            new NoopConverter(_fileSystem).WriteVcsToRaw(Path.Combine(vcsdir, "7.bytes"), zipFile);
        }


        public override Stream RawToVcs(Stream b)
        {
            throw new NotImplementedException();
        }

        public override string RawToConsoleText(Stream zipStream)
        {
            var stringBuilder = new StringBuilder();

            var memoryStream = new MemoryStream();
            zipStream.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();

            var len1 = BitConverter.ToInt32(bytes, 4);
            var start1 = 8;
            var zip1 = bytes.Skip(start1).Take(len1).ToArray();

            var start2 = start1 + len1 + 4;
            var len2 = BitConverter.ToInt32(bytes, start1 + len1);
            var xml1 = bytes.Skip(start2).Take(len2).ToArray();

            var b8 = bytes.Skip(start2 + len2).Take(8).ToArray();
            var start3 = start2 + len2 + 12;
            var len3 = BitConverter.ToInt32(bytes, start2 + len2 + 8);
            if (BitConverter.ToInt32(bytes, start2 + len2) - len3 != 34)
            {
                throw new Exception("TODO");
            }

            var xml2 = bytes.Skip(start3).Take(len3).ToArray();
            var extra = bytes.Skip(start3 + len3).ToArray();

            var zip1Stream = new MemoryStream(zip1);
            zip1Stream.Seek(0, SeekOrigin.Begin);

            var order = new List<string>();
            using (var zip = new ZipArchive(zip1Stream))
            {
                foreach (var zipArchiveEntry in zip.Entries)
                {
                    order.Add(zipArchiveEntry.FullName);
                    var converter = FindConverter(zipArchiveEntry.FullName);

                    var text = converter.WriteRawToConsoleText(zipArchiveEntry.Open());
                    stringBuilder.AppendLine("Filename: " + zipArchiveEntry.FullName);
                    stringBuilder.AppendLine(text);
                }
            }

            var xmlStream1 = new MemoryStream(xml1);
            var xmlString1 = new XMLConverter(Encoding.UTF8, _fileSystem).WriteRawToConsoleText(xmlStream1);
            stringBuilder.AppendLine("DataMashup -> XML Block 1");
            stringBuilder.AppendLine(xmlString1);

            var xmlStream2 = new MemoryStream(xml2);
            var xmlString2 = new XMLConverter(Encoding.UTF8, _fileSystem).WriteRawToConsoleText(xmlStream2);
            stringBuilder.AppendLine("DataMashup -> XML Block 2");
            stringBuilder.AppendLine(xmlString2);

            var extraStream = new MemoryStream(extra);
            var extraContentString = new NoopConverter(_fileSystem).WriteRawToConsoleText(extraStream);
            stringBuilder.AppendLine("DataMashup -> Extra Content");
            stringBuilder.AppendLine(extraContentString);
            return stringBuilder.ToString();
        }

        public override Stream VcsToRaw(Stream b)
        {
            throw new System.NotImplementedException();
        }

        public Converter FindConverter(string path)
        {

            Regex.Escape(path).Replace(@"\*", ".*").Replace(@"\?", ".");
            foreach (var converter in _converters)
            {
                if (path.MatchesGlob(converter.Key))
                {
                    return converter.Value;
                }
            }
            return new NoopConverter(new FileSystem());
        }
    }
}