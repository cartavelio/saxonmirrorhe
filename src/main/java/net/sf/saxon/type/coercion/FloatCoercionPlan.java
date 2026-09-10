package net.sf.saxon.type.coercion;

import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.DecimalValue;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.FloatValue;

/**
 * Coercion plan for use when the required item type is xs:float
 */
public class FloatCoercionPlan extends AtomicCoercionPlan {

    private final static FloatCoercionPlan INSTANCE = new FloatCoercionPlan();

    public static FloatCoercionPlan getInstance() {
        return INSTANCE;
    }
    @Override
    protected AtomicValue promote(AtomicValue item, ItemType requiredType, CoercionRequest request) throws XPathException {
        if (item instanceof DecimalValue) {
            return new FloatValue(((DecimalValue) item).getFloatValue());
        } else if (item instanceof DoubleValue) {
            return new FloatValue(((DoubleValue) item).getFloatValue());
        } else {
            return item;
        }
    }

}


