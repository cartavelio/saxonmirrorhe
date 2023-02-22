using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    sealed class FollowingEnumeration : Saxon.Hej.tree.linked.TreeEnumeration {
        private readonly Saxon.Hej.tree.linked.NodeImpl root;
        public FollowingEnumeration(Saxon.Hej.tree.linked.NodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest) : base (node, nodeTest) {
            root = (Saxon.Hej.tree.linked.NodeImpl)(node.getRoot());
            int type = node.getNodeKind();
            if (((type == Saxon.Hej.type.Type.ATTRIBUTE) || (type == Saxon.Hej.type.Type.NAMESPACE))) {
                nextNode = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)node.getParent()).getNextInDocument(root);
            } else {
                do {
                    nextNode = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)node.getNextSibling());
                    if ((nextNode == null)) {
                        node = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)node.getParent());
                    }
                } while (((nextNode == null) && (node != null)));
            }
            while (!(conforms(nextNode))) {
                step();
            }
        }
        protected internal override void step() {
            nextNode = nextNode.getNextInDocument(root);
        }
    
    }
}