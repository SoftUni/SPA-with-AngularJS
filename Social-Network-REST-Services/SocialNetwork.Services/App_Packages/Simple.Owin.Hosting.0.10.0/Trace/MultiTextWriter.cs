using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Simple.Owin.Hosting.Trace
{
    internal class MultiTextWriter : TextWriter
    {
        private readonly List<TextWriter> _writers = new List<TextWriter>();

        public override Encoding Encoding {
            get { throw new NotImplementedException(); }
        }

        public void Add(TextWriter writer) {
            _writers.Add(writer);
        }

        public override void Close() {
            foreach (var writer in _writers) {
                writer.Close();
            }
            base.Close();
        }

        public override void Flush() {
            foreach (var writer in _writers) {
                writer.Flush();
            }
        }

        public override void Write(char value) {
            foreach (var writer in _writers) {
                writer.Write(value);
            }
        }

        public override void Write(char[] buffer, int index, int count) {
            foreach (var writer in _writers) {
                writer.Write(buffer, index, count);
            }
        }

        public override void Write(string value) {
            foreach (var writer in _writers) {
                writer.Write(value);
            }
        }

        protected override void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }
            foreach (var writer in _writers) {
                writer.Dispose();
            }
        }
    }
}