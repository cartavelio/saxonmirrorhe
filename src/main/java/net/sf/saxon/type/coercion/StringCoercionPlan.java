package net.sf.saxon.type.coercion;

import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

/**
 * Coercion plan for use when the required item type is xs:string
 */
public class StringCoercionPlan extends AtomicCoercionPlan {

    private static final StringCoercionPlan INSTANCE = new StringCoercionPlan();

    public static StringCoercionPlan getInstance() {
        return INSTANCE;
    }


    @Override
    protected AtomicValue promote(AtomicValue item, ItemType requiredType, CoercionRequest request) throws XPathException {
        if (item instanceof AnyURIValue) {
            return new StringValue(item.getUnicodeStringValue());
        } else {
            return item;
        }
    }

}


