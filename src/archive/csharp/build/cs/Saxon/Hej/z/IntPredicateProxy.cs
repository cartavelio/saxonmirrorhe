using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public interface IntPredicateProxy {
        bool test(int value);
        Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other);
    
    }
}