using System.Collections.Generic;

namespace Saxon.Api.Support {
    public class ResourceCollectionWrapper : IResourceCollection {

        private readonly net.sf.saxon.lib.ResourceCollection resources;

        public ResourceCollectionWrapper(net.sf.saxon.lib.ResourceCollection resources) {
            this.resources = resources;
        }

        public string CollectionURI => resources.getCollectionURI();
        
        public IEnumerator<string> GetResourceURIs(DynamicContext context) {
            return resources.getResourceURIs(context.context);
        }

        public IEnumerator<IResource> GetResources(DynamicContext context) {
            IEnumerator<net.sf.saxon.lib.Resource> enumerator = resources.getResources(context.context);
            while (enumerator.MoveNext()) {
                yield return new ResourceWrapper(enumerator.Current);
            }
        }

        public net.sf.saxon.lib.ResourceCollection Implementation => resources;

    }
}