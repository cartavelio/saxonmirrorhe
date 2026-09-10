package com.saxonica.ee.schema;

import net.sf.saxon.type.*;

/**
 * Represents the minLength facet
 */
public class MinLengthFacet extends LengthRangeFacet {

    @Override
    public String getName() {
        return "minLength";
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
        Facet max = type.getFacet(MaxLengthFacet.class);
        if (max != null && max.toLong() < requiredLength) {
            throw new SchemaException("The minLength facet exceeds the maxLength");
        }
        if (base instanceof UserSimpleType) {
            UserSimpleType ubase = (UserSimpleType) base;
            Facet baseFacet = ubase.getFacet(LengthFacet.class);
            if (baseFacet != null) {
                if (requiredLength != baseFacet.toLong()) {
                    throw new SchemaException("In a type derived by restriction, " +
                            "the minLength facet conflicts with the length of the base type");
                }
                //        Part 2 section 4.3.1.4
                //        If length is a member of {facets} then
                //        1 It is an error for minLength to be a member of {facets} unless
                //        1.1 the {value} of minLength <= the {value} of length and
                //        1.2 there is type definition from which this one is derived by one or more restriction
                //            steps in which minLength has the same {value} and length is not specified.

                //        What this means is that there must be an ancestor type with the same minLength and no
                //        length

                SchemaType parent = base;
                boolean ok = false;
                while (parent instanceof UserSimpleType) {
                    Facet parentMinLength = ((UserSimpleType) parent).getFacet(MinLengthFacet.class);
                    if (parentMinLength != null && parentMinLength.toLong() == requiredLength &&
                            ((UserSimpleType) parent).getFacet(LengthFacet.class) == null) {
                        ok = true;
                        break;
                    }
                    parent = parent.getBaseType();
                }
                if (!ok) {
                    throw new SchemaException("A type cannot have both a length and a minLength facet unless" +
                            " it is derived from a type having the same minLength and no length facet");
                }

            }

            baseFacet = ubase.getFacet(MinLengthFacet.class);
            if (baseFacet != null) {
                if (requiredLength < baseFacet.toLong()) {
                    throw new SchemaException("In a type derived by restriction, " +
                            "the minLength facet is shorter than the minLength of the base type");
                }
            }
            baseFacet = ubase.getFacet(MaxLengthFacet.class);
            if (baseFacet != null) {
                if (requiredLength > baseFacet.toLong()) {
                    throw new SchemaException("In a type derived by restriction, " +
                            "the minLength facet is longer than the maxLength of the base type");
                }
            }
        } else if (base instanceof BuiltInListType) {
            // The built-in list types all have minLength = 1
            if (requiredLength == 0) {
                throw new SchemaException("In a type derived by restriction from a built-in list type, " +
                        "the minLength facet cannot be zero because the base type has minLength=1");
            }

        }
    }


    /**
     * Test whether a supplied length conforms to this required length (used
     * when testing the length of a list)
     */

    @Override
    public boolean testLength(int actualLength) {
        return actualLength >= requiredLength;
    }
}

// Copyright (c) 2004-2022 Saxonica Limited