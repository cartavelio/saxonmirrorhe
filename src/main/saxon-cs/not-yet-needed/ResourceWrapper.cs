using net.sf.saxon.expr;
using net.sf.saxon.om;

namespace Saxon.Api {
    internal class ResourceWrapper : net.sf.saxon.lib.Resource
    {

        IResource resource;

        public ResourceWrapper(IResource resource) {
            this.resource = resource;
        }
        public string getContentType()
        {
            return resource.GetContentType();
        }

        public Item getItem(XPathContext xpc)
        {
            return (Item) resource.GetXdmItem(new DynamicContext(xpc)).Unwrap();
        }

        public string getResourceURI()
        {
            return resource.GetResourceURI();
        }
    }
}