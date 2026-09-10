using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class LazySequence : Saxon.Hej.om.Sequence {
        internal Saxon.Hej.om.SequenceIterator iterator;
        internal bool used = false;
        public LazySequence(Saxon.Hej.om.SequenceIterator iterator)  {
            this.iterator = iterator;
        }
        public Saxon.Hej.om.Item head() {
            return iterate().next();
        }
        public Saxon.Hej.om.SequenceIterator iterate() { lock(this) {
            if (used) {
                throw new Saxon.Ejava.lang.IllegalStateException("A LazySequence can only be read once");
            } else {
                used = true;
                return iterator;
            }
        }}
        public Saxon.Hej.om.Sequence makeRepeatable() {
            return materialize();
        }
    
        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

    }
}