package net.sf.saxon.type;

import net.sf.saxon.ma.map.StringMap;

public interface AtomicMetadata {

    AtomicType getType();

    StringMap getLabel();

}

