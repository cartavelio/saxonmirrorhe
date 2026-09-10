using System;

namespace Saxon.Api {
    /// <summary>
    /// The <c>ResourceResolver</c> is a generic callback interface for resolving URIs
    /// referring to resources.
    /// </summary>
    /// <param name="request">Details of the request, including the requested URI
    /// and other relevant information</param>
    /// <returns>An <c>IResource</c> representing the retrieved resource, or null if the
    /// URI is not recognized.</returns>
    [Serializable]
    public delegate IResource ResourceResolver(ResourceRequest request);
}