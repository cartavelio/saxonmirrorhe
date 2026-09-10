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
            rr.purpose = "fn:unparsed-text";
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
                                inputStream = Saxon.Impl.Overrides.ResourceLoader.urlStream(url);
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
            }
            if (debug) {
                err.info(("unparsed-text(): content type = "+contentType));
            }
            bool isXmlMediaType = false;
            if ((contentType != null)) {
                string mediaType;
                int pos = contentType.IndexOf(';');
                if ((pos>=0)) {
                    mediaType = contentType.Substring(0, pos - 0);
                } else {
                    mediaType = contentType;
                }
                mediaType = mediaType.Trim();
                if (debug) {
                    err.info(("unparsed-text(): media type = "+mediaType));
                }
                isXmlMediaType = (((mediaType.StartsWith("application/") || mediaType.StartsWith("text/"))) && ((mediaType.EndsWith("/xml") || mediaType.EndsWith("+xml"))));
                string charset = "";
                pos = contentType.ToLower().IndexOf("charset");
                if ((pos>=0)) {
                    pos = contentType.IndexOf('=', (pos+7));
                    if ((pos>=0)) {
                        charset = contentType.Substring((pos+1));
                    }
                    if (((pos = charset.IndexOf(';'))>0)) {
                        charset = charset.Substring(0, pos - 0);
                    }
                    if (((pos = charset.IndexOf('('))>0)) {
                        charset = charset.Substring(0, pos - 0);
                    }
                    if (((pos = charset.IndexOf('"'))>0)) {
                        charset = charset.Substring((pos+1), charset.IndexOf('"', (pos+2)) - (pos+1));
                    }
                    if (debug) {
                        err.info(("unparsed-text(): charset = "+charset.Trim()));
                    }
                    encoding = charset.Trim();
                }
            }
            try {
                if (((encoding == null) || isXmlMediaType)) {
                    encoding = Saxon.Hej.resource.EncodingDetector.inferStreamEncoding(inputStream, (debug ? err : null));
                    if (debug) {
                        err.info(("unparsed-text(): inferred encoding = "+encoding));
                    }
                }
            } catch (System.IO.IOException e) {
                encoding = "UTF-8";
            }
            return makeReaderFromStream(inputStream, encoding);
        }
        private static System.IO.Stream openClasspathResource(Saxon.Hej.Configuration config, string systemId) {
            throw new Saxon.Hej.trans.XPathException("classpath: URI scheme is not supported on .NET");
        }
        private static System.IO.TextReader makeReaderFromStream(System.IO.Stream stream, string encoding) {
            return new System.IO.StreamReader(stream, System.Text.Encoding.GetEncoding(encoding));
        }
    
    }
}