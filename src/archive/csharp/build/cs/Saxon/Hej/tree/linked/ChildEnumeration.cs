using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    sealed class ChildEnumeration : Saxon.Hej.tree.linked.TreeEnumeration {
        public ChildEnumeration(Saxon.Hej.tree.linked.NodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest) : base (node, nodeTest) {
            nextNode = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)node.getFirstChild());
            while (!(conforms(nextNode))) {
                step();
            }
        }
        protected internal override void step() {
            nextNode = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)nextNode.getNextSibling());
        }
    
    }
}