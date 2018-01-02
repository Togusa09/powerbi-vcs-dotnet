using System.IO;
using System.IO.Compression;

namespace PowerBi
{
    //public interface IFileSystem
    //{
    //    Stream CreateNewFile(string path);
    //    Stream OpenFile(string path);
    //    ZipArchive OpenZipFile(string path);
    //    bool DirectoryExists(string path);
    //    void CreateDirectory(string path);
    //    void DeleteFile(string path);
    //    bool FileExists(string path);
    //}

    //public class FileSystem : IFileSystem
    //{
    //    public Stream CreateNewFile(string path)
    //    {
    //        return new FileStream(path, FileMode.CreateNew);
    //    }

    //    public Stream OpenFile(string path)
    //    {
    //        return new FileStream(path, FileMode.Open);
    //    }

    //    public ZipArchive OpenZipFile(string path)
    //    {
    //        return ZipFile.Open(path, ZipArchiveMode.Read);
    //    }

    //    public bool DirectoryExists(string path)
    //    {
    //        return Directory.Exists(path);
    //    }

    //    public void CreateDirectory(string path)
    //    {
    //        Directory.CreateDirectory(path);
    //    }

    //    public void DeleteFile(string path)
    //    {
    //        File.Delete(path);
    //    }

    //    public bool FileExists(string path)
    //    {
    //        return File.Exists(path);
    //    }
    //}
}