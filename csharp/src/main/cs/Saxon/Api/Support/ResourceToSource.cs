using System.IO;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.om;
using Saxon.Hej.resource;
using Saxon.Hej.trans;

namespace Saxon.Api.Support {
    /// <summary>
    /// Converts an <c>Saxon.Api.IResource</c> (the external SaxonCS representation) to a JAXP <c>Source</c>
    /// object
    /// </summary>
    public class ResourceToSource  {

        public static Source convert(IResource resource) {
            return resource switch {
                TextResource text => new StreamSource(text.TextReader, text.ResourceUri.ToString()),
                NodeResource node => new NodeSource((NodeInfo)((XdmNode)node.GetXdmItem()).Unwrap()),
                XmlReaderResource xml => new NodeSource((NodeInfo)((XdmNode)xml.GetXdmItem()).Unwrap()),
                BinaryResource bin => new StreamSource(bin.InputStream),
                _ => throw new XPathException("Unknown resource type " + resource.GetType())
            };
        } 

    }
}