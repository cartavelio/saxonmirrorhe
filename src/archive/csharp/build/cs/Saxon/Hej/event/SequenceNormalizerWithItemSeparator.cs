using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class SequenceNormalizerWithItemSeparator : Saxon.Hej.@event.SequenceNormalizer {
        private readonly Saxon.Hej.str.UnicodeString separator;
        private bool first = true;
        public SequenceNormalizerWithItemSeparator(Saxon.Hej.@event.Receiver next, Saxon.Hej.str.UnicodeString separator) : base (next) {
            this.separator = separator;
        }
        public override void open() {
            first = true;
            base.open();
        }
        public override void startDocument(int properties) {
            sep();
            base.startDocument(properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            sep();
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            sep();
            base.characters(chars, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            sep();
            base.processingInstruction(target, data, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            sep();
            base.comment(chars, locationId, properties);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                flatten((Saxon.Hej.ma.arrays.ArrayItem)(item), locationId, copyNamespaces);
            } else {
                if ((item is Saxon.Hej.value.AtomicValue)) {
                    sep();
                    nextReceiver.characters(item.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                } else {
                    decompose(item, locationId, copyNamespaces);
                }
            }
        }
        public override void close() {
            base.close();
        }
        private void sep() {
            if (((level == 0) && !(first))) {
                base.characters(separator, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            } else {
                first = false;
            }
        }
    
    }
}