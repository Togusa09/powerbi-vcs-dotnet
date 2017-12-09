using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerBi
{
    public class pbivcs
    {
        private readonly IFileSystem _fileSystem;
        private Dictionary<string, Converter> _converters;

        public pbivcs(IFileSystem fileSystem)
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

        public void ExtractPbit(string path, string outdir, bool overwrite)
        {
            if (string.Compare(Path.GetExtension(path), ".pbit", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException("File must be of type *.pbit", nameof(path));
            }

            if (_fileSystem.DirectoryExists(outdir))
            {
                if (overwrite)
                {
                    var existingFiles = Directory.EnumerateFiles(outdir);
                    foreach (var file in existingFiles)
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    throw new Exception($"Output path \"{outdir}\" already exists");
                }
            }
            else
            {
                Directory.CreateDirectory(outdir);
            }

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
    }

 

    public abstract class Converter
    {
        protected IFileSystem _fileSystem;

        protected Converter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public abstract Stream RawToVcs(Stream b);

        public abstract Stream VcsToRaw(Stream b);

        public virtual void WriteRawToVcs(Stream zipStream, string vcsPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(vcsPath));

            using (var file = _fileSystem.CreateNewFile(vcsPath))
            {
                using (var outStream = RawToVcs(zipStream))
                {
                    file.Seek(0, SeekOrigin.Begin);
                    //outStream.Seek(0, SeekOrigin.Begin);
                    outStream.CopyTo(file);
                    file.Flush();
                }     
            }
        }

        public virtual void WriteVcsToRaw(string vcsPath, ZipArchive zipFile)
        {
            //var zipArchive = ZipFile.Open(vcsPath, ZipArchiveMode.Create);
            if (File.Exists(vcsPath))
            {
                zipFile.CreateEntryFromFile(vcsPath, Path.GetFileName(vcsPath), CompressionLevel.NoCompression);
            }
        }
    }
}
