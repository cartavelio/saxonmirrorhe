using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class IncrementalIterator : Saxon.Hej.tree.iter.AxisIterator {
        private System.Func<Saxon.Hej.om.NodeInfo,Saxon.Hej.om.NodeInfo> stepper;
        private Saxon.Hej.om.NodeInfo nextItem;
        public IncrementalIterator(Saxon.Hej.om.NodeInfo start, System.Func<Saxon.Hej.om.NodeInfo,Saxon.Hej.om.NodeInfo> stepper)  {
            this.nextItem = stepper(start);
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.om.NodeInfo current = nextItem;
            nextItem = stepper(current);
            return current;
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

    }
}