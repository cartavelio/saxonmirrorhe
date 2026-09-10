package net.sf.saxon.type.coercion;

import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
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
    protected AtomicValue promote(AtomicValue atom, ItemType requiredType, CoercionRequest request) throws XPathException {
        AtomicType req = (AtomicType) requiredType;
        AtomicType prim = req.getPrimitiveItemType();
        ConversionRules rules = request.config.getConversionRules();
        TypeHierarchy th = request.config.getTypeHierarchy();

        if (prim.matches(atom, th)
                && !req.matches(atom, th)
                && req.validate(atom, null, rules) == null) {
            return ((AtomicValue) atom).withMetadata(req);
            // TODO: else error?
        } else {
            return atom;
        }

    }

}


