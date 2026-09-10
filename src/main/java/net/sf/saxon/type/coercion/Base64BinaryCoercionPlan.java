package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.Base64BinaryValue;
import net.sf.saxon.value.HexBinaryValue;

/**
 * Coercion plan for use when the required item type is xs:base64Binary
 */
public class Base64BinaryCoercionPlan extends AtomicCoercionPlan {

    private final static Base64BinaryCoercionPlan INSTANCE = new Base64BinaryCoercionPlan();

    public static Base64BinaryCoercionPlan getInstance() {
        return INSTANCE;
    }

    @Override
    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        if (atomizedType.overlaps(UType.HEX_BINARY)) {
            return ItemMapper.of(item -> {
                if (item instanceof HexBinaryValue) {
                    return new Base64BinaryValue(((HexBinaryValue)item).getBinaryValue());
                } else {
                    return item;
                }
            });
        }
        return null;
    }

}


