using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using Org.XmlResolver;
using Org.XmlResolver.Features;
using Org.XmlResolver.Utils;
using Saxon.Api;

namespace Saxon.Callbacks {

    /// <summary>
    /// This object provides an interface between Saxon and the <c>Org.XmlResolver</c> catalog-based
    /// resource resolver, which is incorporated into SaxonCS as a dependency. It provides its own
    /// API for resolving URIs, as well as implementing the (highly fragile) .NET XmlResolver interface.
    /// </summary>
    public class CatalogResourceResolver : CommonResourceResolver {
        private readonly Org.XmlResolver.Resolver catalogBasedResolver;

        /// <summary>
        /// Create a CatalogResourceResolver using a default catalog resolver
        /// </summary>
        public CatalogResourceResolver() {
            XmlResolverConfiguration resolverConfig = new(null, new List<string>());
            // Enable searching of the XmlResolverData library, which holds copies of well-known W3C resources such as DTDs
            resolverConfig.SetFeature(ResolverFeature.ASSEMBLY_CATALOGS, "XmlResolverData.dll");
            catalogBasedResolver = new(resolverConfig);
        }
        
        /// <summary>
        /// Create a CatalogResourceResolver using a user-supplied catalog resolver. This allows use of multiple
        /// resolvers using different catalog configurations.
        /// </summary>

        public CatalogResourceResolver(Org.XmlResolver.Resolver resolver) {
            this.catalogBasedResolver = resolver;
        }

        public override CommonResourceResolver NewInstance() {
            CommonResourceResolver newResolver = new CatalogResourceResolver(catalogBasedResolver);
            newResolver.CopyFrom(this);
            return newResolver;
        }

        /// <summary>
        /// Get a stream, and associated base URI, for a resource identified by a URI, supplied typically
        /// as the combination of a relative URI amd base URI
        /// </summary>
        /// <param name="request">Details of the required resource, including its URI</param>
        /// <returns>An <c>IResource</c> containing the content of the resource as a <c>Stream</c> or
        /// <c>TextReader</c>, together with a <c>SystemId</c> that identifies the base URI of the
        /// resource.</returns>
        public override IResource Resolve(ResourceRequest request) {
            if (catalogBasedResolver != null) {
                try {
                    ResolvedResource rr;
                    if (request.UriIsNamespace) {
                        rr = catalogBasedResolver.CatalogResolver.ResolveNamespace(
                            request.Uri.ToString(), null, request.Nature, request.Purpose);
                    }
                    else {
                        string href = request.RelativeUri ?? request.Uri.ToString();
                        Uri baseUri = request.BaseUri ?? request.Uri;
                        rr = catalogBasedResolver.CatalogResolver.ResolveUri(href, baseUri.ToString());
                        
                    }
                    if (rr != null) {
                        //return null;
                        // Uri abs = new Uri(baseUri, href);
                        // Stream stream = UriUtils.GetStream(abs);
                        // return new TextResource(stream, abs);
                    //}
                    //else {
                        return new TextResource(rr.GetInputStream(), rr.GetResolvedUri());
                    }
                }

                catch (WebException) {
                    // Shouldn't be necessary, but the 0.4.0 resolver code is throwing this for a non-existent URI
                }
                catch (System.ArgumentException) {
                    // Thrown for an unknown URI scheme
                }
                catch (System.IO.FileNotFoundException) {
                    // Shouldn't be necessary, but the 0.4.0 resolver code is throwing this for a non-existent file
                }
          
            }

            return nextResourceResolver?.Resolve(request);
        }

        public override void SetFeature(ResolverFeature feature, object value) {
            if (catalogBasedResolver == null) {
                throw new NullReferenceException();
            }

            catalogBasedResolver.GetConfiguration().SetFeature(feature, value);
        }

        public override object GetFeature(ResolverFeature resolverFeature) {
            return catalogBasedResolver?.GetConfiguration().GetFeature(resolverFeature);
        }

        public override XmlResolver GetEntityResolver() {
            return new XmlResolverWrapper(this);
        }


        private class XmlResolverWrapper : System.Xml.XmlResolver {

            private readonly CatalogResourceResolver _commonResourceResolver;

            public XmlResolverWrapper(CatalogResourceResolver resolver) {
                _commonResourceResolver = resolver;
            }


            public override object GetEntity(System.Uri absoluteUri, string role, System.Type ofObjectToReturn) {
                object result = null;
                Org.XmlResolver.Resolver resolver = _commonResourceResolver.catalogBasedResolver;
                if (resolver != null) {
                    CatalogResolver catalogResolver = resolver.CatalogResolver;
                    string arg = absoluteUri.ToString();
                    ResolvedResource rr;
                    if (arg.StartsWith("-//")) {
                        // Assume it's actually a Public ID
                        rr = catalogResolver.ResolveEntity(null, arg, "http://public-id/" + arg, null);
                    }
                    else {
                        rr = catalogResolver.ResolveEntity(null, null, arg, null);
                    }

                    if (rr == null) {
                        return UriUtils.GetStream(absoluteUri);
                    }
                    else {
                        return rr.GetInputStream();
                    }
                }

                if (_commonResourceResolver.nextResourceResolver != null) {
                    result = _commonResourceResolver.nextResourceResolver.GetEntityResolver()
                        .GetEntity(absoluteUri, role, ofObjectToReturn);
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

                Org.XmlResolver.Resolver resolver = _commonResourceResolver.catalogBasedResolver;
                if (resolver != null) {
                    result = resolver.ResolveUri(baseUri, relativeUri);
                }

                if (result == null && _commonResourceResolver.nextResourceResolver != null) {
                    result = _commonResourceResolver.nextResourceResolver.GetEntityResolver().ResolveUri(baseUri, relativeUri);
                }

                return result;
            }
        }

    }
}