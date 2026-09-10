using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class StreamAvailable : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            bool result = isAvailable(arguments[0].head().getStringValue(), context);
            return Saxon.Hej.value.BooleanValue.get(result);
        }
        private bool isAvailable(string uri, Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.@event.Receiver tester = new Saxon.Hej.functions.StreamAvailable.StreamTester(context.getConfiguration().makePipelineConfiguration());
                Saxon.Hej.functions.DocumentFn.sendDoc(uri, getRetainedStaticContext().getStaticBaseUriString(), context, null, tester, new Saxon.Hej.lib.ParseOptions());
            } catch (Saxon.Hej.trans.QuitParsingException e) {
                return true;
            } catch (Saxon.Hej.trans.XPathException e) {
                return false;
            }
            return false;
        }
        private class StreamTester : Saxon.Hej.@event.ProxyReceiver {
            public StreamTester(Saxon.Hej.@event.PipelineConfiguration pipe) : base (new Saxon.Hej.@event.Sink(pipe)) {
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                throw new Saxon.Hej.trans.QuitParsingException(false);
            }
        
        }
    
    }
}