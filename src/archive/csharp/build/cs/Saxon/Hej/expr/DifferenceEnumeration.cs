using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class DifferenceEnumeration : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.om.SequenceIterator p1;
        private readonly Saxon.Hej.om.SequenceIterator p2;
        private Saxon.Hej.om.NodeInfo nextNode1 = null;
        private Saxon.Hej.om.NodeInfo nextNode2 = null;
        private readonly System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> comparer;
        public DifferenceEnumeration(Saxon.Hej.om.SequenceIterator p1, Saxon.Hej.om.SequenceIterator p2, System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> comparer)  {
            this.p1 = p1;
            this.p2 = p2;
            this.comparer = comparer;
            nextNode1 = nextNode(p1);
            nextNode2 = nextNode(p2);
        }
        private Saxon.Hej.om.NodeInfo nextNode(Saxon.Hej.om.SequenceIterator iter) {
            return (Saxon.Hej.om.NodeInfo)(iter.next());
        }
        public Saxon.Hej.om.Item next() {
            while (true) {
                if ((nextNode1 == null)) {
                    return null;
                }
                if ((nextNode2 == null)) {
                    return deliver();
                }
                int c = comparer.Compare(nextNode1, nextNode2);
                if ((c<0)) {
                    return deliver();
                } else {
                    if ((c>0)) {
                        nextNode2 = nextNode(p2);
                        if ((nextNode2 == null)) {
                            return deliver();
                        }
                    } else {
                        nextNode2 = nextNode(p2);
                        nextNode1 = nextNode(p1);
                    }
                }
            }
        }
        private Saxon.Hej.om.NodeInfo deliver() {
            Saxon.Hej.om.NodeInfo current = nextNode1;
            nextNode1 = nextNode(p1);
            return current;
        }
        public void close() {
            p1.close();
            p2.close();
        }
    
    }
}