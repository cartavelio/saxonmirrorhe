using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class RegexFunctionSansFlags : Saxon.Hej.functions.SystemFunction {
        private Saxon.Hej.functions.SystemFunction addFlagsArgument() {
            Saxon.Hej.Configuration config = getRetainedStaticContext().getConfiguration();
            Saxon.Hej.functions.SystemFunction @fixed = config.makeSystemFunction(getFunctionName().getLocalPart(), (getArity()+1));
            @fixed.setRetainedStaticContext(getRetainedStaticContext());
            return @fixed;
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.functions.SystemFunction withFlags = addFlagsArgument();
            Saxon.Hej.expr.Expression[] newArgs = new Saxon.Hej.expr.Expression[(arguments.Length+1)];
            Saxon.Ejava.lang.System.arraycopy(arguments, 0, newArgs, 0, arguments.Length);
            newArgs[arguments.Length] = new Saxon.Hej.expr.StringLiteral("");
            return withFlags.makeFunctionCall(newArgs);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.functions.SystemFunction withFlags = addFlagsArgument();
            Saxon.Hej.om.Sequence[] newArgs = new Saxon.Hej.om.Sequence[(args.Length+1)];
            Saxon.Ejava.lang.System.arraycopy(args, 0, newArgs, 0, args.Length);
            newArgs[args.Length] = Saxon.Hej.value.StringValue.EMPTY_STRING;
            return withFlags.call(context, newArgs);
        }
    
    }
}