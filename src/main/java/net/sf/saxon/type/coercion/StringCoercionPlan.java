package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.StringValue;

/**
 * Coercion plan for use when the required item type is xs:string
 */
public class StringCoercionPlan extends AtomicCoercionPlan {

    private static final StringCoercionPlan INSTANCE = new StringCoercionPlan();

    public static StringCoercionPlan getInstance() {
        return INSTANCE;
    }

    @Override
    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        if (atomizedType.overlaps(UType.ANY_URI)) {
            return ItemMapper.of(item -> {
                if (item instanceof AnyURIValue) {
                    return new StringValue(item.getUnicodeStringValue());
                } else {
                    return item;
                }
            });
        }
        return null;
    }

}


