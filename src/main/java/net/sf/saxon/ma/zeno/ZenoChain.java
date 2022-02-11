////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.zeno;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

/**
 * An implementation of sequences as a list-of-lists, where the sublists at the
 * end of the master list tend to be small, and the sublists at the start tend
 * to be larger (or the other way around if the list is built by prepending items
 * rather than appending them). The number of sublists is of the order log(N) where
 * N is the length of the sequence, giving logarithmic performance or better for
 * appending items to either end of the sequence, or for getting the Nth item.
 *
 * <p>For a list built by appending to the end, the size of sublists goes as
 * follows as the list grows: (1).. (32).. (32,1).. (32,32).. (64,1).. (64,32)..
 * (64,32,1).. (64,32,32).. (64,64,1).. (64,64,32).. (128,32,1).. (128,64,1)..
 * (128,64,32,1).. For a list of 20,000 items we get 10 sublists with sizes
 * (8192, 4096, 4096, 2048, 1024, 256, 128, 64, 64, 32). The exact numbers don't matter,
 * the important thing is that the number of sublists is log(N) with shorter
 * sublists at the end of the sequence where append/prepend operations take place.</p>
 *
 * <p>When two lists are concatenated, the two master lists are first concatenated,
 * followed by a consolidation to combine short lists now appearing near the middle
 * of the structure, to reduce the number of sublists.</p>
 *
 * <p>The current implementation is a mutable structure, but it is designed to
 * make creation of a mutable variant easy.</p>
 *
 * @param <T> the type of the items in the list
 */

public class ZenoChain<T> implements Iterable<T> {

    private ArrayList<ArrayList<T>> masterList;

    /**
     * Create an empty sequence
     */

    public ZenoChain() {
        masterList = new ArrayList<>(8);
    }

    private ZenoChain(ArrayList<ArrayList<T>> masterList) {
        this.masterList = masterList;
    }

    /**
     * Append an item
     * @param item the item to be appended
     * @return the list after the append operation
     */

    public ZenoChain<T> add(T item) {
        if (masterList.isEmpty()) {
            ArrayList<T> newSegment = new ArrayList<>(32);
            newSegment.add(item);
            masterList.add(newSegment);
            return this;
        }
        int threshold = 32;
        int index = masterList.size() - 1;
        ArrayList<T> segment = masterList.get(index);
        if (segment.size() < threshold) {
            segment.add(item);
            return this;
        } else {
            while (true) {
                index--;
                threshold *= 2;
                if (index < 0) {
                    ArrayList<T> newFinalSegment = new ArrayList<>();
                    newFinalSegment.add(item);
                    masterList.add(newFinalSegment);
                    return this;
                }
                ArrayList<T> priorSegment = masterList.get(index);
                if (priorSegment.size() + segment.size() <= threshold) {
                    priorSegment.addAll(segment);
                    masterList.remove(index+1);
                    ArrayList<T> newFinalSegment = new ArrayList<>();
                    newFinalSegment.add(item);
                    masterList.add(newFinalSegment);
                    return this;
                }
                segment = priorSegment;
            }
        }
    }

    /**
     * Prepend an item
     *
     * @param item the item to be prepended
     * @return the list after the prepend operation
     */

    public ZenoChain<T> prepend(T item) {
        if (masterList.isEmpty()) {
            ArrayList<T> newSegment = new ArrayList<>(32);
            newSegment.add(item);
            masterList.add(newSegment);
            return this;
        }
        int threshold = 32;
        int index = 0;
        ArrayList<T> segment = masterList.get(index);
        if (segment.size() < threshold) {
            segment.add(0, item);
            return this;
        } else {
            while (true) {
                index++;
                threshold *= 2;
                if (index <= masterList.size()) {
                    ArrayList<T> newInitialSegment = new ArrayList<>();
                    newInitialSegment.add(item);
                    masterList.add(0, newInitialSegment);
                    return this;
                }
                ArrayList<T> nextSegment = masterList.get(index);
                if (nextSegment.size() + segment.size() <= threshold) {
                    nextSegment.addAll(segment);
                    masterList.remove(index - 1);
                    ArrayList<T> newInitialSegment = new ArrayList<>();
                    newInitialSegment.add(0, item);
                    masterList.add(0, newInitialSegment);
                    return this;
                }
                segment = nextSegment;
            }
        }
    }

    /**
     * Append a sequence of items
     * @param items the sequence of items to be appended
     * @return the concatenated sequence
     */

