using System.IO;
using System.Text;
using System.Xml;

namespace PowerBi
{
    public class XMLConverter : Converter
    {
        private Encoding _encoding;

        public XMLConverter(Encoding encoding, IFileSystem fileSystem) : base(fileSystem)
        {
            _encoding = encoding;
        }

        public override Stream RawToVcs(Stream b)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(b);

            XmlWriterSettings ws = new XmlWriterSettings {Indent = true, };

            var outputStream = new MemoryStream();
            var writer = XmlWriter.Create(outputStream, ws);
            
            doc.WriteContentTo(writer);

            writer.Flush();
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        public override Stream VcsToRaw(Stream b)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(b);

            XmlWriterSettings ws = new XmlWriterSettings { Indent = false, };

            var outputStream = new MemoryStream();
            var writer = XmlWriter.Create(outputStream, ws);

            doc.WriteContentTo(writer);

            writer.Flush();
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        public static Encoding GetFileEncoding(string srcFile)
        {
            // *** Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.Default;

            // *** Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];
            FileStream file = new FileStream(srcFile, FileMode.Open);
            file.Read(buffer, 0, 5);
            file.Close();

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.Unicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                enc = Encoding.UTF32;
            else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                enc = Encoding.UTF7;
            else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                // 1201 unicodeFFFE Unicode (Big-Endian)
                enc = Encoding.GetEncoding(1201);
            else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                // 1200 utf-16 Unicode
                enc = Encoding.GetEncoding(1200);


            return enc;
        }
    }
}