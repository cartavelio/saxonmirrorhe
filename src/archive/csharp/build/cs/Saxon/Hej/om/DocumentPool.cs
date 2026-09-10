using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public sealed class DocumentPool {
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.DocumentKey,Saxon.Hej.om.TreeInfo> documentNameMap = new System.Collections.Generic.Dictionary<Saxon.Hej.om.DocumentKey, Saxon.Hej.om.TreeInfo>(10);
        private readonly System.Collections.Generic.ISet<Saxon.Hej.om.DocumentKey> unavailableDocuments = new System.Collections.Generic.HashSet<Saxon.Hej.om.DocumentKey>(10);
        public void add(Saxon.Hej.om.TreeInfo doc, string uri) { lock(this) {
            if ((uri != null)) {
                add(doc, new Saxon.Hej.om.DocumentKey(uri));
            }
        }}
        public void add(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.om.DocumentKey uri) { lock(this) {
            if ((uri != null)) {
                Saxon.Hej.om.TreeInfo existing = Saxon.Impl.Helpers.MapUtils.Get(documentNameMap, uri);
                if (((existing != null) && (existing != doc))) {
                    throw new Saxon.Hej.trans.XPathException(("Cannot have two different documents with the same document-uri "+uri.getAbsoluteURI()));
                }
                Saxon.Impl.Helpers.MapUtils.Put(documentNameMap, uri, doc);
            }
        }}
        public Saxon.Hej.om.TreeInfo find(string uri) { lock(this) {
            return Saxon.Impl.Helpers.MapUtils.Get(documentNameMap, new Saxon.Hej.om.DocumentKey(uri));
        }}
        public Saxon.Hej.om.TreeInfo find(Saxon.Hej.om.DocumentKey uri) { lock(this) {
            return Saxon.Impl.Helpers.MapUtils.Get(documentNameMap, uri);
        }}
        public string getDocumentURI(Saxon.Hej.om.NodeInfo doc) { lock(this) {
            foreach (Saxon.Hej.om.DocumentKey uri in Saxon.Impl.Helpers.MapUtils.KeySet(documentNameMap)) {
                Saxon.Hej.om.TreeInfo found = find(uri);
                if ((found == null)) {
                    continue ;
                }
                if (found.getRootNode().Equals(doc)) {
                    return uri.ToString();
                }
            }
            return null;
        }}
        public bool contains(Saxon.Hej.om.TreeInfo doc) { lock(this) {
            return documentNameMap.Values.Contains(doc);
        }}
        public Saxon.Hej.om.TreeInfo discard(Saxon.Hej.om.TreeInfo doc) { lock(this) {
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.DocumentKey,Saxon.Hej.om.TreeInfo> e in documentNameMap) {
                Saxon.Hej.om.DocumentKey name = e.Key;
                Saxon.Hej.om.TreeInfo entry = e.Value;
                if (entry.Equals(doc)) {
                    documentNameMap.Remove(name);
                    return doc;
                }
            }
            return doc;
        }}
        public void discardIndexes(Saxon.Hej.trans.KeyManager keyManager) {
            foreach (Saxon.Hej.om.TreeInfo doc in documentNameMap.Values) {
                keyManager.clearDocumentIndexes(doc);
            }
        }
        public void markUnavailable(Saxon.Hej.om.DocumentKey uri) {
            unavailableDocuments.Add(uri);
        }
        public bool isMarkedUnavailable(Saxon.Hej.om.DocumentKey uri) {
            return unavailableDocuments.Contains(uri);
        }
    
    }
}