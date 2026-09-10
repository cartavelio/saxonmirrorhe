using System;

namespace Saxon.Api {
    /// <summary>
    /// The <c>SchemaResolver</c> is a user-supplied class used for resolving references to
    /// schema documents. It applies to references from one schema document to another
    /// appearing in <c>xs:import</c>, <c>xs:include</c>, and <c>xs:redefine</c>; to
    /// references from an instance document to a schema in <c>xsi:schemaLocation</c> and
    /// <c>xsi:noNamespaceSchemaLocation</c>, to <c>xsl:import-schema</c> in XSLT, and to
    /// the <c>import schema</c> declaration in XQuery.
    /// </summary>

    public interface SchemaResolver
    {

        /// <summary>
        /// Given a target namespace and a set of location hints, return a set of schema documents.
        /// </summary>
        /// <param name="targetNamespace">The target namespace of the required schema components</param>
        /// <param name="baseUri">The base URI of the module containing the reference to a schema document
        /// declaration</param>
        /// <param name="locationHints">The sequence of URIs (if any) listed as location hints.
        /// In most cases there will only be one; but the <c>import schema</c> declaration in
        /// XQuery permits several.</param>
        /// <returns>A set of absolute URIs identifying the schema documents to be loaded. There is no requirement
        /// that these correspond one-to-one with the URIs defined in the <c>locationHints</c>. The 
        /// returned URIs will be dereferenced by calling the <c>GetEntity</c> method.
        /// </returns>

        /**public**/ Uri[] GetSchemaDocuments(String targetNamespace, Uri baseUri, String[] locationHints);

        /// <summary>
        /// Dereference a URI returned by <c>GetSchemaDocuments</c> to retrieve a <c>Stream</c> containing
        /// the actual XML schema document.
        /// </summary>
        /// <param name="absoluteUri">A URI returned by the <code>GetSchemaDocuments</code> method.</param>
        /// <returns>Either a <c>Stream</c> or a <c>String</c> containing the schema text. 
        /// The supplied URI will be used as the base URI of the schema document.</returns>

        /**public**/ Object GetEntity(Uri absoluteUri);

    }
}