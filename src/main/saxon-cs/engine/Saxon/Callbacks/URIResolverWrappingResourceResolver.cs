using System;
using Saxon.Ejavax.xml.transform;

namespace Saxon.Callbacks {
    public class URIResolverWrappingResourceResolver : URIResolver {

        private readonly CommonResourceResolver resourceResolver;

        public URIResolverWrappingResourceResolver(CommonResourceResolver resourceResolver) {
            this.resourceResolver = resourceResolver;
        }

        public Source resolve(string baseUri, string relativeUri) {
            return resourceResolver.DereferenceLocation(new Uri(baseUri), relativeUri, null, null);
        }

    }
}