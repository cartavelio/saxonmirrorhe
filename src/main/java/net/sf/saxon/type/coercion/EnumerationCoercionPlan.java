package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.type.SingletonEnumType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

/**
 * Coercion plan for use with 4.0 when the required item type is a singleton enumeration type
 */
public class EnumerationCoercionPlan extends AtomicCoercionPlan {

    public final static EnumerationCoercionPlan INSTANCE = new EnumerationCoercionPlan();

    @Override
    protected ItemMappingFunction getUntypedAtomicPromoter(CoercionRequest request, UType atomizedtype) {
        SingletonEnumType enumType = (SingletonEnumType) request.requiredType.getPrimaryType();
        TypeHierarchy th = request.config.getTypeHierarchy();
        return ItemMapper.of(item -> {
            if (enumType.matches(item, th)) {
                return item;
            } else if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                if (enumType.allowsValue(item.getUnicodeStringValue())) {
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
        SingletonEnumType enumType = (SingletonEnumType) request.requiredType.getPrimaryType();
        TypeHierarchy th = request.config.getTypeHierarchy();
        return ItemMapper.of(item -> {
            if (enumType.matches(item, th)) {
                return item;
            } else if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                if (enumType.allowsValue(item.getUnicodeStringValue())) {
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


