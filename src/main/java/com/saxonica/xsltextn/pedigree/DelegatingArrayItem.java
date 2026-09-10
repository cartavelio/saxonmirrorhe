package com.saxonica.xsltextn.pedigree;

import net.sf.saxon.ma.arrays.AbstractArrayItem;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.z.IntSet;

/**
 * A <code>DelegatingArrayItem</code> is an abstract class representing an array that
 * is implemented by delegating all array-related functionality to a base class.
 * Unless any methods are overridden, the functionality is identical to that
 * of the wrapped array item.
 * <p>Note that methods such as {@link #put} that create a new array will
 * (unless overridden) simply delegate to the base array, and the new array will
 * therefore not be a delegating array.</p>
 */
public abstract class DelegatingArrayItem extends AbstractArrayItem {

    private final ArrayItem base;

    public DelegatingArrayItem(ArrayItem base) {
        this.base = base;
    }

    @Override
    public GroundedValue get(int index) {
        return base.get(index);
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
        return base.getIfPresent(index);
    }

    @Override
    public ArrayItem put(int index, GroundedValue newValue) {
        return base.put(index, newValue);
    }

    @Override
    public int arrayLength() {
        return base.arrayLength();
    }

    @Override
    public boolean isEmpty() {
        return base.isEmpty();
    }

    @Override
    public Iterable<GroundedValue> members() {
        return base.members();
    }

    @Override
    public ArrayItem append(GroundedValue newMember) {
        return base.append(newMember);
    }

    @Override
    public ArrayItem concat(ArrayItem other) {
        return base.concat(other);
    }

    @Override
    public ArrayItem remove(int index) {
        return base.remove(index);
    }

    @Override
    public ArrayItem removeSeveral(IntSet positions) {
        return base.removeSeveral(positions);
    }

    @Override
    public ArrayItem subArray(int start, int end) {
        return base.subArray(start, end);
    }

    @Override
    public ArrayItem insert(int position, GroundedValue member) {
        return base.insert(position, member);
    }

    public ArrayItem getBaseItem() {
        return base;
    }
}

// Copyright (c) 2018-2023 Saxonica Limited

