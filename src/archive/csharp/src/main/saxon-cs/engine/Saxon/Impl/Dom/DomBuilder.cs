using System.Xml;
using Saxon.Hej.value;

namespace Saxon.Impl.Dom {
    public class DomBuilder : Saxon.Hej.@event.Receiver {
        private Saxon.Hej.@event.PipelineConfiguration pipe;
        private string _systemId;
        private XmlNode _currentNode;
        private XmlDocument _document;

        public void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
        }

        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }

        public Saxon.Hej.Configuration getConfiguration() {
            return pipe.getConfiguration();
        }

        public void setSystemId(string systemId) {
            this._systemId = systemId;
        }

        public string getSystemId() {
            return _systemId;
        }

        public void open() {
        }

        public void close() {
        }

        public void startDocument(int properties) {
            if ((_document == null)) {
                _document = new XmlDocument();
                _currentNode = _document;
            }
        }

        public void endDocument() {
        }

        public void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type,
            Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces,
            Saxon.Hej.s9api.Location location, int properties) {
            if ((_document == null)) {
                _document = new XmlDocument();
                _currentNode = _document;
            }

            string prefix = elemName.getPrefix();
            string local = elemName.getLocalPart();
            string uri = elemName.getURI();
            try {
                XmlElement element = _document.CreateElement(prefix, local, uri);
                _currentNode.AppendChild(element);
                _currentNode = element;
                foreach (Saxon.Hej.om.NamespaceBinding ns in namespaces) {
                    string nsprefix = ns.getPrefix();
                    string nsuri = ns.getNamespaceUri().ToString();
                    if (!(nsuri.Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                        if ((nsprefix.Length == 0)) {
                            element.SetAttribute("xmlns", uri);
                        }
                        else {
                            XmlAttribute decl =
                                _document.CreateAttribute("xmlns", nsprefix, "http://www.w3.org/2000/xmlns/");
                            decl.InnerText = nsuri;
                            element.Attributes.Append(decl);
                        }
                    }
                }

                foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                    Saxon.Hej.om.NodeName attName = att.getNodeName();
                    XmlAttribute attr =
                        _document.CreateAttribute(attName.getPrefix(), attName.getLocalPart(), attName.getURI());
                    attr.Value = att.getValue();
                    element.Attributes.Append(attr);
                }
            }
            catch (System.Exception err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }

        public void endElement() {
            _currentNode.Normalize();
            _currentNode = _currentNode.ParentNode;
        }

        public void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            try {
                XmlText text = _document.CreateTextNode(chars.ToString());
                _currentNode.AppendChild(text);
            }
            catch (System.Exception err) {
                if (Whitespace.isAllWhite(chars) && _currentNode == _document) {
                    return; // No error when adding top-level whitespace text; just ignore it
                }

                throw new Saxon.Hej.trans.XPathException(err);
            }
        }

        public void processingInstruction(string target, Saxon.Hej.str.UnicodeString data,
            Saxon.Hej.s9api.Location locationId, int properties) {
            try {
                XmlProcessingInstruction pi = _document.CreateProcessingInstruction(target, data.ToString());
                _currentNode.AppendChild(pi);
            }
            catch (System.Exception err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }

        public void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            try {
                XmlComment comment = _document.CreateComment(chars.ToString());
                _currentNode.AppendChild(comment);
            }
            catch (System.Exception err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }

        public bool usesTypeAnnotations() {
            return false;
        }

        public void setUnparsedEntity(string name, string systemID, string publicID) {
        }

        public void setAttachmentPoint(XmlNode attachmentPoint) {
            _currentNode = attachmentPoint;
            if ((attachmentPoint is XmlDocument)) {
                _document = (XmlDocument)attachmentPoint;
            }
            else {
                _document = attachmentPoint.OwnerDocument;
            }
        }

        public XmlDocument getDocumentNode() {
            return _document;
        }

        /* Expanding default method append from interface Receiver */
        public void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }

        /* Expanding default method append from interface Receiver */
        public void append(Saxon.Hej.om.Item item) {
            append(item, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
        }

        /* Expanding default method handlesAppend from interface Receiver */
        public bool handlesAppend() {
            return false;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited