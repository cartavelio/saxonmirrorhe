using System;
using System.Impl.Helpers;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej.om;
using Saxon.Hej.pull;

namespace Saxon.Api.Support {
    public class UriResolverWrappingDocumentResolver : URIResolver {

        private readonly ResourceResolver resolver;

        public UriResolverWrappingDocumentResolver(ResourceResolver resolver) {
            this.resolver = resolver;
        }
        public Source resolve(string relativeUri, string baseUri) {
            Uri baseUriUri = new Uri(baseUri);
            Uri abs = new Uri(baseUriUri, (string)relativeUri);
            ResourceProperties properties = new() {
                BaseUri = baseUriUri,
                RelativeUri = relativeUri
            };
            IResource resource = resolver.Invoke(abs, properties);
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
                string content = text.Content;
                // TODO...
                return null;
            }
            else {
                XdmNode node = (XdmNode)resource.GetXdmItem();
                return new NodeSource(node.Implementation);
            }
        }
    }
}