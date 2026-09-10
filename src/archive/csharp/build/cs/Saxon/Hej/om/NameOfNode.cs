using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class NameOfNode : Saxon.Hej.om.NodeName {
        private readonly Saxon.Hej.om.NodeInfo node;
        private NameOfNode(Saxon.Hej.om.NodeInfo node)  {
            this.node = node;
        }
        public static Saxon.Hej.om.NodeName makeName(Saxon.Hej.om.NodeInfo node) {
            if ((node is Saxon.Hej.om.MutableNodeInfo)) {
                return new Saxon.Hej.om.FingerprintedQName(node.getPrefix(), node.getNamespaceUri(), node.getLocalPart());
            } else {
                if ((node is Saxon.Hej.tree.wrapper.AbstractVirtualNode)) {
                    return new Saxon.Hej.om.NameOfNode(((/*1430*/Saxon.Hej.om.NodeInfo)((Saxon.Hej.tree.wrapper.AbstractVirtualNode)(node)).getUnderlyingNode()));
                } else {
                    return new Saxon.Hej.om.NameOfNode(node);
                }
            }
        }
        public string getPrefix() {
            return node.getPrefix();
        }
        public Saxon.Hej.om.NamespaceUri getNamespaceUri() {
            return node.getNamespaceUri();
        }
        public string getLocalPart() {
            return node.getLocalPart();
        }
        public string getDisplayName() {
            return node.getDisplayName();
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return new Saxon.Hej.om.StructuredQName(getPrefix(), getNamespaceUri(), getLocalPart());
        }
        public bool hasURI(Saxon.Hej.om.NamespaceUri ns) {
            if ((node is Saxon.Hej.tree.tiny.TinyNodeImpl)) {
                return ((Saxon.Hej.tree.tiny.TinyNodeImpl)(node)).hasURI(ns);
            }
            return node.getNamespaceUri().Equals(ns);
        }
        public Saxon.Hej.om.NamespaceBinding getNamespaceBinding() {
            return new Saxon.Hej.om.NamespaceBinding(getPrefix(), getNamespaceUri());
        }
        public bool hasFingerprint() {
            return node.hasFingerprint();
        }
        public int getFingerprint() {
            if (hasFingerprint()) {
                return node.getFingerprint();
            } else {
                return -1;
            }
        }
        public int obtainFingerprint(Saxon.Hej.om.NamePool namePool) {
            if (node.hasFingerprint()) {
                return node.getFingerprint();
            } else {
                return namePool.allocateFingerprint(node.getNamespaceUri(), node.getLocalPart());
            }
        }
        public override int GetHashCode() {
            return Saxon.Hej.om.StructuredQName.computeHashCode(getNamespaceUri(), getLocalPart());
        }
        public override bool Equals(System.Object obj) {
            if ((obj is Saxon.Hej.om.NodeName)) {
                Saxon.Hej.om.NodeName n = (Saxon.Hej.om.NodeName)(obj);
                if ((node.hasFingerprint() && n.hasFingerprint())) {
                    return (node.getFingerprint() == n.getFingerprint());
                } else {
                    return (n.getLocalPart().Equals(node.getLocalPart()) && n.hasURI(node.getNamespaceUri()));
                }
            } else {
                return false;
            }
        }
        public bool isIdentical(Saxon.Hej.om.IdentityComparable other) {
            return (((other is Saxon.Hej.om.NodeName) && this.Equals(other)) && this.getPrefix().Equals(((Saxon.Hej.om.NodeName)(other)).getPrefix()));
        }
        public int identityHashCode() {
            return (GetHashCode()^getPrefix().GetHashCode());
        }
    
        /* Expanding default method getURI from interface NodeName */
        public virtual string getURI() {
            return getNamespaceUri().ToString();
        }

    }
}