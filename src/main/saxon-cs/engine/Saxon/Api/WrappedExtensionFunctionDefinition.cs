using System;
using net.sf.saxon.om;
using net.sf.saxon.value;

namespace Saxon.Api {
    internal class WrappedExtensionFunctionDefinition : net.sf.saxon.lib.ExtensionFunctionDefinition
    {
        ExtensionFunctionDefinition definition;

        public WrappedExtensionFunctionDefinition(ExtensionFunctionDefinition definition)
        {
            this.definition = definition;
        }

        public override StructuredQName getFunctionQName()
        {
            return definition.FunctionName.GetStructuredQName();
        }

        public override int getMinimumNumberOfArguments()
        {
            return definition.MinimumNumberOfArguments;
        }

        public override int getMaximumNumberOfArguments()
        {
            return definition.MaximumNumberOfArguments;
        }

        public override SequenceType[] getArgumentTypes()
        {
            XdmSequenceType[] dt = definition.ArgumentTypes;
            SequenceType[] jt = new SequenceType[dt.Length];
            for (int i = 0; i < dt.Length; i++)
            {
                jt[i] = dt[i].ToSequenceType();
            }
            return jt;
        }

        public override SequenceType getResultType(SequenceType[] argumentTypes)
        {
            XdmSequenceType[] dt = new XdmSequenceType[argumentTypes.Length];
            for (int i = 0; i < dt.Length; i++)
            {
                dt[i] = XdmSequenceType.FromSequenceType(argumentTypes[i]);
            }

            XdmSequenceType rt = definition.ResultType(dt);
            return rt.ToSequenceType();
        }

        public override Boolean trustResultType()
        {
            return definition.TrustResultType;
        }

        public override Boolean dependsOnFocus()
        {
            return definition.DependsOnFocus;
        }

        public override Boolean hasSideEffects()
        {
            return definition.HasSideEffects;
        }

        public override net.sf.saxon.lib.ExtensionFunctionCall makeCallExpression()
        {
            return new WrappedExtensionFunctionCall(definition.MakeFunctionCall());
        }

    }
}