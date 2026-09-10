////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.*;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.*;

import java.math.BigDecimal;

/**
 * This class implements the fn:compare() function, introduced in XPath 2.0 and heavily
 * extended in XPath 4.0
 */

public class Compare extends CollatingFunctionFixed {

    private static Int64Value compareStrings(StringValue s1, StringValue s2, AtomicComparer comparer) {
        if (s1 == null || s2 == null) {
            return null;
        }
        int result = comparer.compareAtomicValues(s1, s2);
        if (result < 0) {
            return Int64Value.MINUS_ONE;
        } else if (result > 0) {
            return Int64Value.PLUS_ONE;
        } else {
            return Int64Value.ZERO;
        }
    }

    private int getTypeCategory(BuiltInAtomicType t) {
        switch (t.getFingerprint()) {
            case StandardNames.XS_STRING:
            case StandardNames.XS_ANY_URI:
            case StandardNames.XS_UNTYPED_ATOMIC:
                return StandardNames.XS_STRING;
            case StandardNames.XS_FLOAT:
            case StandardNames.XS_DOUBLE:
            case StandardNames.XS_DECIMAL:
            case StandardNames.XS_INTEGER:
                return StandardNames.XS_NUMERIC;
            case StandardNames.XS_BOOLEAN:
            case StandardNames.XS_DATE:
            case StandardNames.XS_TIME:
            case StandardNames.XS_DATE_TIME:
            case StandardNames.XS_DAY_TIME_DURATION:
            case StandardNames.XS_YEAR_MONTH_DURATION:
                return t.getFingerprint();
            case StandardNames.XS_HEX_BINARY:
            case StandardNames.XS_BASE64_BINARY:
                return StandardNames.XS_HEX_BINARY;
            default:
                return -1;
        }
    }

    private void unorderedError(AtomicType t, int which) throws XPathException {
        throw new XPathException("fn:compare() - " +
                                         (which == 0 ? "first" : "second") +
                                         " argument is of type " + t.getDisplayName() +
                                         ", which has no defined ordering",
                                 "XPTY0004").asTypeError();

    }

    @Override
    public GroundedValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        AtomicValue arg0 = (AtomicValue) arguments[0].head();
        AtomicValue arg1 = (AtomicValue) arguments[1].head();
        if (arg0 == null || arg1 == null) {
            return EmptySequence.getInstance();
        }
        int p0 = getTypeCategory(arg0.getPrimitiveType());
        int p1 = getTypeCategory(arg1.getPrimitiveType());

        if (p0 == -1) {
            unorderedError(arg0.getItemType(), 0);
        }
        if (p1 == -1) {
            unorderedError(arg1.getItemType(), 1);
        }
        if (p0 != p1) {
            throw new XPathException("fn:compare(): cannot compare " +
                    arg0.getItemType() +
                    " to " +
                    arg1.getItemType(), "XPTY0004").asTypeError();
        }
        switch (p0) {
            case StandardNames.XS_STRING:
                GenericAtomicComparer comparer = new GenericAtomicComparer(getStringCollator(), context);
                return compareStrings((StringValue)arg0, (StringValue)arg1, comparer);
            case StandardNames.XS_NUMERIC:
                if (arg0.isNaN()) {
                    return arg1.isNaN() ? Int64Value.ZERO : Int64Value.MINUS_ONE;
                } else if (arg1.isNaN()) {
                    return Int64Value.PLUS_ONE;
                }
                if (arg0.getPrimitiveType() == arg1.getPrimitiveType()) {
                    int c = ((NumericValue) arg0).getXPathComparable().compareTo(
                            ((NumericValue) arg1).getXPathComparable());
                    return Int64Value.makeIntegerValue(c);
                } else {
                    try {
                        BigDecimal dec0 = ((NumericValue) arg0).getExactDecimalValue();
                        BigDecimal dec1 = ((NumericValue) arg1).getExactDecimalValue();
                        return Int64Value.makeIntegerValue(dec0.compareTo(dec1));
                    } catch (ValidationException e) {
                        // Typically one of the values is infinity
                        return Int64Value.makeIntegerValue(
                                Double.compare(
                                        ((NumericValue) arg0).getDoubleValue(),
                                        ((NumericValue) arg1).getDoubleValue()));
                    }
                }
            default:
                XPathComparable c0 = arg0.getXPathComparable(CodepointCollator.getInstance(), context.getImplicitTimezone());
                XPathComparable c1 = arg1.getXPathComparable(CodepointCollator.getInstance(), context.getImplicitTimezone());
                return Int64Value.makeIntegerValue(c0.compareTo(c1));
        }

    }

}

