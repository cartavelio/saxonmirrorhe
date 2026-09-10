using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.util {
    public class NamespaceResolverWithDefault : Saxon.Hej.om.NamespaceResolver {
        private readonly Saxon.Hej.om.NamespaceResolver baseResolver;
        private readonly Saxon.Hej.om.NamespaceUri defaultNamespace;
        public NamespaceResolverWithDefault(Saxon.Hej.om.NamespaceResolver @base, Saxon.Hej.om.NamespaceUri defaultNamespace)  {
            this.baseResolver = @base;
            this.defaultNamespace = defaultNamespace;
        }
        public Saxon.Hej.om.NamespaceUri getURIForPrefix(string prefix, bool useDefault) {
            if ((useDefault && prefix.Length==0)) {
                return defaultNamespace;
            } else {
                return baseResolver.getURIForPrefix(prefix, useDefault);
            }
        }
        public System.Collections.Generic.IEnumerator<string> iteratePrefixes() {
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>(10);
            for (System.Collections.Generic.IEnumerator<string> it = baseResolver.iteratePrefixes(); it.MoveNext(); ) {
                string p = it.Current;
                if ((p.Length != 0)) {
                    list.Add(p);
                }
            }
            list.Add("");
            return list.GetEnumerator();
        }
    
    }
}