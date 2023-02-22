using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    sealed class PrecedingEnumeration : Saxon.Hej.tree.linked.TreeEnumeration {
        internal Saxon.Hej.tree.linked.NodeImpl nextAncestor;
        public PrecedingEnumeration(Saxon.Hej.tree.linked.NodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest) : base (node, nodeTest) {
            nextAncestor = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)node.getParent());
            advance();
        }
        protected internal override bool conforms(Saxon.Hej.tree.linked.NodeImpl node) {
            if ((node != null)) {
                if (node.Equals(nextAncestor)) {
                    nextAncestor = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)nextAncestor.getParent());
                    return false;
                }
            }
            return base.conforms(node);
        }
        protected internal override void step() {
            nextNode = nextNode.getPreviousInDocument();
        }
    
    }
}