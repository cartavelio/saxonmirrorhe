using Saxon.Hej.expr;
using Saxon.Hej.om;

namespace Saxon.Api.Support {
    public class ResourceUnwrapper : Saxon.Hej.lib.Resource {

        private readonly IResource resource;
        private ResourceUnwrapper(IResource resource) {
            this.resource = resource;
        }

        public static Saxon.Hej.lib.Resource Unwrap(IResource resource) {
            if (resource is ResourceWrapper) {
                return ((ResourceWrapper) resource).Implementation;
            }
            else {
                return new ResourceUnwrapper(resource);
            }
        }

        public string getResourceURI() {
            return resource.ResourceURI;
        }

        public Item getItem(XPathContext context) {
            return (Item)resource.GetXdmItem(new DynamicContext(context)).value;
        }

        public string getContentType() {
            return resource.ContentType;
        }
    }
}