using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class NodeListIterator : Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.NodeInfo>, Saxon.Hej.tree.iter.AxisIterator {
        public NodeListIterator(System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> list) : base (list) {
        }
        public override Saxon.Hej.om.NodeInfo next() {
            return (Saxon.Hej.om.NodeInfo)(base.next());
        }
    
    }
}