using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class LargeAttributeMap : Saxon.Hej.om.AttributeMap {
        private class AttributeInfoLink {
            internal Saxon.Hej.om.AttributeInfo payload;
            internal Saxon.Hej.om.NodeName prior;
            internal Saxon.Hej.om.NodeName next;
        
        }
        private System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.om.NodeName,Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink> attributes;
        private Saxon.Hej.om.NodeName first = null;
        private Saxon.Hej.om.NodeName last = null;
        private int _size;
        private LargeAttributeMap()  {
        }
        public LargeAttributeMap(System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> atts)  {
            System.Diagnostics.Debug.Assert(!(atts.Count==0));
            this.attributes = emptyMap();
            this._size = atts.Count;
            Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink current = null;
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                if ((Saxon.Impl.Helpers.MapUtils.Get(attributes, att.getNodeName()) != null)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Attribute map contains duplicates");
                }
                Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink link = new Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink();
                link.payload = att;
                if ((current == null)) {
                    first = att.getNodeName();
                } else {
                    current.next = att.getNodeName();
                    link.prior = current.payload.getNodeName();
                }
                current = link;
                attributes = attributes.SetItem(att.getNodeName(), link);
            }
            last = current.payload.getNodeName();
        }
        private LargeAttributeMap(System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.om.NodeName,Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink> attributes, int size, Saxon.Hej.om.NodeName first, Saxon.Hej.om.NodeName last)  {
            this.attributes = attributes;
            this._size = size;
            this.first = first;
            this.last = last;
        }
        private System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.om.NodeName,Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink> emptyMap() {
            return System.Collections.Immutable.ImmutableDictionary.Create<Saxon.Hej.om.NodeName,Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink>();
        }
        public int size() {
            return _size;
        }
        public Saxon.Hej.om.AttributeInfo get(Saxon.Hej.om.NodeName name) {
            Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink link = Saxon.Impl.Helpers.MapUtils.Get(attributes, name);
            return ((link == null) ? null : link.payload);
        }
        public Saxon.Hej.om.AttributeInfo get(Saxon.Hej.om.NamespaceUri uri, string local) {
            Saxon.Hej.om.NodeName name = new Saxon.Hej.om.FingerprintedQName("", uri, local);
            return get(name);
        }
        public Saxon.Hej.om.AttributeInfo getByFingerprint(int fingerprint, Saxon.Hej.om.NamePool namePool) {
            Saxon.Hej.om.NodeName name = new Saxon.Hej.om.FingerprintedQName(namePool.getStructuredQName(fingerprint), fingerprint);
            return get(name);
        }
        public Saxon.Hej.om.AttributeMap put(Saxon.Hej.om.AttributeInfo att) {
            Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink existing = Saxon.Impl.Helpers.MapUtils.Get(attributes, att.getNodeName());
            Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink link = new Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink();
            Saxon.Hej.om.NodeName last2 = last;
            link.payload = att;
            if ((existing == null)) {
                link.prior = last;
                last2 = att.getNodeName();
                Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink oldLast = Saxon.Impl.Helpers.MapUtils.Get(attributes, last);
                Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink penult = new Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink();
                penult.payload = oldLast.payload;
                penult.next = att.getNodeName();
                penult.prior = oldLast.prior;
                attributes = attributes.SetItem(last, penult);
            } else {
                link.prior = existing.prior;
                link.next = existing.next;
            }
            System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.om.NodeName,Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink> att2 = attributes.SetItem(att.getNodeName(), link);
            int size2 = ((existing == null) ? (_size+1) : _size);
            return new Saxon.Hej.om.LargeAttributeMap(att2, size2, first, last2);
        }
        public Saxon.Hej.om.AttributeMap remove(Saxon.Hej.om.NodeName name) {
            if ((Saxon.Impl.Helpers.MapUtils.Get(attributes, name) == null)) {
                return this;
            } else {
                Saxon.Hej.om.NodeName first2 = first;
                Saxon.Hej.om.NodeName last2 = last;
                System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.om.NodeName,Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink> att2 = attributes.Remove(name);
                Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink existing = Saxon.Impl.Helpers.MapUtils.Get(attributes, name);
                if ((existing.prior != null)) {
                    Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink priorLink = Saxon.Impl.Helpers.MapUtils.Get(attributes, existing.prior);
                    Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink priorLink2 = new Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink();
                    priorLink2.payload = priorLink.payload;
                    priorLink2.prior = priorLink.prior;
                    priorLink2.next = existing.next;
                    att2 = att2.SetItem(existing.prior, priorLink2);
                } else {
                    first2 = existing.next;
                }
                if ((existing.next != null)) {
                    Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink nextLink = Saxon.Impl.Helpers.MapUtils.Get(attributes, existing.next);
                    Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink nextLink2 = new Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink();
                    nextLink2.payload = nextLink.payload;
                    nextLink2.next = nextLink.next;
                    nextLink2.prior = existing.prior;
                    att2 = att2.SetItem(existing.next, nextLink2);
                } else {
                    last2 = existing.prior;
                }
                return new Saxon.Hej.om.LargeAttributeMap(att2, (_size-1), first2, last2);
            }
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.om.AttributeInfo> GetEnumerator() {
            return new Inner_Iterator_63(this);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> asList() { lock(this) {
            System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> result = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(_size);
            foreach (Saxon.Hej.om.AttributeInfo att in this) {
                result.Add(att);
            }
            return result;
        }}
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(256);
            foreach (Saxon.Hej.om.AttributeInfo att in this) {
                sb.append(att.getNodeName().getDisplayName()).append("=\"").append(att.getValue()).append("\" ");
            }
            return sb.ToString().Trim();
        }
    
        private class Inner_Iterator_63 : System.Collections.Generic.IEnumerator<Saxon.Hej.om.AttributeInfo> {
            private readonly LargeAttributeMap outerInstance;
            public Inner_Iterator_63(LargeAttributeMap outerInstance) {
                this.outerInstance = outerInstance;
                current = outerInstance.first;
            }
            internal Saxon.Hej.om.NodeName current;
            public bool hasNext() {
                return (/*field*/current != null);
            }
            public Saxon.Hej.om.AttributeInfo next() {
                Saxon.Hej.om.LargeAttributeMap.AttributeInfoLink link = Saxon.Impl.Helpers.MapUtils.Get(outerInstance.attributes, /*field*/current);
                /*field*/current = /*localvar*/link.next;
                return /*localvar*/link.payload;
            }
        
            object System.Collections.IEnumerator.Current => Current;
            public Saxon.Hej.om.AttributeInfo Current { get; private set; }
            public bool MoveNext() {
                if (hasNext()) {Current = next(); return true;} else return false;
            }
            public void Reset() {throw new NotImplementedException();}
            public void Dispose() {}
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

        /* Expanding default method apply from interface AttributeMap */
        public virtual Saxon.Hej.om.AttributeMap apply(System.Func<Saxon.Hej.om.AttributeInfo,Saxon.Hej.om.AttributeInfo> mapper) {
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(size());
            foreach (Saxon.Hej.om.AttributeInfo a in this) {
                list.Add(mapper(a));
            }
            return Saxon.Hej.om.SequenceTool.attributeMapFromList(list);
        }

        /* Expanding default method itemAt from interface AttributeMap */
        public virtual Saxon.Hej.om.AttributeInfo itemAt(int index) {
            return asList()[index];
        }

    }
}