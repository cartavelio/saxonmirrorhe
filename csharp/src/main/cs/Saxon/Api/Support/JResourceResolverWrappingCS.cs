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
            
            // Make the uri absolute with the base URI, if we can.
            // C#'s Uri class won't make a relative URI without a scheme
            var rbase = string.IsNullOrEmpty(jRequest.baseUri) ? null : new Uri(jRequest.baseUri);
            Uri ruri = null;
            if (jRequest.uri != null)
            {
                if (rbase == null)
                {
                    ruri = new Uri(jRequest.uri); // Fingers crossed!
                }
                else
                {
                    ruri = new Uri(rbase, jRequest.uri);
                
                }
            }
            
            ResourceRequest request = new() {
                Uri = ruri,
                BaseUri = rbase,
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
            
            return ResourceToSource.convert(resource);

        }
    }
}
