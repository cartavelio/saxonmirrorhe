using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class XML10ContentChecker : Saxon.Hej.@event.ProxyReceiver {
        public XML10ContentChecker(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                checkString(Saxon.Hej.str.StringView.of(att.getValue()), att.getLocation());
            }
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            checkString(chars, locationId);
            nextReceiver.characters(chars, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            checkString(chars, locationId);
            nextReceiver.comment(chars, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            checkString(data, locationId);
            nextReceiver.processingInstruction(target, data, locationId, properties);
        }
        private void checkString(Saxon.Hej.str.UnicodeString @in, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.z.IntIterator iter = @in.codePoints();
            while (iter.hasNext()) {
                int ch32 = iter.next();
                if (!(Saxon.Hej.serialize.charcode.XMLCharacterData.isValid10(ch32))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The result tree contains a character not allowed by XML 1.0 (hex "+Saxon.Impl.Helpers.Utils.intToHexString(ch32))+')'));
                    err.setErrorCode("SERE0006");
                    err.setLocator(locationId);
                    throw err;
                }
            }
        }
    
    }
}