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
    
    }
}