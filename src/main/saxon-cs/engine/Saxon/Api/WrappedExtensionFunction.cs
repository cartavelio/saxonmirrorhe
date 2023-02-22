using System;

namespace Saxon.Api {
    internal class WrappedExtensionFunction : net.sf.saxon.s9api.ExtensionFunction
    {
        ExtensionFunction definition;

        public WrappedExtensionFunction(ExtensionFunction definition)
        {
            this.definition = definition;
        }

        public net.sf.saxon.s9api.XdmValue call(net.sf.saxon.s9api.XdmValue[] xvarr)
        {
            XdmValue[] values = new XdmValue[xvarr.Length];
            int len = xvarr.Length;
            for (int i = 0; i < len; i++) {
                values[i] = XdmValue.Wrap(xvarr[i].getUnderlyingValue());
            }
            try {
                XdmValue result = definition.Call(values);
                return XdmValue.FromGroundedValueToJXdmValue(result.value);
            }
            catch (Exception ex) {
                throw new DynamicError(ex.Message);
            }
            
        }

        public JXdmSequenceType[] getArgumentTypes()
        {
            XdmSequenceType [] types = definition.GetArgumentTypes();
            JXdmSequenceType[] results = new JXdmSequenceType[types.Length];
            for (int i = 0; i < types.Length; i++) {
                results[i] = types[i].ToJXdmSequenceType();
            }
            return results;
        }

        public net.sf.saxon.s9api.QName getName()
        {
            return definition.GetName().UnderlyingQName();
        }

        public JXdmSequenceType getResultType()
        {
            JXdmSequenceType declaredResult = definition.GetResultType().ToJXdmSequenceType();
            return declaredResult;
        }
    }
}