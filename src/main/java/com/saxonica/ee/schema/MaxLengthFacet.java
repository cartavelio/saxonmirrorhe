package com.saxonica.ee.schema;

import net.sf.saxon.type.*;

/**
 * Represents the minLength facet
 */

public class MaxLengthFacet extends LengthRangeFacet {

    @Override
    public String getName() {
        return "maxLength";
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if the facet is not legal
     */

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        if (base instanceof UserSimpleType) {
            UserSimpleType ubase = (UserSimpleType) base;
            Facet baseFacet = ubase.getFacet(LengthFacet.class);
            if (baseFacet != null) {
                if (requiredLength != baseFacet.toLong()) {
                    throw new SchemaException("In a type derived by restriction, " +
                            "the maxLength facet conflicts with the length of the base type");
                }
                //        Part 2 section 4.3.1.4
                //        If length is a member of {facets} then
                //        2 It is an error for maxLength to be a member of {facets} unless
                //        2.1 the {value} of maxLength <= the {value} of length and
                //        2.2 there is type definition from which this one is derived by one or more restriction
                //            steps in which maxLength has the same {value} and length is not specified.

                //        What this means is that there must be an ancestor type with the same maxLength and no
                //        length

                SchemaType parent = base;
                boolean ok = false;
                while (parent instanceof UserSimpleType) {
                    Facet parentMaxLength = ((UserSimpleType) parent).getFacet(MaxLengthFacet.class);
                    if (parentMaxLength != null && parentMaxLength.toLong() == requiredLength &&
                            ((UserSimpleType) parent).getFacet(LengthFacet.class) == null) {
                        ok = true;
                        break;
                    }
                    parent = parent.getBaseType();
                }
                if (!ok) {
                    throw new SchemaException("A type cannot have both a length and a maxLength facet unless" +
                            " it is derived from a type having the same maxLength and no length facet");
                }
            }
            baseFacet = ubase.getFacet(MinLengthFacet.class);
            if (baseFacet != null) {
                if (requiredLength < baseFacet.toLong()) {
                    throw new SchemaException("In a type derived by restriction, " +
                            "the maxLength facet is shorter than the minLength of the base type");
                }
            }
            baseFacet = ubase.getFacet(MaxLengthFacet.class);
            if (baseFacet != null) {
                if (requiredLength > baseFacet.toLong()) {
                    throw new SchemaException("In a type derived by restriction, " +
                            "the maxLength facet is longer than the maxLength of the base type");
                }
            }
        } else if (base instanceof BuiltInListType) {
            // The built-in list types all have minLength = 1
            if (requiredLength == 0) {
                throw new SchemaException("In a type derived by restriction from a built-in list type, " +
                        "the maxLength facet cannot be zero because the base type has minLength=1");
            }

        }
    }


    /**
     * Test whether a supplied length conforms to this required length
     */

    @Override
    public boolean testLength(int actualLength) {
        return actualLength <= requiredLength;
    }
}

// Copyright (c) 2004-2023 Saxonica Limited