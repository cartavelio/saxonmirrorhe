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
        private readonly Configuration config;
        private CommonResourceResolver resolver;
        
        public StandardUnparsedTextResolver(Configuration config) {
            this.config = config;
            this.resolver = config.getCommonResourceResolver();
        }

        public StandardUnparsedTextResolver(Configuration config, XmlResolver xmlResolver) {
            this.config = config;
            this.resolver = new XmlResolverWrapper(xmlResolver);
        }

        public TextReader resolve(Uri absoluteURI, string encoding, Configuration config) {
            if (absoluteURI.Scheme == "data") {
                // Handle a URI using the data: URI scheme
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
            
            if (resolver != null) {
                StreamSource ss = resolver.DereferenceLocation(absoluteURI, absoluteURI.ToString(), null, null);
                if (ss != null) {
                    if (ss.getReader() != null) {
                        return ss.getReader();
                    }

                    try {
                        Stream stream = ss.getInputStream();
                        if (encoding == null) {
                            encoding = inferEncoding(stream);
                        }
                        if (encoding == "iso-646") {
                            encoding = "us-ascii";
                        }
                        return new StreamReader(stream, Encoding.GetEncoding(encoding));
                    }
                    catch (Exception e) {
                        throw new Hej.trans.XPathException("Failed to read unparsed text at " + absoluteURI, e);
                    }
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
            else {
                try {
                    using WebClient client = new ();
                    Stream stream = client.OpenRead(absoluteURI);
                    if (encoding == null) {
                        encoding = inferEncoding(stream);
                    }
                    if (encoding == "iso-646") {
                        encoding = "us-ascii";
                    }
                    return new StreamReader(stream, Encoding.GetEncoding(encoding));
                }
                catch (System.Net.WebException e) {
                    throw new Saxon.Hej.trans.XPathException("Failed to read unparsed text at " + absoluteURI, e);
                }
            }

            return null;

        }

        protected string inferEncoding(Stream stream) {
            return EncodingDetector.inferStreamEncoding(stream, null);
        }
        
    }
}

// Copyright (c) 2021 Saxonica Limited