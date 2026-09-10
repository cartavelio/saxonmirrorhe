using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class ConcatenatingAxisIterator : Saxon.Hej.tree.iter.AxisIterator {
        internal Saxon.Hej.tree.iter.AxisIterator first;
        internal Saxon.Hej.tree.iter.AxisIterator second;
        internal Saxon.Hej.tree.iter.AxisIterator active;
        public ConcatenatingAxisIterator(Saxon.Hej.tree.iter.AxisIterator first, Saxon.Hej.tree.iter.AxisIterator second)  {
            this.first = first;
            this.second = second;
            this.active = first;
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.om.NodeInfo n = ((/*1430*/Saxon.Hej.om.NodeInfo)active.next());
            if (((n == null) && (active == first))) {
                active = second;
                n = ((/*1430*/Saxon.Hej.om.NodeInfo)second.next());
            }
            return n;
        }
        public void close() {
            first.close();
            second.close();
        }
    
        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}