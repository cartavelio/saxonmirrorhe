using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public interface Maker<T>  {
        T make();
    
    }
}