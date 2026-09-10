using System;
using System.Impl.Helpers;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej;
using Saxon.Hej.om;
using Saxon.Hej.pull;

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
            if (baseUri == null) {
                Uri href = new Uri(relativeUri, UriKind.RelativeOrAbsolute);
                ResourceRequest request = new() {
                    Uri = href,
                    RelativeUri = relativeUri
                };
                resource = resolver.Invoke(request);
            }
            else {
                Uri baseUriUri = new Uri(baseUri);
                Uri abs = new Uri(baseUriUri, (string)relativeUri);
                ResourceRequest request = new() {
                    Uri = abs,
                    BaseUri = baseUriUri,
                    RelativeUri = relativeUri
                };
                resource = resolver.Invoke(request);
            }

            if (resource is null) {
                return null;
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