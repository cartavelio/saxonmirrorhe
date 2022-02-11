using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class OutputURIResolverWrapper : Saxon.Hej.lib.ResultDocumentResolver {
        private readonly Saxon.Hej.lib.OutputURIResolver outputURIResolver;
        public OutputURIResolverWrapper(Saxon.Hej.lib.OutputURIResolver resolver)  {
            this.outputURIResolver = resolver;
        }
        public Saxon.Hej.@event.Receiver resolve(Saxon.Hej.expr.XPathContext context, string href, string baseUri, Saxon.Hej.serialize.SerializationProperties properties) {
            Saxon.Hej.lib.OutputURIResolver r2 = outputURIResolver.newInstance();
            try {
                Saxon.Ejavax.xml.transform.Result result = r2.resolve(href, baseUri);
                Saxon.Hej.s9api.Action onClose = () => {
                    try {
                        r2.close(result);
                    } catch (Saxon.Ejavax.xml.transform.TransformerException te) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(Saxon.Hej.trans.XPathException.makeXPathException(te));
                    }
                } ;
                Saxon.Hej.@event.Receiver @out;
                if ((result is Saxon.Hej.@event.Receiver)) {
                    @out = (Saxon.Hej.@event.Receiver)(result);
                } else {
                    Saxon.Hej.lib.SerializerFactory factory = context.getConfiguration().getSerializerFactory();
                    Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
                    pipe.setXPathContext(context);
                    @out = factory.getReceiver(result, properties, pipe);
                }
                System.Collections.Generic.IList<Saxon.Hej.s9api.Action> actions = new System.Collections.Generic.List<Saxon.Hej.s9api.Action>();
                actions.Add(onClose);
                return new Saxon.Hej.@event.CloseNotifier(@out, actions);
            } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                throw Saxon.Hej.trans.XPathException.makeXPathException(e);
            }
        }
        public Saxon.Hej.lib.OutputURIResolver getOutputURIResolver() {
            return outputURIResolver;
        }
    
    }
}