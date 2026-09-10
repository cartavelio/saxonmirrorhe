using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed public class AncestorIterator : Saxon.Hej.tree.iter.AxisIterator {
        private readonly Saxon.Hej.om.NodeInfo startNode;
        private Saxon.Hej.om.NodeInfo current;
        private readonly Saxon.Hej.pattern.NodeTest test;
        public AncestorIterator(Saxon.Hej.om.NodeInfo node, Saxon.Hej.pattern.NodeTest nodeTest)  {
            test = nodeTest;
            startNode = node;
            current = startNode;
        }
        public Saxon.Hej.om.Item next() {
            if ((current == null)) {
                return null;
            }
            Saxon.Hej.om.NodeInfo node = current.getParent();
            while (((node != null) && !(test.test(node)))) {
                node = node.getParent();
            }
            return current = node;
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public void discharge() {
        }

    }
}