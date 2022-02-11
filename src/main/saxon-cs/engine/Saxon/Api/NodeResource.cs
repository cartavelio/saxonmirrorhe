using System;
using System.IO;

namespace Saxon.Api {
    /// <summary>
    /// An implementation of <c>IResource</c> that represents an XDM node, simply by wrapping
    /// an existing <c>XdmNode</c> object
    /// </summary>
    public class NodeResource : IResource {

        private readonly XdmNode node;

        /// <summary>
        /// Create a <c>NodeResource</c> whose content is supplied as an existing XDM node
        /// </summary>
        /// <param name="content">The plain text content</param>
        /// <param name="systemId">The base URI of the content</param>

        public NodeResource(XdmNode node) {
            this.node = node;
            this.ResourceUri = node.DocumentUri;
        }

        public Uri ResourceUri { get; }
        
        public XdmItem GetXdmItem() {
            return node;
        }

        public string ContentType => "application/xml";
    }
}