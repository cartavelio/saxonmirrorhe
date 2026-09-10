////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.ItemElaborator;
import net.sf.saxon.expr.elab.ItemEvaluator;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.functions.hof.FunctionLiteral;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.math.BigInteger;


/**
 * Implementation of the fn:sum function
 */
public class Sum extends FoldingFunction {

    @Override
    public Expression makeFunctionCall(Expression... arguments) {
        Expression[] newArgs = new Expression[2];
        newArgs[0] = arguments[0];
        if (arguments.length < 2 || arguments[1] instanceof DefaultedArgumentExpression) {
            newArgs[1] = FunctionLiteral.makeLiteral(Int64Value.ZERO);
            setArity(2);
        } else {
            newArgs[1] = arguments[1];
        }
        return super.makeFunctionCall(newArgs);
    }

    /*@NotNull*/
    @Override
    public ItemType getResultItemType(Expression[] args) {
        TypeHierarchy th = getRetainedStaticContext().getConfiguration().getTypeHierarchy();
        ItemType base = Atomizer.getAtomizedItemType(args[0], false);
        if (base.equals(BuiltInAtomicType.UNTYPED_ATOMIC)) {
            base = BuiltInAtomicType.DOUBLE;
        }
        if (Cardinality.allowsZero(args[0].getCardinality())) {
            if (getArity() == 1) {
                return Type.getCommonSuperType(base, BuiltInAtomicType.INTEGER, th);
            } else {
                return Type.getCommonSuperType(base, args[1].getItemType(), th);
            }
        } else {
            return base.getPrimitiveItemType();
        }
    }

