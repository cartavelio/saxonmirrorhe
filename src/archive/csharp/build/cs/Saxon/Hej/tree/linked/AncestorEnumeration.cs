using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    sealed class AncestorEnumeration : Saxon.Hej.tree.linked.TreeEnumeration {
        public AncestorEnumeration(Saxon.Hej.tree.linked.NodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest, bool includeSelf) : base (node, nodeTest) {
            if ((!(includeSelf) || !(conforms(node)))) {
                advance();
            }
        }
        protected internal override void step() {
            nextNode = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)nextNode.getParent());
        }
    
    }
}