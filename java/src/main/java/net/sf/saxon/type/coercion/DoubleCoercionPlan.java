package net.sf.saxon.type.coercion;

import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.DecimalValue;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.FloatValue;

/**
 * Coercion plan for use when the required item type is xs:double
 */
public class DoubleCoercionPlan extends AtomicCoercionPlan {

    private final static DoubleCoercionPlan INSTANCE = new DoubleCoercionPlan();

    public static DoubleCoercionPlan getInstance() {
        return INSTANCE;
    }

    @Override
    protected AtomicValue promote(AtomicValue item, ItemType requiredType, CoercionRequest request) throws XPathException {
        if (item instanceof DecimalValue) {
            return new DoubleValue(((DecimalValue) item).getDoubleValue());
        } else if (item instanceof FloatValue) {
            return new DoubleValue(((FloatValue) item).getDoubleValue());
        } else {
            return item;
        }
    }

}


