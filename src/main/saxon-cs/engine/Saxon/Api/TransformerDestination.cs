using System;
using Saxon.Hej.s9api;

namespace Saxon.Api {
    
    /// <summary>
    /// An internal class used to wrap an Xslt30Transformer as a Destination so it can
    /// be used in a pipeline.
    /// </summary>
    internal class TransformerDestination : IDestination
    {
		private readonly Saxon.Hej.s9api.Destination wrappedDestination;

        internal TransformerDestination(Xslt30Transformer xslt30Transformer, IDestination finalDestination) {
            wrappedDestination = xslt30Transformer.GetUnderlyingXslt30Transformer.asDocumentDestination(
                finalDestination.UnderlyingDestination);
        }

        Destination IDestination.UnderlyingDestination {
            get { return wrappedDestination; }
        }


        public void OnClose(System.Action action) {
            wrappedDestination.onClose(action.Invoke);
        }
    }
}

// Copyright (c) 2021 Saxonica Limited