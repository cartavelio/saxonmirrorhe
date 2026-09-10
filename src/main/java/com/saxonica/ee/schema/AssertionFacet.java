package com.saxonica.ee.schema;

import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;



/**
 * Represents an assertion facet on a schema-defined simple type.
 */
public class AssertionFacet extends Facet {

    Assertion assertion;

    /**
     * Create a assertion facet
     *
     * @param assertion the assertion
     */

    public AssertionFacet(Assertion assertion) {
        this.assertion = assertion;
    }

    @Override
    public String getName() {
        return "assertion";
    }

    @Override
    public String getConstraintName() {
        return "cvc-assertions-valid";
    }

    @Override
    public UnicodeString getValue() {
        return StringView.tidy(assertion.getConditionText());
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param type     the derived type
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     */

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) {
        // no restrictions
    }

    /**
     * Test whether an atomic value conforms to this facet
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test
     *         fails
     */

    @Override
    public boolean testAtomicValue(AtomicValue value) {
        throw new UnsupportedOperationException("Validation context required");
    }

    /**
     * Test whether an atomic value conforms to this facet
     *
     * @param value             the value to be tested
     * @param validationContext includes values of validation parameters (as declared using saxon:param)
     * @return true if the value conforms; false if it doesn't conform, or if the test
     *         fails
     */

    public boolean testAtomicValue(AtomicValue value, ConversionRules validationContext) {
        return assertion.testSimple(value, validationContext);
    }


    /**
     * Test whether a list value conforms to this facet. This is used only for the enumeration
     * and assertion facets.
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test fails
     */

    @Override
    public boolean testListValue(AtomicSequence value) {
        throw new UnsupportedOperationException("Validation context required");
    }

    /**
     * Test whether a list value conforms to this facet. This is used only for the enumeration
     * and assertion facets.
     *
     * @param value the value to be tested
     * @param rules context information including values of validation parameters
     * @return true if the value conforms; false if it doesn't conform, or if the test fails
     */

    public boolean testListValue(AtomicSequence value, ConversionRules rules) {
        return assertion.testSimple(value, rules);
    }


    /**
     * Serialize this facet
     *
     * @param serializer receives the content for serialization
     */

    @Override
    public void serializeFacet(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement("assert");
        if (isFixed()) {
            serializer.emitAttribute("fixed", "true");
        }
        assertion.serialize(serializer);
        serializer.endElement();
    }

    @Override
    public FunctionItem getFacetAsFunctionItem() {
        // Contrary to the W3C SCM model, each assertion is delivered as a separate schema component
        AssertionFacet facet = this;
        return SchemaStructure.accessorFunction(arg -> {
            String key = arg.head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(facet);
                case "value":
                    return assertion.getComponentAsFunction();

                default:
                    return EmptySequence.getInstance();
            }
        });
    }


}

// Copyright (c) 2009-2023 Saxonica Limited