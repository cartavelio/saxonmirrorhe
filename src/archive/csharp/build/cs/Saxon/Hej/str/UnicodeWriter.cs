using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public interface UnicodeWriter {
        void write(Saxon.Hej.str.UnicodeString chars);
        void writeAscii(byte[] content);
        void writeCodePoint(int codepoint);
        void writeRepeatedAscii(byte asciiChar, int count);
        void write(string chars);
        void close();
        void flush();
    
    }
}