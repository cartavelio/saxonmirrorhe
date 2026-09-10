using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.om {
    public class FleetingElementNode : Saxon.Eej.ee.stream.om.FleetingParentNode {
        private Saxon.Hej.om.NamespaceMap inScopeNamespaces;
        private Saxon.Hej.om.AttributeMap attributeMap;
        public FleetingElementNode(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType type)  {
            setNodeKind(Saxon.Hej.type.Type.ELEMENT);
            setNodeName(name);
            setTypeAnnotation(type);
        }
        public FleetingElementNode(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces)  {
            setNodeKind(Saxon.Hej.type.Type.ELEMENT);
            setNodeName(name);
            setTypeAnnotation(type);
            setAttributes(attributes);
            setNamespaceMap(namespaces);
        }
        public void setNamespaceMap(Saxon.Hej.om.NamespaceMap namespaces) {
            this.inScopeNamespaces = namespaces;
        }
        public override Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return inScopeNamespaces;
        }
        public void setAttributes(Saxon.Hej.om.AttributeMap attributes) {
            this.attributeMap = attributes;
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException((("copy() is not supported for streamed "+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" element node"));
        }
        public override Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                if ((attributeMap.size() == 0)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                } else {
                    if ((nodeTest is Saxon.Hej.pattern.NameTest)) {
                        Saxon.Hej.om.AttributeInfo att = attributeMap.getByFingerprint(((Saxon.Hej.pattern.NameTest)(nodeTest)).getFingerprint(), getConfiguration().getNamePool());
                        return ((att == null) ? Saxon.Hej.tree.iter.EmptyIterator.ofNodes() : Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(makeAttributeNode(att)));
                    } else {
                        Saxon.Eej.ee.stream.om.AttributeIterator @base = new Saxon.Eej.ee.stream.om.AttributeIterator(this, attributeMap);
                        return new Saxon.Hej.tree.util.Navigator.AxisFilter(@base, nodeTest);
                    }
                }
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
                return Saxon.Hej.tree.NamespaceNode.makeIterator(this, nodeTest);
              default:
                return base.iterateAxis(axisNumber, nodeTest);
            }
        }
        public Saxon.Eej.ee.stream.om.FleetingNode makeAttributeNode(Saxon.Hej.om.AttributeInfo attInfo) {
            Saxon.Ejava.util.Objects.requireNonNull(attInfo);
            Saxon.Eej.ee.stream.om.FleetingNode attr = new Saxon.Eej.ee.stream.om.FleetingNode();
            attr.setParent(this);
            attr.setNodeKind(Saxon.Hej.type.Type.ATTRIBUTE);
            attr.setNodeName(attInfo.getNodeName());
            attr.setTypeAnnotation(attInfo.getType());
            attr.setStringValue(Saxon.Hej.str.StringView.tidy(attInfo.getValue()));
            return attr;
        }
        public Saxon.Hej.om.AttributeMap getAttributes() {
            return attributeMap;
        }
        public override Saxon.Hej.om.NamespaceMap getInScopeNamespaces() {
            return inScopeNamespaces;
        }
        public override string getAttributeValue(string uri, string local) {
            return attributeMap.getValue(uri, local);
        }
        public int getAttributeIndex(string uri, string local) {
            int i = 0;
            foreach (Saxon.Hej.om.AttributeInfo att in attributeMap) {
                Saxon.Hej.om.NodeName nn = att.getNodeName();
                if ((nn.hasURI(uri) && nn.getLocalPart().Equals(local))) {
                    return i;
                } else {
                    i++;
                }
            }
            return System.Int32.MaxValue;
        }
        public int compareAttributeOrder(Saxon.Hej.om.NodeName one, Saxon.Hej.om.NodeName two) {
            if (one.Equals(two)) {
                return 0;
            } else {
                foreach (Saxon.Hej.om.AttributeInfo att in attributeMap) {
                    Saxon.Hej.om.NodeName nn = att.getNodeName();
                    if (nn.Equals(one)) {
                        return -1;
                    } else {
                        if (nn.Equals(two)) {
                            return +(1);
                        }
                    }
                }
                throw new Saxon.Ejava.lang.IllegalStateException("Attribute not found in attribute list");
            }
        }
        public int getNamespaceNodeIndex(string prefix) {
            int index = 0;
            Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.NAMESPACE);
            Saxon.Hej.om.NodeInfo node;
            while (((node = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                index++;
                if (node.getLocalPart().Equals(prefix)) {
                    return index;
                }
            }
            return -1;
        }
        public override void notify(Saxon.Hej.@event.Receiver @out) {
            @out.startElement(getNodeName(), getSchemaType(), getAttributes(), getAllNamespaces(), this, Saxon.Hej.@event.ReceiverOption.NONE);
        }
    
    }
}