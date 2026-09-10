using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class SmallAttributeMap : Saxon.Hej.om.AttributeMap {
        public const int LIMIT = 8;
        private readonly System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> attributes;
        public SmallAttributeMap(System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> attributes)  {
            this.attributes = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(attributes);
        }
        public int size() {
            return attributes.Count;
        }
        public Saxon.Hej.om.AttributeInfo get(Saxon.Hej.om.NodeName name) {
            foreach (Saxon.Hej.om.AttributeInfo info in attributes) {
                if (info.getNodeName().Equals(name)) {
                    return info;
                }
            }
            return null;
        }
        public Saxon.Hej.om.AttributeInfo get(string uri, string local) {
            foreach (Saxon.Hej.om.AttributeInfo info in attributes) {
                Saxon.Hej.om.NodeName name = info.getNodeName();
                if ((name.getLocalPart().Equals(local) && name.hasURI(uri))) {
                    return info;
                }
            }
            return null;
        }
        public Saxon.Hej.om.AttributeInfo getByFingerprint(int fingerprint, Saxon.Hej.om.NamePool namePool) {
            foreach (Saxon.Hej.om.AttributeInfo info in attributes) {
                Saxon.Hej.om.NodeName name = info.getNodeName();
                if ((name.obtainFingerprint(namePool) == fingerprint)) {
                    return info;
                }
            }
            return null;
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.om.AttributeInfo> GetEnumerator() {
            return attributes.GetEnumerator();
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> asList() {
            return attributes;
        }
        public Saxon.Hej.om.AttributeInfo itemAt(int index) {
            return attributes[index];
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

        /* Expanding default method put from interface AttributeMap */
        public virtual Saxon.Hej.om.AttributeMap put(Saxon.Hej.om.AttributeInfo att) {
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>((size()+1));
            foreach (Saxon.Hej.om.AttributeInfo a in this) {
                if (!(a.getNodeName().Equals(att.getNodeName()))) {
                    list.Add(a);
                }
            }
            list.Add(att);
            return Saxon.Hej.om.SequenceTool.attributeMapFromList(list);
        }

        /* Expanding default method remove from interface AttributeMap */
        public virtual Saxon.Hej.om.AttributeMap remove(Saxon.Hej.om.NodeName name) {
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(size());
            foreach (Saxon.Hej.om.AttributeInfo a in this) {
                if (!(a.getNodeName().Equals(name))) {
                    list.Add(a);
                }
            }
            return Saxon.Hej.om.SequenceTool.attributeMapFromList(list);
        }

        /* Expanding default method verify from interface AttributeMap */
        public virtual void verify() {
        }

        /* Expanding default method apply from interface AttributeMap */
        public virtual Saxon.Hej.om.AttributeMap apply(System.Func<Saxon.Hej.om.AttributeInfo,Saxon.Hej.om.AttributeInfo> mapper) {
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(size());
            foreach (Saxon.Hej.om.AttributeInfo a in this) {
                list.Add(mapper(a));
            }
            return Saxon.Hej.om.SequenceTool.attributeMapFromList(list);
        }

    }
}