using System;
using System.Collections.Generic;
namespace Saxon.Ejava.util {
    public interface Collection<E> : IEnumerable<E> {
        int size();
        bool isEmpty();
        bool contains(E e);
        bool add(E e);
        void addAll(Collection<E> additions);
    }
}