using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class AttributeMapWithIdentity : Saxon.Hej.om.AttributeMap {
    // Injecting C# code
                public System.Collections.Generic.IEnumerator<Saxon.Hej.om.AttributeInfo> GetEnumerator() {
                    foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                        if (att is not Saxon.Hej.om.AttributeInfo.Deleted) {
                            yield return att;
                        }
                    }
                }
        
                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                    return GetEnumerator();
                }
    // End of injected C# code
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> attributes;
        internal AttributeMapWithIdentity(System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> attributes)  {
            this.attributes = attributes;
        }
        public int size() {
            int count = 0;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                if (!(((att is Saxon.Hej.om.AttributeInfo.Deleted)))) {
                    count++;
                }
            }
            return count;
        }
        public Saxon.Hej.tree.iter.AxisIterator iterateAttributes(Saxon.Hej.tree.linked.ElementImpl owner) {
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>(attributes.Count);
            for (int i = 0; (i<attributes.Count); i++) {
                Saxon.Hej.om.AttributeInfo att = attributes[i];
                if (!(((att is Saxon.Hej.om.AttributeInfo.Deleted)))) {
                    list.Add(new Saxon.Hej.tree.linked.AttributeImpl(owner, i));
                }
            }
            return new Saxon.Hej.tree.iter.NodeListIterator(list);
        }
        private bool isDeleted(Saxon.Hej.om.AttributeInfo info) {
            return ((info is Saxon.Hej.om.AttributeInfo.Deleted));
        }
        public Saxon.Hej.om.AttributeInfo get(Saxon.Hej.om.NodeName name) {
            foreach (Saxon.Hej.om.AttributeInfo info in attributes) {
                if ((info.getNodeName().Equals(name) && !(((info is Saxon.Hej.om.AttributeInfo.Deleted))))) {
                    return info;
                }
            }
            return null;
        }
        public Saxon.Hej.om.AttributeInfo get(Saxon.Hej.om.NamespaceUri uri, string local) {
            foreach (Saxon.Hej.om.AttributeInfo info in attributes) {
                Saxon.Hej.om.NodeName name = info.getNodeName();
                if (((name.getLocalPart().Equals(local) && name.hasURI(uri)) && !(((info is Saxon.Hej.om.AttributeInfo.Deleted))))) {
                    return info;
                }
            }
            return null;
        }
        public Saxon.Hej.tree.linked.AttributeMapWithIdentity set(int index, Saxon.Hej.om.AttributeInfo info) {
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> newList = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(attributes);
            if (((index>=0) && (index<attributes.Count))) {
                newList[index] = info;
            } else {
                if ((index == attributes.Count)) {
                    newList.Add(info);
                }
            }
            return new Saxon.Hej.tree.linked.AttributeMapWithIdentity(newList);
        }
        public Saxon.Hej.tree.linked.AttributeMapWithIdentity add(Saxon.Hej.om.AttributeInfo info) {
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> newList = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(attributes);
            newList.Add(info);
            return new Saxon.Hej.tree.linked.AttributeMapWithIdentity(newList);
        }
        public Saxon.Hej.tree.linked.AttributeMapWithIdentity remove(int index) {
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> newList = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(attributes);
            if (((index>=0) && (index<attributes.Count))) {
                Saxon.Hej.om.AttributeInfo.Deleted del = new Saxon.Hej.om.AttributeInfo.Deleted(attributes[index]);
                newList[index] = del;
            }
            return new Saxon.Hej.tree.linked.AttributeMapWithIdentity(newList);
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
        public Saxon.Hej.om.AttributeInfo itemAt(int index) {
            return attributes[index];
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