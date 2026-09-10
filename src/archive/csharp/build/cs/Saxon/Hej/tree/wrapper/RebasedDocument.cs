using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class RebasedDocument : Saxon.Hej.om.GenericTreeInfo {
        private readonly Saxon.Hej.om.TreeInfo underlyingTree;
        private readonly System.Func<Saxon.Hej.om.NodeInfo,string> baseUriMapper;
        private readonly System.Func<Saxon.Hej.om.NodeInfo,string> systemIdMapper;
        public RebasedDocument(Saxon.Hej.om.TreeInfo doc, System.Func<Saxon.Hej.om.NodeInfo,string> baseUriMapper, System.Func<Saxon.Hej.om.NodeInfo,string> systemIdMapper) : base (doc.getConfiguration()) {
            this.baseUriMapper = baseUriMapper;
            this.systemIdMapper = systemIdMapper;
            setRootNode(wrap(doc.getRootNode()));
            this.underlyingTree = doc;
        }
        public Saxon.Hej.tree.wrapper.RebasedNode wrap(Saxon.Hej.om.NodeInfo node) {
            return Saxon.Hej.tree.wrapper.RebasedNode.makeWrapper(node, this, null);
        }
        public override  /* REPLACING public */ bool isTyped() {
            return underlyingTree.isTyped();
        }
        public override Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            Saxon.Hej.om.NodeInfo n = underlyingTree.selectID(id, false);
            if ((n == null)) {
                return null;
            } else {
                return wrap(n);
            }
        }
        public Saxon.Hej.om.TreeInfo getUnderlyingTree() {
            return underlyingTree;
        }
        public System.Func<Saxon.Hej.om.NodeInfo,string> getBaseUriMapper() {
            return baseUriMapper;
        }
        public System.Func<Saxon.Hej.om.NodeInfo,string> getSystemIdMapper() {
            return systemIdMapper;
        }
    
    }
}