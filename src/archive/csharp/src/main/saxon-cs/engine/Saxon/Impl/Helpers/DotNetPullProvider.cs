using System;
using SysCollGen = System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using Saxon.Hej.str;
using Saxon.Api;
using Saxon.Hej.@event;
using Saxon.Hej.expr.parser;
using Saxon.Hej.type;
using Saxon.Hej.value;
using WSP = Saxon.Api.WhitespacePolicy;

namespace System.Impl.Helpers {
    public class DotNetPullProvider : Saxon.Hej.pull.PullProvider, Saxon.Hej.s9api.Location {
        private Saxon.Hej.@event.PipelineConfiguration pipe;
        private readonly XmlReader parser;
        private string baseURI;
        private bool isEmptyElement = false;
        private Saxon.Hej.pull.PullEvent? currentEvent = Saxon.Hej.pull.PullEvent.START_OF_INPUT;
        private bool expandDefaults = true;
        
        private readonly SysCollGen.Stack<Saxon.Hej.om.NamespaceMap> namespaceStack = new();
        public bool AcceptFragment { get; set; }
        public bool IgnoreIgnorableWhitespace { get; set; } 

        public DotNetPullProvider(XmlReader parser) {
            this.parser = parser;
            namespaceStack.Push(Saxon.Hej.om.NamespaceMap.emptyMap());
        }

        public void setBaseURI(string @base) {
            baseURI = @base;
        }

        public void close() {
            parser.Close();
        }

        public Saxon.Hej.pull.PullEvent current() {
            return currentEvent.Value;
        }

        public Saxon.Hej.value.AtomicValue getAtomicValue() {
            return null;
        }

        public Saxon.Hej.om.AttributeMap getAttributes() {
            if (parser.HasAttributes) {
                Saxon.Hej.om.AttributeMap atts = Saxon.Hej.om.EmptyAttributeMap.getInstance();
                Saxon.Hej.s9api.Location location = saveLocation();
                for (int i = 0; (i < parser.AttributeCount); i++) {
                    parser.MoveToAttribute(i);
                    string prefix = parser.Prefix;
                    string namespaceURI = parser.NamespaceURI;
                    string localName = parser.LocalName;
                    if (("xmlns".Equals(prefix) || (("".Equals(prefix) && "xmlns".Equals(localName))))) {
                    }
                    else {
                        if ((expandDefaults || !(parser.IsDefault))) {
                            Saxon.Hej.om.NodeName nc =
                                new Saxon.Hej.om.FingerprintedQName(prefix, namespaceURI, localName);
                            atts = atts.put(new Saxon.Hej.om.AttributeInfo(nc,
                                Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, parser.Value,
                                location, Saxon.Hej.@event.ReceiverOption.NONE));
                        }
                    }
                }

                parser.MoveToElement();
                return atts;
            }
            else {
                return Saxon.Hej.om.EmptyAttributeMap.getInstance();
            }
        }

        public Saxon.Hej.om.NodeName getNodeName() {
            return new Saxon.Hej.om.FingerprintedQName(parser.Prefix, parser.NamespaceURI, parser.LocalName);
        }

        public Saxon.Hej.om.NamespaceBinding[] getNamespaceDeclarations() {
            if (parser.HasAttributes) {
                int limit = parser.AttributeCount;
                Saxon.Hej.om.NamespaceBinding[] nsBindings = new Saxon.Hej.om.NamespaceBinding[limit];
                int used = 0;
                for (int i = 0; (i < limit); i++) {
                    parser.MoveToAttribute(i);
                    string prefix = parser.Prefix;
                    string localName = parser.LocalName;
                    if ("xmlns".Equals(prefix)) {
                        Saxon.Hej.om.NamespaceBinding nscode =
                            new Saxon.Hej.om.NamespaceBinding(localName, parser.Value);
                        nsBindings[used++] = nscode;
                    }
                    else {
                        if (((prefix.Length == 0) && "xmlns".Equals(localName))) {
                            Saxon.Hej.om.NamespaceBinding nscode = new Saxon.Hej.om.NamespaceBinding("", parser.Value);
                            nsBindings[used++] = nscode;
                        }
                        else {
                        }
                    }
                }

                if ((used < limit)) {
                    nsBindings[used] = null;
                }

                parser.MoveToElement();
                return nsBindings;
            }
            else {
                return Saxon.Hej.om.NamespaceBinding.EMPTY_ARRAY;
            }
        }

        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }

