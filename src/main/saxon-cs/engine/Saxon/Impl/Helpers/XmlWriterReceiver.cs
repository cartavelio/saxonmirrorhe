using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Saxon.Hej.@event;
using Saxon.Hej.om;
using Saxon.Hej.s9api;
using Saxon.Hej.type;


namespace Saxon.Impl.Helpers {
    
    /// <summary>
    /// This class bridges the Saxon Receiver interface (a push API for XML)
    /// to the .NET XmlWriter interface (another push API for XML)
    /// </summary>
    /// <remarks>
    /// <para>A <c>NamespaceDifferencer</c> should be inserted in the pipeline to prevent
    /// redundant namespace declarations being output.</para>
    /// </remarks>
    /// 
    public class XmlWriterReceiver : Saxon.Hej.@event.TreeReceiver {
        private readonly XmlWriter writer;
        private bool closeAfterUse = true;
        public XmlWriterReceiver(XmlWriter writer, PipelineConfiguration pipe) : base(new Sink(pipe)) {
            this.writer = writer;
        }
        public void setCloseAfterUse(bool close) {
            closeAfterUse = close;
        }
        
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            writer.WriteString(chars.ToString());
        }
        public override void close() {
            if (closeAfterUse) {
                writer.Close();
            } else {
                writer.Flush();
            }
        }
        
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location locationId, int properties) {
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
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
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

// Copyright (c) 2021 Saxonica Limited