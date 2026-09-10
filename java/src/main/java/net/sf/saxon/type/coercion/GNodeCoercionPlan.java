package net.sf.saxon.type.coercion;

import net.sf.saxon.ma.MapOrArray;
import net.sf.saxon.ma.jnode.RootJNode;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. This class handles the case where the required
 * type is an array type.
 */
public final class GNodeCoercionPlan extends CoercionPlan {

    public final static GNodeCoercionPlan INSTANCE = new GNodeCoercionPlan();

    public static GNodeCoercionPlan getInstance() {
        return INSTANCE;
    }

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param item the value to be converted
     * @param requiredType the required array type
     * @param request      the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     */

    public GroundedValue coerceItem(Item item, ItemType requiredType, CoercionRequest request) throws XPathException {
        if (item instanceof GNode) {
            return item;
        }
        if (item instanceof MapOrArray) {
            return new RootJNode((MapOrArray)item);
        }
        throw coercionError(item, requiredType, request);
    }


}


