package com.saxonica.ee.pin.pin;

import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.StringMap;
import net.sf.saxon.type.AtomicMetadata;
import net.sf.saxon.type.AtomicType;

/**
 * This class combines the type and label of an atomic value into a single object;
 * this idea is to ensure that in the 99% case where an atomic value has no label,
 * there is no space overhead.
 */

public class TypeAndLabel implements AtomicMetadata {

    private final AtomicType type;
    private final StringMap label;

    public TypeAndLabel(AtomicType type, StringMap label) {
        this.type = type;
        this.label = label;
    }

    @Override
    public AtomicType getType() {
        return type;
    }

    @Override
    public StringMap getLabel() {
        return label;
    }
}

