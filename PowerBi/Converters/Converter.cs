using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;

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

        public virtual void WriteVcsToRaw(string vcsPath, Stream zipEntryStream)
        {
            
                using (var file = _fileSystem.File.Open(vcsPath, FileMode.Open))
                {
                    using (var convertedStream = VcsToRaw(file))
                    {
                        convertedStream.Seek(0, SeekOrigin.Begin);
                        convertedStream.CopyTo(zipEntryStream);
                        zipEntryStream.Flush();
                    }
                }
            
        }

        public string WriteRawToConsoleText(Stream zipStream)
        {
            return RawToConsoleText(zipStream);
        }
    }
}
