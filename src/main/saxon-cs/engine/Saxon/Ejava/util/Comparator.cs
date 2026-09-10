using System;
using System.Collections.Generic;

// A C# emulation of Saxon.Ejava.util.Comparator
 
namespace Saxon.Ejava.util {
    public interface Comparator<in T> {
        int compare(T a, T b);
    }
}