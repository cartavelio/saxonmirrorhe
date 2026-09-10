package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.Genre;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. An AtomicCoercionPlan is used where the required
 * item type is atomic, and is further refined for specific atomic types where special rules
 * apply.
 */
public class AtomicCoercionPlan extends CoercionPlan {

    private final static AtomicCoercionPlan INSTANCE = new AtomicCoercionPlan();

    public static AtomicCoercionPlan getInstance() {
        return INSTANCE;
    }

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param item         the value to be converted
     * @param requiredType
     * @param request      the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     * @throws XPathException if the value cannot be converted to the required type
     */

    public GroundedValue coerceItem(Item item, ItemType requiredType, CoercionRequest request) throws XPathException {

        if (item.getGenre() == Genre.ATOMIC) {
            return coerceAtomicValue((AtomicValue) item, requiredType, request);
        } else {
            AtomicSequence seq;
            try {
                seq = item.atomize();
            } catch (XPathException e) {
                RoleDiagnostic role = request.roleSupplier.get();
                ValidationFailure vf = new ValidationFailure(
                        "Failed to atomize the " + role.getMessage() + ": " + e.getMessage());
                vf.setErrorCode("XPTY0117");
                throw vf.makeException().asTypeError();
            }
            if (seq instanceof AtomicValue) {
                return coerceAtomicValue((AtomicValue) seq, requiredType, request);
            } else {
                ZenoSequence result = new ZenoSequence();
                for (AtomicValue atom : seq) {
                    result = result.append(coerceAtomicValue(atom, requiredType, request));
                }
                return result;
            }
        }
    }

    protected AtomicValue coerceAtomicValue(AtomicValue atom, ItemType requiredType, CoercionRequest request) throws XPathException {
        if (atom.isUntypedAtomic()) {
            atom = convertUntyped(atom, requiredType, request);
        }
        atom = promote(atom, requiredType, request);
        check(atom, requiredType, request);
        return atom;
    }

    protected AtomicValue convertUntyped(AtomicValue atom, ItemType requiredType, CoercionRequest request) throws XPathException {
        if (requiredType.getUType().subsumes(UType.UNTYPED_ATOMIC)) {
            return atom;
        }
        return Converter.convert(
                atom, (AtomicType) requiredType, request.config.getConversionRules());
    }

    protected AtomicValue promote(AtomicValue atom, ItemType requiredType, CoercionRequest request) throws XPathException {
        return atom;
    }




}


