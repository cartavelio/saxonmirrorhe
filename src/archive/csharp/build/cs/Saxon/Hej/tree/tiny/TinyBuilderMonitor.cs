using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public class TinyBuilderMonitor : Saxon.Hej.@event.BuilderMonitor {
        private readonly Saxon.Hej.tree.tiny.TinyBuilder builder;
        private int mark = -1;
        private int markedNodeNr = -1;
        private readonly int markedAttribute = -1;
        private readonly int markedNamespace = -1;
        public TinyBuilderMonitor(Saxon.Hej.tree.tiny.TinyBuilder builder) : base (builder) {
            this.builder = builder;
        }
        public override void markNextNode(int nodeKind) {
            mark = nodeKind;
        }
        public override void startDocument(int properties) {
            if ((mark == Saxon.Hej.type.Type.DOCUMENT)) {
                markedNodeNr = builder.getTree().getNumberOfNodes();
            }
            mark = -1;
            base.startDocument(properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((mark == Saxon.Hej.type.Type.ELEMENT)) {
                markedNodeNr = builder.getTree().getNumberOfNodes();
            }
            mark = -1;
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((mark == Saxon.Hej.type.Type.TEXT)) {
                markedNodeNr = builder.getTree().getNumberOfNodes();
            }
            mark = -1;
            base.characters(chars, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((mark == Saxon.Hej.type.Type.COMMENT)) {
                markedNodeNr = builder.getTree().getNumberOfNodes();
            }
            mark = -1;
            base.comment(chars, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((mark == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION)) {
                markedNodeNr = builder.getTree().getNumberOfNodes();
            }
            mark = -1;
            base.processingInstruction(target, data, locationId, properties);
        }
        public override Saxon.Hej.om.NodeInfo getMarkedNode() {
            if ((markedNodeNr != -1)) {
                return ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)builder.getTree().getNode(markedNodeNr));
            } else {
                if ((markedAttribute != -1)) {
                    return builder.getTree().getAttributeNode(markedAttribute);
                } else {
                    return null;
                }
            }
        }
    
    }
}