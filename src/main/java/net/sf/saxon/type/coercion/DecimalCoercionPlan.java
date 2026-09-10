package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.type.UType;
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
    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        if (atomizedType.overlaps(UType.NUMERIC)) {
            return ItemMapper.of(item -> {
                if (item instanceof DoubleValue) {
                    return new BigDecimalValue(((DoubleValue) item).getDecimalValue());
                } else if (item instanceof FloatValue) {
                    return new BigDecimalValue(((FloatValue) item).getDecimalValue());
                } else {
                    return item;
                }
            });
        }
        return null;
    }

}


