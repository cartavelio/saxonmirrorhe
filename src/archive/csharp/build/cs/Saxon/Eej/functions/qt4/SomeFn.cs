using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class SomeFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.SequenceIterator iter = arguments[0].iterate();
            Saxon.Hej.om.FunctionItem predicate = ((arguments.Length>1) ? (Saxon.Hej.om.FunctionItem)(arguments[1].head()) : new Saxon.Hej.functions.hof.CoercedFunction(Saxon.Hej.functions.SystemFunction.makeFunction("identity", getRetainedStaticContext(), 1), new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ITEM}, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN)));
            for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                bool condition = ((Saxon.Hej.value.BooleanValue)(dynamicCall(predicate, context, new Saxon.Hej.om.Sequence[]{item}).head())).getBooleanValue();
                if (condition) {
                    iter.close();
                    return Saxon.Hej.value.BooleanValue.TRUE;
                }
            }
            return Saxon.Hej.value.BooleanValue.FALSE;
        }
    
    }
}