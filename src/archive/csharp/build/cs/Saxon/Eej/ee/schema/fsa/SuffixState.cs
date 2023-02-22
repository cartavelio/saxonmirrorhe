using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.fsa {
    public class SuffixState : Saxon.Eej.ee.schema.fsa.State {
        private SuffixState()  {
        }
        public static Saxon.Eej.ee.schema.fsa.SuffixState getInstance() {
            return THE_INSTANCE;
        }
        private static readonly Saxon.Eej.ee.schema.fsa.SuffixState THE_INSTANCE = new Saxon.Eej.ee.schema.fsa.SuffixState();
        public Saxon.Eej.ee.schema.fsa.Edge getTransition(int token, Saxon.Eej.ee.schema.UserComplexType type) {
            return null;
        }
        public bool isFinalState() {
            return true;
        }
        public string listAllowedElements() {
            return "";
        }
        public Saxon.Eej.ee.schema.fsa.Edge[] getWildcardEdges() {
            return new Saxon.Eej.ee.schema.fsa.Edge[0];
        }
        public System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.fsa.Edge> getEdges() {
            return new System.Collections.Generic.List<Saxon.Eej.ee.schema.fsa.Edge>();
        }
    
    }
}