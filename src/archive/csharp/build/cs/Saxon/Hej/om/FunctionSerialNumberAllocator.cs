using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class FunctionSerialNumberAllocator {
        public static Saxon.Hej.om.AtomicCounter serial = new Saxon.Hej.om.AtomicCounter(0);
    
    }
}