using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using net.sf.saxon;

namespace Saxon.Callbacks {
    public class StandardUnparsedTextResolver : net.sf.saxon.lib.UnparsedTextURIResolver {
        private XmlResolver resolver = new XmlUrlResolver();

        public StandardUnparsedTextResolver() {
        }

        public TextReader resolve(Uri absoluteURI, string encoding, Configuration config) {
            if (absoluteURI.Scheme == "http" || absoluteURI.Scheme == "https") {
                // TODO: this should also work for file: and ftp: - but then need to
                // supply the encoding
                using (WebClient client = new WebClient()) {
                    string s = client.DownloadString(absoluteURI);
                    return new StringReader(s);
                }
            }
            Stream stream = (Stream)resolver.GetEntity(absoluteURI, "plain/text", typeof(Stream));
            // TODO: peek at the content to guess the encoding
            if (encoding == null) {
                encoding = "utf-8";
            }
            return new StreamReader(stream, Encoding.GetEncoding(encoding));
        }
        
    }
}