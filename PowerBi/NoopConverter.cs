using System.IO;

namespace PowerBi
{
    public class NoopConverter : Converter
    {
        public override Stream RawToVcs(Stream b)
        {
            return b;
        }

        public override Stream VcsToRaw(Stream b)
        {
            return b;
        }

        public NoopConverter(IFileSystem fileSystem) : base(fileSystem)
        {
        }
    }
}