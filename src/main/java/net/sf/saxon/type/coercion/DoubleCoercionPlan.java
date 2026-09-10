package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.type.UType;
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
    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        if (atomizedType.overlaps(UType.NUMERIC)) {
            return ItemMapper.of(item -> {
                if (item instanceof DecimalValue) {
                    return new DoubleValue(((DecimalValue) item).getDoubleValue());
                } else if (item instanceof FloatValue) {
                    return new DoubleValue(((FloatValue) item).getDoubleValue());
                } else {
                    return item;
                }
            });
        }
        return null;
    }

}


