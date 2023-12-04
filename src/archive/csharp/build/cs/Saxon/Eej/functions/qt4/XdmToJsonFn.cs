using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class XdmToJsonFn : Saxon.Hej.functions.SystemFunction {

        static XdmToJsonFn() {
            Saxon.Hej.functions.OptionsParameter parseJsonOptions = new Saxon.Hej.functions.OptionsParameter();
            parseJsonOptions.addAllowedOption("indent", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            parseJsonOptions.addAllowedOption("uniform", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            parseJsonOptions.addAllowedOption("escape-solidus", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            parseJsonOptions.addAllowedOption("layouts", layoutMapType, new Saxon.Impl.DataModel.ImmMap());
            OPTION_DETAILS = parseJsonOptions;
        }
        public static new Saxon.Hej.functions.OptionsParameter OPTION_DETAILS;
        private readonly static Saxon.Hej.value.SequenceType layoutMapType = new Saxon.Hej.value.SequenceType(new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.QNAME, Saxon.Hej.value.SequenceType.SINGLE_STRING), Saxon.Hej.expr.StaticProperty.ALLOWS_ONE);
        private class JsonConversionContext {
            public Saxon.Hej.Configuration config;
            public Saxon.Hej.serialize.JSONEmitter emitter;
            public Saxon.Hej.z.IntToIntHashMap uniformityData = null;
            public Saxon.Hej.z.IntHashMap<string> layoutMap = new Saxon.Hej.z.IntHashMap<string>();
            public System.Collections.Generic.Stack<Saxon.Hej.om.StructuredQName> elementNameStack = new System.Collections.Generic.Stack<Saxon.Hej.om.StructuredQName>();
        
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.GroundedValue val = arguments[0].materialize();
            Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc = new Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext();
            jcc.config = context.getConfiguration();
            jcc.elementNameStack.Push(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "dummy"));
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> checkedOptions = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.GroundedValue>();
            bool indent = false;
            bool escapeSolidus = false;
            bool uniform = false;
            if ((arguments.Length>1)) {
                Saxon.Hej.ma.map.MapItem suppliedOptions = (Saxon.Hej.ma.map.MapItem)(arguments[1].head());
                checkedOptions = getDetails().optionDetails.processSuppliedOptions(suppliedOptions, context);
                indent = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "indent"))).getBooleanValue();
                escapeSolidus = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "escape-solidus"))).getBooleanValue();
                uniform = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "uniform"))).getBooleanValue();
                initLayoutMap(jcc, checkedOptions, context.getConfiguration());
            }
            Saxon.Hej.@event.PipelineConfiguration pipe = context.getConfiguration().makePipelineConfiguration();
            Saxon.Hej.str.UnicodeBuilder builder = new Saxon.Hej.str.UnicodeBuilder();
            Saxon.Ejava.util.Properties outputProperties = new Saxon.Ejava.util.Properties();
            outputProperties.setProperty(Saxon.Hej.lib.SaxonOutputKeys.UNFAILING, "yes");
            outputProperties.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT, (indent ? "yes" : "no"));
            outputProperties.setProperty(Saxon.Hej.lib.SaxonOutputKeys.ESCAPE_SOLIDUS, (escapeSolidus ? "yes" : "no"));
            if (uniform) {
                jcc.uniformityData = gatherUniformityData(val);
            }
            jcc.emitter = new Saxon.Hej.serialize.JSONEmitter(pipe, builder, outputProperties);
            emitSequence(val, jcc, false);
            return new Saxon.Hej.value.StringValue(builder.toUnicodeString());
        }
        private void initLayoutMap(Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc, System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> checkedOptions, Saxon.Hej.Configuration config) {
            Saxon.Hej.ma.map.MapItem val = (Saxon.Hej.ma.map.MapItem)(Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "layouts").materialize());
            foreach (Saxon.Hej.ma.map.KeyValuePair pair in val.keyValuePairs()) {
                Saxon.Hej.om.StructuredQName qn = ((Saxon.Hej.value.QNameValue)(pair.key)).getStructuredQName();
                int fp = config.getNamePool().allocateFingerprint(qn.getNamespaceUri(), qn.getLocalPart());
                jcc.layoutMap.put(fp, ((Saxon.Hej.value.StringValue)(pair.value)).getStringValue());
            }
        }
        private Saxon.Hej.z.IntToIntHashMap gatherUniformityData(Saxon.Hej.om.GroundedValue input) {
            Saxon.Hej.z.IntToIntHashMap data = new Saxon.Hej.z.IntToIntHashMap();
            foreach (Saxon.Hej.om.Item it in input.asIterable()) {
                if ((it is Saxon.Hej.om.NodeInfo)) {
                    if ((((Saxon.Hej.om.NodeInfo)(it)).getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                        Saxon.Hej.tree.iter.AxisIterator iter = ((Saxon.Hej.om.NodeInfo)(it)).iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                        Saxon.Hej.om.NodeInfo element;
                        while (((element = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                            updateUniformityData(element, data);
                        }
                    } else {
                        if ((((Saxon.Hej.om.NodeInfo)(it)).getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                            Saxon.Hej.tree.iter.AxisIterator iter = ((Saxon.Hej.om.NodeInfo)(it)).iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                            Saxon.Hej.om.NodeInfo element;
                            while (((element = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                                updateUniformityData(element, data);
                            }
                        }
                    }
                }
            }
            return data;
        }
        private const int HAS_ATTRIBUTES = (1<<30);
        private const int HAS_CHILD_ELEMENTS = (1<<29);
        private const int HAS_NON_WHITESPACE_TEXT_NODES = (1<<28);
        private const int HAS_WHITESPACE_TEXT_NODES = (1<<27);
        private const int HAS_DIVERSE_CHILD_ELEMENTS = (1<<26);
        private const int HAS_REPEATED_CHILD_ELEMENTS = (1<<25);
        private const int HAS_UNIQUE_CHILD_ELEMENTS = (1<<24);
        private void updateUniformityData(Saxon.Hej.om.NodeInfo element, Saxon.Hej.z.IntToIntHashMap data) {
            int fp = element.getFingerprint();
            bool existing = data.contains(fp);
            int properties = (existing ? data.get(fp) : 0);
            int newProperties = getElementProperties(element, properties);
            if (((newProperties != properties) || !(existing))) {
                data.put(fp, newProperties);
            }
        }
        private int getElementProperties(Saxon.Hej.om.NodeInfo element, int properties) {
            if (((((properties&HAS_ATTRIBUTES)) == 0) && (element.attributes().size()>0))) {
                properties |= HAS_ATTRIBUTES;
            }
            if (((((properties&(((HAS_CHILD_ELEMENTS|HAS_NON_WHITESPACE_TEXT_NODES)|HAS_DIVERSE_CHILD_ELEMENTS)))) == 0) && element.hasChildNodes())) {
                int firstChild = -1;
                int countChildElements = 0;
                bool hasDuplicates = false;
                Saxon.Hej.z.IntSet fingerprints = new Saxon.Hej.z.IntHashSet();
                foreach (Saxon.Hej.om.NodeInfo child in element.children()) {
                    if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                        if (Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue())) {
                            properties |= HAS_WHITESPACE_TEXT_NODES;
                        } else {
                            properties |= HAS_NON_WHITESPACE_TEXT_NODES;
                        }
                    } else {
                        if ((child.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                            int childFingerprint = child.getFingerprint();
                            if (fingerprints.contains(childFingerprint)) {
                                hasDuplicates = true;
                            }
                            countChildElements++;
                            properties |= HAS_CHILD_ELEMENTS;
                            fingerprints.add(childFingerprint);
                            if ((firstChild != -1)) {
                                if ((firstChild != childFingerprint)) {
                                    properties |= HAS_DIVERSE_CHILD_ELEMENTS;
                                } else {
                                    properties |= HAS_REPEATED_CHILD_ELEMENTS;
                                }
                            } else {
                                firstChild = child.getFingerprint();
                            }
                        }
                    }
                }
                if (((firstChild != -1) && ((((properties&HAS_DIVERSE_CHILD_ELEMENTS)) == 0)))) {
                    if ((((properties&Saxon.Hej.om.NamePool.FP_MASK)) == 0)) {
                        properties |= ((firstChild&Saxon.Hej.om.NamePool.FP_MASK));
                    } else {
                        properties |= HAS_DIVERSE_CHILD_ELEMENTS;
                    }
                }
                if ((countChildElements == 1)) {
                    properties |= HAS_DIVERSE_CHILD_ELEMENTS;
                }
                if (!(hasDuplicates)) {
                    properties |= HAS_UNIQUE_CHILD_ELEMENTS;
                }
            }
            return properties;
        }
        private void emitSequence(Saxon.Hej.om.GroundedValue val, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc, bool forceArray) {
            if (((val.getLength() == 0) && !(forceArray))) {
                jcc.emitter.writeAtomicValue(null);
            } else {
                if (((val.getLength() == 1) && !(forceArray))) {
                    emitItem(val.head(), jcc);
                } else {
                    jcc.emitter.startArray(true);
                    foreach (Saxon.Hej.om.Item it in val.asIterable()) {
                        emitItem(it, jcc);
                    }
                    jcc.emitter.endArray();
                }
            }
        }
        private void emitItem(Saxon.Hej.om.Item item, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            Saxon.Hej.serialize.JSONEmitter emitter = jcc.emitter;
            if ((item is Saxon.Hej.value.AtomicValue)) {
                if ((item is Saxon.Hej.value.NumericValue)) {
                    if (((Saxon.Hej.value.NumericValue)(item)).isNaN()) {
                        emitter.writeStringValue("NaN");
                    } else {
                        if (System.Double.IsInfinity(((Saxon.Hej.value.NumericValue)(item)).getDoubleValue())) {
                            emitter.writeStringValue(((((Saxon.Hej.value.NumericValue)(item)).getDoubleValue()<0) ? "-INF" : "INF"));
                        } else {
                            emitter.writeAtomicValue((Saxon.Hej.value.AtomicValue)(item));
                        }
                    }
                } else {
                    emitter.writeAtomicValue((Saxon.Hej.value.AtomicValue)(item));
                }
            } else {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    switch (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind()) {
                      case Saxon.Hej.type.Type.DOCUMENT:
                        emitter.startMap(true);
                        emitter.writeKey("#document");
                        if (((Saxon.Hej.om.NodeInfo)(item)).hasChildNodes()) {
                            emitMixedElementContent((Saxon.Hej.om.NodeInfo)(item), jcc, true);
                        }
                        emitter.endMap();
                        break ;
                      case Saxon.Hej.type.Type.ATTRIBUTE:
                        emitter.startMap(true);
                        Saxon.Hej.om.StructuredQName name = Saxon.Hej.functions.NodeName_1.nodeName(((Saxon.Hej.om.NodeInfo)(item))).getStructuredQName();
                        Saxon.Hej.om.NamespaceUri ns = name.getNamespaceUri();
                        string attKey;
                        if (ns.Equals(Saxon.Hej.om.NamespaceUri.NULL)) {
                            attKey = ("@"+name.getLocalPart());
                        } else {
                            if (ns.Equals(Saxon.Hej.om.NamespaceUri.XML)) {
                                attKey = ("@xml:"+name.getLocalPart());
                            } else {
                                attKey = ("@"+name.getEQName());
                            }
                        }
                        emitter.writeKey(attKey);
                        Saxon.Hej.type.SchemaType type = ((Saxon.Hej.om.NodeInfo)(item)).getSchemaType();
                        emitSequence(item.atomize(), jcc, ((Saxon.Hej.type.SimpleType)(type)).isListType());
                        emitter.endMap();
                        break ;
                      case Saxon.Hej.type.Type.COMMENT:
                        emitter.startMap(true);
                        emitter.writeKey("#comment");
                        emitter.writeStringValue(item.getStringValue());
                        emitter.endMap();
                        break ;
                      case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                        emitter.startMap(true);
                        emitter.writeKey("#processing-instruction");
                        emitter.startMap(true);
                        emitter.writeKey("#name");
                        emitter.writeStringValue(((Saxon.Hej.om.NodeInfo)(item)).getDisplayName());
                        emitter.writeKey("#data");
                        emitter.writeStringValue(item.getStringValue());
                        emitter.endMap();
                        emitter.endMap();
                        break ;
                      case Saxon.Hej.type.Type.TEXT:
                        emitter.startMap(true);
                        emitter.writeKey("#text");
                        emitter.writeStringValue(item.getStringValue());
                        emitter.endMap();
                        break ;
                      case Saxon.Hej.type.Type.ELEMENT:
                        emitElement((Saxon.Hej.om.NodeInfo)(item), jcc);
                        break ;
                      case Saxon.Hej.type.Type.NAMESPACE:
                        emitter.startMap(true);
                        emitter.writeKey("#namespace");
                        emitter.startMap(true);
                        emitter.writeKey("#prefix");
                        emitter.writeStringValue(((Saxon.Hej.om.NodeInfo)(item)).getDisplayName());
                        emitter.writeKey("#uri");
                        emitter.writeStringValue(item.getStringValue());
                        emitter.endMap();
                        emitter.endMap();
                        break ;
                      default:
                        throw new Saxon.Ejava.lang.IllegalStateException();
                    }
                } else {
                    if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                        emitter.startArray(true);
                        foreach (Saxon.Hej.om.GroundedValue member in ((Saxon.Hej.ma.arrays.ArrayItem)(item)).members()) {
                            emitSequence(member, jcc, false);
                        }
                        emitter.endArray();
                    } else {
                        if ((item is Saxon.Hej.ma.map.MapItem)) {
                            emitter.startMap(true);
                            foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((Saxon.Hej.ma.map.MapItem)(item)).keyValuePairs()) {
                                emitter.writeKey(pair.key.getStringValue());
                                emitSequence(pair.value, jcc, false);
                            }
                            emitter.endMap();
                        } else {
                            if ((item is Saxon.Hej.om.FunctionItem)) {
                                emitter.startMap(true);
                                emitter.writeKey("#function");
                                emitter.startMap(true);
                                Saxon.Hej.om.StructuredQName name = ((Saxon.Hej.om.FunctionItem)(item)).getFunctionName();
                                if (((name != null) && !(name.hasURI(Saxon.Hej.om.NamespaceUri.ANONYMOUS)))) {
                                    emitter.writeKey("#name");
                                    emitter.writeStringValue(name.getEQName());
                                }
                                emitter.writeKey("#arity");
                                emitter.writeStringValue((((Saxon.Hej.om.FunctionItem)(item)).getArity()+""));
                                emitter.endMap();
                                emitter.endMap();
                            }
                        }
                    }
                }
            }
        }
        private void emitElement(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            Saxon.Hej.om.StructuredQName name = Saxon.Ejava.util.Objects.requireNonNull(Saxon.Hej.functions.NodeName_1.nodeName(element)).getStructuredQName();
            string tag;
            if (name.hasURI(jcc.elementNameStack.Peek().getNamespaceUri())) {
                tag = name.getLocalPart();
            } else {
                tag = name.getEQName();
            }
            jcc.elementNameStack.Push(name);
            jcc.emitter.startMap(true);
            jcc.emitter.writeKey(tag);
            emitElementContent(element, jcc);
            jcc.emitter.endMap();
            jcc.elementNameStack.Pop();
        }
        private void emitElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            string explicitLayout = jcc.layoutMap.get(element.getFingerprint());
            if ((explicitLayout != null)) {
                switch (explicitLayout) {
                  case "empty":
                    emitEmptyElementContent(element, jcc);
                    break ;
                  case "empty-plus":
                    emitEmptyPlusElementContent(element, jcc);
                    break ;
                  case "simple":
                    emitSimpleElementContent(element, jcc);
                    break ;
                  case "simple-plus":
                    emitSimplePlusElementContent(element, jcc);
                    break ;
                  case "list":
                    if (isSuitableAsList(element)) {
                        emitListElementContent(element, jcc);
                    } else {
                        emitMixedElementContent(element, jcc, false);
                    }
                    break ;
                  case "list-plus":
                    if (isSuitableAsList(element)) {
                        emitListPlusElementContent(element, jcc, -1);
                    } else {
                        emitMixedElementContent(element, jcc, false);
                    }
                    break ;
                  case "record":
                    emitRecordElementContent(element, jcc);
                    break ;
                  case "sequence":
                    emitMixedElementContent(element, jcc, false);
                    break ;
                  case "mixed":
                    emitMixedElementContent(element, jcc, false);
                    break ;
                }
            } else {
                Saxon.Hej.type.SchemaType type = element.getSchemaType();
                if (!((((type == Saxon.Hej.type.AnyType.getInstance()) || (type == Saxon.Hej.type.Untyped.getInstance()))))) {
                    if ((type is Saxon.Hej.type.SimpleType)) {
                        emitSimpleElementContent(element, jcc);
                    } else {
                        Saxon.Hej.type.ComplexType cType = (Saxon.Hej.type.ComplexType)(type);
                        string layout = cType.getPreferredJsonLayout();
                        string suffix = "";
                        int sep = layout.IndexOf("/");
                        if ((sep>0)) {
                            suffix = layout.Substring((sep+1));
                            layout = layout.Substring(0, sep - 0);
                        }
                        switch (layout) {
                          case "simple-plus":
                            emitSimplePlusElementContent(element, jcc);
                            break ;
                          case "empty":
                            emitEmptyElementContent(element, jcc);
                            break ;
                          case "empty-plus":
                            emitEmptyPlusElementContent(element, jcc);
                            break ;
                          case "list":
                            emitListElementContent(element, jcc);
                            break ;
                          case "list-plus":
                            emitListPlusElementContent(element, jcc, Saxon.Impl.Helpers.Utils.ParseInt(suffix));
                            break ;
                          case "record":
                            emitRecordElementContent(element, jcc);
                            break ;
                          case "sequence":
                            emitSequenceElementContent(element, jcc);
                            break ;
                          case "mixed":
                            emitMixedElementContent(element, jcc, false);
                            break ;
                          default:
                            throw new Saxon.Ejava.lang.IllegalStateException();
                        }
                    }
                } else {
                    int properties;
                    if ((jcc.uniformityData != null)) {
                        System.Diagnostics.Debug.Assert(jcc.uniformityData.contains(element.getFingerprint()));
                        properties = jcc.uniformityData.get(element.getFingerprint());
                    } else {
                        properties = getElementProperties(element, 0);
                    }
                    if ((((properties&HAS_NON_WHITESPACE_TEXT_NODES)) == 0)) {
                        if ((((properties&HAS_CHILD_ELEMENTS)) == 0)) {
                            if ((((properties&HAS_WHITESPACE_TEXT_NODES)) == 0)) {
                                if ((((properties&HAS_ATTRIBUTES)) == 0)) {
                                    emitEmptyElementContent(element, jcc);
                                } else {
                                    emitEmptyPlusElementContent(element, jcc);
                                }
                            } else {
                                if ((((properties&HAS_ATTRIBUTES)) == 0)) {
                                    emitSimpleElementContent(element, jcc);
                                } else {
                                    emitSimplePlusElementContent(element, jcc);
                                }
                            }
                        } else {
                            if (((((properties&HAS_WHITESPACE_TEXT_NODES)) != 0) && (((properties&HAS_CHILD_ELEMENTS)) == 0))) {
                                if ((((properties&HAS_ATTRIBUTES)) == 0)) {
                                    emitSimpleElementContent(element, jcc);
                                } else {
                                    emitSimplePlusElementContent(element, jcc);
                                }
                            } else {
                                if (((((properties&HAS_DIVERSE_CHILD_ELEMENTS)) == 0) && ((((properties&HAS_REPEATED_CHILD_ELEMENTS)) != 0)))) {
                                    if ((((properties&HAS_ATTRIBUTES)) == 0)) {
                                        emitListElementContent(element, jcc);
                                    } else {
                                        emitListPlusElementContent(element, jcc, (properties&Saxon.Hej.om.NamePool.FP_MASK));
                                    }
                                } else {
                                    if ((((properties&HAS_UNIQUE_CHILD_ELEMENTS)) != 0)) {
                                        emitRecordElementContent(element, jcc);
                                    } else {
                                        emitSequenceElementContent(element, jcc);
                                    }
                                }
                            }
                        }
                    } else {
                        if ((((properties&HAS_CHILD_ELEMENTS)) == 0)) {
                            if ((((properties&HAS_ATTRIBUTES)) == 0)) {
                                emitSimpleElementContent(element, jcc);
                            } else {
                                emitSimplePlusElementContent(element, jcc);
                            }
                        } else {
                            emitMixedElementContent(element, jcc, false);
                        }
                    }
                }
            }
        }
        private bool isSuitableAsList(Saxon.Hej.om.NodeInfo element) {
            int props = getElementProperties(element, 0);
            return ((((((props&HAS_DIVERSE_CHILD_ELEMENTS)) == 0) && (((props&HAS_CHILD_ELEMENTS)) != 0)) && (((props&HAS_NON_WHITESPACE_TEXT_NODES)) == 0)) && (((props&Saxon.Hej.om.NamePool.FP_MASK)) != 0));
        }
        private void emitEmptyElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            jcc.emitter.writeStringValue("");
        }
        private void emitEmptyPlusElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            jcc.emitter.startMap(false);
            emitAttributes(element, jcc, false);
            jcc.emitter.endMap();
        }
        private void emitSimpleElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            Saxon.Hej.type.SchemaType type = element.getSchemaType();
            bool asList = ((type is Saxon.Hej.type.SimpleType) && ((Saxon.Hej.type.SimpleType)(type)).isListType());
            emitSequence(element.atomize(), jcc, asList);
        }
        private void emitSimplePlusElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            jcc.emitter.startMap(false);
            emitAttributes(element, jcc, false);
            jcc.emitter.writeKey("#content");
            Saxon.Hej.type.SchemaType type = element.getSchemaType();
            bool asList = ((type is Saxon.Hej.type.SimpleType) && ((Saxon.Hej.type.SimpleType)(type)).isListType());
            emitSequence(element.atomize(), jcc, asList);
            jcc.emitter.endMap();
        }
        private void emitListElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            jcc.emitter.startArray(false);
            foreach (Saxon.Hej.om.NodeInfo child in element.children(Saxon.Hej.pattern.NodeKindTest.ELEMENT)) {
                emitElementContent(child, jcc);
            }
            jcc.emitter.endArray();
        }
        private void emitListPlusElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc, int childFingerprint) {
            string key;
            if ((childFingerprint == -1)) {
                Saxon.Hej.om.NodeInfo firstChild = ((/*1430*/Saxon.Hej.om.NodeInfo)element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
                System.Diagnostics.Debug.Assert((firstChild != null));
                if (firstChild.getNamespaceUri().Equals(element.getNamespaceUri())) {
                    key = firstChild.getLocalPart();
                } else {
                    key = ((("Q{"+firstChild.getNamespaceUri())+"}")+firstChild.getLocalPart());
                }
            } else {
                Saxon.Hej.om.StructuredQName qn = jcc.config.getNamePool().getStructuredQName(childFingerprint);
                if (qn.getNamespaceUri().Equals(element.getNamespaceUri())) {
                    key = qn.getLocalPart();
                } else {
                    key = qn.getEQName();
                }
            }
            Saxon.Hej.serialize.JSONEmitter emitter = jcc.emitter;
            emitter.startMap(false);
            emitAttributes(element, jcc, false);
            emitter.writeKey(key);
            emitter.startArray(false);
            foreach (Saxon.Hej.om.NodeInfo child in element.children()) {
                if ((child.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                    emitElementContent(child, jcc);
                }
            }
            emitter.endArray();
            emitter.endMap();
        }
        private void emitRecordElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            Saxon.Hej.serialize.JSONEmitter emitter = jcc.emitter;
            emitter.startMap(false);
            emitAttributes(element, jcc, false);
            if (element.isNilled()) {
                emitter.writeKey("#content");
                emitter.writeAtomicValue(null);
            } else {
                bool containsDuplicates = false;
                Saxon.Hej.z.IntToIntHashMap counters = new Saxon.Hej.z.IntToIntHashMap();
                foreach (Saxon.Hej.om.NodeInfo child in element.children()) {
                    if ((child.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                        int fp = child.getFingerprint();
                        if (counters.contains(fp)) {
                            containsDuplicates = true;
                            counters.put(fp, (counters.get(fp)+1));
                        } else {
                            counters.put(fp, 1);
                        }
                    }
                }
                Saxon.Hej.z.IntToIntHashMap indexes = null;
                if (containsDuplicates) {
                    indexes = new Saxon.Hej.z.IntToIntHashMap();
                }
                foreach (Saxon.Hej.om.NodeInfo child in element.children()) {
                    if ((child.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                        string key = (child.getNamespaceUri().Equals(element.getNamespaceUri()) ? child.getLocalPart() : ((("Q{"+child.getNamespaceUri())+"}")+child.getLocalPart()));
                        if (containsDuplicates) {
                            int fp = child.getFingerprint();
                            if ((counters.get(fp)>1)) {
                                int seq = (indexes.contains(fp) ? (indexes.get(fp)+1) : 1);
                                indexes.put(fp, seq);
                                key += ((("["+seq)+"]"));
                            }
                        }
                        emitter.writeKey(key);
                        emitElementContent(child, jcc);
                    }
                }
            }
            emitter.endMap();
        }
        private void emitSequenceElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc) {
            emitMixedElementContent(element, jcc, true);
        }
        private void emitMixedElementContent(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc, bool stripSpace) {
            Saxon.Hej.serialize.JSONEmitter emitter = jcc.emitter;
            emitter.startArray(false);
            emitAttributes(element, jcc, true);
            if (element.isNilled()) {
                emitter.writeAtomicValue(null);
            } else {
                foreach (Saxon.Hej.om.NodeInfo child in element.children()) {
                    switch (child.getNodeKind()) {
                      case Saxon.Hej.type.Type.TEXT:
                        if (!(((stripSpace && Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))))) {
                            emitter.writeStringValue(child.getStringValue());
                        }
                        break ;
                      case Saxon.Hej.type.Type.COMMENT:
                        emitter.startMap(true);
                        emitter.writeKey("#comment");
                        emitter.writeStringValue(child.getStringValue());
                        emitter.endMap();
                        break ;
                      case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                        emitter.startMap(true);
                        emitter.writeKey("#processing-instruction");
                        emitter.startMap(true);
                        emitter.writeKey("#name");
                        emitter.writeStringValue(child.getDisplayName());
                        emitter.writeKey("#data");
                        emitter.writeStringValue(child.getStringValue());
                        emitter.endMap();
                        emitter.endMap();
                        break ;
                      case Saxon.Hej.type.Type.ELEMENT:
                        emitElement(child, jcc);
                        break ;
                      default:
                        throw new Saxon.Ejava.lang.IllegalStateException();
                    }
                }
            }
            emitter.endArray();
        }
        private void emitAttributes(Saxon.Hej.om.NodeInfo element, Saxon.Eej.functions.qt4.XdmToJsonFn.JsonConversionContext jcc, bool individual) {
            Saxon.Hej.om.NodeInfo att;
            Saxon.Hej.om.SequenceIterator iter = element.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
            while (((att = (Saxon.Hej.om.NodeInfo)(iter.next())) != null)) {
                Saxon.Hej.om.NamespaceUri ns = att.getNamespaceUri();
                if (!(ns.Equals(Saxon.Hej.om.NamespaceUri.SCHEMA_INSTANCE))) {
                    if (individual) {
                        jcc.emitter.startMap(true);
                    }
                    string attKey;
                    if (ns.Equals(Saxon.Hej.om.NamespaceUri.NULL)) {
                        attKey = ("@"+att.getLocalPart());
                    } else {
                        if (ns.Equals(Saxon.Hej.om.NamespaceUri.XML)) {
                            attKey = ("@xml:"+att.getLocalPart());
                        } else {
                            attKey = ((("@Q{"+att.getNamespaceUri())+"}")+att.getLocalPart());
                        }
                    }
                    jcc.emitter.writeKey(attKey);
                    Saxon.Hej.om.AtomicSequence val = att.atomize();
                    Saxon.Hej.type.SchemaType type = att.getSchemaType();
                    bool asList = ((Saxon.Hej.type.SimpleType)(type)).isListType();
                    emitSequence(val, jcc, asList);
                    if (individual) {
                        jcc.emitter.endMap();
                    }
                }
            }
        }
    
    }
}