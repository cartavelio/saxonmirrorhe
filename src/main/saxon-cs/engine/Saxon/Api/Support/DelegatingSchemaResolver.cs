using System;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej;
using Saxon.Hej.lib;

namespace Saxon.Api.Support {
    internal class DelegatingSchemaResolver : SchemaURIResolver {

        private readonly SchemaResolver resolver;
        private Configuration config;

        public DelegatingSchemaResolver(SchemaResolver resolver) {
            this.resolver = resolver;
        }
        public void setConfiguration(Configuration config) {
            this.config = config;
        }

        public Source[] resolve(string targetNamespace, string baseURI, string[] locations) {
            IResource result = resolver(targetNamespace, new Uri(baseURI), 
                locations.Length == 0 ? null : new Uri(locations[0]));
            return new Source[] {StandardURIResolver.ConvertToActiveSource(result, result.ResourceUri, config)};
        }
    }

}