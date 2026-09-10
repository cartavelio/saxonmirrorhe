using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public sealed class DocumentOrderIterator : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.om.SequenceIterator iterator;
        private readonly System.Collections.Generic.List<Saxon.Hej.om.NodeInfo> sequence;
        private Saxon.Hej.om.NodeInfo current = null;
        public DocumentOrderIterator(Saxon.Hej.om.SequenceIterator @base, System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> comparer)  {
            int len = (Saxon.Hej.om.SequenceTool.supportsGetLength(@base) ? Saxon.Hej.om.SequenceTool.getLength(@base) : 50);
            sequence = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>(len);
            Saxon.Hej.om.SequenceTool.supply(@base, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    sequence.Add((Saxon.Hej.om.NodeInfo)(item));
                } else {
                    throw new Saxon.Hej.trans.XPathException(("Item in input for sorting is not a node: "+Saxon.Hej.trans.Err.depict(item)), "XPTY0004");
                }
            } )));
            if ((sequence.Count>1)) {
                sequence.Sort(comparer);
            }
            iterator = new Saxon.Hej.tree.iter.NodeListIterator(sequence);
        }
        public Saxon.Hej.om.Item next() {
            while (true) {
                Saxon.Hej.om.NodeInfo next = (Saxon.Hej.om.NodeInfo)(iterator.next());
                if ((next == null)) {
                    current = null;
                    return null;
                }
                if (!(next.Equals(current))) {
                    current = next;
                    return current;
                }
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

    }
}