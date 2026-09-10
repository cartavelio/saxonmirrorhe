package com.saxonica.ee.trans;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;

/**
 * Given an expression, this characteristic returns a boolean indicating whether
 * or not the context item has a parent (or ancestor, respectively) that matches
 * a given pattern
 */
public class AncestorCharacteristic extends Characteristic<Boolean> {

    private final Pattern pattern;
    private final boolean isParent;

    /**
     * Define an AncestorCharacteristic
     * @param pattern the pattern that an ancestor/parent must satisfy if the the characteristic
     *                is to have the value true
     * @param isParent true if the pattern must match the immediate parent; false if the pattern
     *                 must match some ancestor
     */

    public AncestorCharacteristic(Pattern pattern, boolean isParent) {
        this.pattern = pattern;
        this.isParent = isParent;
    }

    /**
     * Evaluate the characteristic as applied to the context item
     * @param context the dynamic evaluation context; the context item is the item being matched
     * @return true if there is an ancestor/parent satisfying the characteristic pattern
     * @throws XPathException if pattern matching fails with a dynamic error
     */

    @Override
    public Boolean evaluate(XPathContext context) throws XPathException {
        Item item = context.getContextItem();
        if (!(item instanceof NodeInfo)) {
            return false;
        }
        if (isParent) {
            return pattern.matches(((NodeInfo)item).getParent(), context);
        } else {
            NodeInfo a = (NodeInfo)item;
            while (a != null) {
                if (pattern.matches(a, context)) {
                    return true;
                }
                a = a.getParent();
            }
            return false;
        }
    }

    @Override
    public int hashCode() {
        return pattern.hashCode() ^ (isParent ? 8484 : 7373);
    }

    /**
     * Test whether this characteristic is the same as another characteristic.
     * @param obj the object to be compared
     * @return true if the two characteristics are equivalent
     */

    @Override
    public boolean equals(Object obj) {
        return obj instanceof AncestorCharacteristic &&
                pattern.isEqual(((AncestorCharacteristic)obj).pattern) &&
                isParent == ((AncestorCharacteristic)obj).isParent;
    }

    @Override
    public String toString() {
        return pattern + (isParent ? "/" : "//");
    }

}

// Copyright (c) 2018-2020 Saxonica Limited
