package com.saxonica.xsltextn.pedigree;

/**
 * A <code>PedigreeValue</code> is a map or array together with a {@link Pedigree}
 * indicating how that map or array was selected within a JSON-like tree structure.
 * <p>The {@code PedigreeValue} delegates all map and array operations to the wrapped
 * map or array (called the base item), and in addition offers the ability to
 * retrace the selection steps to establish the containing maps or arrays in the
 * tree.</p>
 */

public interface PedigreeValue {

    /**
     * Get the {@code Pedigree} of this value, that is, information about
     * the containing maps and arrays in the containing tree
     * @return the associated {@code Pedigree}.
     */

    Pedigree getPedigree();

//    /**
//     * Get the base item, that is, the map or array that is wrapped by this {@code PedigreeValue}
//     * @return the base item.
//     */
//
//    Item getBaseItem();

    /**
     * Get the root of the pedigree tree: that is, retrace the steps used to select this
     * map or array until a map or array with an empty {@code Pedigree} is found.
     * @return the root of the pedigree tree.
     */

    default PedigreeValue getOrigin() {
        Pedigree ped = getPedigree();
        if (ped == Pedigree.PEDIGREE_ROOT) {
            return this;
        } else {
            PedigreeValue parent = ped.getContainer();
            return parent.getOrigin();
        }
    }


}

// Copyright (c) 2018-2022 Saxonica Limited

