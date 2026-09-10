using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public sealed class TinyDocumentImpl : Saxon.Hej.tree.tiny.TinyParentNodeImpl {
        private Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>> elementList;
        private string baseURI;
        public TinyDocumentImpl(Saxon.Hej.tree.tiny.TinyTree tree)  {
            this.tree = tree;
            nodeNr = 0;
        }
        public override Saxon.Hej.tree.tiny.TinyTree getTree() {
            return tree;
        }
        public Saxon.Hej.om.NodeInfo getRootNode() {
            return this;
        }
        public override Saxon.Hej.Configuration getConfiguration() {
            return tree.getConfiguration();
        }
        public override void setSystemId(string uri) {
            tree.setSystemId(nodeNr, uri);
        }
        public override string getSystemId() {
            return tree.getSystemId(nodeNr);
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
        public override int getLineNumber() {
            return 0;
        }
        public bool isTyped() {
            return (tree.getTypeArray() != null);
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.DOCUMENT;
        }
        public override Saxon.Hej.tree.tiny.TinyNodeImpl getParent() {
            return null;
        }
        public override Saxon.Hej.om.NodeInfo getRoot() {
            return this;
        }
        public override void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            buffer.append('d');
            buffer.append(getTreeInfo().getDocumentNumber());
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            return Saxon.Hej.value.StringValue.makeUntypedAtomic(getUnicodeStringValue());
        }
        internal Saxon.Hej.tree.iter.AxisIterator getAllElements(int fingerprint) {
            if ((elementList == null)) {
                elementList = new Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>>(20);
            }
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> list = elementList.get(fingerprint);
            if ((list == null)) {
                list = makeElementList(fingerprint);
                elementList.put(fingerprint, list);
            }
            return new Saxon.Hej.tree.iter.NodeListIterator(list);
        }
        internal System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> makeElementList(int fingerprint) {
            int size = (tree.getNumberOfNodes()/20);
            if ((size>100)) {
                size = 100;
            }
            if ((size<20)) {
                size = 20;
            }
            System.Collections.Generic.List<Saxon.Hej.om.NodeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>(size);
            int i = (nodeNr+1);
            try {
                while ((tree.depth[i] != 0)) {
                    byte kind = tree.nodeKind[i];
                    if (((((kind&0x0f)) == Saxon.Hej.type.Type.ELEMENT) && (((tree.nameCode[i]&unchecked((int)0xfffff))) == fingerprint))) {
                        list.Add(((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(i)));
                    }
                    i++;
                }
            } catch (System.IndexOutOfRangeException e) {
                return list;
            }
            Saxon.Impl.Helpers.ListUtils.TrimToSize(list);
            return list;
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            Saxon.Hej.tree.iter.AxisIterator children = iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            Saxon.Hej.om.NodeInfo node = ((/*1430*/Saxon.Hej.om.NodeInfo)children.next());
            if (((node == null) || (node.getSchemaType() == Saxon.Hej.type.Untyped.getInstance()))) {
                return Saxon.Hej.type.Untyped.getInstance();
            } else {
                return Saxon.Hej.type.AnyType.getInstance();
            }
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.startDocument(Saxon.Hej.om.CopyOptions.getStartDocumentProperties(copyOptions));
            if ((tree.entityTable != null)) {
                foreach (System.Collections.Generic.KeyValuePair<string,string[]> entry in tree.entityTable) {
                    string name = entry.Key;
                    string[] details = entry.Value;
                    string systemId = details[0];
                    string publicId = details[1];
                    @out.setUnparsedEntity(name, systemId, publicId);
                }
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                child.copy(@out, copyOptions, locationId);
            }
            @out.endDocument();
        }
        public void showSize() {
            tree.showSize();
        }
        public override int GetHashCode() {
            return (int)(tree.getDocumentNumber());
        }
    
    }
}