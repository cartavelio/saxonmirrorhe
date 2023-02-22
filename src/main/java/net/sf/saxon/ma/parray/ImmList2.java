////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.parray;

import net.sf.saxon.tree.jiter.ConcatenatingIterator;

import java.util.Iterator;

/**
 * Implementation of an immutable list of arbitrary length, implemented as a binary tree
 * @param <E> the type of the elements of the list
 */
public class ImmList2<E> extends ImmList<E> {

    private final ImmList<E> left;
    private final ImmList<E> right;
    private final int _size;

    protected ImmList2(ImmList<E> left, ImmList<E> right) {
        this.left = left;
        this.right = right;
        this._size = left.size() + right.size();
    }

    @Override
    public E get(int index) {
        if (index < 0) {
            throw outOfBounds(index, _size);
        } else if (index < left.size()) {
            return left.get(index);
        } else if (index < _size) {
            return right.get(index - left.size());
        } else {
            throw outOfBounds(index, _size);
        }
    }

    @Override
    public int size() {
        return _size;
    }

    @Override
    public boolean isEmpty() {
        return false;
    }

    @Override
    public ImmList<E> replace(int index, E member) {
        if (index < 0) {
            throw outOfBounds(index, _size);
        } else if (index < left.size()) {
            return new ImmList2<>(left.replace(index, member), right);
        } else if (index < _size) {
            return new ImmList2<>(left, right.replace(index - left.size(), member));
        } else {
            throw outOfBounds(index, _size);
        }
    }

    @Override
    public ImmList<E> insert(int index, E member) {
        if (index < 0) {
            throw outOfBounds(index, _size);
        } else if (index <= left.size()) {
            return new ImmList2<>(left.insert(index, member), right).rebalance();
        } else if (index <= _size) {
            return new ImmList2<>(left, right.insert(index - left.size(), member)).rebalance();
        } else {
            throw outOfBounds(index, _size);
        }
    }

    @Override
    public ImmList<E> append(E member) {
        return new ImmList2<>(this, new ImmList1<>(member)).rebalance();
    }

    @Override
    public ImmList<E> appendList(ImmList<E> members) {
        return new ImmList2<>(this, members).rebalance();
    }

    @Override
    public ImmList<E> remove(int index) {
        if (index < 0) {
            throw outOfBounds(index, _size);
        } else if (index < left.size()) {
            return new ImmList2<>(left.remove(index), right).rebalance();
        } else if (index < _size) {
            return new ImmList2<>(left, right.remove(index - left.size())).rebalance();
        } else {
            throw outOfBounds(index, _size);
        }
    }

    @Override
    public ImmList<E> subList(int start, int end) {
        if (start < 0 || start >= _size) {
            throw outOfBounds(start, _size);
        } else if (end < start || end > _size) {
            throw outOfBounds(end, _size);
        }
        if (start < left.size() && end <= left.size()) {
            return left.subList(start, end);
        } else if (start >= left.size() && end >= left.size()) {
            return right.subList(start - left.size(), end - left.size());
        } else {
            return new ImmList2<>(left.subList(start, left.size()), right.subList(0, end - left.size())).rebalance();
        }
    }

    @Override
    public Iterator<E> iterator() {
        return new ConcatenatingIterator<>(left.iterator(), () -> right.iterator());
    }

    private static final int THRESHOLD = 3;

    private static String showBalance(ImmList list, int depth) {
        if (depth == 0 || !(list instanceof ImmList2)) {
            return "" + list.size();
        } else {
            return "(" + showBalance(((ImmList2)list).left, depth-1) + "," + showBalance(((ImmList2) list).right, depth-1)  + ")";
        }
    }

    @Override
    protected ImmList<E> rebalance() {
        if (left.isEmpty()) {
            return right;
        }
        if (right.isEmpty()) {
            return left;
        }
        ImmList<E> l2 = left;
        ImmList<E> r2 = right;
        //System.err.println("Rebalance " + size() + " - " + showBalance(this, 3));
        if (size() > THRESHOLD) {
            if (l2 instanceof ImmList2 && l2.size() > THRESHOLD * r2.size()) {
                return new ImmList2<>(((ImmList2<E>) l2).left, new ImmList2<>(((ImmList2<E>) l2).right, r2));
            } else if (r2 instanceof ImmList2 && r2.size() > THRESHOLD * l2.size()) {
                return new ImmList2<>(new ImmList2<>(l2, ((ImmList2<E>) r2).left), ((ImmList2<E>) r2).right);
            } else {
                return this;
            }
        } else {
            return this;
        } 
    }
}

