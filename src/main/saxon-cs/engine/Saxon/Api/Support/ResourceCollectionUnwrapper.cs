using System.Collections.Generic;
using Saxon.Hej.expr;
using Saxon.Impl.Overrides;

namespace Saxon.Api.Support {
    public class ResourceCollectionUnwrapper : Saxon.Hej.lib.ResourceCollection {

        private readonly Saxon.Api.IResourceCollection resources;

        private ResourceCollectionUnwrapper(Saxon.Api.IResourceCollection resources) {
            this.resources = resources;
        }

        public static Saxon.Hej.lib.ResourceCollection Unwrap(Saxon.Api.IResourceCollection resources) {
            if (resources is ResourceCollectionWrapper) {
                return ((ResourceCollectionWrapper) resources).Implementation;
            }
            else {
                return new ResourceCollectionUnwrapper(resources);
            }
        }

        public string getCollectionURI() {
            return resources.CollectionURI;
        }

        public IEnumerator<string> getResourceURIs(XPathContext context) {
            return resources.GetResourceURIs(new DynamicContext(context));
        }

        public IEnumerator<Saxon.Hej.lib.Resource> getResources(XPathContext context) {
            return new MappingEnumerator<IResource, Saxon.Hej.lib.Resource>(
                resources.GetResources(new DynamicContext(context)),
                r=> Saxon.Api.Support.ResourceUnwrapper.Unwrap(r));
        }

        public bool isStable(XPathContext context) {
            return false;
        }
        
    }
}