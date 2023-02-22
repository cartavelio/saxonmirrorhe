using System;

namespace Saxon.Api {
    /// <summary>
    /// The <c>ResourceResolver</c> is a generic callback interface for resolving URIs
    /// referring to resources.
    /// </summary>
    /// <param name="request">Details of the request, including the requested URI
    /// and other relevant information</param>
    /// <returns>An <c>IResource</c> representing the retrieved resource, or null if the
    /// URI is not recognized. Returning null generally results in the system attempting to resolve
    /// the URI using a fallback resolution method. The type of <c>IResource</c> returned
    /// must be consistent with the <c>Nature</c> of the resource request. In particular,
    /// if the request is for a plain text resource, then the returned <c>IResource</c> must be
    /// a <c>TextResource</c>.</returns>
    [Serializable]
    public delegate IResource ResourceResolver(ResourceRequest request);
}