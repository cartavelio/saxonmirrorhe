using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public sealed class DocumentImpl : Saxon.Hej.tree.linked.ParentNodeImpl, Saxon.Hej.om.TreeInfo, Saxon.Hej.om.MutableDocumentInfo {
        private Saxon.Hej.tree.linked.ElementImpl documentElement;
        private System.Collections.Generic.Dictionary<string,Saxon.Hej.om.NodeInfo> idTable;
        private long documentNumber;
        private string baseURI;
        private System.Collections.Generic.Dictionary<string,string[]> entityTable;
        private System.Collections.Generic.ISet<Saxon.Hej.tree.linked.ElementImpl> nilledElements;
        private System.Collections.Generic.ISet<Saxon.Hej.tree.linked.ElementImpl> topWithinEntityElements;
        private Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>> elementList;
        private System.Collections.Generic.Dictionary<string,System.Object> userData;
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.tree.linked.LineNumberMap lineNumberMap;
        private Saxon.Hej.tree.linked.SystemIdMap systemIdMap = new Saxon.Hej.tree.linked.SystemIdMap();
        private bool imaginary;
        private bool mutable;
        private Saxon.Hej.om.SpaceStrippingRule spaceStrippingRule = Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance();
        public DocumentImpl()  {
            setRawParent(null);
        }
        public Saxon.Hej.om.NodeInfo getRootNode() {
            return this;
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
            documentNumber = config.getDocumentNumberAllocator().allocateDocumentNumber();
        }
        public override Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public bool isMutable() {
            return mutable;
        }
        public void setMutable(bool mutable) {
            this.mutable = mutable;
        }
        public override Saxon.Hej.om.NamePool getNamePool() {
            return config.getNamePool();
        }
        public override Saxon.Hej.@event.Builder newBuilder() {
            Saxon.Hej.tree.linked.LinkedTreeBuilder builder = new Saxon.Hej.tree.linked.LinkedTreeBuilder(config.makePipelineConfiguration());
            builder.setAllocateSequenceNumbers(false);
            return builder;
        }
        public void setImaginary(bool imaginary) {
            this.imaginary = imaginary;
        }
        public bool isImaginary() {
            return imaginary;
        }
        public bool isTyped() {
            return ((documentElement != null) && (documentElement.getSchemaType() != Saxon.Hej.type.Untyped.getInstance()));
        }
        public long getDocumentNumber() {
            return documentNumber;
        }
        public void setDocumentElement(Saxon.Hej.tree.linked.ElementImpl e) {
            documentElement = e;
        }
        public void graftLocationMap(Saxon.Hej.tree.linked.DocumentImpl original) {
            systemIdMap = original.systemIdMap;
            lineNumberMap = original.lineNumberMap;
        }
        public override void setSystemId(string uri) {
            if ((uri == null)) {
                uri = "";
            }
            systemIdMap.setSystemId(getRawSequenceNumber(), uri);
        }
        public override string getSystemId() {
            return systemIdMap.getSystemId(getRawSequenceNumber());
        }
        public void setBaseURI(string uri) {
            baseURI = uri;
        }
        public override string getBaseURI() {
            if ((baseURI != null)) {
                return baseURI;
            }
            return getSystemId();
        }
        internal void setSystemId(int seq, string uri) {
            if ((uri == null)) {
                uri = "";
            }
            systemIdMap.setSystemId(seq, uri);
        }
        internal string getSystemId(int seq) {
            return systemIdMap.getSystemId(seq);
        }
        public void setLineNumbering() {
            lineNumberMap = new Saxon.Hej.tree.linked.LineNumberMap();
            lineNumberMap.setLineAndColumn(getRawSequenceNumber(), 0, -1);
        }
        internal void setLineAndColumn(int sequence, int line, int column) {
            if (((lineNumberMap != null) && (sequence>=0))) {
                lineNumberMap.setLineAndColumn(sequence, line, column);
            }
        }
        internal int getLineNumber(int sequence) {
            if (((lineNumberMap != null) && (sequence>=0))) {
                return lineNumberMap.getLineNumber(sequence);
            }
            return -1;
        }
        internal int getColumnNumber(int sequence) {
            if (((lineNumberMap != null) && (sequence>=0))) {
                return lineNumberMap.getColumnNumber(sequence);
            }
            return -1;
        }
        public void addNilledElement(Saxon.Hej.tree.linked.ElementImpl element) {
            if ((nilledElements == null)) {
                nilledElements = new System.Collections.Generic.HashSet<Saxon.Hej.tree.linked.ElementImpl>();
            }
            nilledElements.Add(element);
        }
        internal bool isNilledElement(Saxon.Hej.tree.linked.ElementImpl element) {
            return ((nilledElements != null) && nilledElements.Contains(element));
        }
        public void markTopWithinEntity(Saxon.Hej.tree.linked.ElementImpl element) {
            if ((topWithinEntityElements == null)) {
                topWithinEntityElements = new System.Collections.Generic.HashSet<Saxon.Hej.tree.linked.ElementImpl>();
            }
            topWithinEntityElements.Add(element);
        }
        public bool isTopWithinEntity(Saxon.Hej.tree.linked.ElementImpl element) {
            return ((topWithinEntityElements != null) && topWithinEntityElements.Contains(element));
        }
        public override int getLineNumber() {
            return 0;
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.DOCUMENT;
        }
        public override Saxon.Hej.tree.linked.NodeImpl getNextSibling() {
            return null;
        }
        public override Saxon.Hej.tree.linked.NodeImpl getPreviousSibling() {
            return null;
        }
        public Saxon.Hej.tree.linked.ElementImpl getDocumentElement() {
            return documentElement;
        }
        public override Saxon.Hej.om.NodeInfo getRoot() {
            return this;
        }
        public override Saxon.Hej.tree.linked.DocumentImpl getPhysicalRoot() {
            return this;
        }
        public override void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            buffer.append('d');
            buffer.append(documentNumber);
        }
        internal Saxon.Hej.tree.iter.AxisIterator getAllElements(int fingerprint) {
            if ((elementList == null)) {
                elementList = new Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>>(500);
            }
            Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>> eList = elementList;
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> list = eList.get(fingerprint);
            if ((list == null)) {
                list = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>(500);
                Saxon.Hej.tree.linked.NodeImpl next = getNextInDocument(this);
                while ((next != null)) {
                    if (((next.getNodeKind() == Saxon.Hej.type.Type.ELEMENT) && (next.getFingerprint() == fingerprint))) {
                        list.Add(next);
                    }
                    next = next.getNextInDocument(this);
                }
                eList.put(fingerprint, list);
            }
            return new Saxon.Hej.tree.iter.NodeListIterator(list);
        }
        public void deIndex(Saxon.Hej.tree.linked.NodeImpl node) {
            if ((node is Saxon.Hej.tree.linked.ElementImpl)) {
                Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>> eList = elementList;
                if ((eList != null)) {
                    System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> list = eList.get(node.getFingerprint());
                    if ((list == null)) {
                        return ;
                    }
                    Saxon.Impl.Helpers.ListUtils.Remove(list, node);
                }
                if (node.isId()) {
                    deregisterID(node.getStringValue());
                }
            } else {
                if ((node is Saxon.Hej.tree.linked.AttributeImpl)) {
                    if (node.isId()) {
                        deregisterID(node.getStringValue());
                    }
                }
            }
        }
        private void indexIDs() {
            if ((idTable != null)) {
                return ;
            }
            idTable = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.NodeInfo>(256);
            Saxon.Hej.tree.linked.NodeImpl curr = this;
            Saxon.Hej.tree.linked.NodeImpl root = curr;
            while ((curr != null)) {
                if ((curr.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                    Saxon.Hej.tree.linked.ElementImpl e = (Saxon.Hej.tree.linked.ElementImpl)(curr);
                    if (e.isId()) {
                        registerID(e, Saxon.Hej.value.Whitespace.trim(e.getStringValue()));
                    }
                    Saxon.Hej.om.AttributeMap atts = e.attributes();
                    foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                        if ((att.isId() && Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.value.Whitespace.trim(att.getValue())))) {
                            registerID(e, Saxon.Hej.value.Whitespace.trim(att.getValue()));
                        }
                    }
                }
                curr = curr.getNextInDocument(root);
            }
        }
        internal void registerID(Saxon.Hej.om.NodeInfo e, string id) {
            if ((idTable == null)) {
                idTable = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.NodeInfo>(256);
            }
            System.Collections.Generic.Dictionary<string,Saxon.Hej.om.NodeInfo> table = idTable;
            Saxon.Impl.Helpers.MapUtils.PutIfAbsent(table, id, e);
        }
        public Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            if ((idTable == null)) {
                indexIDs();
            }
            System.Diagnostics.Debug.Assert((idTable != null));
            Saxon.Hej.om.NodeInfo node = Saxon.Impl.Helpers.MapUtils.Get(idTable, id);
            if (((((node != null) && getParent) && node.isId()) && node.getUnicodeStringValue().Equals(id))) {
                node = node.getParent();
            }
            return node;
        }
        internal void deregisterID(string id) {
            id = Saxon.Hej.value.Whitespace.trim(id);
            if ((idTable != null)) {
                idTable.Remove(id);
            }
        }
        public void setUnparsedEntity(string name, string uri, string publicId) {
            if ((entityTable == null)) {
                entityTable = new System.Collections.Generic.Dictionary<string, string[]>(10);
            }
            string[] ids = new string[2];
            ids[0] = uri;
            ids[1] = publicId;
            Saxon.Impl.Helpers.MapUtils.Put(entityTable, name, ids);
        }
        public System.Collections.Generic.IEnumerator<string> getUnparsedEntityNames() {
            if ((entityTable == null)) {
                System.Collections.Generic.IList<string> ls = new System.Collections.Generic.List<string>();
                return ls.GetEnumerator();
            } else {
                return Saxon.Impl.Helpers.MapUtils.KeySet(entityTable).GetEnumerator();
            }
        }
        public string[] getUnparsedEntity(string name) {
            if ((entityTable == null)) {
                return null;
            }
            return Saxon.Impl.Helpers.MapUtils.Get(entityTable, name);
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            if (((documentElement == null) || (documentElement.getSchemaType() == Saxon.Hej.type.Untyped.getInstance()))) {
                return Saxon.Hej.type.Untyped.getInstance();
            } else {
                return Saxon.Hej.type.AnyType.getInstance();
            }
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.startDocument(Saxon.Hej.om.CopyOptions.getStartDocumentProperties(copyOptions));
            for (System.Collections.Generic.IEnumerator<string> names = getUnparsedEntityNames(); names.MoveNext(); ) {
                string name = names.Current;
                string[] details = getUnparsedEntity(name);
                System.Diagnostics.Debug.Assert((details != null));
                @out.setUnparsedEntity(name, details[0], details[1]);
            }
            Saxon.Hej.tree.linked.NodeImpl next = getFirstChild();
            while ((next != null)) {
                next.copy(@out, copyOptions, locationId);
                next = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)next.getNextSibling());
            }
            @out.endDocument();
        }
        public override void replaceStringValue(Saxon.Hej.str.UnicodeString stringValue) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot replace the value of a document node");
        }
        public void resetIndexes() {
            idTable = null;
            elementList = null;
        }
        public void setSpaceStrippingRule(Saxon.Hej.om.SpaceStrippingRule rule) {
            this.spaceStrippingRule = rule;
        }
        public Saxon.Hej.om.SpaceStrippingRule getSpaceStrippingRule() {
            return spaceStrippingRule;
        }
        public void setUserData(string key, System.Object value) {
            if ((userData == null)) {
                userData = new System.Collections.Generic.Dictionary<string, System.Object>(4);
            }
            if ((value == null)) {
                userData.Remove(key);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(userData, key, value);
            }
        }
        public System.Object getUserData(string key) {
            if ((userData == null)) {
                return null;
            } else {
                return Saxon.Impl.Helpers.MapUtils.Get(userData, key);
            }
        }
    
    }
}