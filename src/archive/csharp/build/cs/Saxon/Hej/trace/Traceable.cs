using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public interface Traceable : Saxon.Hej.expr.Locatable {
        Saxon.Hej.om.StructuredQName getObjectName();
        void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer);
    
    }
}