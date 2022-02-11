using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class SequenceWrapper : Saxon.Hej.@event.SequenceReceiver {
        public static readonly string RESULT_NS = Saxon.Hej.query.QueryResult.RESULT_NS;
        private readonly Saxon.Hej.@event.ComplexContentOutputter @out;
        private int depth = 0;
        private Saxon.Hej.om.FingerprintedQName resultDocument;
        private Saxon.Hej.om.FingerprintedQName resultElement;
        private Saxon.Hej.om.FingerprintedQName resultAttribute;
        private Saxon.Hej.om.FingerprintedQName resultText;
        private Saxon.Hej.om.FingerprintedQName resultComment;
        private Saxon.Hej.om.FingerprintedQName resultPI;
        private Saxon.Hej.om.FingerprintedQName resultNamespace;
        private Saxon.Hej.om.FingerprintedQName resultAtomicValue;
        private Saxon.Hej.om.FingerprintedQName resultFunction;
        private Saxon.Hej.om.FingerprintedQName resultArray;
        private Saxon.Hej.om.FingerprintedQName resultMap;
        private Saxon.Hej.om.FingerprintedQName resultExternalValue;
        private Saxon.Hej.om.FingerprintedQName xsiType;
        private Saxon.Hej.om.NamespaceMap namespaces;
        public SequenceWrapper(Saxon.Hej.@event.Receiver destination) : base (destination.getPipelineConfiguration()) {
            @out = new Saxon.Hej.@event.ComplexContentOutputter(destination);
        }
        public Saxon.Hej.@event.ComplexContentOutputter getDestination() {
            return @out;
        }
        private void startWrapper(Saxon.Hej.om.NodeName name) {
            @out.startElement(name, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            @out.@namespace("", "", Saxon.Hej.@event.ReceiverOption.NONE);
            @out.startContent();
        }
        private void endWrapper() {
            @out.endElement();
        }
        public override void open() {
            Saxon.Hej.om.FingerprintedQName resultSequence = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "sequence");
            resultDocument = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "document");
            resultElement = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "element");
            resultAttribute = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "attribute");
            resultText = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "text");
            resultComment = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "comment");
            resultPI = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "processing-instruction");
            resultNamespace = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "namespace");
            resultAtomicValue = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "atomic-value");
            resultFunction = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "function");
            resultArray = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "array");
            resultMap = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "map");
            resultExternalValue = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "external-object");
            xsiType = new Saxon.Hej.om.FingerprintedQName("xsi", Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE, "type");
            @out.open();
            @out.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
            namespaces = Saxon.Hej.om.NamespaceMap.emptyMap().put("result", RESULT_NS).put("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA).put("xsi", Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE);
            startWrapper(resultSequence);
        }
        public override void startDocument(int properties) {
            startWrapper(resultDocument);
            depth++;
        }
        public override void endDocument() {
            endWrapper();
            depth--;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((depth++ == 0)) {
                startWrapper(resultElement);
            }
            @out.startElement(elemName, type, location, properties);
            @out.@namespace("", "", properties);
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                @out.attribute(att.getNodeName(), att.getType(), att.getValue(), att.getLocation(), att.getProperties());
            }
            @out.startContent();
        }
        public override void endElement() {
            @out.endElement();
            if ((--depth == 0)) {
                endWrapper();
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((depth == 0)) {
                startWrapper(resultText);
                @out.characters(chars, locationId, properties);
                endWrapper();
            } else {
                @out.characters(chars, locationId, properties);
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((depth == 0)) {
                startWrapper(resultComment);
                @out.comment(chars, locationId, properties);
                endWrapper();
            } else {
                @out.comment(chars, locationId, properties);
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((depth == 0)) {
                startWrapper(resultPI);
                @out.processingInstruction(target, data, locationId, properties);
                endWrapper();
            } else {
                @out.processingInstruction(target, data, locationId, properties);
            }
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((item is Saxon.Hej.value.AtomicValue)) {
                Saxon.Hej.om.NamePool pool = getNamePool();
                @out.startElement(resultAtomicValue, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                @out.@namespace("", "", Saxon.Hej.@event.ReceiverOption.NONE);
                Saxon.Hej.type.AtomicType type = ((Saxon.Hej.value.AtomicValue)(item)).getItemType();
                Saxon.Hej.om.StructuredQName name = type.getStructuredQName();
                string prefix = name.getPrefix();
                string localName = name.getLocalPart();
                string uri = name.getURI();
                if (prefix.Length==0) {
                    prefix = pool.suggestPrefixForURI(uri);
                    if ((prefix == null)) {
                        prefix = ("p"+uri.GetHashCode());
                    }
                }
                string displayName = ((prefix+':')+localName);
                @out.@namespace("", "", Saxon.Hej.@event.ReceiverOption.NONE);
                @out.@namespace(prefix, uri, Saxon.Hej.@event.ReceiverOption.NONE);
                @out.attribute(xsiType, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, displayName, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                @out.startContent();
                @out.characters(item.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                @out.endElement();
            } else {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                    int kind = node.getNodeKind();
                    if ((kind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                        attribute(Saxon.Hej.om.NameOfNode.makeName(node), (Saxon.Hej.type.SimpleType)(node.getSchemaType()), node.getStringValue(), Saxon.Hej.expr.parser.Loc.NONE, 0);
                    } else {
                        if ((kind == Saxon.Hej.type.Type.NAMESPACE)) {
                            @namespace(new Saxon.Hej.om.NamespaceBinding(node.getLocalPart(), node.getStringValue()), 0);
                        } else {
                            ((Saxon.Hej.om.NodeInfo)(item)).copy(this, (Saxon.Hej.om.CopyOptions.ALL_NAMESPACES|Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS), locationId);
                        }
                    }
                } else {
                    if ((item is Saxon.Hej.om.Function)) {
                        if ((item is Saxon.Hej.ma.map.MapItem)) {
                            @out.startElement(resultMap, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                            @out.startContent();
                            @out.characters(Saxon.Hej.str.StringView.of(item.toShortString()), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                            @out.endElement();
                        } else {
                            if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                                @out.startElement(resultArray, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                                @out.startContent();
                                @out.characters(Saxon.Hej.str.StringView.of(item.toShortString()), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                                @out.endElement();
                            } else {
                                @out.startElement(resultFunction, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                                @out.startContent();
                                @out.characters(Saxon.Hej.str.StringView.of(((Saxon.Hej.om.Function)(item)).getDescription()), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                                @out.endElement();
                            }
                        }
                    } else {
                        if ((item.getGenre() == Saxon.Hej.om.Genre.EXTERNAL)) {
                            System.Object obj = ((Saxon.Hej.value.ObjectValue<object>)(item)).getObject();
                            @out.startElement(resultExternalValue, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                            @out.attribute(new Saxon.Hej.om.NoNamespaceName("class"), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, obj.GetType().Name, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                            @out.startContent();
                            @out.characters(Saxon.Hej.str.StringView.of(obj.ToString()), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                            @out.endElement();
                        }
                    }
                }
            }
        }
        public override void close() {
            endWrapper();
            @out.endDocument();
            @out.close();
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
        private void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location locationId, int properties) {
            Saxon.Hej.om.AttributeMap atts = Saxon.Hej.om.SingletonAttributeMap.of(new Saxon.Hej.om.AttributeInfo(attName, typeCode, value.ToString(), locationId, properties));
            Saxon.Hej.om.NamespaceMap ns = Saxon.Hej.om.NamespaceMap.emptyMap();
            if (!(attName.hasURI(""))) {
                ns = ns.put(attName.getPrefix(), attName.getURI());
            }
            @out.startElement(resultAttribute, Saxon.Hej.type.Untyped.getInstance(), atts, ns, Saxon.Hej.expr.parser.Loc.NONE, 0);
            @out.startContent();
            @out.endElement();
        }
        private void @namespace(Saxon.Hej.om.NamespaceBindingSet namespaceBindings, int properties) {
            Saxon.Hej.om.NamespaceMap ns = Saxon.Hej.om.NamespaceMap.emptyMap();
            ns = ns.addAll(namespaceBindings);
            @out.startElement(resultNamespace, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.om.EmptyAttributeMap.getInstance(), ns, Saxon.Hej.expr.parser.Loc.NONE, 0);
            @out.startContent();
            @out.endElement();
        }
    
    }
}