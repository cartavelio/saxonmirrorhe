package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.*;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.MemoSequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
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
     * @param request the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     */

    public Sequence coerce(CoercionRequest request) throws XPathException {
        SequenceIterator iterator = request.value.iterate();
        iterator = new ItemMappingIterator(iterator, getRecordTestMapper(request));
        if (request.requiredType.getCardinality() != StaticProperty.ALLOWS_ZERO_OR_MORE) {
            iterator = new CardinalityCheckingIterator(
                    iterator, request.requiredType.getCardinality(), request.roleSupplier, request.locator);
        }
        return new MemoSequence(iterator);
    }


    private ItemMappingFunction getRecordTestMapper(CoercionRequest request) {
        // If R is a record test, apply the rules recursively
        ItemType requiredItemType = request.requiredType.getPrimaryType();
        TypeHierarchy th = request.config.getTypeHierarchy();
        if (requiredItemType instanceof RecordTest) {
            RecordTest test = (RecordTest) requiredItemType;
            return ItemMapper.of(item -> {
                if (item instanceof MapItem) {
                    MapItem m2 = (MapItem) item;
                    for (KeyValuePair kvp : m2.keyValuePairs()) {
                        if (kvp.key instanceof StringValue) {
                            SequenceType fieldType = test.getFieldType(kvp.key.getStringValue());
                            if (!fieldType.matches(kvp.value, th)) {
                                CoercionRequest subRequest = new CoercionRequest(
                                        kvp.value, fieldType, SequenceType.ANY_SEQUENCE,
                                        request.config, request.roleSupplier, request.locator);
                                CoercionPlan fieldPlan = fieldType.getPrimaryType().getCoercionPlan(40);
                                GroundedValue convertedValue =
                                        fieldPlan.coerce(subRequest).materialize();
                                m2 = m2.addEntry(kvp.key, convertedValue);
                            }
                        }
                    }
                    return m2;
                } else {
                    return item;
                }
            });
        }
        return null;
    }

}


