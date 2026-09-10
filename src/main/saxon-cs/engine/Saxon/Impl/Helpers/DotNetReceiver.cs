using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace net.sf.saxon.dotnet {
    public class DotNetReceiver : net.sf.saxon.@event.Outputter {
        private readonly XmlWriter writer;
        private net.sf.saxon.@event.PipelineConfiguration pipe;
        private string systemId;
        private bool closeAfterUse = true;
        public DotNetReceiver(XmlWriter writer)  {
            this.writer = writer;
        }
        public void setCloseAfterUse(bool close) {
            closeAfterUse = close;
        }
        public override void attribute(net.sf.saxon.om.NodeName nameCode, net.sf.saxon.type.SimpleType typeCode, string value, net.sf.saxon.s9api.Location locationId, int properties) {
            writer.WriteAttributeString(nameCode.getPrefix(), nameCode.getLocalPart(), nameCode.getURI(), value.ToString());
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
        public override net.sf.saxon.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public override void @namespace(string prefix, string namespaceUri, int properties) {
            if ((prefix.Length==0)) {
                writer.WriteAttributeString("", "xmlns", null, namespaceUri);
            } else {
                writer.WriteAttributeString("xmlns", prefix, null, namespaceUri);
            }
        }
        public override void open() {
        }
        public override void processingInstruction(string name, net.sf.saxon.str.CodePoints data, net.sf.saxon.s9api.Location locationId, int properties) {
            writer.WriteProcessingInstruction(name, data.ToString());
        }
        public override void setPipelineConfiguration(net.sf.saxon.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
        }
        public override void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
        }
        public override void startContent() {
        }
        public override void startDocument(int properties) {
            writer.WriteStartDocument();
        }
        public override void startElement(net.sf.saxon.om.NodeName nameCode, net.sf.saxon.type.SchemaType typeCode, net.sf.saxon.s9api.Location location, int properties) {
            writer.WriteStartElement(nameCode.getPrefix(), nameCode.getLocalPart(), nameCode.getURI());
        }
        public override string getSystemId() {
            return systemId;
        }
        public override bool usesTypeAnnotations() {
            return false;
        }
    
    }
}