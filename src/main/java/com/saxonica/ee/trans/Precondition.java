package com.saxonica.ee.trans;

import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.z.IntSet;

/**
 * A Precondition is a boolean condition that must be true if a pattern is to match (but in general
 * is not a sufficient condition for the pattern to match).
 *
 * Preconditions may be defined in terms of "characteristics": that is, properties of the item
 * being matched.
 *
 * When an item is matched against a RuleChain to find the best matching rule, subexpressions used
 * by more than one rule may be evaluated as characteristics, so they are not evaluated repeatedly.
 *
 * Preconditions are shared by multiple rules. Evaluating a precondition for one rule may ensure
 * that the patterns for other rules do not need to be evaluated.
 */
public abstract class Precondition {


    public static final int PRECONDITION_UNKNOWN = 0;
    public static final int PRECONDITION_FALSE = -1;
    public static final int PRECONDITION_TRUE = 1;

    int slotNumber;

    /**
     * Allocate a slot number indicating where in the data maintained by the {@link PreconditionMatcher}
     * the value of this precondition should be held
     * @param slotNumber the slot number to be used for the value of this precondition
     */

    public void setSlotNumber(int slotNumber) {
        this.slotNumber = slotNumber;
    }

    /**
     * Ask whether the precondition is satisfied, making use of caching
     * @param matcher The <code>PreconditionMatcher</code> holds the values of conditions
     *                that have already been matched, which is therefore used as a cache
     *                of values that do not need to be evaluated repeatedly. In addition
     *                it holds the dynamic evaluation context; the context item is the
     *                item being matched.
     * @return true if the precondition is satisfied
     * @throws XPathException if evaluation of the precondition fails with a dynamic error
     */

    public final boolean isSatisfied(PreconditionMatcher matcher) throws XPathException {
        int state = matcher.getPreconditionState(slotNumber);
        if (state == PRECONDITION_UNKNOWN) {
            boolean b = testPrecondition(matcher);
            matcher.setPreconditionState(slotNumber, b);
            return b;
        }
        return state == PRECONDITION_TRUE;
    }

    /**
     * Compute the value of the precondition for a given item. The item in question is the
     * context item in the dynamic context held in the PreconditionMatcher
     * @param matcher the PreconditionMatcher: holds the dynamic context for evaluation.
     * @return true if the precondition is satisfied
     * @throws XPathException if evaluation of the precondition fails with a dynamic error
     */

    protected abstract boolean testPrecondition(PreconditionMatcher matcher) throws XPathException;

    /**
     * A StringHashPrecondition compares a characteristic in the form of a set of integer hash codes
     * against a constant hash code; the precondition is satisfied if the constant hash code is present
     * in the set.
     * <p>It is used in two situations:</p>
     * <p>(a) for a predicate pattern in the form *[EXP = 'constant'] where the constant hash code is the
     * hash code of the collation key of the constant string, and the characteristic is the set of hash
     * codes corresponding to the collation keys of the strings selected by evaluating EXP.</p>
     * <p>(b) for a predicate pattern in the form *[contains(EXP, ' constant ')], typically encountered
     * in DITA stylesheets, where the constant hash code is the hash code of the collation key of the token
     * being matched (the constant with whitespace trimmed), and the characteristic is the set of hash codes
     * corresponding to the collation keys of the tokens found by evaluating EXP and splitting on whitespace
     * boundaries.</p>
     */

    public static class StringHashPrecondition extends Precondition {

        private final Characteristic characteristic;
        private final int hash;
        private final StringCollator collator;

        /**
         * Create a <code>StringHashPrecondition</code>
         * @param characteristic the characteristic that is tested by this precondition. This
         *                       must evaluate to a set of hashcodes for permitted string values.
         *                       The precondition is satisfied if the set of hash codes obtained
         *                       as the value of the characteristic includes the hash code of the
         *                       required key value. For example, given <code>match=*[author='Kay']</code>,
         *                       the characteristic will be the set of hash codes of the values of
         *                       'author' children of the context item; the precondition is satisfied
         *                       if this includes the hash code of the string 'Kay' under the specified
         *                       collation.
         * @param required  the required key value, whose hash code must match one of the values in the
         *                  IntSet value of the characteristic
         * @param collator  the collation to be used for comparison
         */

