using System;
using System.Collections.Generic;
namespace java.text {
    public class RuleBasedCollator {
        public CollationElementIterator getCollationElementIterator(string s) {
            return new CollationElementIterator(s); // TODO this is a dummy
        }
    }
}