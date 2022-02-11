using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class EmptyAttributeMap : Saxon.Hej.om.AttributeMap {
        private static readonly Saxon.Hej.om.EmptyAttributeMap THE_INSTANCE = new Saxon.Hej.om.EmptyAttributeMap();
        private EmptyAttributeMap()  {
        }
        public static Saxon.Hej.om.EmptyAttributeMap getInstance() {
            return THE_INSTANCE;
        }
        public int size() {
            return 0;
        }
        public Saxon.Hej.om.AttributeInfo get(Saxon.Hej.om.NodeName name) {
            return null;
        }
        public Saxon.Hej.om.AttributeInfo get(string uri, string local) {
            return null;
        }
        public Saxon.Hej.om.AttributeInfo getByFingerprint(int fingerprint, Saxon.Hej.om.NamePool namePool) {
            return null;
        }
        public Saxon.Hej.om.AttributeMap put(Saxon.Hej.om.AttributeInfo att) {
            return Saxon.Hej.om.SingletonAttributeMap.of(att);
        }
        public Saxon.Hej.om.AttributeMap remove(Saxon.Hej.om.NodeName name) {
            return this;
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.om.AttributeInfo> GetEnumerator() {
            return new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>().GetEnumerator();;
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public Saxon.Hej.om.AttributeMap apply(System.Func<Saxon.Hej.om.AttributeInfo,Saxon.Hej.om.AttributeInfo> mapper) {
            return this;
        }
        public Saxon.Hej.om.AttributeInfo itemAt(int index) {
            throw new System.IndexOutOfRangeException();
        }
    
        /* Expanding default method getValue from interface AttributeMap */
        public virtual string getValue(string uri, string local) {
            Saxon.Hej.om.AttributeInfo att = get(uri, local);
            return ((att == null) ? null : att.getValue());
        }

        /* Expanding default method getValue from interface AttributeMap */
        public virtual string getValue(string local) {
            Saxon.Hej.om.AttributeInfo att = get("", local);
            return ((att == null) ? null : att.getValue());
        }

        /* Expanding default method verify from interface AttributeMap */
        public virtual void verify() {
        }

        /* Expanding default method asList from interface AttributeMap */
        public virtual System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> asList() {
            System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(size());
            foreach (Saxon.Hej.om.AttributeInfo a in this) {
                list.Add(a);
            }
            return list;
        }

    }
}