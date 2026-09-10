package com.saxonica.ee.schema;

import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;



/**
 * Represents a whitespace facet on a schema-defined simple type
 */
public class WhitespaceFacet extends Facet {

    int action;

    public WhitespaceFacet() {
    }

    /**
     * Ask whether this is a constraining facet (as distinct from a pre-lexical facet)
     *
     * @return true if this is a constraining facet. Note: we return false, despite the fact
     *         that the XSD specification misleadingly classifies whiteSpace as a constraining facet
     */

    @Override
    public boolean isConstraining() {
        return false;
    }

    @Override
    public String getName() {
        return "whiteSpace";
    }

    public void setValue(String value) throws SchemaException {
        if (value.equals("preserve")) {
            action = Whitespace.PRESERVE;
        } else if (value.equals("replace")) {
            action = Whitespace.REPLACE;
        } else if (value.equals("collapse")) {
            action = Whitespace.COLLAPSE;
        } else {
            throw new SchemaException("Invalid whiteSpace facet value: " + value);
        }
    }

    /**
     * Returns the lexical representation of this facet
     *
     * @return the value of this facet
     */

    /*@Nullable*/
    @Override
    public UnicodeString getValue() {
        switch (action) {
            case Whitespace.PRESERVE:
                return BMPString.of("preserve");
            case Whitespace.REPLACE:
                return BMPString.of("replace");
            case Whitespace.COLLAPSE:
                return BMPString.of("collapse");
        }
        return null;
    }

    /**
     * Get the whitespace action required: one of PRESERVE, REPLACE, or COLLAPSE
     */

    @Override
    public int getWhitespaceAction() {
        return action;
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler  the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if the facet is not legal
     */

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        if (base.isListType()) {
            if (action != Whitespace.COLLAPSE) {
                throw new SchemaException("The whiteSpace facet on a list type must be {collapse}");
            }
        } else if (base.isUnionType()) {
            throw new SchemaException("The whiteSpace facet is not applicable to a union type");
        } else if (base instanceof AnySimpleType) {
            // do nothing
        } else {
            int p = ((AtomicType) base).getPrimitiveType();
            if (type.getFingerprint() == StandardNames.XS_STRING) {
                // treat the schema for schemas specially
            } else if (p == StandardNames.XS_STRING) {
                if (action < base.getWhitespaceAction()) {
                    throw new SchemaException("The whiteSpace normalization must not be weaker than that of the base type");
                }
            } else {
                if (action != Whitespace.COLLAPSE) {
                    throw new SchemaException("The whiteSpace facet must be {collapse} " +
                            "unless the type is derived from string");
                }
            }
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

    @Override
    public FunctionItem getFacetAsFunctionItem() {
        WhitespaceFacet facet = this;
        return SchemaStructure.accessorFunction(arg -> {
            String key = arg.head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(facet);
                case "value":
                    switch (action) {
                        case Whitespace.PRESERVE:
                        default:
                            return StringValue.bmp("preserve");
                        case Whitespace.COLLAPSE:
                            return StringValue.bmp("collapse");
                        case Whitespace.REPLACE:
                            return StringValue.bmp("replace");
                    }
                case "fixed":
                    return BooleanValue.get(isFixed());
                default:
                    return EmptySequence.getInstance();
            }
        });
    }


}

// Copyright (c) 2004-2023 Saxonica Limited