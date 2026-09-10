package com.saxonica.xsltextn.pedigree;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.GroundedValue;

import java.util.ArrayList;
import java.util.List;

/**
 * A <code>PedigreeArrayItem</code> represents an XDM array together with a {@link Pedigree}:
 * that is, retained information about how the array was selected with a JSON-like tree.
 */
public class PedigreeArrayItem extends DelegatingArrayItem implements PedigreeValue {

    Pedigree pedigree;

    /**
     * Create a PedigreeArrayItem with an empty pedigree
     * @param base the underlying array item
     */

    public PedigreeArrayItem(ArrayItem base) {
        this(base, Pedigree.PEDIGREE_ROOT);
    }

    /**
     * Create a PedigreeArrayItem with a known pedigree
     * @param base the underlying array item
     */

    public PedigreeArrayItem(ArrayItem base, Pedigree pedigree) {
        super(base);
        this.pedigree = pedigree;
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
        Pedigree ped = new Pedigree(this, index);
        return ped.makePedigreeValue(value);
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
        for (GroundedValue seq : super.remove(index).members()) {
            if (i==index) {
                i++;
            }
            Pedigree ped = new Pedigree(this, i++);
            list.add(ped.makePedigreeValue(seq));
        }
        return new SimpleArrayItem(list);
    }

    @Override
    public ArrayItem subArray(int start, int end) {
        List<GroundedValue> list = new ArrayList<>(end-start);
        int index = start;
        for (GroundedValue seq : super.subArray(start, end).members()) {
            Pedigree ped = new Pedigree(this, index++);
            list.add(ped.makePedigreeValue(seq));
        }
        return new SimpleArrayItem(list);
    }

    /**
     * Return the members of the array as an iterable collection
     * @return the members of the array as an iterable collection. Any members
     * that are singleton maps or arrays will have a pedigree indicating
     * how they were selected.
     */

    @Override
    public Iterable<GroundedValue> members() {
        List<GroundedValue> memberValues = new ArrayList<>();
        int index = 0;
        for (GroundedValue val : super.members()) {
            Pedigree ped = new Pedigree(PedigreeArrayItem.this, index++);
            memberValues.add(ped.makePedigreeValue(val));
        }
        return memberValues;
    }

    /**
     * Get the pedigree of this array item, providing information as to how it was selected.
     * @return the pedigree of this array item
     */

    @Override
    public Pedigree getPedigree() {
        return pedigree;
    }

    @Override
    public ArrayItem getBaseItem() {
        return super.getBaseItem();
    }
}

// Copyright (c) 2018-2023 Saxonica Limited

