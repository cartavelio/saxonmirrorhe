using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class ATokenIterator : Saxon.Hej.tree.iter.AtomicIterator {
        private readonly Saxon.Hej.str.UnicodeString input;
        private readonly Saxon.Hej.regex.REMatcher matcher;
        private Saxon.Hej.value.StringValue current;
        private int prevEnd;
        public ATokenIterator(Saxon.Hej.str.UnicodeString input, Saxon.Hej.regex.REMatcher matcher)  {
            this.input = input;
            this.matcher = matcher;
            prevEnd = 0;
        }
        public Saxon.Hej.om.Item next() {
            if ((prevEnd<0)) {
                current = null;
                return null;
            }
            if (matcher.match(input, prevEnd)) {
                int start = matcher.getParenStart(0);
                current = new Saxon.Hej.value.StringValue(input.substring(prevEnd, start));
                prevEnd = matcher.getParenEnd(0);
            } else {
                current = new Saxon.Hej.value.StringValue(input.substring(prevEnd));
                prevEnd = -1;
            }
            return currentStringValue();
        }
        private Saxon.Hej.value.StringValue currentStringValue() {
            return current;
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

    }
}