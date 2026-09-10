package net.sf.saxon.expr.sort;

import net.sf.saxon.value.NumericValue;

public class TransitiveNumericComparable implements XPathComparable {

    private final NumericValue value;

    public TransitiveNumericComparable(NumericValue value) {
        this.value = value;
    }

    /**
     * Compares this object with the specified object for order.  Returns a
     * negative integer, zero, or a positive integer as this object is less
     * than, equal to, or greater than the specified object.
     */

    @Override
    public int compareTo(XPathComparable o) {
        if (o instanceof TransitiveNumericComparable) {
            return value.transitiveCompareTo(((TransitiveNumericComparable)o).value);
        }
        throw new ClassCastException("Cannot compare numeric and non-numeric values");
    }
}

