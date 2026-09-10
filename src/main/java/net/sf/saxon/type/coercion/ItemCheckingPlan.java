package net.sf.saxon.type.coercion;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. The {@code ItemCheckingPlan} is used for types
 * such as item(), node(), or map(*) where no item-level coercion take place, but where the
 * only required action is to check that the items have the correct type and raise an
 * error if not.
 */
public final class ItemCheckingPlan extends CoercionPlan {

    public final static ItemCheckingPlan INSTANCE = new ItemCheckingPlan();

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


