using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public interface StatefulSystemFunction {
        Saxon.Hej.functions.SystemFunction copy();
    
    }
}