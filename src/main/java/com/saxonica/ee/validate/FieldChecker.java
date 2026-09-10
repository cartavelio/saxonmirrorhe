package com.saxonica.ee.validate;

import com.saxonica.ee.schema.EnumerationFacetSet;
import com.saxonica.ee.schema.UserAtomicType;
import com.saxonica.ee.stream.om.FleetingNode;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.str.UnicodeBuilder;

/**
 * A FieldChecker is instantiated when a field in an identity constraint is matched; it receives
 * notification of all events until the end of the element (or attribute) that matched the selector.
 */

public class FieldChecker extends AbstractReceiver {

    private final SelectorWatch parentWatch;
    private NamespaceResolver namespaceResolver;
    private final AtomicSequence[] currentRow;
    private final int column;
    private final boolean matchElements;
    private boolean active = false;
    private boolean nillableElement = false;
    private NodeName elementName;
    private SchemaType elementType;
    private final UnicodeBuilder buffer = new UnicodeBuilder();
    private Location textLocationId = null;
    int depth = 0;

    /**
     * Create a new FieldChecker
     *
     * @param parentWatch The SelectorWatch to which field values should be
     *                    notified
     * @param selection   The selection (that is, the restricted XPath expression)
     *                    used in this field of the constraint
     * @param currentRow  represents the selected node whose fields are being
     *                    evaluated
     * @param column      sequence number for this field within the constraint being
     *                    evaluated
     */

    public FieldChecker(SelectorWatch parentWatch,
                        Pattern selection,
                        AtomicSequence[] currentRow,
                        int column) {
        this.parentWatch = parentWatch;
        this.currentRow = currentRow;
        this.column = column;
        matchElements = selection.getUType().overlaps(UType.ELEMENT);
    }

    /**
     * Set a NamespaceResolver for use by the FieldChecker
     *
     * @param resolver the namespace resolver to be used
     */

    public void setNamespaceResolver(NamespaceResolver resolver) {
        namespaceResolver = resolver;
    }

    /**
     * Notify the start of an element
     * @param nameCode   integer code identifying the name of the element within the name pool.
     * @param typeCode   integer code identifying the element's type within the name pool. The value -1
     *                   indicates the default type, xs:untyped.
     * @param attributes
     * @param location  location of the start tag
     * @param properties bit-significant properties of the element node. If there are no relevant
     *                   properties, zero is supplied. The definitions of the bits are in class {@link ReceiverOption}
     */

    @Override
    public void startElement(NodeName nameCode, SchemaType typeCode,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        nillableElement = false;
        if (matchElements) {
            if (depth > 0) {
                ValidationFailure ve = new ValidationFailure(
                        "An element participating as a field in a constraint cannot have child elements");
                ve.setConstraintReference(1, "cvc-identity-constraint", "3");
                throw ve.makeException();
            }
            elementType = typeCode;
            elementName = nameCode;
            active = true;
            buffer.clear();
        }
        depth++;
    }

    /**
     * Notify an attribute that matches the selection. Attributes are notified after the startElement event, and before any
     * children. Namespaces and attributes may be intermingled.
     *
     * @param attribute  the attribute node being notified
     * @param locationId the location for error reporting purposes
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs
     */

    public void notifySelectedAttribute(FleetingNode attribute, Location locationId) throws XPathException {
        //if (depth==1 && matchAttributes && selection.matchesAttribute(nameCode & 0xfffff)) {
        AtomicSequence val;
        final Configuration config = getConfiguration();
        ConversionRules rules = config.getConversionRules();
        SimpleType type = (SimpleType) attribute.getSchemaType();
        UnicodeString value = attribute.getUnicodeStringValue();
        if (type.equals(BuiltInAtomicType.UNTYPED_ATOMIC)) {
            // See MS test case idZ015 and bug 4063. If the attribute has been skip-validated
            // then it doesn't qualify as a member of the key-sequence of the node according to the XSD 1.1
            // spec. So ignore it.
            // val = StringValue.untypedAtomic(value);
        } else {
            try {
                if (type.isNamespaceSensitive()) {
                    try {
                        val = type.getTypedValue(value, namespaceResolver, rules);
                    } catch (ValidationException e) {
                        // Error already reported, we assume
                        val = null;
                    }
                } else if (type.isAtomicType()) {
                    // We only need an instance of the primitive type for ID checking; checking against facets
                    // is done separately.
                    AtomicType prim = (AtomicType)((AtomicType)type).getPrimitiveItemType();
                    StringConverter converter = prim.getStringConverter(getConfiguration().getConversionRules());
                    ConversionResult result = converter.convertString(value);
                    if (result instanceof ValidationFailure) {
                        val = null;
                    } else {
                        val = result.asAtomic();
                    }
                    if (type instanceof UserAtomicType) {
                        // To reduce memory use, use a shared instance where possible
                        EnumerationFacetSet en = (EnumerationFacetSet)((UserAtomicType)type).getFacet(EnumerationFacetSet.class);
                        if (en != null) {
                            val = en.getSharedInstance(val);
                        }
                    }
                } else {
                    try {
                        val = type.getTypedValue(value, namespaceResolver, rules);
                    } catch (ValidationException e) {
                        val = null;
                    }
                }
            } catch (XPathException e) {
                throw new ValidationException(e);
            }
            if (val != null) {
                parentWatch.addFieldValue(currentRow, column, val, false, locationId);
            }
        }
    }

