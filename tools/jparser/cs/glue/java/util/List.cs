using System;
using System.Collections.Generic;
namespace java.util {
    public interface List<E> : Collection<E> {
        bool add(E entry);
        void add(int index, E entry);
        void remove(int index);
        void set(int index, E entry);
        E get(int index);
        bool contains(E entry);
        bool isEmpty();
        List<E> subList(int start, int end);
        int indexOf(E entry);
        E[] toArray();
        E[] toArray(E[] supplied);
    }
}