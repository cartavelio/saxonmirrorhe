using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public abstract class WhitespaceString : Saxon.Hej.str.UnicodeString {
        public abstract Saxon.Hej.str.UnicodeString uncompress();
        public override int getWidth() {
            return 7;
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            return uncompress().substring(start, end);
        }
        public override long indexOf(int codePoint, long from) {
            return uncompress().indexOf(codePoint, from);
        }
        public override string ToString() {
            return uncompress().ToString();
        }
        public abstract void write(Saxon.Hej.str.UnicodeWriter writer);
        internal override void copy8bit(byte[] target, int offset) {
            uncompress().copy8bit(target, offset);
        }
        internal override void copy16bit(char[] target, int offset) {
            uncompress().copy16bit(target, offset);
        }
        internal override void copy24bit(byte[] target, int offset) {
            uncompress().copy24bit(target, offset);
        }
        public abstract void writeEscape(bool[] specialChars, Saxon.Hej.str.UnicodeWriter writer);
    
    }
}