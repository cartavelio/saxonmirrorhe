using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class AttributeImpl : Saxon.Hej.tree.linked.NodeImpl {
        public AttributeImpl(Saxon.Hej.tree.linked.ElementImpl element, int index)  {
            setRawParent(element);
            setSiblingPosition(index);
        }
        private Saxon.Hej.om.AttributeInfo getAttributeInfo() {
            return getRawParent().attributes().itemAt(getSiblingPosition());
        }
        public override Saxon.Hej.om.NodeName getNodeName() {
            if (((getRawParent() == null) || (getSiblingPosition() == -1))) {
                return null;
            }
            return getAttributeInfo().getNodeName();
        }
        public override int getFingerprint() {
            if (((getRawParent() == null) || (getSiblingPosition() == -1))) {
                return -1;
            }
            return getNodeName().obtainFingerprint(getNamePool());
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            return getAttributeInfo().getType();
        }
        public override bool isId() {
            return getAttributeInfo().isId();
        }
        public override bool isIdref() {
            if (Saxon.Hej.@event.ReceiverOption.contains(getAttributeInfo().getProperties(), Saxon.Hej.@event.ReceiverOption.IS_IDREF)) {
                return true;
            }
            return Saxon.Hej.tree.linked.ElementImpl.isIdRefNode(this);
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Hej.tree.linked.AttributeImpl)))) {
                return false;
            }
            if ((this == other)) {
                return true;
            }
            Saxon.Hej.tree.linked.AttributeImpl otherAtt = (Saxon.Hej.tree.linked.AttributeImpl)(other);
            return (getRawParent().Equals(otherAtt.getRawParent()) && (getSiblingPosition() == otherAtt.getSiblingPosition()));
        }
        public override int GetHashCode() {
            return (getRawParent().GetHashCode()^((getSiblingPosition()<<16)));
        }
        protected internal override long getSequenceNumber() {
            long parseq = getRawParent().getSequenceNumber();
            return (((parseq == -(1L)) ? parseq : ((parseq+unchecked((int)0x8000))+getSiblingPosition())));
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.ATTRIBUTE;
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return Saxon.Hej.str.StringView.tidy(getAttributeInfo().getValue());
        }
        public override string getStringValue() {
            return getAttributeInfo().getValue();
        }
        public override Saxon.Hej.tree.linked.NodeImpl getNextSibling() {
            return null;
        }
        public override Saxon.Hej.tree.linked.NodeImpl getPreviousSibling() {
            return null;
        }
        public override Saxon.Hej.tree.linked.NodeImpl getPreviousInDocument() {
            return ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
        }
        public override Saxon.Hej.tree.linked.NodeImpl getNextInDocument(Saxon.Hej.tree.linked.NodeImpl anchor) {
            if ((anchor == this)) {
                return null;
            }
            return ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getNextInDocument(anchor);
        }
        public override void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).generateId(buffer);
            buffer.append('a');
            buffer.append(getSiblingPosition());
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            throw new Saxon.Ejava.lang.IllegalArgumentException();
        }
        public override void delete() {
            if (!(isDeleted())) {
                if ((getRawParent() != null)) {
                    getRawParent().removeAttribute(this);
                }
                setRawParent(null);
                setSiblingPosition(-1);
            }
        }
        public override bool isDeleted() {
            return (((getRawParent() == null) || (getAttributeInfo() is Saxon.Hej.om.AttributeInfo.Deleted)) || getRawParent().isDeleted());
        }
        public override void replace(Saxon.Hej.om.NodeInfo[] replacement, bool inherit) {
            if (isDeleted()) {
                throw new Saxon.Ejava.lang.IllegalStateException("Cannot replace a deleted node");
            }
            if ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Cannot replace a parentless node");
            }
            Saxon.Hej.tree.linked.ParentNodeImpl element = getRawParent();
            delete();
            foreach (Saxon.Hej.om.NodeInfo n in replacement) {
                if ((n.getNodeKind() != Saxon.Hej.type.Type.ATTRIBUTE)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Replacement nodes must be attributes");
                }
                element.addAttribute(Saxon.Hej.om.NameOfNode.makeName(n), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, n.getStringValue(), Saxon.Hej.@event.ReceiverOption.NONE, inherit);
            }
        }
        public override void rename(Saxon.Hej.om.NodeName newNameCode, bool inherit) {
            Saxon.Hej.tree.linked.ElementImpl owner = (Saxon.Hej.tree.linked.ElementImpl)(getRawParent());
            if (((owner != null) && !(isDeleted()))) {
                Saxon.Hej.om.AttributeInfo att = getAttributeInfo();
                owner.setAttributeInfo(getSiblingPosition(), new Saxon.Hej.om.AttributeInfo(newNameCode, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, att.getValue(), att.getLocation(), att.getProperties()));
                Saxon.Hej.om.NamespaceUri newURI = newNameCode.getNamespaceUri();
                if (!(newURI.isEmpty())) {
                    string newPrefix = newNameCode.getPrefix();
                    Saxon.Hej.om.NamespaceBinding newBinding = new Saxon.Hej.om.NamespaceBinding(newPrefix, newURI);
                    Saxon.Hej.om.NamespaceUri oldURI = ((Saxon.Hej.tree.linked.ElementImpl)(getRawParent())).getURIForPrefix(newPrefix, false);
                    if ((oldURI == null)) {
                        owner.addNamespace(newBinding, inherit);
                    } else {
                        if (!(oldURI.Equals(newURI))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("Namespace binding of new name conflicts with existing namespace binding");
                        }
                    }
                }
            }
        }
        public override void replaceStringValue(Saxon.Hej.str.UnicodeString stringValue) {
            Saxon.Hej.tree.linked.ElementImpl owner = (Saxon.Hej.tree.linked.ElementImpl)(getRawParent());
            if (((owner != null) && !(isDeleted()))) {
                Saxon.Hej.om.AttributeInfo att = getAttributeInfo();
                owner.setAttributeInfo(getSiblingPosition(), new Saxon.Hej.om.AttributeInfo(att.getNodeName(), att.getType(), stringValue.ToString(), att.getLocation(), att.getProperties()));
            }
        }
        public override void removeTypeAnnotation() {
            Saxon.Hej.tree.linked.ElementImpl owner = (Saxon.Hej.tree.linked.ElementImpl)(getRawParent());
            if (((owner != null) && !(isDeleted()))) {
                Saxon.Hej.om.AttributeInfo att = getAttributeInfo();
                owner.setAttributeInfo(getSiblingPosition(), new Saxon.Hej.om.AttributeInfo(att.getNodeName(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, att.getValue(), att.getLocation(), att.getProperties()));
                owner.removeTypeAnnotation();
            }
        }
        public override void setTypeAnnotation(Saxon.Hej.type.SchemaType type) {
            if (!(((type is Saxon.Hej.type.SimpleType)))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Attribute type must be a simple type");
            }
            Saxon.Hej.tree.linked.ElementImpl owner = (Saxon.Hej.tree.linked.ElementImpl)(getRawParent());
            if (((owner != null) && !(isDeleted()))) {
                Saxon.Hej.om.AttributeInfo att = getAttributeInfo();
                owner.setAttributeInfo(getSiblingPosition(), new Saxon.Hej.om.AttributeInfo(att.getNodeName(), (Saxon.Hej.type.SimpleType)(type), att.getValue(), att.getLocation(), att.getProperties()));
                owner.removeTypeAnnotation();
            }
        }
    
    }
}