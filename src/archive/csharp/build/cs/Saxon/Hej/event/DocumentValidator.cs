using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class DocumentValidator : Saxon.Hej.@event.ProxyReceiver {
        private bool foundElement = false;
        private int level = 0;
        private readonly string errorCode;
        public DocumentValidator(Saxon.Hej.@event.Receiver next, string errorCode) : base (next) {
            this.errorCode = errorCode;
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration config) {
            base.setPipelineConfiguration(config);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((foundElement && (level == 0))) {
                throw new Saxon.Hej.trans.XPathException("A valid document must have only one child element", errorCode);
            }
            foundElement = true;
            level++;
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                if (Saxon.Hej.value.Whitespace.isAllWhite(chars)) {
                    return ;
                }
                throw new Saxon.Hej.trans.XPathException((("A valid document must contain no text outside the outermost element (found \""+Saxon.Hej.trans.Err.truncate30(chars.tidy()))+"\")"), errorCode);
            }
            nextReceiver.characters(chars, locationId, properties);
        }
        public override void endElement() {
            level--;
            nextReceiver.endElement();
        }
        public override void endDocument() {
            if ((level == 0)) {
                if (!(foundElement)) {
                    throw new Saxon.Hej.trans.XPathException("A valid document must have a child element", errorCode);
                }
                foundElement = false;
                nextReceiver.endDocument();
                level = -1;
            }
        }
    
    }
}