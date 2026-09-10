package com.saxonica.functions.qt4;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.NodeName_1;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.SaxonOutputKeys;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.serialize.JSONEmitter;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntToIntHashMap;

import javax.xml.transform.OutputKeys;
import java.util.HashMap;
import java.util.Map;
import java.util.Properties;
import java.util.Stack;

/**
 * Implementation of the fn:xdm-to-json function proposed for 4.0
 */
public class XdmToJsonFn extends SystemFunction {

    @CSharpModifiers(code = {"public", "static", "new"})
    public static OptionsParameter OPTION_DETAILS;
    private final static SequenceType layoutMapType =
            new SequenceType(
                    new MapType(BuiltInAtomicType.QNAME, SequenceType.SINGLE_STRING),
                    StaticProperty.ALLOWS_ONE);

    static {
        OptionsParameter parseJsonOptions = new OptionsParameter();
        parseJsonOptions.addAllowedOption("indent", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        parseJsonOptions.addAllowedOption("escape-solidus", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        parseJsonOptions.addAllowedOption("layouts", layoutMapType, new HashTrieMap());
        OPTION_DETAILS = parseJsonOptions;
    }

    private final IntHashMap<String> layoutMap = new IntHashMap<>();
    private Stack<StructuredQName> elementNameStack = new Stack<>();

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences
     * @return the result of the evaluation, in the form of a Sequence
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        GroundedValue val = arguments[0].materialize();
        // Initialise the element name stack with a dummy no-namespace name
        elementNameStack.add(new StructuredQName("", NamespaceUri.NULL, "dummy"));
        // Process the supplied options
        Map<String, Sequence> checkedOptions = null;
        if (getArity() == 2) {
            MapItem options = (MapItem) arguments[1].head();
            checkedOptions = getDetails().optionDetails.processSuppliedOptions(options, context);
            initLayoutMap(checkedOptions, context.getConfiguration());
        }  else {
            checkedOptions = new HashMap<>();
        }
        PipelineConfiguration pipe = context.getConfiguration().makePipelineConfiguration();
        UnicodeBuilder builder = new UnicodeBuilder();
        Properties outputProperties = new Properties();
        outputProperties.setProperty(SaxonOutputKeys.UNFAILING, "yes");
        if (checkedOptions.containsKey(OutputKeys.INDENT) && ((BooleanValue)checkedOptions.get(OutputKeys.INDENT)).getBooleanValue()) {
            outputProperties.setProperty(OutputKeys.INDENT,"yes");
        }
        if (checkedOptions.containsKey(SaxonOutputKeys.ESCAPE_SOLIDUS) && ((BooleanValue) checkedOptions.get(SaxonOutputKeys.ESCAPE_SOLIDUS)).getBooleanValue()) {
            outputProperties.setProperty(SaxonOutputKeys.ESCAPE_SOLIDUS, "yes");
        } else {
            outputProperties.setProperty(SaxonOutputKeys.ESCAPE_SOLIDUS, "no");
        }
        JSONEmitter emitter = new JSONEmitter(pipe, builder, outputProperties);
        emitSequence(val, checkedOptions, false, emitter);
        return new StringValue(builder.toUnicodeString());
    }

    private void initLayoutMap(Map<String, Sequence> checkedOptions, Configuration config) {
        MapItem val = (MapItem)checkedOptions.get("layouts").materialize();
        for (KeyValuePair pair : val.keyValuePairs()) {
             StructuredQName qn = ((QNameValue)pair.key).getStructuredQName();
             int fp = config.getNamePool().allocateFingerprint(qn.getNamespaceUri(), qn.getLocalPart());
             layoutMap.put(fp, ((StringValue)pair.value).getStringValue());
        }
    }

    /**
     * Output the JSON representation of a sequence
     * @param val the value to be output
     * @param options the top-level options to the call
     * @param forceArray if an array is to be used even if the size is zero or one
     * @param emitter the output destination
     * @throws XPathException if anything goes wrong
     */
    public void emitSequence(GroundedValue val, Map<String, Sequence> options, boolean forceArray, JSONEmitter emitter) throws XPathException {
        if (val.getLength() == 0 && !forceArray) {
            emitter.writeAtomicValue(null);
        } else if (val.getLength() == 1 && !forceArray) {
            emitItem(val.head(), options, emitter);
        } else {
            emitter.startArray(true);
            for (Item it : val.asIterable()) {
                emitItem(it, options, emitter);
            }
            emitter.endArray();
        }
    }

    private void emitItem(Item item, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        if (item instanceof AtomicValue) {
            if (item instanceof NumericValue) {
                if (((NumericValue)item).isNaN()) {
                    emitter.writeStringValue("NaN");
                } else if (Double.isInfinite(((NumericValue) item).getDoubleValue())) {
                    emitter.writeStringValue(((NumericValue) item).getDoubleValue() < 0 ? "-INF": "INF");
                } else {
                    emitter.writeAtomicValue((AtomicValue) item);
                }
            } else {
                emitter.writeAtomicValue((AtomicValue) item);
            }
        } else if (item instanceof NodeInfo) {
            switch (((NodeInfo)item).getNodeKind()) {
                case Type.DOCUMENT:
                    emitter.startMap(true);
                    emitter.writeKey("#document");
                    if (((NodeInfo) item).hasChildNodes()) {
                        emitMixedElementContent((NodeInfo)item, options, true, emitter);
                    }
                    emitter.endMap();
                    break;
                case Type.ATTRIBUTE:
                    emitter.startMap(true);
                    StructuredQName name = NodeName_1.nodeName(((NodeInfo)item)).getStructuredQName();
                    NamespaceUri ns = name.getNamespaceUri();
                    String attKey;
                    if (ns.equals(NamespaceUri.NULL)) {
                        attKey = "@" + name.getLocalPart();
                    } else if (ns.equals(NamespaceUri.XML)) {
                        attKey = "@xml:" + name.getLocalPart();
                    } else {
                        attKey = "@" + name.getEQName();
                    }
                    emitter.writeKey(attKey);
                    SchemaType type = ((NodeInfo) item).getSchemaType();
                    emitSequence(item.atomize(), options, ((SimpleType)type).isListType(), emitter);
                    emitter.endMap();
                    break;
                case Type.COMMENT:
                    emitter.startMap(true);
                    emitter.writeKey("#comment");
                    emitter.writeStringValue(item.getStringValue());
                    emitter.endMap();
                    break;
                case Type.PROCESSING_INSTRUCTION:
                    emitter.startMap(true);
                    emitter.writeKey("#processing-instruction");
                    emitter.startMap(true);
                    emitter.writeKey("#name");
                    emitter.writeStringValue(((NodeInfo) item).getDisplayName());
                    emitter.writeKey("#data");
                    emitter.writeStringValue(item.getStringValue());
                    emitter.endMap();
                    emitter.endMap();
                    break;
                case Type.TEXT:
                    emitter.startMap(true);
                    emitter.writeKey("#text");
                    emitter.writeStringValue(item.getStringValue());
                    emitter.endMap();
                    break;
                case Type.ELEMENT:
                    emitElement((NodeInfo)item, options, emitter);
                    break;
                case Type.NAMESPACE:
                    emitter.startMap(true);
                    emitter.writeKey("#namespace");
                    emitter.startMap(true);
                    emitter.writeKey("#prefix");
                    emitter.writeStringValue(((NodeInfo) item).getDisplayName());
                    emitter.writeKey("#uri");
                    emitter.writeStringValue(item.getStringValue());
                    emitter.endMap();
                    emitter.endMap();
                    break;
                default:
                    throw new IllegalStateException();
            }
        } else if (item instanceof ArrayItem) {
            emitter.startArray(true);
            for (GroundedValue member : ((ArrayItem)item).members()) {
                emitSequence(member, options, false, emitter);
            }
            emitter.endArray();
        } else if (item instanceof MapItem) {
            emitter.startMap(true);
            for (KeyValuePair pair : ((MapItem)item).keyValuePairs()) {
                emitter.writeKey(pair.key.getStringValue());
                emitSequence(pair.value, options, false, emitter);
            }
            emitter.endMap();
        } else if (item instanceof FunctionItem) {
            emitter.startMap(true);
            emitter.writeKey("#function");
            emitter.startMap(true);
            StructuredQName name = ((FunctionItem)item).getFunctionName();
            if (name != null && !name.hasURI(NamespaceUri.ANONYMOUS)) {
                emitter.writeKey("#name");
                emitter.writeStringValue(name.getEQName());
            }
            emitter.writeKey("#arity");
            emitter.writeStringValue(((FunctionItem) item).getArity() + "");
            emitter.endMap();
            emitter.endMap();
        }
    }

    private void emitElement(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        StructuredQName name = NodeName_1.nodeName(element).getStructuredQName();
        String tag;
        if (name.hasURI(elementNameStack.peek().getNamespaceUri())) {
            tag = name.getLocalPart();
        } else {
            tag = name.getEQName();
        }
        elementNameStack.push(name);
        emitter.startMap(true);
        emitter.writeKey(tag);
        emitElementContent(element, options, emitter);
        emitter.endMap();
        elementNameStack.pop();
    }

    /**
     * Output the content of an element (excluding the element name) using a layout chosen
     * for the particular element
     * @param element the element to output
     * @param options the original options
     * @param emitter the output destination
     * @throws XPathException in the event of failure
     */
    private void emitElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        // First see if an explicit layout has been chosen for this element in the supplied options
        String explicitLayout = layoutMap.get(element.getFingerprint());
        if (explicitLayout != null) {
            switch (explicitLayout) {
                case "empty":
                    emitEmptyElementContent(element, options, emitter);
                    break;
                case "empty-plus":
                    emitEmptyPlusElementContent(element, options, emitter);
                    break;
                case "simple":
                    emitSimpleElementContent(element, options, emitter);
                    break;
                case "simple-plus":
                    emitSimplePlusElementContent(element, options, emitter);
                    break;
                case "list":
                    emitListElementContent(element, options, emitter, null);
                    break;
                case "list-plus":
                    emitListPlusElementContent(element, options, emitter, null);
                    break;
                case "record":
                    emitRecordElementContent(element, options, emitter);
                    break;
                case "sequence":
                    emitMixedElementContent(element, options, false, emitter);
                    break;
                case "mixed":
                    emitMixedElementContent(element, options, false, emitter);
                    break;
            }
        } else {
            // If the element is schema-validated, use schema information
            SchemaType type = element.getSchemaType();
            if (!(type == AnyType.getInstance() || type == Untyped.getInstance())) {
                if (type instanceof SimpleType) {
                    emitSimpleElementContent(element, options, emitter);
                } else {
                    ComplexType cType = (ComplexType) type;
                    String layout = cType.getPreferredJsonLayout();
                    String suffix = "";
                    int sep = layout.indexOf("/");
                    if (sep > 0) {
                        suffix = layout.substring(sep + 1);
                        layout = layout.substring(0, sep);
                    }
                    switch (layout) {
                        case "simple-plus":
                            emitSimplePlusElementContent(element, options, emitter);
                            break;
                        case "empty":
                            emitEmptyElementContent(element, options, emitter);
                            break;
                        case "empty-plus":
                            emitEmptyPlusElementContent(element, options, emitter);
                            break;
                        case "list":
                            emitListElementContent(element, options, emitter, suffix);
                            break;
                        case "list-plus":
                            emitListPlusElementContent(element, options, emitter, suffix);
                            break;
                        case "record":
                            emitRecordElementContent(element, options, emitter);
                            break;
                        case "sequence":
                            emitSequenceElementContent(element, options, emitter);
                            break;
                        case "mixed":
                            emitMixedElementContent(element, options, false, emitter);
                            break;
                        default:
                            throw new IllegalStateException();

                    }
                }
            } else {
                int nonWhiteTextNodeCounter = 0;
                int whiteTextNodeCounter = 0;
                int childElementCounter = 0;
                IntHashSet distinctFingerprints = new IntHashSet();
                for (NodeInfo child : element.children()) {
                    if (child.getNodeKind() == Type.ELEMENT) {
                        int fp = child.getFingerprint();
                        childElementCounter++;
                        distinctFingerprints.add(fp);
                    } else if (child.getNodeKind() == Type.TEXT) {
                        if (Whitespace.isAllWhite(child.getUnicodeStringValue())) {
                            whiteTextNodeCounter++;
                        } else {
                            nonWhiteTextNodeCounter++;
                        }
                    }
                }

                if (nonWhiteTextNodeCounter == 0 && whiteTextNodeCounter == 0 && childElementCounter == 0) {
                    // Empty content
                    if (element.attributes().size() == 0) {
                        emitEmptyElementContent(element, options, emitter);
                    } else {
                        emitEmptyPlusElementContent(element, options, emitter);
                    }
                } else if (childElementCounter == 0) {
                    // Simple content
                    if (element.attributes().size() == 0) {
                        emitSimpleElementContent(element, options, emitter);
                    } else {
                        emitSimplePlusElementContent(element, options, emitter);
                    }
                } else if (nonWhiteTextNodeCounter == 0) {
                    // Element-only content
                    if (childElementCounter == 1) {
                        emitRecordElementContent(element, options, emitter);
                    } else if (distinctFingerprints.size() == 1 && childElementCounter >= 2) {
                        if (element.attributes().size() == 0) {
                            emitListElementContent(element, options, emitter, null);
                        } else {
                            emitListPlusElementContent(element, options, emitter, null);
                        }
                    } else if (childElementCounter == distinctFingerprints.size()) {
                        emitRecordElementContent(element, options, emitter);
                    } else {
                        emitSequenceElementContent(element, options, emitter);
                    }
                } else {
                    emitMixedElementContent(element, options, false, emitter);
                }
            }
        }
    }



    public void emitEmptyElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        emitter.writeStringValue("");
    }

    public void emitEmptyPlusElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        emitter.startMap(false);
        emitAttributes(element, options, emitter);
        emitter.endMap();
    }

    public void emitSimpleElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        SchemaType type = element.getSchemaType();
        boolean asList = type instanceof SimpleType && ((SimpleType)type).isListType();
        emitSequence(element.atomize(), options, asList, emitter);
    }

    public void emitSimplePlusElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        emitter.startMap(false);
        emitAttributes(element, options, emitter);
        emitter.writeKey("#content");
        SchemaType type = element.getSchemaType();
        boolean asList = type instanceof SimpleType && ((SimpleType) type).isListType();
        emitSequence(element.atomize(), options, asList, emitter);
        emitter.endMap();
    }

    public void emitListElementContent(NodeInfo element,
                                       Map<String, Sequence> options,
                                       JSONEmitter emitter,
                                       String childEQName) throws XPathException {

        String key;
        if (childEQName == null) {
            NodeInfo firstChild = element.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT).next();
            assert firstChild != null;
            if (firstChild.getNamespaceUri().equals(element.getNamespaceUri())) {
                key = firstChild.getLocalPart();
            } else {
                key = "Q{" + firstChild.getNamespaceUri() + "}" + firstChild.getLocalPart();
            }
        } else {
            StructuredQName qn = StructuredQName.fromEQName(childEQName);
            if (qn.getNamespaceUri().equals(element.getNamespaceUri())) {
                key = qn.getLocalPart();
            } else {
                key = childEQName;
            }
        }

        emitter.startArray(false);
        for (NodeInfo child : element.children()) {
            if (child.getNodeKind() == Type.ELEMENT) {
                emitElementContent(child, options, emitter);
            }
        }
        emitter.endArray();
    }


    public void emitListPlusElementContent(NodeInfo element,
                                       Map<String, Sequence> options,
                                       JSONEmitter emitter,
                                       String childEQName) throws XPathException {

        String key;
        if (childEQName == null) {
            NodeInfo firstChild = element.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT).next();
            assert firstChild != null;
            if (firstChild.getNamespaceUri().equals(element.getNamespaceUri())) {
                key = firstChild.getLocalPart();
            } else {
                key = "Q{" + firstChild.getNamespaceUri() + "}" + firstChild.getLocalPart();
            }
        } else {
            StructuredQName qn = StructuredQName.fromEQName(childEQName);
            if (qn.getNamespaceUri().equals(element.getNamespaceUri())) {
                key = qn.getLocalPart();
            } else {
                key = childEQName;
            }
        }

        emitter.startMap(false);
        emitAttributes(element, options, emitter);
        emitter.writeKey(key);
        emitter.startArray(false);
        for (NodeInfo child : element.children()) {
            if (child.getNodeKind() == Type.ELEMENT) {
                emitElementContent(child, options, emitter);
            }
        }
        emitter.endArray();
        emitter.endMap();
    }


    public void emitRecordElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        emitter.startMap(false);
        emitAttributes(element, options, emitter);
        if (element.isNilled()) {
            emitter.writeKey("#content");
            emitter.writeAtomicValue(null);
        } else {
            boolean containsDuplicates = false;
            IntToIntHashMap counters = new IntToIntHashMap();
            for (NodeInfo child : element.children()) {
                if (child.getNodeKind() == Type.ELEMENT) {
                    int fp = child.getFingerprint();
                    if (counters.contains(fp)) {
                        containsDuplicates = true;
                        counters.put(fp, counters.get(fp) + 1);
                    } else {
                        counters.put(fp, 1);
                    }
                }
            }
            IntToIntHashMap indexes = null;
            if (containsDuplicates) {
                indexes = new IntToIntHashMap();
            }
            for (NodeInfo child : element.children()) {
                if (child.getNodeKind() == Type.ELEMENT) {
                    String key = child.getNamespaceUri().equals(element.getNamespaceUri())
                            ? child.getLocalPart()
                            : "Q{" + child.getNamespaceUri() + "}" + child.getLocalPart();
                    if (containsDuplicates) {
                        int fp = child.getFingerprint();
                        if (counters.get(fp) > 1) {
                            int seq = indexes.contains(fp) ? indexes.get(fp) + 1 : 1;
                            indexes.put(fp, seq);
                            key += ("[" + seq + "]");
                        }
                    }
                    emitter.writeKey(key);
                    emitElementContent(child, options, emitter);
                }
            }
        }
        emitter.endMap();
    }

    public void emitSequenceElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        emitMixedElementContent(element, options, true, emitter);
    }
    public void emitMixedElementContent(NodeInfo element, Map<String, Sequence> options, boolean stripSpace, JSONEmitter emitter) throws XPathException {
        emitter.startMap(false);
        emitAttributes(element, options, emitter);
        emitter.writeKey("#content");
        if (element.isNilled()) {
            emitter.writeAtomicValue(null);
        } else {
            emitter.startArray(false);
            for (NodeInfo child : element.children()) {
                switch (child.getNodeKind()) {
                    case Type.TEXT:
                        if (!(stripSpace && Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                            emitter.writeStringValue(child.getStringValue());
                        }
                        break;
                    case Type.COMMENT:
                        emitter.startMap(true);
                        emitter.writeKey("#comment");
                        emitter.writeStringValue(child.getStringValue());
                        emitter.endMap();
                        break;
                    case Type.PROCESSING_INSTRUCTION:
                        emitter.startMap(true);
                        emitter.writeKey("#processing-instruction");
                        emitter.startMap(true);
                        emitter.writeKey("#name");
                        emitter.writeStringValue(child.getDisplayName());
                        emitter.writeKey("#data");
                        emitter.writeStringValue(child.getStringValue());
                        emitter.endMap();
                        emitter.endMap();
                        break;
                    case Type.ELEMENT:
                        emitElement(child, options, emitter);
                        break;
                    default:
                        throw new IllegalStateException();
                }
            }
            emitter.endArray();
        }
        emitter.endMap();
    }

    public void emitAttributes(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        NodeInfo att;
        SequenceIterator iter = element.iterateAxis(AxisInfo.ATTRIBUTE);
        while ((att = (NodeInfo)iter.next()) != null) {
            NamespaceUri ns = att.getNamespaceUri();
            if (!ns.equals(NamespaceUri.SCHEMA_INSTANCE)) {
                String attKey;
                if (ns.equals(NamespaceUri.NULL)) {
                    attKey = "@" + att.getLocalPart();
                } else if (ns.equals(NamespaceUri.XML)) {
                    attKey = "@xml:" + att.getLocalPart();
                } else {
                    attKey = "@Q{" + att.getNamespaceUri() + "}" + att.getLocalPart();
                }
                emitter.writeKey(attKey);
                AtomicSequence val = att.atomize();
                SchemaType type = att.getSchemaType();
                boolean asList = ((SimpleType) type).isListType();
                emitSequence(val, options, asList, emitter);
            }
        }
    }


    private boolean isSimpleContent(NodeInfo element) {
        SchemaType type = element.getSchemaType();
        if (type instanceof SimpleType) {
            return true;
        }
        if (((ComplexType)type).isSimpleContent()) {
            return true;
        }
        int textCount = 0;
        int otherCount = 0;
        for (NodeInfo child : element.children()) {
            if (child.getNodeKind() == Type.TEXT) {
                textCount++;
            } else {
                otherCount++;
            }
        }
        return textCount == 1 && otherCount == 0;
    }


}

// Copyright (c) 2023 Saxonica Limited.

