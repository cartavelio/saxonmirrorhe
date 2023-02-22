using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class CodepointIterator : Saxon.Hej.tree.iter.AtomicIterator {
        readonly Saxon.Hej.z.IntIterator codepoints;
        public CodepointIterator(Saxon.Hej.z.IntIterator codepoints)  {
            this.codepoints = codepoints;
        }
        public Saxon.Hej.om.Item next() {
            return (codepoints.hasNext() ? new Saxon.Hej.value.Int64Value(codepoints.next()) : null);
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}