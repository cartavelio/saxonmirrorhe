package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Whitespace;

/**
 * Represents an XML Schema Facet. Most facets are represented by subclasses of this
 * class, the only exception is whiteSpace.
 */

public abstract class Facet {

    /**
     * Flag to indicate that the facet is fixed (cannot be overridden in a subtype)
     */

    private boolean fixed = false;

    /**
     * Message to be used to report that the facet is violated
     */

    private String message = null;

    /**
     * Say whether the facet is defined as fixed
     *
     * @param fixed true if the facet is fixed
     */

    public void setFixed(boolean fixed) {
        this.fixed = fixed;
    }

    /**
     * Ask whether the facet is defined in the schema with fixed="true"
     *
     * @return true if fixed
     */

    public boolean isFixed() {
        return fixed;
    }

    /**
     * Ask whether this is a constraining facet (as distinct from a pre-lexical facet)
     *
     * @return true if this is a constraining facet
     */

    public boolean isConstraining() {
        return true;
    }

    /**
     * Set the message to be used when this facet is violated. If no message is supplied, a generic
     * message is constructed, for example "the value does not match the pattern XYZ".
     *
     * @param message the message to be used
     */

    public void setMessage(String message) {
        this.message = message;
    }

    /**
     * Get the message to be used when this facet is violated.
     *
     * @return the message, if one has been set, or null otherwise
     */

    /*@Nullable*/
    public String getMessage() {
        return message;
    }

    /**
     * Returns the name of this Facet
     *
     * @return the name of this Facet
     */

    public abstract String getName();

    /**
     * Get the name of the XSD constraint associated with this facet
     * @return the XSD constraint name, for example cvc-maxExclusive-valid
     */

    public String getConstraintName() {
        return "cvc-" + getName() + "-valid";
    }

    /**
     * Returns the lexical representation of the value of this facet
     *
     * @return the value of this facet, as a string
     */

    public abstract UnicodeString getValue();

    /**
     * Returns the numeric value of this facet, for facets where this is appropriate
     *
     * @return a long representation of the value of this facet
     * @throws NumberFormatException if the value is not numeric
     */

    public long toLong() throws NumberFormatException {
        return Long.parseLong(getValue().toString());
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param type     the type on which the facet is defined
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if the facet is not legal
     * @throws net.sf.saxon.type.ValidationException
     *          if a value is found that does not conform to the facet
     */

    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler)
            throws SchemaException, ValidationException {
        // no-op (overridden in subclasses)
    }

    /**
     * Test whether this facet is newly introduced on this type, that is, whether it is absent
     * from the base type
     *
     * @param base the base type
     * @return true if the facet is present on this type and not on the base type
     */

    public boolean isNewlyIntroduced(SimpleType base) {
        Class thisClass = getClass();
        if (base instanceof UserSimpleType) {
            Facet f = ((UserSimpleType) base).getFacet(thisClass);
            return f == null || !getValue().equals(f.getValue());
        }
        return true;
    }

    /**
     * Test whether an atomic value conforms to this facet
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test fails
     */

    public abstract boolean testAtomicValue(AtomicValue value);

    /**
     * Test whether a list value conforms to this facet. This is used only for the enumeration
     * and assertion facets.
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test fails
     */

    public boolean testListValue(AtomicSequence value) {
        throw new UnsupportedOperationException("The " + getName() + " facet is not applicable to list values");
    }

    /**
     * Test whether the length of a list conforms to this facet.
     * Always true except for length facets
     *
     * @param count the actual length of the list
     * @return true if the facet allows a list of this length, otherwise false
     */

    public boolean testLength(int count) {
        return true;
    }

    /**
     * Get the whitespace action required by this facet: one of PRESERVE, REPLACE, or COLLAPSE.
     * Returns a dummy value of PRESERVE for facets other than whitespace facets.
     *
     * @return the whitespace action, by default PRESERVE
     */

    public int getWhitespaceAction() {
        return Whitespace.PRESERVE;
    }

    /**
     * Serialize this facet when externalizing the schema component model
     *
     * @param serializer receives the content for serialization
     * @throws net.sf.saxon.trans.XPathException
     *          if an error occurs writing the value to the output
     */

