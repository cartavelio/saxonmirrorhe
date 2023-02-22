using System;
using System.Linq;
using Saxon.Hej.expr;
using Saxon.Hej.om;
using Saxon.Hej.type;
using Saxon.Hej.value;

namespace Saxon.Api.Support {
    public class LambdaExtensionFunctionWrapper : Saxon.Hej.lib.ExtensionFunctionDefinition {

        private readonly StructuredQName name;
        private readonly SpecificFunctionType type;
        private readonly Func<XdmValue[], XdmValue> implementation;
        
        public LambdaExtensionFunctionWrapper(StructuredQName name, SpecificFunctionType type, Func<XdmValue[], XdmValue> implementation) {
            this.name = name;
            this.type = type;
            this.implementation = implementation;
        }

        public override StructuredQName getFunctionQName() {
            return name;
        }

        public override SequenceType[] getArgumentTypes() {
            return type.getArgumentTypes();
        }

        public override SequenceType getResultType(SequenceType[] suppliedArgumentTypes) {
            return type.getResultType();
        }

        public override Saxon.Hej.lib.ExtensionFunctionCall makeCallExpression() {
            return new LambdaExtensionFunctionCallWrapper(implementation);
        }

        private class LambdaExtensionFunctionCallWrapper : Saxon.Hej.lib.ExtensionFunctionCall {
            private readonly Func<XdmValue[], XdmValue> implementation;
            public LambdaExtensionFunctionCallWrapper(Func<XdmValue[], XdmValue> implementation) {
                this.implementation = implementation;
            }
            public override Sequence call(XPathContext context, Sequence[] arguments) {
                XdmValue[] wrappedArgs = arguments.Select(XdmValue.Wrap).ToArray();
                return implementation.Invoke(wrappedArgs).Unwrap();
            }
        }
    }
}

// Copyright (c) 2021 Saxonica Limited