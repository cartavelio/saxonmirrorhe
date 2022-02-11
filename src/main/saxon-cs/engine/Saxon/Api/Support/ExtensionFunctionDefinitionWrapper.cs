using System.Collections.Generic;
using net.sf.saxon.expr;
using net.sf.saxon.om;
using net.sf.saxon.value;

namespace Saxon.Api.Support {
    public class ExtensionFunctionDefinitionWrapper : net.sf.saxon.lib.ExtensionFunctionDefinition {

        private Saxon.Api.ExtensionFunctionDefinition apiFunction;
        public ExtensionFunctionDefinitionWrapper(Saxon.Api.ExtensionFunctionDefinition apiFunction) {
            this.apiFunction = apiFunction;
        }

        public override StructuredQName getFunctionQName() {
            return apiFunction.FunctionName.GetStructuredQName();
        }

        public override SequenceType[] getArgumentTypes() {
            SequenceType[] result = new SequenceType[apiFunction.ArgumentTypes.Length];
            for (int i = 0; i < apiFunction.ArgumentTypes.Length; i++) {
                result[i] = apiFunction.ArgumentTypes[i].ToSequenceType();
            }

            return result;
        }

        public override SequenceType getResultType(SequenceType[] suppliedArgumentTypes) {
            XdmSequenceType[] args = new XdmSequenceType[suppliedArgumentTypes.Length];
            for (int i = 0; i < suppliedArgumentTypes.Length; i++) {
                args[i] = XdmSequenceType.FromSequenceType(suppliedArgumentTypes[i]);
            }

            XdmSequenceType resultType = apiFunction.ResultType(args);
            return resultType.ToSequenceType();
        }

        public override net.sf.saxon.lib.ExtensionFunctionCall makeCallExpression() {
            Saxon.Api.ExtensionFunctionCall apiCall = apiFunction.MakeFunctionCall();
            return new ExtensionFunctionCallWrapper(apiCall);
        }
    }

    public class ExtensionFunctionCallWrapper : net.sf.saxon.lib.ExtensionFunctionCall {
        private readonly Saxon.Api.ExtensionFunctionCall apiCall;

        public ExtensionFunctionCallWrapper(Saxon.Api.ExtensionFunctionCall apiCall) {
            this.apiCall = apiCall;
        }
        public override Sequence call(XPathContext context, Sequence[] arguments) {
            DynamicContext dc = new DynamicContext(context);
            IEnumerator<XdmItem>[] args = new IEnumerator<XdmItem>[arguments.Length];
            for (int i = 0; i < arguments.Length; i++) {
                args[i] = XdmValue.Wrap(arguments[i]).GetEnumerator();
            }

            IEnumerator<XdmItem> result = apiCall.Call(args, dc);
            List<Item> unwrappedResult = new List<Item>();
            while (result.MoveNext()) {
                unwrappedResult.Add((Item)result.Current.Unwrap());
            }

            return new SequenceExtent.Of<Item>(unwrappedResult);
        }
    }
    
    
}