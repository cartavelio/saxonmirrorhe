using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class NamespaceReducer : Saxon.Hej.@event.ProxyReceiver, Saxon.Hej.om.NamespaceResolver {
        private Saxon.Hej.om.NamespaceBinding[] namespaces = new Saxon.Hej.om.NamespaceBinding[50];
        private int namespacesSize = 0;
        private int[] countStack = new int[50];
        private int depth = 0;
        private bool[] disinheritStack = new bool[50];
        private Saxon.Hej.om.NamespaceBinding[] pendingUndeclarations = null;
        public NamespaceReducer(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaceMap, Saxon.Hej.s9api.Location location, int properties) {
            nextReceiver.startElement(elemName, type, attributes, namespaceMap, location, properties);
            if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.REFUSE_NAMESPACES)) {
                pendingUndeclarations = Saxon.Ejava.util.Arrays.copyOf(namespaces, namespacesSize);
            } else {
                if (((depth>0) && disinheritStack[(depth-1)])) {
                    System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> undeclarations = new System.Collections.Generic.List<Saxon.Hej.om.NamespaceBinding>(namespacesSize);
                    int k = namespacesSize;
                    for (int d = (depth-1); (d>=0); d--) {
                        if (!(disinheritStack[d])) {
                            break ;
                        }
                        for (int i = 0; (i<countStack[d]); i++) {
                            undeclarations.Add(namespaces[--k]);
                        }
                    }
                    pendingUndeclarations = undeclarations.ToArray();
                } else {
                    pendingUndeclarations = null;
                }
            }
            countStack[depth] = 0;
            disinheritStack[depth] = Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.DISINHERIT_NAMESPACES);
            if ((++depth>=countStack.Length)) {
                countStack = Saxon.Ejava.util.Arrays.copyOf(countStack, (depth*2));
                disinheritStack = Saxon.Ejava.util.Arrays.copyOf(disinheritStack, (depth*2));
            }
        }
        private bool isNeeded(Saxon.Hej.om.NamespaceBinding nsBinding) {
            if (nsBinding.isXmlNamespace()) {
                return false;
            }
            string prefix = nsBinding.getPrefix();
            if ((pendingUndeclarations != null)) {
                for (int p = 0; (p<pendingUndeclarations.Length); p++) {
                    Saxon.Hej.om.NamespaceBinding nb = pendingUndeclarations[p];
                    if (((nb != null) && prefix.Equals(nb.getPrefix()))) {
                        pendingUndeclarations[p] = null;
                    }
                }
            }
            for (int i = (namespacesSize-1); (i>=0); i--) {
                if (namespaces[i].Equals(nsBinding)) {
                    return false;
                }
                if (namespaces[i].getPrefix().Equals(nsBinding.getPrefix())) {
                    return true;
                }
            }
            return !(nsBinding.isDefaultUndeclaration());
        }
        private void addToStack(Saxon.Hej.om.NamespaceBinding nsBinding) {
            if (((namespacesSize+1)>=namespaces.Length)) {
                namespaces = Saxon.Ejava.util.Arrays.copyOf(namespaces, (namespacesSize*2));
            }
            namespaces[namespacesSize++] = nsBinding;
        }
        public bool isDisinheritingNamespaces() {
            return ((depth>0) && disinheritStack[(depth-1)]);
        }
        public override void endElement() {
            if ((depth-- == 0)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Attempt to output end tag with no matching start tag");
            }
            namespacesSize -= countStack[depth];
            nextReceiver.endElement();
        }
        public string getURIForPrefix(string prefix, bool useDefault) {
            if ((prefix.Length==0 && !(useDefault))) {
                return Saxon.Hej.lib.NamespaceConstant.NULL;
            } else {
                if ("xml".Equals(prefix)) {
                    return Saxon.Hej.lib.NamespaceConstant.XML;
                } else {
                    for (int i = (namespacesSize-1); (i>=0); i--) {
                        if (namespaces[i].getPrefix().Equals(prefix)) {
                            return namespaces[i].getURI();
                        }
                    }
                }
            }
            return (prefix.Length==0 ? Saxon.Hej.lib.NamespaceConstant.NULL : null);
        }
        public System.Collections.Generic.IEnumerator<string> iteratePrefixes() {
            System.Collections.Generic.IList<string> prefixes = new System.Collections.Generic.List<string>(namespacesSize);
            for (int i = (namespacesSize-1); (i>=0); i--) {
                string prefix = namespaces[i].getPrefix();
                if (!(prefixes.Contains(prefix))) {
                    prefixes.Add(prefix);
                }
            }
            prefixes.Add("xml");
            return prefixes.GetEnumerator();
        }
    
    }
}