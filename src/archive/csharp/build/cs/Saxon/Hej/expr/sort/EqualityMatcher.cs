using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public interface EqualityMatcher<T>  {
        bool equal(T a, T b);
        int hash(T a);
    
    }
}