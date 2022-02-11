using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public sealed class LargeTextBuffer {
        private const int BITS = 16;
        private const int SEGLEN = (1<<BITS);
        private const int MASK = (SEGLEN-1);
        private System.Collections.Generic.IList<Saxon.Hej.str.LargeTextBuffer.Segment> completeSegments;
        private Saxon.Hej.str.LargeTextBuffer.Segment lastSegment;
        private int lastSegmentLength;
        private interface Segment {
            int getWidth();
            Saxon.Hej.str.LargeTextBuffer.Segment stretch(int oldLength, int newLength, int newWidth);
            Saxon.Hej.str.UnicodeString asUnicodeString();
            Saxon.Hej.str.UnicodeString substring(int start, int end);
        
        }
        private class Segment8 : Saxon.Hej.str.LargeTextBuffer.Segment {
            public byte[] bytes;
            public Segment8(byte[] bytes)  {
                this.bytes = bytes;
            }
            public int getWidth() {
                return 8;
            }
            public Saxon.Hej.str.LargeTextBuffer.Segment stretch(int oldLength, int newLength, int newWidth) {
                if ((newWidth<=8)) {
                    if ((newLength>bytes.Length)) {
                        bytes = Saxon.Ejava.util.Arrays.copyOf(bytes, System.Math.Max(newLength, (oldLength*2)));
                    }
                    return this;
                } else {
                    if ((newWidth == 16)) {
                        char[] array16 = new char[newLength];
                        Saxon.Hej.str.StringTool.copy8to16(bytes, 0, array16, 0, oldLength);
                        return new Saxon.Hej.str.LargeTextBuffer.Segment16(array16);
                    } else {
                        byte[] array24 = new byte[(newLength*3)];
                        Saxon.Hej.str.StringTool.copy8to24(bytes, 0, array24, 0, oldLength);
                        return new Saxon.Hej.str.LargeTextBuffer.Segment24(array24);
                    }
                }
            }
            public Saxon.Hej.str.UnicodeString asUnicodeString() {
                return new Saxon.Hej.str.Twine8(bytes);
            }
            public Saxon.Hej.str.UnicodeString substring(int start, int end) {
                return new Saxon.Hej.str.Slice8(bytes, start, end);
            }
        
        }
        private class Segment16 : Saxon.Hej.str.LargeTextBuffer.Segment {
            public char[] chars;
            public Segment16(char[] chars)  {
                this.chars = chars;
            }
            public int getWidth() {
                return 16;
            }
            public Saxon.Hej.str.LargeTextBuffer.Segment stretch(int oldLength, int newLength, int newWidth) {
                if ((newWidth<=16)) {
                    if ((newLength>chars.Length)) {
                        chars = Saxon.Ejava.util.Arrays.copyOf(chars, System.Math.Max(newLength, (oldLength*2)));
                    }
                    return this;
                } else {
                    byte[] array24 = new byte[(newLength*3)];
                    Saxon.Hej.str.StringTool.copy16to24(chars, 0, array24, 0, oldLength);
                    return new Saxon.Hej.str.LargeTextBuffer.Segment24(array24);
                }
            }
            public Saxon.Hej.str.UnicodeString asUnicodeString() {
                return new Saxon.Hej.str.Twine16(chars);
            }
            public Saxon.Hej.str.UnicodeString substring(int start, int end) {
                return new Saxon.Hej.str.Slice16(chars, start, end);
            }
        
        }
        private class Segment24 : Saxon.Hej.str.LargeTextBuffer.Segment {
            public byte[] bytes;
            public Segment24(byte[] bytes)  {
                this.bytes = bytes;
            }
            public int getWidth() {
                return 24;
            }
            public Saxon.Hej.str.LargeTextBuffer.Segment stretch(int oldLength, int newLength, int newWidth) {
                if (((newLength*3)>bytes.Length)) {
                    bytes = Saxon.Ejava.util.Arrays.copyOf(bytes, System.Math.Max((newLength*3), (oldLength*6)));
                }
                return this;
            }
            public Saxon.Hej.str.UnicodeString substring(int start, int length) {
                return new Saxon.Hej.str.Slice24(bytes, start, length);
            }
            public Saxon.Hej.str.UnicodeString asUnicodeString() {
                return new Saxon.Hej.str.Twine24(bytes);
            }
        
        }
        public LargeTextBuffer()  {
            completeSegments = new System.Collections.Generic.List<Saxon.Hej.str.LargeTextBuffer.Segment>(4);
            lastSegment = new Saxon.Hej.str.LargeTextBuffer.Segment8(new byte[0]);
            lastSegmentLength = 0;
        }
        private void addSegment(Saxon.Hej.str.LargeTextBuffer.Segment segment) {
            completeSegments.Add(segment);
        }
        private Saxon.Hej.str.LargeTextBuffer.Segment getSegment(int n) {
            if ((n == completeSegments.Count)) {
                return lastSegment;
            } else {
                return completeSegments[n];
            }
        }
        public void appendUnicodeString(Saxon.Hej.str.UnicodeString chars) {
            int spaceAvailableInLastSegment = (SEGLEN-lastSegmentLength);
            long charsSupplied = chars.length();
            if ((charsSupplied<spaceAvailableInLastSegment)) {
                extendLastSegment(chars);
            } else {
                long start = 0;
                extendLastSegment(chars.substring(0, spaceAvailableInLastSegment));
                charsSupplied -= spaceAvailableInLastSegment;
                start += spaceAvailableInLastSegment;
                while ((charsSupplied>SEGLEN)) {
                    extendLastSegment(chars.substring(start, (start+SEGLEN)));
                    charsSupplied -= SEGLEN;
                    start += SEGLEN;
                }
                if ((charsSupplied>0)) {
                    extendLastSegment(chars.substring(start, (start+charsSupplied)));
                }
            }
        }
        private void extendLastSegment(Saxon.Hej.str.UnicodeString chars) {
            lastSegment = lastSegment.stretch(lastSegmentLength, (lastSegmentLength+chars.length32()), chars.getWidth());
            if ((lastSegment is Saxon.Hej.str.LargeTextBuffer.Segment8)) {
                chars.copy8bit(((Saxon.Hej.str.LargeTextBuffer.Segment8)(lastSegment)).bytes, lastSegmentLength);
            } else {
                if ((lastSegment is Saxon.Hej.str.LargeTextBuffer.Segment16)) {
                    chars.copy16bit(((Saxon.Hej.str.LargeTextBuffer.Segment16)(lastSegment)).chars, lastSegmentLength);
                } else {
                    System.Diagnostics.Debug.Assert((lastSegment is Saxon.Hej.str.LargeTextBuffer.Segment24));
                    chars.copy24bit(((Saxon.Hej.str.LargeTextBuffer.Segment24)(lastSegment)).bytes, (lastSegmentLength*3));
                }
            }
            lastSegmentLength += chars.length32();
            if ((lastSegmentLength == SEGLEN)) {
                addSegment(lastSegment);
                lastSegment = new Saxon.Hej.str.LargeTextBuffer.Segment8(new byte[1024]);
                lastSegmentLength = 0;
            }
        }
        private void showSegmentLengths() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            foreach (Saxon.Hej.str.LargeTextBuffer.Segment s in completeSegments) {
                sb.append(s.asUnicodeString().length()).append(", ");
            }
            sb.append(lastSegmentLength);
            System.Console.Error.WriteLine(sb);
        }
        public Saxon.Hej.str.UnicodeString substring(int start, int end) {
            int firstSeg = (start>>BITS);
            int lastSeg = (((end-1))>>BITS);
            int lastCP = (end&MASK);
            if ((lastCP == 0)) {
                lastCP = SEGLEN;
            }
            if ((firstSeg == lastSeg)) {
                try {
                    Saxon.Hej.str.LargeTextBuffer.Segment seg = getSegment(firstSeg);
                    return seg.substring((start&MASK), lastCP);
                } catch (System.IndexOutOfRangeException e) {
                    System.Console.Error.WriteLine(e);
                    throw;
                }
            } else {
                Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
                int segNr = firstSeg;
                ub.accept(getSegment(segNr++).substring((start&MASK), SEGLEN));
                while ((segNr<lastSeg)) {
                    ub.accept(getSegment(segNr++).asUnicodeString());
                }
                ub.accept(getSegment(lastSeg).substring(0, lastCP));
                return ub.toUnicodeString();
            }
        }
        public void close() {
            if (((lastSegment != null) && (lastSegmentLength>0))) {
                addSegment(lastSegment);
            }
            lastSegment = null;
        }
        public int length() {
            return ((completeSegments.Count*SEGLEN)+(((lastSegment == null) ? 0 : lastSegmentLength)));
        }
        public void setLength(int newLength) {
            if ((newLength<length())) {
                int segCount = completeSegments.Count;
                if ((newLength<=(segCount*SEGLEN))) {
                    lastSegment = completeSegments[(segCount-1)];
                    Saxon.Impl.Helpers.ListUtils.Remove(completeSegments, (segCount-1));
                }
                lastSegmentLength = (newLength&MASK);
            }
        }
    
    }
}