namespace Saxon.Api.Support {
    public class ResourceWrapper : IResource {

        private readonly Saxon.Hej.lib.Resource resource;
        public ResourceWrapper(Saxon.Hej.lib.Resource resource) {
            this.resource = resource;
        }

        public string ResourceURI => resource.getResourceURI();

        public XdmItem GetXdmItem(DynamicContext context) {
            return XdmItem.WrapItem(resource.getItem(context.context));
        }

        public string ContentType => resource.getContentType();

        public Saxon.Hej.lib.Resource Implementation => resource;
    }
}