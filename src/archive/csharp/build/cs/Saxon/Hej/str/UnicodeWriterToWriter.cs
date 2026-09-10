using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class UnicodeWriterToWriter : Saxon.Hej.str.UnicodeWriter {
        private readonly System.IO.TextWriter writer;
        public UnicodeWriterToWriter(System.IO.TextWriter writer)  {
            this.writer = writer;
        }
        public void write(Saxon.Hej.str.UnicodeString chars) {
            writer.Write(chars.ToString());
        }
        public void writeCodePoint(int codepoint) {
            if ((codepoint>65535)) {
                writer.Write(Saxon.Hej.serialize.charcode.UTF16CharacterSet.highSurrogate(codepoint));
                writer.Write(Saxon.Hej.serialize.charcode.UTF16CharacterSet.lowSurrogate(codepoint));
            } else {
                writer.Write((char)(codepoint));
            }
        }
        public void write(string chars) {
            writer.Write(chars);
        }
        public void writeAscii(byte[] content) {
            char[] chars = new char[content.Length];
            Saxon.Hej.str.StringTool.copy8to16(content, 0, chars, 0, content.Length);
            writer.Write(chars);
        }
        public void close() {
            writer.Close();
        }
        public void flush() {
            writer.Flush();
        }
    
        /* Expanding default method writeRepeatedAscii from interface UnicodeWriter */
        public virtual void writeRepeatedAscii(byte asciiChar, int count) {
            System.Diagnostics.Debug.Assert((asciiChar>=0));
            byte[] array = new byte[count];
            Saxon.Ejava.util.Arrays.fill(array, asciiChar);
            writeAscii(array);
        }

    }
}