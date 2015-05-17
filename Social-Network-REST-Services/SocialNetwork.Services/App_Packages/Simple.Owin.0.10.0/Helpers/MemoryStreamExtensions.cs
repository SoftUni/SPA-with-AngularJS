using System;
using System.IO;

namespace Simple.Owin.Helpers
{
    internal static class MemoryStreamExtensions
    {
        public static void Reset(this MemoryStream stream) {
            if (stream == null) {
                return;
            }
            byte[] buffer = stream.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            stream.Position = 0;
            stream.SetLength(0);
        }
    }
}