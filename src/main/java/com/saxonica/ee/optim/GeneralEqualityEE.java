package com.saxonica.ee.optim;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.functions.InsertBefore;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.TwoItemIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;

/**
 * This class provides an implementation of the GeneralComparison "=" operator, which deals efficiently with
 * comparing two sequences.
 */
public class GeneralComparisonEE extends GeneralComparison {

    // TODO: many-to-many comparisons are very rarely encountered in practice, so the complexity of this
    // implementation is hard to justify. In particular, the strays mechanism; especially as this seems
    // to render common cases inefficient (one side untypedAtomic, one side all numeric, say). Suggestion:
    // (a) use this class only in cases where both sides have a well-defined static type (not xs:anyAtomic),
    // (b) if one side is statically untypedAtomic, add a converter to the static type of the other
    // argument.

    /**
     * Create a relational expression identifying the two operands and the operator
     *
     * @param p0 the left-hand operand
     * @param op the operator, as a token returned by the Tokenizer (e.g. Token.LT)
     * @param p1 the right-hand operand
     */

    public GeneralComparisonEE(Expression p0, int op, Expression p1) {
        super(p0, op, p1);
    }

    /*@NotNull*/
    @Override
    public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) throws XPathException {
        // if either operand is an indexed variable, replace the comparison A=V by the indexed filter
        // expression exists(V[.=A])

        if (singletonOperator == Token.FEQ && !(getParentExpression() instanceof FilterExpression)) {
            Expression lhs = getLhsExpression();
            Expression rhs = getRhsExpression();
            Expression e2 = tryIndexedComparison(lhs, rhs);
            if (e2 != null) {
                return e2.typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
            }
            e2 = tryIndexedComparison(rhs, lhs);
            if (e2 != null) {
                return e2.typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
            }
//            if (isIndexedVariable(getLhsExpression())) {
//                return makeIndexTest((VariableReference) getLhsExpression(), getRhsExpression(), visitor, contextItemType);
//            } else if (isIndexedVariable(getRhsExpression())) {
//                return makeIndexTest((VariableReference) getRhsExpression(), getLhsExpression(), visitor, contextItemType);
//            }
        }
        return super.optimize(visitor, contextItemType);
    }

    private Expression tryIndexedComparison(Expression lhs, Expression rhs) {
        if (lhs instanceof VariableReference &&
                Cardinality.allowsMany(lhs.getCardinality()) &&
                (rhs.getDependencies() & StaticProperty.DEPENDS_ON_FOCUS) == 0) {
            // Rewrite ($x = V) as exists($x[. = V]), with potential for indexed evaluation
            Expression predicate = new GeneralComparisonEE(
                    new ContextItemExpression(), Token.EQUALS, rhs);
            Expression filterExp = new FilterExpression(lhs, predicate);
            return SystemFunction.makeCall(
                    "exists", getRetainedStaticContext(), filterExp);
        } else {
            return null;
        }
    }

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        GeneralComparisonEE gc = new GeneralComparisonEE(getLhsExpression().copy(rebindings), operator, getRhsExpression().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, gc);
        gc.setRetainedStaticContext(getRetainedStaticContext());
        gc.comparer = comparer;
        gc.singletonOperator = singletonOperator;
        gc.runtimeCheckNeeded = runtimeCheckNeeded;
        gc.comparisonCardinality = comparisonCardinality;
        return gc;
    }

    /**
     * Evaluate the expression in a boolean context
     *
     * @param context the given context for evaluation
     * @return a boolean representing the result of the numeric comparison of the two operands
     */

    @Override
    public boolean effectiveBooleanValue(XPathContext context) throws XPathException {
        // Avoid the general hash join routine if either sequence is empty or a singleton

        SequenceIterator lhsIter = getLhsExpression().iterate(context);
        Item lhsFirst = lhsIter.next();
        if (lhsFirst == null) {
            return false;
        }

        SequenceIterator rhsIter = getRhsExpression().iterate(context);
        Item rhsFirst = rhsIter.next();
        if (rhsFirst == null) {
            lhsIter.close();
            return false;
        }

        // Compare L1 = R1
        if (compareOneToOne((AtomicValue)lhsFirst, (AtomicValue)rhsFirst, context)) {
            lhsIter.close();
            rhsIter.close();
            return true;
        }

        Item rhsSecond = rhsIter.next();
        if (rhsSecond == null) {
            // Compare L2..N = R1
            return compareManyToOne(lhsIter, (AtomicValue)rhsFirst, context);
        }

        Item lhsSecond = lhsIter.next();
        if (lhsSecond == null) {
            // Compare L1 = R2
            if (compareOneToOne((AtomicValue) lhsFirst, (AtomicValue) rhsSecond, context)) {
                lhsIter.close();
                rhsIter.close();
                return true;
            }
            // Compare L1 = R3..N
            return compareOneToMany((AtomicValue)lhsFirst, rhsIter, context);
        }

        // Both operands contain 2 or more values (this is rare!)
        return hashJoin(prependTwo(lhsFirst, lhsSecond, lhsIter), prependTwo(rhsFirst, rhsSecond, rhsIter), context, comparer);
    }

    private static SequenceIterator prependTwo(Item first, Item second, SequenceIterator rest) {
        return new InsertBefore.InsertIterator(rest, new TwoItemIterator(first, second), 1);
    }

    private boolean compareOneToOne(AtomicValue lhsValue, AtomicValue rhsValue, XPathContext context) throws XPathException {
        if (runtimeCheckNeeded && !UType.isGenerallyComparable(lhsValue.getUType(), rhsValue.getUType())) {
            XPathException e2 = new XPathException("Cannot compare " + Type.displayTypeName(lhsValue) +
                                                           " to " + Type.displayTypeName(rhsValue));
            e2.setErrorCode("XPTY0004");
            e2.setLocation(getLocation());
            e2.setIsTypeError(true);
            throw e2;
        }
        return compare(lhsValue, singletonOperator, rhsValue, comparer.provideContext(context),
                       false, context, getRetainedStaticContext());
    }

    private boolean compareManyToOne(SequenceIterator lhsIter, AtomicValue rhsValue, XPathContext context) throws XPathException {
        AtomicValue lhsValue;
        while ((lhsValue = (AtomicValue) lhsIter.next()) != null) {
            if (compareOneToOne(lhsValue, rhsValue, context)) {
                lhsIter.close();
                return true;
            }
        }
        return false;
    }

    private boolean compareOneToMany(AtomicValue lhsValue, SequenceIterator rhsIter, XPathContext context) throws XPathException {
        AtomicValue rhsValue;
        while ((rhsValue = (AtomicValue) rhsIter.next()) != null) {
            if (compareOneToOne(lhsValue, rhsValue, context)) {
                rhsIter.close();
                return true;
            }
        }
        return false;
    }

    /**
     * Implement the XPath "=" operator for comparing two arbitrary sequences. The comparison is true
     * if any item in the first sequence is equal to any item from the second. This implementation uses
     * a hash join algorithm, modified to cater for the fact that untypedAtomic values act as "wild cards".
     * This algorithm does not handle the special rules for backwards compatibility mode, and is not
     * invoked if BCM is in force.
     *
     * @param iter1    The first sequence
     * @param iter2    The second sequence
     * @param context  The dynamic context
     * @param comparer The comparer (which embodies the collation to be used)
     * @return true if there is a pair of values that are equal, false if not
     * @throws XPathException if there is a pair of non-comparable values, or if an untyped atomic value
     *                        cannot be cast to the type of the other operand.
     */
    private boolean hashJoin(SequenceIterator iter1, SequenceIterator iter2, XPathContext context, AtomicComparer comparer)
            throws XPathException {

        // The complication here is with untyped data: an untyped item in one sequence needs
        // to be converted to the type of the value it is being compared with: so in an
        // expression such as (U, U, U) = (1, "paris") the untyped values need to be converted
        // to a number for comparison with 1, and to a string for comparison with "paris". This
        // makes it difficult to use a conventional hash-join.

        // The solution is that we build a hash-table based on the item types of the first
        // items in each sequence. If these are both untyped, we choose string. If there are
        // values encountered of a different type, we store them in a separate list, and compare
        // them with every item in the other operand.

        // We don't know which list is longer (often one operand will be a singleton), so we
        // read from the two sequences alternately until one is exhausted. This strategy ensures
        // that the amount of data kept in memory is always twice the size of the shorter
        // of the two sequences, which works well in the common case where one of the sequences
        // is of length one.

        //System.err.println("**************** USING HASH JOIN");

        // Note: untypedAtomic values go both into the hashtable and into the strays list. This is inefficient
        // if all values are untypedAtomic. But then, we are invoking this algorithm only in the schema-aware
        // product, so perhaps untypedAtomic values will be relatively uncommon.

        // Get the first item in each sequence, return false if either is empty

        AtomicValue first1 = (AtomicValue) iter1.next();
        if (first1 == null) {
            iter2.close();
            return false;
        }
        AtomicValue first2 = (AtomicValue) iter2.next();
        if (first2 == null) {
            iter1.close();
            return false;
        }

        final ConversionRules rules = context.getConfiguration().getConversionRules();
        AtomicComparer boundComparer = comparer.provideContext(context);

        // Determine the primitive types of these first items, treating all numbers as "numeric"

        UType ut1 = first1.getItemType().getUType();
        if (UType.NUMERIC.subsumes(ut1)) {
            ut1 = UType.NUMERIC;
        }
        UType ut2 = first2.getItemType().getUType();
        if (UType.NUMERIC.subsumes(ut2)) {
            ut2 = UType.NUMERIC;
        }

        // Decide what type of values to keep in the hashtables, guessing that the sequences will be homogeneous

        UType hashTableType;
        if (ut1.equals(ut2)) {
            hashTableType = ut1;
        } else {
            if (ut1.equals(UType.UNTYPED_ATOMIC)) {
                hashTableType = ut2;
            } else {
                hashTableType = ut1;
            }
        }
        StringConverter untypedConverter;
        if (hashTableType.equals(UType.NUMERIC)) {
            untypedConverter = rules.getStringToDoubleConverter();
        } else {
            untypedConverter = ((BuiltInAtomicType)hashTableType.toItemType()).getStringConverter(rules);
        }
        StringCollator collator = comparer.getCollator();

        int implicitTimezone = context.getImplicitTimezone();

        HashSet<AtomicMatchKey> set1 = new HashSet<>(20);  // set1 will always contain at least one value
        HashSet<AtomicMatchKey> set2 = null;             // the other collections are created on demand
        List<AtomicValue> strays1 = null; //contains comparison keys
        List<AtomicValue> strays2 = null; //contains comparison keys

        // Loop getting items from the two sequences alternately. Each new value is compared with
        // all the values already read from the other sequence, and is saved so that it can be compared
        // with values still to be read from the other sequence

        boolean exhausted1 = false;
        boolean exhausted2 = false;

        while (true) {

            // read a value from the first sequence
            if (!exhausted1) {
                UType t1 = ut1;
                AtomicValue v1 = first1;
                if (t1 == UType.UNTYPED_ATOMIC && hashTableType != UType.UNTYPED_ATOMIC) {
                    t1 = hashTableType;
                    ConversionResult result = untypedConverter.convertString(v1.getUnicodeStringValue());
                    if (result instanceof ValidationFailure) {
                        String msg = "Failure converting untypedAtomic value to " +
                                hashTableType +
                                " during comparison operation. " +
                                ((ValidationFailure) result).getMessage();
                        XPathException err = new XPathException(msg, "FORG0001", context);
                        err.setLocation(getLocation());
                        throw err;
                    } else {
                        v1 = (AtomicValue) result;
                    }
                }
                if (t1 == hashTableType) {
                    AtomicMatchKey k1 = v1.getXPathMatchKey(collator, implicitTimezone);
                    if (set2 != null && set2.contains(k1)) {
                        iter1.close();
                        iter2.close();
                        return true;
                    } else if (!exhausted2) {
                        // save it in the hashmap if there are more values to come from the second sequence
                        set1.add(k1);
                    }
                } else if (hashTableType != UType.UNTYPED_ATOMIC &&
                        !UType.isPossiblyComparable(t1, hashTableType, false)) {
                    String msg = "Cannot compare " + t1 + " to " + hashTableType;
                    XPathException err = new XPathException(msg, "XPTY0004", context);
                    err.setLocation(getLocation());
                    throw err;
                }
                if (ut1 != hashTableType) {
                    if (strays2 != null) {
                        for (AtomicValue stray2 : strays2) {
                            try {
                                if (compare(first1, Token.FEQ, stray2, boundComparer, false, context, getRetainedStaticContext())) {
                                    return true;
                                }
                            } catch (XPathException e) {
                                BuiltInAtomicType it1 = first1.getPrimitiveType();
                                BuiltInAtomicType it2 = stray2.getPrimitiveType();
                                XPathException err;
                                if (it1.equals(BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                    err = new XPathException("Cannot convert untypedAtomic value to type of second operand (" + it2 + ")");
                                    err.setErrorCode("FORG0001");
                                } else if (it2.equals(BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                    err = new XPathException("Cannot convert untypedAtomic value to type of first operand (" + it1 + ")");
                                    err.setErrorCode("FORG0001");
                                } else {
                                    err = new XPathException("Cannot compare " + it1 + " to " + it2);
                                    err.setErrorCode("XPTY0004");
                                }
                                err.setXPathContext(context);
                                err.setLocation(getLocation());
                                throw err;
                            }
                        }
                    }
                }
                if (ut1 != hashTableType || ut1 == UType.UNTYPED_ATOMIC) {
                    // if the original value is not the right type for the hash table,
                    // or if it is untypedAtomic, save it in a separate list,
                    // for comparison with values not yet read from the second sequence
                    if (!exhausted2) {
                        if (strays1 == null) {
                            strays1 = new ArrayList<AtomicValue>(10);
                        }
                        strays1.add(first1);
                    }
                }
                first1 = (AtomicValue) iter1.next();
                if (first1 == null) {
                    exhausted1 = true;
                } else {
                    ut1 = first1.getItemType().getUType();
                    if (UType.NUMERIC.subsumes(ut1)) {
                        ut1 = UType.NUMERIC;
                    }
                }
            }

            // read a value from the second sequence
            if (!exhausted2) {
                UType t2 = ut2;
                AtomicValue v2 = first2;
                if (t2 == UType.UNTYPED_ATOMIC && hashTableType != UType.UNTYPED_ATOMIC) {
                    t2 = hashTableType;
                    ConversionResult result = untypedConverter.convertString(v2.getUnicodeStringValue());
                    if (result instanceof ValidationFailure) {
                        String msg = "Failure converting untypedAtomic value to " +
                                hashTableType +
                                " during comparison operation. " +
                                ((ValidationFailure) result).getMessage();
                        XPathException de = new XPathException(msg, "FORG0001", context);
                        de.setLocation(getLocation());
                        throw de;
                    } else {
                        v2 = (AtomicValue) result;
                    }
                }
                if (t2 == hashTableType) {
                    AtomicMatchKey k2 = v2.getXPathMatchKey(collator, implicitTimezone);
                    if (set1.contains(k2)) {
                        iter1.close();
                        iter2.close();
                        return true;
                    } else if (!exhausted1) {
                        if (set2 == null) {
                            set2 = new HashSet<>(20);
                        }
                        set2.add(k2);
                    }
//                Following code deleted (test bool072): the items in the second sequence do not need to
//                  be mutually comparable
//                } else if (!hashTableType.equals(BuiltInAtomicType.UNTYPED_ATOMIC) &&
//                        !Type.isComparable(t2, hashTableType, false)) {
//                    t2 = first2.getPrimitiveType();  // for a more precise error message
//                    XPathException err = new XPathException("Cannot compare " + hashTableType + " to " + t2, "XPTY0004", context);
//                    err.setLocation(getLocation());
//                    throw err;
                }
                if (ut2 != hashTableType) {
                    if (strays1 != null) {
                        for (AtomicValue stray1 : strays1) {
                            try {
                                if (compare(first2, Token.FEQ, stray1, boundComparer, false, context, getRetainedStaticContext())) {
                                    return true;
                                }
                            } catch (XPathException e) {
                                BuiltInAtomicType it2 = first2.getPrimitiveType();
                                BuiltInAtomicType it1 = stray1.getPrimitiveType();
                                XPathException err;
                                if (it1.equals(BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                    err = new XPathException("Cannot convert untypedAtomic value to type of second operand (" + it2 + ")");
                                    err.setErrorCode("FORG0001");
                                } else if (it2.equals(BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                    err = new XPathException("Cannot convert untypedAtomic value to type of first operand (" + it1 + ")");
                                    err.setErrorCode("FORG0001");
                                } else {
                                    err = new XPathException("Cannot compare " + it1 + " to " + it2);
                                    err.setErrorCode("XPTY0004");
                                }
                                err.setXPathContext(context);
                                err.setLocation(getLocation());
                                throw err;
                            }
                        }
                    }
                }
                if (ut2 != hashTableType || ut2 == UType.UNTYPED_ATOMIC) {
                    if (!exhausted1) {
                        if (strays2 == null) {
                            strays2 = new ArrayList<AtomicValue>(10);
                        }
                        strays2.add(first2);
                    }
                }
                first2 = (AtomicValue) iter2.next();
                if (first2 == null) {
                    exhausted2 = true;
                } else {
                    ut2 = first2.getItemType().getUType();
                    if (UType.NUMERIC.subsumes(ut2)) {
                        ut2 = UType.NUMERIC;
                    }
                }
            }
            if (exhausted1 && exhausted2) {
                return false;
            }
        }
    }

    @Override
    protected GeneralComparison getInverseComparison() {
        GeneralComparisonEE gc = new GeneralComparisonEE(getRhsExpression(), Token.inverse(operator), getLhsExpression());
        gc.setRetainedStaticContext(getRetainedStaticContext());
        return gc;
    }

    @Override
    protected String tag() {
        return "gcEE";
    }

    @Override
    public Elaborator getElaborator() {
        return new GeneralComparisonElaborator();
    }

    /**
     * Elaborator for a general comparison expression such as (A = B).
     */

    public static class GeneralComparisonElaboratorEE extends BooleanElaborator {

        public BooleanEvaluator elaborateForBoolean() {
            final GeneralComparison exp = (GeneralComparison) getExpression();
            ComparisonCardinality cardinality = exp.getComparisonCardinality();

            switch (cardinality) {
                case ONE_TO_ONE: {
                    final ItemEvaluator p0 = exp.getLhsExpression().makeElaborator().elaborateForItem();
                    final ItemEvaluator p1 = exp.getRhsExpression().makeElaborator().elaborateForItem();
                    return context -> exp.evaluateOneToOne((AtomicValue) p0.eval(context), (AtomicValue) p1.eval(context), context);
                }
                case MANY_TO_ONE: {
                    final PullEvaluator p0 = exp.getLhsExpression().makeElaborator().elaborateForPull();
                    final ItemEvaluator p1 = exp.getRhsExpression().makeElaborator().elaborateForItem();
                    return context -> exp.evaluateManyToOne(p0.iterate(context), (AtomicValue) p1.eval(context), context);
                }
                case MANY_TO_MANY: {
                    final PullEvaluator p0 = exp.getLhsExpression().makeElaborator().elaborateForPull();
                    final PullEvaluator p1 = exp.getRhsExpression().makeElaborator().elaborateForPull();
                    return context -> exp.evaluateManyToMany(p0.iterate(context), p1.iterate(context), context);
                }
                default:
                    throw new UnsupportedOperationException();
            }

        }


    }
}

// Copyright (c) 2004-2022 Saxonica Limited
