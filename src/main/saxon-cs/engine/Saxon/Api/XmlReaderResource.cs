using System;
using System.Collections.Immutable;
using System.Impl.Helpers;
using System.Runtime.CompilerServices;
using Saxon.Hej.pull;

namespace Saxon.Api {
    /// <summary>
    /// An implementation of <c>IResource</c> that contains the ingredients to construct an XDM node,
    /// supplied in the form of a <c>DocumentBuilder</c> and an <c>XmlReader</c> representing an XML parser
    /// that can be called upon to construct the node when required
    /// </summary>
    public class XmlReaderResource : IResource {

        private readonly DocumentBuilder builder;
        private XdmNode node = null;
        private readonly System.Xml.XmlReader parser;

        /// <summary>
        /// Create a <c>XmlReaderResource</c> whose content is supplied as an existing XDM node
        /// </summary>
        /// <param name="builder">A document builder, suitably initialized</param>
        /// <param name="parser">An XmlReader, suitably initialized</param>

        public XmlReaderResource(DocumentBuilder builder, System.Xml.XmlReader parser) {
            this.builder = builder;
            this.parser = parser;
        }
        
        /// <summary>
        /// The resource URI returned is the base URI property set in the XML parser. Note there
        /// is no guarantee this will be unique.
        /// </summary>

        public Uri ResourceUri => new Uri(parser.BaseURI);

        /// <summary>
        /// The XmlReader (parser), as supplied
        /// </summary>
        public System.Xml.XmlReader XmlReader => parser;
        
        /// <summary>
        /// Get the item represented by this resource
        /// </summary>
        /// <returns>An XdmNode representing the document node of the document constructed
        /// using the document builder and parser.</returns>
        public XdmItem GetXdmItem() {
            if (node != null) {
                return node;
            }
            node = builder.Build(parser);
            return node;
        }

        /// <summary>
        /// The content type, always "application/xml"
        /// </summary>
        public string ContentType => "application/xml";
    }
}