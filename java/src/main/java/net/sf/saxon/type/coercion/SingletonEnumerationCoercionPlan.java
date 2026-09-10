package net.sf.saxon.type.coercion;

import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.SingletonEnumType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

/**
 * Coercion plan for use with 4.0 when the required item type is a singleton enumeration type
 */
public class SingletonEnumerationCoercionPlan extends AtomicCoercionPlan {

    public final static SingletonEnumerationCoercionPlan INSTANCE = new SingletonEnumerationCoercionPlan();

    @Override
    protected AtomicValue convertUntyped(AtomicValue atom, ItemType requiredType, CoercionRequest request) throws XPathException {
        SingletonEnumType enumType = (SingletonEnumType) requiredType;
        if (enumType.matchesString(atom.getUnicodeStringValue())) {
            return new StringValue(atom.getUnicodeStringValue(), enumType);
        } else {
            throw coercionError(atom, requiredType, request);
        }
    }
    @Override
    protected AtomicValue promote(AtomicValue atom, ItemType requiredType, CoercionRequest request) throws XPathException {
        SingletonEnumType enumType = (SingletonEnumType)requiredType;
        TypeHierarchy th = request.config.getTypeHierarchy();
        if (enumType.matches(atom, th)) {
            return atom;
        } else if (enumType.matchesString(atom.getUnicodeStringValue())) {
            return new StringValue(atom.getUnicodeStringValue(), enumType);
        } else {
            throw coercionError(atom, requiredType, request);
        }
    }

    // TODO: when coercing a string to an enumeration type, we're throwing exceptions for every value not matched.



}


