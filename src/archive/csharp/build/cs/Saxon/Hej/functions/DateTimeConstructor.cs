using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class DateTimeConstructor : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.DateValue arg0 = (Saxon.Hej.value.DateValue)(arguments[0].head());
            Saxon.Hej.value.TimeValue arg1 = (Saxon.Hej.value.TimeValue)(arguments[1].head());
            if (((arg0 == null) || (arg1 == null))) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            return Saxon.Hej.value.DateTimeValue.makeDateTimeValue(arg0, arg1);
        }
        public override string getCompilerName() {
            return "DateTimeConstructorCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.DateTimeConstructor.DateTimeFnElaborator();
        }
        public class DateTimeFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall sfc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator arg0eval = sfc.getArg(0).makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator arg1eval = sfc.getArg(1).makeElaborator().elaborateForItem();
                return context => Saxon.Hej.value.DateTimeValue.makeDateTimeValue((Saxon.Hej.value.DateValue)(/*Delegate.eval()*/arg0eval(context)), (Saxon.Hej.value.TimeValue)(/*Delegate.eval()*/arg1eval(context)));
            }
        
        }
    
    }
}