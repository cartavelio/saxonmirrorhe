using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface IdentityComparable {
        bool isIdentical(Saxon.Hej.om.IdentityComparable other);
        int identityHashCode();
    
    }
}