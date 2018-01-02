using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using PowerBi.Converters;
using Shouldly;
using Xunit;

namespace PowerBi.Tests
{
    //public class StubFileSystem : IFileSystem
    //{
    //    private Dictionary<string, Stream> _files = new Dictionary<string, Stream>();

    //    public Stream CreateNewFile(string path)
    //    {
    //        var memoryStream = new MemoryStream();
    //        _files.Add(path, memoryStream);
    //        return memoryStream;
    //    }

    //    public List<string> ListFiles()
    //    {
    //        return _files.Select(x => x.Key).ToList();
    //    }

    //    public Stream OpenFile(string path)
    //    {
    //        CheckFileExists(path);
    //        var memoryStream = _files[path];
    //        memoryStream.Seek(0, SeekOrigin.Begin);
    //        return memoryStream;
    //    }

    //    private void CheckFileExists(string path)
    //    {
    //        if (!_files.ContainsKey(path))
    //        {
    //            throw new Exception("File not found");
    //        }
    //    }

    //    public ZipArchive OpenZipFile(string path)
    //    {
    //        CheckFileExists(path);

    //        var zip = new ZipArchive(_files[path]);
    //        return zip;
    //    }

    //    public bool DirectoryExists(string path)
    //    {
    //        if (Path.HasExtension(path))
    //        {
    //            return false;
    //        }

    //        return _files.Any(x => x.Key == path && x.Value == null);
    //    }

    //    public void CreateDirectory(string path)
    //    {
    //        if (Path.HasExtension(path))
    //        {
    //            throw new Exception("This is a file path, not a directory");
    //        }

    //        _files.Add(path,  null);
    //    }

    //    public void DeleteFile(string path)
    //    {
    //        _files.Remove(path);
    //    }

    //    public bool FileExists(string path)
    //    {
    //        return _files.Any(x => x.Key == path && x.Value != null);
    //    }


    //    public void DeleteDirectory(string path)
    //    {
    //        if (!DirectoryExists(path))
    //        {
    //            throw new Exception("Directory not found");
    //        }

    //        _files.Remove(path);
    //    }

    //    public void AddEmbeddedFile(string embeddedFileName, string registerFileName)
    //    {

    //        var assembly = Assembly.GetExecutingAssembly();
    //        var resourceName = "PowerBi.Tests.Files." + embeddedFileName;

    //        using (var stream = assembly.GetManifestResourceStream(resourceName))
    //        {
    //            var memoryStream = new MemoryStream();
    //            stream.CopyTo(memoryStream);
    //            _files.Add(registerFileName, memoryStream);
    //            memoryStream.Seek(0, SeekOrigin.Begin);
    //        }
    //    }
    //}

    public class PbiVcsTests
    {
        [Theory]
        [InlineData("DataModelSchema", typeof(JsonConverter))]
        [InlineData("DiagramState", typeof(JsonConverter))]
        [InlineData("Report/Layout", typeof(JsonConverter))]
        [InlineData("Report/LinguisticSchema", typeof(XMLConverter))]
        [InlineData("[Content_Types].xml", typeof(XMLConverter))]
        [InlineData("SecurityBindings", typeof(NoopConverter))]
        [InlineData("Settings", typeof(NoopConverter))]
        [InlineData("Version", typeof(NoopConverter))]
        [InlineData("Report/StaticResources/", typeof(NoopConverter))]
        [InlineData("DataMashup", typeof(DataMashupConverter))]
        [InlineData("Metadata", typeof(MetadataConverter))]
        [InlineData("test.json", typeof(JsonConverter))]
        public void FindConverterReturnsTheExpectedConverter(string path, Type converterType)
        {
            var fileSystem = new MockFileSystem();
            var vcs = new PowerBiExtractor(fileSystem);
            vcs.FindConverter(path).GetType().ShouldBe(converterType);
        }
    }
}
