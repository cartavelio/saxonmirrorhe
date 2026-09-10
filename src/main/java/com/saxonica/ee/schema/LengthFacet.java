package com.saxonica.ee.schema;

import net.sf.saxon.type.*;

/**
 * Represents the length facet
 */
public class LengthFacet extends LengthRangeFacet {

    @Override
    public String getName() {
        return "length";
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
    public void checkFacetRestriction(final UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        if (base instanceof UserSimpleType) {
            UserSimpleType ubase = (UserSimpleType) base;
            Facet baseFacet = ubase.getFacet(LengthFacet.class);
            if (baseFacet != null) {
                if (requiredLength != ((LengthFacet) baseFacet).requiredLength) {
                    throw new SchemaException("In a type derived by restriction, " +
                            "the length facet must not be changed");
                }
            }
            baseFacet = ubase.getFacet(MinLengthFacet.class);
            if (baseFacet != null) {
                if (requiredLength < baseFacet.toLong()) {
                    throw new SchemaException("In a type derived by restriction, " +
                            "the length facet is shorter than the minLength of the base type");
                }
                if (baseFacet.isFixed() && requiredLength != baseFacet.toLong()) {
                    throw new SchemaException("The minLength facet is fixed in the base type and cannot be varied");
                }
            }

            baseFacet = ubase.getFacet(MaxLengthFacet.class);
            if (baseFacet != null) {
                if (requiredLength > baseFacet.toLong()) {
                    throw new SchemaException("In a type derived by restriction, " +
                            "the length facet is longer than the maxLength of the base type");
                }
                if (baseFacet.isFixed() && requiredLength != baseFacet.toLong()) {
                    throw new SchemaException("The maxLength facet is fixed in the base type and cannot be varied");
                }
            }
        }

//        Part 2 section 4.3.1.4
//        If length is a member of {facets} then
//        1 It is an error for minLength to be a member of {facets} unless
//        1.1 the {value} of minLength <= the {value} of length and
//        1.2 there is type definition from which this one is derived by one or more restriction steps in which
//            minLength has the same {value} and length is not specified.
//        2 It is an error for maxLength to be a member of {facets} unless
//        2.1 the {value} of length <= the {value} of maxLength and
//        2.2 there is type definition from which this one is derived by one or more restriction steps in which
//            maxLength has the same {value} and length is not specified.

        Facet minLength = type.getFacet(MinLengthFacet.class);
        if (minLength != null) {
            if (minLength.toLong() > requiredLength) {
                throw new SchemaException("The minLength facet is longer than the length facet");
            }
            long thisMin = minLength.toLong();
            SchemaType parent = base;
            boolean ok = false;
            while (parent instanceof UserSimpleType) {
                Facet parentMinLength = ((UserSimpleType) parent).getFacet(MinLengthFacet.class);
                if (parentMinLength != null && parentMinLength.toLong() == thisMin &&
                        ((UserSimpleType) parent).getFacet(LengthFacet.class) == null) {
                    ok = true;
                    break;
                }
                parent = parent.getBaseType();
            }
            if (thisMin == 1 && parent instanceof BuiltInListType) {
                // all the built-in list types have minLength = 1, so specifying it again
                // is not an error
                ok = true;
            }
            if (!ok) {
                throw new SchemaException("A type cannot have both a length and a minLength facet unless " +
                        "it is derived from a type having the same minLength and no length facet");
            }
        }
        Facet maxLength = type.getFacet(MaxLengthFacet.class);
        if (maxLength != null) {
            if (maxLength.toLong() < requiredLength) {
                throw new SchemaException("The maxLength facet is shorter than the length facet");
            }
            long thisMax = maxLength.toLong();
            SchemaType parent = base;
            boolean ok = false;
            while (parent instanceof UserSimpleType) {
                Facet parentMaxLength = ((UserSimpleType) parent).getFacet(MaxLengthFacet.class);
                if (parentMaxLength != null && parentMaxLength.toLong() == thisMax &&
                        ((UserSimpleType) parent).getFacet(LengthFacet.class) == null) {
                    ok = true;
                    break;
                }
                parent = parent.getBaseType();
            }
            if (!ok) {
                throw new SchemaException("A type cannot have both a length and a maxLength facet unless " +
                        "it is derived from a type having the same maxLength and no length facet");
            }
        }

    }

    /**
     * Test whether a supplied length conforms to this required length.
     */

    @Override
    public boolean testLength(int actualLength) {
        return actualLength == requiredLength;
    }

}

// Copyright (c) 2009-2022 Saxonica Limited



