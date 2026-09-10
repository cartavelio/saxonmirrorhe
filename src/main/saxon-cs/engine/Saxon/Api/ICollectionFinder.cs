using System;

namespace Saxon.Api {
    /// <summary>
    ///  An instance of CollectionFinder can be registered with the Saxon configuration; it is called in response
    ///  to calls on the fn:collection() and fn:uri-collection() functions.
    ///  When these functions are called, the <c>FindCollection(XPathContext, String)</c> method is
    ///  called to get a <c>ResourceCollection</c>
    ///  object representing the collection of resources identified by the supplied collection URI.
    /// </summary>
    public interface ICollectionFinder
    {


        /// <summary>
        /// Locate the collection of resources corresponding to a collection URI.
        /// </summary>
        /// <param name="context">The XPath dynamic evaluation context</param>
        /// <param name="collectionURI">The collection URI: an absolute URI, formed by resolving the argument
        /// supplied to the fn:collection or fn:uri-collection against the staticbase URI</param>
        /// <returns>a ResourceCollection object representing the resources in the collection identified
        ///  by this collection URI. Result should not be null.</returns>
        IResourceCollection FindCollection(DynamicContext context, String collectionURI);


    }
}