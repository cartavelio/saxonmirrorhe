package com.saxonica.functions.qt4.pin;

import net.sf.saxon.ma.map.StringMapItem;
import net.sf.saxon.type.AtomicMetadata;
import net.sf.saxon.type.AtomicType;

/**
 * This class combines the type and label of an atomic value into a single object;
 * this idea is to ensure that in the 99% case where an atomic value has no label,
 * there is no space overhead.
 */

public class TypeAndLabel implements AtomicMetadata {

    private final AtomicType type;
    private final StringMapItem label;

    public TypeAndLabel(AtomicType type, StringMapItem label) {
        this.type = type;
        this.label = label;
    }

    @Override
    public AtomicType getType() {
        return type;
    }

    @Override
    public StringMapItem getLabel() {
        return label;
    }
}

