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
        emitSequence(val, checkedOptions, emitter);
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

    public void emitSequence(GroundedValue val, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        if (val.getLength() == 0) {
            emitter.writeAtomicValue(null);
        } else if (val.getLength() == 1) {
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
                    emitSequence(item.atomize(), options, emitter);
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
                emitSequence(member, options, emitter);
            }
            emitter.endArray();
        } else if (item instanceof MapItem) {
            emitter.startMap(true);
            for (KeyValuePair pair : ((MapItem)item).keyValuePairs()) {
                emitter.writeKey(pair.key.getStringValue());
                emitSequence(pair.value, options, emitter);
            }
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

    private void emitElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
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
                    emitListElementContent(element, options, emitter);
                    break;
                case "record":
                    emitRecordElementContent(element, options, emitter);
                    break;
                case "sequence":
                    emitMixedElementContent(element, options, false, emitter);
                    break;
                case "mixed":
                    emitMixedElementContent(element, options, true, emitter);
                    break;
            }
        } else {
            SchemaType type = element.getSchemaType();
            if (!(type == AnyType.getInstance() || type == Untyped.getInstance())) {
                // TODO: skeleton code here
                if (type instanceof SimpleType) {
                    emitSimpleElementContent(element, options, emitter);
                } else {
                    ComplexType cType = (ComplexType) type;
                    if (cType.isSimpleContent() && cType.allowsAttributes()) {
                        emitSimplePlusElementContent(element, options, emitter);
                    } else if (cType.isEmptyContent()) {
                        if (cType.allowsAttributes()) {
                            emitEmptyPlusElementContent(element, options, emitter);
                        } else {
                            emitEmptyElementContent(element, options, emitter);
                        }
                    } else {
                        emitMixedElementContent(element, options, true, emitter);
                    }
                }
            } else {
                if (element.hasChildNodes()) {
                    if (hasSingleTextNodeChild(element)) {
                        if (element.attributes().size() == 0) {
                            emitSimpleElementContent(element, options, emitter);
                        } else {
                            emitSimplePlusElementContent(element, options, emitter);
                        }
                    } else if (hasListContentModel(element)) {
                        emitListElementContent(element, options, emitter);
                    } else if (hasRecordContentModel(element)) {
                        emitMixedElementContent(element, options, true, emitter);
                    } else {
                        emitMixedElementContent(element, options, false, emitter);
                    }
                } else if (element.attributes().size() == 0) {
                    emitEmptyElementContent(element, options, emitter);
                } else {
                    emitEmptyPlusElementContent(element, options, emitter);
                }
            }
        }
    }

    private boolean hasSingleTextNodeChild(NodeInfo element) {
        boolean foundText = false;
        for (NodeInfo child : element.children()) {
            if (child.getNodeKind() == Type.TEXT) {
                if (foundText) {
                    return false;
                }
                foundText = true;
            } else {
                return false;
            }
        }
        return foundText;
    }

    private boolean hasListContentModel(NodeInfo element) {
        int count = 0;
        int lastFingerprint = -1;
        for (NodeInfo child : element.children()) {
            if (child.getNodeKind() == Type.ELEMENT) {
                if (lastFingerprint != -1 && child.getFingerprint() != lastFingerprint) {
                    return false;
                }
                lastFingerprint = child.getFingerprint();
            }
            count++;
            if (child.getNodeKind() == Type.TEXT && !Whitespace.isAllWhite(child.getUnicodeStringValue())) {
                return false;
            }
        }
        return count >= 2;
    }

    private boolean hasRecordContentModel(NodeInfo element) {
        int count = 0;
        IntHashSet fingerprints = new IntHashSet();
        for (NodeInfo child : element.children()) {
            if (child.getNodeKind() == Type.ELEMENT) {
                if (fingerprints.contains(element.getFingerprint())) {
                    return false;
                }
                fingerprints.add(element.getFingerprint());
            }
            count++;
            if (child.getNodeKind() == Type.TEXT && !Whitespace.isAllWhite(child.getUnicodeStringValue())) {
                return false;
            }
        }
        return count >= 2;
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
        emitSequence(element.atomize(), options, emitter);
    }

    public void emitSimplePlusElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        emitter.startMap(false);
        emitAttributes(element, options, emitter);
        emitter.writeKey("#content");
        emitSequence(element.atomize(), options, emitter);
        emitter.endMap();
    }

    public void emitListElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        NodeInfo firstChild = element.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT).next();
        if (firstChild != null) {
            emitter.startMap(false);
            emitAttributes(element, options, emitter);
            emitter.writeKey(firstChild.getLocalPart());
            emitter.startArray(false);
            for (NodeInfo child : element.children()) {
                if (child.getNodeKind() == Type.ELEMENT) {
                    emitElementContent(child, options, emitter);
                }
            }
            emitter.endArray();
            emitter.endMap();
        }

    }

    public void emitRecordElementContent(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        emitter.startMap(false);
        emitAttributes(element, options, emitter);
        emitter.writeKey("#content");
        emitter.startArray(false);
        // TODO: handle element names that appear more than once
        for (NodeInfo child : element.children()) {
            if (child.getNodeKind() == Type.ELEMENT) {
                emitElement(child, options, emitter);
            }
        }
        emitter.endArray();
        emitter.endMap();
    }
    

    public void emitMixedElementContent(NodeInfo element, Map<String, Sequence> options, boolean stripSpace, JSONEmitter emitter) throws XPathException {
        emitter.startMap(false);
        emitAttributes(element, options, emitter);
        emitter.writeKey("#content");
        emitter.startArray(false);
        for (NodeInfo child : element.children()) {
            switch(child.getNodeKind()) {
                case Type.TEXT:
                    if (!stripSpace && Whitespace.isAllWhite(child.getUnicodeStringValue())) {
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
        emitter.endMap();
    }

    public void emitAttributes(NodeInfo element, Map<String, Sequence> options, JSONEmitter emitter) throws XPathException {
        NodeInfo att;
        SequenceIterator iter = element.iterateAxis(AxisInfo.ATTRIBUTE);
        while ((att = (NodeInfo)iter.next()) != null) {
            NamespaceUri ns = att.getNamespaceUri();
            String attKey;
            if (ns.equals(NamespaceUri.NULL)) {
                attKey = "@" + att.getLocalPart();
            } else if (ns.equals(NamespaceUri.XML)) {
                attKey = "@xml:" + att.getLocalPart();
            } else {
                attKey = "@Q{" + att.getNamespaceUri() + "}" + att.getLocalPart();
            }
            emitter.writeKey(attKey);
            emitSequence(att.atomize(), options, emitter);
        }
    }

//        )
//        switch (explicitLayout) {
//
//        }
//        char format = 'A';
//        if (valueElements.contains(name)) {
//            format = 'V';
//        } else if (mapElements.contains(name)) {
//            format = 'M';
//        } else if (arrayElements.contains(name)) {
//            format = 'A';
//        } else if (!element.hasChildNodes()) {
//            format = 'E';
//        } else if (isSimpleContent(element)) {
//            format = 'V';
//        }
//        switch (format) {
//            case 'E':
//                // Empty format
//                break;
//            case 'V':
//                // Value format
//                emitter.writeKey("#value");
//                emitSequence(element.atomize(), options, emitter);
//                break;
//            case 'A':
//                // Array format
//                emitter.writeKey("#content");
//                emitter.startArray(true);
//                for (NodeInfo child : element.children()) {
//                    emitItem(child, options, emitter);
//                }
//                emitter.endArray();
//                break;
//            case 'M':
//                Map<QNameValue, Integer> counters = new HashMap<>();
//                for (NodeInfo child : element.children()) {
//                    // initialise the counters table to the total count of elements, negated
//                    if (child.getNodeKind() == Type.ELEMENT) {
//                        QNameValue childName = NodeName_1.nodeName(child);
//                        if (counters.containsKey(childName)) {
//                            counters.put(childName, counters.get(childName) - 1);
//                        } else {
//                            counters.put(childName, -1);
//                        }
//                    }
//                }
//                for (NodeInfo child : element.children()) {
//                    if (child.getNodeKind() == Type.ELEMENT) {
//                        QNameValue childName = NodeName_1.nodeName(child);
//                        int counter = counters.get(childName);
//                        if (counter == -1) {
//                            emitter.writeKey(childName.getLocalName()); // TODO: namespaces
//                        } else {
//                            if (counter < 0) {
//                                counter = 1;
//                            }
//                            emitter.writeKey(childName.getLocalName() + "[" + counter + "]");
//                            counters.put(childName, counter++);
//                        }
//                        emitItem(child, options, emitter);
//                    }
//                }
//                break;
//            default:
//                throw new IllegalStateException("Bad format code");
//        }


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

