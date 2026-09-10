using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public abstract class ComplexNodeEventFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private bool opened = false;
        private bool previousAtomic = false;
        protected internal Saxon.Hej.@event.Outputter next;
        public ComplexNodeEventFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed parentExpressionFeed, Saxon.Hej.expr.XPathContext context) : base (exp, parentExpressionFeed, context) {
            next = parentExpressionFeed;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            if (!(opened)) {
                base.open(terminator);
                try {
                    processLeft(getContext());
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
                opened = true;
            }
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            return null;
        }
        public override void append(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Eej.ee.stream.om.FleetingParentNode)) {
                throw new Saxon.Hej.trans.XPathException("Internal streaming error: streamed node cannot be used directly to construct output node", Saxon.Hej.trans.SaxonErrorCode.SXST0067, getExpression().getLocation());
            }
            if ((opened && !(hasFailed()))) {
                try {
                    Saxon.Hej.@event.Outputter @out = next;
                    if ((item is Saxon.Hej.om.NodeInfo)) {
                        Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                        int kind = node.getNodeKind();
                        if ((kind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                            @out.attribute(Saxon.Hej.om.NameOfNode.makeName(node), (Saxon.Hej.type.SimpleType)(node.getSchemaType()), node.getStringValue(), getExpression().getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
                        } else {
                            ((Saxon.Hej.om.NodeInfo)(item)).copy(@out, (Saxon.Hej.om.CopyOptions.ALL_NAMESPACES|Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS), getExpression().getLocation());
                        }
                        previousAtomic = false;
                    } else {
                        if ((item is Saxon.Hej.value.AtomicValue)) {
                            if (previousAtomic) {
                                @out.characters(Saxon.Hej.str.StringConstants.SINGLE_SPACE, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.SEPARATOR);
                            }
                            @out.characters(item.getUnicodeStringValue(), getExpression().getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
                            previousAtomic = true;
                        } else {
                            if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                                foreach (Saxon.Hej.om.GroundedValue members in ((Saxon.Hej.ma.arrays.ArrayItem)(item)).members()) {
                                    foreach (Saxon.Hej.om.Item it in members.asIterable()) {
                                        append(it);
                                    }
                                }
                            } else {
                                throw new Saxon.Hej.trans.XPathException("Cannot write a function item to the content of an XDM tree", "XTDE0450");
                            }
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
        }
        public override void close() {
            if ((opened && !(hasFailed()))) {
                try {
                    processRight(getContext());
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                    return ;
                }
                base.close();
                opened = false;
            }
        }
        protected internal void setComplexContentOutputter(Saxon.Hej.@event.ComplexContentOutputter @out) {
        }
        abstract protected internal void processLeft(Saxon.Hej.expr.XPathContext context);
        abstract protected internal void processRight(Saxon.Hej.expr.XPathContext context);
        public override void startDocument(int properties) {
            if (!(hasFailed())) {
                try {
                    next.startDocument(properties);
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void endDocument() {
            if (!(hasFailed())) {
                try {
                    next.endDocument();
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            if (!(hasFailed())) {
                try {
                    next.startElement(elemName, typeCode, location, properties);
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (!(hasFailed())) {
                try {
                    next.startElement(elemName, type, attributes, namespaces, location, properties);
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void @namespace(string prefix, string namespaceUri, int properties) {
            if (!(hasFailed())) {
                try {
                    next.@namespace(prefix, namespaceUri, properties);
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            if (!(hasFailed())) {
                try {
                    next.attribute(attName, typeCode, value, location, properties);
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void startContent() {
            if (!(hasFailed())) {
                try {
                    next.startContent();
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void endElement() {
            if (!(hasFailed())) {
                try {
                    next.endElement();
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            if (!(hasFailed())) {
                try {
                    next.characters(chars, location, properties);
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            if (!(hasFailed())) {
                try {
                    next.processingInstruction(name, data, location, properties);
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            if (!(hasFailed())) {
                try {
                    next.comment(content, location, properties);
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            previousAtomic = false;
        }
    
    }
}