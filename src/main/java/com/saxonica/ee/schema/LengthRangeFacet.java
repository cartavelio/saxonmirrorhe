package com.saxonica.ee.schema;

import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.value.*;



/**
 * Abstract class for the three facets length, minLength, and maxLength.
 * Subclasses represent the minLength and maxLength facets.
 */
public abstract class LengthRangeFacet extends Facet {

    int requiredLength;

    @Override
    public UnicodeString getValue() {
        return BMPString.of("" + requiredLength);
    }

    @Override
    public long toLong() {
        return requiredLength;
    }

    /**
     * Set the value as an integer
     *
     * @param value the value of the length facet as an integer
     */

    public void setNumericValue(int value) {
        if (value < 0) {
            throw new IllegalArgumentException("length facet < 0");
        }
        requiredLength = value;
    }

    /**
     * Set the value of this facet as a string. For internal use only.
     *
     * @param value the value as a string, which must parse as an integer
     * @throws SchemaException if the value is invalid.
     */
    public void setValue(final String value) throws SchemaException {
        try {
            requiredLength = Integer.parseInt(value);
        } catch (NumberFormatException e) {
            throw new SchemaException("Value of " + getName() + " facet "
                    + Err.wrap(value, Err.VALUE) + " is not an integer");
        }
        if (requiredLength < 0) {
            throw new SchemaException("Value of " + getName() + " facet "
                    + Err.wrap(value, Err.VALUE) + " must be a positive integer");
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
    public boolean testAtomicValue(final AtomicValue value) {
        if (value instanceof StringValue) { //includes AnyURIValue
            return testLength(((StringValue) value).length32());
        } else if (value instanceof HexBinaryValue) {
            return testLength(((HexBinaryValue) value).getLengthInOctets());
        } else if (value instanceof Base64BinaryValue) {
            return testLength(((Base64BinaryValue) value).getLengthInOctets());
        } else if (value instanceof QualifiedNameValue) {
            return true;
        } else {
            throw new AssertionError("Length facet found on unexpected type");
        }
    }

    /**
     * Test whether a supplied length conforms to this required length. This method is overridden
     * for the minlength and maxlength facets.
     */

    @Override
    public abstract boolean testLength(int actualLength);

    @Override
    public FunctionItem getFacetAsFunctionItem() {
        return SchemaStructure.accessorFunction(arg -> {
                String key = arg.head().getStringValue();
                switch (key) {
                    case "class":
                        return new StringValue(getName());
                    case "implementation":
                        return new ObjectValue<LengthRangeFacet>(LengthRangeFacet.this);
                    case "value":
                        return new Int64Value(requiredLength, BuiltInAtomicType.NON_NEGATIVE_INTEGER, false);
                    case "fixed":
                        return BooleanValue.get(isFixed());
                    default:
                        return EmptySequence.getInstance();
                }
            });
    }

}

// CCopyright (c) 2004-2023 Saxonica Limited