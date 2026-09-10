package net.sf.saxon.type;

import net.sf.saxon.ma.map.MapItem;

public interface AtomicMetadata {

    AtomicType getType();

    MapItem getLabel();

}

