using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class IntersectionEnumeration : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.om.SequenceIterator e1;
        private readonly Saxon.Hej.om.SequenceIterator e2;
        private Saxon.Hej.om.NodeInfo nextNode1;
        private Saxon.Hej.om.NodeInfo nextNode2;
        private readonly System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> comparer;
        public IntersectionEnumeration(Saxon.Hej.om.SequenceIterator p1, Saxon.Hej.om.SequenceIterator p2, System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> comparer)  {
            e1 = p1;
            e2 = p2;
            this.comparer = comparer;
            nextNode1 = nextNode(e1);
            nextNode2 = nextNode(e2);
        }
        private Saxon.Hej.om.NodeInfo nextNode(Saxon.Hej.om.SequenceIterator iter) {
            return (Saxon.Hej.om.NodeInfo)(iter.next());
        }
        public Saxon.Hej.om.Item next() {
            if ((nextNode1 == null)) {
                e2.close();
                return null;
            }
            if ((nextNode2 == null)) {
                e1.close();
                return null;
            }
            while (((nextNode1 != null) && (nextNode2 != null))) {
                int c = comparer.Compare(nextNode1, nextNode2);
                if ((c<0)) {
                    nextNode1 = nextNode(e1);
                } else {
                    if ((c>0)) {
                        nextNode2 = nextNode(e2);
                    } else {
                        Saxon.Hej.om.NodeInfo current = nextNode2;
                        nextNode2 = nextNode(e2);
                        nextNode1 = nextNode(e1);
                        return current;
                    }
                }
            }
            return null;
        }
        public void close() {
            e1.close();
            e2.close();
        }
        public void discharge() {
            e1.discharge();
            e2.discharge();
        }
    
    }
}