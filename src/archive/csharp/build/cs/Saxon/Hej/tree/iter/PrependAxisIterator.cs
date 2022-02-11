using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class PrependAxisIterator : Saxon.Hej.tree.iter.PrependSequenceIterator, Saxon.Hej.tree.iter.AxisIterator {
        public PrependAxisIterator(Saxon.Hej.om.NodeInfo start, Saxon.Hej.tree.iter.AxisIterator @base) : base (start, @base) {
        }
        public override Saxon.Hej.om.NodeInfo next() {
            return (Saxon.Hej.om.NodeInfo)(base.next());
        }
    
    }
}