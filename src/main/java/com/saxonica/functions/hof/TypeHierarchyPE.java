package com.saxonica.functions.hof;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.type.*;

/**
 * TypeHierarchy for Saxon-PE
 */
public class TypeHierarchyPE extends TypeHierarchy {

    public TypeHierarchyPE(Configuration config) {
        super(config);
    }

    /**
     * Apply function coercion when a function item is supplied as a parameter in a function call
     *
     * @param iterator         An iterator over the supplied value of the parameter
     * @param suppliedItemType the item type of the supplied value
     * @param requiredItemType the required item type (typically a function item type)
     * @param locator          information for diagnostics
     * @return an iterator over the converted value
     */

    protected SequenceIterator<?> applyFunctionCoercion(
            SequenceIterator<?> iterator,
            ItemType suppliedItemType, ItemType requiredItemType,
            Location locator) {
        if (requiredItemType instanceof FunctionItemType && !((FunctionItemType) requiredItemType).isMapType()
                && !((FunctionItemType) requiredItemType).isArrayType()
                && !(relationship(requiredItemType, suppliedItemType) == SUBSUMES)) {

            if (requiredItemType == AnyFunctionType.getInstance()) {
                // no action (the type checking is added later)
                return iterator;

            } else {

                FunctionSequenceCoercer.Coercer coercer = new FunctionSequenceCoercer.Coercer(
                        (SpecificFunctionType) requiredItemType, config, locator);
                return new ItemMappingIterator<>(iterator, coercer, true);
            }

        } else {
            return iterator;
        }
    }


}

// Copyright (c) 2018 Saxonica Limited