package net.sf.saxon.type.coercion;

import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.AtomicValue;
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
    protected AtomicValue promote(AtomicValue item, ItemType requiredType, CoercionRequest request) {
        if (item instanceof Base64BinaryValue) {
            return new HexBinaryValue(((Base64BinaryValue) item).getBinaryValue());
        } else {
            return item;
        }
    }

}


