using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using net.sf.saxon.@event;
using net.sf.saxon.om;
using net.sf.saxon.s9api;
using net.sf.saxon.type;


namespace Saxon.Impl.Helpers {
    
    /// <summary>
    /// This class bridges the Saxon Receiver interface (a push API for XML)
    /// to the .NET XmlWriter interface (another push API for XML)
    /// </summary>
    /// 
    public class XmlWriterReceiver : net.sf.saxon.@event.TreeReceiver {
        private readonly XmlWriter writer;
        private net.sf.saxon.@event.PipelineConfiguration pipe;
        private string _systemId;
        private bool closeAfterUse = true;
        public XmlWriterReceiver(XmlWriter writer, PipelineConfiguration pipe) : base(new Sink(pipe)) {
            this.pipe = pipe;
            this.writer = writer;
        }
        public void setCloseAfterUse(bool close) {
            closeAfterUse = close;
        }
        
        public void characters(net.sf.saxon.str.CodePoints chars, net.sf.saxon.s9api.Location locationId, int properties) {
            writer.WriteString(chars.ToString());
        }
        public void close() {
            if (closeAfterUse) {
                writer.Close();
            } else {
                writer.Flush();
            }
        }
        
        public void comment(net.sf.saxon.str.CodePoints content, net.sf.saxon.s9api.Location locationId, int properties) {
            writer.WriteComment(content.ToString());
        }
        public void endDocument() {
            writer.WriteEndDocument();
        }
        public void endElement() {
            writer.WriteEndElement();
        }
        public net.sf.saxon.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        
        public void open() {
        }
        public void processingInstruction(string name, net.sf.saxon.str.CodePoints data, net.sf.saxon.s9api.Location locationId, int properties) {
            writer.WriteProcessingInstruction(name, data.ToString());
        }
        public void setPipelineConfiguration(net.sf.saxon.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
        }
        public void setSystemId(string systemId) {
            this._systemId = systemId;
        }
        public void setUnparsedEntity(string name, string systemID, string publicID) {
        }
        
        public void startDocument(int properties) {
            writer.WriteStartDocument();
        }

        public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces,
            Location location, int properties) {
            writer.WriteStartElement(elemName.getPrefix(), elemName.getLocalPart(), elemName.getURI());
            
            foreach (NamespaceBinding ns in namespaces) {
                // TODO: avoid sending redundant namespaces
                if ((ns.getPrefix().Length == 0)) {
                    writer.WriteAttributeString("", "xmlns", null, ns.getURI());
                }
                else {
                    writer.WriteAttributeString("xmlns", ns.getPrefix(), null, ns.getURI());
                }
            }
            
            foreach (AttributeInfo att in attributes) {
                NodeName attName = att.getNodeName();
                writer.WriteAttributeString(attName.getPrefix(), attName.getLocalPart(), attName.getURI(), att.getValue().ToString());
            }
            
        }

        public string getSystemId() {
            return _systemId;
        }
        public bool usesTypeAnnotations() {
            return false;
        }
    
    }
}