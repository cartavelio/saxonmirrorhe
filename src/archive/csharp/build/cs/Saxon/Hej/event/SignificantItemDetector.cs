using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class SignificantItemDetector : Saxon.Hej.@event.ProxyOutputter {
        private int level = 0;
        private bool empty = true;
        private readonly Saxon.Hej.om.Action trigger;
        public SignificantItemDetector(Saxon.Hej.@event.Outputter next, Saxon.Hej.om.Action trigger) : base (next) {
            this.trigger = trigger;
        }
        private void start() {
            if (empty) {
                /*Delegate.doAction()*/trigger();
                empty = false;
            }
        }
        public override void startDocument(int properties) {
            if ((level++ != 0)) {
                getNextOutputter().startDocument(properties);
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.s9api.Location location, int properties) {
            start();
            level++;
            getNextOutputter().startElement(elemName, type, location, properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            start();
            level++;
            getNextOutputter().startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
            start();
            getNextOutputter().@namespace(prefix, namespaceUri, properties);
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            start();
            getNextOutputter().attribute(attName, typeCode, value, location, properties);
        }
        public override void startContent() {
            getNextOutputter().startContent();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(chars.isEmpty())) {
                start();
            }
            getNextOutputter().characters(chars, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            start();
            getNextOutputter().processingInstruction(target, data, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            start();
            getNextOutputter().comment(chars, locationId, properties);
        }
        public static bool isSignificant(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                return ((((node.getNodeKind() != Saxon.Hej.type.Type.TEXT) || !(node.getUnicodeStringValue().isEmpty()))) && (((node.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT) || node.hasChildNodes())));
            } else {
                if ((item is Saxon.Hej.value.AtomicValue)) {
                    return !(item.getUnicodeStringValue().isEmpty());
                } else {
                    if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                        if (((Saxon.Hej.ma.arrays.ArrayItem)(item)).isEmpty()) {
                            return true;
                        } else {
                            foreach (Saxon.Hej.om.Sequence mem in ((Saxon.Hej.ma.arrays.ArrayItem)(item)).members()) {
                                try {
                                    Saxon.Hej.om.SequenceIterator memIter = mem.iterate();
                                    Saxon.Hej.om.Item it;
                                    while (((it = memIter.next()) != null)) {
                                        if (isSignificant(it)) {
                                            return true;
                                        }
                                    }
                                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                                    return true;
                                }
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if (isSignificant(item)) {
                start();
            }
            base.append(item, locationId, copyNamespaces);
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (isSignificant(item)) {
                start();
            }
            base.append(item);
        }
        public override void endDocument() {
            if ((--level != 0)) {
                getNextOutputter().endDocument();
            }
        }
        public override void endElement() {
            level--;
            getNextOutputter().endElement();
        }
        public bool isEmpty() {
            return empty;
        }
    
    }
}