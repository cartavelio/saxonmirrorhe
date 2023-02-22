using System.Xml;
using Saxon.Impl.Dom;

namespace Saxon.Api {
    /// <summary>
    /// A <c>DomDestination</c> represents an <c>XmlDocument</c> that is constructed to hold the
    /// output of a query or transformation.
    /// </summary>
    /// <remarks>
    /// No data needs to be supplied to the <c>DomDestination</c> object. The query or transformation
    /// populates an <c>XmlDocument</c>, which may then be retrieved as the value of the <c>XmlDocument</c>
    /// property.
    /// </remarks>

    public class DomDestination : IDestination
    {

        private readonly DomBuilder builder;
        private readonly DomDestinationImpl destination;

        /// <summary>Construct a <c>DomDestination</c>.</summary>
        /// <remarks>With this constructor, the system will create a new DOM Document
        /// to act as the destination of the query or transformation results. The document
        /// node of the new document may be retrieved via the <c>XmlDocument</c> property.</remarks>

        public DomDestination()
        {
            builder = new DomBuilder();
            destination = new DomDestinationImpl(builder);
        }

        /// <summary>Construct a <c>DomDestination</c> based on an existing document node.</summary>
        /// <remarks>The new data will be added as a child of the supplied node.</remarks>
        /// <param name="attachmentPoint">The document node to which new contents will
        /// be attached. To ensure that the new document is well-formed, this document node
        /// should have no existing children.</param>

        public DomDestination(XmlDocument attachmentPoint)
        {
            builder = new DomBuilder();
            builder.setAttachmentPoint(attachmentPoint);
            destination = new DomDestinationImpl(builder);
        }

        /// <summary>Construct a <c>DomDestination</c> based on an existing document fragment node.</summary>
        /// <remarks>The new data will be added as a child of the supplied node.</remarks>
        /// <param name="attachmentPoint">The document fragment node to which new contents will
        /// be attached. The new contents will be added after any existing children.</param>

        public DomDestination(XmlDocumentFragment attachmentPoint)
        {
            builder = new DomBuilder();
            builder.setAttachmentPoint(attachmentPoint);
            destination = new DomDestinationImpl(builder);
        }

        /// <summary>Construct a <c>DomDestination</c> based on an existing element node.</summary>
        /// <remarks>The new data will be added as a child of the supplied element node.</remarks>
        /// <param name="attachmentPoint">The element node to which new contents will
        /// be attached. The new contents will be added after any existing children.</param>

        public DomDestination(XmlElement attachmentPoint)
        {
            builder = new DomBuilder();
            builder.setAttachmentPoint(attachmentPoint);
            destination = new DomDestinationImpl(builder);
        }

        /// <summary>After construction, retrieve the constructed document node.</summary>
        /// <remarks>If the zero-argument constructor was used, this will be a newly
        /// constructed document node. If the constructor supplied a document node, the
        /// same document node will be returned. If the constructor supplied a document fragment
        /// node or an element node, this method returns the <c>OwnerDocument</c> property of 
        /// that node.</remarks>

        public XmlDocument XmlDocument => builder.getDocumentNode();


        /// <summary>
        /// Close any resources associated with this destination. Note that this does <b>not</b>
        /// close any user-supplied OutputStream or Writer; those must be closed explicitly
        /// by the calling application.
        /// </summary>
        public void Close()
        {
            builder.close();
        }

        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public void OnClose(System.Action action) {
            destination.onClose(action.Invoke);
        }

        /// <summary>
        /// The underlying <c>Destination</c> object in the Saxon implementation
        /// </summary>
        /// <remarks>
        /// <para>This property provides access to internal methods in the Saxon engine that are
        /// not specifically exposed in the .NET API. In general these methods should be
        /// considered to be less stable than the classes in the Saxon.Api namespace.</para> 
        /// <para>The internal methods follow
        /// Java naming conventions rather than .NET conventions.</para>
        /// <para>Information about the returned <see cref="Saxon.Hej.s9api.Destination"/> object 
        /// (and the objects it provides access to) is included in the Saxon JavaDoc documentation.
        /// </para>
        /// </remarks>
        /// <value>The underlying Destination object</value>
        public Saxon.Hej.s9api.Destination UnderlyingDestination => destination;
    }
}

// Copyright (c) 2021 Saxonica Limited