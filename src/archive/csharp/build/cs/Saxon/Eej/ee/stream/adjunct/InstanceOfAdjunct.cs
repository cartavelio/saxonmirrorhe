using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class InstanceOfAdjunct : Saxon.Eej.ee.stream.adjunct.InspectionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.InstanceOfAdjunct.InstanceOfFeed(watchManager, getExpression(), @out, context);
        }
        private class InstanceOfFeed : Saxon.Eej.ee.stream.feed.GroundedItemFeed {
            internal Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            internal Saxon.Hej.type.ItemType itemType;
            internal int level;
            internal int count;
            internal int cardinality;
            internal bool matchesSoFar;
            internal bool closed;
            internal InstanceOfFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
                this.watchManager = watchManager;
                itemType = ((Saxon.Hej.expr.InstanceOfExpression)(exp)).getRequiredItemType();
                cardinality = ((Saxon.Hej.expr.InstanceOfExpression)(exp)).getRequiredCardinality();
                matchesSoFar = true;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                count = 0;
                level = 0;
                matchesSoFar = true;
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (((level == 0) && matchesSoFar)) {
                    if (!(itemType.matches(item, getContext().getConfiguration().getTypeHierarchy()))) {
                        matchesSoFar = false;
                    }
                    count++;
                    if (((count == 2) && !(Saxon.Hej.value.Cardinality.allowsMany(cardinality)))) {
                        matchesSoFar = false;
                    }
                    if (!(matchesSoFar)) {
                        getNextOutputter().append(Saxon.Hej.value.BooleanValue.FALSE);
                        if (watchManager.allowsEarlyExit()) {
                            closed = true;
                            getNextOutputter().close();
                            getTerminator().terminate();
                        }
                    }
                }
            }
            public override void close() {
                if (matchesSoFar) {
                    if (((count == 0) && !(Saxon.Hej.value.Cardinality.allowsZero(cardinality)))) {
                        matchesSoFar = false;
                    }
                    getNextOutputter().append(Saxon.Hej.value.BooleanValue.get(matchesSoFar));
                }
                if (!(closed)) {
                    getNextOutputter().close();
                }
            }
            public override void startDocument(int properties) {
                if ((level == 0)) {
                    Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                    orphan.setNodeKind(Saxon.Hej.type.Type.DOCUMENT);
                    append(orphan);
                }
                level++;
            }
            public override void endDocument() {
                level--;
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.s9api.Location location, int properties) {
                if (((level == 0) && matchesSoFar)) {
                    Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                    orphan.setNodeKind(Saxon.Hej.type.Type.ELEMENT);
                    orphan.setNodeName(elemName);
                    orphan.setTypeAnnotation(type);
                    orphan.setStringValue(Saxon.Hej.str.EmptyUnicodeString.getInstance());
                    append(orphan);
                }
                level++;
            }
            public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
                if ((level == 0)) {
                    base.@namespace(prefix, namespaceUri, properties);
                }
            }
            public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
                if ((level == 0)) {
                    base.attribute(attName, typeCode, value, location, properties);
                }
            }
            public override void endElement() {
                level--;
            }
            public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
                if ((level == 0)) {
                    base.characters(chars, location, properties);
                }
            }
            public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
                if ((level == 0)) {
                    base.processingInstruction(name, data, location, properties);
                }
            }
            public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
                if ((level == 0)) {
                    base.comment(content, location, properties);
                }
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                startElement(elemName, type, location, properties);
            }
        
        }
    
    }
}