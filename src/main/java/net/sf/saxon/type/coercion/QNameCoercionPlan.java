package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.type.UType;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.AtomicValue;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. The {@code NoCoercionPlan} is used for types
 * such as item(), node(), or map(*) where no coercions take place.
 */
public final class QNameCoercionPlan extends AtomicCoercionPlan {

    private final static QNameCoercionPlan INSTANCE = new QNameCoercionPlan();

    public static QNameCoercionPlan getInstance() {
        return INSTANCE;
    }

    @Override
    protected ItemMappingFunction getUntypedAtomicPromoter(CoercionRequest request, UType atomizedtype) {
        return ItemMapper.of(item -> {
            if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                RoleDiagnostic role = request.roleSupplier.get();
                ValidationFailure vf = new ValidationFailure(
                        "Failed to convert the " + role.getMessage() + ": " +
                                "Implicit conversion of untypedAtomic value to " + request.requiredType.getPrimaryType() +
                                " is not allowed");
                vf.setErrorCode("XPTY0117");
                throw vf.makeException();
            } else {
                return item;
            }
        });

    }


}


