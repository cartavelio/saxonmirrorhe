using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class CountingFilter : Saxon.Hej.@event.ProxyReceiver {
        private static int nextid = 0;
        private readonly int id;
        public CountingFilter(Saxon.Hej.@event.Receiver nextReceiver) : base (nextReceiver) {
            id = nextid++;
        }
        public CountingFilter(Saxon.Hej.@event.Receiver nextReceiver, Saxon.Ejava.io.PrintStream diagnosticOutput) : base (nextReceiver) {
            id = nextid++;
        }
        public int getId() {
            return id;
        }
        private void count(string counter) {
            Saxon.Hej.trace.Instrumentation.count(((("Filter "+id)+" ")+counter));
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            count("append");
            if ((nextReceiver is Saxon.Hej.@event.SequenceReceiver)) {
                nextReceiver.append(item, locationId, copyNamespaces);
            } else {
                base.append(item, locationId, copyNamespaces);
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            count("characters");
            nextReceiver.characters(chars, locationId, properties);
        }
        public override void close() {
            count("close");
            nextReceiver.close();
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            count("comment");
            nextReceiver.comment(chars, locationId, properties);
        }
        public override void endDocument() {
            count("endDocument");
            nextReceiver.endDocument();
        }
        public override void endElement() {
            count("endElement");
            nextReceiver.endElement();
        }
        public override void open() {
            count("open");
            nextReceiver.open();
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            count("processingInstruction");
            nextReceiver.processingInstruction(target, data, locationId, properties);
        }
        public override void startDocument(int properties) {
            count("startDocument");
            nextReceiver.startDocument(properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            count("startElement");
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        }
    
    }
}