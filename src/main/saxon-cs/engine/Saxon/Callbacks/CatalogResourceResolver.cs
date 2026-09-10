using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using XmlResolver;
using XmlResolver.Features;
using XmlResolver.Utils;
using Saxon.Api;
using Saxon.Ejava.io;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.resource;
using Saxon.Hej.trans;
using ResourceRequest = XmlResolver.ResourceRequest;

#nullable enable 

namespace Saxon.Callbacks {

    /// <summary>
    /// This object provides an interface between Saxon and the <c>XmlResolver</c> catalog-based
    /// resource resolver, which is incorporated into SaxonCS as a dependency. It provides its own
    /// API for resolving URIs, as well as implementing the (highly fragile) .NET XmlResolver interface.
    /// </summary>
    public class CatalogResourceResolver : Saxon.Hej.lib.JJ_ResourceResolver, Saxon.Callbacks.ConfigurableResourceResolver {
        private readonly XmlResolver.XmlResolver? catalogBasedResolver;

        /// <summary>
        /// Create a CatalogResourceResolver using a default catalog resolver
        /// </summary>
        public CatalogResourceResolver() {
            XmlResolverConfiguration resolverConfig = new(null, new List<string>());
            // Enable searching of the XmlResolverData library, which holds copies of well-known W3C resources such as DTDs
            resolverConfig.SetFeature(ResolverFeature.ASSEMBLY_CATALOGS, "XmlResolverData.dll");
            //catalogBasedResolver.GetConfiguration().SetFeature(ResolverFeature.THROW_URI_EXCEPTIONS, true);
            catalogBasedResolver = new XmlResolver.XmlResolver(resolverConfig);
        }
        
        /// <summary>
        /// Create a CatalogResourceResolver using a user-supplied catalog resolver. This allows use of multiple
        /// resolvers using different catalog configurations.
        /// </summary>

        public CatalogResourceResolver(XmlResolver.XmlResolver resolver) {
            catalogBasedResolver = resolver;
        }

        /// <summary>
        /// Get a stream, and associated base URI, for a resource identified by a URI, supplied typically
        /// as the combination of a relative URI amd base URI
        /// </summary>
        /// <param name="request">Details of the required resource, including its URI</param>
        /// <returns>An <c>IResource</c> containing the content of the resource as a <c>Stream</c> or
        /// <c>TextReader</c>, together with a <c>SystemId</c> that identifies the base URI of the
        /// resource.</returns>
        public virtual Saxon.Ejavax.xml.transform.Source resolve(Saxon.Hej.lib.JJ_ResourceRequest request) {
            if (catalogBasedResolver != null) {
                try
                {
                    ResourceRequest? req = null;
                    if (request.uriIsNamespace) {
                        IResolverConfiguration catConfig = catalogBasedResolver.Config;
                        CatalogManager catalog = (CatalogManager) catConfig.GetFeature(ResolverFeature.CATALOG_MANAGER);
                        Uri attempt = catalog.LookupNamespaceUri(request.uri, request.nature, request.purpose);
                        if (attempt == null) {
                            return null;
                        }

                        req = catalogBasedResolver.GetRequest(request.uri, null, request.nature, request.purpose);
                    }
                    else if (request.nature == Saxon.Hej.lib.JJ_ResourceRequest.DTD_NATURE || request.nature == Saxon.Hej.lib.JJ_ResourceRequest.EXTERNAL_ENTITY_NATURE) {
                        string href = request.relativeUri ?? request.uri;
                        string baseUri = request.baseUri ?? request.uri;

                        req = catalogBasedResolver.GetRequest(href, baseUri, request.nature, request.purpose);
                        req.EntityName = request.entityName;
                        req.PublicId = request.publicId;
                    }
                    else {
                        string href = request.relativeUri ?? request.uri;
                        string baseUri = request.baseUri;
                        if (string.IsNullOrEmpty(baseUri)) {
                            baseUri = Saxon.Ejava.io.File.CurrentDirectoryUri().AbsoluteUri;
                        }

                        req = catalogBasedResolver.GetRequest(href, baseUri, ResolverConstants.ANY_NATURE,
                            ResolverConstants.ANY_PURPOSE);
                    }

                    ResourceResponse? rr = catalogBasedResolver.Resolve(req);
                    if (rr.IsResolved && rr.ResolvedUri != null) {
                        TypedStreamSource result = new TypedStreamSource();
                        result.setSystemId(rr.ResolvedUri.ToString());
                        if (rr.Stream != null)
                        {
                            result.setInputStream(rr.Stream);
                        }
                        if (rr.ContentType != null)
                        {
                            result.setContentType(rr.ContentType);
                        }
                        return result;
                    }
                }

                catch (WebException) {
                    // Shouldn't be necessary, but the 0.4.0 resolver code is throwing this for a non-existent URI
                }
                catch (System.ArgumentException) {
                    // Thrown for an unknown URI scheme
                }
                catch (System.UriFormatException u) {
                    // Thrown for an invalid URI
                    throw new XPathException("Invalid URI: ", u);
                }
                catch (System.IO.FileNotFoundException) {
                    // Shouldn't be necessary, but the 0.4.0 resolver code is throwing this for a non-existent file
                }
            }

            return null;
        }

        public virtual void setFeature(ResolverFeature feature, object value) {
            if (catalogBasedResolver == null) {
                throw new NullReferenceException();
            }

            catalogBasedResolver.Config.SetFeature(feature, value);
        }

        public virtual object getFeature(ResolverFeature resolverFeature) {
            return catalogBasedResolver?.Config.GetFeature(resolverFeature)!;
        }

        public virtual XmlResolver.XmlResolver GetEntityResolver() {
            return catalogBasedResolver;
        }
    }


}
