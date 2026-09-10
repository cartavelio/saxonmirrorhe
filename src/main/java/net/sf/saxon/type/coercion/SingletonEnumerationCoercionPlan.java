package net.sf.saxon.type.coercion;

import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SingletonEnumType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

/**
 * Coercion plan for use with 4.0 when the required item type is a singleton enumeration type
 */
public class EnumerationCoercionPlan extends AtomicCoercionPlan {

    public final static EnumerationCoercionPlan INSTANCE = new EnumerationCoercionPlan();

    @Override
    protected AtomicValue promote(AtomicValue atom, CoercionRequest request) throws XPathException {
        SingletonEnumType enumType = (SingletonEnumType)request.requiredType;
        TypeHierarchy th = request.config.getTypeHierarchy();
        if (enumType.matches(atom, th)) {
            return atom;
        } else if (enumType.matchesString(atom.getUnicodeStringValue())) {
            return new StringValue(atom.getUnicodeStringValue(), enumType);
        } else {
            throw coercionError(atom, request);
        }
    }



}


