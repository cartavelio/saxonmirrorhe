namespace Saxon.Api {
    /// <summary>
    /// Resource Wrapper class for a Java Resource object
    /// </summary>
    public class Resource : IResource
    {
        internal net.sf.saxon.lib.Resource resource = null;

        public Resource() { resource = null; }

        public Resource(net.sf.saxon.lib.Resource rc) {
            resource = rc;
        }

        public string GetContentType()
        {
            return resource.getContentType();
        }

        public string GetResourceURI()
        {
            return resource.getResourceURI();
        }

        public XdmItem GetXdmItem(DynamicContext context)
        {
            return XdmItem.FromGroundedValue(resource.getItem(context.context).materialize()).ItemAt(0);
        }
    }
}