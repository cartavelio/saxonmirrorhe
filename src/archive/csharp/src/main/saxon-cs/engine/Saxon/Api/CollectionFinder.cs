using System;
using System.Collections.Generic;

namespace Saxon.Api {
    /// <summary>
    ///  An <c>CollectionFinder</c> can be registered with the <c>Processor</c>; it is called in response
    ///  to calls on the <c>fn:collection()</c> and <c>fn:uri-collection()</c> functions.
    ///  When these functions are called, the delegate is called to get a collection of <c>IResource</c>
    ///  objects representing resources identified by the supplied collection URI.
    /// </summary>
    /// <param name="collectionUri">The URI that will be used to identify the collection in calls on
    /// the <c>fn:collection()</c> and <c>fn:uri-collection()</c> functions. Should be an absolute URI.</param>
    /// <returns>The set of resources represented by the collection URI</returns>
    /// <remarks>
    /// The implementation may be lazy: that is, the returned resource collection might not contain
    /// the actual resources, but only the information needed to fetch the resources when required.
    /// This means that any errors that occur when actually fetching the resources may be deferred.
    /// </remarks>
    public delegate IEnumerable<IResource> CollectionFinder(Uri collectionUri);
    
}

// Copyright (c) 2021 Saxonica Limited