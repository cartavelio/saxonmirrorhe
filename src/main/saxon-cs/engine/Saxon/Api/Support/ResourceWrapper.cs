namespace Saxon.Api.Support {
    public class ResourceWrapper : IResource {

        private readonly net.sf.saxon.lib.Resource resource;
        public ResourceWrapper(net.sf.saxon.lib.Resource resource) {
            this.resource = resource;
        }

        public string ResourceURI => resource.getResourceURI();

        public XdmItem GetXdmItem(DynamicContext context) {
            return XdmItem.WrapItem(resource.getItem(context.context));
        }

        public string ContentType => resource.getContentType();

        public net.sf.saxon.lib.Resource Implementation => resource;
    }
}