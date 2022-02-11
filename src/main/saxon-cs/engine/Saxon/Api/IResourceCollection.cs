using System;
using System.Collections.Generic;

namespace Saxon.Api {
    /// <summary>
    /// This interface defines a ResourceCollection. This class
    /// is used to map the URI of collection into a sequence of Resource objects.
    /// It is used to support the fn:collection() and fn:uri-collection() functions.
    /// </summary>
    public interface IResourceCollection
    {
        /// <summary>
        /// Get the URI of the collection
        /// </summary>
        /// <returns> The URI as passed to the fn:collection() or fn:uri-collection()
        /// function, resolved if it is relative against the static base URI.
        /// If the collection() or uri-collection() function
        /// was called with no arguments(to get the "default collection") this
        /// will be the URI of the default collection registered with the Configuration.</returns>
        String CollectionURI();

        /// <summary>
        /// Get the URIs of the resources in the collection. This supports the fn:uri-collection()
        /// function.It is not required that all collections expose a list of URIs in this way, or
        /// that the URIs bear any particular relationship to the resources returned by the
        /// getResources() method for the same collection URI.The URIs that are returned should be
        /// suitable for passing to the registered URIResolver (in the case of XML resources),
        /// or the { @link UnparsedTextURIResolver }
        /// (in the case of unparsed text and JSON resources), etc.
        /// </summary>
        /// <param name="context">context the XPath evaluation context</param>
        /// <returns>an iterator over the URIs of the resources in the collection. The URIs are represented
        /// as Strings.They should preferably be absolute URIs.</returns>
        List<String> GetResourceURIs(DynamicContext context);

        /// <summary>
        /// Get the resources in the collection. This supports the fn:collection() function. It is not
        /// required that all collections expose a set of resources in this way, or that the resources
        /// returned bear any particular relationship to the URIs returned by the getResourceURIs() method
        /// for the same collection URI.
        /// </summary>
        /// <param name="context">the XPath evaluation context</param>
        /// <returns>a List over the resources in the collection. This returns objects of class
        /// <c>Riesource</c>.</returns>
        List<IResource> GetResources(DynamicContext context);


    }
}