using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class StringView : Saxon.Hej.str.UnicodeString {
        private readonly string baseString;
        private Saxon.Hej.str.UnicodeString baseUnicodeString;
        private StringView(string baseString)  {
            Saxon.Ejava.util.Objects.requireNonNull(baseString);
            this.baseString = baseString;
        }
        public static Saxon.Hej.str.UnicodeString of(string @base) {
            return new Saxon.Hej.str.StringView(@base);
        }
        public static Saxon.Hej.str.UnicodeString wrap(string @base) {
            return new Saxon.Hej.str.StringView(@base).tidy();
        }
        public static Saxon.Hej.str.UnicodeString tidy(string @base) {
            return new Saxon.Hej.str.StringView(@base).tidy();
        }
        public override Saxon.Hej.str.UnicodeString tidy() { lock(this) {
            if ((baseUnicodeString != null)) {
                return baseUnicodeString;
            } else {
                int uLength = Saxon.Hej.str.StringTool.getStringLength(baseString);
                if ((uLength == baseString.Length)) {
                    return baseUnicodeString = new Saxon.Hej.str.BMPString(baseString);
                } else {
                    byte[] triples = new byte[(uLength*3)];
                    for (int i = 0, j = 0; (i<baseString.Length); i++) {
                        char c = baseString[i];
                        if (Saxon.Hej.serialize.charcode.UTF16CharacterSet.isSurrogate(c)) {
                            int cp = Saxon.Hej.serialize.charcode.UTF16CharacterSet.combinePair(c, baseString[++i]);
                            triples[j++] = (byte)(((((cp>>16))&unchecked((int)0xff))));
                            triples[j++] = (byte)(((((cp>>8))&unchecked((int)0xff))));
                            triples[j++] = (byte)(((cp&unchecked((int)0xff))));
                        } else {
                            triples[j++] = 0;
                            triples[j++] = (byte)(((((c>>8))&unchecked((int)0xff))));
                            triples[j++] = (byte)(((c&unchecked((int)0xff))));
                        }
                    }
                    return baseUnicodeString = new Saxon.Hej.str.Twine24(triples);
                }
            }
        }}
        public override bool isEmpty() {
            return baseString.Length==0;
        }
        public override long estimatedLength() {
            return ((baseUnicodeString != null) ? baseUnicodeString.length() : baseString.Length);
        }
        public override long length() {
            return tidy().length();
        }
        public override int getWidth() {
            return tidy().getWidth();
        }
        public override long indexOf(int codePoint, long from) {
            return tidy().indexOf(codePoint, from);
        }
        public override int codePointAt(long index) {
            return tidy().codePointAt(index);
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            return tidy().substring(start, end);
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return ((baseUnicodeString != null) ? baseUnicodeString.codePoints() : Saxon.Hej.str.StringTool.codePoints(baseString));
        }
        public override Saxon.Hej.str.UnicodeString concat(Saxon.Hej.str.UnicodeString other) {
            if ((other is Saxon.Hej.str.StringView)) {
                Saxon.Hej.str.StringView sv = new Saxon.Hej.str.StringView((baseString+((Saxon.Hej.str.StringView)(other)).baseString));
                Saxon.Hej.str.UnicodeString us1 = baseUnicodeString;
                Saxon.Hej.str.UnicodeString us2 = ((Saxon.Hej.str.StringView)(other)).baseUnicodeString;
                if (((us1 != null) && (us2 != null))) {
                    baseUnicodeString = us1.concat(us2);
                }
                return sv;
            } else {
                Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
                return ((/*1430*/Saxon.Hej.str.UnicodeBuilder)((/*1430*/Saxon.Hej.str.UnicodeBuilder)ub.accept(this)).accept(other)).toUnicodeString();
            }
        }
        public override int compareTo(Saxon.Hej.str.UnicodeString other) {
            if ((((baseUnicodeString != null) && (other is Saxon.Hej.str.StringView)) && (((Saxon.Hej.str.StringView)(other)).baseUnicodeString != null))) {
                return System.String.Compare(baseString, ((Saxon.Hej.str.StringView)(other)).baseString, System.StringComparison.Ordinal);
            } else {
                return base.compareTo(other);
            }
        }
        public override bool Equals(System.Object obj) {
            if ((obj is Saxon.Hej.str.StringView)) {
                return this.baseString.Equals(((Saxon.Hej.str.StringView)(obj)).baseString);
            } else {
                if ((obj is Saxon.Hej.str.BMPString)) {
                    return this.baseString.Equals(obj.ToString());
                } else {
                    return base.Equals(obj);
                }
            }
        }
        public override int GetHashCode() {
            return (baseUnicodeString != null) ? baseUnicodeString.GetHashCode() : base.GetHashCode();
        }
        public override Saxon.Hej.str.UnicodeString economize() {
            return tidy();
        }
        public override string ToString() {
            return baseString;
        }
        internal override void copy8bit(byte[] target, int offset) {
            tidy().copy8bit(target, offset);
        }
        internal override void copy16bit(char[] target, int offset) {
            tidy().copy16bit(target, offset);
        }
        internal override void copy24bit(byte[] target, int offset) {
            tidy().copy24bit(target, offset);
        }
    
    }
}