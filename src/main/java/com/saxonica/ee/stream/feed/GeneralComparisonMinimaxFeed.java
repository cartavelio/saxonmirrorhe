package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.GeneralComparison;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.XPathComparable;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringItem;

import java.util.List;

/**
 * This class is used when a streaming template requests a general comparison with one operand
 * being pushed from the streamed input document, and the other being evaluated in pull mode.
 * It is used for the operators lt, le, gt, ge. In general we only need to remember the min
 * and max values encountered so far; except that we also need to remember all untyped values,
 * because we may have to convert them when a typed value is encountered. We only
 * have to remember the min and max values for one data type: if there is more than
 * one (other than untyped) then we can always report a type error.
 */


public class GeneralComparisonMinimaxFeed extends AtomicItemFeed {

    private final StringCollator collator;
    BuiltInAtomicType matchType;
    private XPathComparable minStreamed;
    private XPathComparable maxStreamed;
    private XPathComparable minUnstreamed;
    private XPathComparable maxUnstreamed;
    private List<StringItem> unstreamedUntypedValues;
    private int operator;
    private boolean done = false;
    private final Expression pullOperand;
    private final int implicitTimezone;

    public GeneralComparisonMinimaxFeed(Expression expr, int pushedArg, ItemFeed result, XPathContext context) {
        super(expr, result, context);
        final GeneralComparison gc = (GeneralComparison) getExpression();
        collator = gc.getAtomicComparer().getCollator();
        operator = gc.getSingletonOperator();
        if (pushedArg == 1) {
            operator = Token.inverse(operator);
        }
        pullOperand = pushedArg==0 ? gc.getRhsExpression() : gc.getLhsExpression();
        implicitTimezone = context.getImplicitTimezone();
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        done = false;
        minStreamed = null;
        maxStreamed = null;
        minUnstreamed = null;
        maxUnstreamed = null;
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
            boolean ordered = operator != Token.FEQ && operator != Token.FNE;
            AtomicValue next = (AtomicValue) item;
            XPathComparable k1 = next.getXPathComparable(collator, implicitTimezone);
            if (minStreamed == null) {
                if (next instanceof NumericValue) {
                    matchType = BuiltInAtomicType.DOUBLE;
                } else {
                    matchType = ((AtomicValue) item).getPrimitiveType();
                }
                SequenceIterator comparand = pullOperand.iterate(context);
                AtomicValue val;
                boolean existsUnstreamed = false;
                while ((val = (AtomicValue) comparand.next()) != null) {
                    if (val.isUntypedAtomic()) {
                        unstreamedUntypedValues.add((StringItem) val);
                    } else {
                        XPathComparable k2 = val.getXPathComparable(collator, implicitTimezone);
                        if (!existsUnstreamed) {
                            existsUnstreamed = true;
                            minUnstreamed = k2;
                            maxUnstreamed = k2;
                        } else {
                            if (k2.compareTo(minUnstreamed) < 0) {
                                minUnstreamed = k2;
                            }
                            if (maxUnstreamed != null && k2.compareTo(maxUnstreamed) > 0) {
                                maxUnstreamed = k2;
                            }
                        }
                    }
                }
                if (!existsUnstreamed) {
                    reportDone(false);
                }
            }
            if (next.isUntypedAtomic()) {
                //TODO
            } else {
                switch (operator) {
                    case Token.FLT:
                        if (k1.compareTo(maxUnstreamed) < 0) {
                            reportDone(true);
                        }
                        break;
                    case Token.FLE:
                        if (k1.compareTo(maxUnstreamed) <= 0) {
                            reportDone(true);
                        }
                        break;
                    case Token.FGT:
                        if (k1.compareTo(minUnstreamed) > 0) {
                            reportDone(true);
                        }
                        break;
                    case Token.FGE:
                        if (k1.compareTo(minUnstreamed) >= 0) {
                            reportDone(true);
                        }
                        break;
                }
            }
        }


    }

    private void reportDone(boolean result) throws XPathException {
        Outputter out = getNextOutputter();
        done = true;
        out.append(BooleanValue.get(result));
        out.close();
        getTerminator().terminate();
    }

    @Override
    public void close() throws XPathException {
        if (!done) {
            // If we've come to the end without finding an item that meets the conditions, report the result false
            getNextOutputter().append(BooleanValue.FALSE);
            super.close();
            minStreamed = null;
            maxStreamed = null;
            minUnstreamed = null;
            maxUnstreamed = null;

        }

    }
}

// Copyright (c) 2010-2020 Saxonica Limited