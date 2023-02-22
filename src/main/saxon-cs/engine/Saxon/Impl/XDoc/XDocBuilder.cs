using System.Xml;
using System.Xml.Linq;
using Saxon.Api;
using Saxon.Hej.value;

namespace Saxon.Impl.XDoc {
    public class XDocBuilder : Saxon.Hej.@event.Receiver {
        private Saxon.Hej.@event.PipelineConfiguration pipe;
        private string _systemId;
        private XContainer _currentNode;
        private XDocument _document;

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
                _document = new XDocument();
                _currentNode = _document;
            }
        }

        public void endDocument() {
        }

        public void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type,
            Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces,
            Saxon.Hej.s9api.Location location, int properties) {
            if ((_document == null)) {
                _document = new XDocument();
                _currentNode = _document;
            }

            string prefix = elemName.getPrefix();
            string local = elemName.getLocalPart();
            string uri = elemName.getURI();
            try {
                XElement element = new XElement(XNamespace.Get(uri) + local);
                _currentNode.Add(element);
                _currentNode = element;
                foreach (Saxon.Hej.om.NamespaceBinding ns in namespaces) {
                    // TODO: avoid creating redundant namespaces
                    string nsprefix = ns.getPrefix();
                    string nsuri = ns.getURI();
                    if (!(nsuri.Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                        if ((nsprefix.Length == 0)) {
                            element.SetAttributeValue("xmlns", nsuri);
                        }
                        else {
                            element.SetAttributeValue(XNamespace.Xmlns + nsprefix, nsuri);
                        }
                    }
                }

                foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                    Saxon.Hej.om.StructuredQName attName = att.getNodeName().getStructuredQName();
                    element.SetAttributeValue(new QName(attName).ToXName(), att.getValue());
                }
            }
            catch (System.Exception err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }

        public void endElement() {
            _currentNode = _currentNode.Parent == null ? _currentNode.Document : _currentNode.Parent; 
        }

        public void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            try {
                XText text = new XText(chars.ToString());
                _currentNode.Add(text);
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
                XProcessingInstruction pi = new XProcessingInstruction(target, data.ToString());
                _currentNode.Add(pi);
            }
            catch (System.Exception err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }

        public void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            try {
                XComment comment = new XComment(chars.ToString());
                _currentNode.Add(comment);
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

        public void setAttachmentPoint(XContainer attachmentPoint) {
            _currentNode = attachmentPoint;
            if ((attachmentPoint is XDocument xDoc)) {
                _document = xDoc;
            }
            else {
                _document = attachmentPoint.Document;
            }
        }

        public XDocument getDocumentNode() {
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