////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceCopier;
import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.SaxonOutputKeys;
import net.sf.saxon.lib.SerializerFactory;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.*;
import net.sf.saxon.serialize.*;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntHashMap;

import javax.xml.transform.OutputKeys;
import java.util.Properties;

/**
 * Implementation of fn:serialize() as defined in XPath 3.1/4.0
 */

public class Serialize extends SystemFunction implements Callable {
    
    public static OptionsParameter makeOptionsParameter(int version) {
        SequenceType listOfQNames = BuiltInAtomicType.QNAME.zeroOrMore();
        OptionsParameter op = new OptionsParameter(version);
        op.addAllowedOption("allow-duplicate-names", SequenceType.SINGLE_BOOLEAN); //yes-no-param-type
        op.addAllowedOption("byte-order-mark", SequenceType.SINGLE_BOOLEAN); //yes-no-param-type
        op.addAllowedOption("cdata-section-elements", listOfQNames, EmptySequence.INSTANCE);
        //QNames-param-type - sequence or an array of xs:QName values. Note that an array will be converted to a sequence, as required
        op.addAllowedOption("doctype-public", SequenceType.SINGLE_STRING); //doctype-public-param-type pubid-char-string-type
        op.addAllowedOption("doctype-system", SequenceType.SINGLE_STRING); //doctype-system-param-type system-id-string-type
        op.addAllowedOption("encoding", SequenceType.SINGLE_STRING); //encoding-param-type encoding-string-type
        op.addAllowedOption("escape-uri-attributes", SequenceType.SINGLE_BOOLEAN); //yes-no-param-type
        op.addAllowedOption("html-version", SequenceType.SINGLE_DECIMAL); //decimal-param-type
        op.addAllowedOption("include-content-type", SequenceType.SINGLE_BOOLEAN); //yes-no-param-type
        op.addAllowedOption("indent", SequenceType.SINGLE_BOOLEAN);//yes-no-param-type
        op.addAllowedOption("item-separator", SequenceType.SINGLE_STRING); //string-param-type
        op.addAllowedOption("json-node-output-method", SequenceType.SINGLE_STRING);
        //json-node-output-method-param-type  json-node-output-method-type - xs:string or xs:QName
        op.addAllowedOption("media-type", SequenceType.SINGLE_STRING); //string-param-type
        op.addAllowedOption("method", SequenceType.SINGLE_STRING);
        //method-param-type method-type - xs:string or xs:QName            
        op.addAllowedOption("normalization-form", SequenceType.SINGLE_STRING);
        //NMTOKEN-param-type  BuiltInAtomicType.NMTOKEN
        op.addAllowedOption("omit-xml-declaration", SequenceType.SINGLE_BOOLEAN); //yes-no-param-type
        op.addAllowedOption("standalone", SequenceType.OPTIONAL_BOOLEAN); //yes-no-omit-type
        op.addAllowedOption("suppress-indentation", listOfQNames);
        //QNames-param-type - sequence or an array of xs:QName values. Note that an array will be converted to a sequence, as required
        op.addAllowedOption("undeclare-prefixes", SequenceType.SINGLE_BOOLEAN); //yes-no-param-type
        op.addAllowedOption("use-character-maps", SequenceType.one(new MapType(BuiltInAtomicType.STRING, SequenceType.SINGLE_STRING)));
        //use-character-maps-param-type
        op.addAllowedOption("version", SequenceType.SINGLE_STRING);

        op.addAllowedOption(sx("attribute-order"), SequenceType.ATOMIC_SEQUENCE);
        op.addAllowedOption(sx("canonical"), SequenceType.SINGLE_BOOLEAN);
        //eqnames
        op.addAllowedOption(sx("character-representation"), SequenceType.SINGLE_STRING); //string
        op.addAllowedOption(sx("conditional-cdata"), SequenceType.SINGLE_BOOLEAN); //boolean
        op.addAllowedOption(sx("double-space"), listOfQNames);
        //eqnames
        op.addAllowedOption(sx("indent-spaces"), SequenceType.SINGLE_INTEGER); //integer
        op.addAllowedOption(sx("internal-dtd-subset"), SequenceType.SINGLE_STRING); //string
        op.addAllowedOption(sx("line-length"), SequenceType.SINGLE_INTEGER); //integer
        op.addAllowedOption(sx("newline"), SequenceType.SINGLE_STRING); //integer
        //requiredTypes.put("next-in-chain", SequenceType.SINGLE_STRING); //uri
        op.addAllowedOption(sx("property-order"), SequenceType.STRING_SEQUENCE);
        op.addAllowedOption(sx("recognize-binary"), SequenceType.SINGLE_BOOLEAN); //boolean
        op.addAllowedOption(sx("require-well-formed"), SequenceType.SINGLE_BOOLEAN); //boolean
        op.addAllowedOption(sx("single-quotes"), SequenceType.SINGLE_BOOLEAN); //boolean
        op.addAllowedOption(sx("spec-version"), SequenceType.SINGLE_STRING);
        op.addAllowedOption(sx("supply-source-locator"), SequenceType.SINGLE_BOOLEAN); //boolean
        op.addAllowedOption(sx("suppress-indentation"), listOfQNames); //boolean

        if (version >= 40) {
            op.addAllowedOption("canonical", SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("csv-header", SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("csv-quote-character", SequenceType.SINGLE_STRING);
            op.addAllowedOption("csv-separator", SequenceType.SINGLE_STRING);
            op.addAllowedOption("escape-solidus", SequenceType.SINGLE_BOOLEAN); //yes-no-param-type
            op.addAllowedOption("indent-attributes", SequenceType.SINGLE_BOOLEAN);//yes-no-param-type
            op.addAllowedOption("indent-unit", SequenceType.SINGLE_STRING);
            op.addAllowedOption("json-lines", SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("line-ending", SequenceType.SINGLE_STRING);
        }
        return op;
    }
    
    private static String sx(String s) {
        return "Q{" + NamespaceConstant.SAXON + "}" + s;
    }


    // Convert a boolean value to a yes-no-type string.

    private static String toYesNoTypeString(Sequence seqVal) throws XPathException {
        String s;
        boolean booleanValue = ((BooleanValue) seqVal.head()).getBooleanValue();
        if (booleanValue) {
            s = "yes";
        } else {
            s = "no";
        }
        return s;
    }

    // Convert a value to a yes-no-omit-type string.

    private static String toYesNoOmitTypeString(Sequence seqVal) throws XPathException {
        String stringVal = "";
        if (seqVal instanceof EmptySequence) {
            stringVal = "omit";
        } else if (seqVal.head() instanceof BooleanValue) {
            stringVal = toYesNoTypeString(seqVal);
        }
        // otherwise invalid
        return stringVal;
    }

    // Convert a sequence of QNames to a qnames-type string (containing a space-separated list of the QNames).

    private static String toQNamesTypeString(Sequence seqVal, boolean allowStar) throws XPathException {
        SequenceIterator iterator = seqVal.iterate();
        Item item;
        StringBuilder stringVal = new StringBuilder();
        while ((item = iterator.next()) != null) {
            if (item instanceof QNameValue qNameValue) {
                stringVal.append(" Q{")
                        .append(qNameValue.getComponent(AccessorFn.Component.NAMESPACE).getUnicodeStringValue())
                        .append('}')
                        .append(qNameValue.getComponent(AccessorFn.Component.LOCALNAME).getUnicodeStringValue());
            } else if (allowStar && item instanceof StringValue && item.getStringValue().equals("*")) {
                stringVal.append(" *");
            } else {
                throw new XPathException("Invalid serialization parameter value: expected sequence of QNames "
                + (allowStar ? "(or *) " : ""), "SEPM0017");
            }

        }
        return stringVal.toString();
    }

    // Convert a sequence of strings to a single space-separated string.

    private static String toSpaceSeparatedString(Sequence seqVal) throws XPathException {
        SequenceIterator iterator = seqVal.iterate();
        Item item;
        StringBuilder stringVal = new StringBuilder();
        while ((item = iterator.next()) != null) {
            stringVal.append(" ").append(item.getUnicodeStringValue());
        }
        return stringVal.toString();
    }

    // Convert a QName or string value to a method-type (or json-node-output-method-type) string.

    private static String toMethodTypeString(Sequence seqVal) throws XPathException {
        String stringVal;
        if (seqVal.head() instanceof QNameValue qNameValue) {
            stringVal = '{' + qNameValue.getComponent(AccessorFn.Component.NAMESPACE).toString() + '}' + qNameValue.getComponent(AccessorFn.Component.LOCALNAME);
        } else {
            stringVal = seqVal.head().getStringValue();
        }
        return stringVal;
    }


    /**
     * By the option parameter conventions, check the 'options' supplied in a character map:
     * 1. any string is allowed as an option, QNames not recognised by product are ignored;
     * 2. validate the types of the option values supplied (required type is always xs:string).
     */

    private static MapItem checkCharacterMapOptions(MapItem map, XPathContext context) throws XPathException {
        for (KeyValuePair pair : map.keyValuePairs()) {
            AtomicValue key = pair.key();
            if (!(key instanceof StringValue)) {
                throw new XPathException(
                        "Keys in a character map must all be strings. Found a value of type " + key.getItemType(), "XPTY0004");
            }
            if (((StringValue) key).length() != 1) {
                throw new XPathException("Keys in a character map must all be one-character strings. Found " + Err.wrap(key.show()), "SEPM0016");
            }
            if (!SequenceType.SINGLE_STRING.matches(pair.value())) {
                throw new XPathException("Values in a character map must all be single strings. Found " + Err.wrap(key.show()), "XPTY0004");
            }
        }
        return map;
    }

    // Convert a map defining a character map to a CharacterMap

    private static CharacterMap toCharacterMap(Sequence seqVal, XPathContext context) throws XPathException {
        MapItem charMap = checkCharacterMapOptions((MapItem) seqVal.head(), context);
        return toCharacterMap(charMap);
    }

    public static CharacterMap toCharacterMap(MapItem charMap) throws XPathException {
        IntHashMap<String> intHashMap = new IntHashMap<>();
        for (KeyValuePair kvp : charMap.keyValuePairs()) {
            AtomicValue charKey = kvp.key();
            UnicodeString ch = charKey.getUnicodeStringValue();
            String str = kvp.value().head().getStringValue();
            if (ch.length() != 1) {
                throw new XPathException("In the serialization parameter for the character map, each character to be mapped " +
                    "must be a single Unicode character", "SEPM0016");
            }
            int code = ch.codePointAt(0);
            String prev = intHashMap.put(code, str);
            if (prev != null) { // This should never happen in this case because keys in a HashTrieMap must be unique
                throw new XPathException("In the serialization parameters, the character map contains two entries for the character \\u" +
                    Integer.toHexString(65536 + code).substring(1), "SEPM0018");
            }
        }
        StructuredQName name = new StructuredQName("output", NamespaceUri.OUTPUT, "serialization-parameters");
        return new CharacterMap(name, intHashMap);
    }

    private static SerializationProperties serializationParamsFromMap(
            ProcessedOptions map, XPathContext context) throws XPathException {
        Sequence seqVal;
        Properties props = new Properties();
        CharacterMapIndex charMapIndex = new CharacterMapIndex();
        if ((seqVal = map.get("allow-duplicate-names")) != null) {
            props.setProperty("allow-duplicate-names", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("byte-order-mark")) != null) {
            props.setProperty("byte-order-mark", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("cdata-section-elements")) != null) {
            props.setProperty("cdata-section-elements", toQNamesTypeString(seqVal, false));
        }
        if ((seqVal = map.get("canonical")) != null) {
            props.setProperty("canonical", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("csv-header")) != null) {
            props.setProperty("csv-header", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("csv-quote-character")) != null) {
            props.setProperty("csv-quote-character", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("csv-separator")) != null) {
            props.setProperty("csv-separator", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("doctype-public")) != null) {
            props.setProperty("doctype-public", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("doctype-system")) != null) {
            props.setProperty("doctype-system", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("encoding")) != null) {
            props.setProperty("encoding", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("escape-solidus")) != null) {
            props.setProperty("escape-solidus", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("escape-uri-attributes")) != null) {
            props.setProperty("escape-uri-attributes", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("html-version")) != null) {
            props.setProperty("html-version", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("include-content-type")) != null) {
            props.setProperty("include-content-type", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("indent")) != null) {
            props.setProperty("indent", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("indent-attributes")) != null) {
            props.setProperty("indent-attributes", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("indent-unit")) != null) {
            props.setProperty("indent-unit", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("item-separator")) != null) {
            props.setProperty("item-separator", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("json-lines")) != null) {
            props.setProperty("json-lines", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("json-node-output-method")) != null) {
            props.setProperty("json-node-output-method", toMethodTypeString(seqVal));
        }
        if ((seqVal = map.get("line-ending")) != null) {
            props.setProperty("line-ending", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("media-type")) != null) {
            props.setProperty("media-type", seqVal.head().getStringValue());
        }
        if ((seqVal = map.get("method")) != null) {
            props.setProperty(OutputKeys.METHOD, toMethodTypeString(seqVal));
        }
        if ((seqVal = map.get("normalization-form")) != null) {
            props.setProperty("normalization-form", seqVal.head().getStringValue()); //NMTOKEN param type
        }
        if ((seqVal = map.get("omit-xml-declaration")) != null) {
            props.setProperty("omit-xml-declaration", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("standalone")) != null) {
            props.setProperty("standalone", toYesNoOmitTypeString(seqVal));
        }
        if ((seqVal = map.get("suppress-indentation")) != null) {
            props.setProperty("suppress-indentation", toQNamesTypeString(seqVal, false));
        }
        if ((seqVal = map.get("undeclare-prefixes")) != null) {
            props.setProperty("undeclare-prefixes", toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get("use-character-maps")) != null) {
            CharacterMap characterMap = toCharacterMap(seqVal, context);
            charMapIndex.putCharacterMap(NamespaceUri.NULL.qName("charMap"), characterMap);
            props.setProperty(SaxonOutputKeys.USE_CHARACTER_MAPS, "charMap");
        }
        if ((seqVal = map.get("version")) != null) {
            props.setProperty("version", seqVal.head().getStringValue());
        }
        // Saxon extension serialization parameters
        if ((seqVal = map.get(sx("attribute-order"))) != null) {
            props.setProperty(SaxonOutputKeys.ATTRIBUTE_ORDER, toQNamesTypeString(seqVal, true));
        }
        if ((seqVal = map.get(sx("canonical"))) != null) {
            props.setProperty(SaxonOutputKeys.CANONICAL, toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get(sx("character-representation"))) != null) {
            props.setProperty(SaxonOutputKeys.CHARACTER_REPRESENTATION, seqVal.head().getStringValue());
        }
        if ((seqVal = map.get(sx("double-space"))) != null) {
            props.setProperty(SaxonOutputKeys.DOUBLE_SPACE, toQNamesTypeString(seqVal, false));
        }
        if ((seqVal = map.get(sx("indent-spaces"))) != null) {
            props.setProperty(SaxonOutputKeys.INDENT_SPACES, seqVal.head().getStringValue());
        }
        if ((seqVal = map.get(sx("internal-dtd-subset"))) != null) {
            props.setProperty(SaxonOutputKeys.INTERNAL_DTD_SUBSET, seqVal.head().getStringValue());
        }
        if ((seqVal = map.get(sx("line-length"))) != null) {
            props.setProperty(SaxonOutputKeys.LINE_LENGTH, seqVal.head().getStringValue());
        }
        if ((seqVal = map.get(sx("newline"))) != null) {
            props.setProperty(SaxonOutputKeys.NEWLINE, seqVal.head().getStringValue());
        }
        if ((seqVal = map.get(sx("property-order"))) != null) {
            props.setProperty(SaxonOutputKeys.PROPERTY_ORDER, toSpaceSeparatedString(seqVal));
        }
        if ((seqVal = map.get(sx("recognize-binary"))) != null) {
            props.setProperty(SaxonOutputKeys.RECOGNIZE_BINARY, toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get(sx("conditional-cdata"))) != null) {
            props.setProperty(SaxonOutputKeys.CONDITIONAL_CDATA, toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get(sx("require-well-formed"))) != null) {
            props.setProperty(SaxonOutputKeys.REQUIRE_WELL_FORMED, toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get(sx("single-quotes"))) != null) {
            props.setProperty(SaxonOutputKeys.SINGLE_QUOTES, toYesNoTypeString(seqVal));
        }
        if ((seqVal = map.get(sx("supply-source-locator"))) != null) {
            props.setProperty(SaxonOutputKeys.SUPPLY_SOURCE_LOCATOR, toYesNoTypeString(seqVal));
        }
        // TODO: some of this validation is redundant
        SerializerFactory sf = context.getConfiguration().getSerializerFactory();
        for (String key : props.stringPropertyNames()) {
            String value = props.getProperty(key);
            sf.checkOutputProperty(key, value);
        }
        return new SerializationProperties(props, charMapIndex);
    }


    @Override
    public StringValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        return evalSerialize(arguments[0].iterate(),
            arguments.length == 1 ? null : arguments[1].head(), context);
    }

    private StringValue evalSerialize(SequenceIterator iter, Item param, XPathContext context) throws XPathException {

        SerializationProperties params;

        // The default parameter values are implementation-defined when an output:serialization-parameters
        // element is used (or when the argument is omitted), but are fixed by this specification in the
        // case where a map (including an empty map) is supplied for the argument.
        if (param != null) {
            if (param instanceof NodeInfo paramNode) {
                if (paramNode.getNodeKind() != Type.ELEMENT ||
                    !NamespaceUri.OUTPUT.equals(paramNode.getNamespaceUri()) ||
                    !"serialization-parameters".equals(paramNode.getLocalPart())) {
                    throw new XPathException("Second argument to fn:serialize() must be an element named {"
                        + NamespaceConstant.OUTPUT + "}serialization-parameters", "XPTY0004");
                }
                SerializationParamsHandler sph = new SerializationParamsHandler();
                sph.setSerializationParams(paramNode);
                params = sph.getSerializationProperties();

            } else if (param instanceof MapItem paramMap) {
                params = serializationOptionsAsProperties(context, paramMap, getDetails().optionDetails);

            } else {
                throw new XPathException("Second argument to fn:serialize() must either be an element named {"
                    + NamespaceConstant.OUTPUT + "}serialization-parameters, or a map (if using XPath 3.1)", "XPTY0004");
            }


        } else {
            params = new SerializationProperties(new Properties());
        }

        Properties props = params.getProperties();
        if (props.getProperty(OutputKeys.METHOD) == null) {
            props.setProperty(OutputKeys.METHOD, "xml");
        }
        if (props.getProperty(OutputKeys.OMIT_XML_DECLARATION) == null) {
            props.setProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
        }
        if (props.getProperty(SaxonOutputKeys.SPEC_VERSION) == null) {
            props.setProperty(SaxonOutputKeys.SPEC_VERSION, getRetainedStaticContext().getPackageData().getHostLanguageVersion() + "");
        }
                                                                                  
        // TODO add more spec-defined defaults here (for both cases)
        try {
            UnicodeBuilder builder = new UnicodeBuilder();
            UnicodeWriterResult result = new UnicodeWriterResult(builder, null);

            SerializerFactory sf = context.getConfiguration().getSerializerFactory();
            PipelineConfiguration pipe = context.getConfiguration().makePipelineConfiguration();
            PackageData pack = getRetainedStaticContext().getPackageData();
            pipe.setHostLanguage(pack.getHostLanguage(), pack.getHostLanguageVersion());
            Receiver out = sf.getReceiver(result, params, pipe);
            SequenceCopier.copySequence(iter, out);
            return new StringValue(builder.toString());
        } catch (XPathException e) {
            e.maybeSetErrorCode("SENR0001");
            throw e;
        }


    }

    public static SerializationProperties serializationOptionsAsProperties(XPathContext context, MapItem paramMap, OptionsParameter optionDetails) throws XPathException {
        SerializationProperties params;
        // If any parameters are supplied as QNames in the Saxon namespace, convert them to EQName strings
        for (KeyValuePair kvp : paramMap.keyValuePairs()) {
            AtomicValue k = kvp.key();
            if (k instanceof QNameValue && ((QNameValue)k).getStructuredQName().hasURI(NamespaceUri.SAXON)) {
                String s = ((QNameValue) k).getStructuredQName().getEQName();
                paramMap = paramMap.put(new StringValue(s), kvp.value());
            }
        }
        ProcessedOptions checkedOptions = optionDetails.processSuppliedOptions(paramMap, context);
        params = serializationParamsFromMap(checkedOptions, context);
        return params;
    }

}

// Copyright (c) 2011-2026 Saxonica Limited