    @Override
    public int getCardinality(Expression[] arguments) {
        if (getArity() == 1 || arguments[1].getCardinality() == 1) {
            return StaticProperty.EXACTLY_ONE;
        } else {
            return StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
    }

    /**
     * Create the Fold object which actually performs the evaluation.
     *
     * @param context             the dynamic evaluation context
     * @param additionalArguments the values of all arguments other than the first.
     * @return the Fold object used to compute the function
     */
    @Override
    public Fold getFold(XPathContext context, Sequence... additionalArguments) throws XPathException {
        if (additionalArguments.length > 0) {
            AtomicValue z = (AtomicValue)additionalArguments[0].head();
            return new SumFold(context, z);
        } else {
            return new SumFold(context, Int64Value.ZERO);
        }
    }

    /**
     * Implementation of Fold class to do the summation in push mode
     */

    public static class SumFold implements Fold {
        private final XPathContext context;
        private final AtomicValue zeroValue; // null means empty sequence
        private AtomicValue data;
        private boolean atStart = true;
        private final ConversionRules rules;
        private final StringConverter toDouble;

        public SumFold(XPathContext context, AtomicValue zeroValue) {
            this.context = context;
            this.zeroValue = zeroValue;
            this.rules = context.getConfiguration().getConversionRules();
            this.toDouble = BuiltInAtomicType.DOUBLE.getStringConverter(rules);
        }

        /**
         * Process one item in the input sequence, returning a new copy of the working data
         *
         * @param item the item to be processed from the input sequence
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        @Override
        public void processItem(Item item) throws XPathException {
            AtomicValue next = (AtomicValue)item;
            if (atStart) {
                atStart = false;
                if (next.isUntypedAtomic()) {
                    data = toDouble.convert(next).asAtomic();
                    return;
                } else if (next instanceof NumericValue || next instanceof DayTimeDurationValue || next instanceof YearMonthDurationValue) {
                    data = next;
                    return;
                } else {
                    throw new XPathException(
                        "Input to sum() contains a value of type " +
                                next.getPrimitiveType().getDisplayName() +
                                " which is neither numeric, nor a duration")
                            .withXPathContext(context).withErrorCode("FORG0006");
                }
            }

            if (data instanceof NumericValue) {
                if (next.isUntypedAtomic()) {
                    next = toDouble.convert(next).asAtomic();
                } else if (!(next instanceof NumericValue)) {
                    throw new XPathException("Input to sum() contains a mix of numeric and non-numeric values")
                            .withXPathContext(context).withErrorCode("FORG0006");
                }
                data = ArithmeticExpression.compute(data, Calculator.PLUS, next, context);
            } else if (data instanceof DurationValue) {
                if (!((data instanceof DayTimeDurationValue) || (data instanceof YearMonthDurationValue))) {
                    throw new XPathException("Input to sum() contains a duration that is neither a dayTimeDuration nor a yearMonthDuration")
                        .withXPathContext(context).withErrorCode("FORG0006");
                }
                if (!(next instanceof DurationValue)) {
                    throw new XPathException("Input to sum() contains a mix of duration and non-duration values")
                            .withXPathContext(context).withErrorCode("FORG0006");
                }
                data = ((DurationValue) data).add((DurationValue) next);
            } else {
                throw new XPathException(
                        "Input to sum() contains a value of type " +
                                data.getPrimitiveType().getDisplayName() +
                                " which is neither numeric, nor a duration")
                        .withXPathContext(context).withErrorCode("FORG0006");
            }
        }

        /**
         * Ask whether the computation has completed. A function that can deliver its final
         * result without reading the whole input should return true; this will be followed
         * by a call on result() to deliver the final result.
         * @return true if the result of the function is now available even though not all
         * items in the sequence have been processed
         */
        @Override
        public boolean isFinished() {
            return data instanceof DoubleValue && data.isNaN();
        }

        /**
         * Compute the final result of the function, when all the input has been processed
         *
         * @return the result of the function
         */
        @Override
        public Sequence result() {
            if (atStart) {
                return SequenceTool.itemOrEmpty(zeroValue);
            } else {
                return data;
            }
        }
    }

    /**
     * Make an elaborator for a system function call on this function
     *
     * @return a suitable elaborator; or null if no custom elaborator is available
     */
    @Override
    public Elaborator getElaborator() {
        return new SumFnElaborator();
    }

    public static class SumFnElaborator extends ItemElaborator {

        public ItemEvaluator elaborateForItem() {
            SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
            PullEvaluator puller = fnc.getArg(0).makeElaborator().elaborateForPull();
            boolean defaultSecondArg = fnc.getArity() < 2 || fnc.getArg(1) instanceof DefaultedArgumentExpression;
            ItemEvaluator zero = defaultSecondArg
                    ? context -> Int64Value.ZERO
                    : fnc.getArg(1).makeElaborator().elaborateForItem();
            ItemType argType = fnc.getArg(0).getItemType();
            if (argType == BuiltInAtomicType.DOUBLE) {
                return context -> sumDoubles(context, puller, zero);
            } else if (argType == BuiltInAtomicType.INTEGER) {
                return context -> sumIntegers(context, puller, zero);
            }
            return context -> {
                SumFold fold = new SumFold(context, (AtomicValue)zero.eval(context));
                SequenceIterator iter = puller.iterate(context);
                for (Item it; (it = iter.next()) != null; ) {
                    fold.processItem(it);
                }
                return fold.result().head();
            };
        }

        /**
         * Fast path for use when the argument of sum() has static type xs:double*
         * @param context the evaluation context
         * @param puller used to iterate over the argument values
         * @param zero used to compute the result for an empty sequence
         * @return the total
         * @throws XPathException if a failure occurs
         */

        private Item sumDoubles(XPathContext context, PullEvaluator puller, ItemEvaluator zero) throws XPathException {
            SequenceIterator iter = puller.iterate(context);
            Item it = iter.next();
            if (it == null) {
                return zero.eval(context);
            }
            double total = ((DoubleValue) it).getDoubleValue();
            while ((it = iter.next()) != null) {
                total += ((DoubleValue)it).getDoubleValue();
            }
            return new DoubleValue(total);
        }

        /**
         * Fast path for use when the argument of sum() has static type xs:integer*
         *
         * @param context the evaluation context
         * @param puller  used to iterate over the argument values
         * @param zero    used to compute the result for an empty sequence
         * @return the total
         * @throws XPathException if a failure occurs
         */

        private Item sumIntegers(XPathContext context, PullEvaluator puller, ItemEvaluator zero) throws XPathException {
            SequenceIterator iter = puller.iterate(context);
            Item it = iter.next();
            if (it == null) {
                return zero.eval(context);
            }
            BigInteger total = ((IntegerValue) it).asBigInteger();
            while ((it = iter.next()) != null) {
                total = total.add(((IntegerValue) it).asBigInteger());
            }
            return new BigIntegerValue(total);
        }


    }
}

