using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface BindingReference {
        void setStaticType(Saxon.Hej.value.SequenceType type, Saxon.Hej.om.GroundedValue constantValue, int properties);
        void fixup(Saxon.Hej.expr.Binding binding);
    
    }
}