using net.sf.saxon.s9api;

namespace Saxon.Api {
    internal class AbstractDestination : XmlDestination
    {
        private Xslt30Transformer xslt30Transformer;
        private XmlDestination destination;

        internal AbstractDestination(Xslt30Transformer xslt30Transformer, XmlDestination destination)
        {
            this.xslt30Transformer = xslt30Transformer;
            this.destination = destination;
        }

        Destination XmlDestination.GetUnderlyingDestination()
        {
            return xslt30Transformer.GetUnderlyingXslt30Transformer.asDocumentDestination(destination.GetUnderlyingDestination());
        }
    }
}