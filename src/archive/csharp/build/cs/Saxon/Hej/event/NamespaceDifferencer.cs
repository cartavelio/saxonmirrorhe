using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class NamespaceDifferencer : Saxon.Hej.@event.ProxyReceiver {
        private bool undeclareNamespaces = false;
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap> namespaceStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap>();
        private Saxon.Hej.om.NodeName currentElement;
        public NamespaceDifferencer(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties details) : base (next) {
            undeclareNamespaces = "yes".Equals(details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.UNDECLARE_PREFIXES));
            namespaceStack.Push(Saxon.Hej.om.NamespaceMap.emptyMap());
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            currentElement = elemName;
            Saxon.Hej.om.NamespaceMap parentMap = namespaceStack.Peek();
            namespaceStack.Push(namespaces);
            Saxon.Hej.om.NamespaceMap delta = getDifferences(namespaces, parentMap, currentElement.hasURI(Saxon.Hej.om.NamespaceUri.NULL));
            nextReceiver.startElement(elemName, type, attributes, delta, location, properties);
        }
        public override void endElement() {
            namespaceStack.Pop();
            base.endElement();
        }
        private Saxon.Hej.om.NamespaceMap getDifferences(Saxon.Hej.om.NamespaceMap thisMap, Saxon.Hej.om.NamespaceMap parentMap, bool elementInDefaultNamespace) {
            if ((thisMap != parentMap)) {
                Saxon.Hej.om.NamespaceMap delta = Saxon.Hej.om.NamespaceDeltaMap.emptyMap();
                foreach (Saxon.Hej.om.NamespaceBinding nb in thisMap) {
                    Saxon.Hej.om.NamespaceUri parentUri = parentMap.getNamespaceUri(nb.getPrefix());
                    if ((parentUri == null)) {
                        delta = delta.put(nb.getPrefix(), nb.getNamespaceUri());
                    } else {
                        if (!(parentUri.Equals(nb.getNamespaceUri()))) {
                            delta = delta.put(nb.getPrefix(), nb.getNamespaceUri());
                        }
                    }
                }
                if (undeclareNamespaces) {
                    foreach (Saxon.Hej.om.NamespaceBinding nb in parentMap) {
                        if ((thisMap.getNamespaceUri(nb.getPrefix()) == null)) {
                            delta = delta.put(nb.getPrefix(), Saxon.Hej.om.NamespaceUri.NULL);
                        }
                    }
                } else {
                    if ((!(parentMap.getDefaultNamespace().isEmpty()) && thisMap.getDefaultNamespace().isEmpty())) {
                        delta = delta.put("", Saxon.Hej.om.NamespaceUri.NULL);
                    }
                }
                return delta;
            }
            return Saxon.Hej.om.NamespaceMap.emptyMap();
        }
    
    }
}