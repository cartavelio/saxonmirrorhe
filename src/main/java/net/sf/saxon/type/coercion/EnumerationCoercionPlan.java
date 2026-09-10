package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.type.EnumerationType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

/**
 * Coercion plan for use with 4.0 when the required item type is an enumeration type
 */
public class EnumerationCoercionPlan extends AtomicCoercionPlan {

    public final static EnumerationCoercionPlan INSTANCE = new EnumerationCoercionPlan();

    @Override
    protected ItemMappingFunction getUntypedAtomicPromoter(CoercionRequest request, UType atomizedtype) {
        EnumerationType enumType = (EnumerationType) request.requiredType.getPrimaryType();
        TypeHierarchy th = request.config.getTypeHierarchy();
        return ItemMapper.of(item -> {
            if (enumType.matches(item, th)) {
                return item;
            } else if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                if (enumType.allowsValue(item.getStringValue())) {
                    return new StringValue(item.getStringValue());
                } else {
                    throw coercionError(request, item);
                }
            } else {
                throw coercionError(request, item);
            }
        });
    }

    @Override
    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        EnumerationType enumType = (EnumerationType) request.requiredType.getPrimaryType();
        TypeHierarchy th = request.config.getTypeHierarchy();
        return ItemMapper.of(item -> {
            if (enumType.matches(item, th)) {
                return item;
            } else if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                if (enumType.allowsValue(item.getStringValue())) {
                    return new StringValue(item.getStringValue());
                } else {
                    throw coercionError(request, item);
                }
            } else {
                throw coercionError(request, item);
            }
        });
    }

}


