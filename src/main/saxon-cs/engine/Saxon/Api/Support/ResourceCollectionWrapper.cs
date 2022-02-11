using System.Collections.Generic;

namespace Saxon.Api.Support {
    public class ResourceCollectionWrapper : IResourceCollection {

        private readonly Saxon.Hej.lib.ResourceCollection resources;

        public ResourceCollectionWrapper(Saxon.Hej.lib.ResourceCollection resources) {
            this.resources = resources;
        }

        public string CollectionURI => resources.getCollectionURI();
        
        public IEnumerator<string> GetResourceURIs(DynamicContext context) {
            return resources.getResourceURIs(context.context);
        }

        public IEnumerator<IResource> GetResources(DynamicContext context) {
            IEnumerator<Saxon.Hej.lib.Resource> enumerator = resources.getResources(context.context);
            while (enumerator.MoveNext()) {
                yield return new ResourceWrapper(enumerator.Current);
            }
        }

        public Saxon.Hej.lib.ResourceCollection Implementation => resources;

    }
}