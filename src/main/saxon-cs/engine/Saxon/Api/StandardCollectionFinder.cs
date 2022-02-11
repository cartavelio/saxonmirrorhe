using System;
using net.sf.saxon.lib;
using Saxon.Api.Support;

namespace Saxon.Api {
    /// <summary>
    /// Default implementation of the CollectionFinder interface. 
    /// </summary>
    public class StandardCollectionFinder : ICollectionFinder
    {
        

        //JStandardCollectionFinder collectionFinder;

        public StandardCollectionFinder() {
            throw new NotImplementedException();
            //collectionFinder = new JStandardCollectionFinder();
        }

        internal StandardCollectionFinder(CollectionFinder cf)
        {
            throw new NotImplementedException();
            //collectionFinder = (JStandardCollectionFinder)cf;
        }

        /// <summary>
        /// Locate the collection of  resources corresponding to a collection URI
        /// </summary>
        /// <param name="context">The XPath dynamic evaluation context</param>
        /// <param name="collectionURI">The collection URI: an absolute URI, formed by resolving the argument
        /// supplied to the fn:collection or fn:uri-collection against the static base URI</param>
        /// <returns>a ResourceCollection object representing resources in the collection identified by this collection URI</returns>
        public IResourceCollection FindCollection(DynamicContext context, string collectionURI)
        {
            throw new NotImplementedException();
            //return new ResourceCollection(collectionFinder.findCollection(context.context, collectionURI));
        }

        /// <summary>
        /// The underlying java CollectionFinder object
        /// </summary>
        // public CollectionFinder Implementation {
        //     get {
        //         return collectionFinder;
        //     }
        // }

        /// <summary>
        /// Register a specific URI and bind it to a specific ResourceCollection
        /// </summary>
        /// <param name="collectionURI">collectionURI the collection URI to be registered. Must not be null.</param>
        /// <param name="collection">collection the ResourceCollection to be associated with this URI. Must not be null.</param>
        public void RegisterCollection(String collectionURI, IResourceCollection collection) {
            throw new NotImplementedException();
            //collectionFinder.registerCollection(collectionURI, new ResourceCollectionWrapper(collection));

        }

    }
}