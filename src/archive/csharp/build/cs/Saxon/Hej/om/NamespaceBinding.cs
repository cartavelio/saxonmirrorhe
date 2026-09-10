using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public sealed class NamespaceBinding : Saxon.Hej.om.NamespaceBindingSet {
        private readonly string prefix;
        private readonly string uri;
        public readonly static Saxon.Hej.om.NamespaceBinding XML = new Saxon.Hej.om.NamespaceBinding("xml", Saxon.Hej.lib.NamespaceConstant.XML);
        public readonly static Saxon.Hej.om.NamespaceBinding DEFAULT_UNDECLARATION = new Saxon.Hej.om.NamespaceBinding("", "");
        public readonly static Saxon.Hej.om.NamespaceBinding[] EMPTY_ARRAY = new Saxon.Hej.om.NamespaceBinding[0];
        public NamespaceBinding(string prefix, string uri)  {
            this.prefix = prefix;
            this.uri = uri;
            if (((prefix == null) || (uri == null))) {
                throw new Saxon.Ejava.lang.NullPointerException();
            }
        }
        public string getURI(string prefix) {
            return (prefix.Equals(this.prefix) ? uri : null);
        }
        public static Saxon.Hej.om.NamespaceBinding makeNamespaceBinding(string prefix, string uri) {
            if (((prefix.Length == 0) && (uri.Length == 0))) {
                return DEFAULT_UNDECLARATION;
            } else {
                if ((prefix.Equals("xml") && uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                    return XML;
                } else {
                    return new Saxon.Hej.om.NamespaceBinding(prefix.ToString(), uri.ToString());
                }
            }
        }
        public string getPrefix() {
            return prefix;
        }
        public string getURI() {
            return uri;
        }
        public bool isXmlNamespace() {
            return prefix.Equals("xml");
        }
        public bool isDefaultUndeclaration() {
            return (prefix.Length==0 && uri.Length==0);
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.om.NamespaceBinding> GetEnumerator() {
            return new Saxon.Hej.tree.jiter.MonoIterator<Saxon.Hej.om.NamespaceBinding>(this);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Hej.om.NamespaceBinding) && prefix.Equals(((Saxon.Hej.om.NamespaceBinding)(obj)).getPrefix())) && uri.Equals(((Saxon.Hej.om.NamespaceBinding)(obj)).getURI()));
        }
        public override int GetHashCode() {
            return (prefix.GetHashCode()^uri.GetHashCode());
        }
        public override string ToString() {
            return ((prefix+"=")+uri);
        }
    
    }
}