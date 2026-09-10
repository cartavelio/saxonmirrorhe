package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.Base64BinaryValue;
import net.sf.saxon.value.HexBinaryValue;

/**
 * Coercion plan for use when the required item type is xs:hexBinary
 */
public class HexBinaryCoercionPlan extends AtomicCoercionPlan {

    private final static HexBinaryCoercionPlan INSTANCE = new HexBinaryCoercionPlan();

    public static HexBinaryCoercionPlan getInstance() {
        return INSTANCE;
    }

    @Override
    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        if (atomizedType.overlaps(UType.BASE64_BINARY)) {
            return ItemMapper.of(item -> {
                if (item instanceof Base64BinaryValue) {
                    return new HexBinaryValue(((Base64BinaryValue)item).getBinaryValue());
                } else {
                    return item;
                }
            });
        }
        return null;
    }

}


