using System.Xml.Linq;
using Saxon.Impl.XDoc;

namespace Saxon.Api {
    /// <summary>
    /// A <c>LinqDestination</c> represents a LINQ-for-XML <c>XDocument</c> that is constructed to hold the
    /// output of a query or transformation.
    /// </summary>
    /// <remarks>
    /// No data needs to be supplied to the <c>LinqDestination</c> object. The query or transformation
    /// populates an <c>XDocument</c>, which may then be retrieved as the value of the <c>XDocument</c>
    /// property.
    /// </remarks>

    public class LinqDestination : IDestination
    {

        private readonly XDocBuilder builder;
        private readonly LinqDestinationImpl destination;

        /// <summary>Construct a <c>LinqDestination</c>.</summary>
        /// <remarks>With this constructor, the system will create a new LINQ XDocument
        /// to act as the destination of the query or transformation results. The document
        /// node of the new document may be retrieved via the <c>XmlDocument</c> property.</remarks>

        public LinqDestination()
        {
            builder = new XDocBuilder();
            destination = new LinqDestinationImpl(builder);
        }
        
        /// <summary>Construct a <c>LinqDestination</c> using a supplied document.</summary>
        /// <remarks>With this constructor, the system will add the results of the query
        /// or transformation to the supplied (skeletal) XDocument.</remarks>
        /// <param name="doc">A document</param>

        public LinqDestination(XDocument doc)
        {
            builder = new XDocBuilder(doc);
            destination = new LinqDestinationImpl(builder);
        }
        
        /// <summary>After construction, retrieve the newly constructed document node.</summary>

        public System.Xml.Linq.XDocument XDocument => builder.getDocumentNode();


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