    public ZenoChain<T> addAll(Iterable<? extends T> items) {
        ZenoChain<T> result = this;
        for (T item : items) {
            result = result.add(item);
        }
        return result;
    }

    public ZenoChain<T> concat(ZenoChain<T> other) {
        ArrayList<ArrayList<T>> newMaster = new ArrayList<>(masterList.size() + other.masterList.size());
        newMaster.addAll(masterList);
        newMaster.addAll(other.masterList);
        return new ZenoChain<T>(newMaster).reorganize();
    }

    private ZenoChain<T> reorganize() {
        // Useful after concatenating multiple chains, to reduce the number of segments.
        // Starting from the right, if we find a segment that is smaller than both its
        // neighbours, merge it with its left-hand neighbour.
        for (int i=masterList.size()-2; i>=1; i--) {
            int priorSize = masterList.get(i-1).size();
            int segSize = masterList.get(i).size();
            int nextSize = masterList.get(i+1).size();
            if (segSize < priorSize && segSize < nextSize) {
                ArrayList<T> combinedSegment = new ArrayList<>(priorSize + segSize);
                combinedSegment.addAll(masterList.get(i-1));
                combinedSegment.addAll(masterList.get(i));
                masterList.set(i-1, combinedSegment);
                masterList.remove(i);
            }
        }
        return new ZenoChain<T>(masterList);
    }


    /**
     * Get the item at position n, zero-based
     * @param n the requested index
     * @return the item at position n
     * @throws IndexOutOfBoundsException if n is negative or beyond the end of the list
     */

    public T get(int n) {
        if (n < 0) {
            throw new IndexOutOfBoundsException("Index " + n + " is negative");
        }
        int offset = 0;
        for (ArrayList<T> segment : masterList) {
            if (offset + segment.size() > n) {
                return segment.get(n - offset);
            }
            offset += segment.size();
        }
        throw new IndexOutOfBoundsException("Index " + n + " is too large");
    }

    public ZenoChain<T> subList(int start, int end) {
        ArrayList<ArrayList<T>> newMaster = new ArrayList<>();
        int offset = 0;
        int remainingLength = end - start;
        boolean active = false;
        for (ArrayList<T> segment : masterList) {
            if (active) {
                if (remainingLength > segment.size()) {
                    remainingLength -= segment.size();
                    newMaster.add(new ArrayList<T>(segment));
                } else {
                    newMaster.add(new ArrayList<T>(segment.subList(0, remainingLength)));
                    return new ZenoChain<T>(newMaster);
                }
            } else if (offset + segment.size() >= start) {
                int localStart = start - offset;
                if (remainingLength > segment.size() - localStart) {
                    newMaster.add(new ArrayList<T>(segment.subList(localStart, segment.size())));
                    remainingLength -= (segment.size() - localStart);
                    active = true;
                } else {
                    newMaster.add(new ArrayList<T>(segment.subList(localStart, localStart + remainingLength)));
                    return new ZenoChain<T>(newMaster);
                }
            }
            offset += segment.size();
        }
        return new ZenoChain<T>(newMaster);
    }

    /**
     * Get the size of the list
     * @return the size of the list
     */

    public int size() {
        int total = 0;
        for (ArrayList<T> segment : masterList) {
            total += segment.size();
        }
        return total;
    }

    /**
     * Ask if the list is empty
     * @return true if the size is zero
     */

    public boolean isEmpty() {
        return masterList.isEmpty()
                || (masterList.size()==1 && masterList.get(0).isEmpty());
    }

    /**
     * Ask if the list is a singleton
     *
     * @return true if the size is one
     */

    public boolean isSingleton() {
        return masterList.size() == 1 && masterList.get(0).size() == 1;
    }

    /**
     * Iterate over the items
     * @return an iterator over the items
     */

    public Iterator<T> iterator() {
        return new ZenoChainIterator<T>(masterList);
    }


    public String toString() {
        StringBuilder sb = new StringBuilder();
        for (List<T> segment : masterList) {
            sb.append("(");
            for (T item : segment) {
                sb.append(item).append(",");
            }
            sb.setCharAt(sb.length()-1, ')');
        }
        return sb.toString();
    }

    public String show() {
        StringBuilder sb = new StringBuilder();
        for (List<T> segment : masterList) {
            sb.append(segment.size()).append(",");
        }
        return sb.toString();
    }
    public static void main(String[] args) {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i=0; i<20000; i++) {
            chain.add(i);
            System.err.println(chain.show());
        }
        System.err.println(chain.toString());
        System.err.println("ITER");
        for (Integer integer : chain) {
            System.err.println(integer);
        }
    }


}

