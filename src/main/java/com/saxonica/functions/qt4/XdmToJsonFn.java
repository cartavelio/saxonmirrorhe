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
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntSet;
import net.sf.saxon.z.IntToIntHashMap;

import javax.xml.transform.OutputKeys;
import java.util.*;

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
        parseJsonOptions.addAllowedOption("uniform", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        parseJsonOptions.addAllowedOption("escape-solidus", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        parseJsonOptions.addAllowedOption("layouts", layoutMapType, new HashTrieMap());
        OPTION_DETAILS = parseJsonOptions;
    }

    /**
     * A bundle of context data that is passed down the recursive calls
     */

    private static class JsonConversionContext {
        public Configuration config;
        public JSONEmitter emitter;
        public Schema schema;
        public IntToIntHashMap uniformityData = null;
        public IntHashMap<String> layoutMap = new IntHashMap<>();
        public Stack<StructuredQName> elementNameStack = new Stack<>();
    }



    /**
     * Evaluate the function call
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
        JsonConversionContext jcc = new JsonConversionContext();
        jcc.config = context.getConfiguration();
        jcc.schema = getRetainedStaticContext().getPackageData().getImportedSchema();
        // Initialise the element name stack with a dummy no-namespace name
        jcc.elementNameStack.push(new StructuredQName("", NamespaceUri.NULL, "dummy"));
        // Process the supplied options
        Map<String, Sequence> checkedOptions = new HashMap<>();
        boolean indent = false;
        boolean escapeSolidus = false;
        boolean uniform = false;
        if (arguments.length > 1) {
            MapItem suppliedOptions = (MapItem) arguments[1].head();
            checkedOptions = getDetails().optionDetails.processSuppliedOptions(suppliedOptions, context);
            indent = ((BooleanValue) checkedOptions.get("indent")).getBooleanValue();
            escapeSolidus = ((BooleanValue) checkedOptions.get("escape-solidus")).getBooleanValue();
            uniform = ((BooleanValue) checkedOptions.get("uniform")).getBooleanValue();
            initLayoutMap(jcc, checkedOptions, context.getConfiguration());
        }

        PipelineConfiguration pipe = context.getConfiguration().makePipelineConfiguration();
        UnicodeBuilder builder = new UnicodeBuilder();
        Properties outputProperties = new Properties();
        outputProperties.setProperty(SaxonOutputKeys.UNFAILING, "yes");
        outputProperties.setProperty(OutputKeys.INDENT, indent ? "yes" : "no");
        outputProperties.setProperty(SaxonOutputKeys.ESCAPE_SOLIDUS, escapeSolidus ? "yes": "no");

        if (uniform) {
            jcc.uniformityData = gatherUniformityData(val);
        }
        jcc.emitter = new JSONEmitter(pipe, builder, outputProperties);
        emitSequence(val, jcc, false);
        return new StringValue(builder.toUnicodeString());
    }

    /**
     * Process the supplied value of the {@code layouts} option: a map of specific layouts
     * chosen for specific element names
     * @param jcc the conversion context
     * @param checkedOptions the supplied options to the function, after validation and expansion of defaults
     * @param config the Saxon configuration
     */
    private void initLayoutMap(JsonConversionContext jcc, Map<String, Sequence> checkedOptions, Configuration config)
            throws XPathException {
        MapItem val = (MapItem)checkedOptions.get("layouts").materialize();
        for (KeyValuePair pair : val.keyValuePairs()) {
             StructuredQName qn = ((QNameValue)pair.key).getStructuredQName();
             int fp = config.getNamePool().allocateFingerprint(qn.getNamespaceUri(), qn.getLocalPart());
             jcc.layoutMap.put(fp, ((StringValue)pair.value).getStringValue());
        }
    }

    /**
     * Examine all the items in the input sequence, together with their descendants in the
     * case of document or element nodes, to gather for each distinct element name, the properties
     * that are common to all the elements of any given name
     * @param input the input sequence to the function
     * @return a hash map, indexed by element name fingerprint, containing the properties
     * applicable to each element name encountered.
     */

    private IntToIntHashMap gatherUniformityData(GroundedValue input) {
        IntToIntHashMap data = new IntToIntHashMap();
        for (Item it : input.asIterable()) {
            if (it instanceof NodeInfo) {
                if (((NodeInfo)it).getNodeKind() == Type.DOCUMENT) {
                    AxisIterator iter = ((NodeInfo)it).iterateAxis(AxisInfo.DESCENDANT, NodeKindTest.ELEMENT);
                    NodeInfo element;
                    while ((element = iter.next()) != null) {
                        updateUniformityData(element, data);
                    }
                } else if (((NodeInfo) it).getNodeKind() == Type.ELEMENT) {
                    AxisIterator iter = ((NodeInfo) it).iterateAxis(AxisInfo.DESCENDANT_OR_SELF, NodeKindTest.ELEMENT);
                    NodeInfo element;
                    while ((element = iter.next()) != null) {
                        updateUniformityData(element, data);
                    }
                }
            }
        }
        return data;
    }

    /**
     * Property indicating that an element has one or more attributes
     */
    private final static int HAS_ATTRIBUTES = 1 << 30;

    /**
     * Property indicating that an element has one or more child elements
     */
    private final static int HAS_CHILD_ELEMENTS = 1 << 29;

    /**
     * Property indicating that an element has one or more non-whitespace text node children
     */

    private final static int HAS_NON_WHITESPACE_TEXT_NODES = 1 << 28;

    /**
     * Property indicating that an element has one or more text node children consisting entirely of whitespace
     */
    private final static int HAS_WHITESPACE_TEXT_NODES = 1 << 27;

    /**
     * Property indicating that there are at least two children of an element having different names
     */

    private final static int HAS_DIVERSE_CHILD_ELEMENTS = 1 << 26;

    /**
     * Property indicating that all the element node children of an element have the same name, and that
     * there are at least two such children
     */
    private final static int HAS_REPEATED_CHILD_ELEMENTS = 1 << 25;

    /**
     * Property indicating that all the element node children of an element have different names
     */

    private final static int HAS_UNIQUE_CHILD_ELEMENTS = 1 << 24;

    /**
     * Get the properties of an element node, and add the details to
     * a hashmap containing details for all element nodes, to support
     * the uniform=yes option
     * @param element the element node
     * @param data the hash map to be updated
     */

    private void updateUniformityData(NodeInfo element, IntToIntHashMap data) {
        int fp = element.getFingerprint();
        boolean existing = data.contains(fp);
        int properties = existing ? data.get(fp) : 0;
        int newProperties = getElementProperties(element, properties);
        if (newProperties != properties || !existing) {
            data.put(fp, newProperties);
        }
    }

    /**
     * Get the properties of an element node.
     * @param element The element node.
     * @param properties Zero if evaluating for this element on its own. Otherwise, when
     *                   looking for common properties of all elements sharing the same name,
     *                   the properties already gathered for other elements having the same name.
     * @return the properties of this element, or, the properties shared by this element
     * and others having the same element name. The returned value consists of a set of
     * flags in the top 10 bits indicating for example whether the element has attributes,
     * child element nodes, whitespace and/or non-whitespace text nodes; and in the bottom
     * 20 bits, where all the children of an element have the same name, the fingerprint of the
     * child element name.
     */

    private int getElementProperties(NodeInfo element, int properties) {
        if ((properties & HAS_ATTRIBUTES) == 0 && element.attributes().size() > 0) {
            properties |= HAS_ATTRIBUTES;
        }
        if ((properties & (HAS_CHILD_ELEMENTS | HAS_NON_WHITESPACE_TEXT_NODES | HAS_DIVERSE_CHILD_ELEMENTS)) == 0
                && element.hasChildNodes()) {
            int firstChild = -1;
            int countChildElements = 0;
            boolean hasDuplicates = false;
            IntSet fingerprints = new IntHashSet();
            for (NodeInfo child : element.children()) {
                if (child.getNodeKind() == Type.TEXT) {
                    if (Whitespace.isAllWhite(child.getUnicodeStringValue())) {
                        properties |= HAS_WHITESPACE_TEXT_NODES;
                    } else {
                        properties |= HAS_NON_WHITESPACE_TEXT_NODES;
                    }
                } else if (child.getNodeKind() == Type.ELEMENT) {
                    int childFingerprint = child.getFingerprint();
                    if (fingerprints.contains(childFingerprint)) {
                        hasDuplicates = true;
                    }
                    countChildElements++;
                    properties |= HAS_CHILD_ELEMENTS;
                    fingerprints.add(childFingerprint);
                    if (firstChild != -1) {
                        if (firstChild != childFingerprint) {
                            properties |= HAS_DIVERSE_CHILD_ELEMENTS;
                        } else {
                            properties |= HAS_REPEATED_CHILD_ELEMENTS;
                        }
                    } else {
                        firstChild = child.getFingerprint();
                    }
                }
            }
            if (firstChild != -1 && ((properties & HAS_DIVERSE_CHILD_ELEMENTS) == 0)) {
                if ((properties & NamePool.FP_MASK) == 0) {
                    properties |= (firstChild & NamePool.FP_MASK);
                } else {
                    properties |= HAS_DIVERSE_CHILD_ELEMENTS;
                }
            }
            if (countChildElements == 1) {
                properties |= HAS_DIVERSE_CHILD_ELEMENTS;  // there's only one, so they are all different
            }
            if (!hasDuplicates) {
                properties |= HAS_UNIQUE_CHILD_ELEMENTS;
            }
        }
        return properties;
    }

    /**
     * Output the JSON representation of a sequence
     * @param val the value to be output
     * @param jcc the context information for the call
     * @param forceArray if an array is to be used even if the size is zero or one
     * @throws XPathException if anything goes wrong
     */
    private void emitSequence(GroundedValue val,
                             JsonConversionContext jcc,
                             boolean forceArray) throws XPathException {
        if (val.getLength() == 0 && !forceArray) {
            jcc.emitter.writeAtomicValue(null);
        } else if (val.getLength() == 1 && !forceArray) {
            emitItem(val.head(), jcc);
        } else {
            jcc.emitter.startArray(true);
            for (Item it : val.asIterable()) {
                emitItem(it, jcc);
            }
            jcc.emitter.endArray();
        }
    }

    /**
     * Output the JSON represention of an item
     * @param item the item in question
     * @param jcc the conversion context
     * @throws XPathException if anything goes wong
     */

    private void emitItem(Item item,
                          JsonConversionContext jcc) throws XPathException {

        JSONEmitter emitter = jcc.emitter;
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
                        emitMixedElementContent((NodeInfo)item, jcc, true);
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
                    emitSequence(item.atomize(), jcc, ((SimpleType)type).isListType());
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
                    emitElement((NodeInfo)item, jcc);
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
                emitSequence(member, jcc, false);
            }
            emitter.endArray();
        } else if (item instanceof MapItem) {
            emitter.startMap(true);
            for (KeyValuePair pair : ((MapItem)item).keyValuePairs()) {
                emitter.writeKey(pair.key.getStringValue());
                emitSequence(pair.value, jcc, false);
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

    /**
     * Output an element node
     * @param element the element node
     * @param jcc the conversion context
     * @throws XPathException if anything goes wrong
     */
    private void emitElement(NodeInfo element,
                             JsonConversionContext jcc) throws XPathException {
        StructuredQName name = Objects.requireNonNull(
                NodeName_1.nodeName(element)).getStructuredQName();
        String tag;
        if (name.hasURI(jcc.elementNameStack.peek().getNamespaceUri())) {
            tag = name.getLocalPart();
        } else {
            tag = name.getEQName();
        }
        jcc.elementNameStack.push(name);
        jcc.emitter.startMap(true);
        jcc.emitter.writeKey(tag);
        emitElementContent(element, jcc);
        jcc.emitter.endMap();
        jcc.elementNameStack.pop();
    }

    /**
     * Output the content of an element (excluding the element name) using a layout chosen
     * for the particular element
     * @param element the element to output
     * @param jcc the context for the operation
     * @throws XPathException in the event of failure
     */
    private void emitElementContent(NodeInfo element,
                                    JsonConversionContext jcc) throws XPathException {
        // First see if an explicit layout has been chosen for this element in the supplied options
        String explicitLayout = jcc.layoutMap.get(element.getFingerprint());
        if (explicitLayout != null) {
            switch (explicitLayout) {
                case "empty":
                    emitEmptyElementContent(element, jcc);
                    break;
                case "empty-plus":
                    emitEmptyPlusElementContent(element, jcc);
                    break;
                case "simple":
                    emitSimpleElementContent(element, jcc);
                    break;
                case "simple-plus":
                    emitSimplePlusElementContent(element, jcc);
                    break;
                case "list":
                    if (isSuitableAsList(element)) {
                        emitListElementContent(element, jcc);
                    } else {
                        emitMixedElementContent(element, jcc, false);
                    }
                    break;
                case "list-plus":
                    if (isSuitableAsList(element)) {
                        emitListPlusElementContent(element, jcc, -1);
                    } else {
                        emitMixedElementContent(element, jcc, false);
                    }
                    break;
                case "record":
                    emitRecordElementContent(element, jcc);
                    break;
                case "sequence":
                    emitMixedElementContent(element, jcc, false);
                    break;
                case "mixed":
                    emitMixedElementContent(element, jcc, false);
                    break;
            }
        } else {
            // If the element is schema-validated, use schema information
            SchemaType type = element.getSchemaType();
            if (!(type == AnyType.getInstance() || type == Untyped.getInstance())) {
                if (type instanceof SimpleType) {
                    emitSimpleElementContent(element, jcc);
                } else {
                    ComplexType cType = (ComplexType) type;
                    String layout = cType.getPreferredJsonLayout(jcc.schema);
                    String suffix = "";
                    int sep = layout.indexOf("/");
                    if (sep > 0) {
                        suffix = layout.substring(sep + 1);
                        layout = layout.substring(0, sep);
                    }
                    switch (layout) {
                        case "simple-plus":
                            emitSimplePlusElementContent(element, jcc);
                            break;
                        case "empty":
                            emitEmptyElementContent(element, jcc);
                            break;
                        case "empty-plus":
                            emitEmptyPlusElementContent(element, jcc);
                            break;
                        case "list":
                            emitListElementContent(element, jcc);
                            break;
                        case "list-plus":
                            emitListPlusElementContent(element, jcc, Integer.parseInt(suffix));
                            break;
                        case "record":
                            emitRecordElementContent(element, jcc);
                            break;
                        case "sequence":
                            emitSequenceElementContent(element, jcc);
                            break;
                        case "mixed":
                            emitMixedElementContent(element, jcc, false);
                            break;
                        default:
                            throw new IllegalStateException();

                    }
                }
            } else {
                int properties;
                if (jcc.uniformityData != null) {
                    assert jcc.uniformityData.contains(element.getFingerprint());
                    properties = jcc.uniformityData.get(element.getFingerprint());
                } else {
                    properties = getElementProperties(element, 0);
                }
                // TODO: there must be a simpler way of expressing the following tortuous logic...
                if ((properties & HAS_NON_WHITESPACE_TEXT_NODES) == 0) {
                    if ((properties & HAS_CHILD_ELEMENTS) == 0) {
                        if ((properties & HAS_WHITESPACE_TEXT_NODES) == 0) {
                            if ((properties & HAS_ATTRIBUTES) == 0) {
                                emitEmptyElementContent(element, jcc);
                            } else {
                                emitEmptyPlusElementContent(element, jcc);
                            }
                        } else {
                            if ((properties & HAS_ATTRIBUTES) == 0) {
                                emitSimpleElementContent(element, jcc);
                            } else {
                                emitSimplePlusElementContent(element, jcc);
                            }
                        }
                    } else {
                        if ((properties & HAS_WHITESPACE_TEXT_NODES) != 0 && (properties & HAS_CHILD_ELEMENTS) == 0) {
                            if ((properties & HAS_ATTRIBUTES) == 0) {
                                emitSimpleElementContent(element, jcc);
                            } else {
                                emitSimplePlusElementContent(element, jcc);
                            }
                        } else if ((properties & HAS_DIVERSE_CHILD_ELEMENTS) == 0
                                && ((properties & HAS_REPEATED_CHILD_ELEMENTS) != 0)) {
                            if ((properties & HAS_ATTRIBUTES) == 0) {
                                emitListElementContent(element, jcc);
                            } else {
                                emitListPlusElementContent(element, jcc, properties & NamePool.FP_MASK);
                            }
                        } else if ((properties & HAS_UNIQUE_CHILD_ELEMENTS) != 0) {
                            emitRecordElementContent(element, jcc);
                        } else {
                            emitSequenceElementContent(element, jcc);
                        }
                    }
                } else if ((properties & HAS_CHILD_ELEMENTS) == 0) {
                    if ((properties & HAS_ATTRIBUTES) == 0) {
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

    /**
     * Ask whether list representation is possible for an element. This method
     * is called when list layout is chosen explicitly by the user; if it is
     * chosen inappropriately, we call back to mixed content representation
     * @param element the element in question
     * @return true if it is possible to use list layout for this element
     */
    private boolean isSuitableAsList(NodeInfo element) {
        int props = getElementProperties(element, 0);
        return (props & HAS_DIVERSE_CHILD_ELEMENTS) == 0
                && (props & HAS_CHILD_ELEMENTS) != 0
                && (props & HAS_NON_WHITESPACE_TEXT_NODES) == 0
                && (props & NamePool.FP_MASK) != 0;
    }

    /**
     * Element formatter for empty elements. Element {@code <e/>} becomes JSON <code>{"e":""}</code>
     * @param element the element to be formatted
     * @param jcc the conversion context
     * @throws XPathException if anything goes wrong
     */
    private void emitEmptyElementContent(NodeInfo element, JsonConversionContext jcc) throws XPathException {
        jcc.emitter.writeStringValue("");
    }

    /**
     * Element formatter for empty elements with attributes.
     * Element {@code <e id="x"/>} becomes JSON <code>{"e":{"@id":"x"}}</code>
     *
     * @param element the element to be formatted
     * @param jcc     the conversion context
     * @throws XPathException if anything goes wrong
     */
    private void emitEmptyPlusElementContent(NodeInfo element, JsonConversionContext jcc) throws XPathException {
        jcc.emitter.startMap(false);
        emitAttributes(element, jcc, false);
        jcc.emitter.endMap();
    }

    /**
     * Element formatter for elements with simple content.
     * Element {@code <e>xx</e>} becomes JSON <code>{"e":"xx"}</code>
     *
     * @param element the element to be formatted
     * @param jcc     the conversion context
     * @throws XPathException if anything goes wrong
     */

    private void emitSimpleElementContent(NodeInfo element, JsonConversionContext jcc) throws XPathException {
        SchemaType type = element.getSchemaType();
        boolean asList = type instanceof SimpleType && ((SimpleType)type).isListType();
        emitSequence(element.atomize(), jcc, asList);
    }

    /**
     * Element formatter for elements with simple content plus attributes.
     * Element {@code <e id="aa">xx</e>} becomes JSON <code>{"e":{"@id":"aa","#content":"xx"}}</code>
     *
     * @param element the element to be formatted
     * @param jcc     the conversion context
     * @throws XPathException if anything goes wrong
     */

    private void emitSimplePlusElementContent(NodeInfo element, JsonConversionContext jcc) throws XPathException {
        jcc.emitter.startMap(false);
        emitAttributes(element, jcc, false);
        jcc.emitter.writeKey("#content");
        SchemaType type = element.getSchemaType();
        boolean asList = type instanceof SimpleType && ((SimpleType) type).isListType();
        emitSequence(element.atomize(), jcc, asList);
        jcc.emitter.endMap();
    }

    /**
     * Element formatter for elements with list content.
     * Element {@code <e><x>3<x/><x>4<x/><x>5<x/></e>} becomes
     * JSON <code>{"e":["3","4","5"]}.</code> The child
     * element name is dropped.
     *
     * @param element the element to be formatted
     * @param jcc     the conversion context
     * @throws XPathException if anything goes wrong
     */

    private void emitListElementContent(NodeInfo element,
                                       JsonConversionContext jcc) throws XPathException {

        jcc.emitter.startArray(false);
        for (NodeInfo child : element.children(NodeKindTest.ELEMENT)) {
            emitElementContent(child, jcc);
        }
        jcc.emitter.endArray();
    }

    /**
     * Element formatter for elements with list content plus attributes
     * Element {@code <e id="aa"><x>3<x/><x>4<x/><x>5<x/></e>} becomes
     * JSON <code>{"e":{"@id":"aa","x":["3","4","5"]}}</code>.
     * The child element name is retained.
     *
     * @param element the element to be formatted
     * @param jcc     the conversion context
     * @throws XPathException if anything goes wrong
     */
    private void emitListPlusElementContent(NodeInfo element,
                                           JsonConversionContext jcc,
                                           int childFingerprint) throws XPathException {

        String key;
        if (childFingerprint == -1) {
            NodeInfo firstChild = element.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT).next();
            assert firstChild != null;
            if (firstChild.getNamespaceUri().equals(element.getNamespaceUri())) {
                key = firstChild.getLocalPart();
            } else {
                key = "Q{" + firstChild.getNamespaceUri() + "}" + firstChild.getLocalPart();
            }
        } else {
            StructuredQName qn = jcc.config.getNamePool().getStructuredQName(childFingerprint);
            if (qn.getNamespaceUri().equals(element.getNamespaceUri())) {
                key = qn.getLocalPart();
            } else {
                key = qn.getEQName();
            }
        }

        JSONEmitter emitter = jcc.emitter;
        emitter.startMap(false);
        emitAttributes(element, jcc, false);
        emitter.writeKey(key);
        emitter.startArray(false);
        for (NodeInfo child : element.children()) {
            if (child.getNodeKind() == Type.ELEMENT) {
                emitElementContent(child, jcc);
            }
        }
        emitter.endArray();
        emitter.endMap();
    }

    /**
     * Element formatter for elements with record content (with or without attributes)
     * Element {@code <e id="aa"><x>3<x/><y>4<x/><z>5<x/></e>} becomes
     * JSON <code>{"e":{"@id":"aa","x":"3","y":"4","z":"5"}}</code>.
     * The child element name is retained.
     *
     * @param element the element to be formatted
     * @param jcc     the conversion context
     * @throws XPathException if anything goes wrong
     */
    private void emitRecordElementContent(NodeInfo element, JsonConversionContext jcc) throws XPathException {
        JSONEmitter emitter = jcc.emitter;
        emitter.startMap(false);
        emitAttributes(element, jcc, false);
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
                    emitElementContent(child, jcc);
                }
            }
        }
        emitter.endMap();
    }

    private void emitSequenceElementContent(NodeInfo element, JsonConversionContext jcc) throws XPathException {
        emitMixedElementContent(element, jcc, true);
    }
    private void emitMixedElementContent(NodeInfo element, JsonConversionContext jcc, boolean stripSpace) throws XPathException {

        JSONEmitter emitter = jcc.emitter;
        emitter.startArray(false);
        emitAttributes(element, jcc, true);
        if (element.isNilled()) {
            emitter.writeAtomicValue(null);
        } else {
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
                        emitElement(child, jcc);
                        break;
                    default:
                        throw new IllegalStateException();
                }
            }
        }
        emitter.endArray();
    }

    private void emitAttributes(NodeInfo element, JsonConversionContext jcc, boolean individual) throws XPathException {
        NodeInfo att;
        SequenceIterator iter = element.iterateAxis(AxisInfo.ATTRIBUTE);
        while ((att = (NodeInfo)iter.next()) != null) {
            NamespaceUri ns = att.getNamespaceUri();
            if (!ns.equals(NamespaceUri.SCHEMA_INSTANCE)) {
                if (individual) {
                    jcc.emitter.startMap(true);
                }
                String attKey;
                if (ns.equals(NamespaceUri.NULL)) {
                    attKey = "@" + att.getLocalPart();
                } else if (ns.equals(NamespaceUri.XML)) {
                    attKey = "@xml:" + att.getLocalPart();
                } else {
                    attKey = "@Q{" + att.getNamespaceUri() + "}" + att.getLocalPart();
                }
                jcc.emitter.writeKey(attKey);
                AtomicSequence val = att.atomize();
                SchemaType type = att.getSchemaType();
                boolean asList = ((SimpleType) type).isListType();
                emitSequence(val, jcc, asList);
                if (individual) {
                    jcc.emitter.endMap();
                }
            }
        }
    }


}

// Copyright (c) 2023 Saxonica Limited.

