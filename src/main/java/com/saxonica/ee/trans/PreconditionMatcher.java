package com.saxonica.ee.trans;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.rules.RuleChain;
import net.sf.saxon.trans.rules.RuleSearchState;
import net.sf.saxon.trans.XPathException;

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
    private XPathContext context;
    private Boolean[] preconditionState;  // This must be a Boolean[] object as null is a valid value.
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
        if (chain.optimizationData != null) {
            ModeEE.RuleChainOptimizationData data = (ModeEE.RuleChainOptimizationData)chain.optimizationData;
            preconditionState = new Boolean[data.preconditions.size()];
            characteristics = new Object[data.characteristics.size()];
        }
    }

    /**
     * Get the state of a given precondition.
     * @param slot the slot number allocated to the precondition
     * @return true if the precondition is known to be true for the item being matched;
     * false if it is known to be false; or null if its value is not known
     */

    public Boolean getPreconditionState(int slot) {
        return preconditionState[slot];
    }

    /**
     * Set the value of a given precondition.
     * @param slot the slot number allocated to the precondition
     * @param value true if the precondition (for the item being matched) is known to be true,
     *              false if it is known to be false.
     */

    public void setPreconditionState(int slot, boolean value) {
        preconditionState[slot] = value;
    }

    /**
     * Get the value of a particular characteristic of the item being matched, evaluating
     * it if it has not already been evaluated, and retaining its value for future use
     * @param chic the characteristic to be evaluated
     * @param <T> the type of value of this characteristic
     * @return the value of the characteristic
     * @throws XPathException if evaluation of the characteristic failed
     */

    public <T> T obtainCharacteristic(Characteristic<T> chic) throws XPathException {
        T val = (T)characteristics[chic.getSlotNumber()];
        if (val == null) {
            val = chic.evaluate(context);
            characteristics[chic.getSlotNumber()] = val;
        }
        return val;
    }



}

// Copyright (c) 2018 Saxonica Limited.
