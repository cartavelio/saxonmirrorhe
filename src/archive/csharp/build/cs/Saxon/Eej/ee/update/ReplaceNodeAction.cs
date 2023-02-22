using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class ReplaceNodeAction : Saxon.Eej.ee.update.PendingUpdateAction {
        private readonly Saxon.Hej.om.NodeInfo[] content;
        private readonly Saxon.Hej.om.MutableNodeInfo targetNode;
        private readonly bool inherit;
        internal ReplaceNodeAction(Saxon.Hej.om.NodeInfo[] content, Saxon.Hej.om.MutableNodeInfo targetNode, bool inherit)  {
            this.content = content;
            this.targetNode = targetNode;
            this.inherit = inherit;
        }
        public override void apply(Saxon.Hej.expr.XPathContext context, System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedRootSet) {
            if (!(targetNode.getTreeInfo().isMutable())) {
                throw new Saxon.Hej.trans.XPathException("Attempting to update a non-updatable element", Saxon.Hej.trans.SaxonErrorCode.SXUP0081);
            }
            Saxon.Hej.om.NodeInfo root = targetNode.getRoot();
            Saxon.Hej.om.MutableNodeInfo parent = (Saxon.Hej.om.MutableNodeInfo)(targetNode.getParent());
            if ((!(parent.isDeleted()) && !(targetNode.isDeleted()))) {
                targetNode.replace(content, inherit);
                affectedRootSet.Add((Saxon.Hej.om.MutableNodeInfo)(root));
                parent.removeTypeAnnotation();
            }
        }
        public override Saxon.Hej.om.NodeInfo getTargetNode() {
            return targetNode;
        }
        public Saxon.Hej.om.NodeInfo[] getNewContent() {
            return content;
        }
        public override int getApplyPhase() {
            return 3;
        }
    
    }
}