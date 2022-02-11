using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class SpaceStrippedNode : Saxon.Hej.tree.wrapper.AbstractVirtualNode, Saxon.Hej.tree.wrapper.WrappingFunction {
        protected internal SpaceStrippedNode()  {
        }
        protected internal SpaceStrippedNode(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.SpaceStrippedNode parent)  {
            this.node = node;
            this.parent = parent;
        }
        protected internal static Saxon.Hej.tree.wrapper.SpaceStrippedNode makeWrapper(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.SpaceStrippedDocument docWrapper, Saxon.Hej.tree.wrapper.SpaceStrippedNode parent) {
            Saxon.Hej.tree.wrapper.SpaceStrippedNode wrapper = new Saxon.Hej.tree.wrapper.SpaceStrippedNode(node, parent);
            wrapper.docWrapper = docWrapper;
            return wrapper;
        }
        public Saxon.Hej.tree.wrapper.VirtualNode makeWrapper(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.VirtualNode parent) {
            Saxon.Hej.tree.wrapper.SpaceStrippedNode wrapper = new Saxon.Hej.tree.wrapper.SpaceStrippedNode(node, (Saxon.Hej.tree.wrapper.SpaceStrippedNode)(parent));
            wrapper.docWrapper = this.docWrapper;
            return wrapper;
        }
        public static bool isPreservedNode(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.SpaceStrippedDocument docWrapper, Saxon.Hej.om.NodeInfo actualParent) {
            if ((((node.getNodeKind() != Saxon.Hej.type.Type.TEXT) || (actualParent == null)) || !(Saxon.Hej.value.Whitespace.isAllWhite(node.getUnicodeStringValue())))) {
                return true;
            }
            Saxon.Hej.type.SchemaType type = actualParent.getSchemaType();
            if ((type.isSimpleType() || ((Saxon.Hej.type.ComplexType)(type)).isSimpleContent())) {
                return true;
            }
            if (docWrapper.containsPreserveSpace()) {
                Saxon.Hej.om.NodeInfo p = actualParent;
                while ((p.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                    string val = p.getAttributeValue(Saxon.Hej.lib.NamespaceConstant.XML, "space");
                    if ((val != null)) {
                        if ("preserve".Equals(val)) {
                            return true;
                        } else {
                            if ("default".Equals(val)) {
                                break ;
                            }
                        }
                    }
                    p = p.getParent();
                }
            }
            if (docWrapper.containsAssertions()) {
                Saxon.Hej.om.NodeInfo p = actualParent;
                while ((p.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                    Saxon.Hej.type.SchemaType t = p.getSchemaType();
                    if (((t is Saxon.Hej.type.ComplexType) && ((Saxon.Hej.type.ComplexType)(t)).hasAssertions())) {
                        return true;
                    }
                    p = p.getParent();
                }
            }
            try {
                int preserve = docWrapper.getStrippingRule().isSpacePreserving(Saxon.Hej.om.NameOfNode.makeName(actualParent), null);
                return (preserve == Saxon.Hej.@event.Stripper.ALWAYS_PRESERVE);
            } catch (Saxon.Hej.trans.XPathException e) {
                return true;
            }
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            if ((getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                return getSchemaType().atomize(this);
            } else {
                return node.atomize();
            }
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.tree.wrapper.SpaceStrippedNode)) {
                return node.Equals(((Saxon.Hej.tree.wrapper.SpaceStrippedNode)(other)).node);
            } else {
                return node.Equals(other);
            }
        }
        public override int GetHashCode() {
            return node.GetHashCode();
        }
        public override int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if ((other is Saxon.Hej.tree.wrapper.SpaceStrippedNode)) {
                return node.compareOrder(((Saxon.Hej.tree.wrapper.SpaceStrippedNode)(other)).node);
            } else {
                return node.compareOrder(other);
            }
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
              case Saxon.Hej.type.Type.ELEMENT:
                Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(Saxon.Hej.type.Type.TEXT));
                Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
                Saxon.Hej.om.NodeInfo it;
                while (((it = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                    sb.accept(it.getUnicodeStringValue());
                }
                return sb.toUnicodeString();
              default:
                return node.getUnicodeStringValue();
            }
        }
        public override Saxon.Hej.om.NodeInfo getParent() {
            if ((parent == null)) {
                Saxon.Hej.om.NodeInfo realParent = node.getParent();
                if ((realParent != null)) {
                    parent = makeWrapper(realParent, (Saxon.Hej.tree.wrapper.SpaceStrippedDocument)(docWrapper), null);
                }
            }
            return parent;
        }
        public override Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
            if (((((nodeTest is Saxon.Hej.pattern.NodeTest) && (((Saxon.Hej.pattern.NodeTest)(nodeTest)).getUType().intersection(Saxon.Hej.type.UType.TEXT) == Saxon.Hej.type.UType.VOID)) || (axisNumber == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) || (axisNumber == Saxon.Hej.om.AxisInfo.NAMESPACE))) {
                return new Saxon.Hej.tree.wrapper.WrappingIterator(node.iterateAxis(axisNumber, nodeTest), this, getParentForAxis(axisNumber));
            } else {
                return new Saxon.Hej.tree.wrapper.SpaceStrippedNode.StrippingIterator(node.iterateAxis(axisNumber, nodeTest), (Saxon.Hej.tree.wrapper.SpaceStrippedDocument)(docWrapper), getParentForAxis(axisNumber));
            }
        }
        public override Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
                return new Saxon.Hej.tree.wrapper.WrappingIterator(node.iterateAxis(axisNumber), this, this);
              case Saxon.Hej.om.AxisInfo.CHILD:
                return new Saxon.Hej.tree.wrapper.SpaceStrippedNode.StrippingIterator(node.iterateAxis(axisNumber), (Saxon.Hej.tree.wrapper.SpaceStrippedDocument)(docWrapper), this);
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                Saxon.Hej.tree.wrapper.SpaceStrippedNode parent = (Saxon.Hej.tree.wrapper.SpaceStrippedNode)(getParent());
                if ((parent == null)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                } else {
                    return new Saxon.Hej.tree.wrapper.SpaceStrippedNode.StrippingIterator(node.iterateAxis(axisNumber), (Saxon.Hej.tree.wrapper.SpaceStrippedDocument)(docWrapper), parent);
                }
              default:
                return new Saxon.Hej.tree.wrapper.SpaceStrippedNode.StrippingIterator(node.iterateAxis(axisNumber), (Saxon.Hej.tree.wrapper.SpaceStrippedDocument)(docWrapper), null);
            }
        }
        private Saxon.Hej.tree.wrapper.SpaceStrippedNode getParentForAxis(int axisNumber) {
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
                return this;
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                return (Saxon.Hej.tree.wrapper.SpaceStrippedNode)(getParent());
              default:
                return null;
            }
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.@event.Receiver temp = @out;
            if ((node.getTreeInfo().isTyped() && !(Saxon.Hej.om.CopyOptions.includes(copyOptions, Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS)))) {
                temp = new Saxon.Eej.ee.validate.SkipValidator(@out);
                copyOptions |= Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS;
            }
            Saxon.Hej.@event.Stripper stripper = new Saxon.Hej.@event.Stripper(((Saxon.Hej.tree.wrapper.SpaceStrippedDocument)(docWrapper)).getStrippingRule(), temp);
            node.copy(stripper, copyOptions, locationId);
        }
        private class StrippingIterator : Saxon.Hej.tree.iter.AxisIterator {
            internal Saxon.Hej.tree.iter.AxisIterator @base;
            internal Saxon.Hej.tree.wrapper.SpaceStrippedNode parent;
            internal Saxon.Hej.om.NodeInfo currentVirtualNode;
            internal Saxon.Hej.tree.wrapper.SpaceStrippedDocument docWrapper;
            internal int position;
            public StrippingIterator(Saxon.Hej.tree.iter.AxisIterator @base, Saxon.Hej.tree.wrapper.SpaceStrippedDocument docWrapper, Saxon.Hej.tree.wrapper.SpaceStrippedNode parent)  {
                this.@base = @base;
                this.docWrapper = docWrapper;
                this.parent = parent;
                position = 0;
            }
            public Saxon.Hej.om.Item next() {
                Saxon.Hej.om.NodeInfo nextRealNode;
                do {
                    nextRealNode = ((/*1430*/Saxon.Hej.om.NodeInfo)@base.next());
                    if ((nextRealNode == null)) {
                        return null;
                    }
                } while (!(isPreserved(nextRealNode)));
                currentVirtualNode = makeWrapper(nextRealNode, docWrapper, parent);
                position++;
                return currentVirtualNode;
            }
            private bool isPreserved(Saxon.Hej.om.NodeInfo nextRealNode) {
                if ((nextRealNode.getNodeKind() != Saxon.Hej.type.Type.TEXT)) {
                    return true;
                }
                Saxon.Hej.om.NodeInfo actualParent = ((parent == null) ? nextRealNode.getParent() : parent.node);
                return isPreservedNode(nextRealNode, docWrapper, actualParent);
            }
            public void close() {
                @base.close();
            }
        
        }
    
    }
}