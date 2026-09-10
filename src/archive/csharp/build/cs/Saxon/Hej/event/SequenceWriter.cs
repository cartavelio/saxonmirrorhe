using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class SequenceWriter : Saxon.Hej.@event.SequenceReceiver {
        private Saxon.Hej.om.TreeModel treeModel = null;
        private Saxon.Hej.@event.Builder builder = null;
        private int level = 0;
        public SequenceWriter(Saxon.Hej.@event.PipelineConfiguration pipe) : base (pipe) {
        }
        public abstract void write(Saxon.Hej.om.Item item);
        public override void startDocument(int properties) {
            if ((builder == null)) {
                createTree(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.MUTABLE_TREE));
            }
            if ((level++ == 0)) {
                builder.startDocument(properties);
            }
        }
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
            if ((builder != null)) {
                builder.setUnparsedEntity(name, systemID, publicID);
            }
        }
        private void createTree(bool mutable) {
            Saxon.Hej.@event.PipelineConfiguration pipe = getPipelineConfiguration();
            if ((treeModel != null)) {
                builder = treeModel.makeBuilder(pipe);
            } else {
                if ((pipe.getController() != null)) {
                    if (mutable) {
                        Saxon.Hej.om.TreeModel model = pipe.getController().getModel();
                        if (model.isMutable()) {
                            builder = pipe.getController().makeBuilder();
                        } else {
                            builder = new Saxon.Hej.tree.linked.LinkedTreeBuilder(pipe);
                        }
                    } else {
                        builder = pipe.getController().makeBuilder();
                    }
                } else {
                    Saxon.Hej.om.TreeModel model = getConfiguration().getParseOptions().getModel();
                    builder = model.makeBuilder(pipe);
                }
            }
            builder.setPipelineConfiguration(pipe);
            builder.setSystemId(systemId);
            builder.setBaseURI(systemId);
            builder.setTiming(false);
            builder.setUseEventLocation(false);
            builder.open();
        }
        public Saxon.Hej.om.TreeModel getTreeModel() {
            return treeModel;
        }
        public void setTreeModel(Saxon.Hej.om.TreeModel treeModel) {
            this.treeModel = treeModel;
        }
        public override void endDocument() {
            if ((--level == 0)) {
                builder.endDocument();
                Saxon.Hej.om.NodeInfo doc = builder.getCurrentRoot();
                append(doc, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                builder = null;
                systemId = null;
            }
            previousAtomic = false;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((builder == null)) {
                createTree(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.MUTABLE_TREE));
            }
            builder.startElement(elemName, type, attributes, namespaces, location, properties);
            level++;
            previousAtomic = false;
        }
        public override void endElement() {
            builder.endElement();
            if ((--level == 0)) {
                builder.close();
                Saxon.Hej.om.NodeInfo element = builder.getCurrentRoot();
                append(element, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                builder = null;
                systemId = null;
            }
            previousAtomic = false;
        }
        public override void characters(Saxon.Hej.str.UnicodeString s, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                o.setNodeKind(Saxon.Hej.type.Type.TEXT);
                o.setStringValue(s.tidy());
                write(o);
            } else {
                if (!(s.isEmpty())) {
                    builder.characters(s, locationId, properties);
                }
            }
            previousAtomic = false;
        }
        public override void comment(Saxon.Hej.str.UnicodeString comment, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                o.setNodeKind(Saxon.Hej.type.Type.COMMENT);
                o.setStringValue(comment.tidy());
                write(o);
            } else {
                builder.comment(comment, locationId, properties);
            }
            previousAtomic = false;
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                o.setNodeName(new Saxon.Hej.om.NoNamespaceName(target));
                o.setNodeKind(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION);
                o.setStringValue(data.tidy());
                write(o);
            } else {
                builder.processingInstruction(target, data, locationId, properties);
            }
            previousAtomic = false;
        }
        public override void close() {
            previousAtomic = false;
            if ((builder != null)) {
                builder.close();
            }
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((item != null)) {
                if ((level == 0)) {
                    write(item);
                    previousAtomic = false;
                } else {
                    decompose(item, locationId, copyNamespaces);
                }
            }
        }
        public override bool usesTypeAnnotations() {
            return ((builder == null) || builder.usesTypeAnnotations());
        }
    
    }
}