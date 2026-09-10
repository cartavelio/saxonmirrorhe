using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class ReplaceValueAction : Saxon.Eej.ee.update.PendingUpdateAction {
        private readonly Saxon.Hej.str.UnicodeString content;
        private readonly Saxon.Hej.om.MutableNodeInfo targetNode;
        internal ReplaceValueAction(Saxon.Hej.str.UnicodeString content, Saxon.Hej.om.MutableNodeInfo targetNode)  {
            this.content = content;
            this.targetNode = targetNode;
        }
        public override void apply(Saxon.Hej.expr.XPathContext context, System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedRootSet) {
            Saxon.Hej.om.NodeInfo root = targetNode.getRoot();
            targetNode.replaceStringValue(content);
            int kind = targetNode.getNodeKind();
            if ((((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.TEXT)) || (kind == Saxon.Hej.type.Type.ATTRIBUTE))) {
                targetNode.removeTypeAnnotation();
            }
            affectedRootSet.Add((Saxon.Hej.om.MutableNodeInfo)(root));
        }
        public override Saxon.Hej.om.NodeInfo getTargetNode() {
            return targetNode;
        }
        public override int getApplyPhase() {
            return (((targetNode.getNodeKind() == Saxon.Hej.type.Type.ELEMENT) ? 4 : 1));
        }
    
    }
}