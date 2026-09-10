using System;
using Org.XmlResolver.Features;
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

        public virtual CommonResourceResolver chain(CommonResourceResolver resolver) {
            nextResourceResolver = resolver;
            return this;
        }
        
        /// <summary>
        /// Get a stream, and associated base URI, for a resource identified by a URI, supplied typically
        /// as the combination of a relative URI amd base URI
        /// </summary>
        /// <param name="baseUri">The base URI for resolving the relative URI; ignored
        /// if the <c>href</c> argument is an absolute URI.</param>
        /// <param name="nature"></param>
        /// <param name="purpose"></param>
        /// <returns>A StreamSource containing the content of the resource as a <c>Stream</c> or
        /// <c>TextReader</c>, together with a <c>SystemId</c> that identifies the base URI of the
        /// resource.</returns>
       
        public abstract StreamSource DereferenceLocation(Uri baseUri, string href, string nature, string purpose);
        
        /// <summary>
        /// Get a stream, and associated base URI, for a resource identified only by namespace, for example
        /// a schema document with a given target namespace, or an XQuery library module for a given module
        /// namespace
        /// </summary>
        /// <param name="namespaceUri">The URI of the required resource</param>
        /// <param name="nature"></param>
        /// <param name="purpose"></param>
        /// <returns>A StreamSource containing the content of the resource as a <c>Stream</c> or
        /// <c>TextReader</c>, together with a <c>SystemId</c> that identifies the base URI of the
        /// resource.</returns>

        public abstract StreamSource DereferenceNamespace(string namespaceUri, string nature, string purpose);

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