        public StringHashPrecondition(Characteristic characteristic, AtomicMatchKey required, StringCollator collator) {
            this.characteristic = characteristic;
            this.collator = collator;
            this.hash = required.hashCode();
        }

        @Override
        protected boolean testPrecondition(PreconditionMatcher matcher) throws XPathException {
            IntSet comparand = (IntSet)matcher.obtainCharacteristic(characteristic);
            return comparand.contains(hash);
        }

        @Override
        public int hashCode() {
            return characteristic.hashCode() ^ hash;
        }

        @Override
        public boolean equals(Object obj) {
            if (obj instanceof StringHashPrecondition) {
                StringHashPrecondition other = (StringHashPrecondition) obj;
                return collator == other.collator &&
                        hash == other.hash &&
                        characteristic.equals(other.characteristic);
            } else {
                return false;
            }
        }

        @Override
        public String toString() {
            return "hash(" + characteristic + ")=" + Integer.toHexString(hash);
        }

    }

    /**
     * A numeric comparison precondition is used for patterns containing a predicate that compares some
     * expression E with a numeric constant. The expression E is evaluated as a characteristic, so that
     * in a set of rules with predicates that compare E against different numeric constants, the expression
     * E is only evaluated once.
     */

    public static class NumericComparisonPrecondition extends Precondition {

        private final Characteristic characteristic;
        private final long required;

        public NumericComparisonPrecondition(Characteristic characteristic, long required) {
            this.characteristic = characteristic;
            this.required = required;
        }

        @Override
        protected boolean testPrecondition(PreconditionMatcher matcher) throws XPathException {
            NumericValue comparand = (NumericValue)matcher.obtainCharacteristic(characteristic);
            return !comparand.isNaN() && comparand.compareTo(required) == 0;
        }

        @Override
        public int hashCode() {
            return characteristic.hashCode() ^ (int)required;
        }

        @Override
        public boolean equals(Object obj) {
            if (obj instanceof NumericComparisonPrecondition) {
                NumericComparisonPrecondition other = (NumericComparisonPrecondition) obj;
                return required == other.required &&
                        characteristic.equals(other.characteristic);
            } else {
                return false;
            }
        }

        @Override
        public String toString() {
            return "hash(" + characteristic + ")=" + Long.toHexString(required);
        }

    }

    /**
     * A <code>BooleanPrecondition</code> can in principle test any boolean characteristic
     * of a pattern. In practice it is used to test for the existence of a named ancestor/parent
     * element. That is, in current use the {@link Characteristic} will always be an
     * {@link AncestorCharacteristic}
     */

    public static class BooleanPrecondition extends Precondition {

        private final Characteristic characteristic;

        /**
         * Create a precondition that maps directly to a boolean characteristic
         * @param characteristic the boolean characteristic
         */

        public BooleanPrecondition(Characteristic characteristic) {
            this.characteristic = characteristic;
        }

        /**
         * Evaluate the precondition, which will be true if and only if the value of
         * the characteristic for the current item is the boolean value true.
         * @param matcher the PreconditionMatcher: holds the dynamic context for evaluation.
         * @return true if the precondition is satisfied
         * @throws XPathException if evaluation of the precondition fails with a dynamic error
         */

        @Override
        protected boolean testPrecondition(PreconditionMatcher matcher) throws XPathException {
            return ((BooleanValue)matcher.obtainCharacteristic(characteristic)).getBooleanValue();
        }

        @Override
        public int hashCode() {
            return characteristic.hashCode() ^ 7720;
        }

        @Override
        public boolean equals(Object obj) {
            if (obj instanceof BooleanPrecondition) {
                BooleanPrecondition other = (BooleanPrecondition) obj;
                return characteristic.equals(other.characteristic);
            } else {
                return false;
            }
        }

        @Override
        public String toString() {
            return characteristic.toString();
        }

    }


}

// Copyright (c) 2018-2022 Saxonica Limited