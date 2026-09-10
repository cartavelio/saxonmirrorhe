using System;
using System.Collections.Generic;

/**
 * A C# emulation of java.util.Comparator
 */
 
namespace java.util {
    public interface Comparator<in T> {
        int compare(T a, T b);
    }
}