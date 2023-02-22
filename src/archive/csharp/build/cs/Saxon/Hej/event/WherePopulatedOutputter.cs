using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class WherePopulatedOutputter : Saxon.Hej.@event.ProxyOutputter {
        private int level = 0;
        private bool pendingStartTag = false;
        private Saxon.Hej.om.NodeName pendingElemName;
        private Saxon.Hej.type.SchemaType pendingSchemaType;
        private Saxon.Hej.s9api.Location pendingLocationId;
        private int pendingProperties;
        private Saxon.Hej.om.AttributeMap pendingAttributes;
        private Saxon.Hej.om.NamespaceMap pendingNamespaces;
        public WherePopulatedOutputter(Saxon.Hej.@event.Outputter next) : base (next) {
        }
        public override void startDocument(int properties) {
            if ((level++ == 0)) {
                pendingStartTag = true;
                pendingElemName = null;
                pendingProperties = properties;
            } else {
                base.startDocument(properties);
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.s9api.Location location, int properties) {
            releaseStartTag();
            if ((level++ == 0)) {
                pendingStartTag = true;
                pendingElemName = elemName;
                pendingSchemaType = type;
                pendingLocationId = location.saveLocation();
                pendingProperties = properties;
                pendingAttributes = Saxon.Hej.om.EmptyAttributeMap.getInstance();
                pendingNamespaces = Saxon.Hej.om.NamespaceMap.emptyMap();
            } else {
                base.startElement(elemName, type, location, properties);
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            releaseStartTag();
            if ((level++ == 0)) {
                pendingStartTag = true;
                pendingElemName = elemName;
                pendingSchemaType = type;
                pendingLocationId = location.saveLocation();
                pendingProperties = properties;
                pendingAttributes = attributes;
                pendingNamespaces = namespaces;
            } else {
                base.startElement(elemName, type, attributes, namespaces, location, properties);
            }
        }
        public override void @namespace(string prefix, string namespaceUri, int properties) {
            if ((level == 1)) {
                pendingNamespaces = pendingNamespaces.put(prefix, namespaceUri);
            } else {
                base.@namespace(prefix, namespaceUri, properties);
            }
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            if ((level == 1)) {
                pendingAttributes = pendingAttributes.put(new Saxon.Hej.om.AttributeInfo(attName, typeCode, value.ToString(), location, properties));
            } else {
                if (!((((level == 0) && (value.Length == 0))))) {
                    base.attribute(attName, typeCode, value, location, properties);
                }
            }
        }
        public override void endDocument() {
            if ((--level == 0)) {
                if (!(pendingStartTag)) {
                    base.endDocument();
                }
            } else {
                base.endDocument();
            }
        }
        public override void endElement() {
            if ((--level == 0)) {
                if (!(pendingStartTag)) {
                    base.endElement();
                }
            } else {
                base.endElement();
            }
            pendingStartTag = false;
        }
        public void releaseStartTag() {
            if (((level>=1) && pendingStartTag)) {
                if ((pendingElemName == null)) {
                    getNextOutputter().startDocument(pendingProperties);
                } else {
                    getNextOutputter().startElement(pendingElemName, pendingSchemaType, pendingAttributes, pendingNamespaces, pendingLocationId, pendingProperties);
                }
                pendingStartTag = false;
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((level == 0)) {
                if (!(chars.isEmpty())) {
                    base.characters(chars, locationId, properties);
                }
            } else {
                if ((level == 1)) {
                    if (!(chars.isEmpty())) {
                        releaseStartTag();
                        base.characters(chars, locationId, properties);
                    }
                } else {
                    base.characters(chars, locationId, properties);
                }
            }
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            if ((level == 0)) {
                if (!(data.isEmpty())) {
                    base.processingInstruction(name, data, location, properties);
                }
            } else {
                if ((level == 1)) {
                    if (!(data.isEmpty())) {
                        releaseStartTag();
                        base.processingInstruction(name, data, location, properties);
                    }
                } else {
                    base.processingInstruction(name, data, location, properties);
                }
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            if ((level == 0)) {
                if (!(content.isEmpty())) {
                    base.comment(content, location, properties);
                }
            } else {
                if ((level == 1)) {
                    if (!(content.isEmpty())) {
                        releaseStartTag();
                        base.comment(content, location, properties);
                    }
                } else {
                    base.comment(content, location, properties);
                }
            }
        }
        public override void append(Saxon.Hej.om.Item item) {
            if ((level == 0)) {
                if (!(Saxon.Hej.expr.instruct.WherePopulated.isDeemedEmpty(item))) {
                    getNextOutputter().append(item);
                }
            } else {
                if (((level == 1) && pendingStartTag)) {
                    if ((item is Saxon.Hej.om.NodeInfo)) {
                        Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                        switch (node.getNodeKind()) {
                          case Saxon.Hej.type.Type.TEXT:
                            if (((node.getNodeKind() == Saxon.Hej.type.Type.TEXT) && (node.getUnicodeStringValue().length() == 0))) {
                                return ;
                            }
                            break ;
                          case Saxon.Hej.type.Type.DOCUMENT:
                            if (((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT) && !(node.hasChildNodes()))) {
                                return ;
                            }
                            break ;
                          case Saxon.Hej.type.Type.ATTRIBUTE:
                            attribute(Saxon.Hej.om.NameOfNode.makeName(node), (Saxon.Hej.type.SimpleType)(node.getSchemaType()), node.getStringValue(), Saxon.Hej.expr.parser.Loc.NONE, 0);
                            return ;
                          case Saxon.Hej.type.Type.NAMESPACE:
                            @namespace(node.getLocalPart(), node.getStringValue(), 0);
                            return ;
                          default:
                            break ;
                        }
                    }
                    releaseStartTag();
                    getNextOutputter().append(item);
                } else {
                    base.append(item);
                }
            }
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((level == 0)) {
                if (!(Saxon.Hej.expr.instruct.WherePopulated.isDeemedEmpty(item))) {
                    getNextOutputter().append(item, locationId, copyNamespaces);
                }
            } else {
                if (((level == 1) && pendingStartTag)) {
                    if ((item is Saxon.Hej.om.NodeInfo)) {
                        Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                        switch (node.getNodeKind()) {
                          case Saxon.Hej.type.Type.TEXT:
                            if (((node.getNodeKind() == Saxon.Hej.type.Type.TEXT) && (node.getUnicodeStringValue().length() == 0))) {
                                return ;
                            }
                            break ;
                          case Saxon.Hej.type.Type.DOCUMENT:
                            if (((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT) && !(node.hasChildNodes()))) {
                                return ;
                            }
                            break ;
                          case Saxon.Hej.type.Type.ATTRIBUTE:
                            attribute(Saxon.Hej.om.NameOfNode.makeName(node), (Saxon.Hej.type.SimpleType)(node.getSchemaType()), node.getStringValue(), locationId, 0);
                            return ;
                          case Saxon.Hej.type.Type.NAMESPACE:
                            @namespace(node.getLocalPart(), node.getStringValue(), 0);
                            return ;
                          default:
                            break ;
                        }
                    }
                    releaseStartTag();
                    getNextOutputter().append(item, locationId, copyNamespaces);
                } else {
                    base.append(item, locationId, copyNamespaces);
                }
            }
        }
    
    }
}