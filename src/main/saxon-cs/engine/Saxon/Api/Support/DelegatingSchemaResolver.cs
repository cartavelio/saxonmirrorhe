using System;
using System.IO;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej;
using Saxon.Hej.lib;
using DirectResourceResolver = Saxon.Callbacks.DirectResourceResolver;

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
            if (result != null) {
                return new Source[] { ResourceToSource.convert(result) };
            }
            
            // Fallback: first try the location hint
            JJ_ResourceRequest request = new () {
                nature = ResourceRequest.XSD_NATURE,
                baseUri = baseURI
            };
            Source src;
            if (locations.Length > 0) {
                request.uri = locations[0];
                src = config.getResourceResolver().resolve(request);
                if (src != null) {
                    return new Source[] { src };
                }
            }
            if (!String.IsNullOrEmpty(targetNamespace)) {
                request.uri = targetNamespace;
                src = config.getResourceResolver().resolve(request);
                if (src != null) {
                    return new Source[] { src };
                }
            }
            if (locations.Length > 0) {
                request.uri = locations[0];
                src = new Saxon.Callbacks.DirectResourceResolver(config).resolve(request);
                if (src != null) {
                    return new Source[] { src };
                }
            }

            return null;
        }
    }

}