        public Saxon.Hej.s9api.Location getSourceLocator() {
            if (((parser is IXmlLineInfo) /* || (parser is XmlValidatingReader) */)) {
                return this;
            }
            else {
                return null;
            }
        }

        public Saxon.Hej.str.UnicodeString getStringValue() {
            try {
                return StringView.of(parser.Value);
            }
            catch (System.Xml.XmlException e) {
                throw new Saxon.Hej.trans.XPathException(e); // TODO: similar exception code throughout
            }
        }

        public Saxon.Hej.type.SchemaType getSchemaType() {
            return Saxon.Hej.type.Untyped.getInstance();
        }

        public Saxon.Hej.pull.PullEvent next() {
            if ((currentEvent == Saxon.Hej.pull.PullEvent.START_OF_INPUT)) {
                currentEvent = Saxon.Hej.pull.PullEvent.START_DOCUMENT;
                return currentEvent.Value;
            }
            else if (((currentEvent == Saxon.Hej.pull.PullEvent.END_DOCUMENT) ||
                      (currentEvent == Saxon.Hej.pull.PullEvent.END_OF_INPUT))) {
                currentEvent = Saxon.Hej.pull.PullEvent.END_OF_INPUT;
                return currentEvent.Value;
            }
            else if (((currentEvent == Saxon.Hej.pull.PullEvent.START_ELEMENT) && isEmptyElement)) {
                currentEvent = Saxon.Hej.pull.PullEvent.END_ELEMENT;
                return currentEvent.Value;
            }

            do {
                try {
                    parser.Read();
                }
                catch (XmlException e) {
                    Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(
                        ("Error reported by XML parser"
                         + (baseURI == null ? ": " : " processing " + baseURI + ": ")
                         + e.Message), e);
                    Saxon.Hej.expr.parser.Loc loc =
                        new Saxon.Hej.expr.parser.Loc(getSystemId(), e.LineNumber, e.LinePosition);
                    de.setLocator(loc);
                    throw de;
                }
                catch (XmlSchemaException e) {
                    Saxon.Hej.trans.XPathException de =
                        new Saxon.Hej.trans.XPathException(("Validation error reported by XML parser: " + e.Message),
                            e);
                    Saxon.Hej.expr.parser.Loc loc =
                        new Saxon.Hej.expr.parser.Loc(getSystemId(), e.LineNumber, e.LinePosition);
                    de.setLocator(loc);
                    throw de;
                }
                catch (System.Exception e) {
                    Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(
                        ("Error reported by XML parser"
                         + (baseURI == null ? ": " : " processing " + baseURI + ": ")
                         + e.Message), e);
                    Saxon.Hej.expr.parser.Loc loc = new Saxon.Hej.expr.parser.Loc(getSystemId(), -1, -1);
                    de.setLocator(loc);
                    throw de;
                }

                XmlNodeType intype = parser.NodeType;
                isEmptyElement = parser.IsEmptyElement;
                if (parser.EOF) {
                    currentEvent = Saxon.Hej.pull.PullEvent.END_DOCUMENT;
                    return currentEvent.Value;
                }

                if ((intype == XmlNodeType.EntityReference)) {
                    currentEvent = null;
                }
                else {
                    currentEvent = mapInputKindToOutputKind(intype);
                    if (currentEvent == Saxon.Hej.pull.PullEvent.TEXT && parser.Depth == 0 && !AcceptFragment) {
                        currentEvent = null;
                    }
                    else if ((currentEvent == Saxon.Hej.pull.PullEvent.START_ELEMENT)) {
                        Saxon.Hej.om.NamespaceMap nsMap = namespaceStack.Peek();
                        if (parser.HasAttributes) {
                            int limit = parser.AttributeCount;
                            for (int i = 0; (i < limit); i++) {
                                parser.MoveToAttribute(i);
                                string prefix = parser.Prefix;
                                string localName = parser.LocalName;
                                if ("xmlns".Equals(prefix)) {
                                    nsMap = nsMap.bind(localName, parser.Value);
                                }
                                else {
                                    if (((prefix.Length == 0) && "xmlns".Equals(localName))) {
                                        nsMap = nsMap.bind("", parser.Value);
                                    }
                                }
                            }
                        }

                        namespaceStack.Push(nsMap);
                    }
                    else if ((currentEvent == Saxon.Hej.pull.PullEvent.END_ELEMENT)) {
                        namespaceStack.Pop();
                    }
                }
            } while ((currentEvent == null));

            return currentEvent.Value;
        }

