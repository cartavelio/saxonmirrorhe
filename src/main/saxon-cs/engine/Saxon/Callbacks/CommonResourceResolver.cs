using System;
using Org.XmlResolver;
using Org.XmlResolver.Features;
using Saxon.Api;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.lib;

namespace Saxon.Callbacks {
    public abstract class CommonResourceResolver : System.Xml.XmlResolver {
        protected CommonResourceResolver nextResourceResolver = null;

        public abstract CommonResourceResolver NewInstance();

        internal void CopyFrom(CommonResourceResolver resolver) {
            nextResourceResolver = resolver.nextResourceResolver;
        }
        
        /// <summary>
        /// Add a fallback resolver to be used if the primary resolver fails to locate the resource.
        /// </summary>
        /// <param name="resolver">A second resolver that will be invoked as a fallback when
        /// this resolver cannot resolve the requested URI</param>
        /// <returns></returns>

        public virtual CommonResourceResolver chain(CommonResourceResolver resolver) {
            nextResourceResolver = resolver;
            return this;
        }

        /// <summary>
        /// Get a resource identified by a URI, supplied typically as the combination of a relative URI and base URI.
        /// This method is used for URIs that typically represent locations (for example URIs passed to the <c>fn:doc</c>
        /// function) as distinct from namespace URIs.
        /// </summary>
        /// <param name="request">Details of the required resource, including its URI</param>
        /// <returns>An IResource containing the content of the resource, together with its base URI. It is
        /// always acceptable to return a <c>TextResource</c>; other kinds of resource may be acceptable
        /// depending on the nature of the request.</returns>
        public abstract IResource DereferenceLocation(ResourceRequest request);

        /// <summary>
        /// Get a resource identified by a URI.
        /// This method is used for URIs that typically represent namespaces, for example the namespace of a query module
        /// or the target namespace of a schema, as distinct from URIs that normally correspond to addresses.
        /// </summary>
        /// <param name="namespaceUri">The URI of the required resource</param>
        /// <param name="nature">As defined in RDDL (corresponds to the <c>role</c> attribute in XLink)</param>
        /// <param name="purpose">As defined in RDDL (corresponds to the <c>arcrole</c> attribute in XLink)</param>
        /// <returns>An IResource containing the content of the resource, together with its base URI. It is
        /// always acceptable to return a <c>TextResource</c>; other kinds of resource may be acceptable
        /// depending on the nature of the request.</returns>
        public abstract IResource DereferenceNamespace(string namespaceUri, ResourceRequest request);

        /// <summary>
        /// Set a feature of the underlying implementation (geared towards the case where the underlying
        /// implementation is a catalog resolver)
        /// </summary>
        /// <param name="feature">An object that identifies the feature</param>
        /// <param name="value">The value to be set for the feature</param>
        public virtual void SetFeature(ResolverFeature feature, object value) {}

        /// <summary>
        /// Get a feature of the underlying implementation (geared towards the case where the underlying
        /// implementation is a catalog resolver)
        /// </summary>
        /// <param name="feature">An object that identifies the feature</param>
        /// <returns>The current value of the feature</returns>
        public virtual object GetFeature(ResolverFeature feature) {
            return null;
        }

        
    }
}