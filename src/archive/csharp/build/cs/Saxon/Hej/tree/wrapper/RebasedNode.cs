using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class RebasedNode : Saxon.Hej.tree.wrapper.AbstractVirtualNode, Saxon.Hej.tree.wrapper.WrappingFunction {
        protected internal RebasedNode()  {
        }
        protected internal RebasedNode(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.RebasedNode parent)  {
            this.node = node;
            this.parent = parent;
        }
        public static Saxon.Hej.tree.wrapper.RebasedNode makeWrapper(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.RebasedDocument docWrapper, Saxon.Hej.tree.wrapper.RebasedNode parent) {
            Saxon.Hej.tree.wrapper.RebasedNode wrapper = new Saxon.Hej.tree.wrapper.RebasedNode(node, parent);
            wrapper.docWrapper = docWrapper;
            return wrapper;
        }
        public Saxon.Hej.tree.wrapper.VirtualNode makeWrapper(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.VirtualNode parent) {
            Saxon.Hej.tree.wrapper.RebasedNode wrapper = new Saxon.Hej.tree.wrapper.RebasedNode(node, (Saxon.Hej.tree.wrapper.RebasedNode)(parent));
            wrapper.docWrapper = this.docWrapper;
            return wrapper;
        }
        private System.Func<Saxon.Hej.om.NodeInfo,string> getBaseUriMappingFunction() {
            return ((Saxon.Hej.tree.wrapper.RebasedDocument)(docWrapper)).getBaseUriMapper();
        }
        private System.Func<Saxon.Hej.om.NodeInfo,string> getSystemIdMappingFunction() {
            return ((Saxon.Hej.tree.wrapper.RebasedDocument)(docWrapper)).getSystemIdMapper();
        }
        public override string getBaseURI() {
            return getBaseUriMappingFunction()(node);
        }
        public override string getSystemId() {
            return getSystemIdMappingFunction()(node);
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.tree.wrapper.RebasedNode) && node.Equals(((Saxon.Hej.tree.wrapper.RebasedNode)(other)).node));
        }
        public override int GetHashCode() {
            return node.GetHashCode();
        }
        public override int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if ((other is Saxon.Hej.tree.wrapper.RebasedNode)) {
                return node.compareOrder(((Saxon.Hej.tree.wrapper.RebasedNode)(other)).node);
            } else {
                return node.compareOrder(other);
            }
        }
        public override Saxon.Hej.om.NodeInfo getParent() {
            if ((parent == null)) {
                Saxon.Hej.om.NodeInfo realParent = node.getParent();
                if ((realParent != null)) {
                    parent = makeWrapper(realParent, (Saxon.Hej.tree.wrapper.RebasedDocument)(docWrapper), null);
                }
            }
            return parent;
        }
        public override Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            return new Saxon.Hej.tree.wrapper.WrappingIterator(node.iterateAxis(axisNumber), this, null);
        }
    
    }
}