using Saxon.Hej.s9api;
using Action = System.Action;

namespace Saxon.Api {
    
    /// <summary>
    /// An internal class used to wrap an Xslt30Transformer as a Destination so it can
    /// be used in a pipeline.
    /// </summary>
    internal class TransformerDestination : IDestination
    {
        private Xslt30Transformer xslt30Transformer;
        private readonly Saxon.Hej.s9api.Destination wrappedDestination;
        private IDestination finalDestination;

        internal TransformerDestination(Xslt30Transformer xslt30Transformer, IDestination finalDestination)
        {
            this.wrappedDestination = xslt30Transformer.GetUnderlyingXslt30Transformer.asDocumentDestination(
                finalDestination.GetUnderlyingDestination());
            this.finalDestination = finalDestination;
        }

        Destination IDestination.GetUnderlyingDestination() {
            return wrappedDestination;
        }
        
        

        public void OnClose(Action action) {
            wrappedDestination.onClose(action.Invoke);
        }
    }
}