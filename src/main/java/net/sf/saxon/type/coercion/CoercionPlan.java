package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. A coercion plan is associated with each item
 * type, and may differ depending on the XPath language version in use.
 */
public abstract class CoercionPlan {

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     * @param request the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     * @throws net.sf.saxon.trans.XPathException if the value cannot be converted to the required type
     */

    public abstract Sequence coerce(CoercionRequest request) throws XPathException;

    /**
     * For use in subclasses:
     * construct an ItemMappingFunction by composing two supplied ItemMappingFunctions
     *
     * @param mf1 the first ItemMappingFunction, possibly null
     * @param mf2 the second ItemMappingFunction, possibly null
     * @return the composed function
     */
    protected static ItemMappingFunction chain(ItemMappingFunction mf1, ItemMappingFunction mf2) {
        if (mf1 == null) {
            return mf2;
        }
        if (mf2 == null) {
            return mf1;
        }
        return ItemMapper.of(item -> {
            Item temp = mf1.mapItem(item);
            if (temp != null) {
                return mf2.mapItem(temp);
            } else {
                return null;
            }
        });
    }

    protected XPathException coercionError(CoercionRequest request, Sequence value) {
        RoleDiagnostic role = request.roleSupplier.get();
        String representation = value instanceof Item ? Err.depict((Item)value) : value.toString();
        return new XPathException(
                "Failed to convert the " + role.getMessage() + ": " +
                        "Cannot convert " + representation + " to "
                        + request.requiredType)
                .withErrorCode("XPTY0004")
                .withLocation(request.locator);
    }


}


