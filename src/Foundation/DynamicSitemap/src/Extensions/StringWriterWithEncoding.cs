using System.IO;
using System.Text;

namespace Sitecore.SharedSource.DynamicSitemap.Extensions
{
    /// <summary>
    /// StringWriter extended with availability of setting up encoding
    /// </summary>
    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }
}
