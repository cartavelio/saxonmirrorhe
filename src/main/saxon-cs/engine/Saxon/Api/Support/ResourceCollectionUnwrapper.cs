using System.Collections.Generic;
using net.sf.saxon.expr;
using Saxon.Impl.Overrides;

namespace Saxon.Api.Support {
    public class ResourceCollectionUnwrapper : net.sf.saxon.lib.ResourceCollection {

        private readonly Saxon.Api.IResourceCollection resources;

        private ResourceCollectionUnwrapper(Saxon.Api.IResourceCollection resources) {
            this.resources = resources;
        }

        public static net.sf.saxon.lib.ResourceCollection Unwrap(Saxon.Api.IResourceCollection resources) {
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

        public IEnumerator<net.sf.saxon.lib.Resource> getResources(XPathContext context) {
            return new MappingEnumerator<IResource, net.sf.saxon.lib.Resource>(
                resources.GetResources(new DynamicContext(context)),
                r=> Saxon.Api.Support.ResourceUnwrapper.Unwrap(r));
        }

        public bool isStable(XPathContext context) {
            return false;
        }
        
    }
}