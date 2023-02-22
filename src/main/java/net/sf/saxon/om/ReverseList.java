package net.sf.saxon.om;

import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;

import java.util.*;

/**
 * A List constructed as a reverse view of an underlying list. Update operations not supported.
 * Inspired by Guava.
 * @param <T> the type of the items in the list
 */

public class ReverseList<T> extends AbstractList<T> {
    private final List<T> forwardList;

    public ReverseList(List<T> forwardList) {
        this.forwardList = Objects.requireNonNull(forwardList);
    }
    
    public static <U> List<U> of(List<U> forwardList) {
        if (forwardList instanceof ReverseList) {
            return ((ReverseList<U>)forwardList).getForwardList();
        } else {
            return new ReverseList<>(forwardList);
        }
    }



    private static void checkElementIndex(int index, int size) {
        if (index < 0 || index >= size) {
            throw new IndexOutOfBoundsException();
        }
    }

    public static void checkPositionIndex(int index, int size) {
        if (index < 0 || index > size) {
            throw new IndexOutOfBoundsException();
        }
    }

    public static void checkPositionIndexes(int start, int end, int size) {
        if (start < 0 || end < start || end > size) {
            throw new IndexOutOfBoundsException();
        }
    }

    List<T> getForwardList() {
        return forwardList;
    }

    private int reverseIndex(int index) {
        int len = size();
        checkElementIndex(index, len);
        return (len - 1) - index;
    }

    private int reversePosition(int index) {
        int len = size();
        checkPositionIndex(index, len);
        return len - index;
    }

    @Override
    public void add(int index, @Nullable T element) {
        throw new UnsupportedOperationException();
    }

    @Override
    public void clear() {
        throw new UnsupportedOperationException();
    }

    @Override
    public T remove(int index) {
        throw new UnsupportedOperationException();
    }

    @Override
    protected void removeRange(int fromIndex, int toIndex) {
        throw new UnsupportedOperationException();
    }

    @Override
    public T set(int index, @Nullable T element) {
        throw new UnsupportedOperationException();
    }

    @Override
    public T get(int index) {
        return forwardList.get(reverseIndex(index));
    }

    @Override
    public int size() {
        return forwardList.size();
    }

    @Override
    public @NotNull List<T> subList(int fromIndex, int toIndex) {
        checkPositionIndexes(fromIndex, toIndex, size());
        return new ReverseList<>(forwardList.subList(reversePosition(toIndex), reversePosition(fromIndex)));
    }

    //#if CSHARP==false
    @Override
    public @NotNull ListIterator<T> listIterator(int index) {
        throw new UnsupportedOperationException();
    }
    //#endif

    @Override
    public @NotNull Iterator<T> iterator() {
        return new Iterator<T>() {
            int i = size() - 1;
            @Override
            public boolean hasNext() {
                return i > 0;
            }

            @Override
            public T next() {
                return forwardList.get(i--);
            }
        };
    }
}