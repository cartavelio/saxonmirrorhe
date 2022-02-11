using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed public class TinyAttributeImpl : Saxon.Hej.tree.tiny.TinyNodeImpl {
        public TinyAttributeImpl(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr)  {
            this.tree = tree;
            this.nodeNr = nodeNr;
        }
        public override void setSystemId(string uri) {
        }
        public override string getSystemId() {
            Saxon.Hej.om.NodeInfo parent = getParent();
            return ((parent == null) ? null : getParent().getSystemId());
        }
        public override Saxon.Hej.tree.tiny.TinyNodeImpl getParent() {
            return ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(tree.attParent[nodeNr]));
        }
        public override Saxon.Hej.om.NodeInfo getRoot() {
            Saxon.Hej.om.NodeInfo parent = getParent();
            if ((parent == null)) {
                return this;
            } else {
                return parent.getRoot();
            }
        }
        protected internal override long getSequenceNumber() {
            return ((getParent().getSequenceNumber()+unchecked((int)0x8000))+((nodeNr-tree.alpha[tree.attParent[nodeNr]])));
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.ATTRIBUTE;
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return Saxon.Hej.str.StringView.of(tree.attValue[nodeNr]).tidy();
        }
        public override int getFingerprint() {
            return (tree.attCode[nodeNr]&unchecked((int)0xfffff));
        }
        public int getNameCode() {
            return tree.attCode[nodeNr];
        }
        public override string getPrefix() {
            int code = tree.attCode[nodeNr];
            if (!(Saxon.Hej.om.NamePool.isPrefixed(code))) {
                return "";
            }
            return tree.prefixPool.getPrefix((code>>20));
        }
        public override string getDisplayName() {
            int code = tree.attCode[nodeNr];
            if ((code<0)) {
                return "";
            }
            if (Saxon.Hej.om.NamePool.isPrefixed(code)) {
                return ((getPrefix()+":")+getLocalPart());
            } else {
                return getLocalPart();
            }
        }
        public override string getLocalPart() {
            return tree.getNamePool().getLocalName(tree.attCode[nodeNr]);
        }
        public override string getURI() {
            int code = tree.attCode[nodeNr];
            if (!(Saxon.Hej.om.NamePool.isPrefixed(code))) {
                return "";
            }
            return tree.getNamePool().getURI(code);
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            if ((tree.attType == null)) {
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
            }
            return tree.getAttributeType(nodeNr);
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            return tree.getTypedValueOfAttribute(this, nodeNr);
        }
        public override void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            getParent().generateId(buffer);
            buffer.append("a");
            buffer.append(tree.attCode[nodeNr]);
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("copy() applied to attribute node");
        }
        public override int getLineNumber() {
            return getParent().getLineNumber();
        }
        public override int getColumnNumber() {
            return getParent().getColumnNumber();
        }
        public override bool isId() {
            return tree.isIdAttribute(nodeNr);
        }
        public override bool isIdref() {
            return tree.isIdrefAttribute(nodeNr);
        }
        public bool isDefaultedAttribute() {
            return tree.isDefaultedAttribute(nodeNr);
        }
        public override int GetHashCode() {
            return (((((int)(((tree.getDocumentNumber()&0x3ff)))<<20))^nodeNr)^(7<<17));
        }
    
    }
}