using System;
using System.Collections.Generic;
namespace Saxon.Ejava.text {
    public class RuleBasedCollator : Saxon.Ejava.util.Comparator<object> {
        public CollationElementIterator getCollationElementIterator(string s) {
            return new CollationElementIterator(s); // TODO this is a dummy
        }

        public int compare(object a, object b) {
            throw new NotImplementedException();
        }
    }
}