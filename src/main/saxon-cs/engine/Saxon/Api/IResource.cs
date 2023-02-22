using System;

namespace Saxon.Api {
    /// <summary>
    /// This interface defines a Resource. The Resource objects belong to a collection.
    /// It is used to support the fn:collection() and fn:uri-collection() functions.
    /// <para>It is recommended (but is not universally the case) that fetching (and where necessary parsing)
    /// the content of a Resource should be delayed until the <c>getItem</c> method is called. This means
    /// that errors in fetching the resource or parsing its contents may go undetected until the resource
    /// is materialized in this way.</para>
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// Get a URI that identifies this resource
        /// </summary>
        /// <returns>a URI identifying the resource</returns>
        string ResourceURI { get; }

        /// <summary>
        /// Get an XDM Item holding the contents of this resource.
        /// </summary>
        /// <param name="context">the XPath evaluation context</param>
        /// <returns>an item holding the contents of the resource. The type of item will
        /// reflect the type of the resource: a document node for XML resources, a string
        /// for text resources, a map or array for JSON resources, a base64Binary value
        /// for binary resource.May also return null if the resource cannot be materialized
        /// and this is not to be treated as an error.</returns>
        XdmItem GetXdmItem(DynamicContext context);

        /// <summary> 
        /// Get the media type (MIME type) of the resource if known
        /// </summary>
        /// <returns>the media type if known; otherwise null</returns>
        string ContentType { get; }
    }
}