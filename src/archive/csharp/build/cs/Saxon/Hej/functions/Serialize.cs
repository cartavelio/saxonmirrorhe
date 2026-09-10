using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Serialize : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {

        static Serialize() {
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "allow-duplicate-names", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "byte-order-mark", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "cdata-section-elements", Saxon.Hej.type.BuiltInAtomicType.QNAME.zeroOrMore());
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "doctype-public", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "doctype-system", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "encoding", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "escape-uri-attributes", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "html-version", Saxon.Hej.value.SequenceType.SINGLE_DECIMAL);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "include-content-type", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "indent", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "item-separator", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "json-node-output-method", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "media-type", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "method", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "normalization-form", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "omit-xml-declaration", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "standalone", Saxon.Hej.value.SequenceType.OPTIONAL_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "suppress-indentation", Saxon.Hej.type.BuiltInAtomicType.QNAME.zeroOrMore());
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "undeclare-prefixes", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "use-character-maps", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypes, "version", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "attribute-order", Saxon.Hej.type.BuiltInAtomicType.QNAME.zeroOrMore());
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "canonical", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "character-representation", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "double-space", Saxon.Hej.type.BuiltInAtomicType.QNAME.zeroOrMore());
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "indent-spaces", Saxon.Hej.value.SequenceType.SINGLE_INTEGER);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "line-length", Saxon.Hej.value.SequenceType.SINGLE_INTEGER);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "newline", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "recognize-binary", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "require-well-formed", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "single-quotes", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "supply-source-locator", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Impl.Helpers.MapUtils.Put(requiredTypesSaxon, "suppress-indentation", Saxon.Hej.type.BuiltInAtomicType.QNAME.zeroOrMore());
        }
        public static Saxon.Hej.functions.OptionsParameter makeOptionsParameter() {
            Saxon.Hej.value.SequenceType listOfQNames = Saxon.Hej.type.BuiltInAtomicType.QNAME.zeroOrMore();
            Saxon.Hej.functions.OptionsParameter op = new Saxon.Hej.functions.OptionsParameter();
            op.addAllowedOption("allow-duplicate-names", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("byte-order-mark", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("cdata-section-elements", listOfQNames, Saxon.Hej.value.EmptySequence.getInstance());
            op.addAllowedOption("doctype-public", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("doctype-system", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("encoding", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("escape-uri-attributes", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("html-version", Saxon.Hej.value.SequenceType.SINGLE_DECIMAL);
            op.addAllowedOption("include-content-type", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("indent", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("item-separator", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("json-node-output-method", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("media-type", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("method", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("normalization-form", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("omit-xml-declaration", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("standalone", Saxon.Hej.value.SequenceType.OPTIONAL_BOOLEAN);
            op.addAllowedOption("suppress-indentation", listOfQNames);
            op.addAllowedOption("undeclare-prefixes", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("use-character-maps", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            op.addAllowedOption("version", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption(sx("attribute-order"), Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE);
            op.addAllowedOption(sx("canonical"), Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption(sx("character-representation"), Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption(sx("double-space"), listOfQNames);
            op.addAllowedOption(sx("indent-spaces"), Saxon.Hej.value.SequenceType.SINGLE_INTEGER);
            op.addAllowedOption(sx("line-length"), Saxon.Hej.value.SequenceType.SINGLE_INTEGER);
            op.addAllowedOption(sx("newline"), Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption(sx("property-order"), Saxon.Hej.value.SequenceType.STRING_SEQUENCE);
            op.addAllowedOption(sx("recognize-binary"), Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption(sx("require-well-formed"), Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption(sx("single-quotes"), Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption(sx("supply-source-locator"), Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            return op;
        }
        private static string sx(string s) {
            return ((("Q{"+Saxon.Hej.lib.NamespaceConstant.SAXON)+"}")+s);
        }
        private readonly string[] paramNames = new string[]{"allow-duplicate-names", "byte-order-mark", "cdata-section-elements", "doctype-public", "doctype-system", "encoding", "escape-uri-attributes", "html-version", "include-content-type", "indent", "item-separator", "json-node-output-method", "media-type", "method", "normalization-form", "omit-xml-declaration", "standalone", "suppress-indentation", "undeclare-prefixes", "use-character-maps", "version"};
        private bool isParamName(string name) {
            foreach (string s in paramNames) {
                if (s.Equals(name)) {
                    return true;
                }
            }
            return false;
        }
        private readonly string[] paramNamesSaxon = new string[]{"attribute-order", "canonical", "character-representation", "double-space", "indent-spaces", "line-length", "newline", "property-order", "recognize-binary", "require-well-formed", "single-quotes", "supply-source-locator", "suppress-indentation"};
        private bool isParamNameSaxon(string name) {
            foreach (string s in paramNamesSaxon) {
                if (s.Equals(name)) {
                    return true;
                }
            }
            return false;
        }
        private readonly static System.Collections.Generic.IDictionary<string,Saxon.Hej.value.SequenceType> requiredTypes = new System.Collections.Generic.Dictionary<string, Saxon.Hej.value.SequenceType>(40);
        private readonly static System.Collections.Generic.IDictionary<string,Saxon.Hej.value.SequenceType> requiredTypesSaxon = new System.Collections.Generic.Dictionary<string, Saxon.Hej.value.SequenceType>(20);
        private Saxon.Hej.ma.map.MapItem checkOptions(Saxon.Hej.ma.map.MapItem map, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.ma.map.MapItem result = new Saxon.Impl.DataModel.ImmMap();
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            Saxon.Hej.tree.iter.AtomicIterator keysIterator = map.keys();
            Saxon.Hej.value.AtomicValue key;
            while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keysIterator.next())) != null)) {
                if ((key is Saxon.Hej.value.StringValue)) {
                    string keyName = key.getStringValue();
                    if (isParamName(keyName)) {
                        Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.OPTION, keyName, 0, "XPTY0004");
                        Saxon.Hej.om.Sequence converted = th.applyFunctionConversionRules(map.get(key), Saxon.Impl.Helpers.MapUtils.Get(requiredTypes, keyName), role, Saxon.Hej.expr.parser.Loc.NONE);
                        result = result.addEntry(key, converted.materialize());
                    }
                } else {
                    if ((key is Saxon.Hej.value.QNameValue)) {
                        if (key.getComponent(Saxon.Hej.functions.AccessorFn.Component.NAMESPACE).getUnicodeStringValue().isEmpty()) {
                            throw new Saxon.Hej.trans.XPathException("A serialization parameter supplied with a QName key must have non-absent namespace", "SEPM0017");
                        } else {
                            if (key.getComponent(Saxon.Hej.functions.AccessorFn.Component.NAMESPACE).getUnicodeStringValue().Equals(Saxon.Hej.str.StringView.of("http://saxon.sf.net/"))) {
                                string keyName = ((Saxon.Hej.value.QNameValue)(key)).getLocalName();
                                if (isParamNameSaxon(keyName)) {
                                    Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.OPTION, keyName, 0);
                                    Saxon.Hej.om.Sequence converted = th.applyFunctionConversionRules(map.get(key), Saxon.Impl.Helpers.MapUtils.Get(requiredTypesSaxon, keyName), role, Saxon.Hej.expr.parser.Loc.NONE);
                                    result = result.addEntry(key, converted.materialize());
                                }
                            }
                        }
                    } else {
                        break ;
                    }
                }
            }
            return result;
        }
        private string toYesNoTypeString(Saxon.Hej.om.Sequence seqVal) {
            string s;
            bool booleanValue = ((Saxon.Hej.value.BooleanValue)(seqVal.head())).getBooleanValue();
            if (booleanValue) {
                s = "yes";
            } else {
                s = "no";
            }
            return s;
        }
        private string toYesNoOmitTypeString(Saxon.Hej.om.Sequence seqVal) {
            string stringVal = "";
            if ((seqVal is Saxon.Hej.value.EmptySequence)) {
                stringVal = "omit";
            } else {
                if ((seqVal.head() is Saxon.Hej.value.BooleanValue)) {
                    stringVal = toYesNoTypeString(seqVal);
                }
            }
            return stringVal;
        }
        private string toQNamesTypeString(Saxon.Hej.om.Sequence seqVal, bool allowStar) {
            Saxon.Hej.om.SequenceIterator iterator = seqVal.iterate();
            Saxon.Hej.om.Item item;
            Saxon.Ejava.lang.StringBuilder stringVal = new Saxon.Ejava.lang.StringBuilder();
            while (((item = iterator.next()) != null)) {
                if ((item is Saxon.Hej.value.QNameValue)) {
                    Saxon.Hej.value.QNameValue qNameValue = (Saxon.Hej.value.QNameValue)(item);
                    stringVal.append(" Q{").append(qNameValue.getComponent(Saxon.Hej.functions.AccessorFn.Component.NAMESPACE).getUnicodeStringValue()).append('}').append(qNameValue.getComponent(Saxon.Hej.functions.AccessorFn.Component.LOCALNAME).getUnicodeStringValue());
                } else {
                    if (((allowStar && (item is Saxon.Hej.value.StringValue)) && item.getStringValue().Equals("*"))) {
                        stringVal.append(" *");
                    } else {
                        throw new Saxon.Hej.trans.XPathException(("Invalid serialization parameter value: expected sequence of QNames "+((allowStar ? "(or *) " : ""))), "SEPM0017");
                    }
                }
            }
            return stringVal.ToString();
        }
        private string toSpaceSeparatedString(Saxon.Hej.om.Sequence seqVal) {
            Saxon.Hej.om.SequenceIterator iterator = seqVal.iterate();
            Saxon.Hej.om.Item item;
            Saxon.Ejava.lang.StringBuilder stringVal = new Saxon.Ejava.lang.StringBuilder();
            while (((item = iterator.next()) != null)) {
                stringVal.append(" ").append(item.getUnicodeStringValue());
            }
            return stringVal.ToString();
        }
        private string toMethodTypeString(Saxon.Hej.om.Sequence seqVal) {
            string stringVal;
            if ((seqVal.head() is Saxon.Hej.value.QNameValue)) {
                Saxon.Hej.value.QNameValue qNameValue = (Saxon.Hej.value.QNameValue)(seqVal.head());
                stringVal = ((('{'+qNameValue.getComponent(Saxon.Hej.functions.AccessorFn.Component.NAMESPACE).ToString())+'}')+qNameValue.getComponent(Saxon.Hej.functions.AccessorFn.Component.LOCALNAME));
            } else {
                stringVal = seqVal.head().getStringValue();
            }
            return stringVal;
        }
        private static Saxon.Hej.ma.map.MapItem checkCharacterMapOptions(Saxon.Hej.ma.map.MapItem map, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            foreach (Saxon.Hej.ma.map.KeyValuePair pair in map.keyValuePairs()) {
                Saxon.Hej.value.AtomicValue key = pair.key;
                if (!(((key is Saxon.Hej.value.StringValue)))) {
                    throw new Saxon.Hej.trans.XPathException(("Keys in a character map must all be strings. Found a value of type "+key.getItemType()), "XPTY0004");
                }
                if ((((Saxon.Hej.value.StringValue)(key)).length() != 1)) {
                    throw new Saxon.Hej.trans.XPathException(("Keys in a character map must all be one-character strings. Found "+Saxon.Hej.trans.Err.wrap(key.show())), "SEPM0016");
                }
                if (!(Saxon.Hej.value.SequenceType.SINGLE_STRING.matches(pair.value, th))) {
                    throw new Saxon.Hej.trans.XPathException(("Values in a character map must all be single strings. Found "+Saxon.Hej.trans.Err.wrap(key.show())), "XPTY0004");
                }
            }
            return map;
        }
        private Saxon.Hej.serialize.CharacterMap toCharacterMap(Saxon.Hej.om.Sequence seqVal, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.ma.map.MapItem charMap = checkCharacterMapOptions((Saxon.Hej.ma.map.MapItem)(seqVal.head()), context);
            return toCharacterMap(charMap);
        }
        public static Saxon.Hej.serialize.CharacterMap toCharacterMap(Saxon.Hej.ma.map.MapItem charMap) {
            Saxon.Hej.tree.iter.AtomicIterator iterator = charMap.keys();
            Saxon.Hej.value.AtomicValue charKey;
            Saxon.Hej.z.IntHashMap<string> intHashMap = new Saxon.Hej.z.IntHashMap<string>();
            while (((charKey = ((/*1430*/Saxon.Hej.value.AtomicValue)iterator.next())) != null)) {
                Saxon.Hej.str.UnicodeString ch = charKey.getUnicodeStringValue();
                string str = charMap.get(charKey).head().getStringValue();
                if ((ch.length() != 1)) {
                    throw new Saxon.Hej.trans.XPathException(("In the serialization parameter for the character map, each character to be mapped "+"must be a single Unicode character"), "SEPM0016");
                }
                int code = ch.codePointAt(0);
                string prev = intHashMap.put(code, str);
                if ((prev != null)) {
                    throw new Saxon.Hej.trans.XPathException(("In the serialization parameters, the character map contains two entries for the character \\u"+Saxon.Impl.Helpers.Utils.intToHexString((65536+code)).Substring(1)), "SEPM0018");
                }
            }
            Saxon.Hej.om.StructuredQName name = new Saxon.Hej.om.StructuredQName("output", Saxon.Hej.om.NamespaceUri.OUTPUT, "serialization-parameters");
            return new Saxon.Hej.serialize.CharacterMap(name, intHashMap);
        }
        private Saxon.Hej.serialize.SerializationProperties serializationParamsFromMap(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> map, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Sequence seqVal;
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            Saxon.Hej.serialize.CharacterMapIndex charMapIndex = new Saxon.Hej.serialize.CharacterMapIndex();
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "allow-duplicate-names")) != null)) {
                props.setProperty("allow-duplicate-names", toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "byte-order-mark")) != null)) {
                props.setProperty("byte-order-mark", toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "cdata-section-elements")) != null)) {
                props.setProperty("cdata-section-elements", toQNamesTypeString(seqVal, false));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "doctype-public")) != null)) {
                props.setProperty("doctype-public", seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "doctype-system")) != null)) {
                props.setProperty("doctype-system", seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "encoding")) != null)) {
                props.setProperty("encoding", seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "escape-uri-attributes")) != null)) {
                props.setProperty("escape-uri-attributes", toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "html-version")) != null)) {
                props.setProperty("html-version", seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "include-content-type")) != null)) {
                props.setProperty("include-content-type", toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "indent")) != null)) {
                props.setProperty("indent", toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "item-separator")) != null)) {
                props.setProperty("item-separator", seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "json-node-output-method")) != null)) {
                props.setProperty("json-node-output-method", toMethodTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "media-type")) != null)) {
                props.setProperty("media-type", seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "method")) != null)) {
                props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, toMethodTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "normalization-form")) != null)) {
                props.setProperty("normalization-form", seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "omit-xml-declaration")) != null)) {
                props.setProperty("omit-xml-declaration", toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "standalone")) != null)) {
                props.setProperty("standalone", toYesNoOmitTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "suppress-indentation")) != null)) {
                props.setProperty("suppress-indentation", toQNamesTypeString(seqVal, false));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "undeclare-prefixes")) != null)) {
                props.setProperty("undeclare-prefixes", toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "use-character-maps")) != null)) {
                Saxon.Hej.serialize.CharacterMap characterMap = toCharacterMap(seqVal, context);
                charMapIndex.putCharacterMap(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "charMap"), characterMap);
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS, "charMap");
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, "version")) != null)) {
                props.setProperty("version", seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("attribute-order"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER, toQNamesTypeString(seqVal, true));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("canonical"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.CANONICAL, toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("character-representation"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.CHARACTER_REPRESENTATION, seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("double-space"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.DOUBLE_SPACE, toQNamesTypeString(seqVal, false));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("indent-spaces"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.INDENT_SPACES, seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("internal-dtd-subset"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.INTERNAL_DTD_SUBSET, seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("line-length"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.LINE_LENGTH, seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("newline"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.NEWLINE, seqVal.head().getStringValue());
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("property-order"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.PROPERTY_ORDER, toSpaceSeparatedString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("recognize-binary"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.RECOGNIZE_BINARY, toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("require-well-formed"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.REQUIRE_WELL_FORMED, toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("single-quotes"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.SINGLE_QUOTES, toYesNoTypeString(seqVal));
            }
            if (((seqVal = Saxon.Impl.Helpers.MapUtils.Get(map, sx("supply-source-locator"))) != null)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.SUPPLY_SOURCE_LOCATOR, toYesNoTypeString(seqVal));
            }
            return new Saxon.Hej.serialize.SerializationProperties(props, charMapIndex);
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return evalSerialize(arguments[0].iterate(), ((arguments.Length == 1) ? null : arguments[1].head()), context);
        }
        private Saxon.Hej.value.StringValue evalSerialize(Saxon.Hej.om.SequenceIterator iter, Saxon.Hej.om.Item param, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.serialize.SerializationProperties @params;
            if ((param != null)) {
                if ((param is Saxon.Hej.om.NodeInfo)) {
                    Saxon.Hej.om.NodeInfo paramNode = (Saxon.Hej.om.NodeInfo)(param);
                    if ((((paramNode.getNodeKind() != Saxon.Hej.type.Type.ELEMENT) || !(Saxon.Hej.om.NamespaceUri.OUTPUT.Equals(paramNode.getNamespaceUri()))) || !("serialization-parameters".Equals(paramNode.getLocalPart())))) {
                        throw new Saxon.Hej.trans.XPathException((("Second argument to fn:serialize() must be an element named {"+Saxon.Hej.lib.NamespaceConstant.OUTPUT)+"}serialization-parameters"), "XPTY0004");
                    }
                    Saxon.Hej.serialize.SerializationParamsHandler sph = new Saxon.Hej.serialize.SerializationParamsHandler();
                    sph.setSerializationParams(paramNode);
                    @params = sph.getSerializationProperties();
                } else {
                    if ((param is Saxon.Hej.ma.map.MapItem)) {
                        Saxon.Hej.ma.map.MapItem paramMap = (Saxon.Hej.ma.map.MapItem)(param);
                        Saxon.Hej.tree.iter.AtomicIterator keyIter = ((Saxon.Hej.ma.map.MapItem)(param)).keys();
                        Saxon.Hej.value.AtomicValue k;
                        while (((k = ((/*1430*/Saxon.Hej.value.AtomicValue)keyIter.next())) != null)) {
                            if ((k is Saxon.Hej.value.QNameValue)) {
                                string s = ((Saxon.Hej.value.QNameValue)(k)).getStructuredQName().getEQName();
                                paramMap = paramMap.addEntry(new Saxon.Hej.value.StringValue(s), paramMap.get(k));
                            }
                        }
                        System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> checkedOptions = getDetails().optionDetails.processSuppliedOptions(paramMap, context);
                        @params = serializationParamsFromMap(checkedOptions, context);
                    } else {
                        throw new Saxon.Hej.trans.XPathException((("Second argument to fn:serialize() must either be an element named {"+Saxon.Hej.lib.NamespaceConstant.OUTPUT)+"}serialization-parameters, or a map (if using XPath 3.1)"), "XPTY0004");
                    }
                }
            } else {
                @params = new Saxon.Hej.serialize.SerializationProperties(new Saxon.Ejava.util.Properties());
            }
            Saxon.Ejava.util.Properties props = @params.getProperties();
            if ((props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD) == null)) {
                props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, "xml");
            }
            if ((props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION) == null)) {
                props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "true");
            }
            try {
                Saxon.Hej.str.UnicodeBuilder builder = new Saxon.Hej.str.UnicodeBuilder();
                Saxon.Hej.serialize.UnicodeWriterResult result = new Saxon.Hej.serialize.UnicodeWriterResult(builder, null);
                Saxon.Hej.lib.SerializerFactory sf = context.getConfiguration().getSerializerFactory();
                Saxon.Hej.@event.PipelineConfiguration pipe = context.getConfiguration().makePipelineConfiguration();
                Saxon.Hej.@event.Receiver @out = sf.getReceiver(result, @params, pipe);
                Saxon.Hej.@event.SequenceCopier.copySequence(iter, @out);
                return new Saxon.Hej.value.StringValue(builder.ToString());
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetErrorCode("SENR0001");
                throw;
            }
        }
    
    }
}