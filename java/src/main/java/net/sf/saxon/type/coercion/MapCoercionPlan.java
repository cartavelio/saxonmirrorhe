package net.sf.saxon.type.coercion;

import net.sf.saxon.ma.map.GeneralMapBuilder;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.PlainType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. This class handles the case where the required
 * type is an array type.
 */
public final class MapCoercionPlan extends CoercionPlan {

    public final static MapCoercionPlan INSTANCE = new MapCoercionPlan();

    public static MapCoercionPlan getInstance() {
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
        PlainType requiredKeyType = ((MapType) requiredType).getKeyType();
        SequenceType requiredValueType = ((MapType)requiredType).getValueType();
        if (item instanceof MapItem) {
            GeneralMapBuilder builder = new GeneralMapBuilder();
            CoercionPlan keyPlan = requiredKeyType.getCoercionPlan(40);
            CoercionPlan valuePlan = requiredValueType.getPrimaryType().getCoercionPlan(40);
            for (KeyValuePair pair : ((MapItem)item).keyValuePairs()) {
                builder.put((AtomicValue)keyPlan.coerceItem(pair.key, requiredKeyType, request),
                            SequenceExtent.from(valuePlan.coerceSequence(
                                    pair.value.iterate(), requiredValueType, request)));
            }
            try {
                return builder.getCompletedMap();
            } catch (XPathException e) {
                throw e.withErrorCode("XPTY0004").asTypeError();
            }
        } else {
            throw coercionError(item, requiredType, request);
        }
    }


}


