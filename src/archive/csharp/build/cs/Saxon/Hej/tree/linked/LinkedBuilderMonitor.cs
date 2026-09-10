using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class LinkedBuilderMonitor : Saxon.Hej.@event.BuilderMonitor {
        private readonly Saxon.Hej.tree.linked.LinkedTreeBuilder builder;
        private int mark = -1;
        private Saxon.Hej.om.NodeInfo markedNode;
        public LinkedBuilderMonitor(Saxon.Hej.tree.linked.LinkedTreeBuilder builder) : base (builder) {
            this.builder = builder;
        }
        public override void markNextNode(int nodeKind) {
            mark = nodeKind;
        }
        public override void startDocument(int properties) {
            base.startDocument(properties);
            if ((mark == Saxon.Hej.type.Type.DOCUMENT)) {
                markedNode = builder.getCurrentParentNode();
            }
            mark = -1;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            base.startElement(elemName, type, attributes, namespaces, location, properties);
            if ((mark == Saxon.Hej.type.Type.ELEMENT)) {
                markedNode = builder.getCurrentParentNode();
            }
            mark = -1;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            base.characters(chars, locationId, properties);
            if ((mark == Saxon.Hej.type.Type.TEXT)) {
                markedNode = builder.getCurrentLeafNode();
            }
            mark = -1;
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            base.comment(chars, locationId, properties);
            if ((mark == Saxon.Hej.type.Type.COMMENT)) {
                markedNode = builder.getCurrentLeafNode();
            }
            mark = -1;
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            base.processingInstruction(target, data, locationId, properties);
            if ((mark == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION)) {
                markedNode = builder.getCurrentLeafNode();
            }
            mark = -1;
        }
        public override Saxon.Hej.om.NodeInfo getMarkedNode() {
            return markedNode;
        }
    
    }
}