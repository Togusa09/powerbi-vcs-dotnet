using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PowerBi.Converters;

namespace PowerBi
{
    public class PowerBiExtractor
    {
        private readonly IFileSystem _fileSystem;
        private readonly Dictionary<string, Converter> _converters;

        public PowerBiExtractor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _converters = new Dictionary<string, Converter>()
            {
                {"DataModelSchema", new JsonConverter(Encoding.Unicode, fileSystem)  },
                {"DiagramState", new  JsonConverter(Encoding.Unicode, fileSystem)},
                {"Report/Layout", new JsonConverter(Encoding.Unicode, fileSystem)},
                {"Report/LinguisticSchema", new  XMLConverter(Encoding.Unicode, fileSystem)},
                {"[Content_Types].xml", new  XMLConverter(Encoding.UTF8, fileSystem)},
                {"SecurityBindings", new  NoopConverter(fileSystem)},
                {"Settings", new  NoopConverter(fileSystem)},
                {"Version", new  NoopConverter(fileSystem)},
                {"Report/StaticResources/", new  NoopConverter(fileSystem)},
                {"DataMashup", new DataMashupConverter(fileSystem)},
                {"Metadata", new MetadataConverter(fileSystem)},
                {"*.json", new  JsonConverter(Encoding.UTF8, fileSystem)},
            };
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
            return new NoopConverter(_fileSystem);
        }

        public void CompressPbit(string path, string outdir, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public void WritePbitToScreen(string path)
        {
            var stringBuilder = new StringBuilder();
            using (var zip = _fileSystem.OpenZipFile(path))
            {
                foreach (var zipArchiveEntry in zip.Entries)
                {
                    
                    var converter = FindConverter(zipArchiveEntry.FullName);

                    using (var zipStream = zipArchiveEntry.Open())
                    {
                        var fileText = converter.WriteRawToConsoleText(zipStream);
                        stringBuilder.AppendLine("Filename: " + zipArchiveEntry.FullName);
                        stringBuilder.AppendLine(fileText);
                    }
                }
            }

            Console.WriteLine(stringBuilder.ToString());
        }

        public void ExtractPbit(string path, string outdir, bool overwrite)
        {
            //if (string.Compare(Path.GetExtension(path), ".pbit", StringComparison.OrdinalIgnoreCase) != 0)
            //{
            //    throw new ArgumentException("File must be of type *.pbit", nameof(path));
            //}

            EnsureDestinationFolderExists(outdir, overwrite);

            var order = new List<string>();
            using (var zip = _fileSystem.OpenZipFile(path))
            {
                foreach (var zipArchiveEntry in zip.Entries)
                {
                    order.Add(zipArchiveEntry.FullName);
                    var outpath = Path.Combine(outdir, zipArchiveEntry.FullName);
                    var converter = FindConverter(zipArchiveEntry.FullName);

                    converter.WriteRawToVcs(zipArchiveEntry.Open(), outpath);
                }
            }

            using (var file = _fileSystem.CreateNewFile(outdir + ".zo"))
            using (var writer = new StreamWriter(file))
            {
                writer.Write(string.Join("\n", order));
            }
        }

        private void EnsureDestinationFolderExists(string outdir, bool overwrite)
        {
            if (_fileSystem.DirectoryExists(outdir))
            {
                if (overwrite)
                {
                    var existingFiles = Directory.EnumerateFiles(outdir);
                    foreach (var file in existingFiles)
                    {
                        _fileSystem.DeleteFile(file);
                    }
                }
                else
                {
                    throw new Exception($"Output path \"{outdir}\" already exists");
                }
            }
            else
            {
                _fileSystem.CreateDirectory(outdir);
            }
        }

        
    }
}