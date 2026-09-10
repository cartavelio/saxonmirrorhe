using System;
using Saxon.Ejava.lang;

#nullable enable

namespace Saxon.Api.Sapling
{
    /// <summary>
    /// A document node in a sapling tree. A document node may have a sequence of children, each of which
    /// is an element, text, comment, or processing-instruction node. A document node has an optional base URI.
    /// <para>Like all sapling nodes, a <code>SaplingDocument</code> is immutable. All operations, such as adding children,
    /// deliver a new document node. A sapling node generally exists only transiently during tree
    /// construction; to make use of the constructed tree, it will usually be converted to a regular tree when
    /// construction is complete, using <see cref="ToXdmNode"/>.
    /// </para>
    /// </summary>
    public class SaplingDocument: SaplingNode
    {
        /// <summary>
        /// Returns the (optional) base URI.
        /// </summary>
        public Uri? BaseUri { get; }

        /// <summary>
        /// Constructs a new document with no base URI and no children.
        /// </summary>
        public SaplingDocument()
        {
            BaseUri = null;
            NodeKind = XdmNodeKind.Document;
            javaDelegate = new Saxon.Hej.sapling.SaplingDocument();
        }
        
        /// <summary>
        /// Constructs a new document with the specified base URI and no children.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        public SaplingDocument(Uri baseUri)
        {
            BaseUri = baseUri;
            NodeKind = XdmNodeKind.Document;
            if (baseUri == null)
            {
                javaDelegate = new Saxon.Hej.sapling.SaplingDocument();
            }
            else
            {
                javaDelegate = new Saxon.Hej.sapling.SaplingDocument(baseUri.ToString());
            }
        }

        private SaplingDocument(Saxon.Hej.sapling.SaplingDocument javaSapling)
        {
            BaseUri = javaSapling.getSystemId() == null ? null : new Uri(javaSapling.getSystemId());
            NodeKind = XdmNodeKind.Document;
            javaDelegate = javaSapling;
        }

        /// <summary>
        /// Constructs an XdmNode from the sapling document.
        /// </summary>
        /// <param name="processor">The processor that is to own the resulting node.</param>
        /// <returns>The document node at the root of the constructed tree. The implementation model
        /// for the tree will be the default tree model of the processor's configuration.</returns>
        /// <exception cref="SaxonApiException">Throws <c>SaxonApiException</c> if construction fails.</exception>
        public XdmNode ToXdmNode(Processor processor)
        {
            return ((Saxon.Hej.sapling.SaplingDocument)javaDelegate).toXdmNode(processor);
        }

        /// <summary>
        /// Sends the document to an arbitrary destination.
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <param name="destination">The destination to which the document will be copied.</param>
        /// <exception cref="SaxonApiException">if anything goes wrong.</exception>
        public void Send(Processor processor, IDestination destination)
        {
            ((Saxon.Hej.sapling.SaplingDocument) javaDelegate).Send(processor, destination);
        }

        /// <summary>
        /// Serialize the document.
        /// </summary>
        /// <param name="serializer">the serializer to be used.</param>
        /// <exception cref="SaxonApiException">if anything goes wrong.</exception>
        public void Serialize(Serializer serializer)
        {
            ((Saxon.Hej.sapling.SaplingDocument) javaDelegate).Serialize(serializer);
        }
        
        /// <summary>
        /// Add a number of child nodes to the document node, returning a new document with additional children
        /// beyond those already present. Neither the original document nor the children are modified.
        /// </summary>
        /// <param name="children">The nodes to add, in document order.</param>
        /// <returns>A new parent document node.</returns>
        /// <exception cref="IllegalArgumentException">if any of the supplied nodes is a document node.</exception>
        public SaplingDocument WithChild(params SaplingNode[] children)
        {
            var jchildren = new Saxon.Hej.sapling.SaplingNode[children.Length];
            for (int pos = 0; pos < children.Length; pos++)
            {
                jchildren[pos] = children[pos].javaDelegate;
            }

            var newDocument = ((Saxon.Hej.sapling.SaplingDocument)javaDelegate).withChild(jchildren);
            return new SaplingDocument(newDocument);
        }
    }
}