using System;
using System.Impl.Helpers;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej;
using Saxon.Hej.om;
using Saxon.Hej.pull;
using Saxon.Hej.trans;

namespace Saxon.Api.Support {
    public class UriResolverWrappingDocumentResolver : URIResolver {

        private readonly ResourceResolver resolver;
        private readonly Configuration config;

        public UriResolverWrappingDocumentResolver(ResourceResolver resolver, Configuration config) {
            this.resolver = resolver;
            this.config = config;
        }
        public Source resolve(string relativeUri, string baseUri) {
            IResource resource = null;
            ResourceRequest request;
            if (baseUri == null) {
                Uri href;
                try {
                    href = new Uri(relativeUri, UriKind.RelativeOrAbsolute);
                }
                catch (UriFormatException e) {
                    throw new XPathException("Invalid relative URI: " + relativeUri + " (" + e.Message + ")");
                }
                request = new() {
                    Uri = href,
                    RelativeUri = relativeUri
                };
                resource = resolver.Invoke(request);
            }
            else {
                Uri baseUriUri;
                try {
                    baseUriUri = new Uri(baseUri);
                }
                catch (UriFormatException e) {
                    throw new XPathException("Invalid base URI: " + baseUri + " (" + e.Message + ")");
                }
                Uri abs;
                try {
                    abs = new Uri(baseUriUri, (string)relativeUri);
                }
                catch (UriFormatException e) {
                    throw new XPathException("Unable to resolve relative URI (" + relativeUri + ") against base URI (" + baseUri + "): " + e.Message);
                }
                request = new() {
                    Uri = abs,
                    BaseUri = baseUriUri,
                    RelativeUri = relativeUri
                };
                resource = resolver.Invoke(request);
            }

            if (resource is null) {
                resource = config.getCommonResourceResolver().chain(new DirectResourceResolver()).Resolve(request);
            }

            if (resource is XmlReaderResource readerResource) {
                // For streaming, we mustn't do the parsing yet
                PullProvider pull = new DotNetPullProvider(readerResource.XmlReader);
                PullSource source = new PullSource(pull);
                source.setSystemId(readerResource.ResourceUri.ToString());
                return source;
            }
            if (resource is TextResource text) {
                return StandardURIResolver.ConvertToActiveSource(text, text.ResourceUri, config);
            }
            else {
                XdmNode node = (XdmNode)resource.GetXdmItem();
                return new NodeSource(node.Implementation);
            }
        }
    }
}