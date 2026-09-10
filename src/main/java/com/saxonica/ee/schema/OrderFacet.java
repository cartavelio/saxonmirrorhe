package com.saxonica.ee.schema;

import net.sf.saxon.expr.sort.SimpleTypeComparison;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.*;



/**
 * The saxon:order facet on a list type can be used to constrain the list to be in ascending or descending order.
 */
public class OrderFacet extends Facet {

    private boolean ascending;

    /**
     * Create a saxon:order facet
     */

    public OrderFacet() {
    }

    @Override
    public String getName() {
        return "order";
    }

    @Override
    public UnicodeString getValue() {
        String base = ascending ? "ascending" : "descending";
        return BMPString.of(base);
    }

    public void setValue(String value) throws SchemaException {
        switch (value) {
            case "ascending":
                this.ascending = true;
                break;
            case "descending":
                this.ascending = false;
                break;
            default:
                throw new SchemaException("Value of saxon:order facet must be ascending or descending");
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
            UserListType ubase = (UserListType) base;

            OrderFacet baseFacet = (OrderFacet) ubase.getFacet(OrderFacet.class);
            if (baseFacet != null) {
                if (baseFacet.ascending != ascending) {
                    throw new SchemaException(
                            "The saxon:order facet cannot be varied in a restricted type");
                }
            }
        } else {
            throw new SchemaException(
                        "The saxon:order facet is allowed only on list types");
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
     * Test whether a list value conforms to this facet. This is used only for the enumeration
     * and assertion facets.
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test fails
     */
    @Override
    public boolean testListValue(AtomicSequence value) {
        AtomicValue latest = null;
        if (ascending) {
            for (AtomicValue atom : value) {
                if (latest != null && SimpleTypeComparison.getInstance().compareItems(latest, atom) > 0) {
                    return false;
                }
                latest = atom;
            }
        } else {
            for (AtomicValue atom : value) {
                if (latest != null && SimpleTypeComparison.getInstance().compareItems(latest, atom) < 0) {
                    return false;
                }
                latest = atom;
            }
        }
        return true;
    }

    @Override
    public FunctionItem getFacetAsFunctionItem() {
        OrderFacet facet = this;
        return SchemaStructure.accessorFunction(arg -> {
            String key = arg.head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(facet);
                case "value":
                    return new StringValue(getValue());
                case "fixed":
                    return BooleanValue.get(isFixed());
                default:
                    return EmptySequence.getInstance();
            }
        });
    }


}

// Copyright (c) 2018-2023 Saxonica Limited