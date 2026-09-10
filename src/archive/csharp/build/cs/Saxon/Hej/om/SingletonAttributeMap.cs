using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class SingletonAttributeMap : Saxon.Hej.om.AttributeInfo, Saxon.Hej.om.AttributeMap {
        private SingletonAttributeMap(Saxon.Hej.om.NodeName nodeName, Saxon.Hej.type.SimpleType type, string value, Saxon.Hej.s9api.Location location, int properties) : base (nodeName, type, value, location, properties) {
        }
        public static Saxon.Hej.om.SingletonAttributeMap of(Saxon.Hej.om.AttributeInfo att) {
            if ((att is Saxon.Hej.om.SingletonAttributeMap)) {
                return (Saxon.Hej.om.SingletonAttributeMap)(att);
            } else {
                return new Saxon.Hej.om.SingletonAttributeMap(att.getNodeName(), att.getType(), att.getValue(), att.getLocation(), att.getProperties());
            }
        }
        public int size() {
            return 1;
        }
        public Saxon.Hej.om.AttributeInfo get(Saxon.Hej.om.NodeName name) {
            return (name.Equals(getNodeName()) ? this : null);
        }
        public Saxon.Hej.om.AttributeInfo get(Saxon.Hej.om.NamespaceUri uri, string local) {
            return ((getNodeName().getLocalPart().Equals(local) && getNodeName().hasURI(uri)) ? this : null);
        }
        public Saxon.Hej.om.AttributeInfo getByFingerprint(int fingerprint, Saxon.Hej.om.NamePool namePool) {
            return ((getNodeName().obtainFingerprint(namePool) == fingerprint) ? this : null);
        }
        public Saxon.Hej.om.AttributeMap put(Saxon.Hej.om.AttributeInfo att) {
            if (getNodeName().Equals(att.getNodeName())) {
                return Saxon.Hej.om.SingletonAttributeMap.of(att);
            } else {
                System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(2);
                list.Add(this);
                list.Add(att);
                return new Saxon.Hej.om.SmallAttributeMap(list);
            }
        }
        public Saxon.Hej.om.AttributeMap remove(Saxon.Hej.om.NodeName name) {
            if (name.Equals(getNodeName())) {
                return Saxon.Hej.om.EmptyAttributeMap.getInstance();
            } else {
                return this;
            }
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.om.AttributeInfo> GetEnumerator() {
            return new Saxon.Hej.tree.jiter.MonoIterator<Saxon.Hej.om.AttributeInfo>(this);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public Saxon.Hej.om.AttributeMap apply(System.Func<Saxon.Hej.om.AttributeInfo,Saxon.Hej.om.AttributeInfo> mapper) {
            return Saxon.Hej.om.SingletonAttributeMap.of(mapper(this));
        }
        public System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> asList() {
            System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(1);
            list.Add(this);
            return list;
        }
        public Saxon.Hej.om.AttributeInfo itemAt(int index) {
            if ((index == 0)) {
                return this;
            } else {
                throw new System.IndexOutOfRangeException((index+" of 1"));
            }
        }
    
        /* Expanding default method getValue from interface AttributeMap */
        public virtual string getValue(Saxon.Hej.om.NamespaceUri uri, string local) {
            Saxon.Hej.om.AttributeInfo att = get(uri, local);
            return ((att == null) ? null : att.getValue());
        }

        /* Expanding default method getValue from interface AttributeMap */
        public virtual string getValue(string local) {
            Saxon.Hej.om.AttributeInfo att = get(Saxon.Hej.om.NamespaceUri.NULL, local);
            return ((att == null) ? null : att.getValue());
        }

        /* Expanding default method verify from interface AttributeMap */
        public virtual void verify() {
        }

    }
}