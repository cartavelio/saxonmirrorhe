using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public abstract class GroundedItemFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private int level;
        private Saxon.Hej.@event.Builder builder;
        private Saxon.Hej.@event.Outputter buildingOutputter;
        public GroundedItemFeed(Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result, context) {
        }
        public GroundedItemFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
        }
        private void makeBuilder() {
            builder = new Saxon.Hej.tree.tiny.TinyBuilder(getPipelineConfiguration());
            buildingOutputter = new Saxon.Hej.@event.ComplexContentOutputter(builder);
            buildingOutputter.open();
        }
        public override void startDocument(int properties) {
            if ((buildingOutputter == null)) {
                level = 0;
                makeBuilder();
            } else {
                level++;
            }
            buildingOutputter.startDocument(0);
        }
        public override void endDocument() {
            level--;
            buildingOutputter.endDocument();
            if ((level == 0)) {
                buildingOutputter.close();
                Saxon.Hej.om.NodeInfo node = builder.getCurrentRoot();
                append(node);
                builder = null;
                buildingOutputter = null;
            }
        }
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
            if ((buildingOutputter != null)) {
                buildingOutputter.setUnparsedEntity(name, systemID, publicID);
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((builder == null)) {
                level = 0;
                makeBuilder();
            }
            level++;
            buildingOutputter.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            if ((builder == null)) {
                level = 0;
                makeBuilder();
            }
            level++;
            buildingOutputter.startElement(elemName, typeCode, location, properties);
        }
        public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
            if ((buildingOutputter == null)) {
                Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                orphan.setNodeKind(Saxon.Hej.type.Type.NAMESPACE);
                orphan.setNodeName(new Saxon.Hej.om.NoNamespaceName(prefix));
                orphan.setStringValue(namespaceUri.toUnicodeString());
                append(orphan);
            } else {
                buildingOutputter.@namespace(prefix, namespaceUri, properties);
            }
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            if ((buildingOutputter == null)) {
                Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                orphan.setNodeKind(Saxon.Hej.type.Type.ATTRIBUTE);
                orphan.setTypeAnnotation(typeCode);
                orphan.setNodeName(attName);
                orphan.setStringValue(Saxon.Hej.str.StringView.tidy(value));
                append(orphan);
            } else {
                buildingOutputter.attribute(attName, typeCode, value, location, properties);
            }
        }
        public override void startContent() {
            if ((buildingOutputter != null)) {
                buildingOutputter.startContent();
            }
        }
        public override void endElement() {
            level--;
            if ((buildingOutputter != null)) {
                buildingOutputter.endElement();
                if ((level == 0)) {
                    buildingOutputter.close();
                    Saxon.Hej.om.NodeInfo node = builder.getCurrentRoot();
                    append(node);
                    builder = null;
                    buildingOutputter = null;
                }
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            if ((buildingOutputter == null)) {
                Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                orphan.setNodeKind(Saxon.Hej.type.Type.TEXT);
                orphan.setStringValue(chars.tidy());
                append(orphan);
            } else {
                buildingOutputter.characters(chars, location, properties);
            }
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            if ((buildingOutputter == null)) {
                Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                orphan.setNodeKind(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION);
                orphan.setNodeName(new Saxon.Hej.om.NoNamespaceName(name));
                orphan.setStringValue(data.tidy());
                append(orphan);
            } else {
                buildingOutputter.processingInstruction(name, data, location, properties);
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            if ((buildingOutputter == null)) {
                Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                orphan.setNodeKind(Saxon.Hej.type.Type.COMMENT);
                orphan.setStringValue(content.tidy());
                append(orphan);
            } else {
                buildingOutputter.comment(content, location, properties);
            }
        }
    
    }
}