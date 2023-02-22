using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using net.sf.saxon;
using net.sf.saxon.query;
using net.sf.saxon.resource;

namespace Saxon.Callbacks {
    public class StandardUnparsedTextResolver : net.sf.saxon.lib.UnparsedTextURIResolver {
        private XmlResolver resolver = new StandardXmlResolver();

        public StandardUnparsedTextResolver() {
        }

        public TextReader resolve(Uri absoluteURI, string encoding, Configuration config) {
            if (absoluteURI.Scheme == "http" || absoluteURI.Scheme == "https") {
                // TODO: this should also work for file: and ftp: - but then need to
                // supply the encoding
                try {
                    using (WebClient client = new WebClient()) {
                        string s = client.DownloadString(absoluteURI);
                        return new StringReader(s);
                    }
                }
                catch (System.Net.WebException e) {
                    throw new net.sf.saxon.trans.XPathException("Failed to read unparsed text at " + absoluteURI, e);
                }
            }

            try {
                Stream stream = (Stream)resolver.GetEntity(absoluteURI, "plain/text", typeof(Stream));
                if (encoding == null) {
                    encoding = inferEncoding(stream);
                }
                return new StreamReader(stream, Encoding.GetEncoding(encoding));
            }
            catch (Exception e) {
                throw new net.sf.saxon.trans.XPathException("Failed to read unparsed text at " + absoluteURI, e);

            }
        }

        protected string inferEncoding(Stream stream) {
            return EncodingDetector.inferStreamEncoding(stream, null);
        }
        
    }
}