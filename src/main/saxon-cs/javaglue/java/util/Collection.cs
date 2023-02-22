using System;
using System.Collections.Generic;
namespace java.util {
    public interface Collection<E> : java.lang.Iterable<E> {
        int size();
        bool isEmpty();
        bool contains(E e);
        bool add(E e);
        void addAll(Collection<E> additions);
    }
}