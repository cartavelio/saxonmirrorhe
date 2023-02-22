using System.Collections.Generic;
using net.sf.saxon.expr;
using net.sf.saxon.om;
using Saxon.Api.Support;

namespace Saxon.Api {
    internal class WrappedExtensionFunctionCall : net.sf.saxon.lib.ExtensionFunctionCall {

        ExtensionFunctionCall functionCall;

        public WrappedExtensionFunctionCall(ExtensionFunctionCall call)
        {
            this.functionCall = call;
        }
        
        public override void supplyStaticContext(net.sf.saxon.expr.StaticContext context, int locationId, Expression[] arguments)
        {
            StaticContext sc = new StaticContext(context);
            functionCall.SupplyStaticContext(sc);
        }

        public override void copyLocalData(net.sf.saxon.lib.ExtensionFunctionCall destination)
        {
            functionCall.CopyLocalData(((WrappedExtensionFunctionCall)destination).functionCall);
        }

        public override Sequence call(XPathContext context, Sequence [] argument)
        {
            SequenceEnumerator<XdmItem>[] na = new SequenceEnumerator<XdmItem>[argument.Length];
            for (int i = 0; i < na.Length; i++)
            {
                na[i] = new SequenceEnumerator<XdmItem>((JXdmSequenceIterator)XdmValue.FromGroundedValueToJXdmValue(argument[i].materialize()).iterator());
            }
            DynamicContext dc = new DynamicContext(context);
            IEnumerator<XdmItem> result = functionCall.Call(na, dc);
            return new net.sf.saxon.om.LazySequence(new net.sf.saxon.om.IteratorWrapper(new net.sf.saxon.dotnet.DotNetIterator(result, new Mapper())));
        }
    }
}