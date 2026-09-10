using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface Binding {
        Saxon.Hej.value.SequenceType getRequiredType();
        Saxon.Hej.value.IntegerValue[] getIntegerBoundsForVariable();
        Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext context);
        bool isGlobal();
        bool isAssignable();
        Saxon.Hej.om.StructuredQName getVariableQName();
        void addReference(Saxon.Hej.expr.VariableReference @ref, bool isLoopingReference);
    
    }
}