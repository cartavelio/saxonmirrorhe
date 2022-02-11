using System;
using System.Xml;
using Org.XmlResolver.Features;
using Saxon.Api;


namespace Saxon.Callbacks {
    public abstract class CommonResourceResolver {
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
        /// </summary>
        /// <remarks>
        /// <para>Note that this method has the same signature as the <c>ResourceResolver</c> delegate, so
        /// where a <c>ResourceResolver</c> is required, it is possible to supply <c>commonResourceResolver.Resolve</c>
        /// as the value.</para>
        /// <para>This method is used for URIs that typically represent locations (for example URIs passed to the <c>fn:doc</c>
        /// function). It is also used to resolve namespaces appearing in XQuery <c>import module</c> and <c>import schema</c>
        /// declarations (and other constructs for importing schema documents) where no location hint is provided: in this
        /// case the property <c>request.UriIsNamespace</c> will be set to true.</para>
        /// </remarks>
        /// <param name="request">Details of the required resource, including its URI</param>
        /// <returns>An IResource containing the content of the resource, together with its base URI. It is
        /// always acceptable to return a <c>TextResource</c>; other kinds of resource may be acceptable
        /// depending on the nature of the request. Returns null if the request cannot be resolved.</returns>
        public abstract IResource Resolve(ResourceRequest request);

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
        
        /// <summary>
        /// Get an instance of <c>System.Xml.XmlResolver</c> based on this <c>CommonResourceResolver</c>,
        /// which can be passed to an XML parser (<c>XmlReader</c>) to be used for resolving external entities
        /// within an XML document (including the external DTD subset).
        /// </summary>
        /// <returns>An <c>System.Xml.XmlResolver</c> based on this <c>CommonResourceResolver</c></returns>

        
        public virtual XmlResolver GetEntityResolver() {
            return new DelegatingXmlResolver(this);
        }

        private class DelegatingXmlResolver : XmlUrlResolver {

            private readonly CommonResourceResolver commonResolver;

            public DelegatingXmlResolver(CommonResourceResolver resolver) {
                commonResolver = resolver;
            }
            
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
                object o = base.GetEntity(absoluteUri, role, ofObjectToReturn);
                return o ?? commonResolver.nextResourceResolver.GetEntityResolver()
                    .GetEntity(absoluteUri, role, ofObjectToReturn);
            }
            
        }
    }
}