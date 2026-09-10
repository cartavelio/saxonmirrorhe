package net.sf.saxon.type.coercion;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. This class handles the case where the required
 * type is an array type.
 */
public final class ArrayCoercionPlan extends CoercionPlan {

    public final static ArrayCoercionPlan INSTANCE = new ArrayCoercionPlan();

    public static ArrayCoercionPlan getInstance() {
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
        SequenceType requiredMemberType = ((ArrayItemType)requiredType).getMemberType();
        if (item instanceof ArrayItem) {
            List<GroundedValue> result = new ArrayList<>(((ArrayItem)item).arrayLength());
            CoercionPlan memberPlan = requiredMemberType.getPrimaryType().getCoercionPlan(40);
            for (GroundedValue member : ((ArrayItem)item).members()) {
                 SequenceIterator newMember = memberPlan.coerceSequence(member.iterate(), requiredMemberType, request);
                 result.add(SequenceExtent.from(newMember));
            }
            return new SimpleArrayItem(result);
        } else {
            throw coercionError(item, requiredType, request);
        }
    }


}


