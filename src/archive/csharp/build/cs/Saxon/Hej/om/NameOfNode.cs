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
                return new Saxon.Hej.om.FingerprintedQName(node.getPrefix(), node.getURI(), node.getLocalPart());
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
        public string getURI() {
            return node.getURI();
        }
        public string getLocalPart() {
            return node.getLocalPart();
        }
        public string getDisplayName() {
            return node.getDisplayName();
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return new Saxon.Hej.om.StructuredQName(getPrefix(), getURI(), getLocalPart());
        }
        public bool hasURI(string ns) {
            return node.getURI().Equals(ns);
        }
        public Saxon.Hej.om.NamespaceBinding getNamespaceBinding() {
            return Saxon.Hej.om.NamespaceBinding.makeNamespaceBinding(getPrefix(), getURI());
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
                return namePool.allocateFingerprint(node.getURI(), node.getLocalPart());
            }
        }
        public override int GetHashCode() {
            return Saxon.Hej.om.StructuredQName.computeHashCode(getURI(), getLocalPart());
        }
        public override bool Equals(System.Object obj) {
            if ((obj is Saxon.Hej.om.NodeName)) {
                Saxon.Hej.om.NodeName n = (Saxon.Hej.om.NodeName)(obj);
                if ((node.hasFingerprint() && n.hasFingerprint())) {
                    return (node.getFingerprint() == n.getFingerprint());
                } else {
                    return (n.getLocalPart().Equals(node.getLocalPart()) && n.hasURI(node.getURI()));
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
    
    }
}