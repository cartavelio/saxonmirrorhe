using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public sealed class ComplexContentOutputter : Saxon.Hej.@event.Outputter, Saxon.Hej.@event.Receiver, Saxon.Ejavax.xml.transform.Result {
        private Saxon.Hej.@event.Receiver nextReceiver;
        private Saxon.Hej.om.NodeName pendingStartTag = null;
        private int level = -1;
        private bool[] currentLevelIsDocument = new bool[20];
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> pendingAttributes = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>();
        private Saxon.Hej.om.NamespaceMap pendingNSMap;
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap> inheritedNamespaces = new System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap>();
        private Saxon.Hej.type.SchemaType currentSimpleType = null;
        private int startElementProperties;
        private Saxon.Hej.s9api.Location startElementLocationId = Saxon.Hej.expr.parser.Loc.NONE;
        private Saxon.Api.HostLanguage hostLanguage = Saxon.Api.HostLanguage.XSLT;
        private Saxon.Hej.@event.RegularSequenceChecker.State state = /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.INITIAL;
        private bool previousAtomic = false;
        public ComplexContentOutputter(Saxon.Hej.@event.Receiver next)  {
            Saxon.Hej.@event.PipelineConfiguration pipe = next.getPipelineConfiguration();
            setPipelineConfiguration(pipe);
            setReceiver(next);
            Saxon.Ejava.util.Objects.requireNonNull(pipe);
            setHostLanguage(pipe.getHostLanguage());
            inheritedNamespaces.Push(Saxon.Hej.om.NamespaceMap.emptyMap());
        }
        public static Saxon.Hej.@event.ComplexContentOutputter makeComplexContentReceiver(Saxon.Hej.@event.Receiver receiver, Saxon.Hej.lib.ParseOptions options) {
            string systemId = receiver.getSystemId();
            bool validate = ((options != null) && (options.getSchemaValidationMode() != Saxon.Hej.lib.Validation.PRESERVE));
            if (validate) {
                Saxon.Hej.Configuration config = receiver.getPipelineConfiguration().getConfiguration();
                receiver = config.getDocumentValidator(receiver, systemId, options, null);
            }
            Saxon.Hej.@event.ComplexContentOutputter result = new Saxon.Hej.@event.ComplexContentOutputter(receiver);
            result.setSystemId(systemId);
            return result;
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            if ((pipelineConfiguration != pipe)) {
                pipelineConfiguration = pipe;
                if ((nextReceiver != null)) {
                    nextReceiver.setPipelineConfiguration(pipe);
                }
            }
        }
        public override void setSystemId(string systemId) {
            base.setSystemId(systemId);
            nextReceiver.setSystemId(systemId);
        }
        public void setHostLanguage(Saxon.Api.HostLanguage language) {
            hostLanguage = language;
        }
        public void setReceiver(Saxon.Hej.@event.Receiver receiver) {
            this.nextReceiver = receiver;
        }
        public Saxon.Hej.@event.Receiver getReceiver() {
            return nextReceiver;
        }
        public override void open() {
            nextReceiver.open();
            previousAtomic = false;
            state = /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.OPEN;
        }
        public override void startDocument(int properties) {
            level++;
            if ((level == 0)) {
                nextReceiver.startDocument(properties);
            } else {
                if ((state == /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG)) {
                    startContent();
                }
            }
            previousAtomic = false;
            if ((currentLevelIsDocument.Length<(level+1))) {
                currentLevelIsDocument = Saxon.Ejava.util.Arrays.copyOf(currentLevelIsDocument, (level*2));
            }
            currentLevelIsDocument[level] = true;
            state = /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT;
        }
        public override void endDocument() {
            if ((level == 0)) {
                nextReceiver.endDocument();
            }
            previousAtomic = false;
            level--;
            state = ((level<0) ? /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.OPEN : /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT);
        }
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
            nextReceiver.setUnparsedEntity(name, systemID, publicID);
        }
        public override void characters(Saxon.Hej.str.UnicodeString s, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level>=0)) {
                previousAtomic = false;
                if ((s == null)) {
                    return ;
                }
                if (s.isEmpty()) {
                    return ;
                }
                if ((state == /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG)) {
                    startContent();
                }
            }
            nextReceiver.characters(s, locationId, properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            level++;
            if ((state == /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG)) {
                startContent();
            }
            startElementProperties = properties;
            startElementLocationId = location.saveLocation();
            pendingAttributes.Clear();
            pendingNSMap = Saxon.Hej.om.NamespaceMap.emptyMap();
            pendingStartTag = elemName;
            currentSimpleType = typeCode;
            previousAtomic = false;
            if ((currentLevelIsDocument.Length<(level+1))) {
                currentLevelIsDocument = Saxon.Ejava.util.Arrays.copyOf(currentLevelIsDocument, (level*2));
            }
            currentLevelIsDocument[level] = false;
            state = /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG;
        }
        public override void @namespace(string prefix, string namespaceUri, int properties) {
            Saxon.Ejava.util.Objects.requireNonNull(prefix);
            Saxon.Ejava.util.Objects.requireNonNull(namespaceUri);
            if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.NAMESPACE_OK)) {
                pendingNSMap = pendingNSMap.put(prefix, namespaceUri);
            } else {
                if ((level>=0)) {
                    if ((state != /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG)) {
                        throw Saxon.Hej.@event.NoOpenStartTagException.makeNoOpenStartTagException(Saxon.Hej.type.Type.NAMESPACE, prefix, hostLanguage, currentLevelIsDocument[level], startElementLocationId);
                    }
                    bool elementIsInNullNamespace = pendingStartTag.hasURI("");
                    if ((prefix.Length==0 && !(namespaceUri.Length==0))) {
                        if (elementIsInNullNamespace) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Cannot output a namespace node for the default namespace ("+namespaceUri)+") when the element is in no namespace"));
                            err.setErrorCode(((hostLanguage == Saxon.Api.HostLanguage.XSLT) ? "XTDE0440" : "XQDY0102"));
                            throw err;
                        }
                    }
                    bool rejectDuplicates = Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.REJECT_DUPLICATES);
                    if (rejectDuplicates) {
                        string uri = pendingNSMap.getURI(prefix);
                        if (((uri != null) && !(uri.Equals(namespaceUri)))) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((((((("Cannot create two namespace nodes with the same prefix "+"mapped to different URIs (prefix=\"")+prefix)+"\", URIs=(")+uri)+"\", \"")+namespaceUri)+"\")"));
                            err.setErrorCode(((hostLanguage == Saxon.Api.HostLanguage.XSLT) ? "XTDE0430" : "XQDY0102"));
                            throw err;
                        }
                    }
                    pendingNSMap = pendingNSMap.put(prefix, namespaceUri);
                } else {
                    Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                    orphan.setNodeKind(Saxon.Hej.type.Type.NAMESPACE);
                    orphan.setNodeName(new Saxon.Hej.om.NoNamespaceName(prefix));
                    orphan.setStringValue(Saxon.Hej.str.StringView.tidy(namespaceUri));
                    nextReceiver.append(orphan, Saxon.Hej.expr.parser.Loc.NONE, properties);
                }
            }
            previousAtomic = false;
        }
        public override void namespaces(Saxon.Hej.om.NamespaceBindingSet bindings, int properties) {
            if ((((bindings is Saxon.Hej.om.NamespaceMap) && pendingNSMap.isEmpty()) && Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.NAMESPACE_OK))) {
                pendingNSMap = (Saxon.Hej.om.NamespaceMap)(bindings);
            } else {
                base.namespaces(bindings, properties);
            }
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location locationId, int properties) {
            if (((level>=0) && (state != /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG))) {
                Saxon.Hej.trans.XPathException err = Saxon.Hej.@event.NoOpenStartTagException.makeNoOpenStartTagException(Saxon.Hej.type.Type.ATTRIBUTE, attName.getDisplayName(), hostLanguage, currentLevelIsDocument[level], startElementLocationId);
                err.setLocator(locationId);
                throw err;
            }
            Saxon.Hej.om.AttributeInfo attInfo = new Saxon.Hej.om.AttributeInfo(attName, typeCode, value, locationId, properties);
            if (((level>=0) && !(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.NOT_A_DUPLICATE)))) {
                for (int a = 0; (a<pendingAttributes.Count); a++) {
                    if (pendingAttributes[a].getNodeName().Equals(attName)) {
                        if ((hostLanguage == Saxon.Api.HostLanguage.XSLT)) {
                            pendingAttributes[a] = attInfo;
                            return ;
                        } else {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot create an element having two attributes with the same name: "+Saxon.Hej.trans.Err.wrap(attName.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE)));
                            err.setErrorCode("XQDY0025");
                            throw err;
                        }
                    }
                }
            }
            if ((((level == 0) && !(typeCode.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) && currentLevelIsDocument[0])) {
                if (typeCode.isNamespaceSensitive()) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot copy attributes whose type is namespace-sensitive (QName or NOTATION): "+Saxon.Hej.trans.Err.wrap(attName.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE)));
                    err.setErrorCode(((hostLanguage == Saxon.Api.HostLanguage.XSLT) ? "XTTE0950" : "XQTY0086"));
                    throw err;
                }
            }
            if ((level<0)) {
                Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                orphan.setNodeKind(Saxon.Hej.type.Type.ATTRIBUTE);
                orphan.setNodeName(attName);
                orphan.setTypeAnnotation(typeCode);
                orphan.setStringValue(Saxon.Hej.str.StringView.tidy(value));
                nextReceiver.append(orphan, locationId, properties);
            }
            pendingAttributes.Add(attInfo);
            previousAtomic = false;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((state == /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG)) {
                startContent();
            }
            level++;
            startElementLocationId = location.saveLocation();
            if ((currentLevelIsDocument.Length<(level+1))) {
                currentLevelIsDocument = Saxon.Ejava.util.Arrays.copyOf(currentLevelIsDocument, (level*2));
            }
            currentLevelIsDocument[level] = false;
            if ((elemName.hasURI("") && !(namespaces.getDefaultNamespace().Length==0))) {
                namespaces = namespaces.remove("");
            }
            bool inherit = !(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.DISINHERIT_NAMESPACES));
            Saxon.Hej.om.NamespaceMap ns2;
            if (inherit) {
                Saxon.Hej.om.NamespaceMap inherited = inheritedNamespaces.Peek();
                if ((!(inherited.getDefaultNamespace().Length==0) && elemName.getURI().Length==0)) {
                    inherited = inherited.remove("");
                }
                ns2 = inherited.putAll(namespaces);
                if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.BEQUEATH_INHERITED_NAMESPACES_ONLY)) {
                    inheritedNamespaces.Push(inherited);
                } else {
                    inheritedNamespaces.Push(ns2);
                }
            } else {
                ns2 = namespaces;
                inheritedNamespaces.Push(Saxon.Hej.om.NamespaceMap.emptyMap());
            }
            bool refuseInheritedNamespaces = Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.REFUSE_NAMESPACES);
            Saxon.Hej.om.NamespaceMap ns3 = (refuseInheritedNamespaces ? namespaces : ns2);
            nextReceiver.startElement(elemName, type, attributes, ns3, location, properties);
            state = /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT;
        }
        private Saxon.Hej.om.NodeName checkProposedPrefix(Saxon.Hej.om.NodeName nodeName, int seq) {
            string nodePrefix = nodeName.getPrefix();
            string nodeURI = nodeName.getURI();
            if (nodeURI.Length==0) {
                return nodeName;
            } else {
                string uri = pendingNSMap.getURI(nodePrefix);
                if ((uri == null)) {
                    pendingNSMap = pendingNSMap.put(nodePrefix, nodeURI);
                    return nodeName;
                } else {
                    if (nodeURI.Equals(uri)) {
                        return nodeName;
                    } else {
                        string newPrefix = getSubstitutePrefix(nodePrefix, nodeURI, seq);
                        Saxon.Hej.om.NodeName newName = new Saxon.Hej.om.FingerprintedQName(newPrefix, nodeURI, nodeName.getLocalPart());
                        pendingNSMap = pendingNSMap.put(newPrefix, nodeURI);
                        return newName;
                    }
                }
            }
        }
        private string getSubstitutePrefix(string prefix, string uri, int seq) {
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML)) {
                return "xml";
            }
            return ((prefix+'_')+seq);
        }
        public override void endElement() {
            if ((state == /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG)) {
                startContent();
            } else {
                pendingStartTag = null;
            }
            nextReceiver.endElement();
            level--;
            previousAtomic = false;
            state = ((level<0) ? /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.OPEN : /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT);
            inheritedNamespaces.Pop();
        }
        public override void comment(Saxon.Hej.str.UnicodeString comment, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level>=0)) {
                if ((state == /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG)) {
                    startContent();
                }
                previousAtomic = false;
            }
            nextReceiver.comment(comment, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level>=0)) {
                if ((state == /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG)) {
                    startContent();
                }
                previousAtomic = false;
            }
            nextReceiver.processingInstruction(target, data, locationId, properties);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((level>=0)) {
                decompose(item, locationId, copyNamespaces);
            } else {
                nextReceiver.append(item, locationId, copyNamespaces);
            }
        }
        public override Saxon.Hej.str.UniStringConsumer getStringReceiver(bool asTextNode) {
            if ((level>=0)) {
                return new Saxon.Hej.@event.ComplexContentOutputter.UnicodeStringReceiver(this, previousAtomic, asTextNode, prev => {
                    previousAtomic = prev;
                } );
            } else {
                return base.getStringReceiver(asTextNode);
            }
        }
        private class UnicodeStringReceiver : Saxon.Hej.str.UniStringConsumer {
            private readonly Saxon.Hej.@event.ComplexContentOutputter cco;
            private readonly bool previousAtomic;
            private readonly bool asTextNode;
            private readonly Saxon.Ejava.util.function.Consumer<bool> resetPreviousAtomic;
            public UnicodeStringReceiver(Saxon.Hej.@event.ComplexContentOutputter cco, bool previousAtomic, bool asTextNode, Saxon.Ejava.util.function.Consumer<bool> resetPreviousAtomic)  {
                this.cco = cco;
                this.previousAtomic = previousAtomic;
                this.asTextNode = asTextNode;
                this.resetPreviousAtomic = resetPreviousAtomic;
            }
            public void open() {
                if ((previousAtomic && !(asTextNode))) {
                    cco.characters(Saxon.Hej.str.StringConstants.SINGLE_SPACE, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                }
            }
            public Saxon.Hej.str.UniStringConsumer accept(Saxon.Hej.str.UnicodeString chars) {
                cco.characters(chars, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                return this;
            }
            public void close() {
                resetPreviousAtomic(!(asTextNode));
            }
        
        }
        public override void close() {
            nextReceiver.close();
            previousAtomic = false;
            state = /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.FINAL;
        }
        public override void startContent() {
            if ((state != /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.START_TAG)) {
                return ;
            }
            Saxon.Hej.om.NodeName elcode = checkProposedPrefix(pendingStartTag, 0);
            int props = (startElementProperties|Saxon.Hej.@event.ReceiverOption.NAMESPACE_OK);
            for (int a = 0; (a<pendingAttributes.Count); a++) {
                Saxon.Hej.om.NodeName oldName = pendingAttributes[a].getNodeName();
                if (!(oldName.hasURI(""))) {
                    Saxon.Hej.om.NodeName newName = checkProposedPrefix(oldName, (a+1));
                    if ((newName != oldName)) {
                        Saxon.Hej.om.AttributeInfo newInfo = pendingAttributes[a].withNodeName(newName);
                        pendingAttributes[a] = newInfo;
                    }
                }
            }
            Saxon.Hej.om.NamespaceMap inherited = ((inheritedNamespaces.Count==0) ? Saxon.Hej.om.NamespaceMap.emptyMap() : inheritedNamespaces.Peek());
            if (!(Saxon.Hej.@event.ReceiverOption.contains(startElementProperties, Saxon.Hej.@event.ReceiverOption.REFUSE_NAMESPACES))) {
                pendingNSMap = inherited.putAll(pendingNSMap);
            }
            if ((pendingStartTag.hasURI("") && !(pendingNSMap.getDefaultNamespace().Length==0))) {
                pendingNSMap = pendingNSMap.remove("");
            }
            Saxon.Hej.om.AttributeMap attributes = Saxon.Hej.om.SequenceTool.attributeMapFromList(pendingAttributes);
            nextReceiver.startElement(elcode, currentSimpleType, attributes, pendingNSMap, startElementLocationId, props);
            bool inherit = !(Saxon.Hej.@event.ReceiverOption.contains(startElementProperties, Saxon.Hej.@event.ReceiverOption.DISINHERIT_NAMESPACES));
            inheritedNamespaces.Push((inherit ? pendingNSMap : inherited));
            pendingAttributes.Clear();
            pendingNSMap = Saxon.Hej.om.NamespaceMap.emptyMap();
            previousAtomic = false;
            state = /*explicit enum*/ Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT;
        }
        public override bool usesTypeAnnotations() {
            return nextReceiver.usesTypeAnnotations();
        }
        private void flatten(Saxon.Hej.ma.arrays.ArrayItem array, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            foreach (Saxon.Hej.om.Sequence member in array.members()) {
                Saxon.Hej.om.SequenceTool.supply(member.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((it => append(it, locationId, copyNamespaces))));
            }
        }
        private void decompose(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((item != null)) {
                Saxon.Hej.om.Genre genre = item.getGenre();
                switch (genre) {
                  case Saxon.Hej.om.Genre.ATOMIC:
                  case Saxon.Hej.om.Genre.EXTERNAL:
                    if (previousAtomic) {
                        characters(Saxon.Hej.str.StringConstants.SINGLE_SPACE, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    characters(item.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    previousAtomic = true;
                    break ;
                  case Saxon.Hej.om.Genre.ARRAY:
                    flatten((Saxon.Hej.ma.arrays.ArrayItem)(item), locationId, copyNamespaces);
                    break ;
                  case Saxon.Hej.om.Genre.FUNCTION:
                  case Saxon.Hej.om.Genre.MAP:
                    string thing = ((item is Saxon.Hej.ma.map.MapItem) ? "map" : "function item");
                    string errorCode = getErrorCodeForDecomposingFunctionItems();
                    if (errorCode.StartsWith("SENR")) {
                        throw new Saxon.Hej.trans.XPathException((("Cannot serialize a "+thing)+" using this output method"), errorCode, locationId);
                    } else {
                        throw new Saxon.Hej.trans.XPathException((("Cannot add a "+thing)+" to an XDM node tree"), errorCode, locationId);
                    }
                  case Saxon.Hej.om.Genre.NODE:
                  default:
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                    switch (node.getNodeKind()) {
                      case Saxon.Hej.type.Type.TEXT:
                        int options = Saxon.Hej.@event.ReceiverOption.NONE;
                        if (((node is Saxon.Hej.tree.util.Orphan) && ((Saxon.Hej.tree.util.Orphan)(node)).isDisableOutputEscaping())) {
                            options = Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING;
                        }
                        characters(item.getUnicodeStringValue(), locationId, options);
                        break ;
                      case Saxon.Hej.type.Type.ATTRIBUTE:
                        if (((Saxon.Hej.type.SimpleType)(node.getSchemaType())).isNamespaceSensitive()) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot copy attributes whose type is namespace-sensitive (QName or NOTATION): "+Saxon.Hej.trans.Err.wrap(node.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE)));
                            err.setErrorCode((getPipelineConfiguration().isXSLT() ? "XTTE0950" : "XQTY0086"));
                            throw err;
                        }
                        attribute(Saxon.Hej.om.NameOfNode.makeName(node), (Saxon.Hej.type.SimpleType)(node.getSchemaType()), node.getStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                      case Saxon.Hej.type.Type.NAMESPACE:
                        @namespace(node.getLocalPart(), node.getStringValue(), Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                      case Saxon.Hej.type.Type.DOCUMENT:
                        startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                        foreach (Saxon.Hej.om.NodeInfo child in node.children()) {
                            append(child, locationId, copyNamespaces);
                        }
                        endDocument();
                        break ;
                      default:
                        int copyOptions = Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS;
                        if (Saxon.Hej.@event.ReceiverOption.contains(copyNamespaces, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES)) {
                            copyOptions |= Saxon.Hej.om.CopyOptions.ALL_NAMESPACES;
                        }
                        ((Saxon.Hej.om.NodeInfo)(item)).copy(this, copyOptions, locationId);
                        break ;
                    }
                    previousAtomic = false;
                    break ;
                }
            }
        }
        private string getErrorCodeForDecomposingFunctionItems() {
            return (getPipelineConfiguration().isXSLT() ? "XTDE0450" : "XQTY0105");
        }
    
    }
}