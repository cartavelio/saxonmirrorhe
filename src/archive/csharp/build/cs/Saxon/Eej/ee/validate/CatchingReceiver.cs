using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class CatchingReceiver : Saxon.Hej.@event.ProxyReceiver {
        private readonly Saxon.Hej.lib.ErrorReporter listener;
        private bool failed = false;
        public CatchingReceiver(Saxon.Hej.@event.Receiver next, Saxon.Hej.lib.ErrorReporter listener) : base (next) {
            this.listener = listener;
        }
        public override void open() {
            if (!(failed)) {
                try {
                    base.open();
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public override void close() {
            if (!(failed)) {
                try {
                    base.close();
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public override void startDocument(int properties) {
            if (!(failed)) {
                try {
                    base.startDocument(properties);
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public override void endDocument() {
            if (!(failed)) {
                try {
                    base.endDocument();
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (!(failed)) {
                try {
                    base.startElement(elemName, type, attributes, namespaces, location, properties);
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public override void endElement() {
            if (!(failed)) {
                try {
                    base.endElement();
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(failed)) {
                try {
                    base.characters(chars, locationId, properties);
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(failed)) {
                try {
                    base.processingInstruction(target, data, locationId, properties);
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(failed)) {
                try {
                    base.comment(chars, locationId, properties);
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if (!(failed)) {
                try {
                    base.append(item, locationId, copyNamespaces);
                } catch (Saxon.Hej.trans.XPathException err) {
                    failed = true;
                    listener.report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
    
    }
}