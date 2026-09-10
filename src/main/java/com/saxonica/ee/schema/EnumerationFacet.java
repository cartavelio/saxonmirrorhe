package com.saxonica.ee.schema;

import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.AtomicValue;

/**
 * Represents a single enumeration facet defined on a simple type. Note that unlike other
 * facets, a value cannot be validated against a single enumeration facet; for this reason
 * all the individual enumeration facets are assembled into an {@link EnumerationFacetSet} which
 * is a pseudo-facet used to perform the validation.
 */
public class EnumerationFacet extends Facet {

    private UnicodeString value;
    private AtomicSequence typedValue;

    /**
     * The enumeration value may be a QName. But we don't necessarily know this at the time we encounter
     * it. So we have to keep the namespace context just in case its a QName.
     */
    NamespaceResolver namespaceContext = null;

    /**
     * Get the name of this facet
     *
     * @return "enumeration"
     */

    @Override
    public String getName() {
        return "enumeration";
    }

    /**
     * Returns the lexical representation of this facet
     *
     * @return the value of this facet
     */

    @Override
    public UnicodeString getValue() {
        return value;
    }

    /**
     * Get the namespace context of the value
     *
     * @return the namespace context from the xs:enumeration element that defined the value
     */

    /*@Nullable*/
    public NamespaceResolver getNamespaceContext() {
        return namespaceContext;
    }

    /**
     * Set the value of this facet
     *
     * @param value the raw enumeration value as written in the schema
     */

    public void setValue(String value) {
        this.value = StringView.of(value).tidy();
    }

    /**
     * Get the typed value of the enumeration value
     *
     * @param type  the simple type containing this facet
     * @param rules the conversion rules for the configuration
     * @return the enumeration value as written, converted to the simple type on which it appears. If the value
     *         is not valid against the type, throws a SchemaException
     * @throws net.sf.saxon.type.SchemaException
     *                                      if the schema is invalid
     * @throws MissingComponentException if an unresolved reference to a schema component is encountered
     */

    public AtomicSequence getTypedValue(SimpleType type, ConversionRules rules) throws SchemaException, MissingComponentException {
        if (typedValue != null) {
            return typedValue;
        }
        try {
            typedValue = type.getTypedValue(getValue(), namespaceContext, rules);
            return typedValue;
        } catch (XPathException e) {
            throw new SchemaException(e);
        }
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     * @throws SchemaException if the facet value is not legal given the base type
     */

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler)
            throws SchemaException, MissingComponentException {
        ConversionRules rules = type.getConfiguration().getConversionRules();
        // The Configuration does not hold the schema components until we have finished processing them,
        // so validation of notations is against the set of notations known to the compiler
        rules = rules.copy();
        rules.setNotationSet(compiler.getPreparedSchema());
        // At this stage we know the primitive type, so we can replace the set of string values with
        // values of the correct target type.
        if (base instanceof UserAtomicType) {
            getTypedValue(base, rules);
            // This checks that the enumeration value is a valid value of the base type.
        } else {
            if (namespaceContext == null && base.isNamespaceSensitive()) {
                throw new SchemaException("No namespace context available for checking enumeration QName");
            }
            ValidationFailure err = base.validateContent((getValue()), namespaceContext, rules);
            if (err != null) {
                throw new SchemaException("Enumeration value " + Err.wrap(value) +
                        " is not permitted by the base type " + base.getDescription() + ": " + err.getMessage());
            }
        }
    }

    /**
     * Save the namespace context for this enumeration value. For internal use only.
     *
     * @param map a NamespaceResolver mapping namespace prefixes (as strings) onto namespace URIs (as strings)
     */

    public void setNamespaceContext(NamespaceResolver map) {
        namespaceContext = map;
    }

    /**
     * Test whether an atomic value conforms to this facet
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test
     *         fails. The implementation of this method is called only while testing whether the enumeration values
     *         themselves conform to the type, so it always returns true.
     */

    @Override
    public boolean testAtomicValue(AtomicValue value) {
        return true;
    }

    @Override
    public FunctionItem getFacetAsFunctionItem() {
        throw new UnsupportedOperationException();
    }

}

// Copyright (c) 2004-2023 Saxonica Limited See Terms and Conditions.