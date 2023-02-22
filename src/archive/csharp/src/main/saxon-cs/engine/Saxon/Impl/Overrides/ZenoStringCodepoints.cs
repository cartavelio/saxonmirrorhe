using System.Diagnostics;
using Saxon.Hej.str;
using SysCollGen = System.Collections.Generic;


namespace Saxon.Impl.Overrides {
    /// <summary>
    /// Iterator over the codepoints of a ZenoString
    /// </summary>

    public class ZenoStringCodepoints : Saxon.Hej.z.IntIterator {
        private bool finished;
        private readonly SysCollGen.IEnumerator<Saxon.Hej.str.UnicodeString> outerIterator;
        private Saxon.Hej.z.IntIterator innerIterator;
            
        public ZenoStringCodepoints(SysCollGen.IList<Saxon.Hej.str.UnicodeString> segments) {
            outerIterator = segments.GetEnumerator();
            lookAhead();
        }

        public bool hasNext() {
            return !finished;
        }
        public int next() {
            if (finished) {
                throw new Saxon.Ejava.util.NoSuchElementException();
            }
            int result = innerIterator.next();
            if (!innerIterator.hasNext()) {
                lookAhead();
            }
            return result;
        }

        private void lookAhead() {
            // Note: segments of a ZenoString cannot be empty
            if (outerIterator.MoveNext()) {
                Debug.Assert(outerIterator.Current != null, "outerIterator.Current != null");
                innerIterator = outerIterator.Current.codePoints();
            }
            else {
                finished = true;
            } 
        }
    }
}

// Copyright (c) 2021 Saxonica Limited