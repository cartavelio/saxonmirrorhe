using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface FunctionItem : Saxon.Hej.om.Item, Saxon.Hej.expr.Callable, Saxon.Hej.om.GroundedValue {
        bool isMap();
        bool isArray();
        Saxon.Hej.type.FunctionItemType getFunctionItemType();
        Saxon.Hej.om.StructuredQName getFunctionName();
        int getArity();
        bool isSequenceVariadic();
        Saxon.Hej.expr.OperandRole[] getOperandRoles();
        Saxon.Hej.query.AnnotationList getAnnotations();
        Saxon.Hej.expr.XPathContext makeNewContext(Saxon.Hej.expr.XPathContext callingContext, Saxon.Hej.expr.ContextOriginator originator);
        bool deepEquals(Saxon.Hej.om.FunctionItem other, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.sort.AtomicComparer comparer, int flags);
        string getDescription();
        void export(Saxon.Hej.trace.ExpressionPresenter @out);
        bool isTrustedResultType();
    
    }
}