using System.Xml;
using System.Xml.Schema;
using net.sf.saxon.om;
using net.sf.saxon.pull;

/*
 * This module interfaces Saxon to the Microsoft .NET pull parser, exposing it
 * as an instance of Saxon's PullProvider interface. It is derived from the Java
 * code in net.sf.saxon.dotnet.DotNetPullProvider, converted to C# and now
 * maintained in C#.
 */

namespace Saxon.Impl.Helpers {
    public class PullParser : net.sf.saxon.pull.PullProvider, net.sf.saxon.s9api.Location {
        private readonly java.util.Stack<NamespaceMap> namespaceStack = new java.util.Stack<NamespaceMap>();
        private readonly XmlReader parser;
        private string baseURI;
        private PullEvent currentEvent = PullEvent.START_OF_INPUT;
        private bool expandDefaults = true;
        private bool isEmptyElement = false;
        private net.sf.saxon.@event.PipelineConfiguration pipe;

        public PullParser(XmlReader parser) {
            this.parser = parser;
            namespaceStack.push(NamespaceMap.emptyMap());
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
            if (((@base == null) || (@base.Length == 0))) {
                return baseURI;
            }
            else {
                return @base;
            }
        }

        public net.sf.saxon.s9api.Location saveLocation() {
            return new net.sf.saxon.expr.parser.Loc(this);
        }

        public void close() {
            parser.Close();
        }

        public PullEvent current() {
            return currentEvent;
        }

        public net.sf.saxon.value.AtomicValue getAtomicValue() {
            return null;
        }

        public AttributeMap getAttributes() {
            if (parser.HasAttributes) {
                AttributeMap atts = EmptyAttributeMap.getInstance();
                for (int i = 0; (i < parser.AttributeCount); i++) {
                    parser.MoveToAttribute(i);
                    string prefix = parser.Prefix;
                    string namespaceURI = parser.NamespaceURI;
                    string localName = parser.LocalName;
                    if (("xmlns".Equals(prefix) || (("".Equals(prefix) && "xmlns".Equals(localName))))) {
                    }
                    else {
                        if ((expandDefaults || !(parser.IsDefault))) {
                            NodeName nc = new FingerprintedQName(prefix, namespaceURI, localName);
                            atts = atts.put(new AttributeInfo(nc, net.sf.saxon.type.BuiltInAtomicType.UNTYPED_ATOMIC,
                                parser.Value, net.sf.saxon.expr.parser.Loc.NONE,
                                net.sf.saxon.@event.ReceiverOption.NONE));
                        }
                    }
                }

                return atts;
            }
            else {
                return EmptyAttributeMap.getInstance();
            }
        }

        public NodeName getNodeName() {
            return new FingerprintedQName(
                parser.Prefix, parser.NamespaceURI, parser.LocalName);
        }

        public NamespaceBinding[] getNamespaceDeclarations() {
            if (parser.HasAttributes) {
                int limit = parser.AttributeCount;
                NamespaceBinding[] nsBindings = new NamespaceBinding[limit];
                int used = 0;
                for (int i = 0; (i < limit); i++) {
                    parser.MoveToAttribute(i);
                    string prefix = parser.Prefix;
                    string localName = parser.LocalName;
                    if ("xmlns".Equals(prefix)) {
                        NamespaceBinding nscode = new NamespaceBinding(localName, parser.Value);
                        nsBindings[used++] = nscode;
                    }
                    else {
                        if (((prefix.Length == 0) && "xmlns".Equals(localName))) {
                            NamespaceBinding nscode = new NamespaceBinding("", parser.Value);
                            nsBindings[used++] = nscode;
                        }
                        else {
                        }
                    }
                }

                if ((used < limit)) {
                    nsBindings[used] = null;
                }

                return nsBindings;
            }
            else {
                return NamespaceBinding.EMPTY_ARRAY;
            }
        }

        public net.sf.saxon.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }

        public net.sf.saxon.s9api.Location getSourceLocator() {
            if (((parser is IXmlLineInfo) || (parser is XmlValidatingReader))) {
                return this;
            }
            else {
                return null;
            }
        }

        public net.sf.saxon.str.CodePoints getStringValue() {
            return net.sf.saxon.str.StringView.of(parser.Value);
        }

        public net.sf.saxon.type.SchemaType getSchemaType() {
            return net.sf.saxon.type.Untyped.getInstance();
        }

