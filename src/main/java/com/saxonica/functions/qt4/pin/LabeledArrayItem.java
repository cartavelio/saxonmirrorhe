package com.saxonica.functions.qt4.pin.pin;

import com.saxonica.xsltextn.pedigree.DelegatingArrayItem;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.StringMap;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.z.IntSet;

import java.util.ArrayList;
import java.util.List;

/**
 * A <code>LabeledArrayItem</code> represents an XDM array together with a label, which is a map:
 * generally containing retained information about how the array was selected within a JSON-like tree.
 */
public class LabeledArrayItem extends DelegatingArrayItem implements LabeledItem {

    StringMap label;

    /**
     * Create a PedigreeArrayItem with an empty pedigree
     * @param base the underlying array item
     */

    public LabeledArrayItem(ArrayItem base, StringMap label) {
        super(base);
        this.label = label;
    }

    public ArrayItem getSubject() {
        return getBaseItem();
    }

    public static LabeledArrayItem makePinnedArrayItem(ArrayItem base) {
        return new LabeledArrayItem(base, LabelTool.PIN_LABEL);
    }

    /**
     * Get the member of the array at a specified index, retaining information
     * about how that member was selected
     * @param index the zero-based subscript of the selected member of the array
     * @return the requested array member, which will itself have a pedigree if it is
     * a singleton map or array.
     * @throws IndexOutOfBoundsException if the index is out of bounds
     */

    @Override
    public GroundedValue get(int index) {
        GroundedValue value = super.get(index);
        return adaptValue(index+1, value);
    }

    /**
     * Get a member of the array at a specific offset, or an empty sequence
     * if the supplied offset is out of range
     *
     * @param index the position of the member to retrieve (zero-based)
     * @return the value at the given position, or an empty sequence
     * if the index is out of range
     */
    @Override
    public GroundedValue getIfPresent(int index) {
        GroundedValue value = super.getIfPresent(index);
        return adaptValue(index + 1, value);
    }

    @Override
    public Iterable<GroundedValue> members() {
        List<GroundedValue> adaptedMembers = new ArrayList<>();
        int index = 0;
        for (GroundedValue value : getBaseItem().members()) {
            adaptedMembers.add(LabeledMapItem.adaptValue(this, Int64Value.makeIntegerValue(++index), value));
        }
        return adaptedMembers;
    }


    private GroundedValue adaptValue(int index, GroundedValue value) {
        return LabeledMapItem.adaptValue(this, Int64Value.makeIntegerValue(index),value);
    }

    /**
     * Get all members of the array except that at a specified position, retaining information
     * about how the remaining members were selected
     *
     * @param index the zero-based subscript of the member of the array that is to be removed
     * @return the remaining array members, which will themselves have a pedigree (indicating
     * their original position in the original array) in the case of members that are singleton
     * maps or arrays.
     * @throws IndexOutOfBoundsException if the index is out of bounds
     */

    @Override
    public ArrayItem remove(int index) {
        List<GroundedValue> list = new ArrayList<>(arrayLength()-1);
        int i = 0;
        for (GroundedValue value : getBaseItem().members()) {
            if (i!=index) {
                list.add(adaptValue(i+1, value));
            }
            i++;
        }
        return new SimpleArrayItem(list);
    }

    @Override
    public ArrayItem removeSeveral(IntSet positions) {
        List<GroundedValue> list = new ArrayList<>(arrayLength() - 1);
        int i = 0;
        for (GroundedValue value : getBaseItem().members()) {
            if (!positions.contains(i)) {
                list.add(adaptValue(i + 1, value));
            }
            i++;
        }
        return new SimpleArrayItem(list);
    }

    /**
     * Get a sub-array given a start and end position
     *
     * @param start the start position (zero based)
     * @param end   the end position (the position of the first item not to be returned)
     *              (zero based)
     * @return a new array item containing the sub-array
     * @throws IndexOutOfBoundsException if start, or start+end, is out of range
     */

    @Override
    public ArrayItem subArray(int start, int end) {
        List<GroundedValue> list = new ArrayList<>(end-start);
        int index = start+1;  // to give a 1-based index
        for (GroundedValue value : super.subArray(start, end).members()) {
            GroundedValue labeledValue = adaptValue(index, value);
            list.add(labeledValue);
            index++;
        }
        return new SimpleArrayItem(list);
    }

    /**
     * Get the pedigree of this array item, providing information as to how it was selected.
     *
     * @return the pedigree of this array item
     */

    @Override
    public StringMap getLabel() {
        return label;
    }

    @Override
    public ArrayItem getBaseItem() {
        return super.getBaseItem();
    }

    public String toString() {
        return "¶" + getBaseItem().toString();
    }
}

// Copyright (c) 2018-2023 Saxonica Limited

