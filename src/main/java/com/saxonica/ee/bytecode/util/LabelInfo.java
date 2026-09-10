package com.saxonica.ee.bytecode.util;

/**
 * An ASM label object with some control information to enable better diagnostics
 */
public class LabelInfo {
    String name;
    NamedLabel theLabel;
    boolean isUsed = false;
    boolean isMarked = false;

    /**
     * Get the underlying ASM label object
     *
     * @return the ASM label
     */
    public NamedLabel label() {
        isUsed = true;
        return theLabel;
    }

    /**
     * Ask whether this label has been used (that is, whether any instructions have been generated that refer to it)
     *
     * @return true if the label has been used
     */

    public boolean isUsed() {
        return isUsed;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited