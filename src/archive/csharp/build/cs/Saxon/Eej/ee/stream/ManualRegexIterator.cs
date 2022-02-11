using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class ManualRegexIterator : Saxon.Hej.tree.iter.ManualIterator, Saxon.Hej.regex.RegexIterator {
        private readonly bool currentItemMatches;
        private readonly Saxon.Hej.str.UnicodeString[] groups;
        public ManualRegexIterator(Saxon.Hej.regex.RegexIterator @base)  {
            currentItemMatches = @base.isMatching();
            groups = new Saxon.Hej.str.UnicodeString[(@base.getNumberOfGroups()+1)];
            for (int i = 0; (i<groups.Length); i++) {
                groups[i] = @base.getRegexGroup(i);
            }
        }
        public override Saxon.Hej.value.StringValue next() {
            return (Saxon.Hej.value.StringValue)(base.next());
        }
        public int getNumberOfGroups() {
            return groups.Length;
        }
        public bool isMatching() {
            return currentItemMatches;
        }
        public Saxon.Hej.str.UnicodeString getRegexGroup(int number) {
            if (((number>=0) && (number<groups.Length))) {
                return groups[number];
            } else {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
        }
        public void processMatchingSubstring(Saxon.Hej.regex.RegexMatchHandler action) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
    
    }
}