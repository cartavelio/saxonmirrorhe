using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class JTokenIterator : Saxon.Hej.tree.iter.AtomicIterator {
        private readonly string input;
        private readonly Saxon.Ejava.util.regex.Pattern pattern;
        private readonly Saxon.Ejava.util.regex.Matcher matcher;
        private string current;
        private int prevEnd = 0;
        public JTokenIterator(string input, Saxon.Ejava.util.regex.Pattern pattern)  {
            this.input = input;
            this.pattern = pattern;
            matcher = pattern.matcher(input);
            prevEnd = 0;
        }
        public Saxon.Hej.om.Item next() {
            if ((prevEnd<0)) {
                current = null;
                return null;
            }
            if (matcher.find()) {
                current = input.Substring(prevEnd, matcher.start() - prevEnd);
                prevEnd = matcher.end();
            } else {
                current = input.Substring(prevEnd, input.Length - prevEnd);
                prevEnd = -1;
            }
            return Saxon.Hej.value.StringValue.makeStringValue(current);
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}