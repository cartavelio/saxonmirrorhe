package net.sf.saxon.type;

import net.sf.saxon.ma.map.MapItem;

/**
 * This class is an attempt to generalise the concept of a type annotation to allow an optional label
 * as well. The idea of labelled values was an experiment, first in Saxon and then in XDM 4.0; it is
 * largely superseded by the introduction of JNodes, but is not yet entirely obsolete.
 */

public interface AtomicMetadata {

    AtomicType getType();

    MapItem getLabel();

}

