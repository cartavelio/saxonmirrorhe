using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    sealed class AttributeAxisIterator : Saxon.Hej.tree.iter.AxisIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.tree.linked.ElementImpl element;
        private readonly Saxon.Hej.pattern.NodeTest nodeTest;
        private Saxon.Hej.om.NodeInfo nextNode;
        private int index;
        private readonly int length;
        internal AttributeAxisIterator(Saxon.Hej.tree.linked.ElementImpl node, Saxon.Hej.pattern.NodeTest nodeTest)  {
            this.element = node;
            this.nodeTest = nodeTest;
            index = 0;
            length = node.attributes().size();
            advance();
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (nextNode != null);
        }
        public Saxon.Hej.om.Item next() {
            if ((nextNode == null)) {
                return null;
            } else {
                Saxon.Hej.om.NodeInfo current = nextNode;
                advance();
                return current;
            }
        }
        private void advance() {
            while (true) {
                if ((index>=length)) {
                    nextNode = null;
                    return ;
                } else {
                    Saxon.Hej.om.AttributeInfo info = element.attributes().itemAt(index);
                    if ((info is Saxon.Hej.om.AttributeInfo.Deleted)) {
                        index++;
                    } else {
                        nextNode = new Saxon.Hej.tree.linked.AttributeImpl(element, index);
                        index++;
                        if (nodeTest.test(nextNode)) {
                            return ;
                        }
                    }
                }
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

    }
}