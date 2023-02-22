using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public sealed class SequenceCollector : Saxon.Hej.@event.SequenceWriter {
        private System.Collections.Generic.IList<Saxon.Hej.om.Item> list;
        public SequenceCollector(Saxon.Hej.@event.PipelineConfiguration pipe) : this (pipe, 20) {
        }
        public SequenceCollector(Saxon.Hej.@event.PipelineConfiguration pipe, int estimatedSize) : base (pipe) {
            this.list = new System.Collections.Generic.List<Saxon.Hej.om.Item>(estimatedSize);
        }
        public void reset() {
            list = new System.Collections.Generic.List<Saxon.Hej.om.Item>(System.Math.Min((list.Count+10), 50));
        }
        public override void write(Saxon.Hej.om.Item item) {
            list.Add(item);
        }
        public Saxon.Hej.om.GroundedValue getSequence() {
            switch (list.Count) {
              case 0:
                return Saxon.Hej.value.EmptySequence.getInstance();
              case 1:
                return list[0];
              default:
                return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(list);
            }
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            if (list.Count==0) {
                return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
            } else {
                return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(list);
            }
        }
        public System.Collections.Generic.IList<Saxon.Hej.om.Item> getList() {
            return list;
        }
        public Saxon.Hej.om.Item getFirstItem() {
            if (list.Count==0) {
                return null;
            } else {
                return list[0];
            }
        }
    
    }
}