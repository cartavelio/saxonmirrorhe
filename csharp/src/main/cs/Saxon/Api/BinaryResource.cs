using System;
using System.IO;
using System.Text;
using System.Xml;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.resource;
using Saxon.Hej.value;

namespace Saxon.Api {
    
    /// <summary>
    /// An implementation of <c>IResource</c> that represents plain text
    /// </summary>
    public class BinaryResource : IResource {
        
        private readonly byte[] _content;

        /// <summary>
        /// Create a <c>BinaryResource</c> whose content is supplied as a constant string
        /// </summary>
        /// <param name="content">The plain text content</param>
        /// <param name="systemId">The base URI of the content</param>

        public BinaryResource(byte[] content, Uri systemId) {
            this._content = content;
            this.ResourceUri = systemId;
        }

        /// <summary>
        /// A URI that identifies this resource
        /// </summary>

        public Uri ResourceUri { get; }


        /// <summary>
        /// Get a <c>Stream</c> for reading the content of this resource in binary.
        /// </summary>
        /// <remarks>
        /// <para>If an input stream is available, it is returned.</para>
        /// <para>If a reader is available, or if the string content is available,
        /// then the content is read and a <c>MemoryStream</c> is returned delivering the content
        /// in UTF-8 encoding.
        /// </para>
        /// </remarks>

        public Stream InputStream => new MemoryStream(_content);

        /// <summary>
        /// Return the content of the resource as an item
        /// </summary>
        /// <returns>An <c>XdmAtomicValue</c> of type <c>xs:string</c> holding the content of the resource</returns>

        public XdmItem GetXdmItem() {
            return new XdmAtomicValue(new Base64BinaryValue(_content));
        }
        
        /// <summary>
        /// The content type of the resource: always <c>application/binary</c>
        /// </summary>

        public string ContentType => "application/binary";
    }
}