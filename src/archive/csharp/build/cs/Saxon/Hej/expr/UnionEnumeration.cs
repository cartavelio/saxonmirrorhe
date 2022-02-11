using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class UnionEnumeration : Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.SequenceIterator e1;
        private readonly Saxon.Hej.om.SequenceIterator e2;
        private Saxon.Hej.om.NodeInfo nextNode1 = null;
        private Saxon.Hej.om.NodeInfo nextNode2 = null;
        private readonly System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> comparer;
        public UnionEnumeration(Saxon.Hej.om.SequenceIterator p1, Saxon.Hej.om.SequenceIterator p2, System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> comparer)  {
            this.e1 = p1;
            this.e2 = p2;
            this.comparer = comparer;
            nextNode1 = nextNode(e1);
            nextNode2 = nextNode(e2);
        }
        private Saxon.Hej.om.NodeInfo nextNode(Saxon.Hej.om.SequenceIterator iter) {
            return (Saxon.Hej.om.NodeInfo)(iter.next());
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return ((nextNode1 != null) || (nextNode2 != null));
        }
        public Saxon.Hej.om.Item next() {
            if (((nextNode1 != null) && (nextNode2 != null))) {
                int c = comparer.Compare(nextNode1, nextNode2);
                if ((c<0)) {
                    Saxon.Hej.om.NodeInfo current = nextNode1;
                    nextNode1 = nextNode(e1);
                    return current;
                } else {
                    if ((c>0)) {
                        Saxon.Hej.om.NodeInfo current = nextNode2;
                        nextNode2 = nextNode(e2);
                        return current;
                    } else {
                        Saxon.Hej.om.NodeInfo current = nextNode2;
                        nextNode2 = nextNode(e2);
                        nextNode1 = nextNode(e1);
                        return current;
                    }
                }
            }
            if ((nextNode1 != null)) {
                Saxon.Hej.om.NodeInfo current = nextNode1;
                nextNode1 = nextNode(e1);
                return current;
            }
            if ((nextNode2 != null)) {
                Saxon.Hej.om.NodeInfo current = nextNode2;
                nextNode2 = nextNode(e2);
                return current;
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