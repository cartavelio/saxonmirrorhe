using System;
using System.Collections.Generic;
namespace java.util {
    public interface List<E> : Collection<E> {
        bool add(E entry);
        void add(int index, E entry);
        // The overloading in Java between remove(E) and remove(int) is hopelessly
        // difficult to resolve. We provide remove(E) and removeAt(int).
        bool remove(E entry);
        E removeAt(int index);
        void clear();
        void set(int index, E entry);
        E get(int index);
        bool contains(E entry);
        bool isEmpty();
        List<E> subList(int start, int end);
        int indexOf(E entry);
        E[] toArray();
        S[] toArray<S>(S[] supplied) where S : E;
        Iterator<E> iterator();
        void sort(java.util.Comparator<E> comparer);
        void sort(System.Comparison<E> comparer);
    }
}