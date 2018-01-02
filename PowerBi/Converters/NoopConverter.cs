using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace PowerBi.Converters
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

        public override string RawToConsoleText(Stream b)
        {
            SHA256 mySHA256 = SHA256Managed.Create();


            StringBuilder sBuilder = new StringBuilder();
            var hash = mySHA256.ComputeHash(b);

            for (int i = 0; i < hash.Length; i++)
            {
                sBuilder.Append(hash[i].ToString("x2"));
            }

            return "File hash: " + sBuilder;
        }

        public NoopConverter(IFileSystem fileSystem) : base(fileSystem)
        {
        }
    }
}