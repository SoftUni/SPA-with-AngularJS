using System.IO;
using System.Text;

namespace Simple.Owin.Hosting.Trace
{
    internal class NullTextWriter : TextWriter
    {
        public override Encoding Encoding {
            get { return Encoding.UTF8; }
        }

        public override void Write(char value) { }

        public override void Write(char[] buffer, int index, int count) { }

        public override void Write(string value) { }
    }
}