    public void serializeFacet(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement(getName());
        if (getValue() == null) {
            throw new NullPointerException("*** Facet " + getName() + " has no value");
        }
        serializer.emitAttribute("value", getValue().toString());
        if (isFixed()) {
            serializer.emitAttribute("fixed", "true");
        }
        if (message != null) {
            serializer.emitAttribute("message", message);
        }
        if (this instanceof PatternFacet) {
            String flags = ((PatternFacet) this).getFlags();
            if (flags != null && !flags.isEmpty()) {
                serializer.emitAttribute("flags", flags);
            }
        }
        serializer.endElement();
    }

    /**
     * Test whether a given string is one of the recognized facet names
     *
     * @param name       a candidate facet name, for example "totalDigits"
     * @param xsdVersion either XSD10 or XSD11
     * @return true if the name is recognized as a facet name in the selected version of the
     *         XSD specification
     */


    public static boolean isFacetName(String name, int xsdVersion) {
        return ("assertion".equals(name) && xsdVersion == Configuration.XSD11) ||
                "enumeration".equals(name) ||
                "length".equals(name) ||
                "pattern".equals(name) ||
                "maxExclusive".equals(name) ||
                "minExclusive".equals(name) ||
                "maxInclusive".equals(name) ||
                "minInclusive".equals(name) ||
                "maxLength".equals(name) ||
                "minLength".equals(name) ||
                "maxScale".equals(name) ||
                "minScale".equals(name) ||
                "whiteSpace".equals(name) ||
                "totalDigits".equals(name) ||
                "fractionDigits".equals(name) ||
                ("explicitTimezone".equals(name) && xsdVersion == Configuration.XSD11);

    }

    /**
     * Get a function item that represents this Facet schema component.
     */

    public abstract Function getFacetAsFunctionItem();

    /**
     * Factory method: creates a new Facet with the given name
     *
     * @param name     the name of the Facet
     * @param value    the value of the Facet
     * @param config   the Saxon configuration
     * @param resolver a namespace resolver used for resolving prefixes in namespace-sensitive enumeration values
     * @return the constructed facet
     * @throws net.sf.saxon.type.SchemaException
     *          if an error is found
     */

    public static Facet makeFacet(String name, String value,
                                  EnterpriseConfiguration config,
                                  NamespaceResolver resolver
    ) throws SchemaException {
        switch (name) {
            case "pattern": {
                PatternFacet lf = new PatternFacet(config);
                lf.setValue(value);
                return lf;
            }
            case "enumeration": {
                EnumerationFacet ef = new EnumerationFacet();
                ef.setValue(value);
                // The enumeration value might contain a QName. We don't know yet, so we have to save the
                // namespace context just in case.
                ef.setNamespaceContext(resolver);
                return ef;
            }
            case "length": {
                LengthFacet lf = new LengthFacet();
                lf.setValue(value);
                return lf;
            }
            case "minLength": {
                MinLengthFacet lf = new MinLengthFacet();
                lf.setValue(value);
                return lf;
            }
            case "maxLength": {
                MaxLengthFacet lf = new MaxLengthFacet();
                lf.setValue(value);
                return lf;
            }
            case "minInclusive": {
                ValueRangeFacet vrf = new MinInclusiveFacet();
                vrf.setValue(value);
                return vrf;
            }
            case "maxInclusive": {
                ValueRangeFacet vrf = new MaxInclusiveFacet();
                vrf.setValue(value);
                return vrf;
            }
            case "minExclusive": {
                ValueRangeFacet vrf = new MinExclusiveFacet();
                vrf.setValue(value);
                return vrf;
            }
            case "maxExclusive": {
                ValueRangeFacet vrf = new MaxExclusiveFacet();
                vrf.setValue(value);
                return vrf;
            }
            case "totalDigits": {
                DigitsFacet df = new TotalDigitsFacet();
                df.setValue(value);
                return df;
            }
            case "fractionDigits": {
                DigitsFacet df = new FractionDigitsFacet();
                df.setValue(value);
                return df;
            }
            case "explicitTimezone": {
                ExplicitTimezoneFacet tf = new ExplicitTimezoneFacet();
                tf.setValue(value);
                return tf;
            }
            case "whiteSpace": {
                WhitespaceFacet wf = new WhitespaceFacet();
                wf.setValue(value);
                return wf;
            }
            case "order": {
                OrderFacet of = new OrderFacet();
                of.setValue(value);
                return of;
            }
            case "distinct": {
                DistinctFacet df = new DistinctFacet();
                df.setValue(value);
                return df;
            }
            default:
                throw new SchemaException("Unknown facet: " + name);
        }
    }
}

// Copyright (c) 2004-2022 Saxonica Limited