using System;
using System.Collections.Generic;
namespace java.util {
    public interface Set<E> : Collection<E> {
        int size();
        bool contains(object key);
        void add(E entry);
    }
}