using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Xml;

namespace PowerBi.Converters
{
    public class XMLConverter : Converter
    {
        private Encoding _encoding;
        private bool _omitXmlDeclaration;

        public XMLConverter(Encoding encoding, IFileSystem fileSystem, bool omitXmlDeclaration) : base(fileSystem)
        {
            _encoding = encoding;
            _omitXmlDeclaration = omitXmlDeclaration;
        }

        public override Stream RawToVcs(Stream b)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(b);

            XmlWriterSettings ws = new XmlWriterSettings {Indent = true, CloseOutput = false};

            var outputStream = new MemoryStream();
            using (var writer = XmlWriter.Create(outputStream, ws))
            {
                doc.WriteContentTo(writer);

                writer.Flush();
                outputStream.Seek(0, SeekOrigin.Begin);
                return outputStream;
            }
        }

        public override Stream VcsToRaw(Stream b)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(b);

            XmlWriterSettings ws = new XmlWriterSettings {Indent = false, NewLineHandling = NewLineHandling.None, Encoding = _encoding, OmitXmlDeclaration = _omitXmlDeclaration };

            var outputStream = new MemoryStream();

            using (var writer = XmlWriter.Create(outputStream, ws))
            {
                doc.WriteContentTo(writer);

                writer.Flush();
                outputStream.Seek(0, SeekOrigin.Begin);
                return outputStream;
            }
        }

        public static Encoding GetFileEncoding(string srcFile)
        {
            // *** Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.Default;

            // *** Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];
            using (FileStream file = new FileStream(srcFile, FileMode.Open))
            {
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

        public override string RawToConsoleText(Stream b)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(b);

            using (var writer = new StringWriter())
            using (var textWriter = new XmlTextWriter(writer))
            {
                textWriter.Formatting = Formatting.Indented;
                doc.WriteTo(textWriter);
                return writer.ToString();
            }
        }
    }
}