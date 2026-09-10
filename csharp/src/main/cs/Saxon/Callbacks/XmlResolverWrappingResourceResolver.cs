using System;
using System.Security.AccessControl;
using Saxon.Hej.lib;
using XmlResolver;

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
            try
            {
                if (resolver is CatalogResourceResolver crr)
                {
                    Object resp = crr!.GetEntityResolver()!.GetXmlResolver().GetEntity(absoluteUri, role, ofObjectToReturn);
                    return resp;
                }
            }
            catch (UriFormatException ex)
            {
                // If the parser encounters an entity with a public identifier, it calls GetEntity using
                // the public identifier as a URI. No, really. I sh*t you not. Somehow, that Uri gets
                // created by the processor and passed to us, but when we try to pass it to our 
                // implementation of GetEntity, System.Uri throws a UriFormatException. All we can do
                // is ignore it. There has to be a system identifier, and the parser will try again with
                // the actual URI. If this arises in some *other* way, well, resolution will fail, and
                // that's about all we can do.
                return null;
            }

            return null;
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

            // XmlResolver.Resolver resolver = _commonResourceResolver.catalogBasedResolver;
            // if (resolver != null) {
            //     result = resolver.ResolveUri(baseUri, relativeUri);
            // }

            // if (result == null && _commonResourceResolver.nextResourceResolver != null) {
            //     result = _commonResourceResolver.nextResourceResolver.GetEntityResolver().ResolveUri(baseUri, relativeUri);
            // }

   
        }
    }

}
