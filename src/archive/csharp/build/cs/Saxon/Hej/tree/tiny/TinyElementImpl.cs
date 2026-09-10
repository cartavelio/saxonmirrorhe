using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public class TinyElementImpl : Saxon.Hej.tree.tiny.TinyParentNodeImpl {
        public TinyElementImpl(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr)  {
            this.tree = tree;
            this.nodeNr = nodeNr;
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.ELEMENT;
        }
        public override string getBaseURI() {
            if ((tree.getUniformBaseUri() != null)) {
                return tree.getUniformBaseUri();
            }
            lock (tree) {
                if ((tree.knownBaseUris == null)) {
                    tree.knownBaseUris = new Saxon.Hej.z.IntHashMap<string>();
                }
                string uri = tree.knownBaseUris.get(nodeNr);
                if ((uri == null)) {
                    uri = Saxon.Hej.tree.util.Navigator.getBaseURI(this, n => tree.isTopWithinEntity(((Saxon.Hej.tree.tiny.TinyElementImpl)(n)).getNodeNumber()));
                    tree.knownBaseUris.put(nodeNr, uri);
                }
                return uri;
            }
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            return tree.getSchemaType(nodeNr);
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            return tree.getTypedValueOfElement(this);
        }
        public override Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            Saxon.Hej.tree.tiny.TinyNodeImpl parent = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getParent());
            if (((parent != null) && (parent.getNodeKind() == Saxon.Hej.type.Type.ELEMENT))) {
                return getAllNamespaces().getDifferences(parent.getAllNamespaces(), false);
            } else {
                return getAllNamespaces().getNamespaceBindings();
            }
        }
        public override Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return tree.namespaceMaps[tree.beta[nodeNr]];
        }
        public bool hasUniformNamespaces() {
            return false;
        }
        public override string getAttributeValue(string uri, string local) {
            int a = tree.alpha[nodeNr];
            if ((a<0)) {
                return null;
            }
            Saxon.Hej.om.NamePool pool = getNamePool();
            while (((a<tree.numberOfAttributes) && (tree.attParent[a] == nodeNr))) {
                int nc = tree.attCode[a];
                Saxon.Hej.om.StructuredQName name = pool.getUnprefixedQName(nc);
                if ((name.getLocalPart().Equals(local) && name.hasURI(uri))) {
                    return tree.attValue[a].ToString();
                }
                a++;
            }
            return null;
        }
        public virtual string getAttributeValue(int fp) {
            int a = tree.alpha[nodeNr];
            if ((a<0)) {
                return null;
            }
            while (((a<tree.numberOfAttributes) && (tree.attParent[a] == nodeNr))) {
                if ((fp == ((tree.attCode[a]&Saxon.Hej.om.NamePool.FP_MASK)))) {
                    return tree.attValue[a].ToString();
                }
                a++;
            }
            return null;
        }
        private int subtreeSize() {
            int next = tree.next[nodeNr];
            while ((next<nodeNr)) {
                if ((next<0)) {
                    return (tree.numberOfNodes-nodeNr);
                }
                next = tree.next[next];
            }
            return (nodeNr-next);
        }
        public override void copy(Saxon.Hej.@event.Receiver receiver, int copyOptions, Saxon.Hej.s9api.Location location) {
            bool copyTypes = Saxon.Hej.om.CopyOptions.includes(copyOptions, Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS);
            short level = -1;
            bool closePending = false;
            short startLevel = tree.depth[nodeNr];
            bool disallowNamespaceSensitiveContent = (((((copyOptions&Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS)) != 0)) && ((((copyOptions&Saxon.Hej.om.CopyOptions.ALL_NAMESPACES)) == 0)));
            Saxon.Hej.Configuration config = tree.getConfiguration();
            Saxon.Hej.om.NamePool pool = config.getNamePool();
            int next = nodeNr;
            System.Func<Saxon.Hej.om.NodeInfo,System.Object> informee = receiver.getPipelineConfiguration().getCopyInformee();
            Saxon.Hej.type.SchemaType elementType = Saxon.Hej.type.Untyped.getInstance();
            Saxon.Hej.type.SimpleType attributeType = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
            do {
                short nodeLevel = tree.depth[next];
                if (closePending) {
                    level++;
                }
                for (; (level>nodeLevel); level--) {
                    receiver.endElement();
                }
                level = nodeLevel;
                int kind = tree.nodeKind[next];
                switch (kind) {
                  case Saxon.Hej.type.Type.ELEMENT:
                  case Saxon.Hej.type.Type.TEXTUAL_ELEMENT:
                    {
                        if (copyTypes) {
                            elementType = tree.getSchemaType(next);
                            if (disallowNamespaceSensitiveContent) {
                                try {
                                    checkNotNamespaceSensitiveElement(elementType, next);
                                } catch (Saxon.Hej.@event.CopyNamespaceSensitiveException e) {
                                    e.setErrorCode((receiver.getPipelineConfiguration().isXSLT() ? "XTTE0950" : "XQTY0086"));
                                    throw;
                                }
                            }
                        }
                        if ((informee != null)) {
                            Saxon.Hej.s9api.Location loc = (Saxon.Hej.s9api.Location)(informee(((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(next))));
                            if ((loc != null)) {
                                location = loc;
                            }
                        }
                        int nameCode = tree.nameCode[next];
                        int fp = (nameCode&Saxon.Hej.om.NamePool.FP_MASK);
                        string prefix = tree.getPrefix(next);
                        if ((location.getLineNumber()<tree.getLineNumber(next))) {
                            string systemId = ((location.getSystemId() == null) ? getSystemId() : location.getSystemId());
                            location = new Saxon.Hej.expr.parser.Loc(systemId, tree.getLineNumber(next), getColumnNumber());
                        }
                        Saxon.Hej.om.NamespaceMap namespaces = Saxon.Hej.om.NamespaceMap.emptyMap();
                        bool addAttributeNamespaces = false;
                        if (tree.usesNamespaces) {
                            if ((((copyOptions&Saxon.Hej.om.CopyOptions.ALL_NAMESPACES)) != 0)) {
                                if ((kind == Saxon.Hej.type.Type.TEXTUAL_ELEMENT)) {
                                    int parent = getParentNodeNr(tree, next);
                                    namespaces = tree.namespaceMaps[tree.beta[parent]];
                                } else {
                                    namespaces = tree.namespaceMaps[tree.beta[next]];
                                }
                            } else {
                                addAttributeNamespaces = true;
                                string uri = pool.getURI(nameCode);
                                if (!(uri.Length==0)) {
                                    namespaces = Saxon.Hej.om.NamespaceMap.of(prefix, uri);
                                }
                            }
                        }
                        if ((kind == Saxon.Hej.type.Type.TEXTUAL_ELEMENT)) {
                            closePending = false;
                            receiver.startElement(new Saxon.Hej.om.CodedName(fp, prefix, pool), elementType, Saxon.Hej.om.EmptyAttributeMap.getInstance(), namespaces, location, Saxon.Hej.@event.ReceiverOption.BEQUEATH_INHERITED_NAMESPACES_ONLY);
                            Saxon.Hej.str.UnicodeString value = Saxon.Hej.tree.tiny.TinyTextImpl.getStringValue(tree, next);
                            receiver.characters(value, location, Saxon.Hej.@event.ReceiverOption.WHOLE_TEXT_NODE);
                            receiver.endElement();
                        } else {
                            closePending = true;
                            Saxon.Hej.om.AttributeMap attributes = Saxon.Hej.om.EmptyAttributeMap.getInstance();
                            int att = tree.alpha[next];
                            if ((att>=0)) {
                                while (((att<tree.numberOfAttributes) && (tree.attParent[att] == next))) {
                                    int attCode = tree.attCode[att];
                                    int attfp = (attCode&Saxon.Hej.om.NamePool.FP_MASK);
                                    if (copyTypes) {
                                        attributeType = tree.getAttributeType(att);
                                        if (disallowNamespaceSensitiveContent) {
                                            try {
                                                checkNotNamespaceSensitiveAttribute(attributeType, att);
                                            } catch (Saxon.Hej.@event.CopyNamespaceSensitiveException e) {
                                                e.setErrorCode((receiver.getPipelineConfiguration().isXSLT() ? "XTTE0950" : "XQTY0086"));
                                                throw;
                                            }
                                        }
                                    }
                                    string attPrefix = tree.prefixPool.getPrefix((attCode>>20));
                                    int attProps = Saxon.Hej.@event.ReceiverOption.NOT_A_DUPLICATE;
                                    if (tree.isIdAttribute(att)) {
                                        attProps |= Saxon.Hej.@event.ReceiverOption.IS_ID;
                                    }
                                    if (tree.isIdrefAttribute(att)) {
                                        attProps |= Saxon.Hej.@event.ReceiverOption.IS_IDREF;
                                    }
                                    attributes = attributes.put(new Saxon.Hej.om.AttributeInfo(new Saxon.Hej.om.CodedName(attfp, attPrefix, pool), attributeType, tree.attValue[att].ToString(), location, attProps));
                                    if ((addAttributeNamespaces && !(attPrefix.Length==0))) {
                                        namespaces = namespaces.put(attPrefix, pool.getURI(attCode));
                                    }
                                    att++;
                                }
                            }
                            receiver.startElement(new Saxon.Hej.om.CodedName(fp, prefix, pool), elementType, attributes, namespaces, location, Saxon.Hej.@event.ReceiverOption.BEQUEATH_INHERITED_NAMESPACES_ONLY);
                        }
                        break ;
                    }
                  case Saxon.Hej.type.Type.TEXT:
                    {
                        closePending = false;
                        Saxon.Hej.str.UnicodeString value = Saxon.Hej.tree.tiny.TinyTextImpl.getStringValue(tree, next);
                        receiver.characters(value, location, Saxon.Hej.@event.ReceiverOption.WHOLE_TEXT_NODE);
                        break ;
                    }
                  case Saxon.Hej.type.Type.WHITESPACE_TEXT:
                    {
                        closePending = false;
                        long compressedValue = ((((long)(tree.alpha[next])<<32))|(((long)(tree.beta[next])&unchecked((long)0xffffffffL))));
                        Saxon.Hej.str.UnicodeString value = new Saxon.Hej.str.CompressedWhitespace(compressedValue);
                        receiver.characters(value, location, Saxon.Hej.@event.ReceiverOption.WHOLE_TEXT_NODE);
                        break ;
                    }
                  case Saxon.Hej.type.Type.COMMENT:
                    {
                        closePending = false;
                        int start = tree.alpha[next];
                        int len = tree.beta[next];
                        if ((len>0)) {
                            receiver.comment(tree.commentBuffer.substring(start, (start+len)), location, Saxon.Hej.@event.ReceiverOption.NONE);
                        } else {
                            receiver.comment(Saxon.Hej.str.EmptyUnicodeString.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                        }
                        break ;
                    }
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    {
                        closePending = false;
                        Saxon.Hej.om.NodeInfo pi = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(next));
                        receiver.processingInstruction(pi.getLocalPart(), pi.getUnicodeStringValue(), location, Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                    }
                  case Saxon.Hej.type.Type.PARENT_POINTER:
                    {
                        closePending = false;
                        break ;
                    }
                }
                next++;
            } while (((next<tree.numberOfNodes) && (tree.depth[next]>startLevel)));
            if (closePending) {
                level++;
            }
            for (; (level>startLevel); level--) {
                receiver.endElement();
            }
        }
        protected internal void checkNotNamespaceSensitiveElement(Saxon.Hej.type.SchemaType type, int nodeNr) {
            if (((type is Saxon.Hej.type.SimpleType) && ((Saxon.Hej.type.SimpleType)(type)).isNamespaceSensitive())) {
                if (type.isAtomicType()) {
                    throw new Saxon.Hej.@event.CopyNamespaceSensitiveException("Cannot copy QName or NOTATION values without copying namespaces");
                } else {
                    Saxon.Hej.om.AtomicSequence value = tree.getTypedValueOfElement(nodeNr);
                    foreach (Saxon.Hej.value.AtomicValue val in value) {
                        if (val.getPrimitiveType().isNamespaceSensitive()) {
                            throw new Saxon.Hej.@event.CopyNamespaceSensitiveException("Cannot copy QName or NOTATION values without copying namespaces");
                        }
                    }
                }
            }
        }
        private void checkNotNamespaceSensitiveAttribute(Saxon.Hej.type.SimpleType type, int nodeNr) {
            if (type.isNamespaceSensitive()) {
                if (type.isAtomicType()) {
                    throw new Saxon.Hej.@event.CopyNamespaceSensitiveException("Cannot copy QName or NOTATION values without copying namespaces");
                } else {
                    Saxon.Hej.om.AtomicSequence value = tree.getTypedValueOfAttribute(null, nodeNr);
                    foreach (Saxon.Hej.value.AtomicValue val in value) {
                        if (val.getPrimitiveType().isNamespaceSensitive()) {
                            throw new Saxon.Hej.@event.CopyNamespaceSensitiveException("Cannot copy QName or NOTATION values without copying namespaces");
                        }
                    }
                }
            }
        }
        public string getURIForPrefix(string prefix, bool useDefault) {
            if ((!(useDefault) && (((prefix == null) || prefix.Length==0)))) {
                return "";
            }
            int ns = tree.beta[nodeNr];
            Saxon.Hej.om.NamespaceMap map = tree.namespaceMaps[ns];
            return map.getURIForPrefix(prefix, useDefault);
        }
        public override bool isId() {
            return tree.isIdElement(nodeNr);
        }
        public override bool isIdref() {
            return tree.isIdrefElement(nodeNr);
        }
        private bool isSkipValidator(Saxon.Hej.@event.Receiver r) {
            if ((r is Saxon.Eej.ee.validate.SkipValidator)) {
                return true;
            }
            return false;
        }
    
    }
}