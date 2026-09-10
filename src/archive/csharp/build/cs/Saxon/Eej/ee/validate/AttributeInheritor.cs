using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class AttributeInheritor : Saxon.Hej.@event.ProxyReceiver {
        private Saxon.Hej.om.AttributeMap bufferedAttributes;
        private readonly Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.om.AttributeMap> inheritedAttributeStack = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.om.AttributeMap>();
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NodeName> elementNameStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NodeName>();
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap> namespaceMapStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap>();
        private bool empty = true;
        private Saxon.Hej.om.NodeInfo elementNode;
        public AttributeInheritor(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            bufferedAttributes = attributes;
            inheritedAttributeStack.push(Saxon.Hej.om.EmptyAttributeMap.getInstance());
            elementNameStack.Push(elemName);
            namespaceMapStack.Push(namespaces);
            elementNode = null;
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void endElement() {
            inheritedAttributeStack.pop();
            elementNameStack.Pop();
            namespaceMapStack.Pop();
            base.endElement();
        }
        public void notifyInheritableAttribute(Saxon.Hej.om.NodeName nameCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            Saxon.Hej.om.AttributeMap atts = inheritedAttributeStack.pop();
            atts = atts.put(new Saxon.Hej.om.AttributeInfo(nameCode, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, value, location, properties));
            inheritedAttributeStack.push(atts);
            empty = false;
        }
        public Saxon.Hej.om.AttributeMap gatherInheritedAttributes(Saxon.Hej.om.AttributeMap initialAtts) {
            for (int i = (inheritedAttributeStack.size()-1); (i>=0); i--) {
                Saxon.Hej.om.AttributeMap ac = inheritedAttributeStack.get(i);
                foreach (Saxon.Hej.om.AttributeInfo att in ac) {
                    Saxon.Hej.om.NodeName nc = att.getNodeName();
                    if ((initialAtts.get(nc) == null)) {
                        Saxon.Hej.om.AttributeInfo newAtt = new Saxon.Hej.om.AttributeInfo(nc, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, att.getValue(), att.getLocation(), att.getProperties());
                        initialAtts = initialAtts.put(newAtt);
                    }
                }
            }
            return initialAtts;
        }
        public bool hasInheritedAttributes() {
            return !(empty);
        }
        internal Saxon.Hej.om.NodeInfo createSkeletonElementNode() {
            if ((elementNode == null)) {
                int len = bufferedAttributes.size();
                Saxon.Hej.tree.tiny.TinyBuilder builder = new Saxon.Hej.tree.tiny.TinyBuilder(getPipelineConfiguration());
                builder.setSystemId(systemId);
                Saxon.Hej.tree.tiny.Statistics stats = new Saxon.Hej.tree.tiny.Statistics(2, (len+2), 20, 16);
                builder.setStatistics(stats);
                builder.open();
                Saxon.Hej.om.AttributeMap allAttributes = bufferedAttributes;
                if (hasInheritedAttributes()) {
                    allAttributes = gatherInheritedAttributes(allAttributes);
                }
                builder.startElement(elementNameStack.Peek(), Saxon.Hej.type.Untyped.getInstance(), allAttributes, namespaceMapStack.Peek(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                builder.endElement();
                elementNode = builder.getCurrentRoot();
            }
            return elementNode;
        }
    
    }
}