using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class AnnotationList : System.Collections.Generic.IEnumerable<Saxon.Hej.query.Annotation> {
        private readonly System.Collections.Generic.IList<Saxon.Hej.query.Annotation> list;
        public static Saxon.Hej.query.AnnotationList EMPTY = new Saxon.Hej.query.AnnotationList(new System.Collections.Generic.List<Saxon.Hej.query.Annotation>());
        public AnnotationList(System.Collections.Generic.IList<Saxon.Hej.query.Annotation> list)  {
            this.list = list;
        }
        public static Saxon.Hej.query.AnnotationList singleton(Saxon.Hej.query.Annotation ann) {
            return new Saxon.Hej.query.AnnotationList(Saxon.Ejava.util.Collections.singletonList(ann));
        }
        public void check(Saxon.Hej.Configuration config, string where) {
            System.Collections.Generic.IDictionary<Saxon.Hej.om.NamespaceUri,System.Collections.Generic.IList<Saxon.Hej.query.Annotation>> map = groupByNamespace();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.NamespaceUri,System.Collections.Generic.IList<Saxon.Hej.query.Annotation>> entry in map) {
                Saxon.Hej.lib.FunctionAnnotationHandler handler = config.getFunctionAnnotationHandler(entry.Key);
                if ((handler != null)) {
                    handler.check(new Saxon.Hej.query.AnnotationList(entry.Value), where);
                }
            }
        }
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.NamespaceUri,System.Collections.Generic.IList<Saxon.Hej.query.Annotation>> groupByNamespace() {
            System.Collections.Generic.IDictionary<Saxon.Hej.om.NamespaceUri,System.Collections.Generic.IList<Saxon.Hej.query.Annotation>> result = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NamespaceUri, System.Collections.Generic.IList<Saxon.Hej.query.Annotation>>();
            foreach (Saxon.Hej.query.Annotation ann in list) {
                Saxon.Hej.om.NamespaceUri ns = ann.getAnnotationQName().getNamespaceUri();
                if (result.ContainsKey(ns)) {
                    Saxon.Impl.Helpers.MapUtils.Get(result, ns).Add(ann);
                } else {
                    System.Collections.Generic.IList<Saxon.Hej.query.Annotation> list = new System.Collections.Generic.List<Saxon.Hej.query.Annotation>();
                    list.Add(ann);
                    Saxon.Impl.Helpers.MapUtils.Put(result, ns, list);
                }
            }
            return result;
        }
        public Saxon.Hej.query.AnnotationList filterByNamespace(Saxon.Hej.om.NamespaceUri ns) {
            System.Collections.Generic.IList<Saxon.Hej.query.Annotation> @out = new System.Collections.Generic.List<Saxon.Hej.query.Annotation>();
            foreach (Saxon.Hej.query.Annotation ann in list) {
                if (ann.getAnnotationQName().hasURI(ns)) {
                    @out.Add(ann);
                }
            }
            return new Saxon.Hej.query.AnnotationList(@out);
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.query.Annotation> GetEnumerator() {
            return list.GetEnumerator();
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public bool isEmpty() {
            return list.Count==0;
        }
        public int size() {
            return list.Count;
        }
        public Saxon.Hej.query.Annotation get(int i) {
            return list[i];
        }
        public bool includes(Saxon.Hej.om.StructuredQName name) {
            foreach (Saxon.Hej.query.Annotation a in list) {
                if (a.getAnnotationQName().Equals(name)) {
                    return true;
                }
            }
            return false;
        }
        public bool includes(string localName) {
            foreach (Saxon.Hej.query.Annotation a in list) {
                if (a.getAnnotationQName().getLocalPart().Equals(localName)) {
                    return true;
                }
            }
            return false;
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.query.AnnotationList) && list.Equals(((Saxon.Hej.query.AnnotationList)(other)).list));
        }
        public override int GetHashCode() {
            return list.GetHashCode();
        }
    
    }
}