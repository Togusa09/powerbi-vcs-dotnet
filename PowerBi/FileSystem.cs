using System.IO;
using System.IO.Compression;

namespace PowerBi
{public interface IFileSystem
    {
        Stream CreateNewFile(string path);
        Stream OpenFile(string path);
        ZipArchive OpenZipFile(string path);
        bool DirectoryExists(string path);
    }

    public class FileSystem : IFileSystem
    {
        public Stream CreateNewFile(string path)
        {
            return new FileStream(path, FileMode.CreateNew);
        }

        public Stream OpenFile(string path)
        {
            return new FileStream(path, FileMode.Open);
        }

        public ZipArchive OpenZipFile(string path)
        {
            return ZipFile.Open(path, ZipArchiveMode.Read);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
    }
}