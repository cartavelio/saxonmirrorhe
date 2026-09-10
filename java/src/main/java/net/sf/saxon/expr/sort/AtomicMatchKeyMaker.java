package net.sf.saxon.expr.sort;

import net.sf.saxon.value.AtomicValue;

@FunctionalInterface
public interface AtomicMatchKeyMaker {

    /**
     * Given an atomic value, make a match key (for use in maps, keys, etc.) Two atomic
     * values are considered equal (for some given purpose) if they have the same match
     * key (as compared using Java equals()). Match keys are used only for equality testing,
     * not for order comparisons.
     * @param value the atomic value
     * @return a match key corresponding to the atomic value.
     */
    AtomicMatchKey makeMatchKey(AtomicValue value);
}

