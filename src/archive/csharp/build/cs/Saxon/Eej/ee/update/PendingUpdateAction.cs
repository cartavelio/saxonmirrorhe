using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public abstract class PendingUpdateAction {
        internal Saxon.Hej.expr.Expression originator;
        public void setOriginator(Saxon.Hej.expr.Expression originator) {
            this.originator = originator;
        }
        public Saxon.Hej.expr.Expression getOriginator() {
            return originator;
        }
        public abstract void apply(Saxon.Hej.expr.XPathContext context, System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedRootSet);
        public abstract Saxon.Hej.om.NodeInfo getTargetNode();
        public abstract int getApplyPhase();
        protected internal Saxon.Hej.om.NodeName fixNodeName(Saxon.Hej.om.NodeName name) {
            if ((name is Saxon.Hej.om.NameOfNode)) {
                if (name.hasFingerprint()) {
                    return new Saxon.Hej.om.FingerprintedQName(name.getPrefix(), name.getURI(), name.getLocalPart(), name.getFingerprint());
                } else {
                    return new Saxon.Hej.om.FingerprintedQName(name.getPrefix(), name.getURI(), name.getLocalPart());
                }
            } else {
                return name;
            }
        }
    
    }
}