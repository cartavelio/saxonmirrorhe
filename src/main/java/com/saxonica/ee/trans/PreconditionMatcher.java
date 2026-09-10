package com.saxonica.ee.trans;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.trans.rules.RuleChain;
import net.sf.saxon.trans.rules.RuleSearchState;

/**
 * This class contains the stateful part of the precondition checker. A new PreconditionMatcher is allocated
 * when searching a rule chain to find the best matching rule for a given node (or other item). It holds the
 * values of preconditions and characteristics. A precondition is a boolean property of an item that must
 * be true if the item is to match a particular pattern; once the precondition has been evaluated, its value
 * can be used to eliminate other rules with the same precondition. A characteristic is a property of an item
 * that must match a corresponding property of a rule if the rule is to match; for example, a property might
 * be "name of parent element". Once the value of a characteristic (for example, the name of the parent element)
 * has been evaluated, a rule can be eliminated if it requires a different value for the characteristic.
 */
public class PreconditionMatcher extends RuleSearchState {
    private final XPathContext context;
    private int[] preconditionState;  // Values are true, false, or unknown, with 0 meaning unknown
    private Object[] characteristics;

    /**
     * Allocate a PreconditionMatcher for a given set of rules
     * @param chain the set of rules being matched
     * @param context the XPath evaluation context; the context item is the item being matched against
     *                the set of rules
     */

    public PreconditionMatcher(RuleChain chain, XPathContext context) {
        super();
        this.context = context;
        if (chain.hasOptimizationData()) {
            ModeEE.RuleChainOptimizationData data = (ModeEE.RuleChainOptimizationData)chain.optimizationData;
            preconditionState = new int[data.preconditions.size()];
            characteristics = new Object[data.characteristics.size()];
        }
    }

    /**
     * Get the state of a given precondition.
     * @param slot the slot number allocated to the precondition
     * @return {@link Precondition#PRECONDITION_TRUE} if the precondition is known to be true
     * for the item being matched; {@link Precondition#PRECONDITION_FALSE} if it is known to be false;
     * or {@link Precondition#PRECONDITION_UNKNOWN} if its value is not known
     */

    public int getPreconditionState(int slot) {
        return preconditionState[slot];
    }

    /**
     * Set the value of a given precondition.
     * @param slot the slot number allocated to the precondition
     * @param value true if the precondition (for the item being matched) is known to be true,
     *              false if it is known to be false.
     */

    public void setPreconditionState(int slot, boolean value) {
        preconditionState[slot] = value ? Precondition.PRECONDITION_TRUE : Precondition.PRECONDITION_FALSE;
    }

    /**
     * Use this search state to check whether the preconditions for a particular rule
     * are satisfied
     *
     * @param rule the rule being tested
     * @return true if all preconditions are satisfied; in the HE implementation, there
     * are no preconditions, so this is always true.
     * @throws XPathException if evaluation of preconditions fails.
     */

    public boolean checkPreconditions(Rule rule) throws XPathException {
        return ((RuleEE)rule).checkPreconditions(this);
    }

    /**
     * Get the value of a particular characteristic of the item being matched, evaluating
     * it if it has not already been evaluated, and retaining its value for future use
     * @param chic the characteristic to be evaluated
     * @return the value of the characteristic
     * @throws XPathException if evaluation of the characteristic failed
     */

    public Object obtainCharacteristic(Characteristic chic) throws XPathException {
        Object val = characteristics[chic.getSlotNumber()];
        if (val == null) {
            val = chic.evaluate(context);
            characteristics[chic.getSlotNumber()] = val;
        }
        return val;
    }



}

// Copyright (c) 2018-2022 Saxonica Limited
