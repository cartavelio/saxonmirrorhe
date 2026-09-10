using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class InsertAction : Saxon.Eej.ee.update.PendingUpdateAction {
        private readonly Saxon.Hej.om.NodeInfo[] content;
        private readonly Saxon.Hej.om.MutableNodeInfo targetNode;
        private readonly int position;
        private readonly bool inheritNamespaces;
        public InsertAction(Saxon.Hej.om.NodeInfo[] content, Saxon.Hej.om.MutableNodeInfo targetNode, int position, bool inherit)  {
            this.content = content;
            this.targetNode = targetNode;
            this.position = position;
            this.inheritNamespaces = inherit;
        }
        public override void apply(Saxon.Hej.expr.XPathContext context, System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedRootSet) {
            bool affectsType = false;
            foreach (Saxon.Hej.om.NodeInfo node in content) {
                int kind = node.getNodeKind();
                if (((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.TEXT))) {
                    affectsType = true;
                    break ;
                }
            }
            switch (position) {
              case Saxon.Hej.expr.parser.Token.FIRST_INTO:
                targetNode.insertChildren(content, true, inheritNamespaces);
                if (affectsType) {
                    targetNode.removeTypeAnnotation();
                }
                break ;
              case Saxon.Hej.expr.parser.Token.LAST_INTO:
              case Saxon.Hej.expr.parser.Token.INTO:
                targetNode.insertChildren(content, false, inheritNamespaces);
                if (affectsType) {
                    targetNode.removeTypeAnnotation();
                }
                break ;
              case Saxon.Hej.expr.parser.Token.BEFORE:
                targetNode.insertSiblings(content, true, inheritNamespaces);
                if (affectsType) {
                    ((Saxon.Hej.om.MutableNodeInfo)(targetNode.getParent())).removeTypeAnnotation();
                }
                break ;
              case Saxon.Hej.expr.parser.Token.AFTER:
                targetNode.insertSiblings(content, false, inheritNamespaces);
                if (affectsType) {
                    ((Saxon.Hej.om.MutableNodeInfo)(targetNode.getParent())).removeTypeAnnotation();
                }
                break ;
              default:
                throw new Saxon.Ejava.lang.UnsupportedOperationException(("Unknown insert position "+position));
            }
            affectedRootSet.Add((Saxon.Hej.om.MutableNodeInfo)(targetNode.getRoot()));
        }
        public override Saxon.Hej.om.NodeInfo getTargetNode() {
            return targetNode;
        }
        public override int getApplyPhase() {
            return (((position == Saxon.Hej.expr.parser.Token.INTO) ? 1 : 2));
        }
    
    }
}