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
        /// <param name="encoding">The <c>Encoding</c> used to decode the binary contents of the stream</param>
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
        /// <param name="systemId">The base URI of the content</param>
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

        public bool hasReader() {
            return reader != null;
        }

        public bool hasStream() {
            return inputStream != null;
        }
        
        public Uri ResourceUri { get; }

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

        public XdmItem GetXdmItem() {
            return new XdmAtomicValue(Content);
        }

        public string ContentType => "text/plain";
    }
}