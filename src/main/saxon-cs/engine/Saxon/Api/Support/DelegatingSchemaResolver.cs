using System;
using System.IO;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej;
using Saxon.Hej.lib;

namespace Saxon.Api.Support {
    internal class DelegatingSchemaResolver : SchemaURIResolver {

        private readonly SchemaResolver resolver;
        private Configuration config;

        public DelegatingSchemaResolver(SchemaResolver resolver, Configuration config) {
            this.resolver = resolver;
            this.config = config;
        }
        public void setConfiguration(Configuration config) {
            this.config = config;
        }

        public Source[] resolve(string targetNamespace, string baseURI, string[] locations) {
            IResource result = resolver(targetNamespace, new Uri(baseURI), 
                locations.Length == 0 ? null : new Uri(locations[0]));
            
            // Fallback: first try the location hint
            ResourceRequest request = null;
            if (result == null) {
                request = new ResourceRequest() {
                    Nature = ResourceRequest.XSD_NATURE,
                    BaseUri = new Uri(baseURI)
                };
            }
            if (result == null && locations.Length > 0) {
                request.Uri = new Uri(locations[0]);
                result = config.getCommonResourceResolver().Resolve(request);
            }
            if (request == null && !String.IsNullOrEmpty(targetNamespace)) {
                request.Uri = new Uri(targetNamespace);
                result = config.getCommonResourceResolver().Resolve(request);
            }
            if (result == null && locations.Length > 0) {
                request.Uri = new Uri(locations[0]);
                result = new DirectResourceResolver().Resolve(request);
            }
            if (result != null) {
                return new Source[] { StandardURIResolver.ConvertToActiveSource(result, result.ResourceUri, config) };
            }

            return null;
        }
    }

}