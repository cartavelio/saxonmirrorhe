using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    sealed class PrecedingSiblingEnumeration : Saxon.Hej.tree.linked.TreeEnumeration {
        public PrecedingSiblingEnumeration(Saxon.Hej.tree.linked.NodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest) : base (node, nodeTest) {
            advance();
        }
        protected internal override void step() {
            nextNode = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)nextNode.getPreviousSibling());
        }
    
    }
}