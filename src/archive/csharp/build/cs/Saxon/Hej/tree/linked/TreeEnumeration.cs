using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    abstract class TreeEnumeration : Saxon.Hej.tree.iter.AxisIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        protected internal Saxon.Hej.tree.linked.NodeImpl start;
        protected internal Saxon.Hej.tree.linked.NodeImpl nextNode;
        protected internal Saxon.Hej.pattern.NodePredicate nodeTest;
        protected internal Saxon.Hej.tree.linked.NodeImpl current = null;
        protected internal int position = 0;
        public TreeEnumeration(Saxon.Hej.tree.linked.NodeImpl origin, Saxon.Hej.pattern.NodePredicate nodeTest)  {
            nextNode = origin;
            start = origin;
            this.nodeTest = nodeTest;
        }
        protected internal virtual bool conforms(Saxon.Hej.tree.linked.NodeImpl node) {
            return (((node == null) || (nodeTest == null)) || nodeTest.test(node));
        }
        protected internal void advance() {
            do {
                step();
            } while (!(conforms(nextNode)));
        }
        protected internal abstract void step();
        public bool hasNext() {
            return (nextNode != null);
        }
        public Saxon.Hej.om.Item next() {
            if ((nextNode == null)) {
                current = null;
                position = -1;
                return null;
            } else {
                current = nextNode;
                position++;
                advance();
                return current;
            }
        }
        public bool supportsHasNext() {
            return true;
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}