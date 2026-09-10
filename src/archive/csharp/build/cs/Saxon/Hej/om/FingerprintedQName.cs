using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class FingerprintedQName : Saxon.Hej.om.NodeName {
        private readonly Saxon.Hej.om.StructuredQName qName;
        private int fingerprint = -1;
        public FingerprintedQName(string prefix, string uri, string localName)  {
            qName = new Saxon.Hej.om.StructuredQName(prefix, uri, localName);
        }
        public FingerprintedQName(string prefix, string uri, string localName, int fingerprint)  {
            qName = new Saxon.Hej.om.StructuredQName(prefix, uri, localName);
            this.fingerprint = fingerprint;
        }
        public FingerprintedQName(string prefix, string uri, string localName, Saxon.Hej.om.NamePool pool)  {
            qName = new Saxon.Hej.om.StructuredQName(prefix, uri, localName);
            this.fingerprint = pool.allocateFingerprint(uri, localName);
        }
        public FingerprintedQName(Saxon.Hej.om.StructuredQName qName, int fingerprint)  {
            this.qName = qName;
            this.fingerprint = fingerprint;
        }
        public FingerprintedQName(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.om.NamePool pool)  {
            this.qName = qName;
            this.fingerprint = pool.allocateFingerprint(qName.getURI(), qName.getLocalPart());
        }
        public static Saxon.Hej.om.FingerprintedQName fromClarkName(string expandedName) {
            string @namespace;
            string localName;
            if ((expandedName[0] == '{')) {
                int closeBrace = expandedName.IndexOf('}');
                if ((closeBrace<0)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("No closing '}' in Clark name");
                }
                @namespace = expandedName.Substring(1, closeBrace - 1);
                if ((closeBrace == expandedName.Length)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Missing local part in Clark name");
                }
                localName = expandedName.Substring((closeBrace+1));
            } else {
                @namespace = "";
                localName = expandedName;
            }
            return new Saxon.Hej.om.FingerprintedQName("", @namespace, localName);
        }
        public static Saxon.Hej.om.FingerprintedQName fromEQName(string expandedName) {
            string @namespace;
            string localName;
            if (expandedName.StartsWith("Q{")) {
                int closeBrace = expandedName.IndexOf('}', 2);
                if ((closeBrace<0)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("No closing '}' in EQName");
                }
                @namespace = expandedName.Substring(2, closeBrace - 2);
                if ((closeBrace == expandedName.Length)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Missing local part in EQName");
                }
                localName = expandedName.Substring((closeBrace+1));
            } else {
                @namespace = "";
                localName = expandedName;
            }
            return new Saxon.Hej.om.FingerprintedQName("", @namespace, localName);
        }
        public bool hasFingerprint() {
            return (fingerprint != -1);
        }
        public int getFingerprint() {
            return fingerprint;
        }
        public int obtainFingerprint(Saxon.Hej.om.NamePool pool) {
            if ((fingerprint == -1)) {
                fingerprint = pool.allocateFingerprint(getURI(), getLocalPart());
            }
            return fingerprint;
        }
        public string getDisplayName() {
            return qName.getDisplayName();
        }
        public string getPrefix() {
            return qName.getPrefix();
        }
        public string getURI() {
            return qName.getURI();
        }
        public string getLocalPart() {
            return qName.getLocalPart();
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return qName;
        }
        public bool hasURI(string ns) {
            return qName.hasURI(ns);
        }
        public Saxon.Hej.om.NamespaceBinding getNamespaceBinding() {
            return qName.getNamespaceBinding();
        }
        public int identityHashCode() {
            return 0;
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.om.NodeName)) {
                if (((fingerprint != -1) && ((Saxon.Hej.om.NodeName)(other)).hasFingerprint())) {
                    return (getFingerprint() == ((Saxon.Hej.om.NodeName)(other)).getFingerprint());
                } else {
                    return (getLocalPart().Equals(((Saxon.Hej.om.NodeName)(other)).getLocalPart()) && hasURI(((Saxon.Hej.om.NodeName)(other)).getURI()));
                }
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            return qName.GetHashCode();
        }
        public bool isIdentical(Saxon.Hej.om.IdentityComparable other) {
            return (((other is Saxon.Hej.om.NodeName) && this.Equals(other)) && this.getPrefix().Equals(((Saxon.Hej.om.NodeName)(other)).getPrefix()));
        }
        public override string ToString() {
            return qName.getDisplayName();
        }
    
    }
}