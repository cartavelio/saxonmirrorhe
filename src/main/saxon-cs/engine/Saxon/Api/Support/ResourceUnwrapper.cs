using net.sf.saxon.expr;
using net.sf.saxon.om;

namespace Saxon.Api.Support {
    public class ResourceUnwrapper : net.sf.saxon.lib.Resource {

        private readonly IResource resource;
        private ResourceUnwrapper(IResource resource) {
            this.resource = resource;
        }

        public static net.sf.saxon.lib.Resource Unwrap(IResource resource) {
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