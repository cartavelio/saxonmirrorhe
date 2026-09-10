using System;
using System.Impl.Helpers;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej;
using Saxon.Hej.om;
using Saxon.Hej.pull;
using Saxon.Hej.trans;

namespace Saxon.Api.Support {
    internal class JResourceResolverWrappingCS : Saxon.Hej.lib.JJ_ResourceResolver {

        private readonly ResourceResolver resolver;
        private readonly Configuration config;

        public JResourceResolverWrappingCS(ResourceResolver resolver, Configuration config) {
            this.resolver = resolver;
            this.config = config;
        }
        
        public Source resolve(Saxon.Hej.lib.JJ_ResourceRequest jRequest) {
            IResource resource = null;
            ResourceRequest request = new() {
                Uri = new Uri(jRequest.uri),
                BaseUri = new Uri(jRequest.baseUri),
                RelativeUri = jRequest.relativeUri,
                Nature = jRequest.nature,
                Purpose = jRequest.purpose,
                UriIsNamespace = jRequest.uriIsNamespace
            };
            resource = resolver.Invoke(request);
            
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
            // if (resource is TextResource text) {
            //     return StandardURIResolver.ConvertToActiveSource(text, text.ResourceUri, config);
            // }
            
            XdmItem item = resource.GetXdmItem();
            if (item is XdmNode node) {
                return new NodeSource(node.Implementation);
            }

            return null;
        }
    }
}