        public PullEvent next() {
            if ((currentEvent == PullEvent.START_OF_INPUT)) {
                currentEvent = PullEvent.START_DOCUMENT;
                return currentEvent;
            }
            else {
                if (((currentEvent == PullEvent.END_DOCUMENT) || (currentEvent == PullEvent.END_OF_INPUT))) {
                    currentEvent = PullEvent.END_OF_INPUT;
                    return currentEvent;
                }
                else {
                    if (((currentEvent == PullEvent.START_ELEMENT) && isEmptyElement)) {
                        currentEvent = PullEvent.END_ELEMENT;
                        return currentEvent;
                    }
                }
            }

            do {
                try {
                    parser.Read();
                }
                catch (XmlException e) {
                    net.sf.saxon.trans.XPathException de =
                        new net.sf.saxon.trans.XPathException(("Error reported by XML parser: " + e.Message), e);
                    net.sf.saxon.expr.parser.Loc loc =
                        new net.sf.saxon.expr.parser.Loc(getSystemId(), e.LineNumber, e.LinePosition);
                    de.setLocator(loc);
                    throw de;
                }
                catch (XmlSchemaException e) {
                    net.sf.saxon.trans.XPathException de =
                        new net.sf.saxon.trans.XPathException(("Validation error reported by XML parser: " + e.Message),
                            e);
                    net.sf.saxon.expr.parser.Loc loc =
                        new net.sf.saxon.expr.parser.Loc(getSystemId(), e.LineNumber, e.LinePosition);
                    de.setLocator(loc);
                    throw de;
                }
                catch (System.Exception e) {
                    net.sf.saxon.trans.XPathException de =
                        new net.sf.saxon.trans.XPathException(("Error reported by XML parser: " + e.Message), e);
                    net.sf.saxon.expr.parser.Loc loc = new net.sf.saxon.expr.parser.Loc(getSystemId(), -1, -1);
                    de.setLocator(loc);
                    throw de;
                }

                XmlNodeType intype = parser.NodeType;
                isEmptyElement = parser.IsEmptyElement;
                if (parser.EOF) {
                    currentEvent = PullEvent.END_DOCUMENT;
                    return currentEvent;
                }

                if ((intype == XmlNodeType.EntityReference)) {
                    currentEvent = PullEvent.WORK_IN_PROGRESS;
                }
                else {
                    currentEvent = mapInputKindToOutputKind(intype);
                    if (((currentEvent == PullEvent.TEXT) && (parser.Depth == 0))) {
                        currentEvent = PullEvent.WORK_IN_PROGRESS;
                    }
                    else {
                        if ((currentEvent == PullEvent.START_ELEMENT)) {
                            NamespaceMap nsMap = namespaceStack.peek();
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

                            namespaceStack.push(nsMap);
                        }
                        else {
                            if ((currentEvent == PullEvent.END_ELEMENT)) {
                                namespaceStack.pop();
                            }
                        }
                    }
                }
            } while ((currentEvent == PullEvent.WORK_IN_PROGRESS));

            return currentEvent;
        }

        public void setPipelineConfiguration(net.sf.saxon.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
            net.sf.saxon.Configuration config = pipe.getConfiguration();
            expandDefaults = config.isExpandAttributeDefaults();
        }

        public PullEvent skipToMatchingEnd() {
            if ((currentEvent == PullEvent.START_ELEMENT)) {
                currentEvent = PullEvent.END_ELEMENT;
                parser.Skip();
            }
            else {
                if ((currentEvent == PullEvent.START_DOCUMENT)) {
                    currentEvent = PullEvent.END_DOCUMENT;
                }
                else {
                    throw new java.lang.IllegalStateException((currentEvent + ""));
                }
            }

            return currentEvent;
        }

        public java.util.List<net.sf.saxon.pull.UnparsedEntity> getUnparsedEntities() {
            return null;
        }

        public void setBaseURI(string @base) {
            baseURI = @base;
        }

        private PullEvent mapInputKindToOutputKind(XmlNodeType @in) {
            switch (@in) {
                case XmlNodeType.Attribute:
                    return PullEvent.ATTRIBUTE;
                case XmlNodeType.CDATA:
                    return PullEvent.TEXT;
                case XmlNodeType.Comment:
                    return PullEvent.COMMENT;
                case XmlNodeType.Document:
                    return PullEvent.START_DOCUMENT;
                case XmlNodeType.DocumentFragment:
                    return PullEvent.START_DOCUMENT;
                case XmlNodeType.Element:
                    return PullEvent.START_ELEMENT;
                case XmlNodeType.EndElement:
                    return PullEvent.END_ELEMENT;
                case XmlNodeType.ProcessingInstruction:
                    return PullEvent.PROCESSING_INSTRUCTION;
                case XmlNodeType.SignificantWhitespace:
                    return PullEvent.TEXT;
                case XmlNodeType.Text:
                    return PullEvent.TEXT;
                case XmlNodeType.Whitespace:
                    return PullEvent.TEXT;
                default:
                    return PullEvent.END_OF_INPUT;
            }
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
    }
}