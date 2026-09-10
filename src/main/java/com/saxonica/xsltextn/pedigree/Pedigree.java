package com.saxonica.xsltextn.pedigree;

import com.saxonica.ee.pin.pin.LabeledItem;
import net.sf.saxon.value.AtomicValue;

/**
 * A <code>Pedigree</code> is transient information relating to a map or array indicating
 * the route by which it was reached; it thus compensates for the absence of parent
 * pointers in a JSON-like tree, by allowing the steps used to reach a map or array
 * in such a structure to be retraced.
 * <p>A map or array that has a {@code Pedigree} is represented by a {@link LabeledItem},
 * which is a combination of the map/array and the {@code Pedigree}. The {@code Pedigree}
 * holds a reference to the "container" {@code PedigreeValue} from which this map or
 * array was reached, and the key or index value within that container.</p>
 * <p>Note that maps and arrays have no externally visible identity. It is not meaningful
 * to ask whether the same map can be reached by two different routes, because there
 * is no way to establish "sameness".</p>
 * <p>A map or array will have a {@code Pedigree} if it is found by downwards selection
 * from a map or array that itself has a {@code Pedigree}, or if it was explicitly
 * created with a {@code Pedigree}.</p>
 */

public class Pedigree {

    private final LabeledItem container;
    private int index = -1;
    private AtomicValue key;

    /**
     * An empty {@code Pedigree}: that is, the {@code Pedigree} of a map or array that
     * was created as the root of a pedigree tree.
     */

    public final static Pedigree PEDIGREE_ROOT = new Pedigree(null, null);

    /**
     * Create a pedigree for a map or array obtained by selection
     * from a map, using a key value
     * @param container the containing map
     * @param key the key value used to select an entry in the containing map
     */

    public Pedigree(LabeledItem container, AtomicValue key) {
        this.container = container;
        this.key = key;
    }

    /**
     * Create a pedigree for a map or array obtained by selection
     * from an array, using an index value as a subscript into the array
     *
     * @param container the containing map
     * @param index  the zero-based index value used to select a member of the containing array
     */

    public Pedigree(LabeledItem container, int index) {
        this.container = container;
        this.index = index;
    }

    /**
     * Get the containing map or array
     * @return the containing map or array
     */

    public LabeledItem getContainer() {
        return container;
    }

    /**
     * Get the index if this item was obtained by selection from an array
     * @return the zero-based index if this item was obtained by selection from an array,
     * otherwise -1.
     */

    public int getIndex() {
        return index;
    }

    /**
     * Get the key if this item was obtained by selection from a map
     *
     * @return the key if this item was obtained by selection from a map,
     * otherwise null.
     */

    public AtomicValue getKey() {
        return key;
    }

    /**
     * Make a map or array that combines a supplied map or array with this pedigree
     * @param value the supplied value
     * @return if the supplied value is a map or array, returns a map or array
     * augmented with this pedigree. Otherwise, returns the supplied value unchanged.
     */

//    public GroundedValue makePedigreeValue(GroundedValue value) {
//        if (value instanceof LabeledArrayItem) {
//            return new LabeledArrayItem(((LabeledArrayItem)value).getBaseItem(), this);
//        } else if (value instanceof LabeledMapItem) {
//            return new LabeledMapItem(((LabeledMapItem) value).getBaseItem(), this);
//        } else if (value instanceof ArrayItem) {
//            return new LabeledArrayItem((ArrayItem) value, this);
//        } else if (value instanceof MapItem) {
//            return new LabeledMapItem((MapItem) value, this);
//        } else {
//            return value;
//        }
//    }

}

// Copyright (c) 2018-2023 Saxonica Limited


