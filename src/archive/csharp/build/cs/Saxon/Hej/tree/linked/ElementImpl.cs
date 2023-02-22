using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class ElementImpl : Saxon.Hej.tree.linked.ParentNodeImpl, Saxon.Hej.om.NamespaceResolver {
        private Saxon.Hej.om.NodeName nodeName;
        private Saxon.Hej.type.SchemaType type = Saxon.Hej.type.Untyped.getInstance();
        private Saxon.Hej.om.AttributeMap attributeMap;
        private Saxon.Hej.om.NamespaceMap namespaceMap = Saxon.Hej.om.NamespaceMap.emptyMap();
        public ElementImpl()  {
            this.attributeMap = Saxon.Hej.om.EmptyAttributeMap.getInstance();
        }
        public override void setAttributes(Saxon.Hej.om.AttributeMap atts) {
            this.attributeMap = atts;
        }
        public void setNodeName(Saxon.Hej.om.NodeName name) {
            this.nodeName = name;
        }
        public virtual void initialise(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType elementType, Saxon.Hej.om.AttributeMap atts, Saxon.Hej.om.NodeInfo parent, int sequenceNumber) {
            this.nodeName = elemName;
            this.type = elementType;
            setRawParent((Saxon.Hej.tree.linked.ParentNodeImpl)(parent));
            setRawSequenceNumber(sequenceNumber);
            attributeMap = atts;
        }
        public override Saxon.Hej.om.NodeName getNodeName() {
            return nodeName;
        }
        public void setLocation(string systemId, int line, int column) {
            Saxon.Hej.tree.linked.DocumentImpl root = getRawParent().getPhysicalRoot();
            root.setLineAndColumn(getRawSequenceNumber(), line, column);
            root.setSystemId(getRawSequenceNumber(), systemId);
        }
        public override void setSystemId(string uri) {
            getPhysicalRoot().setSystemId(getRawSequenceNumber(), uri);
        }
        public override Saxon.Hej.om.NodeInfo getRoot() {
            Saxon.Hej.tree.linked.ParentNodeImpl up = getRawParent();
            if (((up == null) || (((up is Saxon.Hej.tree.linked.DocumentImpl) && ((Saxon.Hej.tree.linked.DocumentImpl)(up)).isImaginary())))) {
                return this;
            } else {
                return up.getRoot();
            }
        }
        public override string getSystemId() {
            Saxon.Hej.tree.linked.DocumentImpl root = getPhysicalRoot();
            return ((root == null) ? null : root.getSystemId(getRawSequenceNumber()));
        }
        public override string getBaseURI() {
            return Saxon.Hej.tree.util.Navigator.getBaseURI(this, n => getPhysicalRoot().isTopWithinEntity((Saxon.Hej.tree.linked.ElementImpl)(n)));
        }
        public override bool isNilled() {
            return getPhysicalRoot().isNilledElement(this);
        }
        public override void setTypeAnnotation(Saxon.Hej.type.SchemaType type) {
            this.type = type;
        }
        public void setNilled() {
            getPhysicalRoot().addNilledElement(this);
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            return type;
        }
        public override int getLineNumber() {
            Saxon.Hej.tree.linked.DocumentImpl root = getPhysicalRoot();
            if ((root == null)) {
                return -1;
            } else {
                return root.getLineNumber(getRawSequenceNumber());
            }
        }
        public override int getColumnNumber() {
            Saxon.Hej.tree.linked.DocumentImpl root = getPhysicalRoot();
            if ((root == null)) {
                return -1;
            } else {
                return root.getColumnNumber(getRawSequenceNumber());
            }
        }
        public override void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            int sequence = getRawSequenceNumber();
            if ((sequence>=0)) {
                getPhysicalRoot().generateId(buffer);
                buffer.append("e");
                buffer.append(sequence);
            } else {
                getRawParent().generateId(buffer);
                buffer.append("f");
                buffer.append(getSiblingPosition());
            }
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.ELEMENT;
        }
        public override Saxon.Hej.om.AttributeMap attributes() {
            return attributeMap;
        }
        internal Saxon.Hej.tree.iter.AxisIterator iterateAttributes(Saxon.Hej.pattern.NodeTest test) {
            if ((attributeMap is Saxon.Hej.tree.linked.AttributeMapWithIdentity)) {
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(((Saxon.Hej.tree.linked.AttributeMapWithIdentity)(attributeMap)).iterateAttributes(this), test);
            } else {
                return new Saxon.Hej.tree.linked.AttributeAxisIterator(this, test);
            }
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location location) {
            bool copyTypes = Saxon.Hej.om.CopyOptions.includes(copyOptions, Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS);
            bool copyForUpdate = Saxon.Hej.om.CopyOptions.includes(copyOptions, Saxon.Hej.om.CopyOptions.FOR_UPDATE);
            Saxon.Hej.type.SchemaType typeCode = (copyTypes ? getSchemaType() : Saxon.Hej.type.Untyped.getInstance());
            System.Func<Saxon.Hej.om.NodeInfo,System.Object> informee = @out.getPipelineConfiguration().getCopyInformee();
            if ((informee != null)) {
                System.Object o = informee(this);
                if ((o is Saxon.Hej.s9api.Location)) {
                    location = (Saxon.Hej.s9api.Location)(o);
                }
            }
            Saxon.Hej.om.NamespaceMap ns = (Saxon.Hej.om.CopyOptions.includes(copyOptions, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES) ? getAllNamespaces() : Saxon.Hej.om.NamespaceMap.emptyMap());
            bool disallowNamespaceSensitiveContent = (((((copyOptions&Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS)) != 0)) && ((((copyOptions&Saxon.Hej.om.CopyOptions.ALL_NAMESPACES)) == 0)));
            if ((copyTypes && disallowNamespaceSensitiveContent)) {
                try {
                    checkNotNamespaceSensitiveElement(getSchemaType());
                } catch (Saxon.Hej.@event.CopyNamespaceSensitiveException e) {
                    e.setErrorCode((@out.getPipelineConfiguration().isXSLT() ? "XTTE0950" : "XQTY0086"));
                    throw;
                }
            }
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> atts = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(attributes().size());
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.type.SimpleType attributeType = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                if (copyTypes) {
                    attributeType = att.getType();
                    if (disallowNamespaceSensitiveContent) {
                        try {
                            checkNotNamespaceSensitiveAttribute(attributeType, att);
                        } catch (Saxon.Hej.@event.CopyNamespaceSensitiveException e) {
                            e.setErrorCode((@out.getPipelineConfiguration().isXSLT() ? "XTTE0950" : "XQTY0086"));
                            throw;
                        }
                    }
                }
                atts.Add(new Saxon.Hej.om.AttributeInfo(att.getNodeName(), attributeType, att.getValue(), att.getLocation(), 0));
            }
            int receiverOptions = (Saxon.Hej.@event.ReceiverOption.BEQUEATH_INHERITED_NAMESPACES_ONLY|Saxon.Hej.@event.ReceiverOption.NAMESPACE_OK);
            if (copyForUpdate) {
                receiverOptions |= Saxon.Hej.@event.ReceiverOption.MUTABLE_TREE;
            }
            @out.startElement(Saxon.Hej.om.NameOfNode.makeName(this), typeCode, Saxon.Hej.om.SequenceTool.attributeMapFromList(atts), ns, location, receiverOptions);
            Saxon.Hej.tree.linked.NodeImpl next = getFirstChild();
            while ((next != null)) {
                next.copy(@out, copyOptions, location);
                next = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)next.getNextSibling());
            }
            @out.endElement();
        }
        protected internal void checkNotNamespaceSensitiveElement(Saxon.Hej.type.SchemaType type) {
            if (((type is Saxon.Hej.type.SimpleType) && ((Saxon.Hej.type.SimpleType)(type)).isNamespaceSensitive())) {
                if (type.isAtomicType()) {
                    throw new Saxon.Hej.@event.CopyNamespaceSensitiveException("Cannot copy QName or NOTATION values without copying namespaces");
                } else {
                    Saxon.Hej.om.AtomicSequence value = atomize();
                    foreach (Saxon.Hej.value.AtomicValue val in value) {
                        if (val.getPrimitiveType().isNamespaceSensitive()) {
                            throw new Saxon.Hej.@event.CopyNamespaceSensitiveException("Cannot copy QName or NOTATION values without copying namespaces");
                        }
                    }
                }
            }
        }
        private void checkNotNamespaceSensitiveAttribute(Saxon.Hej.type.SimpleType type, Saxon.Hej.om.AttributeInfo att) {
            if (type.isNamespaceSensitive()) {
                if (type.isAtomicType()) {
                    throw new Saxon.Hej.@event.CopyNamespaceSensitiveException("Cannot copy QName or NOTATION values without copying namespaces");
                } else {
                    Saxon.Hej.om.AtomicSequence value = type.getTypedValue(att.getXdmStringValue().getUnicodeStringValue(), namespaceMap, getConfiguration().getConversionRules());
                    foreach (Saxon.Hej.value.AtomicValue val in value) {
                        if (val.getPrimitiveType().isNamespaceSensitive()) {
                            throw new Saxon.Hej.@event.CopyNamespaceSensitiveException("Cannot copy QName or NOTATION values without copying namespaces");
                        }
                    }
                }
            }
        }
        public override void delete() {
            Saxon.Hej.tree.linked.DocumentImpl root = getPhysicalRoot();
            base.delete();
            if ((root != null)) {
                Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                while (true) {
                    Saxon.Hej.tree.linked.ElementImpl n = (Saxon.Hej.tree.linked.ElementImpl)(((/*1430*/Saxon.Hej.om.NodeInfo)iter.next()));
                    foreach (Saxon.Hej.om.AttributeInfo att in attributeMap) {
                        if (att.isId()) {
                            root.deregisterID(att.getValue());
                        }
                    }
                    if ((n == null)) {
                        break ;
                    }
                    root.deIndex(n);
                }
            }
        }
        public override void rename(Saxon.Hej.om.NodeName newName, bool inherit) {
            string prefix = newName.getPrefix();
            Saxon.Hej.om.NamespaceUri uri = newName.getNamespaceUri();
            Saxon.Hej.om.NamespaceBinding ns = new Saxon.Hej.om.NamespaceBinding(prefix, uri);
            Saxon.Hej.om.NamespaceUri uc = getURIForPrefix(prefix, true);
            if ((uc == null)) {
                uc = Saxon.Hej.om.NamespaceUri.NULL;
            }
            if (!(uc.Equals(uri))) {
                if (uc.isEmpty()) {
                    addNamespace(ns, inherit);
                } else {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Namespace binding of new name conflicts with existing namespace binding");
                }
            }
            nodeName = newName;
        }
        public override void addNamespace(Saxon.Hej.om.NamespaceBinding binding, bool inherit) {
            if (binding.getNamespaceUri().isEmpty()) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Cannot add a namespace undeclaration");
            }
            Saxon.Hej.om.NamespaceUri existing = namespaceMap.getNamespaceUri(binding.getPrefix());
            if ((existing != null)) {
                if (!(existing.Equals(binding.getNamespaceUri()))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("New namespace conflicts with existing namespace binding");
                }
            } else {
                Saxon.Hej.om.NamespaceMap oldMap = namespaceMap;
                namespaceMap = namespaceMap.put(binding.getPrefix(), binding.getNamespaceUri());
                if ((inherit && (namespaceMap != oldMap))) {
                    foreach (Saxon.Hej.om.NodeInfo child in children(Saxon.Hej.pattern.NodeKindTest.ELEMENT)) {
                        ((Saxon.Hej.tree.linked.ElementImpl)(child)).inheritParentNamespaces(binding, oldMap, namespaceMap);
                    }
                }
            }
        }
        private void inheritParentNamespaces(Saxon.Hej.om.NamespaceBinding binding, Saxon.Hej.om.NamespaceMap oldParentMap, Saxon.Hej.om.NamespaceMap newParentMap) {
            Saxon.Hej.om.NamespaceMap oldMap = namespaceMap;
            if ((oldMap.getURIForPrefix(binding.getPrefix(), false) == null)) {
                if ((namespaceMap == oldParentMap)) {
                    namespaceMap = newParentMap;
                } else {
                    namespaceMap = namespaceMap.put(binding.getPrefix(), binding.getNamespaceUri());
                }
                foreach (Saxon.Hej.om.NodeInfo child in children(Saxon.Hej.pattern.NodeKindTest.ELEMENT)) {
                    ((Saxon.Hej.tree.linked.ElementImpl)(child)).inheritParentNamespaces(binding, oldMap, namespaceMap);
                }
            }
        }
        public override void replaceStringValue(Saxon.Hej.str.UnicodeString stringValue) {
            if (stringValue.isEmpty()) {
                setChildren(null);
            } else {
                Saxon.Hej.tree.linked.TextImpl text = new Saxon.Hej.tree.linked.TextImpl(stringValue);
                text.setRawParent(this);
                setChildren(text);
            }
        }
        public void setAttributeInfo(int index, Saxon.Hej.om.AttributeInfo attInfo) {
            Saxon.Hej.tree.linked.AttributeMapWithIdentity attMap = prepareAttributesForUpdate();
            attMap = attMap.set(index, attInfo);
            setAttributes(attMap);
        }
        private Saxon.Hej.tree.linked.AttributeMapWithIdentity prepareAttributesForUpdate() {
            if ((attributes() is Saxon.Hej.tree.linked.AttributeMapWithIdentity)) {
                return (Saxon.Hej.tree.linked.AttributeMapWithIdentity)(attributes());
            } else {
                Saxon.Hej.tree.linked.AttributeMapWithIdentity newAtts = new Saxon.Hej.tree.linked.AttributeMapWithIdentity(attributes().asList());
                setAttributes(newAtts);
                return newAtts;
            }
        }
        public override void addAttribute(Saxon.Hej.om.NodeName nodeName, Saxon.Hej.type.SimpleType attType, string value, int properties, bool inheritNamespaces) {
            Saxon.Hej.tree.linked.AttributeMapWithIdentity atts = prepareAttributesForUpdate();
            atts = atts.add(new Saxon.Hej.om.AttributeInfo(nodeName, attType, value, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE));
            setAttributes(atts);
            if (!(nodeName.hasURI(Saxon.Hej.om.NamespaceUri.NULL))) {
                Saxon.Hej.om.NamespaceBinding binding = nodeName.getNamespaceBinding();
                string prefix = binding.getPrefix();
                Saxon.Hej.om.NamespaceUri uc = getURIForPrefix(prefix, false);
                if ((uc == null)) {
                    addNamespace(binding, inheritNamespaces);
                } else {
                    if (!(uc.Equals(binding.getNamespaceUri()))) {
                        throw new Saxon.Ejava.lang.IllegalStateException("Namespace binding of new name conflicts with existing namespace binding");
                    }
                }
            }
            if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.IS_ID)) {
                Saxon.Hej.tree.linked.DocumentImpl root = getPhysicalRoot();
                if ((root != null)) {
                    root.registerID(this, Saxon.Hej.value.Whitespace.trim(value));
                }
            }
        }
        public override void removeAttribute(Saxon.Hej.om.NodeInfo attribute) {
            if (!(((attribute is Saxon.Hej.tree.linked.AttributeImpl)))) {
                return ;
            }
            int index = ((Saxon.Hej.tree.linked.AttributeImpl)(attribute)).getSiblingPosition();
            Saxon.Hej.om.AttributeInfo info = attributes().itemAt(index);
            Saxon.Hej.tree.linked.AttributeMapWithIdentity atts = prepareAttributesForUpdate();
            atts = atts.remove(index);
            setAttributes(atts);
            if (((index>=0) && info.isId())) {
                Saxon.Hej.tree.linked.DocumentImpl root = getPhysicalRoot();
                root.deregisterID(info.getValue());
            }
            ((Saxon.Hej.tree.linked.AttributeImpl)(attribute)).setRawParent(null);
        }
        public override void removeNamespace(string prefix) {
            Saxon.Ejava.util.Objects.requireNonNull(prefix);
            if (prefix.Equals(getPrefix())) {
                throw new Saxon.Ejava.lang.IllegalStateException("Cannot remove binding of namespace prefix used on the element name");
            }
            foreach (Saxon.Hej.om.AttributeInfo att in attributeMap) {
                if (att.getNodeName().getPrefix().Equals(prefix)) {
                    throw new Saxon.Ejava.lang.IllegalStateException("Cannot remove binding of namespace prefix used on an existing attribute name");
                }
            }
            namespaceMap = namespaceMap.remove(prefix);
        }
        public override void addNamespace(string prefix, Saxon.Hej.om.NamespaceUri uri) {
            Saxon.Hej.om.NamespaceUri existingURI = namespaceMap.getNamespaceUri(prefix);
            if ((existingURI == null)) {
                namespaceMap = namespaceMap.put(prefix, uri);
            } else {
                if (!(existingURI.Equals(uri))) {
                    throw new Saxon.Ejava.lang.IllegalStateException("New namespace binding conflicts with existing namespace binding");
                }
            }
        }
        public override void removeTypeAnnotation() {
            if ((getSchemaType() != Saxon.Hej.type.Untyped.getInstance())) {
                type = Saxon.Hej.type.AnyType.getInstance();
                getRawParent().removeTypeAnnotation();
            }
        }
        public void setNamespaceMap(Saxon.Hej.om.NamespaceMap map) {
            namespaceMap = map;
        }
        public Saxon.Hej.om.NamespaceUri getURIForPrefix(string prefix, bool useDefault) {
            if (prefix.Length==0) {
                if (useDefault) {
                    return namespaceMap.getDefaultNamespace();
                } else {
                    return Saxon.Hej.om.NamespaceUri.NULL;
                }
            } else {
                return namespaceMap.getNamespaceUri(prefix);
            }
        }
        public System.Collections.Generic.IEnumerator<string> iteratePrefixes() {
            return namespaceMap.iteratePrefixes();
        }
        public bool isInScopeNamespace(Saxon.Hej.om.NamespaceUri uri) {
            foreach (Saxon.Hej.om.NamespaceBinding b in namespaceMap) {
                if (b.getNamespaceUri().Equals(uri)) {
                    return true;
                }
            }
            return false;
        }
        public override Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> bindings = new System.Collections.Generic.List<Saxon.Hej.om.NamespaceBinding>();
            foreach (Saxon.Hej.om.NamespaceBinding nb in namespaceMap) {
                bindings.Add(nb);
            }
            return bindings.ToArray();
        }
        protected internal void fixupInsertedNamespaces(bool inherit) {
            if ((getRawParent().getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                return ;
            }
            Saxon.Hej.tree.linked.ElementImpl parent = (Saxon.Hej.tree.linked.ElementImpl)(getRawParent());
            Saxon.Hej.om.NamespaceMap parentNamespaces = parent.namespaceMap;
            if (inherit) {
                deepAddNamespaces(parentNamespaces);
            }
        }
        private void deepAddNamespaces(Saxon.Hej.om.NamespaceMap inheritedNamespaces) {
            Saxon.Hej.om.NamespaceMap childNamespaces = namespaceMap;
            foreach (Saxon.Hej.om.NamespaceBinding binding in inheritedNamespaces) {
                if ((childNamespaces.getNamespaceUri(binding.getPrefix()) == null)) {
                    childNamespaces = childNamespaces.put(binding.getPrefix(), binding.getNamespaceUri());
                } else {
                    inheritedNamespaces = inheritedNamespaces.remove(binding.getPrefix());
                }
            }
            namespaceMap = childNamespaces;
            foreach (Saxon.Hej.om.NodeInfo child in children(Saxon.Hej.pattern.NodeSelector.of(x => (x is Saxon.Hej.tree.linked.ElementImpl)))) {
                ((Saxon.Hej.tree.linked.ElementImpl)(child)).deepAddNamespaces(inheritedNamespaces);
            }
        }
        public override Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return namespaceMap;
        }
        public override string getAttributeValue(Saxon.Hej.om.NamespaceUri uri, string localName) {
            return ((attributeMap == null) ? null : attributeMap.getValue(uri, localName));
        }
        public override bool isId() {
            try {
                Saxon.Hej.type.SchemaType type = getSchemaType();
                return ((type.getFingerprint() == Saxon.Hej.om.StandardNames.XS_ID) || (type.isIdType() && Saxon.Hej.om.NameChecker.isValidNCName(getUnicodeStringValue().codePoints())));
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return false;
            }
        }
        public override bool isIdref() {
            return isIdRefNode(this);
        }
        public static bool isIdRefNode(Saxon.Hej.tree.linked.NodeImpl node) {
            Saxon.Hej.type.SchemaType type = node.getSchemaType();
            try {
                if (type.isIdRefType()) {
                    if (((type == Saxon.Hej.type.BuiltInAtomicType.IDREF) || (type == Saxon.Hej.type.BuiltInListType.IDREFS))) {
                        return true;
                    }
                    try {
                        foreach (Saxon.Hej.value.AtomicValue av in node.atomize()) {
                            if (av.getItemType().isIdRefType()) {
                                return true;
                            }
                        }
                    } catch (Saxon.Hej.trans.XPathException err) {
                    }
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return false;
            }
            return false;
        }
    
    }
}