using System.Collections.Generic;
using Saxon.Hej.expr;
using Saxon.Hej.om;
using Saxon.Hej.value;

namespace Saxon.Api.Support {
    public class SimpleExtensionFunctionWrapper : Saxon.Hej.lib.ExtensionFunctionDefinition {

        private Saxon.Api.ExtensionFunction apiFunction;
        public SimpleExtensionFunctionWrapper(Saxon.Api.ExtensionFunction apiFunction) {
            this.apiFunction = apiFunction;
        }
        
        

        public override StructuredQName getFunctionQName() {
            return apiFunction.GetName().GetStructuredQName();
        }

        public override SequenceType[] getArgumentTypes() {
            XdmSequenceType[] declaredArgs = apiFunction.GetArgumentTypes();
            SequenceType[] result = new SequenceType[declaredArgs.Length];
            for (int i = 0; i < declaredArgs.Length; i++) {
                result[i] = declaredArgs[i].ToSequenceType();
            }

            return result;
        }

        public override SequenceType getResultType(SequenceType[] suppliedArgumentTypes) {
            return apiFunction.GetResultType().ToSequenceType();
        }

        public override Saxon.Hej.lib.ExtensionFunctionCall makeCallExpression() {
            return new SimpleExtensionFunctionCallWrapper(apiFunction);
        }
    }
    
    public class SimpleExtensionFunctionCallWrapper : Saxon.Hej.lib.ExtensionFunctionCall {
        private Saxon.Api.ExtensionFunction apiFunction;

        public SimpleExtensionFunctionCallWrapper(Saxon.Api.ExtensionFunction apiFunction) {
            this.apiFunction = apiFunction;
        }
        public override Sequence call(XPathContext context, Sequence[] arguments) {
            DynamicContext dc = new DynamicContext(context);
            XdmValue[] args = new XdmValue[arguments.Length];
            for (int i = 0; i < arguments.Length; i++) {
                args[i] = XdmValue.Wrap(arguments[i]);
            }

            XdmValue result = apiFunction.Call(args);
            return result.Unwrap();
        }
    }

}