        private Saxon.Hej.pull.PullEvent? mapInputKindToOutputKind(XmlNodeType @in) {
            switch (@in) {
                case XmlNodeType.Attribute:
                    return Saxon.Hej.pull.PullEvent.ATTRIBUTE;
                case XmlNodeType.CDATA:
                    return Saxon.Hej.pull.PullEvent.TEXT;
                case XmlNodeType.Comment:
                    return Saxon.Hej.pull.PullEvent.COMMENT;
                case XmlNodeType.Document:
                    return Saxon.Hej.pull.PullEvent.START_DOCUMENT;
                case XmlNodeType.DocumentFragment:
                    return Saxon.Hej.pull.PullEvent.START_DOCUMENT;
                case XmlNodeType.Element:
                    return Saxon.Hej.pull.PullEvent.START_ELEMENT;
                case XmlNodeType.EndElement:
                    return Saxon.Hej.pull.PullEvent.END_ELEMENT;
                case XmlNodeType.ProcessingInstruction:
                    return Saxon.Hej.pull.PullEvent.PROCESSING_INSTRUCTION;
                case XmlNodeType.SignificantWhitespace:
                    // Whitespace stripping is handled downstream
                    return Saxon.Hej.pull.PullEvent.TEXT;
                case XmlNodeType.Text:
                    return Saxon.Hej.pull.PullEvent.TEXT;
                case XmlNodeType.Whitespace:
                    return IgnoreIgnorableWhitespace ? null : Saxon.Hej.pull.PullEvent.TEXT;

                case XmlNodeType.EntityReference:
                    //Console.WriteLine("Found entity reference " + parser.Name);

                    if (parser.CanResolveEntity) {
                        parser.ResolveEntity();
                        parser.Read();
                    }

                    return null;

                default:
                    return null;
            }
        }

        public void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
            Saxon.Hej.Configuration config = pipe.getConfiguration();
            expandDefaults = config.isExpandAttributeDefaults();
        }

        public Saxon.Hej.pull.PullEvent skipToMatchingEnd() {
            if ((currentEvent == Saxon.Hej.pull.PullEvent.START_ELEMENT)) {
                currentEvent = Saxon.Hej.pull.PullEvent.END_ELEMENT;
                parser.Skip();
            }
            else {
                if ((currentEvent == Saxon.Hej.pull.PullEvent.START_DOCUMENT)) {
                    currentEvent = Saxon.Hej.pull.PullEvent.END_DOCUMENT;
                }
                else {
                    throw new Saxon.Ejava.lang.IllegalStateException((currentEvent + ""));
                }
            }

            return currentEvent.Value;
        }

        public int getColumnNumber() {
            if (((parser is IXmlLineInfo) && ((IXmlLineInfo) parser).HasLineInfo())) {
                return ((IXmlLineInfo) parser).LinePosition;
            }
            else {
                return -1;
            }
        }

        public int getLineNumber() {
            if (((parser is IXmlLineInfo) && ((IXmlLineInfo) parser).HasLineInfo())) {
                return ((IXmlLineInfo) parser).LineNumber;
            }
            else {
                return -1;
            }
        }

        public string getPublicId() {
            return null;
        }

        public string getSystemId() {
            string @base = parser.BaseURI;
            //Console.WriteLine("BASE " + @base);
            if (((@base == null) || (@base.Length == 0))) {
                return baseURI;
            }
            else {
                return @base;
            }
        }

        public Saxon.Hej.s9api.Location saveLocation() {
            return new Saxon.Hej.expr.parser.Loc(this);
        }
        
        public SysCollGen.IList<Saxon.Hej.pull.UnparsedEntity> getUnparsedEntities() {
            return null;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited