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
        /// <param name="node">The XDM node</param>

        public NodeResource(XdmNode node) {
            this.node = node;
            this.ResourceUri = node.DocumentUri;
        }
        
        /// <summary>
        /// The <c>ResourceUri</c> of a <c>NodeResource<c> is the document URI of the node, if it exists
        /// </summary>
        /// <remarks>
        /// <para>For many nodes, (including all non-document nodes) the <c>ResourceUri</c> will be null.</para> 
        /// </remarks>

        public Uri ResourceUri { get; }
        
        /// <summary>
        /// Get the item representing the value of the resource
        /// </summary>
        /// <returns>The contained XDM node</returns>
        public XdmItem GetXdmItem() {
            return node;
        }

        /// <summary>
        /// The content type of the resource: for a <c>NodeResource</c> this is "application/xml".
        /// </summary>
        public string ContentType => "application/xml";
    }
}