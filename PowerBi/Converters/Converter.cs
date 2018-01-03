using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Text;

namespace PowerBi.Converters
{
    public abstract class Converter
    {
        protected IFileSystem _fileSystem;

        protected Converter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public abstract Stream RawToVcs(Stream b);
        public abstract string RawToConsoleText(Stream b);
        

        public abstract Stream VcsToRaw(Stream b);

        public virtual void WriteRawToVcs(Stream zipStream, string vcsPath)
        {
            _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(vcsPath.Replace('/', '\\')));

            using (var file = _fileSystem.File.Create(vcsPath))
            {
                using (var outStream = RawToVcs(zipStream))
                {
                    file.Seek(0, SeekOrigin.Begin);
                    outStream.CopyTo(file);
                    file.Flush();
                }     
            }
        }

        public virtual void WriteVcsToRaw(string vcsPath, ZipArchive zipFile)
        {
            if (_fileSystem.File.Exists(vcsPath))
            {
                var entry = zipFile.CreateEntry(vcsPath, CompressionLevel.Fastest);
                using (var stream = entry.Open())
                {
                    using (var file = _fileSystem.File.Open(vcsPath, FileMode.Open))
                    {
                        file.CopyTo(stream);
                    }
                }

                //zipFile.CreateEntryFromFile(vcsPath, Path.GetFileName(vcsPath), CompressionLevel.Fastest);
            }
            else
            {
                throw new Exception($"File {vcsPath} does not exist");
            }
        }

        public string WriteRawToConsoleText(Stream zipStream)
        {
            return RawToConsoleText(zipStream);
        }
    }
}
