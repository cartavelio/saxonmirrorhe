package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.StringValue;

/**
 * Coercion plan for use when the required item type is xs:anyURI
 */
public class AnyURICoercionPlan extends AtomicCoercionPlan {

    public final static AnyURICoercionPlan INSTANCE = new AnyURICoercionPlan();
    @Override
    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        if (atomizedType.overlaps(UType.STRING)) {
            return ItemMapper.of(item -> {
                if (item instanceof StringValue && !(item instanceof AnyURIValue)) {
                    return new AnyURIValue(item.getUnicodeStringValue());
                } else {
                    return item;
                }
            });
        }
        return null;
    }

}


