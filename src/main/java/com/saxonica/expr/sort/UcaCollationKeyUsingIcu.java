package com.saxonica.expr.sort;

import net.sf.saxon.transpile.CSharpModifiers;

import java.text.CollationKey;

/**
 * This class is an implementation of Saxon collation keys based on
 * the functionality of collation keys supplied by the ICU library.
 * Collation keys are used not only to support the <code>fn:collation-key</code>
 * function in XPath, but also internally to support <code>fn:distinct-values</code>,
 * <code>xsl:for-each-group</code>, and XQuery grouping.
 */


public class UcaCollationKeyUsingIcu extends CollationKey {
    private final com.ibm.icu.text.CollationKey ucaKey;

    /**
     * Package-private constructor. Collation keys are constructed using the factory
     * method {@link UcaCollatorUsingIcu#getCollationKey(String)}
     * @param source the string whose collation key this is
     * @param key the internal collation key as allocated by UCA
     */

    UcaCollationKeyUsingIcu(String source, byte[] key) {
        super(source);
        this.ucaKey = new com.ibm.icu.text.CollationKey(source, key);
    }

    /**
     * Get the internal ICU collation key
     * @return the internal ICU collation key
     */

    public com.ibm.icu.text.CollationKey getKey() {
        return ucaKey;
    }

    /**
     * Compare this collation key to another.
     * @param target the other collation key
     * @return true if the collation keys are equal, implying that the corresponding strings are to
     * be considered equal
     * @throws ClassCastException if the other collation key is not an ICU-based collation key.
     */

    @Override
    @CSharpModifiers(code={"public", "override"})
    public int compareTo(CollationKey target) {
        return ucaKey.compareTo(((UcaCollationKeyUsingIcu)target).getKey());
    }

    /**
     * Get a binary representation of this collection key
     * @return a binary representation of the key, such that binary comparison can be used to compare keys.
     */

    @Override
    @CSharpModifiers(code = {"public", "override"})
    public byte[] toByteArray() {
        return ucaKey.toByteArray();
    }

    /**
     * Get a hash code supporting the equals() method
     * @return a hash code for the collation key
     */

    @Override
    public int hashCode() {
        return ucaKey.hashCode();
    }

    /**
     * Test if two collation keys are equal
     * @param obj the other collation key
     * @return true if the argument is an ICU-based collation key representing a string that is considered
     * equal to the string represented by this collation key.
     */

    @Override
    public boolean equals(Object obj) {
        return obj instanceof UcaCollationKeyUsingIcu && ucaKey.equals(((UcaCollationKeyUsingIcu)obj).ucaKey);
    }
}

// Copyright (c) 2018-2023 Saxonica Limited
