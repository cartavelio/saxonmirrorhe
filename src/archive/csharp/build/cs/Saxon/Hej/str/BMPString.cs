using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class BMPString : Saxon.Hej.str.UnicodeString {
        private readonly string baseString;
        protected internal BMPString(string baseString)  {
            this.baseString = baseString;
        }
        public static Saxon.Hej.str.UnicodeString of(string @base) {
            if (Saxon.Hej.Configuration.isAssertionsEnabled()) {
                for (int i = 0; (i<@base.Length); i++) {
                    System.Diagnostics.Debug.Assert(!(Saxon.Hej.serialize.charcode.UTF16CharacterSet.isSurrogate(@base[i])));
                }
            }
            return new Saxon.Hej.str.BMPString(@base);
        }
        public override long length() {
            return baseString.Length;
        }
        public override bool isEmpty() {
            return baseString.Length==0;
        }
        public override int getWidth() {
            return 16;
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return Saxon.Hej.str.StringTool.codePoints(baseString);
        }
        public override long indexOf(int codePoint) {
            if ((codePoint>65535)) {
                return -1;
            }
            return baseString.IndexOf((char)(codePoint));
        }
        public override long indexOf(int codePoint, long from) {
            int from32 = requireInt(from);
            if ((codePoint>65535)) {
                return -1;
            }
            return baseString.IndexOf((char)(codePoint), from32);
        }
        public override long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            for (int i = requireInt(from); (i<baseString.Length); i++) {
                if (predicate(baseString[i])) {
                    return i;
                }
            }
            return -1;
        }
        public override int codePointAt(long index) {
            return baseString[Saxon.Hej.str.StringTool.requireInt(index)];
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            int start32 = Saxon.Hej.str.StringTool.requireInt(start);
            int end32 = Saxon.Hej.str.StringTool.requireInt(end);
            return new Saxon.Hej.str.BMPString(baseString.Substring(start32, end32 - start32));
        }
        public override Saxon.Hej.str.UnicodeString concat(Saxon.Hej.str.UnicodeString other) {
            if ((other is Saxon.Hej.str.BMPString)) {
                return new Saxon.Hej.str.BMPString((baseString+((Saxon.Hej.str.BMPString)(other)).baseString));
            } else {
                Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
                return ((/*1430*/Saxon.Hej.str.UnicodeBuilder)((/*1430*/Saxon.Hej.str.UnicodeBuilder)ub.accept(this)).accept(other)).toUnicodeString();
            }
        }
        public override int compareTo(Saxon.Hej.str.UnicodeString other) {
            if ((other is Saxon.Hej.str.BMPString)) {
                return System.String.Compare(baseString, ((Saxon.Hej.str.BMPString)(other)).baseString, System.StringComparison.Ordinal);
            } else {
                return base.compareTo(other);
            }
        }
        public override bool Equals(System.Object obj) {
            if ((obj is Saxon.Hej.str.BMPString)) {
                return this.baseString.Equals(((Saxon.Hej.str.BMPString)(obj)).baseString);
            } else {
                return base.Equals(obj);
            }
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public override string ToString() {
            return baseString;
        }
        internal override void copy16bit(char[] target, int offset) {
            Saxon.Impl.Helpers.StringUtils.stringToCharArray(baseString, 0, baseString.Length, target, offset);
        }
        internal override void copy24bit(byte[] target, int offset) {
            char[] chars = baseString.ToCharArray();
            for (int i = 0, j = offset; (i<chars.Length); ) {
                char c = chars[i++];
                target[j++] = 0;
                target[j++] = (byte)(((c>>8)));
                target[j++] = (byte)(((c&unchecked((int)0xff))));
            }
        }
    
    }
}