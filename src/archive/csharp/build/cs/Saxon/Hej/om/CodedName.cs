using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class CodedName : Saxon.Hej.om.NodeName {
        private readonly int fingerprint;
        private readonly string prefix;
        private readonly Saxon.Hej.om.NamePool pool;
        public CodedName(int fingerprint, string prefix, Saxon.Hej.om.NamePool pool)  {
            this.fingerprint = fingerprint;
            this.prefix = prefix;
            this.pool = pool;
        }
        public string getPrefix() {
            return prefix;
        }
        public string getURI() {
            return pool.getURI(fingerprint);
        }
        public string getLocalPart() {
            return pool.getLocalName(fingerprint);
        }
        public string getDisplayName() {
            return (prefix.Length==0 ? getLocalPart() : ((prefix+":")+getLocalPart()));
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            Saxon.Hej.om.StructuredQName qn = pool.getUnprefixedQName(fingerprint);
            if (prefix.Length==0) {
                return qn;
            } else {
                return new Saxon.Hej.om.StructuredQName(prefix, qn.getURI(), qn.getLocalPart());
            }
        }
        public bool hasURI(string ns) {
            return getURI().Equals(ns);
        }
        public Saxon.Hej.om.NamespaceBinding getNamespaceBinding() {
            return new Saxon.Hej.om.NamespaceBinding(prefix, pool.getURI(fingerprint));
        }
        public bool hasFingerprint() {
            return true;
        }
        public int getFingerprint() {
            return fingerprint;
        }
        public int obtainFingerprint(Saxon.Hej.om.NamePool namePool) {
            return fingerprint;
        }
        public override int GetHashCode() {
            return Saxon.Hej.om.StructuredQName.computeHashCode(getURI(), getLocalPart());
        }
        public override bool Equals(System.Object obj) {
            if ((obj is Saxon.Hej.om.NodeName)) {
                Saxon.Hej.om.NodeName n = (Saxon.Hej.om.NodeName)(obj);
                if (n.hasFingerprint()) {
                    return (getFingerprint() == n.getFingerprint());
                } else {
                    return (n.getLocalPart().Equals(getLocalPart()) && n.hasURI(getURI()));
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
        public override string ToString() {
            return getDisplayName();
        }
    
    }
}