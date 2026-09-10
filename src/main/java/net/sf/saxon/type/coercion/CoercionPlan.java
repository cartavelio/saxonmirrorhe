package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.CardinalityCheckingIterator;
import net.sf.saxon.expr.LastPositionFinder;
import net.sf.saxon.expr.MappingIterator;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. A coercion plan is associated with each item
 * type, and may differ depending on the XPath language version in use.
 */
public abstract class CoercionPlan {

    /**
     * Apply the coercion rules (function conversion rules) to a sequence, given a required type.
     * @param value the value to be coerced
     * @param requiredType the required sequence type
     * @param request other input to the coercion, for example diagnostic information
     * @return the converted value (lazily-evaluated)
     * @throws net.sf.saxon.trans.XPathException if the value cannot be converted to the required type
     */

    public final SequenceIterator coerceSequence(SequenceIterator value, SequenceType requiredType, CoercionRequest request)
            throws XPathException {
        int cardinality = requiredType.getCardinality();
        boolean singleton = false;
        if (value instanceof LastPositionFinder && ((LastPositionFinder) value).supportsGetLength()) {
            int length = ((LastPositionFinder) value).getLength();
            if (length == 0) {
                if (Cardinality.allowsZero(cardinality)) {
                    return value;
                } else {
                    // force an error
                    return new CardinalityCheckingIterator(
                            value, cardinality, request.roleSupplier, request.locator);
                }
            } else if (length == 1) {
                singleton = true;
            }
        }
        // If the required type is empty-sequence(), check that the sequence is actually empty
        if (cardinality == StaticProperty.ALLOWS_ZERO) {
            return new CardinalityCheckingIterator(
                    value, StaticProperty.ALLOWS_ZERO, request.roleSupplier, request.locator);
        }
        // Convert the items in the sequence (lazily)
        SequenceIterator uncheckedResult;
        if (singleton) {
            Item input = value.next();
            uncheckedResult = coerceItem(input, requiredType.getPrimaryType(), request).iterate();
        } else {
            uncheckedResult = makeMappingIterator(value, requiredType.getPrimaryType(), request);
        }
        // If necessary, check the cardinality of the result (lazily)
        if (cardinality != StaticProperty.ALLOWS_ZERO_OR_MORE) {
            return new CardinalityCheckingIterator(
                    uncheckedResult, cardinality, request.roleSupplier, request.locator);
        } else {
            return uncheckedResult;
        }
    }

    private SequenceIterator makeMappingIterator(SequenceIterator value, ItemType requiredType, CoercionRequest request) {
        return new MappingIterator(value, item -> coerceItem(item, requiredType, request).iterate());
    }

    /**
     * Apply the coercion rules (3.1: function conversion rules) to an item, given this target item type.
     *
     * @param item         the item to be coerced
     * @param requiredType the required item type
     * @param request      the input to the coercion
     * @return the converted value. We define this as a grounded value because in the vast
     * majority of cases it will be a single item, and in other cases (a typed node with a list type,
     * an array of atomic values) there is little benefit in lazy evaluation. The implementation
     * is responsible for ensuring that the returned value does indeed consist entirely of items
     * that match the required item type; it is not responsible for cardinality checking. The item
     * type checking can be achieved, if required, by a callback to the check() method.
     * @throws net.sf.saxon.trans.XPathException if the value cannot be converted to the required type
     */

    public abstract GroundedValue coerceItem(Item item, ItemType requiredType, CoercionRequest request) throws XPathException;

    /**
     * Check that the item is actually an instance of the required type, throwing an error if not
     * @param item the item to be checked
     * @param requiredType the required type
     * @param request diagnostic data
     * @throws XPathException if the item is not an instance of the required type
     */
    protected final void check(Item item, ItemType requiredType, CoercionRequest request) throws XPathException {
        if (!requiredType.matches(item, request.config.getTypeHierarchy())) {
            throw coercionError(item, requiredType, request);
        }
    }

    /**
     * Create an exception object representing a coercion error
     * @param value the input value to the coercion
     * @param requiredType the required item type
     * @param request the coercion request details
     * @return an exception for the caller to throw
     */

    protected XPathException coercionError(Sequence value, ItemType requiredType, CoercionRequest request) {
        RoleDiagnostic role = request.roleSupplier.get();
        String representation = value instanceof Item ? Err.depict((Item)value) : value.toString();
        return new XPathException(
                "Failed to convert the " + role.getMessage() + ": " +
                        "Cannot convert " + representation + " to "
                        + requiredType)
                .withErrorCode("XPTY0004")
                .withLocation(request.locator);
    }
    

}


