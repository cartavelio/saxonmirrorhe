using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class ZenoString : Saxon.Hej.str.UnicodeString {
        private System.Collections.Generic.IList<Saxon.Hej.str.UnicodeString> segments = new System.Collections.Generic.List<Saxon.Hej.str.UnicodeString>();
        private System.Collections.Generic.IList<System.Int64> offsets = new System.Collections.Generic.List<System.Int64>();
        private ZenoString()  {
        }
        private ZenoString(Saxon.Hej.str.UnicodeString content)  {
            segments.Add(content);
            offsets.Add(0L);
        }
        public static Saxon.Hej.str.ZenoString of(Saxon.Hej.str.UnicodeString content) {
            if ((content is Saxon.Hej.str.ZenoString)) {
                return (Saxon.Hej.str.ZenoString)(content);
            } else {
                if (content.isEmpty()) {
                    return new Saxon.Hej.str.ZenoString();
                } else {
                    return new Saxon.Hej.str.ZenoString(content);
                }
            }
        }
        private int segmentForOffset(long offset) {
            if ((segments.Count == 0)) {
                throw new System.IndexOutOfRangeException("ZenoString is empty");
            }
            int result = binarySearch(offset, 0, (offsets.Count-1));
            if ((result<0)) {
                throw new System.IndexOutOfRangeException(((("Index "+offset)+" out of range 0-")+((length()-1))));
            }
            return result;
        }
        private int binarySearch(long offset, int start, int end) {
            if ((start == end)) {
                long s = offsets[start];
                long e = (s+segments[start].length());
                if (((s<=offset) && (e>offset))) {
                    return start;
                } else {
                    return -1;
                }
            } else {
                int mid = (start+((((end-start)+1))/2));
                if ((offsets[mid]>offset)) {
                    return binarySearch(offset, start, (mid-1));
                } else {
                    return binarySearch(offset, mid, end);
                }
            }
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return new Saxon.Impl.Overrides.ZenoStringCodepoints(segments);
        }
        public override long length() {
            int i = (segments.Count-1);
            return ((i<0) ? 0L : (offsets[i]+segments[i].length()));
        }
        public override bool isEmpty() {
            return segments.Count==0;
        }
        public override int getWidth() {
            int maxWidth = 7;
            foreach (Saxon.Hej.str.UnicodeString entry in segments) {
                int width = entry.getWidth();
                if ((width == 24)) {
                    return 24;
                } else {
                    maxWidth = System.Math.Max(maxWidth, width);
                }
            }
            return maxWidth;
        }
        public override long indexOf(int codePoint, long from) {
            from = System.Math.Max(from, 0);
            if ((from>=length())) {
                return -(1L);
            }
            int first = segmentForOffset(from);
            for (int i = first; (i<segments.Count); i++) {
                Saxon.Hej.str.UnicodeString segment = segments[i];
                long offset = offsets[i];
                long pos = segment.indexOf(codePoint, ((i == first) ? (from-offset) : 0));
                if ((pos>=0)) {
                    return (pos+offset);
                }
            }
            return -1;
        }
        public override int codePointAt(long index) {
            int entry = segmentForOffset(index);
            Saxon.Hej.str.UnicodeString segment = segments[entry];
            return segment.codePointAt((index-offsets[entry]));
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            checkSubstringBounds(start, end);
            if ((start == end)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                if (((start+1) == end)) {
                    return new Saxon.Hej.str.UnicodeChar(codePointAt(start));
                }
            }
            int first = segmentForOffset(start);
            int last = segmentForOffset((end-1));
            if ((first == last)) {
                Saxon.Hej.str.UnicodeString segment = segments[first];
                long offset = offsets[first];
                return segment.substring((start-offset), (end-offset));
            } else {
                Saxon.Hej.str.ZenoString z = Saxon.Hej.str.ZenoString.of(segments[first].substring((start-offsets[first])));
                for (int i = (first+1); (i<last); i++) {
                    z = z.concat(segments[i]);
                }
                return z.concat(segments[last].substring(0, (end-offsets[last])));
            }
        }
        public override Saxon.Hej.str.ZenoString concat(Saxon.Hej.str.UnicodeString other) {
            if (isEmpty()) {
                return ((other is Saxon.Hej.str.ZenoString) ? (Saxon.Hej.str.ZenoString)(other) : new Saxon.Hej.str.ZenoString(other));
            } else {
                if (other.isEmpty()) {
                    return this;
                }
            }
            if ((other is Saxon.Hej.str.ZenoString)) {
                Saxon.Hej.str.ZenoString z = new Saxon.Hej.str.ZenoString();
                z.segments = new System.Collections.Generic.List<Saxon.Hej.str.UnicodeString>(segments);
                Saxon.Impl.Helpers.ListUtils.AddAll(z.segments, ((Saxon.Hej.str.ZenoString)(other)).segments);
                z.offsets = new System.Collections.Generic.List<System.Int64>(offsets);
                long len = length();
                foreach (long offset in ((Saxon.Hej.str.ZenoString)(other)).offsets) {
                    z.offsets.Add((offset+len));
                }
                return ((((len<32) || (other.length()<32)) ? z.consolidate0() : z));
            } else {
                Saxon.Hej.str.ZenoString z = new Saxon.Hej.str.ZenoString();
                z.segments = new System.Collections.Generic.List<Saxon.Hej.str.UnicodeString>(segments);
                z.offsets = new System.Collections.Generic.List<System.Int64>(offsets);
                z.segments.Add(other);
                z.offsets.Add(length());
                return z.consolidate0();
            }
        }
        internal override void copy8bit(byte[] target, int offset) {
            foreach (Saxon.Hej.str.UnicodeString us in segments) {
                us.copy8bit(target, offset);
                offset += us.length32();
            }
        }
        internal override void copy16bit(char[] target, int offset) {
            foreach (Saxon.Hej.str.UnicodeString us in segments) {
                us.copy16bit(target, offset);
                offset += us.length32();
            }
        }
        internal override void copy24bit(byte[] target, int offset) {
            foreach (Saxon.Hej.str.UnicodeString us in segments) {
                us.copy24bit(target, offset);
                offset += ((us.length32()*3));
            }
        }
        private Saxon.Hej.str.ZenoString consolidate() {
            int i = (segments.Count-2);
            long prevLength = segments[(i+1)].length();
            while ((i>=0)) {
                long thisLength = segments[i].length();
                long nextLength = ((i == 0) ? 0 : segments[(i-1)].length());
                if (((((thisLength<=prevLength) && (thisLength<=nextLength))) || ((thisLength+prevLength)<=32))) {
                    segments[i] = concatSegments(segments[i], segments[(i+1)]);
                    Saxon.Impl.Helpers.ListUtils.Remove(segments, (i+1));
                    Saxon.Impl.Helpers.ListUtils.Remove(offsets, (i+1));
                    prevLength = segments[i].length();
                } else {
                    prevLength = thisLength;
                }
                i--;
            }
            return this;
        }
        public static Saxon.Hej.str.UnicodeString concatSegments(Saxon.Hej.str.UnicodeString left, Saxon.Hej.str.UnicodeString right) {
            if (((left.getWidth()<=8) && (right.getWidth()<=8))) {
                byte[] newByteArray = new byte[(left.length32()+right.length32())];
                left.copy8bit(newByteArray, 0);
                right.copy8bit(newByteArray, left.length32());
                return new Saxon.Hej.str.Twine8(newByteArray);
            } else {
                if (((left.getWidth()<=16) && (right.getWidth()<=16))) {
                    char[] newCharArray = new char[(left.length32()+right.length32())];
                    left.copy16bit(newCharArray, 0);
                    right.copy16bit(newCharArray, left.length32());
                    return new Saxon.Hej.str.Twine16(newCharArray);
                } else {
                    byte[] newByteArray = new byte[(((left.length32()+right.length32()))*3)];
                    left.copy24bit(newByteArray, 0);
                    right.copy24bit(newByteArray, (left.length32()*3));
                    return new Saxon.Hej.str.Twine24(newByteArray);
                }
            }
        }
        private Saxon.Hej.str.ZenoString consolidate0() {
            for (int i = (segments.Count-2); (i>=0); i--) {
                double nextLength = (segments[(i+1)].length()*1.1);
                if ((segments[i].length()<nextLength)) {
                    segments[i] = concatSegments(segments[i], segments[(i+1)]);
                    Saxon.Impl.Helpers.ListUtils.Remove(segments, (i+1));
                    Saxon.Impl.Helpers.ListUtils.Remove(offsets, (i+1));
                }
            }
            verifySegmentLengths();
            return this;
        }
        private Saxon.Hej.str.ZenoString consolidate1() {
            int halfway = (segments.Count/2);
            for (int i = 0; (i<((halfway-1))); i++) {
                if (((segments[i].length()+segments[(i+1)].length())<((32L<<i)))) {
                    Saxon.Hej.str.UnicodeString merged = segments[i].concat(segments[(i+1)]);
                    Saxon.Impl.Helpers.ListUtils.Remove(segments, (i+1));
                    Saxon.Impl.Helpers.ListUtils.Remove(offsets, (i+1));
                    segments[i] = merged;
                }
            }
            int distance = 0;
            for (int i = (segments.Count-1); (i>halfway); i--) {
                if (((segments[i].length()+segments[(i-1)].length())<((32L<<(distance++))))) {
                    Saxon.Hej.str.UnicodeString merged = segments[(i-1)].concat(segments[i]);
                    Saxon.Impl.Helpers.ListUtils.Remove(segments, i);
                    Saxon.Impl.Helpers.ListUtils.Remove(offsets, i);
                    segments[(i-1)] = merged;
                }
            }
            return this;
        }
        public override Saxon.Hej.str.UnicodeString economize() {
            int segs = segments.Count;
            if ((segs == 0)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                if ((segs == 1)) {
                    return segments[0];
                } else {
                    if ((((segs<32) && (length()<256)) && (getWidth()<=16))) {
                        return new Saxon.Hej.str.BMPString(ToString());
                    } else {
                        return this;
                    }
                }
            }
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            foreach (Saxon.Hej.str.UnicodeString str in segments) {
                sb.append(str.ToString());
            }
            return sb.ToString();
        }
        public System.Collections.Generic.IList<System.Int64> debugSegmentLengths() {
            System.Collections.Generic.IList<System.Int64> result = new System.Collections.Generic.List<System.Int64>(segments.Count);
            foreach (Saxon.Hej.str.UnicodeString str in segments) {
                result.Add(str.length());
            }
            return result;
        }
        private void showSegmentLengths() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            foreach (Saxon.Hej.str.UnicodeString str in segments) {
                sb.append((str.length()+", "));
            }
            System.Console.Error.WriteLine(sb);
        }
        private void verifySegmentLengths() {
            long total = 0;
            for (int i = 0; (i<segments.Count); i++) {
                if ((offsets[i] != total)) {
                    showSegmentLengths();
                    throw new Saxon.Ejava.lang.IllegalStateException(("Bad offset for segment "+i));
                }
                total += segments[i].length();
            }
        }
    
        private class Inner_IntIterator_d1605e380 : Saxon.Hej.z.IntIterator {
            private readonly ZenoString outerInstance;
            public Inner_IntIterator_d1605e380(ZenoString outerInstance) {
                this.outerInstance = outerInstance;
                outerIterator = outerInstance.segments.GetEnumerator();
            }
            readonly System.Collections.Generic.IEnumerator<Saxon.Hej.str.UnicodeString> outerIterator;
            internal Saxon.Hej.z.IntIterator innerIterator;
            public bool hasNext() {
                if ((/*field*/innerIterator == null)) {
                    return /*field*/outerIterator.MoveNext() /*UNSAFE CONVERSION*/;
                } else {
                    if (/*field*/innerIterator.hasNext()) {
                        return true;
                    } else {
                        /*field*/innerIterator = null;
                        return /*field*/outerIterator.MoveNext() /*UNSAFE CONVERSION*/;
                    }
                }
            }
            public int next() {
                if ((/*field*/innerIterator == null)) {
                    if (/*field*/outerIterator.MoveNext() /*UNSAFE CONVERSION*/) {
                        /*field*/innerIterator = /*field*/outerIterator.Current.codePoints();
                    } else {
                        throw new Saxon.Ejava.util.NoSuchElementException();
                    }
                }
                return /*field*/innerIterator.next();
            }
        }

    }
}