using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class TypeStrippedNode : Saxon.Hej.tree.wrapper.AbstractVirtualNode, Saxon.Hej.tree.wrapper.WrappingFunction {
        protected internal TypeStrippedNode()  {
        }
        protected internal TypeStrippedNode(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.TypeStrippedNode parent)  {
            this.node = node;
            this.parent = parent;
        }
        public static Saxon.Hej.tree.wrapper.TypeStrippedNode makeWrapper(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.TypeStrippedDocument docWrapper, Saxon.Hej.tree.wrapper.TypeStrippedNode parent) {
            Saxon.Hej.tree.wrapper.TypeStrippedNode wrapper = new Saxon.Hej.tree.wrapper.TypeStrippedNode(node, parent);
            wrapper.docWrapper = docWrapper;
            return wrapper;
        }
        public Saxon.Hej.tree.wrapper.VirtualNode makeWrapper(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.VirtualNode parent) {
            Saxon.Hej.tree.wrapper.TypeStrippedNode wrapper = new Saxon.Hej.tree.wrapper.TypeStrippedNode(node, (Saxon.Hej.tree.wrapper.TypeStrippedNode)(parent));
            wrapper.docWrapper = this.docWrapper;
            return wrapper;
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            return Saxon.Hej.value.StringValue.makeUntypedAtomic(getUnicodeStringValue());
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            if ((getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                return Saxon.Hej.type.Untyped.getInstance();
            } else {
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
            }
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.tree.wrapper.TypeStrippedNode)) {
                return node.Equals(((Saxon.Hej.tree.wrapper.TypeStrippedNode)(other)).node);
            } else {
                return node.Equals(other);
            }
        }
        public override int GetHashCode() {
            return node.GetHashCode();
        }
        public override int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if ((other is Saxon.Hej.tree.wrapper.TypeStrippedNode)) {
                return node.compareOrder(((Saxon.Hej.tree.wrapper.TypeStrippedNode)(other)).node);
            } else {
                return node.compareOrder(other);
            }
        }
        public override Saxon.Hej.om.NodeInfo getParent() {
            if ((parent == null)) {
                Saxon.Hej.om.NodeInfo realParent = node.getParent();
                if ((realParent != null)) {
                    parent = makeWrapper(realParent, (Saxon.Hej.tree.wrapper.TypeStrippedDocument)(docWrapper), null);
                }
            }
            return parent;
        }
        public override Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            return new Saxon.Hej.tree.wrapper.WrappingIterator(node.iterateAxis(axisNumber), this, null);
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            node.copy(@out, (copyOptions&~(Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS)), locationId);
        }
        public override bool isNilled() {
            return false;
        }
    
    }
}