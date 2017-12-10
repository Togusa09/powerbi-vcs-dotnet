using System.IO;
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
            Directory.CreateDirectory(Path.GetDirectoryName(vcsPath));

            using (var file = _fileSystem.CreateNewFile(vcsPath))
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
            if (File.Exists(vcsPath))
            {
                zipFile.CreateEntryFromFile(vcsPath, Path.GetFileName(vcsPath), CompressionLevel.NoCompression);
            }
        }

        public string WriteRawToConsoleText(Stream zipStream)
        {
            return RawToConsoleText(zipStream);
        }
    }
}
