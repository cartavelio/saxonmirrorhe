using System;

namespace Saxon.Api {
    /// <summary>
    /// This interface defines a Resource, which might be an external XML file, text file, JSON file, etc.
    /// Resources might belong to a collection, or might be accessed individually by URI.
    /// </summary>
    /// <remarks>
    /// <para>It is recommended (but is not universally the case) that fetching (and where necessary parsing)
    /// the content of an <c>IResource</c> should be delayed until the <c>GetXdmItem</c> method is called. This means
    /// that errors in fetching the resource or parsing its contents may go undetected until the resource
    /// is materialized in this way.</para>
    /// <para>For deterministic behavior and conformance with W3C Recommendations, it is strongly recommended
    /// that repeated calls on <c>GetXdmItem</c> should deliver the same result (if the result is a node,
    /// it should be the same node each time). This can be achieved by caching the result. However, this
    /// can be expensive in use of memory, and Saxon does not attempt to enforce this rule.</para>
    /// </remarks>
    public interface IResource
    {
        /// <summary>
        /// A URI that identifies this resource
        /// </summary>

        /**public**/ Uri ResourceUri { get; }   // Note, this is marked "public" for the benefit of API documentation

        ///  <summary>
        ///  Get an XDM Item holding the contents of this resource.
        ///  </summary>
        ///  <returns>an item holding the contents of the resource. The type of item will
        ///  reflect the type of the resource: a document node for XML resources, a string
        ///  for text resources, a map or array for JSON resources, a base64Binary value
        ///  for binary resource. May also return null if the resource cannot be materialized
        ///  and this is not to be treated as an error.</returns>

        /**public**/ XdmItem GetXdmItem();

        /// <summary> 
        /// The media type (MIME type) of the resource if known (otherwise null)
        /// </summary>
        /**public**/ string ContentType { get; }
    }
}

// Copyright (c) 2021 Saxonica Limited