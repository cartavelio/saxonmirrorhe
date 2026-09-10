using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.om {
    public abstract class FleetingParentNode : Saxon.Eej.ee.stream.om.FleetingNode {
        private bool hasChildren = false;
        public override bool hasChildNodes() {
            return hasChildren;
        }
        public void setHasChildren(bool has) {
            hasChildren = has;
        }
        public abstract void notify(Saxon.Hej.@event.Receiver @out);
    
    }
}