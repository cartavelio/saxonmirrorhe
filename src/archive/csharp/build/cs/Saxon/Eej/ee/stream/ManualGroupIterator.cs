using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class ManualGroupIterator : Saxon.Hej.tree.iter.ManualIterator, Saxon.Hej.expr.sort.GroupIterator {
        private Saxon.Hej.om.AtomicSequence currentGroupingKey;
        private Saxon.Hej.om.GroundedValue currentGroupItems;
        public ManualGroupIterator(Saxon.Hej.om.Item item, int position) : base (item, position) {
        }
        public Saxon.Eej.ee.stream.ManualGroupIterator getSnapShot(Saxon.Hej.expr.XPathContext context) {
            return this;
        }
        public void setCurrentGroupingKey(Saxon.Hej.om.AtomicSequence key) {
            currentGroupingKey = key;
        }
        public Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() {
            return currentGroupingKey;
        }
        public void setCurrentGroup(Saxon.Hej.om.GroundedValue group) {
            this.currentGroupItems = group;
        }
        public void appendToCurrentGroup(Saxon.Hej.om.Item item) {
            this.currentGroupItems = item;
        }
        public void startNewCurrentGroup(Saxon.Hej.om.Item item) {
            this.currentGroupItems = item;
        }
        public Saxon.Hej.om.GroundedValue currentGroup() {
            return currentGroupItems;
        }
        public bool currentGroupContains(Saxon.Hej.om.NodeInfo node) {
            return currentGroupItems.containsNode(node);
        }
    
    }
}