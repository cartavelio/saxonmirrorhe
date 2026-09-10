using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public interface Receiver : Saxon.Ejavax.xml.transform.Result {
        void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe);
        Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration();
        void open();
        void startDocument(int properties);
        void endDocument();
        void setUnparsedEntity(string name, string systemID, string publicID);
        void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties);
        void endElement();
        void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties);
        void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties);
        void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties);
        void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties);
        void append(Saxon.Hej.om.Item item);
        void close();
        bool usesTypeAnnotations();
        bool handlesAppend();
    
    }
}