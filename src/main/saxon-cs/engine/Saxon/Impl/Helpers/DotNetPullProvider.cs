using System;
using SysCollGen = System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace System.Impl.Helpers {
    public class DotNetPullProvider : net.sf.saxon.pull.PullProvider, net.sf.saxon.s9api.Location {
        private net.sf.saxon.@event.PipelineConfiguration pipe;
        private readonly XmlReader parser;
        private string baseURI;
        private bool isEmptyElement = false;
        private net.sf.saxon.pull.PullEvent? currentEvent = net.sf.saxon.pull.PullEvent.START_OF_INPUT;
        private bool expandDefaults = true;
        private readonly SysCollGen.Stack<net.sf.saxon.om.NamespaceMap> namespaceStack = new SysCollGen.Stack<net.sf.saxon.om.NamespaceMap>();
        public bool AcceptFragment { get; set; }
        public DotNetPullProvider(XmlReader parser)  {
            this.parser = parser;
            namespaceStack.Push(net.sf.saxon.om.NamespaceMap.emptyMap());
        }
        public void setBaseURI(string @base) {
            baseURI = @base;
        }
        public void close() {
            parser.Close();
        }
        public net.sf.saxon.pull.PullEvent current() {
            return currentEvent.Value;
        }
        public net.sf.saxon.value.AtomicValue getAtomicValue() {
            return null;
        }
        public net.sf.saxon.om.AttributeMap getAttributes() {
            if (parser.HasAttributes) {
                net.sf.saxon.om.AttributeMap atts = net.sf.saxon.om.EmptyAttributeMap.getInstance();
                for (int i = 0; (i<parser.AttributeCount); i++) {
                    parser.MoveToAttribute(i);
                    string prefix = parser.Prefix;
                    string namespaceURI = parser.NamespaceURI;
                    string localName = parser.LocalName;
                    if (("xmlns".Equals(prefix)||(("".Equals(prefix)&&"xmlns".Equals(localName))))) {
                    } else {
                        if ((expandDefaults||!(parser.IsDefault))) {
                            net.sf.saxon.om.NodeName nc = new net.sf.saxon.om.FingerprintedQName(prefix, namespaceURI, localName);
                            atts = atts.put(new net.sf.saxon.om.AttributeInfo(nc, net.sf.saxon.type.BuiltInAtomicType.UNTYPED_ATOMIC, parser.Value, net.sf.saxon.expr.parser.Loc.NONE, net.sf.saxon.@event.ReceiverOption.NONE));
                        }
                    }
                }
                parser.MoveToElement();
                return atts;
            } else {
                return net.sf.saxon.om.EmptyAttributeMap.getInstance();
            }
        }
        public net.sf.saxon.om.NodeName getNodeName() {
            return new net.sf.saxon.om.FingerprintedQName(parser.Prefix, parser.NamespaceURI, parser.LocalName);
        }
        public net.sf.saxon.om.NamespaceBinding[] getNamespaceDeclarations() {
            if (parser.HasAttributes) {
                int limit = parser.AttributeCount;
                net.sf.saxon.om.NamespaceBinding[] nsBindings = new net.sf.saxon.om.NamespaceBinding[limit];
                int used = 0;
                for (int i = 0; (i<limit); i++) {
                    parser.MoveToAttribute(i);
                    string prefix = parser.Prefix;
                    string localName = parser.LocalName;
                    if ("xmlns".Equals(prefix)) {
                        net.sf.saxon.om.NamespaceBinding nscode = new net.sf.saxon.om.NamespaceBinding(localName, parser.Value);
                        nsBindings[used++] = nscode;
                    } else {
                        if (((prefix.Length==0)&&"xmlns".Equals(localName))) {
                            net.sf.saxon.om.NamespaceBinding nscode = new net.sf.saxon.om.NamespaceBinding("", parser.Value);
                            nsBindings[used++] = nscode;
                        } else {
                        }
                    }
                }
                if ((used<limit)) {
                    nsBindings[used] = null;
                }

                parser.MoveToElement();
                return nsBindings;
            } else {
                return net.sf.saxon.om.NamespaceBinding.EMPTY_ARRAY;
            }
        }
        public net.sf.saxon.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public net.sf.saxon.s9api.Location getSourceLocator() {
            if (((parser is IXmlLineInfo)||(parser is XmlValidatingReader))) {
                return this;
            } else {
                return null;
            }
        }
        public net.sf.saxon.str.CodePoints getStringValue() {
            return net.sf.saxon.str.StringView.of(parser.Value);
        }
        public net.sf.saxon.type.SchemaType getSchemaType() {
            return net.sf.saxon.type.Untyped.getInstance();
        }
        public net.sf.saxon.pull.PullEvent next() {
            if ((currentEvent==net.sf.saxon.pull.PullEvent.START_OF_INPUT)) {
                currentEvent = net.sf.saxon.pull.PullEvent.START_DOCUMENT;
                return currentEvent.Value;
            } else if (((currentEvent==net.sf.saxon.pull.PullEvent.END_DOCUMENT) || (currentEvent==net.sf.saxon.pull.PullEvent.END_OF_INPUT))) {
                currentEvent = net.sf.saxon.pull.PullEvent.END_OF_INPUT;
                return currentEvent.Value;
            } else if (((currentEvent==net.sf.saxon.pull.PullEvent.START_ELEMENT) && isEmptyElement)) {
                currentEvent = net.sf.saxon.pull.PullEvent.END_ELEMENT;
                return currentEvent.Value;
            }
            do {
                try {
                    parser.Read();
                } catch (XmlException e) {
                    net.sf.saxon.trans.XPathException de = new net.sf.saxon.trans.XPathException(
                        ("Error reported by XML parser"
                         + (baseURI == null ? ": " : " processing " + baseURI + ": ")
                         + e.Message), e);
                    net.sf.saxon.expr.parser.Loc loc = new net.sf.saxon.expr.parser.Loc(getSystemId(), e.LineNumber, e.LinePosition);
                    de.setLocator(loc);
                    throw de;
                } catch (XmlSchemaException e) {
                    net.sf.saxon.trans.XPathException de = new net.sf.saxon.trans.XPathException(("Validation error reported by XML parser: "+e.Message), e);
                    net.sf.saxon.expr.parser.Loc loc = new net.sf.saxon.expr.parser.Loc(getSystemId(), e.LineNumber, e.LinePosition);
                    de.setLocator(loc);
                    throw de;
                } catch (System.Exception e) {
                    net.sf.saxon.trans.XPathException de = new net.sf.saxon.trans.XPathException(
                        ("Error reported by XML parser"
                         + (baseURI == null ? ": " : " processing " + baseURI + ": ")
                         + e.Message), e);
                    net.sf.saxon.expr.parser.Loc loc = new net.sf.saxon.expr.parser.Loc(getSystemId(), -1, -1);
                    de.setLocator(loc);
                    throw de;
                }
                XmlNodeType intype = parser.NodeType;
                isEmptyElement = parser.IsEmptyElement;
                if (parser.EOF) {
                    currentEvent = net.sf.saxon.pull.PullEvent.END_DOCUMENT;
                    return currentEvent.Value;
                }
                if ((intype==XmlNodeType.EntityReference)) {
                    currentEvent = null;
                } else {
                    currentEvent = mapInputKindToOutputKind(intype);
                    if (currentEvent==net.sf.saxon.pull.PullEvent.TEXT && parser.Depth==0 && !AcceptFragment) {
                        currentEvent = null;
                    } else if ((currentEvent==net.sf.saxon.pull.PullEvent.START_ELEMENT)) {
                        net.sf.saxon.om.NamespaceMap nsMap = namespaceStack.Peek();
                        if (parser.HasAttributes) {
                            int limit = parser.AttributeCount;
                            for (int i = 0; (i<limit); i++) {
                                parser.MoveToAttribute(i);
                                string prefix = parser.Prefix;
                                string localName = parser.LocalName;
                                if ("xmlns".Equals(prefix)) {
                                    nsMap = nsMap.bind(localName, parser.Value);
                                } else {
                                    if (((prefix.Length==0)&&"xmlns".Equals(localName))) {
                                        nsMap = nsMap.bind("", parser.Value);
                                    }
                                }
                            }
                        }
                        namespaceStack.Push(nsMap);
                    } else if ((currentEvent==net.sf.saxon.pull.PullEvent.END_ELEMENT)) {
                        namespaceStack.Pop(); 
                    }
                }
            } while ((currentEvent==null));
            return currentEvent.Value;
        }
        private net.sf.saxon.pull.PullEvent? mapInputKindToOutputKind(XmlNodeType @in) {
            switch (@in) {
              case XmlNodeType.Attribute:
                return net.sf.saxon.pull.PullEvent.ATTRIBUTE;
              case XmlNodeType.CDATA:
                return net.sf.saxon.pull.PullEvent.TEXT;
              case XmlNodeType.Comment:
                return net.sf.saxon.pull.PullEvent.COMMENT;
              case XmlNodeType.Document:
                return net.sf.saxon.pull.PullEvent.START_DOCUMENT;
              case XmlNodeType.DocumentFragment:
                return net.sf.saxon.pull.PullEvent.START_DOCUMENT;
              case XmlNodeType.Element:
                return net.sf.saxon.pull.PullEvent.START_ELEMENT;
              case XmlNodeType.EndElement:
                return net.sf.saxon.pull.PullEvent.END_ELEMENT;
              case XmlNodeType.ProcessingInstruction:
                return net.sf.saxon.pull.PullEvent.PROCESSING_INSTRUCTION;
              case XmlNodeType.SignificantWhitespace:
                return net.sf.saxon.pull.PullEvent.TEXT;
              case XmlNodeType.Text:
                return net.sf.saxon.pull.PullEvent.TEXT;
              case XmlNodeType.Whitespace:
                return net.sf.saxon.pull.PullEvent.TEXT;
              default:
                return null;
            }
        }
        public void setPipelineConfiguration(net.sf.saxon.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
            net.sf.saxon.Configuration config = pipe.getConfiguration();
            expandDefaults = config.isExpandAttributeDefaults();
        }
        public net.sf.saxon.pull.PullEvent skipToMatchingEnd() {
            if ((currentEvent==net.sf.saxon.pull.PullEvent.START_ELEMENT)) {
                currentEvent = net.sf.saxon.pull.PullEvent.END_ELEMENT;
                parser.Skip();
            } else {
                if ((currentEvent==net.sf.saxon.pull.PullEvent.START_DOCUMENT)) {
                    currentEvent = net.sf.saxon.pull.PullEvent.END_DOCUMENT;
                } else {
                    throw new java.lang.IllegalStateException((currentEvent+""));
                }
            }
            return currentEvent.Value;
        }
        public int getColumnNumber() {
            if (((parser is IXmlLineInfo)&&((IXmlLineInfo)parser).HasLineInfo())) {
                return ((IXmlLineInfo)parser).LinePosition;
            } else {
                return -1;
            }
        }
        public int getLineNumber() {
            if (((parser is IXmlLineInfo)&&((IXmlLineInfo)parser).HasLineInfo())) {
                return ((IXmlLineInfo)parser).LineNumber;
            } else {
                return -1;
            }
        }
        public string getPublicId() {
            return null;
        }
        public string getSystemId() {
            string @base = parser.BaseURI;
            if (((@base==null)||(@base.Length==0))) {
                return baseURI;
            } else {
                return @base;
            }
        }
        public net.sf.saxon.s9api.Location saveLocation() {
            return new net.sf.saxon.expr.parser.Loc(this);
        }
        public int getLineNumber(int locationId) {
            return getLineNumber();
        }
        public int getColumnNumber(int locationId) {
            return getColumnNumber();
        }
        public string getSystemId(int locationId) {
            return getSystemId();
        }
        public SysCollGen.IList<net.sf.saxon.pull.UnparsedEntity> getUnparsedEntities() {
            return null;
        }
    
    }
}