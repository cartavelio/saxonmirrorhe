using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    sealed class PrecedingOrAncestorEnumeration : Saxon.Hej.tree.linked.TreeEnumeration {
        public PrecedingOrAncestorEnumeration(Saxon.Hej.tree.linked.NodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest) : base (node, nodeTest) {
            advance();
        }
        protected internal override void step() {
            nextNode = nextNode.getPreviousInDocument();
        }
    
    }
}