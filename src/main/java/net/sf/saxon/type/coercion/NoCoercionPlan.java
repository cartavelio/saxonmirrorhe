package net.sf.saxon.type.coercion;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.MemoSequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.TypeHierarchy;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. The {@code NoCoercionPlan} is used for types
 * such as item(), node(), or map(*) where no coercions take place.
 */
public final class NoCoercionPlan extends CoercionPlan {

    public final static NoCoercionPlan INSTANCE = new NoCoercionPlan();

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     * @param request the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     */

    public Sequence coerce(CoercionRequest request) throws XPathException {
        TypeHierarchy th = request.config.getTypeHierarchy();
        if (request.value instanceof GroundedValue) {
            if (request.requiredType.matches((GroundedValue)request.value, th)) {
                return request.value;
            } else {
                throw coercionError(request, request.value);
            }
        } else {
            Affinity affinity = th.sequenceTypeRelationship(request.knownType, request.requiredType);
            switch (affinity) {
                case DISJOINT:
                    throw coercionError(request, request.value);
                case SUBSUMED_BY:
                case SAME_TYPE:
                    return request.value;
                default:
                    // Inject lazy type-checking
                    SequenceIterator iter = request.value.iterate();
                    iter = checkingIterator(request, iter);
                    return new MemoSequence(iter);
            }
        }
    }



}


