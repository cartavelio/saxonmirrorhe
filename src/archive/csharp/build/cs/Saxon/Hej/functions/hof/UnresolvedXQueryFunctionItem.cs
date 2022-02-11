using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class UnresolvedXQueryFunctionItem : Saxon.Hej.functions.AbstractFunction {
        private readonly Saxon.Hej.query.XQueryFunction fd;
        private readonly Saxon.Hej.trans.SymbolicName.F functionName;
        private readonly Saxon.Hej.functions.hof.UserFunctionReference @ref;
        public UnresolvedXQueryFunctionItem(Saxon.Hej.query.XQueryFunction fd, Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.functions.hof.UserFunctionReference @ref)  {
            this.fd = fd;
            this.functionName = functionName;
            this.@ref = @ref;
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            return new Saxon.Hej.type.SpecificFunctionType(fd.getArgumentTypes(), fd.getResultType());
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return functionName.getComponentName();
        }
        public override int getArity() {
            return fd.getNumberOfArguments();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            return @ref.evaluateItem(context).call(context, args);
        }
        public override string getDescription() {
            return functionName.ToString();
        }
        public Saxon.Hej.functions.hof.UserFunctionReference getFunctionReference() {
            return @ref;
        }
    
    }
}