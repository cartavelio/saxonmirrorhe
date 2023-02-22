using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class NamespaceMap : Saxon.Hej.om.NamespaceBindingSet, Saxon.Hej.om.NamespaceResolver {
        protected internal string[] prefixes;
        protected internal string[] uris;
        private static readonly string[] emptyArray = new string[]{};
        private static readonly Saxon.Hej.om.NamespaceMap EMPTY_MAP = new Saxon.Hej.om.NamespaceMap();
        public static Saxon.Hej.om.NamespaceMap emptyMap() {
            return EMPTY_MAP;
        }
        public static Saxon.Hej.om.NamespaceMap of(string prefix, string uri) {
            Saxon.Hej.om.NamespaceMap map = new Saxon.Hej.om.NamespaceMap();
            if (map.isPointlessMapping(prefix, uri)) {
                return EMPTY_MAP;
            }
            map.prefixes = new string[]{prefix};
            map.uris = new string[]{uri};
            return map;
        }
        protected internal NamespaceMap()  {
            prefixes = emptyArray;
            uris = emptyArray;
        }
        protected internal virtual Saxon.Hej.om.NamespaceMap makeNamespaceMap() {
            return new Saxon.Hej.om.NamespaceMap();
        }
        public NamespaceMap(System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> bindings)  {
            Saxon.Hej.om.NamespaceBinding[] bindingArray = bindings.ToArray();
            sortByPrefix(bindingArray);
            bool bindsXmlNamespace = false;
            prefixes = new string[bindingArray.Length];
            uris = new string[bindingArray.Length];
            for (int i = 0; (i<bindingArray.Length); i++) {
                prefixes[i] = bindingArray[i].getPrefix();
                uris[i] = bindingArray[i].getURI();
                if (prefixes[i].Equals("xml")) {
                    bindsXmlNamespace = true;
                    if (!(uris[i].Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException("Binds xml prefix to the wrong namespace");
                    }
                } else {
                    if (uris[i].Equals(Saxon.Hej.lib.NamespaceConstant.XML)) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException("Binds xml namespace to the wrong prefix");
                    }
                }
            }
            if (bindsXmlNamespace) {
                remove("xml");
            }
        }
        private void sortByPrefix(Saxon.Hej.om.NamespaceBinding[] bindingArray) {
            System.Array.Sort(bindingArray, (x, y) => System.String.Compare(x.getPrefix(), y.getPrefix(), System.StringComparison.Ordinal));
        }
        public static Saxon.Hej.om.NamespaceMap fromNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            System.Collections.Generic.IEnumerator<string> iter = resolver.iteratePrefixes();
            System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> bindings = new System.Collections.Generic.List<Saxon.Hej.om.NamespaceBinding>();
            while (iter.MoveNext()) {
                string prefix = iter.Current;
                string uri = resolver.getURIForPrefix(prefix, true);
                bindings.Add(new Saxon.Hej.om.NamespaceBinding(prefix, uri));
            }
            return new Saxon.Hej.om.NamespaceMap(bindings);
        }
        public virtual bool allowsNamespaceUndeclarations() {
            return false;
        }
        public int size() {
            return prefixes.Length;
        }
        public bool isEmpty() {
            return (prefixes.Length == 0);
        }
        public string getURI(string prefix) {
            if (prefix.Equals("xml")) {
                return Saxon.Hej.lib.NamespaceConstant.XML;
            }
            int position = Saxon.Ejava.util.Arrays.binarySearch(prefixes, prefix);
            return ((position>=0) ? uris[position] : null);
        }
        public string getDefaultNamespace() {
            if (((prefixes.Length>0) && prefixes[0].Length==0)) {
                return uris[0];
            } else {
                return "";
            }
        }
        public virtual Saxon.Hej.om.NamespaceMap put(string prefix, string uri) {
            if ((uri == null)) {
                uri = "";
            }
            if (isPointlessMapping(prefix, uri)) {
                return this;
            }
            int position = Saxon.Ejava.util.Arrays.binarySearch(prefixes, prefix);
            if ((position>=0)) {
                if (uris[position].Equals(uri)) {
                    return this;
                } else {
                    if (uri.Length==0) {
                        Saxon.Hej.om.NamespaceMap n2 = makeNamespaceMap();
                        if ((prefixes.Length>1)) {
                            n2.prefixes = new string[(prefixes.Length-1)];
                            Saxon.Ejava.lang.System.arraycopy(prefixes, 0, n2.prefixes, 0, position);
                            Saxon.Ejava.lang.System.arraycopy(prefixes, (position+1), n2.prefixes, position, ((prefixes.Length-position)-1));
                            n2.uris = new string[(uris.Length-1)];
                            Saxon.Ejava.lang.System.arraycopy(uris, 0, n2.uris, 0, position);
                            Saxon.Ejava.lang.System.arraycopy(uris, (position+1), n2.uris, position, ((uris.Length-position)-1));
                        }
                        return n2;
                    } else {
                        Saxon.Hej.om.NamespaceMap n2 = makeNamespaceMap();
                        n2.prefixes = Saxon.Ejava.util.Arrays.copyOf(prefixes, prefixes.Length);
                        n2.uris = Saxon.Ejava.util.Arrays.copyOf(uris, uris.Length);
                        n2.uris[position] = uri;
                        return n2;
                    }
                }
            } else {
                int insertionPoint = (-(position)-1);
                string[] p2 = new string[(prefixes.Length+1)];
                string[] u2 = new string[(uris.Length+1)];
                Saxon.Ejava.lang.System.arraycopy(prefixes, 0, p2, 0, insertionPoint);
                Saxon.Ejava.lang.System.arraycopy(uris, 0, u2, 0, insertionPoint);
                p2[insertionPoint] = prefix;
                u2[insertionPoint] = uri;
                Saxon.Ejava.lang.System.arraycopy(prefixes, insertionPoint, p2, (insertionPoint+1), (prefixes.Length-insertionPoint));
                Saxon.Ejava.lang.System.arraycopy(uris, insertionPoint, u2, (insertionPoint+1), (prefixes.Length-insertionPoint));
                Saxon.Hej.om.NamespaceMap n2 = makeNamespaceMap();
                n2.prefixes = p2;
                n2.uris = u2;
                return n2;
            }
        }
        private bool isPointlessMapping(string prefix, string uri) {
            if (prefix.Equals("xml")) {
                if (!(uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Invalid URI for xml prefix");
                }
                return true;
            } else {
                if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Invalid prefix for XML namespace");
                }
            }
            return false;
        }
        public Saxon.Hej.om.NamespaceMap bind(string prefix, string uri) {
            if (uri.Length==0) {
                return remove(prefix);
            } else {
                return put(prefix, uri);
            }
        }
        public virtual Saxon.Hej.om.NamespaceMap remove(string prefix) {
            int position = Saxon.Ejava.util.Arrays.binarySearch(prefixes, prefix);
            if ((position>=0)) {
                string[] p2 = new string[(prefixes.Length-1)];
                string[] u2 = new string[(uris.Length-1)];
                Saxon.Ejava.lang.System.arraycopy(prefixes, 0, p2, 0, position);
                Saxon.Ejava.lang.System.arraycopy(uris, 0, u2, 0, position);
                Saxon.Ejava.lang.System.arraycopy(prefixes, (position+1), p2, position, ((prefixes.Length-position)-1));
                Saxon.Ejava.lang.System.arraycopy(uris, (position+1), u2, position, ((uris.Length-position)-1));
                Saxon.Hej.om.NamespaceMap n2 = makeNamespaceMap();
                n2.prefixes = p2;
                n2.uris = u2;
                return n2;
            } else {
                return this;
            }
        }
        public Saxon.Hej.om.NamespaceMap putAll(Saxon.Hej.om.NamespaceMap delta) {
            if ((this == delta)) {
                return this;
            } else {
                if (isEmpty()) {
                    return delta;
                } else {
                    if (delta.isEmpty()) {
                        return this;
                    } else {
                        string[] p1 = prefixes;
                        string[] u1 = uris;
                        string[] p2 = delta.prefixes;
                        string[] u2 = delta.uris;
                        System.Collections.Generic.IList<string> p3 = new System.Collections.Generic.List<string>((p1.Length+p2.Length));
                        System.Collections.Generic.IList<string> u3 = new System.Collections.Generic.List<string>((p1.Length+p2.Length));
                        int i1 = 0;
                        int i2 = 0;
                        while (true) {
                            int c = System.String.Compare(p1[i1], p2[i2], System.StringComparison.Ordinal);
                            if ((c<0)) {
                                p3.Add(p1[i1]);
                                u3.Add(u1[i1]);
                                if ((++i1>=p1.Length)) {
                                    break ;
                                }
                            } else {
                                if ((c>0)) {
                                    p3.Add(p2[i2]);
                                    u3.Add(u2[i2]);
                                    if ((++i2>=p2.Length)) {
                                        break ;
                                    }
                                } else {
                                    p3.Add(p2[i2]);
                                    u3.Add(u2[i2]);
                                    i1++;
                                    i2++;
                                    if (((i1>=p1.Length) || (i2>=p2.Length))) {
                                        break ;
                                    }
                                }
                            }
                        }
                        while ((i1<p1.Length)) {
                            p3.Add(p1[i1]);
                            u3.Add(u1[i1]);
                            i1++;
                        }
                        while ((i2<p2.Length)) {
                            p3.Add(p2[i2]);
                            u3.Add(u2[i2]);
                            i2++;
                        }
                        Saxon.Hej.om.NamespaceMap n2 = new Saxon.Hej.om.NamespaceMap();
                        n2.prefixes = p3.ToArray();
                        n2.uris = u3.ToArray();
                        return n2;
                    }
                }
            }
        }
        public Saxon.Hej.om.NamespaceMap addAll(Saxon.Hej.om.NamespaceBindingSet namespaces) {
            if ((namespaces is Saxon.Hej.om.NamespaceMap)) {
                return putAll((Saxon.Hej.om.NamespaceMap)(namespaces));
            } else {
                Saxon.Hej.om.NamespaceMap map = this;
                foreach (Saxon.Hej.om.NamespaceBinding nb in namespaces) {
                    map = map.put(nb.getPrefix(), nb.getURI());
                }
                return map;
            }
        }
        public Saxon.Hej.om.NamespaceMap applyDifferences(Saxon.Hej.om.NamespaceDeltaMap delta) {
            if (delta.isEmpty()) {
                return this;
            } else {
                string[] p1 = prefixes;
                string[] u1 = uris;
                string[] p2 = delta.prefixes;
                string[] u2 = delta.uris;
                System.Collections.Generic.IList<string> prefixList = new System.Collections.Generic.List<string>((p1.Length+p2.Length));
                System.Collections.Generic.IList<string> uriList = new System.Collections.Generic.List<string>((p1.Length+p2.Length));
                int i1 = 0;
                int i2 = 0;
                while (((i1<p1.Length) && (i2<p2.Length))) {
                    int c = System.String.Compare(p1[i1], p2[i2], System.StringComparison.Ordinal);
                    if ((c<0)) {
                        prefixList.Add(p1[i1]);
                        uriList.Add(u1[i1]);
                        i1++;
                    } else {
                        if ((c>0)) {
                            if (!(u2[i2].Length==0)) {
                                prefixList.Add(p2[i2]);
                                uriList.Add(u2[i2]);
                            }
                            i2++;
                        } else {
                            if ((!(u2[i2].Length==0) || p2[i2].Length==0)) {
                                prefixList.Add(p2[i2]);
                                uriList.Add(u2[i2]);
                            }
                            i1++;
                            i2++;
                        }
                    }
                }
                while ((i1<p1.Length)) {
                    prefixList.Add(p1[i1]);
                    uriList.Add(u1[i1]);
                    i1++;
                }
                while ((i2<p2.Length)) {
                    if (!(u2[i2].Length==0)) {
                        prefixList.Add(p2[i2]);
                        uriList.Add(u2[i2]);
                    }
                    i2++;
                }
                Saxon.Hej.om.NamespaceMap n2 = new Saxon.Hej.om.NamespaceMap();
                n2.prefixes = prefixList.ToArray();
                n2.uris = uriList.ToArray();
                return n2;
            }
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.om.NamespaceBinding> GetEnumerator() {
            return new Inner_Iterator_169(this);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public Saxon.Hej.om.NamespaceBinding[] getNamespaceBindings() {
            Saxon.Hej.om.NamespaceBinding[] result = new Saxon.Hej.om.NamespaceBinding[prefixes.Length];
            for (int i = 0; (i<prefixes.Length); i++) {
                result[i] = new Saxon.Hej.om.NamespaceBinding(prefixes[i], uris[i]);
            }
            return result;
        }
        public Saxon.Hej.om.NamespaceBinding[] getDifferences(Saxon.Hej.om.NamespaceMap other, bool addUndeclarations) {
            System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> result = new System.Collections.Generic.List<Saxon.Hej.om.NamespaceBinding>();
            int i = 0, j = 0;
            while (true) {
                if (((i<prefixes.Length) && (j<other.prefixes.Length))) {
                    int c = System.String.Compare(prefixes[i], other.prefixes[j], System.StringComparison.Ordinal);
                    if ((c<0)) {
                        result.Add(new Saxon.Hej.om.NamespaceBinding(prefixes[i], uris[i]));
                        i++;
                    } else {
                        if ((c == 0)) {
                            if (uris[i].Equals(other.uris[j])) {
                            } else {
                                result.Add(new Saxon.Hej.om.NamespaceBinding(prefixes[i], uris[i]));
                            }
                            i++;
                            j++;
                        } else {
                            if ((addUndeclarations || other.prefixes[i].Length==0)) {
                                result.Add(new Saxon.Hej.om.NamespaceBinding(other.prefixes[j], ""));
                            }
                            j++;
                        }
                    }
                } else {
                    if ((i<prefixes.Length)) {
                        result.Add(new Saxon.Hej.om.NamespaceBinding(prefixes[i], uris[i]));
                        i++;
                    } else {
                        if ((j<other.prefixes.Length)) {
                            result.Add(new Saxon.Hej.om.NamespaceBinding(other.prefixes[j], ""));
                            j++;
                        } else {
                            return result.ToArray();
                        }
                    }
                }
            }
        }
        public string getURIForPrefix(string prefix, bool useDefault) {
            if (prefix.Equals("xml")) {
                return Saxon.Hej.lib.NamespaceConstant.XML;
            }
            if (prefix.Equals("")) {
                if (useDefault) {
                    return getDefaultNamespace();
                } else {
                    return "";
                }
            }
            return getURI(prefix);
        }
        public System.Collections.Generic.IEnumerator<string> iteratePrefixes() {
            System.Collections.Generic.IList<string> prefixList = new System.Collections.Generic.List<string>(Saxon.Ejava.util.Arrays.asList(prefixes));
            prefixList.Add("xml");
            return prefixList.GetEnumerator();
        }
        public string[] getPrefixArray() {
            return prefixes;
        }
        public string[] getURIsAsArray() {
            return uris;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            foreach (Saxon.Hej.om.NamespaceBinding nb in this) {
                sb.append(nb.getPrefix()).append("=").append(nb.getURI()).append(" ");
            }
            return sb.ToString();
        }
        public override int GetHashCode() {
            return (Saxon.Ejava.util.Arrays.hashCode(prefixes)^Saxon.Ejava.util.Arrays.hashCode(uris));
        }
        public override bool Equals(System.Object obj) {
            return ((this == obj) || ((((obj is Saxon.Hej.om.NamespaceMap) && Saxon.Ejava.util.Arrays.Equals(prefixes, ((Saxon.Hej.om.NamespaceMap)(obj)).prefixes)) && Saxon.Ejava.util.Arrays.Equals(uris, ((Saxon.Hej.om.NamespaceMap)(obj)).uris))));
        }
    
        private class Inner_Iterator_169 : System.Collections.Generic.IEnumerator<Saxon.Hej.om.NamespaceBinding> {
            private readonly NamespaceMap outerInstance;
            public Inner_Iterator_169(NamespaceMap outerInstance) {
                this.outerInstance = outerInstance;
                i = 0;
            }
            internal int i;
            public bool hasNext() {
                return (/*field*/i<outerInstance.prefixes.Length);
            }
            public Saxon.Hej.om.NamespaceBinding next() {
                Saxon.Hej.om.NamespaceBinding nb = new Saxon.Hej.om.NamespaceBinding(outerInstance.prefixes[/*field*/i], outerInstance.uris[/*field*/i]);
                /*field*/i++;
                return /*localvar*/nb;
            }
        
            object System.Collections.IEnumerator.Current => Current;
            public Saxon.Hej.om.NamespaceBinding Current { get; private set; }
            public bool MoveNext() {
                if (hasNext()) {Current = next(); return true;} else return false;
            }
            public void Reset() {throw new NotImplementedException();}
            public void Dispose() {}
      }

    }
}