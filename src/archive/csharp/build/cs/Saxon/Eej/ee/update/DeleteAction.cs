using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class DeleteAction : Saxon.Eej.ee.update.PendingUpdateAction {
        private readonly Saxon.Hej.om.NodeInfo targetNode;
        public DeleteAction(Saxon.Hej.om.NodeInfo node)  {
            targetNode = node;
            if (!(((node is Saxon.Hej.om.MutableNodeInfo)))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Attempting to delete a non-updatable node");
            }
        }
        public override void apply(Saxon.Hej.expr.XPathContext context, System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedRootSet) {
            if (!(((Saxon.Hej.om.MutableNodeInfo)(targetNode)).isDeleted())) {
                int kind = targetNode.getNodeKind();
                Saxon.Hej.om.MutableNodeInfo parent = (Saxon.Hej.om.MutableNodeInfo)(targetNode.getParent());
                affectedRootSet.Add((Saxon.Hej.om.MutableNodeInfo)(targetNode.getRoot()));
                ((Saxon.Hej.om.MutableNodeInfo)(targetNode)).delete();
                if ((((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.ATTRIBUTE)) || (kind == Saxon.Hej.type.Type.TEXT))) {
                    parent.removeTypeAnnotation();
                }
            }
        }
        public override Saxon.Hej.om.NodeInfo getTargetNode() {
            return targetNode;
        }
        public override int getApplyPhase() {
            return PHASE;
        }
        public const int PHASE = 5;
    
    }
}