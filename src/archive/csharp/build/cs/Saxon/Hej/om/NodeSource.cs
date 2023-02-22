using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class NodeSource : Saxon.Hej.lib.ActiveSource {
        private readonly Saxon.Hej.om.NodeInfo node;
        private string systemId;
        public NodeSource(Saxon.Hej.om.NodeInfo node)  {
            this.node = node;
            this.systemId = node.getSystemId();
        }
        public void deliver(Saxon.Hej.@event.Receiver receiver, Saxon.Hej.lib.ParseOptions options) {
            Saxon.Hej.@event.Sender.sendDocumentInfo(node, receiver, new Saxon.Hej.expr.parser.Loc(getSystemId(), -1, -1));
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getSystemId() {
            if ((systemId == null)) {
                return node.getSystemId();
            } else {
                return this.systemId;
            }
        }
        public Saxon.Hej.om.NodeInfo getNode() {
            return node;
        }
    
    }
}