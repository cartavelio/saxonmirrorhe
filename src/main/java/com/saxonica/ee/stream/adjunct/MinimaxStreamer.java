package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.AtomicItemFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.DescendingComparer;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.functions.CollatingFunctionFree;
import net.sf.saxon.functions.Minimax;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.FloatValue;


/**
 * Generate streaming code for the min() and max() functions.
 */
public class MinimaxStreamer extends StreamingAdjunct implements FeedMaker {

    @Override
    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : getDefaultFeedMaker(arg);
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new MinimaxFeed(getExpression(), out, context);
    }

    private static class MinimaxFeed extends AtomicItemFeed {

        /*@Nullable*/ private AtomicValue min;
        private AtomicComparer atomicComparer;
        private boolean foundDouble;
        private boolean foundFloat;
        private boolean foundNaN;
        private AtomicType lowestCommonSuperType;
        private boolean ignoreNaN;


        public MinimaxFeed(Expression expr, ItemFeed result, XPathContext context) {
            super(expr, result, context);
        }

        @Override
        public void open(Terminator terminator) throws XPathException {
            SystemFunctionCall call = (SystemFunctionCall) getExpression();
            SystemFunction fn = call.getTargetFunction();
            ignoreNaN = fn instanceof Minimax && ((Minimax) call.getTargetFunction()).isIgnoreNaN();
            min = null;
            foundDouble = false;
            foundFloat = false;
            foundNaN = false;
            lowestCommonSuperType = null;
            getResultFeed().open(terminator);
        }

        /**
         * Supply an item to the feed
         *
         * @param item the item being fed
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        @Override
        public void append(Item item) throws XPathException {
            if (!hasFailed()) {

                AtomicValue value = (AtomicValue) item;
                TypeHierarchy th = getContext().getConfiguration().getTypeHierarchy();

                // Process the sequence, retaining the min (or max) so far. This will be an actual value found
                // in the sequence. At the same time, remember if a double and/or float has been encountered
                // anywhere in the sequence, and if so, convert the min/max to double/float at the end. This is
                // done to avoid problems if a decimal is converted first to a float and then to a double.

                // Get the first value in the sequence, ignoring any NaN values if we are ignoring NaN values

                if (value.isUntypedAtomic()) {
                    try {
                        StringToDouble converter = getContext().getConfiguration().getConversionRules().getStringToDoubleConverter();
                        value = new DoubleValue(converter.stringToNumber(value.getUnicodeStringValue()));
                        foundDouble = true;
                    } catch (NumberFormatException e) {
                        dynamicError(
                                new XPathException("Failure converting " + Err.wrap(value.getUnicodeStringValue())
                                                           + " to a number")
                                            .withErrorCode("FORG0001")
                                            .withXPathContext(getContext()));
                        return;
                    }
                } else {
                    if (value instanceof DoubleValue) {
                        foundDouble = true;
                    } else if (value instanceof FloatValue) {
                        foundFloat = true;
                    }
                }

                if (value.isNaN()) {
                    // if there's a NaN in the sequence, return NaN, unless ignoreNaN is set
                    if (ignoreNaN) {
                        return;   // ignore the NaN
                    } else {
                        foundNaN = true;
                    }
                } else {
                    if (!value.getPrimitiveType().isOrdered(false)) {
                        XPathException de = new XPathException("Type " + value.getPrimitiveType() + " is not an ordered type")
                                .withErrorCode("FORG0006").withXPathContext(getContext());
                        de.setIsTypeError(true);
                        dynamicError(de);
                        return;
                    }
                }

                if (min == null) {
                    // first item
                    min = value;
                    lowestCommonSuperType = value.getItemType();

                } else {
                    // subsequent item
                    if (atomicComparer == null) {
                        SystemFunctionCall call = (SystemFunctionCall) getExpression();
                        SystemFunction targetFunction = call.getTargetFunction();
                        boolean isMax = false;
                        if (targetFunction instanceof Minimax) {
                            atomicComparer = ((Minimax) targetFunction).getAtomicComparer(getContext());
                            isMax = targetFunction instanceof Minimax.Max;
                        } else {
                            assert targetFunction instanceof CollatingFunctionFree;
                            Expression collationArg = call.getArg(call.getArity() - 1);
                            String collationName = collationArg.evaluateAsString(getContext()).toString();
                            StringCollator collator = getContext().getConfiguration().getCollation(collationName);
                            if (collator == null) {
                                dynamicError(new XPathException("Unknown collation " + collationName, "FOCH0002"));
                            }
                            atomicComparer = new GenericAtomicComparer(collator, getContext());
                            isMax = targetFunction.getFunctionName().getLocalPart().equals("max");
                        }
                        // For the max function, reverse the collator
                        if (isMax) {
                            atomicComparer = new DescendingComparer(atomicComparer);
                        }
                    }

                    lowestCommonSuperType = (AtomicType) Type.getCommonSuperType(
                            lowestCommonSuperType, value.getItemType(), th);

                    try {
                        if (atomicComparer.compareAtomicValues(value, min) < 0) {
                            min = value;
                        }
                    } catch (XPathException err) {
                        dynamicError(err);
                    } catch (ClassCastException err) {
                        XPathException de = new XPathException("Cannot compare " + min.getItemType() + " with " + value.getItemType())
                            .withErrorCode("FORG0006").withXPathContext(getContext());
                        de.setIsTypeError(true);
                        dynamicError(de);
                    }
                }
            }

        }

        /**
         * Signal the end of the sequence of items
         *
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        @Override
        public void close() throws XPathException {
            if (!hasFailed()) {
                if (min == null) {
                    // result is empty sequence
                } else {
                    if (foundNaN) {
                        min = FloatValue.NaN;
                    }
                    final ConversionRules rules = getContext().getConfiguration().getConversionRules();
                    if (foundDouble) {
                        if (!(min instanceof DoubleValue)) {
                            min = Converter.convert(min, BuiltInAtomicType.DOUBLE, rules);
                        }
                    } else if (foundFloat) {
                        if (!(min instanceof FloatValue)) {
                            min = Converter.convert(min, BuiltInAtomicType.FLOAT, rules);
                        }
                    }
                    if (lowestCommonSuperType != NumericType.getInstance()) {
                        min = Converter.convert(min, lowestCommonSuperType, rules);
                    }
                    getNextOutputter().append(min);
                }
                getNextOutputter().close();
            }
        }

    }
}

// Copyright (c) 2010-2023 Saxonica Limited