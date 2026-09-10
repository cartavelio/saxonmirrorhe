using System.Collections.Generic;
using Saxon.Hej.expr;
using Saxon.Hej.om;
using Saxon.Hej.value;

namespace Saxon.Api.Support {
    public class ExtensionFunctionDefinitionWrapper : Saxon.Hej.lib.ExtensionFunctionDefinition {

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

        public override Saxon.Hej.lib.ExtensionFunctionCall makeCallExpression() {
            Saxon.Api.ExtensionFunctionCall apiCall = apiFunction.MakeFunctionCall();
            return new ExtensionFunctionCallWrapper(apiCall);
        }
    }

    public class ExtensionFunctionCallWrapper : Saxon.Hej.lib.ExtensionFunctionCall {
        private readonly Saxon.Api.ExtensionFunctionCall apiCall;

        public ExtensionFunctionCallWrapper(Saxon.Api.ExtensionFunctionCall apiCall) {
            this.apiCall = apiCall;
        }

        public override void supplyStaticContext(Saxon.Hej.expr.StaticContext context, int locationId, Expression[] arguments) {
            apiCall.SupplyStaticContext(new Saxon.Api.StaticContext(context));
        }

        public override Sequence call(XPathContext context, Sequence[] arguments) {
            DynamicContext dc = new DynamicContext(context);
            XdmValue[] args = new XdmValue[arguments.Length];
            for (int i = 0; i < arguments.Length; i++) {
                args[i] = XdmValue.Wrap(arguments[i]);
            }

            return apiCall.Call(args, dc).Unwrap();
        }
    }
    
    
}

// Copyright (c) 2021 Saxonica Limited