using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed class AttributeIterator : Saxon.Hej.tree.iter.AxisIterator, Saxon.Hej.om.AtomizedValueIterator {
        private readonly Saxon.Hej.tree.tiny.TinyTree tree;
        private readonly int element;
        private readonly Saxon.Hej.pattern.NodeTest nodeTest;
        private int index;
        private int currentNodeNr;
        internal AttributeIterator(Saxon.Hej.tree.tiny.TinyTree tree, int element, Saxon.Hej.pattern.NodeTest nodeTest)  {
            this.nodeTest = nodeTest;
            this.tree = tree;
            this.element = element;
            index = tree.alpha[element];
            currentNodeNr = -1;
        }
        private bool moveToNext() {
            while (true) {
                if (((index>=tree.numberOfAttributes) || (tree.attParent[index] != element))) {
                    index = System.Int32.MaxValue;
                    currentNodeNr = -1;
                    return false;
                }
                Saxon.Hej.type.SchemaType typeCode = tree.getAttributeType(index);
                if (nodeTest.matches(Saxon.Hej.type.Type.ATTRIBUTE, new Saxon.Hej.om.CodedName((tree.attCode[index]&Saxon.Hej.om.NamePool.FP_MASK), "", tree.getNamePool()), typeCode)) {
                    currentNodeNr = index++;
                    if ((nodeTest is Saxon.Hej.pattern.NameTest)) {
                        index = System.Int32.MaxValue;
                    }
                    return true;
                }
                index++;
            }
        }
        public Saxon.Hej.om.Item next() {
            if (moveToNext()) {
                return tree.getAttributeNode(currentNodeNr);
            } else {
                return null;
            }
        }
        public Saxon.Hej.om.AtomicSequence nextAtomizedValue() {
            if (moveToNext()) {
                return tree.getTypedValueOfAttribute(null, currentNodeNr);
            } else {
                return null;
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

    }
}