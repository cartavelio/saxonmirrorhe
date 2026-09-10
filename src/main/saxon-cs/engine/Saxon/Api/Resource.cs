using Saxon.Hej.ma.arrays;
using Saxon.Hej.ma.map;
using Saxon.Hej.om;
using Saxon.Hej.resource;

namespace Saxon.Api {
    /// <summary>
    /// A concrete implementation of the IResource interface that wraps an internal resource representation;
    /// also provides static methods for constructing various types of resource.
    /// </summary>
    public class Resource : IResource
    {
        private readonly Saxon.Hej.lib.Resource resource;
        
        /// <summary>
        /// Construct a <c>Resource</c> by wrapping an internal representation of the resource
        /// </summary>
        /// <param name="rc">The wrapped resource</param>
        private Resource(Saxon.Hej.lib.Resource rc) {
            resource = rc;
        }

        /// <summary>
        /// Construct an XML resource, supplying a URI that can be dereferenced to yield the
        /// lexical XML content
        /// </summary>
        /// <param name="processor">The Saxon Processor</param>
        /// <param name="uri">The URI location of the resource</param>
        /// <returns>A newly constructed XML resource</returns>
        public static Resource XmlResource(Processor processor, string uri) {
            AbstractResourceCollection.InputDetails details = new();
            details.contentType = "application/xml";
            details.resourceUri = uri;
            return new Resource(new XmlResource(processor.Implementation, details));
        }
        
        /// <summary>
        /// Construct an XML resource, supplying the root node of the XML tree
        /// </summary>
        /// <param name="uri">The resource URI (system ID)</param>
        /// <param name="node">The root node</param>
        /// <returns>A newly constructed XML resource</returns>
        public static Resource XmlResource(string uri, XdmNode node) {
            NodeInfo nodeInfo = node.Implementation;
            return new(new XmlResource(nodeInfo.getConfiguration(), nodeInfo));
        }
        
        /// <summary>
        /// Construct a JSON resource, supplying a URI that can be dereferenced to obtain the
        /// lexical JSON content
        /// </summary>
        /// <param name="uri">The URI location of the JSON content</param>
        /// <returns>A newly constructed JSON resource</returns>
        /// 
        public static Resource JsonResource(string uri) {
            AbstractResourceCollection.InputDetails details = new();
            details.contentType = "application/json";
            details.resourceUri = uri;
            return new(new JSONResource(details));
        }
        
        /// <summary>
        /// Construct a JSON resource, supplying the parsed JSON in the form of an XDM map representing
        /// the root JSON object
        /// </summary>
        /// <param name="uri">The URI used to identify the JSON resource</param>
        /// <param name="map">The content of the resource as a map</param>
        /// <returns>A newly constructed JSON resource</returns>
        
        public static Resource JsonResource(string uri, XdmMap map) {
            return new(new JSONResource(uri, (MapItem)map.value));
        }
        
        /// <summary>
        /// Construct a JSON resource, supplying the parsed JSON in the form of an XDM array representing
        /// the root JSON array
        /// </summary>
        /// <param name="uri">The URI used to identify the JSON resource</param>
        /// <param name="array">The content of the resource as an array</param>
        /// <returns>A newly constructed JSON resource</returns>

        public static Resource JsonResource(string uri, XdmArray array) {
            return new(new JSONResource(uri, (ArrayItem)array.value));
        }
        
        /// <summary>
        /// Construct a plain text resource, supplying the URI location from whence
        /// the text can be retrieved
        /// </summary>
        /// <param name="uri">The URI used to locate the plain text resource</param>
        /// <returns>A newly constructed plain text resource</returns>

        public static Resource TextResource(string uri) {
            AbstractResourceCollection.InputDetails details = new();
            details.contentType = "text/plain";
            details.resourceUri = uri;
            return new(new UnparsedTextResource(details));
        }
        
        /// <summary>
        /// Construct a plain text resource, supplying the textual content
        /// </summary>
        /// <param name="uri">The URI used to identify the plain text resource</param>
        /// <param name="content">The content of the text resource</param>
        /// <returns>A newly constructed plain text resource</returns>

        public static Resource TextResource(string uri, string content) {
            return new(new UnparsedTextResource(uri, content));
        }
        
        /// <summary>
        /// Construct a binary resource, supplying the URI location from whence
        /// the binary content can be retrieved
        /// </summary>
        /// <param name="uri">The URI used to locate the binary resource</param>
        /// <returns>A newly constructed binary resource</returns>
        
        public static Resource BinaryResource(string uri) {
            AbstractResourceCollection.InputDetails details = new();
            details.contentType = "text/plain";
            details.resourceUri = uri;
            return new(new BinaryResource(details));
        }
        
        /// <summary>
        /// Construct a binary resource, supplying the binary content
        /// </summary>
        /// <param name="uri">The URI used to identify the plain text resource</param>
        /// <param name="contentType">The media type of the binary content</param>
        /// <param name="content">The content of the binary resource</param>
        /// <returns>A newly constructed binary resource</returns>
        /// 
        public static Resource BinaryResource(string uri, string contentType, byte[] content) {
            return new(new BinaryResource(uri, contentType, content));
        }

        public string ContentType => resource.getContentType();

        public string ResourceURI => resource.getResourceURI();

        public XdmItem GetXdmItem(DynamicContext context)
        {
            return XdmItem.FromGroundedValue(resource.getItem(context.context).materialize())[0];
        }
    }
}