package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.GeneralComparison;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;

/**
 * This class is used when a streaming template requests a general comparison with one operand
 * being pushed from the streamed input document, and the other being evaluated in pull mode;
 * used when the operator is "!=" which means we only need to remember one value (the result is true
 * as soon as we find two values that differ).
 */


public class GeneralComparisonInequalityFeed extends AtomicItemFeed {

    private WatchManager watchManager;
    private final StringCollator collator;
    private AtomicMatchKey firstStreamed = null;
    private StringValue firstUntypedStreamed = null;
    private boolean done = false;
    private boolean closed = false;
    private final Expression pullOperand;
    private final int implicitTimezone;

    // Algorithm: When the first value is encountered from the streamed operand,
    // we evaluate the unstreamed operand. If the unstreamed operand is empty, we can immediately
    // return false. Otherwise, if the unstreamed value includes a value that is not equal to the
    // first streamed value, we can immediately return true. If this doesn't immediately produce
    // a result, then we only need to compare subsequent streamed values with the first streamed
    // value; the result is true as soon as one differs.

    // The only complication is untyped values. In principle, we don't know how to convert these
    // until we've seen all values from the other operand. However, if both values are non-empty,
    // then the result is false if the other operand contains more than one distinct value, which
    // limits what we need to retain.

    public GeneralComparisonInequalityFeed(WatchManager watchManager, Expression expr, int pushedArg, ItemFeed result, XPathContext context) {
        super(expr, result, context);
        this.watchManager = watchManager;
        final GeneralComparison gc = (GeneralComparison) getExpression();
        collator = gc.getStringCollator();
        pullOperand = pushedArg==0 ? gc.getRhsExpression() : gc.getLhsExpression();
        implicitTimezone = context.getImplicitTimezone();
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        done = false;
        firstStreamed = null;
        firstUntypedStreamed = null;
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed (in this case, an item in the "pushed" operand
     *             sequence that is being compared
     * @throws XPathException
     *          to report any failure
     */

    @Override
    public void append(Item item) throws XPathException {
        if (!done) {
            // If we haven't already reported a result of true
            XPathContext context = getContext();
            ConversionRules rules = context.getConfiguration().getConversionRules();
            AtomicValue next = (AtomicValue) item;
            AtomicMatchKey ks1 = next.getXPathMatchKey(collator, implicitTimezone);
            BuiltInAtomicType matchType;
            if (next instanceof NumericValue) {
                matchType = BuiltInAtomicType.DOUBLE;
            } else {
                matchType = ((AtomicValue) item).getPrimitiveType();
            }
            if (firstStreamed == null) {
                firstStreamed = ks1;
                if (next.isUntypedAtomic()) {
                    firstUntypedStreamed = (StringValue)next;
                }
                SequenceIterator comparand = pullOperand.iterate(context);
                AtomicValue val;
                boolean existsUnstreamed = false;
                while ((val = (AtomicValue) comparand.next()) != null) {
                    existsUnstreamed = true;
                    if (!Type.isGenerallyComparable(matchType, val.getPrimitiveType(), false)) {
                        throw new XPathException("Comparison between " + matchType + " and " + val.getPrimitiveType() + " is not allowed",
                                                 "XPTY0004", getExpression().getLocation());
                    }
                    if (val.isUntypedAtomic()) {
                        AtomicValue cval = Converter.convert(firstUntypedStreamed, matchType, rules);
                        AtomicMatchKey ku1 = cval.getXPathMatchKey(collator, implicitTimezone);
                        if (!ku1.equals(ks1)) {
                            reportDone(true);
                            break;
                        }
                    } else {

                        AtomicMatchKey ku2 = val.getXPathMatchKey(collator, implicitTimezone);
                        if (!ks1.equals(ku2)) {
                            reportDone(true);
                            break;
                        }
                        if (firstUntypedStreamed != null) {
                            AtomicValue cval = Converter.convert(firstUntypedStreamed, matchType, rules);
                            AtomicMatchKey ks2 = cval.getXPathMatchKey(collator, implicitTimezone);
                            if (ku2 != ks2) {
                                reportDone(true);
                                break;
                            }
                        }
                    }
                }
                if (!existsUnstreamed) {
                    reportDone(false);
                }
            } else {
                if (ks1 != firstStreamed) {
                    reportDone(true);
                }
            }
        }
    }

    private void reportDone(boolean result) throws XPathException {
        Outputter out = getNextOutputter();
        done = true;
        out.append(BooleanValue.get(result));
        if (watchManager.allowsEarlyExit()) {
            closed = true;
            out.close();
            getTerminator().terminate();
        }
    }


    @Override
    public void close() throws XPathException {
        if (!done) {
            // If we've come to the end without finding an item that meets the conditions, report the result false
            getNextOutputter().append(BooleanValue.FALSE);
        }
        if (!closed) {
            super.close();
            firstStreamed = null;
            firstUntypedStreamed = null;
        }

    }
}

// Copyright (c) 2010-2022 Saxonica Limited