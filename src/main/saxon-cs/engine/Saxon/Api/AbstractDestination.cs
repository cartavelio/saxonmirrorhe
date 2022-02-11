using net.sf.saxon.s9api;
using Action = System.Action;

namespace Saxon.Api {
    
    /// <summary>
    /// An internal class used to wrap an Xslt30Transformer as a Destination so it can
    /// be used in a pipeline.
    /// </summary>
    internal class TransformerDestination : XmlDestination
    {
        private Xslt30Transformer xslt30Transformer;
        private readonly net.sf.saxon.s9api.Destination wrappedDestination;
        private XmlDestination finalDestination;

        internal TransformerDestination(Xslt30Transformer xslt30Transformer, XmlDestination finalDestination)
        {
            this.wrappedDestination = xslt30Transformer.GetUnderlyingXslt30Transformer.asDocumentDestination(
                finalDestination.GetUnderlyingDestination());
            this.finalDestination = finalDestination;
        }

        Destination XmlDestination.GetUnderlyingDestination() {
            return wrappedDestination;
        }
        
        

        public void OnClose(Action action) {
            wrappedDestination.onClose(action.Invoke);
        }
    }
}