using System;
using System.Collections.Immutable;
using System.Impl.Helpers;
using System.Runtime.CompilerServices;
using Saxon.Hej.pull;

namespace Saxon.Api {
    /// <summary>
    /// An implementation of <c>IResource</c> that represents an XDM node, supplied in the form of
    /// an <c>XmlReader</c> representing an XML parser that can be called upon to construct the node when required
    /// </summary>
    public class XmlReaderResource : IResource {

        private readonly DocumentBuilder builder;
        private XdmNode node = null;
        private readonly System.Xml.XmlReader parser;

        /// <summary>
        /// Create a <c>XmlReaderResource</c> whose content is supplied as an existing XDM node
        /// </summary>
        /// <param name="content">The plain text content</param>
        /// <param name="systemId">The base URI of the content</param>

        public XmlReaderResource(DocumentBuilder builder, System.Xml.XmlReader parser) {
            this.builder = builder;
            this.parser = parser;
        }

        public Uri ResourceUri => new Uri(parser.BaseURI);

        public System.Xml.XmlReader XmlReader => parser;


        public XdmItem GetXdmItem() {
            if (node != null) {
                return node;
            }
            node = builder.Build(parser);
            return node;
        }

        public string ContentType => "application/xml";
    }
}