using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public sealed class NoNamespaceName : Saxon.Hej.om.NodeName {
        private readonly string localName;
        private int nameCode = -1;
        public NoNamespaceName(string localName)  {
            this.localName = localName;
        }
        public NoNamespaceName(string localName, int nameCode)  {
            this.localName = localName;
            this.nameCode = nameCode;
        }
        public string getPrefix() {
            return "";
        }
        public Saxon.Hej.om.NamespaceUri getNamespaceUri() {
            return Saxon.Hej.om.NamespaceUri.NULL;
        }
        public string getLocalPart() {
            return localName;
        }
        public string getDisplayName() {
            return localName;
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, getLocalPart());
        }
        public bool hasURI(Saxon.Hej.om.NamespaceUri ns) {
            return ns.isEmpty();
        }
        public Saxon.Hej.om.NamespaceBinding getNamespaceBinding() {
            return Saxon.Hej.om.NamespaceBinding.DEFAULT_UNDECLARATION;
        }
        public bool hasFingerprint() {
            return (nameCode != -1);
        }
        public int getFingerprint() {
            return (nameCode&Saxon.Hej.om.NamePool.FP_MASK);
        }
        public int obtainFingerprint(Saxon.Hej.om.NamePool namePool) {
            if ((nameCode == -1)) {
                return nameCode = namePool.allocateFingerprint(Saxon.Hej.om.NamespaceUri.NULL, localName);
            } else {
                return nameCode;
            }
        }
        public override int GetHashCode() {
            return Saxon.Hej.om.StructuredQName.computeHashCode(Saxon.Hej.om.NamespaceUri.NULL, localName);
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Hej.om.NodeName) && ((Saxon.Hej.om.NodeName)(obj)).getLocalPart().Equals(localName)) && ((Saxon.Hej.om.NodeName)(obj)).hasURI(Saxon.Hej.om.NamespaceUri.NULL));
        }
        public override string ToString() {
            return localName;
        }
        public bool isIdentical(Saxon.Hej.om.IdentityComparable other) {
            return (((other is Saxon.Hej.om.NodeName) && this.Equals(other)) && ((Saxon.Hej.om.NodeName)(other)).getPrefix().Length==0);
        }
        public int identityHashCode() {
            return (GetHashCode()^getPrefix().GetHashCode());
        }
    
        /* Expanding default method getURI from interface NodeName */
        public string getURI() {
            return getNamespaceUri().ToString();
        }

    }
}