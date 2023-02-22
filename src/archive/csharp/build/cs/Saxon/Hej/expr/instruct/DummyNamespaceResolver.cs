using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class DummyNamespaceResolver : Saxon.Hej.om.NamespaceResolver {
        private readonly static Saxon.Hej.expr.instruct.DummyNamespaceResolver THE_INSTANCE = new Saxon.Hej.expr.instruct.DummyNamespaceResolver();
        public static Saxon.Hej.expr.instruct.DummyNamespaceResolver getInstance() {
            return THE_INSTANCE;
        }
        private DummyNamespaceResolver()  {
        }
        public Saxon.Hej.om.NamespaceUri getURIForPrefix(string prefix, bool useDefault) {
            if (prefix.Length==0) {
                return Saxon.Hej.om.NamespaceUri.NULL;
            } else {
                if ("xml".Equals(prefix)) {
                    return Saxon.Hej.om.NamespaceUri.XML;
                } else {
                    return Saxon.Hej.om.NamespaceUri.NULL;
                }
            }
        }
        public System.Collections.Generic.IEnumerator<string> iteratePrefixes() {
            return new Saxon.Hej.tree.jiter.PairIterator<string>("", "xml");
        }
    
    }
}