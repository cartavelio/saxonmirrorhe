package net.sf.saxon.type.coercion;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. The {@code NoCoercionPlan} is used for types
 * such as item(), node(), or map(*) where no item-level coercion take place.
 */
public final class NoCoercionPlan extends CoercionPlan {

    public final static NoCoercionPlan INSTANCE = new NoCoercionPlan();

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param item the item to be (not) coerced
     * @param requiredType the required type
     * @param request      the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     */

    public GroundedValue coerceItem(Item item, ItemType requiredType, CoercionRequest request) throws XPathException {
        check(item, requiredType, request);
        return item;
    }



}


