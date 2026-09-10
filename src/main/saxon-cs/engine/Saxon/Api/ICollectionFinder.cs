using System;

namespace Saxon.Api {
    /// <summary>
    ///  An CollectionFinder can be registered with the <c>Processor<c/>; it is called in response
    ///  to calls on the fn:collection() and fn:uri-collection() functions.
    ///  When these functions are called, the delegate is called to get a <c>ResourceCollection</c>
    ///  object representing the collection of resources identified by the supplied collection URI.
    /// </summary>
    public delegate IResourceCollection ICollectionFinder(DynamicContext context, string collectionURI);
    
}