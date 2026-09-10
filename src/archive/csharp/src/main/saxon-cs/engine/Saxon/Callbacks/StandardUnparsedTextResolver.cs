using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Saxon.Ejava.lang;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.query;
using Saxon.Hej.resource;
using Saxon.Api;
using Exception = System.Exception;

namespace Saxon.Callbacks {
    public class StandardUnparsedTextResolver {
        private XmlResolver resolver;
        
        public StandardUnparsedTextResolver() {
            this.resolver = null;
        }

        public StandardUnparsedTextResolver(XmlResolver xmlResolver) {
            this.resolver = xmlResolver;
        }

        public TextReader resolve(Uri absoluteURI, string encoding, Configuration config) {
            resolver ??= new StandardXmlResolver(config);
            // Handle a URI using the data: URI scheme
            if (absoluteURI.Scheme == "data") {
                Saxon.Hej.lib.Resource resource;
                try {
                    resource = DataURIScheme.decode(absoluteURI);
                }
                catch (IllegalArgumentException e) {
                    throw new XPathException("Invalid URI using 'data' scheme: " + e.getMessage());
                }

                if (resource is BinaryResource binaryResource) {
                    MemoryStream stream = new MemoryStream(binaryResource.getData());
                    return new StreamReader(stream, Encoding.UTF8);
                }
                else {
                    //resource is UnparsedTextResource;
                    return new StringReader(((UnparsedTextResource) resource).getContent());
                }
            }
            if (absoluteURI.Scheme == "http" || absoluteURI.Scheme == "https") {
                // TODO: this should also work for file: and ftp: - but then need to
                // supply the encoding
                try {
                    using WebClient client = new ();
                    string s = client.DownloadString(absoluteURI);
                    return new StringReader(s);
                }
                catch (System.Net.WebException e) {
                    throw new Saxon.Hej.trans.XPathException("Failed to read unparsed text at " + absoluteURI, e);
                }
            }

            try {
                Stream stream = (Stream)resolver.GetEntity(absoluteURI, "plain/text", typeof(Stream));
                if (encoding == null) {
                    encoding = inferEncoding(stream);
                }
                if (encoding == "iso-646") {
                    encoding = "us-ascii";
                }
                return new StreamReader(stream, Encoding.GetEncoding(encoding));
            }
            catch (Exception e) {
                throw new Saxon.Hej.trans.XPathException("Failed to read unparsed text at " + absoluteURI, e);

            }
        }

        protected string inferEncoding(Stream stream) {
            return EncodingDetector.inferStreamEncoding(stream, null);
        }
        
    }
}

// Copyright (c) 2021 Saxonica Limited