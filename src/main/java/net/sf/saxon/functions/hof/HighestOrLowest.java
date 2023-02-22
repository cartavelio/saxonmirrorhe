package net.sf.saxon.functions.hof;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.DescendingComparer;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.StringToDouble;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.StringValue;

import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;

/**
 * This class implements the saxon:highest() and saxon:lowest() extension functions.
 * <p>The first argument is a sequence S, the second an optional collation, the third
 * a function F (defaulting to fn:data).
 * The function returns the item or items I in the input sequence S for which
 * F(I) has its highest/lowest value. </p>
 */

public class HighestOrLowest extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
     *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
     *                  If the argument is always a singleton, then the single item may be obtained by calling
     *                  <code>arguments[i].next()</code>. The implementation is not obliged to read all the items in each
     *                  <code>SequenceIterator</code> if they are not required to compute the result; but if any SequenceIterator is not read
     *                  to completion, it is good practice to call its close() method.
     * @return an iterator over the results of the function. If the result is a single item, it can be
     *         returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}. If the result is an empty sequence,
     *         the method should return <code>EmptyIterator.getInstance()</code>
     * @throws XPathException
     *          if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *          code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Configuration config = context.getConfiguration();

        Sequence input = arguments[0];
        String collationUri = null;
        if (arguments.length >= 2) {
            StringValue val = (StringValue) arguments[1].head();
            if (val != null) {
                try {
                    collationUri = ResolveURI.makeAbsolute(val.getStringValue(), getStaticBaseUriString()).toString();
                } catch (URISyntaxException e) {
                    throw new XPathException(e);
                }
            }
        }
        if (collationUri == null) {
            collationUri = getRetainedStaticContext().getDefaultCollationName();
        }

        StringCollator collator = config.getCollation(collationUri);
        if (collator == null) {
            XPathException e = new XPathException("Unknown collation " + getRetainedStaticContext().getDefaultCollationName());
            e.setErrorCode("FOCH0002");
            throw e;
        }
        AtomicComparer comparer = new GenericAtomicComparer(collator, context);
        if (getFunctionName().getLocalPart().equals("lowest")) {
            comparer = new DescendingComparer(comparer);
        }

        FunctionItem keyFn;
        if (arguments.length >= 3) {
            keyFn = (FunctionItem) arguments[2].head();
        } else {
            keyFn = SystemFunction.makeFunction("data", getRetainedStaticContext(), 1);
        }

        Sequence[] args = new Sequence[1];
        AtomicValue maxSoFar = null;
        List<Item> highestSoFar = new ArrayList<>();

        SequenceIterator iter = arguments[0].iterate();
        Item next;
        while ((next = iter.next()) != null) {
            args[0] = next;
            Sequence key = dynamicCall(keyFn, context, args);
            Item head = key.head();
            if (head != null) {
                if (!(head instanceof AtomicValue)) {
                    throw new XPathException(
                            "Result of function supplied as second argument to " +
                                    getFunctionName().getDisplayName() +
                                    " must be an atomic value", "XPTY0004");
                }
                AtomicValue keyVal = (AtomicValue)key.head();
                if (keyVal.isUntypedAtomic()) {
                    StringToDouble converter = context.getConfiguration().getConversionRules().getStringToDoubleConverter();
                    try {
                        keyVal = new DoubleValue(converter.stringToNumber(keyVal.getUnicodeStringValue()));
                    } catch (NumberFormatException e) {
                        XPathException de = new XPathException("Failed to convert key to xs:double: " + e.getMessage(), "FORG0001");
                        de.setXPathContext(context);
                        throw de;
                    }
                }
                if (keyVal.isNaN()) {
                    continue;
                }
                if (!keyVal.getPrimitiveType().isOrdered(false)) {
                    XPathException de = new XPathException("Type " + keyVal.getPrimitiveType() + " is not an ordered type");
                    de.setErrorCode("FORG0006");
                    de.setIsTypeError(true);
                    de.setXPathContext(context);
                    throw de;
                }
                if (maxSoFar == null) {
                    highestSoFar.add(next);
                    maxSoFar = keyVal;
                } else {
                    int result;
                    try {
                        result = comparer.compareAtomicValues(keyVal, maxSoFar);
                    } catch (ClassCastException e) {
                        throw new XPathException(
                                "Keys computed for " + getFunctionName().getDisplayName() + " are not comparable", "FORG0006");
                    }
                    if (result > 0) {
                        highestSoFar.clear();
                        highestSoFar.add(next);
                        maxSoFar = keyVal;
                    } else if (result == 0) {
                        highestSoFar.add(next);
                    }
                }
            }
        }
        return new SequenceExtent.Of<>(highestSoFar);
    }


}

// Copyright (c) 2009-2022 Saxonica Limited
