using System;
using System.IO;
using System.Text;
using System.Xml;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej.resource;

namespace Saxon.Api {
    
    /// <summary>
    /// An implementation of <c>IResource</c> that represents plain text
    /// </summary>
    public class TextResource : IResource {

        private System.IO.TextReader reader;
        private System.IO.Stream inputStream;
        private Encoding encoding;
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
        
        /// <summary>
        /// Create a <c>TextResource</c> whose content is delivered using a <c>Stream</c>,
        /// with an inferred encoding
        /// </summary>
        /// <param name="stream">The <c>Stream</c> used to deliver the content</param>
        /// <param name="systemId">The base URI of the content</param>
        
        public TextResource(System.IO.Stream stream, Uri systemId) {
            this.inputStream = stream;
            this.ResourceUri = systemId;
        }

        /// <summary>
        /// Create a <c>TextResource</c> whose content is delivered using a <c>Stream</c>,
        /// with an explicit encoding
        /// </summary>
        /// <param name="stream">The <c>Stream</c> used to deliver the content</param>
        /// <param name="systemId">The resource URI of the content, also used as the base URI</param>
        /// <param name="encoding">The <c>Encoding</c> used to decode the binary contents of the stream</param>
        public TextResource(System.IO.Stream stream, Uri systemId, Encoding encoding) {
            this.inputStream = stream;
            this.encoding = encoding;
            this.ResourceUri = systemId;
        }

        internal TextResource(StreamSource source) {
            if (source.getReader() != null) {
                this.reader = source.getReader();
            }

            if (source.getInputStream() != null) {
                this.inputStream = source.getInputStream();
            }

            this.ResourceUri = new Uri(source.getSystemId(), UriKind.RelativeOrAbsolute);

        }
        
        /// <summary>
        /// Ask whether the text resource has a known <c>TextReader</c>
        /// </summary>
        /// <returns>
        /// True if there is an existing <c>TextReader</c>
        /// </returns>

        public bool hasReader() {
            return reader != null;
        }
        
        /// <summary>
        /// Ask whether the text resource has a known input <c>Stream</c>
        /// </summary>
        /// <returns>
        /// True if there is an existing input <c>Stream</c>
        /// </returns>

        public bool hasStream() {
            return inputStream != null;
        }
        
        /// <summary>
        /// The URI of the text resource
        /// </summary>
        
        public Uri ResourceUri { get; }

        /// <summary>
        /// The content of the text resource, as a string
        /// </summary>
        /// <remarks>
        /// <para>Reading this property may cause any contained <c>TextReader</c> or <c>Stream</c>
        /// to be consumed and closed. The value will be retained internally and can be accessed
        /// repeatedly.
        /// </para>
        /// <para>If the value needs to be obtained by reading a (binary) stream, the supplied
        /// encoding will be used if available; if not, the system attempts to infer an encoding
        /// from the content of the stream, for example by looking for a byte-order mark.
        /// </para>
        /// </remarks>
        public string Content {

            get {
                if (_content != null) {
                    return _content;
                } else if (reader != null) {
                    _content = reader.ReadToEnd();
                    reader.Close();
                    return _content;
                } else if (inputStream != null) {
                    if (encoding != null) {
                        StreamReader sr = new StreamReader(inputStream, encoding);
                        return sr.ReadToEnd();
                    }
                    else {
                        string enc = EncodingDetector.inferStreamEncoding(inputStream, null /*new StandardLogger()*/);
                        StreamReader sr = new StreamReader(inputStream, Encoding.GetEncoding(enc));
                        return sr.ReadToEnd();
                    }
                }
                else {
                    return "";
                }
            }
        }
        
        /// <summary>
        /// Get a TextReader for reading the content of this resource in character form
        /// </summary>
        /// <remarks>
        /// <para>If an explicit <c>TextReader</c> is already available, it is returned.</para>
        /// <para>If the content of the resource is available as a string, then a <c>StringReader</c>
        /// giving access to the content is returned.</para>
        /// <para>If an input <c>Stream</c> is available, then a <c>TextReader</c> is constructed,
        /// taking account of the encoding if known, or with an inferred encoding otherwise.</para>
        /// <para>As a last resort, a <c>StringReader</c> delivering a zero-length string is returned.</para>
        /// </remarks>

        public TextReader TextReader {
            get {
                if (reader != null) {
                    return reader;
                }
                else if (_content != null) {
                    return new StringReader(_content);
                }
                else if (inputStream != null) {
                    string enc = EncodingDetector.inferStreamEncoding(inputStream, null /*new StandardLogger()*/);
                    return new StreamReader(inputStream, Encoding.GetEncoding(enc));
                }
                else {
                    return new StringReader("");
                }
            }
        }
        
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

        public Stream InputStream {
            get {
                if (inputStream != null) {
                    return inputStream;
                } else if (reader != null || _content != null) {
                    byte[] encodedContent = Encoding.UTF8.GetBytes(Content);
                    return new MemoryStream(encodedContent);
                } else {
                    return null;
                }
            }
        }
        
        /// <summary>
        /// Return the content of the resource as an item
        /// </summary>
        /// <returns>An <c>XdmAtomicValue</c> of type <c>xs:string</c> holding the content of the resource</returns>

        public XdmItem GetXdmItem() {
            return new XdmAtomicValue(Content);
        }
        
        /// <summary>
        /// The content type of the resource: always <c>text/plain</c>
        /// </summary>

        public string ContentType => "text/plain";
    }
}