package net.sf.saxon.type.coercion;

import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. The {@code NoCoercionPlan} is used for types
 * such as item(), node(), or map(*) where no coercions take place.
 */
public final class RecordCoercionPlan extends CoercionPlan {

    public final static RecordCoercionPlan INSTANCE = new RecordCoercionPlan();

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param item
     * @param requiredType
     * @param request      the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     */

    public GroundedValue coerceItem(Item item, ItemType requiredType, CoercionRequest request) throws XPathException {
        if (item instanceof MapItem) {
            MapItem m2 = (MapItem) item;
            RecordTest test = (RecordTest) requiredType;
            TypeHierarchy th = request.config.getTypeHierarchy();
            for (KeyValuePair kvp : m2.keyValuePairs()) {
                if (kvp.key instanceof StringValue) {
                    SequenceType fieldType = test.getFieldType(kvp.key.getStringValue());
                    if (fieldType != null) {
                        CoercionPlan fieldPlan = fieldType.getPrimaryType().getCoercionPlan(40);
                        if (!fieldType.matches(kvp.value, th)) {
                            CoercionRequest subRequest = new CoercionRequest(
                                    SequenceType.ANY_SEQUENCE,
                                    request.config, request.roleSupplier, request.locator);
                            SequenceIterator convertedValue =
                                    fieldPlan.coerceSequence(kvp.value.iterate(), fieldType, subRequest);
                            m2 = m2.addEntry(kvp.key, SequenceTool.toGroundedValue(convertedValue));
                        }
                    }
                }
            }
            check(m2, test, request);  // TODO: is this needed?
            return m2;
        } else {
            throw coercionError(item, requiredType, request);
        }
    }


}


