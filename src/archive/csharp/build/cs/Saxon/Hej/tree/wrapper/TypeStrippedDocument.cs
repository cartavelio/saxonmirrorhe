using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class TypeStrippedDocument : Saxon.Hej.om.GenericTreeInfo {
        internal Saxon.Hej.om.TreeInfo underlyingTree;
        public TypeStrippedDocument(Saxon.Hej.om.TreeInfo doc) : base (doc.getConfiguration()) {
            setRootNode(wrap(doc.getRootNode()));
            this.underlyingTree = doc;
        }
        public Saxon.Hej.tree.wrapper.TypeStrippedNode wrap(Saxon.Hej.om.NodeInfo node) {
            return Saxon.Hej.tree.wrapper.TypeStrippedNode.makeWrapper(node, this, null);
        }
        public override Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            Saxon.Hej.om.NodeInfo n = underlyingTree.selectID(id, false);
            if ((n == null)) {
                return null;
            } else {
                return wrap(n);
            }
        }
    
    }
}