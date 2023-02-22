using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class IndentWhitespace : Saxon.Hej.str.WhitespaceString {
        private readonly int newlines;
        private readonly int spaces;
        private IndentWhitespace(int newlines, int spaces)  {
            this.newlines = newlines;
            this.spaces = spaces;
        }
        public static Saxon.Hej.str.IndentWhitespace of(int newlines, int spaces) {
            return new Saxon.Hej.str.IndentWhitespace(newlines, spaces);
        }
        public override Saxon.Hej.str.UnicodeString uncompress() {
            byte[] bytes = new byte[(newlines+spaces)];
            Saxon.Ejava.util.Arrays.fill(bytes, 0, newlines, (byte)(0x0a));
            Saxon.Ejava.util.Arrays.fill(bytes, newlines, (newlines+spaces), (byte)(0x20));
            return new Saxon.Hej.str.Twine8(bytes);
        }
        public override long length() {
            return (newlines+spaces);
        }
        public override int length32() {
            return (newlines+spaces);
        }
        public int getNewlines() {
            return newlines;
        }
        public int getSpaces() {
            return spaces;
        }
        public override int codePointAt(long index) {
            if (((index>=0) && (index<newlines))) {
                return 0x0A;
            } else {
                if ((index<=(newlines+spaces))) {
                    return 0x20;
                } else {
                    throw new System.IndexOutOfRangeException();
                }
            }
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return new Saxon.Hej.z.ConcatenatingIntIterator(new Saxon.Hej.z.IntRepeatIterator(10, newlines), () => new Saxon.Hej.z.IntRepeatIterator(32, spaces));
        }
        public override string ToString() {
            char[] chars = new char[(newlines+spaces)];
            Saxon.Ejava.util.Arrays.fill(chars, 0, newlines, '\n');
            Saxon.Ejava.util.Arrays.fill(chars, newlines, (newlines+spaces), ' ');
            return new string(chars);
        }
        public override void write(Saxon.Hej.str.UnicodeWriter writer) {
            if ((newlines>0)) {
                writer.writeRepeatedAscii((byte)(0x0A), newlines);
            }
            if ((spaces>0)) {
                writer.writeRepeatedAscii((byte)(0x20), spaces);
            }
        }
        public override void writeEscape(bool[] specialChars, Saxon.Hej.str.UnicodeWriter writer) {
            if (specialChars[0x0A]) {
                for (int i = 0; (i<newlines); i++) {
                    writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_NL);
                }
            } else {
                writer.writeRepeatedAscii((byte)(0x0A), newlines);
            }
            writer.writeRepeatedAscii((byte)(0x20), spaces);
        }
    
    }
}