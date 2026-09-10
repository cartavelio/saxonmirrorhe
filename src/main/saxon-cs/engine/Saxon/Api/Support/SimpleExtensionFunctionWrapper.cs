using System.Collections.Generic;
using net.sf.saxon.expr;
using net.sf.saxon.om;
using net.sf.saxon.value;

namespace Saxon.Api.Support {
    public class SimpleExtensionFunctionWrapper : net.sf.saxon.lib.ExtensionFunctionDefinition {

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

        public override net.sf.saxon.lib.ExtensionFunctionCall makeCallExpression() {
            return new SimpleExtensionFunctionCallWrapper(apiFunction);
        }
    }
    
    public class SimpleExtensionFunctionCallWrapper : net.sf.saxon.lib.ExtensionFunctionCall {
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