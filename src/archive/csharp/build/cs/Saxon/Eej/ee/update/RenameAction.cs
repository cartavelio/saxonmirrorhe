using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class RenameAction : Saxon.Eej.ee.update.PendingUpdateAction {
        private readonly Saxon.Hej.om.NodeInfo node;
        private readonly Saxon.Hej.om.NodeName newName;
        private readonly bool inherit;
        internal RenameAction(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeName newName, bool inherit)  {
            this.node = node;
            this.newName = fixNodeName(newName);
            this.inherit = inherit;
        }
        public override void apply(Saxon.Hej.expr.XPathContext context, System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedRootSet) {
            affectedRootSet.Add((Saxon.Hej.om.MutableNodeInfo)(node.getRoot()));
            ((Saxon.Hej.om.MutableNodeInfo)(node)).rename(newName, inherit);
            int kind = node.getNodeKind();
            if (((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.ATTRIBUTE))) {
                ((Saxon.Hej.om.MutableNodeInfo)(node)).removeTypeAnnotation();
            }
        }
        public override Saxon.Hej.om.NodeInfo getTargetNode() {
            return node;
        }
        public Saxon.Hej.om.NodeName getNewName() {
            return newName;
        }
        public override int getApplyPhase() {
            return PHASE;
        }
        public bool isInheritNamespaces() {
            return inherit;
        }
        private const int PHASE = 0;
    
    }
}