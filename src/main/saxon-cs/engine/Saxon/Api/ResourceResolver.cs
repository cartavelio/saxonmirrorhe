using System;

namespace Saxon.Api {
    /// <summary>
    /// The <c>ResourceResolver</c> is a generic callback interface for resolving URIs
    /// referring to resources.
    /// </summary>
    /// <param name="requestedUri">The requested URI. This may be absolute or relative. If it is
    /// relative, the <c>ResourceProperties</c> should include a BaseUri that can be used
    /// to resolve the relative reference</param>
    /// <returns>An <c>IResource</c> representing the retrieved resource, or null if the
    /// URI is not recognized.</returns>
    [Serializable]
    public delegate IResource ResourceResolver(Uri requestedUri, ResourceProperties properties);
}