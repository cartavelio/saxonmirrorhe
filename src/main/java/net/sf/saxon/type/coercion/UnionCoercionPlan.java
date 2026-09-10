package net.sf.saxon.type.coercion;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.List;

/**
 * Coercion plan for use with 4.0 when the required item type is an enumeration type
 */
public class UnionCoercionPlan extends AtomicCoercionPlan {

    public final static UnionCoercionPlan INSTANCE = new UnionCoercionPlan();
    protected ItemMappingFunction getUntypedAtomicPromoter(CoercionRequest request, UType atomizedType) {

        Configuration config = request.config;
        TypeHierarchy th = config.getTypeHierarchy();
        ItemType requiredItemType = request.requiredType.getPrimaryType();
//        if (!atomizedType.overlaps(UType.UNTYPED_ATOMIC) &&
//                !th.isSubType(BuiltInAtomicType.UNTYPED_ATOMIC, requiredItemType)) {
            final boolean nsSensitive = ((PlainType) requiredItemType).isNamespaceSensitive();
            if (nsSensitive) {
                return ItemMapper.of(item -> {
                    if (((AtomicValue) item).isUntypedAtomic()) {
                        // TODO: this rule should be 4.0-only
                        StringValue correspondingString = new StringValue(item.getUnicodeStringValue());
                        if (requiredItemType.matches(correspondingString, th)) {
                            return correspondingString;
                        }
                        RoleDiagnostic role = request.roleSupplier.get();
                        ValidationFailure vf = new ValidationFailure(
                                "Failed to convert the " + role.getMessage() + ": " +
                                        "Implicit conversion of untypedAtomic value to " + requiredItemType +
                                        " is not allowed");
                        vf.setErrorCode("XPTY0117");
                        throw vf.makeException();
                    } else {
                        return item;
                    }
                });

            } else {
                final ConversionRules rules = config.getConversionRules();
                return ItemMapper.of(item -> {
                    if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                        try {
                            return ((UnionType) requiredItemType).getTypedValue(
                                    item.getUnicodeStringValue(), null, rules).head();
                        } catch (ValidationException ve) {
                            throw ve.withErrorCode("XPTY0004");
                        }
                    } else {
                        return item;
                    }
                });
            } 
//        }
//        return null;
    }


    @Override
    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        ItemType requiredItemType = request.requiredType.getPrimaryType();
        UnionType unionType = ((UnionType) requiredItemType);
        List<? extends PlainType> memberTypes = unionType.getPlainMemberTypes();
        ConversionRules rules = request.config.getConversionRules();
        TypeHierarchy th = request.config.getTypeHierarchy();
        return ItemMapper.of(item -> {
            for (PlainType memberType : memberTypes) {
                if (memberType.isAtomicType()) {
                    AtomicType prim = memberType.getPrimitiveItemType();
                    if (prim.matches(item, th)
                            && ((AtomicType) memberType).validate((AtomicValue) item, null, rules) == null) {
                        return ((AtomicValue) item).withMetadata((AtomicType) memberType);
                    }
                }
            }
            throw coercionError(request, item);
        });
    }

}


