using System;
using System.Security.AccessControl;
using Saxon.Hej.lib;

namespace Saxon.Callbacks {
    
    /// <summary>
    /// An implementation of the .NET <c>System.Xml.XmlResolver</c> interface that wraps an instance of
    /// <c>Saxon.Hej.lib.ResourceResolver</c>.
    /// </summary>
    internal class XmlResolverWrappingResourceResolver : System.Xml.XmlResolver {

        private readonly Saxon.Hej.lib.JJ_ResourceResolver resolver;

        public XmlResolverWrappingResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            this.resolver = resolver;
        }
        
        public override object GetEntity(System.Uri absoluteUri, string role, System.Type ofObjectToReturn) {
            if (resolver is CatalogResourceResolver crr) {
                return crr.GetEntityResolver().GetEntity(absoluteUri, role, ofObjectToReturn);
            }

            return null; // TODO: implement me
        }




        /// <summary>
        /// Resolve a relative URI against a base URI
        /// </summary>
        /// <param name="baseUri">The base URI</param>
        /// <param name="relativeUri">The relative URI reference</param>
        /// <returns>The resolved URI</returns>

        public override Uri ResolveUri(Uri baseUri, string relativeUri) {
            if (relativeUri.StartsWith("-//")) {
                // then it's probably a Public ID, which the Microsoft parser doesn't seem to understand.
                // We'll pass it back unresolved
                return new Uri(relativeUri, UriKind.Relative);
            }

            if (baseUri == null) {
                return new Uri(relativeUri, UriKind.RelativeOrAbsolute);
            }

            return new Uri(baseUri, relativeUri);

            // Org.XmlResolver.Resolver resolver = _commonResourceResolver.catalogBasedResolver;
            // if (resolver != null) {
            //     result = resolver.ResolveUri(baseUri, relativeUri);
            // }

            // if (result == null && _commonResourceResolver.nextResourceResolver != null) {
            //     result = _commonResourceResolver.nextResourceResolver.GetEntityResolver().ResolveUri(baseUri, relativeUri);
            // }

   
        }
    }

}