    /**
     * Notify character data. Note that some receivers may require the character data to be
     * sent in a single event, but in general this is not a requirement.
     * @param chars      The characters
     * @param locationId an integer which can be interpreted using a LocationMap to return
     *                   information such as line number and system ID. If no location information is available,
     *                   the value zero is supplied.
     * @param properties Bit significant value. The following bits are defined:
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        // buffer the value, because it may be split by comments and PIs
        //System.err.println("FieldChecker " + this + ".characters() " + chars);
        if (active && matchElements && depth == 1) {
            buffer.accept(chars);
        }
        nillableElement = ReceiverOption.contains(properties, ReceiverOption.NILLABLE_ELEMENT);
        textLocationId = locationId;
    }

    @Override
    public void endElement() throws XPathException {
        if (active && matchElements && depth == 1) {
            AtomicSequence val;
            SchemaType eltype = elementType;
            UnicodeString fieldVal = buffer.toUnicodeString();
            if (eltype == null || eltype instanceof AnySimpleType) {
                val = StringValue.makeUntypedAtomic(fieldVal); // force to String because buffer is mutable
            } else {
                SchemaType contentType = eltype;
                if (eltype instanceof ComplexType) {
                    if (((ComplexType) eltype).isSimpleContent()) {
                        // The Schema 1.0 spec actually says it must be a simple type. But that's a bug (#5780, #10236),
                        // and has been fixed in 1.1. Other vendors allow a complex type with simple content.
                        // See Microsoft test case idH031
                        contentType = ((ComplexType) eltype).getSimpleContentType();
                    } else {
                        ValidationFailure err = new ValidationFailure(
                                "The value of a field participating in an identity constraint must have a simple type," +
                                        " or a complex type with simple content. Element " +
                                        elementName.getDisplayName() + " has type " + eltype.getDescription());
                        err.setConstraintReference(1, "cvc-identity-constraint", "3");
                        throw err.makeException();
                    }
                }
                assert contentType != null;
                if (contentType.getFingerprint() == StandardNames.XS_STRING) {
                    // fast path for a commmon case
                    val = StringValue.makeStringValue(buffer.toString()); // force to String because buffer is mutable
                } else if (((SimpleType) contentType).isNamespaceSensitive()) {
                    try {
                        val = ((SimpleType) contentType).getTypedValue(
                                fieldVal, namespaceResolver, getConfiguration().getConversionRules());
                    } catch (ValidationException e) {
                        // Presumably we've already reported the value as invalid; now we just need to avoid crashing
                        val = null;
                    }
                } else if (contentType.isAtomicType()) {
                    // We've already validated the value, and for equality matching we only need a value of the
                    // correct primitive type.
                    AtomicType prim = (AtomicType)((AtomicType)contentType).getPrimitiveItemType();
                    StringConverter converter = prim.getStringConverter(getConfiguration().getConversionRules());
                    ConversionResult result = converter.convertString(fieldVal);
                    if (result instanceof ValidationFailure) {
                        // Presumably we've already reported the value as invalid; now we just need to avoid crashing
                        val = null;
                    } else {
                        val = result.asAtomic();
                    }
                    if (contentType instanceof UserAtomicType) {
                        // To reduce memory use, use a shared instance where possible
                        EnumerationFacetSet en = (EnumerationFacetSet)((UserAtomicType)contentType).getFacet(EnumerationFacetSet.class);
                        if (en != null) {
                            val = en.getSharedInstance(val);
                        }
                    }
                } else {
                    // a list or union type
                    try {
                        val = ((SimpleType) contentType).getTypedValue(
                                fieldVal, namespaceResolver, getConfiguration().getConversionRules());
                    } catch (ValidationException e) {
                        val = null;
                    }
                }
            }
            if (val != null) {
                parentWatch.addFieldValue(currentRow, column, val, nillableElement, textLocationId);
            }
        }
        depth--;
    }

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope.
     */

    @Override
    public void close() {
        active = false;
    }

    /**
     * Ask whether this Receiver (or the downstream pipeline) makes any use of the type annotations
     * supplied on element and attribute events
     *
     * @return true if the Receiver makes any use of this information. If false, the caller
     *         may supply untyped nodes instead of supplying the type annotation
     */

    @Override
    public boolean usesTypeAnnotations() {
        return true;
    }
}

// Copyright (c) 2004-2022 Saxonica Limited No warranty.
