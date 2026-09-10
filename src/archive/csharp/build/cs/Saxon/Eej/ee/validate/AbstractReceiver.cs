using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public abstract class AbstractReceiver : Saxon.Hej.@event.Receiver {
        private Saxon.Hej.@event.PipelineConfiguration pipe;
        private string systemId;
        public virtual void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public virtual void close() {
        }
        public void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public void endDocument() {
        }
        public virtual void endElement() {
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return pipe.getConfiguration();
        }
        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public void open() {
        }
        public void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration config) {
            pipe = config;
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public void setUnparsedEntity(string name, string systemID, string publicID) {
        }
        public void startDocument(int properties) {
        }
        public virtual void startElement(Saxon.Hej.om.NodeName nameCode, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
        }
        public string getSystemId() {
            return systemId;
        }
        public virtual bool usesTypeAnnotations() {
            return false;
        }
    
        /* Expanding default method append from interface Receiver */
        public virtual void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }

        /* Expanding default method append from interface Receiver */
        public virtual void append(Saxon.Hej.om.Item item) {
            append(item, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
        }

        /* Expanding default method handlesAppend from interface Receiver */
        public virtual bool handlesAppend() {
            return false;
        }

    }
}