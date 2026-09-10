using System;
using System.Collections.Generic;
using System.Xml;

namespace Saxon.Impl.Dom {
    public class DomBuilder : net.sf.saxon.@event.Receiver {
        private net.sf.saxon.@event.PipelineConfiguration pipe;
        private net.sf.saxon.om.NamePool namePool;
        private string _systemId;
        private XmlNode _currentNode;
        private XmlDocument _document;
        public void setPipelineConfiguration(net.sf.saxon.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
            this.namePool = pipe.getConfiguration().getNamePool();
        }
        public net.sf.saxon.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public net.sf.saxon.Configuration getConfiguration() {
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
        public void startElement(net.sf.saxon.om.NodeName elemName, net.sf.saxon.type.SchemaType type, net.sf.saxon.om.AttributeMap attributes, net.sf.saxon.om.NamespaceMap namespaces, net.sf.saxon.s9api.Location location, int properties) {
            if ((_document==null)) {
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
                foreach (net.sf.saxon.om.NamespaceBinding ns in namespaces) {
                    string nsprefix = ns.getPrefix();
                    string nsuri = ns.getURI();
                    if (!(nsuri.Equals(net.sf.saxon.lib.NamespaceConstant.XML))) {
                        if ((prefix.Length==0)) {
                            element.SetAttribute("xmlns", uri);
                        } else {
                            XmlAttribute decl = _document.CreateAttribute("xmlns", nsprefix, "http://www.w3.org/2000/xmlns/");
                            decl.InnerText = nsuri;
                            element.Attributes.Append(decl);
                        }
                    }
                }
                foreach (net.sf.saxon.om.AttributeInfo att in attributes) {
                    net.sf.saxon.om.NodeName attName = att.getNodeName();
                    XmlAttribute attr = _document.CreateAttribute(attName.getPrefix(), attName.getLocalPart(), attName.getURI());
                    attr.Value = att.getValue();
                    element.Attributes.Append(attr);
                }
            } catch (System.Exception err) {
                throw new net.sf.saxon.trans.XPathException(err);
            }
        }
        public void endElement() {
            _currentNode.Normalize();
            _currentNode = _currentNode.ParentNode;
        }
        public void characters(net.sf.saxon.str.CodePoints chars, net.sf.saxon.s9api.Location locationId, int properties) {
            try {
                XmlText text = _document.CreateTextNode(chars.ToString());
                _currentNode.AppendChild(text);
            } catch (System.Exception err) {
                throw new net.sf.saxon.trans.XPathException(err);
            }
        }
        public void processingInstruction(string target, net.sf.saxon.str.CodePoints data, net.sf.saxon.s9api.Location locationId, int properties) {
            try {
                XmlProcessingInstruction pi = _document.CreateProcessingInstruction(target, data.ToString());
                _currentNode.AppendChild(pi);
            } catch (System.Exception err) {
                throw new net.sf.saxon.trans.XPathException(err);
            }
        }
        public void comment(net.sf.saxon.str.CodePoints chars, net.sf.saxon.s9api.Location locationId, int properties) {
            try {
                XmlComment comment = _document.CreateComment(chars.ToString());
                _currentNode.AppendChild(comment);
            } catch (System.Exception err) {
                throw new net.sf.saxon.trans.XPathException(err);
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
            } else {
                _document = attachmentPoint.OwnerDocument;
            }
        }
        public XmlDocument getDocumentNode() {
            return _document;
        }
    
        /* Expanding default method append from interface Receiver */
        public void append(net.sf.saxon.om.Item item, net.sf.saxon.s9api.Location locationId, int properties) {
            throw new java.lang.UnsupportedOperationException();
        }

        /* Expanding default method append from interface Receiver */
        public void append(net.sf.saxon.om.Item item) {
            append(item, net.sf.saxon.expr.parser.Loc.NONE, net.sf.saxon.@event.ReceiverOption.ALL_NAMESPACES);
        }

        /* Expanding default method handlesAppend from interface Receiver */
        public bool handlesAppend() {
            return false;
        }
}
}