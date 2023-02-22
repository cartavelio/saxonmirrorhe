using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class NamespaceDeltaMap : Saxon.Hej.om.NamespaceMap, Saxon.Hej.om.NamespaceBindingSet, Saxon.Hej.om.NamespaceResolver {
        private static readonly Saxon.Hej.om.NamespaceDeltaMap EMPTY_MAP = new Saxon.Hej.om.NamespaceDeltaMap();
        public static new Saxon.Hej.om.NamespaceDeltaMap emptyMap() {
            return EMPTY_MAP;
        }
        private NamespaceDeltaMap()  {
            prefixes = new string[]{};
            uris = new string[]{};
        }
        protected internal override Saxon.Hej.om.NamespaceMap makeNamespaceMap() {
            return new Saxon.Hej.om.NamespaceDeltaMap();
        }
        public override bool allowsNamespaceUndeclarations() {
            return true;
        }
        public override Saxon.Hej.om.NamespaceDeltaMap put(string prefix, string uri) {
            return (Saxon.Hej.om.NamespaceDeltaMap)(base.put(prefix, uri));
        }
        public override Saxon.Hej.om.NamespaceDeltaMap remove(string prefix) {
            return (Saxon.Hej.om.NamespaceDeltaMap)(base.remove(prefix));
        }
    
    }
}