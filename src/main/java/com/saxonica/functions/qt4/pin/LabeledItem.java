package com.saxonica.functions.qt4.pin.pin;

import net.sf.saxon.ma.map.StringMap;
import net.sf.saxon.om.Item;

/**
 * A <code>LabeledItem</code> represents an item together with a label. Most operations on a labeledItem
 * simply delegate to the subject item.
 * <p>Because of the widespread use of tests such as {@code XX instanceof NodeInfo}, concrete implementations
 * of LabeledItem are subclasses of NodeInfo, AtomicValue, etc</p>
 */

public interface LabeledItem extends Item {

    /**
     * Get the label of this value, typically, information about
     * the containing maps and arrays in the containing tree
     *
     * @return the associated {@code Pedigree}.
     */

    StringMap getLabel();

    /**
     * Get the subject item without its label
     * @return the subject item
     */

    Item getSubject();



}

// Copyright (c) 2024 Saxonica Limited

