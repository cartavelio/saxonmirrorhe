using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Org.XmlResolver;
using Org.XmlResolver.Features;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.expr.parser;
using Saxon.Hej.lib;

namespace Saxon.Callbacks {
    public class CatalogResourceResolver : CommonResourceResolver {
        private readonly Org.XmlResolver.Resolver resolver;

        public CatalogResourceResolver() {
            resolver = new(new XmlResolverConfiguration(null, new List<string>()));
        }

        public CatalogResourceResolver(Resolver resolver) {
            this.resolver = resolver;
        }
        
        public override CommonResourceResolver NewInstance() {
            CommonResourceResolver newResolver = new CatalogResourceResolver(resolver);
            newResolver.CopyFrom(this);
            return newResolver;
        }
        
 

        public override object GetEntity(System.Uri absoluteUri, string role, System.Type ofObjectToReturn) {
            object result = null;
            if (resolver != null) {
                CatalogResolver catalogResolver = resolver.CatalogResolver;
                string arg = absoluteUri.ToString();
                if (arg.StartsWith("-//")) {
                    // Assume it's actually a Public ID
                    result = catalogResolver.ResolveEntity(null, arg, null, null);
                }
                else {
                    result = catalogResolver.ResolveEntity(null, null, arg, null);
                }
                //result = resolver.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
            if (result == null && nextResourceResolver != null) {
                result = nextResourceResolver.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
            return result;
        }
        
        /// <summary>
        /// Resolve a relative URI against a base URI
        /// </summary>
        /// <param name="baseUri">The base URI</param>
        /// <param name="relativeUri">The relative URI reference</param>
        /// <returns>The resolved URI</returns>
        
        public override Uri ResolveUri(Uri baseUri, string relativeUri) {
            Uri result = null;
            if (relativeUri.StartsWith("-//")) {
                // then it's probably a Public ID, which the Microsoft parser doesn't seem to understand.
                // We'll pass it back unresolved
                return new Uri(relativeUri, UriKind.Relative);
            }
            if (resolver != null) {
                result = resolver.ResolveUri(baseUri, relativeUri);
            }
            if (result == null && nextResourceResolver != null) {
                result = nextResourceResolver.ResolveUri(baseUri, relativeUri);
            }
            return result;
        }

        /// <summary>
        /// Get a stream, and associated base URI, for a resource identified by a URI, supplied typically
        /// as the combination of a relative URI amd base URI
        /// </summary>
        /// <param name="baseUri">The base URI for resolving the relative URI; ignored
        /// if the <c>href</c> argument is an absolute URI.</param>
        /// <param name="nature">Indicates the "nature" of a resource as defined
        /// in RDDL (see www.rddl.org/2 section 3.1), which in turn corresponds to the "role"
        /// property of a URI reference as defined in XLink. Possible values include (a) an XML namespace
        /// URI such as <c>http://www.w3.org/2001/XmlSchema</c>, or (b) a registered media type prefixed by
        /// <c>http://www.isi.edu/in-notes/iana/assignments/media-types/</c>.</param>
        /// <param name="purpose">Indicates the "purpose" of the URI reference as defined in RDDL,
        /// which in turn corresponds to the "arcrole" property of a URI reference as defined in XLink.</param>
        /// <returns>A StreamSource containing the content of the resource as a <c>Stream</c> or
        /// <c>TextReader</c>, together with a <c>SystemId</c> that identifies the base URI of the
        /// resource.</returns>

        public override StreamSource DereferenceLocation(Uri baseUri, string href, string nature, string purpose) {
            if (resolver != null) {
                try {
                    ResolvedResource rr = resolver.CatalogResolver.ResolveUri(href, baseUri.ToString());
                    if (rr != null) {
                        return new StreamSource(rr.GetInputStream(), rr.GetResolvedUri().ToString());
                    }
                }
                catch (WebException webex) {
                    // Shouldn't be necessary, but the 0.4.0 resolver code is throwing this for a non-existent URI
                }
                catch (System.ArgumentException argex) {
                    // Thrown for an unknown URI scheme
                }
            }
            if (nextResourceResolver != null) {
                return nextResourceResolver.DereferenceLocation(baseUri, href, nature, purpose);
            }

            return null;
        }
        
        public override StreamSource DereferenceNamespace(string namespaceUri, string nature, string purpose) {
            if (resolver != null) {
                ResolvedResource rr = resolver.CatalogResolver.ResolveNamespace(namespaceUri, null, nature, purpose);
                if (rr != null) {
                    return new StreamSource(rr.GetInputStream(), rr.GetResolvedUri().ToString());
                }
            }

            if (nextResourceResolver != null) {
                return nextResourceResolver.DereferenceNamespace(namespaceUri, nature, purpose);
            }

            return null;

        }

        public override void SetFeature(ResolverFeature feature, object value) {
            if (resolver == null) {
                throw new NullReferenceException();
            }
            resolver.GetConfiguration().SetFeature(feature, value);
        }

        public override object GetFeature(ResolverFeature resolverFeature) {
            return resolver?.GetConfiguration().GetFeature(resolverFeature);
        }

    }
}