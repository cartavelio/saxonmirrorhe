package net.sf.saxon.ma.pin;

import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.Genre;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;

/**
 * A <code>AnyLabeledItem</code> represents a labeled item whose subject is any kind of item
 * other than a map or array.
 */

// TODO: expressions such as "XX instanceof NodeInfo" will fail if given an AnyLabeledItem.
//   We either need to ban use of Java instanceof, or integrated labeled items into the Java
//   type hierarchy.

public class AnyLabeledItem implements LabeledItem {


    private final Item subject;
    private final MapItem label;

    /**
     * Create an AnyLabeledItem with a supplied label
     * @param subject the subject item
     * @param label the label
     */

    public AnyLabeledItem(Item subject, MapItem label) {
        this.subject = subject;
        this.label = label;
    }


    /**
     * Get the pedigree of this map item, providing information as to how it was selected.
     * @return the pedigree of this map item
     */

    @Override
    public MapItem getLabel() {
        return label;
    }

    /**
     * Get the subject item without its label
     *
     * @return the subject item
     */
    @Override
    public Item getSubject() {
        return subject;
    }

    /**
     * Get the genre of this item (to distinguish the top-level categories of item,
     * such as nodes, atomic values, and functions)
     *
     * @return the genre
     */
    @Override
    public Genre getGenre() {
        return subject.getGenre();
    }

    /**
     * Get the value of the item as a Unicode string. For nodes, this is the string value of the
     * node as defined in the XPath 2.0 data model, except that all nodes are treated as being
     * untyped: it is not an error to get the string value of a node with a complex type.
     * For atomic values, the method returns the result of casting the atomic value to a string.
     *
     * @return the string value of the item
     * @throws UnsupportedOperationException if the item is a function item (an unchecked exception
     *                                       is used here to avoid introducing exception handling to a large number of paths where it is not
     *                                       needed)
     * @since 8.4
     */
    @Override
    public UnicodeString getUnicodeStringValue() {
        return subject.getUnicodeStringValue();
    }

    /**
     * Atomize the item.
     *
     * @return the result of atomization
     * @throws XPathException if atomization is not allowed
     *                        for this kind of item
     */
    @Override
    public AtomicSequence atomize() throws XPathException {
        return subject.atomize();
    }

    public String toString() {
        return "¶" + subject.toString();
    }

}

// Copyright (c) 2024 Saxonica Limited

