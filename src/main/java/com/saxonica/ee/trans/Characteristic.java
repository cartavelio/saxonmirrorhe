package com.saxonica.ee.trans;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.rules.RuleSearchState;

/**
 * A Characteristic represents a distinguishing feature of an item that may be used by several
 * patterns in a rule chain, and which can usefully be evaluated once for all rules in the rule chain.
 * Characteristics are evaluated in the course of evaluating preconditions, and their values may be
 * used again when evaluating a different precondition. For example (a classic DITA use case), the tokenized
 * value of an element's <code>@class</code> attribute is a characteristic that may be used by several
 * preconditions that test for the presence of particular tokens in the value.
 */
public abstract class Characteristic {

    int slotNumber;

    public Characteristic() {
    }

    /**
     * Get the slot number allocated to the value of this characteristic in the {@link RuleSearchState}
     * @return the allocated slot number
     */

    public final int getSlotNumber() {
        return slotNumber;
    }

    /**
     * Allocate a slot number to this characteristic in the {@link RuleSearchState}
     * @param slotNumber the slot number to be allocated
     */

    public final void setSlotNumber(int slotNumber) {
        this.slotNumber = slotNumber;
    }

    /**
     * Evaluate the value of this characteristic for the context item
     * @param context the dynamic evaluation context; the context item is the item being matched
     * @return the value of the characteristic
     * @throws XPathException if evaluation of the characteristic fails
     */

    public abstract Object evaluate(XPathContext context) throws XPathException;

}

// Copyright (c) 2018-2022 Saxonica Limited