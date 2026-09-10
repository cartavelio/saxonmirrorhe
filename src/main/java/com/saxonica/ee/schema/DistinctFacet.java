package com.saxonica.ee.schema;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.Function;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.*;

import java.util.HashSet;
import java.util.Set;

/**
 * The saxon:distinct facet on a list type can be used to constrain the list to contain distinct values.
 */
public class DistinctFacet extends Facet {

    /**
     * Create a saxon:distinct facet
     */

    public DistinctFacet() {
    }

    @Override
    public String getName() {
        return "distinct";
    }

    @Override
    public UnicodeString getValue() {
        return BMPString.of("true");
    }

    public void setValue(String value) throws SchemaException {
        if (value != null && !value.equals("true")) {
            throw new SchemaException("The only permitted value for saxon:distinct is 'true'");
        }
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     * @throws SchemaException
     *          if the facet is not legal
     */

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException {

        if (base instanceof UserListType) {
            // all is well
        } else {
            throw new SchemaException(
                        "The saxon:distinct facet is allowed only on list types");
        }
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
        return true;
    }

    /**
     * Test whether a list value conforms to this facet.
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test fails
     */
    @Override
    public boolean testListValue(AtomicSequence value) {
        Set<Comparable> values = new HashSet<>();
        for (AtomicValue atom : value) {
            if (!values.add(atom.getSchemaComparable())) {
                return false;
            }
        }
        return true;
    }

    @Override
    public Function getFacetAsFunctionItem() {
        Callable callable = (context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return new StringItem(getName());
                case "implementation":
                    return new ObjectValue<>(DistinctFacet.this);
                case "value":
                    return new StringItem(getValue());
                case "fixed":
                    return BooleanValue.get(isFixed());
                default:
                    return EmptySequence.getInstance();
            }
        };
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }


}

// Copyright (c) 2018-2020 Saxonica Limited