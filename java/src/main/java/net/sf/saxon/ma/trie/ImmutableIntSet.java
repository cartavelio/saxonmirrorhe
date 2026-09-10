package net.sf.saxon.ma.trie;

import java.util.BitSet;

/**
 * A simple implementation of an immutable set of integers, supporting add() and contains()
 * only (no remove(), no enumeration of the contents), and only intended to support a few
 * hundred elements. The use of a Bloom filter means that contains() will usually be very
 * fast in the case where the value is absent from the set, whereas confirming presence may
 * be slower.
 */

public class ImmutableIntSet {

    private final BitSet bloom;
    private final int head;
    private final ImmutableIntSet tail;
    private final int _size;

    public ImmutableIntSet() {
        this.head = Integer.MIN_VALUE;
        this.tail = null;
        this.bloom = new BitSet(1024);
        this._size = 0;
    }

    public ImmutableIntSet(int head, ImmutableIntSet tail, BitSet bloom, int size) {
        this.head = head;
        this.tail = tail;
        this.bloom = bloom;
        this._size = size;
    }

    private int hash1(int n) {
        return n & 1023;
    }

    private int hash2(int n) {
        return n >> 10 & 1023;
    }

    private int hash3(int n) {
        return n >> 20 & 1023;
    }

    public boolean contains(int n) {
        if (_size == 0) {
            return false;
        }
        if (bloom.get(hash1(n)) && bloom.get(hash2(n)) && bloom.get(hash3(n))) {
            ImmutableIntSet current = this;
            while (current != null) {
                if (current.head == n) {
                    return true;
                } else {
                    current = current.tail;
                }
            }
            //System.err.println("Bloom filter: false positive");
        }
        return false;
    }

    public int size() {
        return _size;
    }

    public ImmutableIntSet add(int n) {
        if (contains(n)) {
            return this;
        }

        BitSet b2 = (BitSet)bloom.clone();
        b2.set(hash1(n));
        b2.set(hash2(n));
        b2.set(hash3(n));
        return new ImmutableIntSet(n, this, b2, _size +1);
    }


}

