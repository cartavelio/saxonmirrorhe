using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface LastPositionFinder {
        bool supportsGetLength();
        int getLength();
    
    }
}