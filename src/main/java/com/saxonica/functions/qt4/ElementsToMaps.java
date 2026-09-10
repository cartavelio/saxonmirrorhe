package com.saxonica.functions.qt4;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.ImmutableArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.*;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.query.QueryResult;
import net.sf.saxon.serialize.JSONEmitter;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.serialize.UnicodeWriterResult;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.transpile.CSharpSimpleEnum;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntSet;
import net.sf.saxon.z.IntToIntHashMap;

import javax.xml.transform.OutputKeys;
import java.util.*;

/**
 * Implementation of the fn:elements-to-maps function proposed for 4.0
 */
public class ElementsToMaps extends SystemFunction {


    @CSharpModifiers(code = {"public", "static"})
    public static OptionsParameter OPTION_DETAILS;
    private final static SequenceType layoutMapType =
            SequenceType.one(new MapType(BuiltInAtomicType.QNAME, SequenceType.SINGLE_STRING));

    static {
        OptionsParameter options = new OptionsParameter(40);
        options.addAllowedOption("attribute-marker",
                                 SequenceType.SINGLE_STRING, new StringValue("@"));
        options.addAllowedOption("uniform",
                                 SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        options.addAllowedOption("name-format",
                                 SequenceType.one(EnumerationUnionType.of("local", "lexical", "eqname", "default")));
        options.addAllowedOption("layouts",
                                 layoutMapType, EmptyMap.getInstance());
        options.addAllowedOption("disable-layouts",
                                 SequenceType.zeroOrMore(EnumerationUnionType.of(
                                         "empty", "empty-plus", "simple", "simple-plus", "list", "list-plus", "record", "sequence")));
        OPTION_DETAILS = options;
    }

    @CSharpSimpleEnum
    public enum NameFormat {LEXICAL, LOCAL, DEFAULT, EQNAME}

    /**
     * A bundle of context data that is passed down the recursive calls
     */

    private static class ConversionContext {
        public Configuration config;
        public Schema schema;
        public IntToIntHashMap uniformityData = null;
        public IntHashMap<String> layoutMap = new IntHashMap<>();
        public Set<String> disabledLayouts = Collections.emptySet();
        public NameFormat nameFormat = NameFormat.DEFAULT;
        public String attributeMarker = "@";
        public Stack<StructuredQName> elementNameStack = new Stack<>();

        /**
         * Construct a string used as a map key in the output, to represent an element
         * or attribute name in the input
         * @param node the element or attribute node in question
         * @return a string to be used to represent the name of the node
         */
        public String makeName(NodeInfo node) {
            String marker = node.getNodeKind() == Type.ATTRIBUTE ? attributeMarker : "";
            if (node.getNodeKind() == Type.ATTRIBUTE && node.getNamespaceUri() == NamespaceUri.XML) {
                return marker + node.getDisplayName();
            }
            switch (nameFormat) {
                case LOCAL:
                    return marker + node.getLocalPart();
                case LEXICAL:
                    return marker + node.getDisplayName();
                case EQNAME:
                    if (node.getNamespaceUri() == NamespaceUri.NULL) {
                        return marker + node.getLocalPart();
                    } else {
                        return marker + node.getQName().getEQName();
                    }
                case DEFAULT:
                default:
                    if ((node.getNodeKind() == Type.ATTRIBUTE && node.getNamespaceUri() == NamespaceUri.NULL)
                            || node.getNamespaceUri() == elementNameStack.peek().getNamespaceUri()) {
                        return marker + node.getLocalPart();
                    } else {
                        return marker + node.getQName().getEQName();
                    }

            }
        }

        /**
         * Format the value of an attribute, an element with a simple type, or an element with a complex
         * type having simple content
         * @param node the element or attribute node
         * @return a representation of the value. For an untyped node this will be a simple atomic item
         * of type xs:untypedAtomic.
         */
        public GroundedValue makeSimpleContent(NodeInfo node) {
            SchemaType type = node.getSchemaType();
            SimpleType st;
            if (type.isSimpleType()) {
                st = (SimpleType) type;
            } else if (type.isComplexType() && ((ComplexType)type).isSimpleContent()) {
                st = ((ComplexType) type).getSimpleContentType();
            } else {
                return new StringValue(node.getStringValue());
            }
            if (st instanceof AtomicType) {
                try {
                    return node.atomize();
                } catch (XPathException e) {
                    return new StringValue(node.getStringValue());
                }
            } else {
                // List type
                try {
                    return ImmutableArrayItem.from(node.atomize().iterate());
                } catch (XPathException e) {
                    return new StringValue(node.getStringValue());
                }
            }

        }
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

        ConversionContext cc = new ConversionContext();
        cc.config = context.getConfiguration();
        cc.schema = getRetainedStaticContext().getPackageData().getImportedSchema();
        // Initialise the element name stack with a dummy no-namespace name
        cc.elementNameStack.push(new StructuredQName("", NamespaceUri.NULL, "dummy"));
        // Process the supplied options
        Map<String, GroundedValue> checkedOptions;
        boolean uniform = false;
        if (arguments.length > 1) {
            MapItem suppliedOptions = (MapItem) arguments[1].head();
            checkedOptions = getDetails().optionDetails.processSuppliedOptions(suppliedOptions, context);
            uniform = ((BooleanValue) checkedOptions.get("uniform")).getBooleanValue();
            cc.attributeMarker = ((StringValue) checkedOptions.get("attribute-marker")).getStringValue();
            GroundedValue formatValue = checkedOptions.get("name-format");
            if (formatValue != null) {
                switch (formatValue.getStringValue()) {
                    case "lexical":
                        cc.nameFormat = NameFormat.LEXICAL;
                        break;
                    case "local":
                        cc.nameFormat = NameFormat.LOCAL;
                        break;
                    case "eqname":
                        cc.nameFormat = NameFormat.EQNAME;
                        break;
                    case "default":
                    default:
                        cc.nameFormat = NameFormat.DEFAULT;
                        break;
                }
            }
            GroundedValue disabledLayouts = checkedOptions.get("disable-layouts");
            if (disabledLayouts != null) {
                cc.disabledLayouts = new HashSet<>();
                for (Item d : disabledLayouts.asIterable()) {
                    cc.disabledLayouts.add(d.getStringValue());
                }
            }
            initLayoutMap(cc, checkedOptions, context.getConfiguration());
        }

        Sequence val = arguments[0];
        
        if (uniform) {
            val = val.materialize();
            cc.uniformityData = gatherUniformityData((GroundedValue)val);
        }

        SequenceIterator in = val.iterate();
        ItemMapper mapper = ItemMapper.of(item -> processElement((NodeInfo) item, cc));
        return new LazySequence(new ItemMappingIterator(in, mapper, true));
    }

    /**
     * Process the supplied value of the {@code layouts} option: a map of specific layouts
     * chosen for specific element names
     * @param cc the conversion context
     * @param checkedOptions the supplied options to the function, after validation and expansion of defaults
     * @param config the Saxon configuration
     */
    private void initLayoutMap(ConversionContext cc, Map<String, GroundedValue> checkedOptions, Configuration config) {
        MapItem val = (MapItem)checkedOptions.get("layouts").materialize();
        for (KeyValuePair pair : val.keyValuePairs()) {
             StructuredQName qn = ((QNameValue)pair.key).getStructuredQName();
             int fp = config.getNamePool().allocateFingerprint(qn.getNamespaceUri(), qn.getLocalPart());
             cc.layoutMap.put(fp, ((StringValue)pair.value).getStringValue());
        }
    }

    /**
     * Examine all the items in the input sequence, together with their descendants,
     * to gather for each distinct element name, the properties
     * that are common to all the elements of any given name
     * @param input the input sequence to the function
     * @return a hash map, indexed by element name fingerprint, containing the properties
     * applicable to each element name encountered.
     */

    private IntToIntHashMap gatherUniformityData(GroundedValue input) {
        IntToIntHashMap data = new IntToIntHashMap();
        for (Item it : input.asIterable()) {
            AxisIterator iter = ((NodeInfo) it).iterateAxis(AxisInfo.DESCENDANT_OR_SELF, NodeKindTest.ELEMENT);
            NodeInfo element;
            while ((element = iter.next()) != null) {
                updateUniformityData(element, data);
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

    private MapItem processElement(NodeInfo element, ConversionContext cc) throws XPathException {
        // First see if an explicit layout has been chosen for this element in the supplied options
        String explicitLayout = cc.layoutMap.get(element.getFingerprint());
        if (explicitLayout != null) {
            switch (explicitLayout) {
                case "empty":
                    return makeEmptyElementContent(element, cc);
                case "empty-plus":
                    return makeEmptyPlusElementContent(element, cc);
                case "simple":
                    return makeSimpleElementContent(element, cc);
                case "simple-plus":
                    return makeSimplePlusElementContent(element, cc);
                case "list":
                    if (isSuitableAsList(element)) {
                        return makeListElementContent(element, cc);
                    } else {
                        return makeMixedElementContent(element, cc, false);
                    }
                case "list-plus":
                    if (isSuitableAsList(element)) {
                        return makeListPlusElementContent(element, cc, -1);
                    } else {
                        return makeMixedElementContent(element, cc, false);
                    }
                case "record":
                    return makeRecordElementContent(element, cc);
                case "sequence":
                    return makeMixedElementContent(element, cc, true);
                case "mixed":
                    return makeMixedElementContent(element, cc, false);
                case "xml":
                    return makeXmlElementContent(element, cc);
                default:
                    throw new XPathException("Invalid layout selected: " + explicitLayout, "XPTY0004");
            }
        } else {
            // If the element is schema-validated, use schema information
            SchemaType type = element.getSchemaType();
            if (!(type == AnyType.getInstance() || type == Untyped.getInstance())) {
                if (type instanceof SimpleType) {
                    // Rule 7b
                    if (!cc.disabledLayouts.contains("empty") && ((SimpleType)type).isZeroLength()) {
                        return makeEmptyElementContent(element, cc);
                    }
                    if (!cc.disabledLayouts.contains("simple")) {
                        return makeSimpleElementContent(element, cc);
                    }
                    return makeMixedElementContent(element, cc, false);
                } else {
                    // Rule 7c
                    ComplexType cType = (ComplexType) type;
                    String layout = cType.getPreferredJsonLayout(cc.schema, cc.disabledLayouts);
                    String suffix = "";
                    int sep = layout.indexOf("/");
                    if (sep > 0) {
                        suffix = layout.substring(sep + 1);
                        layout = layout.substring(0, sep);
                    }
                    switch (layout) {
                        case "simple-plus":
                            return makeSimplePlusElementContent(element, cc);
                        case "empty":
                            return makeEmptyElementContent(element, cc);
                        case "empty-plus":
                            return makeEmptyPlusElementContent(element, cc);
                        case "list":
                            return makeListElementContent(element, cc);
                        case "list-plus":
                            return makeListPlusElementContent(element, cc, Integer.parseInt(suffix));
                        case "record":
                            return makeRecordElementContent(element, cc);
                        case "sequence":
                            return makeSequenceElementContent(element, cc);
                        case "mixed":
                            return makeMixedElementContent(element, cc, false);
                        default:
                            throw new IllegalStateException();

                    }
                }
            } else {
                int properties;
                if (cc.uniformityData != null) {
                    assert cc.uniformityData.contains(element.getFingerprint());
                    properties = cc.uniformityData.get(element.getFingerprint());
                } else {
                    properties = getElementProperties(element, 0);
                }


                if ((properties & (HAS_CHILD_ELEMENTS | HAS_WHITESPACE_TEXT_NODES | HAS_NON_WHITESPACE_TEXT_NODES)) == 0) {
                    // Rule 6a
                    if ((properties & HAS_ATTRIBUTES) == 0 && !cc.disabledLayouts.contains("empty")) {
                        return makeEmptyElementContent(element, cc);
                    }
                    if (!cc.disabledLayouts.contains("empty-plus")) {
                        return makeEmptyPlusElementContent(element, cc);
                    }
                }
                if ((properties & HAS_CHILD_ELEMENTS) == 0) {
                    // Rule 6b
                    if ((properties & HAS_ATTRIBUTES) == 0 && !cc.disabledLayouts.contains("simple")) {
                        return makeSimpleElementContent(element, cc);
                    }
                    if (!cc.disabledLayouts.contains("simple-plus")) {
                        return makeSimplePlusElementContent(element, cc);
                    }
                }
                if ((properties & HAS_NON_WHITESPACE_TEXT_NODES) == 0) {
                    // Rule 6c
                    if ((properties & HAS_DIVERSE_CHILD_ELEMENTS) == 0
                            && ((properties & HAS_REPEATED_CHILD_ELEMENTS) != 0)) {
                        // Rule 6c(i)
                        if ((properties & HAS_ATTRIBUTES) == 0 && !cc.disabledLayouts.contains("list")) {
                            return makeListElementContent(element, cc);
                        }
                        if (!cc.disabledLayouts.contains("list-plus")) {
                            return makeListPlusElementContent(element, cc, properties & NamePool.FP_MASK);
                        }
                    }
                    if ((properties & HAS_UNIQUE_CHILD_ELEMENTS) != 0 && !cc.disabledLayouts.contains("record")) {
                        // Rule 6c(ii)
                        return makeRecordElementContent(element, cc);
                    }
                    if (!cc.disabledLayouts.contains("sequence")) {
                        // Rule 6c(iii)
                        return makeSequenceElementContent(element, cc);
                    }
                }
                // Rule 6d
                return makeMixedElementContent(element, cc, false);

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
     *
     * @param element the element to be formatted
     * @param cc     the conversion context
     */
    private MapItem makeEmptyElementContent(NodeInfo element, ConversionContext cc) {
        return new SingleEntryMap(new StringValue(cc.makeName(element)),
                                  element.isNilled() ? JSONEmitter.JSON_NULL : StringValue.EMPTY_STRING);
    }

    /**
     * Element formatter for empty elements with attributes.
     * Element {@code <e id="x"/>} becomes JSON <code>{"e":{"@id":"x"}}</code>
     *
     * @param element the element to be formatted
     * @param cc     the conversion context
     */
    private MapItem makeEmptyPlusElementContent(NodeInfo element, ConversionContext cc) {
        StringMapBuilder innerMap = new StringMapBuilder().unordered();
        addAttributes(element, cc, innerMap);
        if (element.isNilled()) {
            innerMap.put(new Twine8("#content"), JSONEmitter.JSON_NULL);
        }
        return new SingleEntryMap(new StringValue(cc.makeName(element)),
                                  innerMap.getCompletedMapConfidently());
    }

    /**
     * Element formatter for elements with simple content.
     * Element {@code <e>xx</e>} becomes JSON <code>{"e":"xx"}</code>
     *
     * @param element the element to be formatted
     * @param jcc     the conversion context
     */

    private MapItem makeSimpleElementContent(NodeInfo element, ConversionContext jcc) {
        return new SingleEntryMap(new StringValue(jcc.makeName(element)),
                                  element.isNilled() ? JSONEmitter.JSON_NULL : jcc.makeSimpleContent(element));
    }

    /**
     * Element formatter for elements with simple content plus attributes.
     * Element {@code <e id="aa">xx</e>} becomes JSON <code>{"e":{"@id":"aa","#content":"xx"}}</code>
     *
     * @param element the element to be formatted
     * @param cc     the conversion context
     */

    private MapItem makeSimplePlusElementContent(NodeInfo element, ConversionContext cc) {
        StringMapBuilder innerMap = new StringMapBuilder().unordered();
        addAttributes(element, cc, innerMap);
        innerMap.put(new Twine8("#content"),
                     element.isNilled() ? JSONEmitter.JSON_NULL : cc.makeSimpleContent(element));
        return new SingleEntryMap(new StringValue(cc.makeName(element)),
                                  innerMap.getCompletedMapConfidently());
    }

    private static void addAttributes(NodeInfo element, ConversionContext cc, StringMapBuilder map) {
        AxisIterator atts = element.iterateAxis(AxisInfo.ATTRIBUTE);
        for (NodeInfo att; (att = atts.next()) != null; ) {
            if (att.getNamespaceUri() != NamespaceUri.SCHEMA_INSTANCE) {
                map.put(StringView.of(cc.makeName(att)), cc.makeSimpleContent(att));
            }
        }
    }

    /**
     * Element formatter for elements with list content.
     * Element {@code <e><x>3<x/><x>4<x/><x>5<x/></e>} becomes
     * JSON <code>{"e":["3","4","5"]}.</code> The child
     * element name is dropped.
     *
     * @param element the element to be formatted
     * @param cc     the conversion context
     * @throws XPathException if anything goes wrong
     */

    private MapItem makeListElementContent(NodeInfo element,
                                       ConversionContext cc) throws XPathException {
        if (element.isNilled()) {
            return new SingleEntryMap(new StringValue(cc.makeName(element)), JSONEmitter.JSON_NULL);
        }
        List<GroundedValue> list = new ArrayList<>();
        cc.elementNameStack.push(element.getQName());
        for (NodeInfo child : element.children(NodeKindTest.ELEMENT)) {
            MapItem item = processElement(child, cc);
            for (KeyValuePair pair : item.keyValuePairs()) {
                list.add(pair.value);
                break;
            }
        }
        cc.elementNameStack.pop();
        return new SingleEntryMap(new StringValue(cc.makeName(element)), new SimpleArrayItem(list));
    }

    /**
     * Element formatter for elements with list content plus attributes
     * Element {@code <e id="aa"><x>3<x/><x>4<x/><x>5<x/></e>} becomes
     * JSON <code>{"e":{"@id":"aa","x":["3","4","5"]}}</code>.
     * The child element name is retained.
     *
     * @param element the element to be formatted
     * @param cc     the conversion context
     * @throws XPathException if anything goes wrong
     */
    private MapItem makeListPlusElementContent(NodeInfo element,
                                           ConversionContext cc,
                                           int childFingerprint) throws XPathException {
        MapItem innerMap = new HashTrieMap();
        AxisIterator atts = element.iterateAxis(AxisInfo.ATTRIBUTE);
        for (NodeInfo att; (att = atts.next()) != null; ) {
            if (att.getNamespaceUri() != NamespaceUri.SCHEMA_INSTANCE) {
                innerMap = innerMap.put(new StringValue(cc.makeName(att)), cc.makeSimpleContent(att));
            }
        }
        if (element.isNilled()) {
            return new SingleEntryMap(new StringValue("#content"), JSONEmitter.JSON_NULL);
        }
        cc.elementNameStack.push(element.getQName());
        List<GroundedValue> list = new ArrayList<>();
        StringValue innerKey = null;
        for (NodeInfo child : element.children(NodeKindTest.ELEMENT)) {
            if (innerKey == null) {
                innerKey = new StringValue(cc.makeName(child));
            }
            MapItem item = processElement(child, cc);
            for (KeyValuePair pair : item.keyValuePairs()) {
                list.add(pair.value);
                break;
            }
        }
        if (innerKey != null) {
            innerMap = innerMap.put(innerKey, new SimpleArrayItem(list));
        } else if (childFingerprint > 0) {
            Orphan e = new Orphan(cc.config);
            e.setNodeKind(Type.ELEMENT);
            e.setNodeName(new FingerprintedQName(cc.config.getNamePool().getStructuredQName(childFingerprint)));
            innerMap = innerMap.put(new StringValue(cc.makeName(e)), SimpleArrayItem.EMPTY_ARRAY);
        }
        cc.elementNameStack.pop();
        return new SingleEntryMap(new StringValue(cc.makeName(element)), innerMap);
    }

    /**
     * Element formatter for elements with record content (with or without attributes)
     * Element {@code <e id="aa"><x>3<x/><y>4<x/><z>5<x/></e>} becomes
     * JSON <code>{"e":{"@id":"aa","x":"3","y":"4","z":"5"}}</code>.
     * The child element name is retained.
     *
     * @param element the element to be formatted
     * @param cc     the conversion context
     * @throws XPathException if anything goes wrong
     */
    private MapItem makeRecordElementContent(NodeInfo element, ConversionContext cc) throws XPathException {
        cc.elementNameStack.push(element.getQName());
        MapItem innerMap = new HashTrieMap();
        AxisIterator atts = element.iterateAxis(AxisInfo.ATTRIBUTE);
        for (NodeInfo att; (att = atts.next()) != null; ) {
            if (att.getNamespaceUri() != NamespaceUri.SCHEMA_INSTANCE) {
                innerMap = addCombiningDuplicates(innerMap, new StringValue(cc.makeName(att)), cc.makeSimpleContent(att));
            }
        }
        if (element.isNilled()) {
            innerMap = innerMap.put(new StringValue("#content"), EmptySequence.getInstance());
        } else {
            for (NodeInfo child : element.children()) {
                if (child.getNodeKind() == Type.ELEMENT) {
                    MapItem entry = processElement(child, cc);
                    assert entry != null;
                    for (KeyValuePair pair : entry.keyValuePairs()) {
                        innerMap = addCombiningDuplicates(innerMap, pair.key, pair.value);
                    }
                }
            }
        }
        cc.elementNameStack.pop();
        return new SingleEntryMap(new StringValue(cc.makeName(element)), innerMap);
    }

    private MapItem addCombiningDuplicates(MapItem map, AtomicValue key, GroundedValue value) {
        GroundedValue existing = map.get(key);
        if (existing == null) {
            return map.put(key, value);
        } else if (existing instanceof ArrayItem) {
            ArrayItem extended = ((ArrayItem)existing).append(value);
            return map.put(key, extended);
        } else {
            ArrayItem extended = SimpleArrayItem.of(existing, value);
            return map.put(key, extended);
        }
    }

    private MapItem makeSequenceElementContent(NodeInfo element, ConversionContext cc) throws XPathException {
        return makeMixedElementContent(element, cc, true);
    }
    private final static Shape PI_SHAPE = new Shape(new Twine8("#target"), new Twine8("#data"));

    private MapItem makeMixedElementContent(NodeInfo element, ConversionContext cc, boolean stripSpace) throws XPathException {
        cc.elementNameStack.push(element.getQName());
        List<GroundedValue> list = new ArrayList<>();
        AxisIterator atts = element.iterateAxis(AxisInfo.ATTRIBUTE);
        for (NodeInfo att; (att = atts.next()) != null; ) {
            if (att.getNamespaceUri() != NamespaceUri.SCHEMA_INSTANCE) {
                list.add(new SingleEntryMap(new StringValue(cc.makeName(att)), cc.makeSimpleContent(att)));
            }
        }
        if (element.isNilled()) {
            list.add(new SingleEntryMap(new StringValue("#content"), JSONEmitter.JSON_NULL));
        } else {
            for (NodeInfo child : element.children()) {
                switch (child.getNodeKind()) {
                    case Type.TEXT:
                        if (!(stripSpace && Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                            list.add(new StringValue(child.getStringValue()));
                        }
                        break;
                    case Type.COMMENT:
                        list.add(new SingleEntryMap(new StringValue("#comment"), new StringValue(child.getStringValue())));
                        break;
                    case Type.PROCESSING_INSTRUCTION:
                        ShapedMap inner = new ShapedMap(PI_SHAPE,
                                                        new StringValue(child.getLocalPart(), BuiltInAtomicType.NCNAME),
                                                        new StringValue(child.getStringValue()));
                        list.add(new SingleEntryMap(new StringValue("#processing-instruction"), inner));
                        break;
                    case Type.ELEMENT:
                        list.add(processElement(child, cc));
                        break;
                    default:
                        throw new IllegalStateException();
                }
            }
        }

        cc.elementNameStack.pop();
        return new SingleEntryMap(new StringValue(cc.makeName(element)), new SimpleArrayItem(list));
    }

    /**
     * Element formatter for elements with record content (with or without attributes)
     * Element {@code <e id="aa"><x>3<x/><y>4<x/><z>5<x/></e>} becomes
     * JSON <code>{"e":{"@id":"aa","x":"3","y":"4","z":"5"}}</code>.
     * The child element name is retained.
     *
     * @param element the element to be formatted
     * @param cc      the conversion context
     * @throws XPathException if anything goes wrong
     */
    private MapItem makeXmlElementContent(NodeInfo element, ConversionContext cc) throws XPathException {
        UnicodeBuilder builder = new UnicodeBuilder();
        UnicodeWriterResult result = new UnicodeWriterResult(builder, null);
        SerializationProperties params = new SerializationProperties();
        params.setProperty(OutputKeys.METHOD, "xml");
        params.setProperty(OutputKeys.INDENT, "no");
        params.setProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
        QueryResult.serialize(element, result, params);
        return new SingleEntryMap(new StringValue(cc.makeName(element)), new StringValue(builder.toUnicodeString()));
    }


}

// Copyright (c) 2024 Saxonica Limited.

