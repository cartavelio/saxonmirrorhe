package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.GeneralComparison;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.OperatorSymbol;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.NumericComparer;
import net.sf.saxon.expr.sort.NumericComparer11;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Predicate;

/**
 * This class is used when a streaming template requests a general comparison with one operand
 * being pushed from the streamed input document, and the other being evaluated in pull mode.
 * It is used for the operators lt, le, gt, ge.
 *
 * <p>The unstreamed operand is evaluated when the first streamed item is encountered.
 * At this point we build a list of all the values from the unstreamed operand, unconverted,
 * plus the min and max of all numeric values including all untyped values that are
 * castable to numeric.</p>
 *
 * <p>Each streamed input value is then compared against this data. If the streamed input
 * value is numeric, or is untyped and castable to numeric, then it is compared with the
 * min and max numeric values. Otherwise, it is compared against the complete list of
 * unconverted values.</p>
 */


public class GeneralComparisonMinimaxFeed extends AtomicItemFeed {

    private WatchManager watchManager;
    private AtomicComparer comparer;
    private List<AtomicValue> unstreamedValues;
    private NumericValue minUnstreamedNumeric;
    private NumericValue maxUnstreamedNumeric;
    private OperatorSymbol operator;
    private boolean done = false;
    private boolean closed = false;
    private final Expression pullOperand;
    private Predicate<Integer> predicate;

    public GeneralComparisonMinimaxFeed(WatchManager watchManager, Expression expr, int pushedArg, ItemFeed result, XPathContext context) {
        super(expr, result, context);
        this.watchManager = watchManager;
        final GeneralComparison gc = (GeneralComparison) getExpression();
        comparer = gc.getAtomicComparer();
        operator = gc.getSingletonOperator();
        // Adjust the expression so the streamed argument is effectively first
        if (pushedArg == 1) {
            operator = OperatorSymbol.inverse(operator);
        }
        pullOperand = pushedArg==0 ? gc.getRhsExpression() : gc.getLhsExpression();
        predicate = makeTest(operator);
    }

    private static Predicate<Integer> makeTest(OperatorSymbol operator) {
        switch (operator) {
            case FLT:
                return i -> i<0;
            case FLE:
                return i -> i<=0;
            case FGT:
                return i -> i>0;
            case FGE:
                return i -> i>0;
            default:
                throw new IllegalArgumentException();
        }
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        done = false;
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
            if (unstreamedValues == null) {
                // when the first streamed value is encountered, evaluate the unstreamed operand
                // TODO: compare these values with the first streamed value as we read them
                unstreamedValues = new ArrayList<>();
                SequenceIterator unstreamed = pullOperand.iterate(context);
                Item it;
                while ((it = unstreamed.next()) != null) {
                    AtomicValue val = (AtomicValue) it;
                    unstreamedValues.add(val);
                    if (val.isUntypedAtomic()) {
                        // try converting to numeric; but no failure if this isn't possible
                        String str = Whitespace.trim(val.getStringValue());
                        if (str.matches("^[-+0-9\\.]*$")) {
                            NumericValue nv = NumericValue.parseNumber(str);
                            if (!nv.isNaN()) {
                                val = nv;
                            }
                        }
                    }
                    if (val instanceof NumericValue) {
                        // Compute the min and max unstreamed values.
                        // Note: technically, if the operator is LT/LE we only need the max,
                        // while if it is GT/GE then we only need the min.
                        if (minUnstreamedNumeric == null) {
                            minUnstreamedNumeric = (NumericValue) val;
                            maxUnstreamedNumeric = (NumericValue) val;
                        } else {
                            NumericComparer comp = NumericComparer11.getInstance();
                            if (comp.compareAtomicValues(val, minUnstreamedNumeric) < 0) {
                                minUnstreamedNumeric = (NumericValue) val;
                            }
                            if (comp.compareAtomicValues(val, maxUnstreamedNumeric) > 0) {
                                maxUnstreamedNumeric = (NumericValue) val;
                            }
                        }
                    }
                }
                if (unstreamedValues.isEmpty()) {
                    reportDone(false);
                    return;
                }
            }
            AtomicValue streamedVal = (AtomicValue) item;
            if (streamedVal.isUntypedAtomic() && minUnstreamedNumeric != null) {
                streamedVal = new DoubleValue(StringToDouble11.getInstance().stringToNumber(streamedVal.getUnicodeStringValue()));
            }
            if (streamedVal instanceof NumericValue) {
                NumericComparer comparer = NumericComparer11.getInstance();
                switch (operator) {
                    case FLE:
                        if (comparer.compareAtomicValues(streamedVal, maxUnstreamedNumeric) <= 0) {
                            reportDone(true);
                            return;
                        }
                        break;
                    case FLT:
                        if (comparer.compareAtomicValues(streamedVal, maxUnstreamedNumeric) < 0) {
                            reportDone(true);
                            return;
                        }
                        break;
                    case FGE:
                        if (comparer.compareAtomicValues(streamedVal, minUnstreamedNumeric) >= 0) {
                            reportDone(true);
                            return;
                        }
                        break;
                    case FGT:
                        if (comparer.compareAtomicValues(streamedVal, minUnstreamedNumeric) > 0) {
                            reportDone(true);
                            return;
                        }
                        break;
                    default:
                        throw new IllegalStateException();
                }
            } else {
                // TODO: invert the comparer if we inverted the operand
                for (AtomicValue unstreamedVal : unstreamedValues) {
                    int comp = comparer.compareAtomicValues(streamedVal, unstreamedVal);
                    if (predicate.test(comp)) {
                        reportDone(true);
                        return;
                    }
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
            minUnstreamedNumeric = null;
            maxUnstreamedNumeric = null;
            done = false;
        }
    }
}

// Copyright (c) 2010-2023 Saxonica Limited