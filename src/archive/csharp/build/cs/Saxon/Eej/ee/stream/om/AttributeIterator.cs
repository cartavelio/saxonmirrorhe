using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.om {
    sealed public class AttributeIterator : Saxon.Hej.tree.iter.AxisIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Eej.ee.stream.om.FleetingElementNode element;
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> attributeList;
        private int position;
        public AttributeIterator(Saxon.Eej.ee.stream.om.FleetingElementNode element, Saxon.Hej.om.AttributeMap attributes)  {
            this.element = element;
            this.attributeList = attributes.asList();
            this.position = 0;
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (position<attributeList.Count);
        }
        public Saxon.Hej.om.Item next() {
            if (hasNext()) {
                Saxon.Hej.om.AttributeInfo info = attributeList[position++];
                return element.makeAttributeNode(info);
            } else {
                return null;
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

    }
}