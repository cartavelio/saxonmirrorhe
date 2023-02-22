package net.sf.saxon.value;

import net.sf.saxon.expr.sort.XPathComparable;

/**
 * This interface is implemented by AtomicValues that can be compared without regard
 * to context - specifically, the result of eq and lt comparisons does not depend
 * on collations or on the context-dependent timezone.
 */

public interface ContextFreeAtomicValue {

    /**
     * Get an XPathComparable object that supports the semantics of context-free
     * eq and lt comparisons between atomic values. Note that in many cases the
     * returned XPathComparable will be the AtomicValue itself; however because
     * of the constaints of the generic {@link Comparable} interface, this
     * cannot be assumed.
     * @return an XPathComparable that can be used in comparisons with other atomic
     * values.
     */

    XPathComparable getXPathComparable();
}

