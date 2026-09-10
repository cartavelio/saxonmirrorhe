using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class UnicodeChar : Saxon.Hej.str.UnicodeString {
        private readonly int codepoint;
        public UnicodeChar(int codepoint)  {
            this.codepoint = codepoint;
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return new Saxon.Hej.z.IntSingletonIterator(codepoint);
        }
        public int getCodepoint() {
            return codepoint;
        }
        public override long length() {
            return 1;
        }
        public override int getWidth() {
            if ((codepoint<128)) {
                return 7;
            } else {
                if ((codepoint<256)) {
                    return 8;
                } else {
                    if ((codepoint<65536)) {
                        return 16;
                    } else {
                        return 24;
                    }
                }
            }
        }
        public override long indexOf(int codePoint, long from) {
            return ((((this.codepoint == codePoint) && (from<=0))) ? 0 : -1);
        }
        public override int codePointAt(long index) {
            if ((index == 0)) {
                return codepoint;
            } else {
                throw new System.IndexOutOfRangeException("Only valid index for a single-character string is zero");
            }
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            checkSubstringBounds(start, end);
            if (((start == 0) && (end == 1))) {
                return this;
            } else {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
        }
        public override long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            return ((((from == 0) && predicate(codepoint))) ? 0 : -1);
        }
        public override string ToString() {
            if ((codepoint<65536)) {
                return (""+(char)(codepoint));
            } else {
                return ((""+Saxon.Hej.serialize.charcode.UTF16CharacterSet.highSurrogate(codepoint))+Saxon.Hej.serialize.charcode.UTF16CharacterSet.lowSurrogate(codepoint));
            }
        }
        internal override void copy8bit(byte[] target, int offset) {
            target[offset] = (byte)(((codepoint&0xFF)));
        }
        internal override void copy16bit(char[] target, int offset) {
            target[offset] = (char)(codepoint);
        }
        internal override void copy24bit(byte[] target, int offset) {
            target[offset] = (byte)(((codepoint>>16)));
            target[(offset+1)] = (byte)(((codepoint>>8)));
            target[(offset+2)] = (byte)(((codepoint&unchecked((int)0xff))));
        }
        internal override void copy32bit(int[] target, int offset) {
            target[offset] = codepoint;
        }
    
    }
}