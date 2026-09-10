using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public abstract class Emitter : Saxon.Hej.@event.SequenceReceiver, Saxon.Hej.@event.ReceiverWithOutputProperties {
        protected internal Saxon.Hej.str.UnicodeWriter writer;
        protected internal Saxon.Ejava.util.Properties outputProperties;
        protected internal Saxon.Hej.serialize.charcode.CharacterSet characterSet;
        protected internal bool allCharactersEncodable = false;
        private bool mustClose = false;
        public Emitter() : base (null) {
        }
        public void setOutputProperties(Saxon.Ejava.util.Properties details) {
            if ((characterSet == null)) {
                characterSet = getConfiguration().getCharacterSetFactory().getCharacterSet(details);
                allCharactersEncodable = (((characterSet is Saxon.Hej.serialize.charcode.UTF8CharacterSet) || (characterSet is Saxon.Hej.serialize.charcode.UTF16CharacterSet)));
            }
            outputProperties = details;
        }
        public Saxon.Ejava.util.Properties getOutputProperties() {
            return outputProperties;
        }
        public void setUnicodeWriter(Saxon.Hej.str.UnicodeWriter unicodeWriter) {
            this.writer = unicodeWriter;
        }
        public void setMustClose(bool mustClose) {
            this.mustClose = mustClose;
        }
        public override void setUnparsedEntity(string name, string uri, string publicId) {
        }
        public override void close() {
            if ((mustClose && (writer != null))) {
                try {
                    writer.close();
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException("Failed to close output stream");
                }
            }
        }
        public override bool usesTypeAnnotations() {
            return false;
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                decompose(item, locationId, copyNamespaces);
            } else {
                characters(item.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }
    
    }
}