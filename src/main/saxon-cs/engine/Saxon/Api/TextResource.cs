using System;
using System.IO;

namespace Saxon.Api {
    
    /// <summary>
    /// An implementation of <c>IResource</c> that represents plain text
    /// </summary>
    public class TextResource : IResource {

        private System.IO.TextReader reader;
        private string _content;

        /// <summary>
        /// Create a <c>TextResource</c> whose content is supplied as a constant string
        /// </summary>
        /// <param name="content">The plain text content</param>
        /// <param name="systemId">The base URI of the content</param>

        public TextResource(string content, Uri systemId) {
            this.reader = null;
            this._content = content;
            this.ResourceUri = systemId;
        }
        
        /// <summary>
        /// Create a <c>TextResource</c> whose content is delivered using a <c>TextReader</c>
        /// </summary>
        /// <param name="reader">The <c>TextReader</c> used to deliver the content</param>
        /// <param name="systemId">The base URI of the content</param>

        public TextResource(System.IO.TextReader reader, Uri systemId) {
            this.reader = reader;
            this.ResourceUri = systemId;
        }
        
        /// <summary>
        /// Create a <c>TextResource</c> whose content is delivered using a <c>Stream</c>,
        /// with a specified encoding
        /// </summary>
        /// <param name="stream">The <c>Stream</c> used to deliver the content</param>
        /// <param name="encoding">The <c>Encoding</c> used to decode the binary contents of the stream</param>
        /// <param name="systemId">The base URI of the content</param>
        
        public TextResource(System.IO.Stream stream, System.Text.Encoding encoding, Uri systemId) {
            this.reader = new StreamReader(stream, encoding);
            this.ResourceUri = systemId;
        }
        
        public Uri ResourceUri { get; }

        public string Content {
            get {
                if (_content == null) {
                    _content = reader.ReadToEnd();
                }
                return _content;
            }
        }
        public XdmItem GetXdmItem() {
            return new XdmAtomicValue(Content);
        }

        public string ContentType => "text/plain";
    }
}