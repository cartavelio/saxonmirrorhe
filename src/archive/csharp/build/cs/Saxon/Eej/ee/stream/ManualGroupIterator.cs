using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class ManualGroupIterator : Saxon.Hej.tree.iter.ManualIterator, Saxon.Hej.expr.sort.GroupIterator {
        private Saxon.Hej.om.AtomicSequence currentGroupingKey;
        private System.Collections.Generic.IList<Saxon.Hej.om.Item> currentGroupItems = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
        public ManualGroupIterator(Saxon.Hej.om.Item item, int position) : base (item, position) {
        }
        public Saxon.Eej.ee.stream.ManualGroupIterator getSnapShot(Saxon.Hej.expr.XPathContext context) {
            return this;
        }
        public void setCurrentGroupingKey(Saxon.Hej.om.AtomicSequence key) {
            currentGroupingKey = key;
        }
        public virtual Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() {
            return currentGroupingKey;
        }
        public void setCurrentGroup(System.Collections.Generic.IList<Saxon.Hej.om.Item> group) {
            this.currentGroupItems = group;
        }
        public void appendToCurrentGroup(Saxon.Hej.om.Item item) {
            this.currentGroupItems.Clear();
            this.currentGroupItems.Add(item);
        }
        public void startNewCurrentGroup(Saxon.Hej.om.Item item) {
            this.currentGroupItems.Clear();
            this.currentGroupItems.Add(item);
        }
        public virtual Saxon.Hej.om.SequenceIterator iterateCurrentGroup() {
            return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(currentGroupItems);
        }
        public bool currentGroupContains(Saxon.Hej.om.Item item) {
            return currentGroupItems.Contains(item);
        }
    
    }
}