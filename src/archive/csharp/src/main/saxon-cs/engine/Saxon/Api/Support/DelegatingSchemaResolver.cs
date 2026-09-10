using System;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej;
using Saxon.Hej.lib;

namespace Saxon.Api.Support {
    internal class DelegatingSchemaResolver : SchemaURIResolver {

        private readonly SchemaResolver resolver;

        public DelegatingSchemaResolver(SchemaResolver resolver) {
            this.resolver = resolver;
        }
        public void setConfiguration(Configuration config) {
            // no action
        }

        public Source[] resolve(string targetNamespace, string baseURI, string[] locations) {
            XdmNode result = resolver(targetNamespace, 
                new Uri(baseURI), 
                locations.Length == 0 ? null : new Uri(locations[0]));
            return new Source[] {result.Implementation.asActiveSource()};
        }
    }

}