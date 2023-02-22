using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StandardUnparsedTextResolver : Saxon.Hej.lib.UnparsedTextURIResolver {
        private bool debug = false;
        public void setDebugging(bool debug) {
            this.debug = debug;
        }
        public System.IO.TextReader resolve(System.Uri absoluteURI, string encoding, Saxon.Hej.Configuration config) {
            Saxon.Hej.lib.Logger err = config.getLogger();
            if (debug) {
                err.info(("unparsed-text(): processing "+absoluteURI));
                err.info(("unparsed-text(): requested encoding = "+encoding));
            }
            if (!(absoluteURI.IsAbsoluteUri)) {
                throw new Saxon.Hej.trans.XPathException(("Resolved URI supplied to unparsed-text() is not absolute: "+absoluteURI.ToString()), "FOUT1170");
            }
            Saxon.Hej.lib.JJ_ResourceRequest rr = new Saxon.Hej.lib.JJ_ResourceRequest();
            rr.uri = absoluteURI.ToString();
            rr.nature = Saxon.Hej.lib.JJ_ResourceRequest.TEXT_NATURE;
            rr.purpose = Saxon.Hej.lib.JJ_ResourceRequest.ANY_PURPOSE;
            Saxon.Ejavax.xml.transform.Source resolved = rr.resolve(config.getResourceResolver(), new Saxon.Callbacks.DirectResourceResolver(config));
            if ((resolved == null)) {
                throw new Saxon.Hej.trans.XPathException(("unparsed-text(): failed to resolve URI "+absoluteURI));
            }
            if ((resolved is Saxon.Ejavax.xml.transform.stream.StreamSource)) {
                return getReaderFromStreamSource((Saxon.Ejavax.xml.transform.stream.StreamSource)(resolved), encoding, config, debug);
            } else {
                throw new Saxon.Hej.trans.XPathException("Resolver for unparsed-text() returned non-StreamSource");
            }
        }
        public static System.IO.TextReader getReaderFromStreamSource(Saxon.Ejavax.xml.transform.stream.StreamSource source, string encoding, Saxon.Hej.Configuration config, bool debug) {
            Saxon.Hej.lib.Logger err = config.getLogger();
            System.IO.Stream inputStream = source.getInputStream();
            if ((inputStream == null)) {
                if ((source.getReader() != null)) {
                    return source.getReader();
                } else {
                    string systemId = source.getSystemId();
                    if ((systemId != null)) {
                        try {
                            if (systemId.StartsWith("classpath:")) {
                                inputStream = openClasspathResource(config, systemId);
                            } else {
                                Saxon.Ejava.net.URL url = new Saxon.Ejava.net.URL(systemId);
                                return Saxon.Impl.Overrides.ResourceLoader.urlReader(url, encoding);
                            }
                        } catch (System.IO.IOException e) {
                            throw new Saxon.Hej.trans.XPathException(("unparsed-text(): cannot retrieve "+systemId), e);
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException("unparsed-text(): resolver returned empty StreamSource");
                    }
                }
            }
            string contentType = null;
            if ((source is Saxon.Hej.resource.TypedStreamSource)) {
                contentType = ((Saxon.Hej.resource.TypedStreamSource)(source)).getContentType();
                if ((contentType != null)) {
                    Saxon.Hej.resource.ParsedContentType parsedContentType = new Saxon.Hej.resource.ParsedContentType(contentType);
                    string contentEncoding = parsedContentType.encoding;
                    if ((contentEncoding != null)) {
                        encoding = contentEncoding;
                    }
                    if ((encoding == null)) {
                        try {
                            encoding = Saxon.Hej.resource.EncodingDetector.inferStreamEncoding(inputStream, "UTF-8", null);
                        } catch (System.IO.IOException e) {
                            throw new Saxon.Hej.trans.XPathException("Unable to infer encoding", e);
                        }
                    }
                }
                if ((encoding != null)) {
                    return Saxon.Impl.Overrides.ResourceLoader.getReaderFromStream(inputStream, encoding);
                }
            }
            try {
                if ((encoding == null)) {
                    inputStream = Saxon.Hej.query.InputStreamMarker.ensureMarkSupported(inputStream);
                    encoding = Saxon.Hej.resource.EncodingDetector.inferStreamEncoding(inputStream, "utf-8", (debug ? err : null));
                    if (debug) {
                        err.info(("unparsed-text(): inferred encoding = "+encoding));
                    }
                }
            } catch (System.IO.IOException e) {
                encoding = "UTF-8";
            }
            return Saxon.Impl.Overrides.ResourceLoader.getReaderFromStream(inputStream, encoding);
        }
        private static System.IO.Stream openClasspathResource(Saxon.Hej.Configuration config, string systemId) {
            throw new Saxon.Hej.trans.XPathException("classpath: URI scheme is not supported on .NET");
        }
    
    }
}