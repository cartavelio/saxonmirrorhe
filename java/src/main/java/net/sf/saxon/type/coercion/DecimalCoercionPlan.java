package net.sf.saxon.type.coercion;

import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BigDecimalValue;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.FloatValue;

/**
 * Coercion plan for use when the required item type is xs:decimal
 */
public class DecimalCoercionPlan extends AtomicCoercionPlan {

    private final static DecimalCoercionPlan INSTANCE = new DecimalCoercionPlan();

    public static DecimalCoercionPlan getInstance() {
        return INSTANCE;
    }

    @Override
    protected AtomicValue promote(AtomicValue item, ItemType requiredType, CoercionRequest request) throws XPathException {
        try {
            if (item instanceof DoubleValue) {
                return new BigDecimalValue(((DoubleValue) item).getDecimalValue());
            } else if (item instanceof FloatValue) {
                return new BigDecimalValue(((FloatValue) item).getDecimalValue());
            } else {
                return item;
            }
        } catch (ValidationException e) {
            throw coercionError(item, requiredType, request);
        }
    }

}


