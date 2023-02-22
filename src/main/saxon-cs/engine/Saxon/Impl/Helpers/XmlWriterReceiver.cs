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
        private bool closeAfterUse = true;
        public XmlWriterReceiver(XmlWriter writer, PipelineConfiguration pipe) : base(new Sink(pipe)) {
            this.writer = writer;
        }
        public void setCloseAfterUse(bool close) {
            closeAfterUse = close;
        }
        
        public override void characters(net.sf.saxon.str.CodePoints chars, net.sf.saxon.s9api.Location locationId, int properties) {
            writer.WriteString(chars.ToString());
        }
        public override void close() {
            if (closeAfterUse) {
                writer.Close();
            } else {
                writer.Flush();
            }
        }
        
        public override void comment(net.sf.saxon.str.CodePoints content, net.sf.saxon.s9api.Location locationId, int properties) {
            writer.WriteComment(content.ToString());
        }
        public override void endDocument() {
            writer.WriteEndDocument();
        }
        public override void endElement() {
            writer.WriteEndElement();
        }

        public override void open() {
        }
        public override void processingInstruction(string name, net.sf.saxon.str.CodePoints data, net.sf.saxon.s9api.Location locationId, int properties) {
            writer.WriteProcessingInstruction(name, data.ToString());
        }
        
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
        }
        
        public override void startDocument(int properties) {
            writer.WriteStartDocument();
        }

        public override void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces,
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
        
        public override bool usesTypeAnnotations() {
            return false;
        }
    
    }
}