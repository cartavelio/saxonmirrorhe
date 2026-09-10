using System;
using System.IO;
using System.Text;
using System.Xml;
using net.sf.saxon;

namespace Saxon.Callbacks {
    public class StandardUnparsedTextResolver : net.sf.saxon.lib.UnparsedTextURIResolver {
        private XmlResolver resolver = new XmlUrlResolver();

        public StandardUnparsedTextResolver() {
        }

        public TextReader resolve(Uri absoluteURI, string encoding, Configuration config) {
            Stream stream = (Stream)resolver.GetEntity(absoluteURI, "plain/text", typeof(Stream));
            return new StreamReader(stream, Encoding.GetEncoding(encoding));
        }
        
    }
}