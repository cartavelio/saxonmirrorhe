using System;
using System.Collections.Generic;
using System.IO;

namespace com.saxonica.glue {
    public class DotNetInputStream : java.io.InputStream {
        internal Stream stream;
        internal long currentOffset;
        internal long markedOffset = 0;
        public DotNetInputStream(Stream stream)  {
            this.stream = stream;
        }
        public Stream getUnderlyingStream() {
            return stream;
        }
        public int read() {
            int i = stream.ReadByte();
            if ((i!=-1)) {
                currentOffset++;
                return i;
            } else {
                return -1;
            }
        }
        public int read(byte[] b, int off, int len) {
            int i = stream.Read(b, off, len);
            if ((i>0)) {
                currentOffset += i;
                return i;
            } else {
                return -1;
            }
        }
        public bool markSupported() {
            return stream.CanSeek;
        }
        public void mark(int readlimit) { lock(this) {
            markedOffset = currentOffset;
        }}
        public void reset() { lock(this) {
            currentOffset = markedOffset;
            stream.Seek(markedOffset, SeekOrigin.wrap(SeekOrigin.Begin));
        }}
        public void close() {
            stream.Close();
        }
    }
}