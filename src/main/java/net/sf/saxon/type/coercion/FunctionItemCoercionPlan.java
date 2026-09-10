package net.sf.saxon.type.coercion;

import net.sf.saxon.functions.hof.CoercedFunction;
import net.sf.saxon.functions.hof.FunctionSequenceCoercer;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;

/**
 * Coercion plan for use when the required item type is xs:hexBinary
 */
public class FunctionItemCoercionPlan extends CoercionPlan {

    private final static FunctionItemCoercionPlan INSTANCE31 = new FunctionItemCoercionPlan(31);
    private final static FunctionItemCoercionPlan INSTANCE40 = new FunctionItemCoercionPlan(31);

    private final int version;

    public static FunctionItemCoercionPlan getInstance(int version) {
        return version >= 40 ? INSTANCE40 : INSTANCE31;
    }

    private FunctionItemCoercionPlan (int version) {
        this.version = version;
    }

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param request the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     * @throws XPathException if the value cannot be converted to the required type
     */
    @Override
    public Sequence coerce(CoercionRequest request) throws XPathException {
        boolean is40 = version >= 40;
        Sequence value = request.value;
        if (value instanceof FunctionItem) {
            FunctionSequenceCoercer.checkAnnotations((FunctionItem) value,
                                                     (FunctionItemType) request.requiredType.getPrimaryType(),
                                                     request.config);
            return new CoercedFunction((FunctionItem) value, (SpecificFunctionType) request.requiredType.getPrimaryType(), is40, is40);
        } else {
            return value;
        }
    }

}


