package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AtomicValue;

/**
 * Coercion plan for use with 4.0 when the required item type is a derived atomic type
 */
public class DerivedAtomicCoercionPlan extends AtomicCoercionPlan {

    private static final DerivedAtomicCoercionPlan INSTANCE = new DerivedAtomicCoercionPlan();

    public static DerivedAtomicCoercionPlan getInstance() {
        return INSTANCE;
    }

    @Override
    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        AtomicType req = (AtomicType) request.requiredType.getPrimaryType();
        AtomicType prim = (AtomicType) req.getPrimitiveItemType();
        ConversionRules rules = request.config.getConversionRules();
        TypeHierarchy th = request.config.getTypeHierarchy();
        return ItemMapper.of(item -> {
            if (prim.matches(item, th)
                    && !req.matches(item, th)
                    && req.validate((AtomicValue) item, null, rules) == null) {
                return ((AtomicValue) item).withMetadata(req);
                // TODO: else error?
            } else {
                return item;
            }
        });
    }

}


