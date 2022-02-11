using System;
using System.Xml;
using System.IO;

namespace Saxon.Callbacks {
    public class StandardURIResolver : net.sf.saxon.lib.RelativeURIResolver {
        private readonly XmlResolver resolver;
        private net.sf.saxon.Configuration config;
        private bool recognizeQueryParameters;

        public StandardURIResolver(XmlResolver resolver) {
            this.resolver = resolver;
        }
        
        public StandardURIResolver(net.sf.saxon.Configuration config) {
            this.config = config;
        }

        public void setConfiguration(net.sf.saxon.Configuration config) {
            this.config = config;
        }
        
        public net.sf.saxon.Configuration getConfiguration() {
            return config;
        }

        public void setRecognizeQueryParameters(bool setting) {
            // TODO: implement me
            recognizeQueryParameters = setting;
        }

        public bool queryParametersAreRecognized() {
            return recognizeQueryParameters;
        }

        public XmlResolver getXmlResolver() {
            return resolver;
        }

        public string makeAbsolute(string href, string @base) {
            if (((@base == null) || (@base.Length == 0))) {
                try {
                    return new Uri(href).ToString();
                }
                catch (UriFormatException e) {
                    string expandedBase = net.sf.saxon.functions.ResolveURI.tryToExpand(@base);
                    if (!(expandedBase.Equals(@base))) {
                        return makeAbsolute(href, expandedBase);
                    }
                    else {
                        net.sf.saxon.trans.XPathException de =
                            new net.sf.saxon.trans.XPathException(("Invalid URI: " + e.Message));
                        de.setErrorCode("FODC0005");
                        throw de;
                    }
                }
                catch (System.Exception e) {
                    net.sf.saxon.trans.XPathException de =
                        new net.sf.saxon.trans.XPathException(("Invalid URI: " + e.Message));
                    de.setErrorCode("FODC0005");
                    throw de;
                }
            }
            else {
                try {
                    if (false) {
                        throw new UriFormatException();
                    }

                    return resolver.ResolveUri(new Uri(@base), href).ToString();
                }
                catch (UriFormatException e) {
                    net.sf.saxon.trans.XPathException de = new net.sf.saxon.trans.XPathException(
                        ((((("Failure making absolute URI (base=" + @base) + ", relative=") + href) + "): ") +
                         e.Message));
                    de.setErrorCode("FODC0005");
                    throw de;
                }
                catch (System.Exception e) {
                    net.sf.saxon.trans.XPathException de = new net.sf.saxon.trans.XPathException(
                        ((((("Failure making absolute URI (base=" + @base) + ", relative=") + href) + "): ") +
                         e.Message));
                    de.setErrorCode("FODC0005");
                    throw de;
                }
            }
        }

        public javax.xml.transform.Source resolve(string href, string @base) {
            return dereference(makeAbsolute(href, @base));
        }

        public javax.xml.transform.Source dereference(string uri) {
            try {
                Uri abs = new Uri(uri);
                System.Object obj = resolver.GetEntity(abs, null, Type.GetType("System.IO.Stream"));
                if ((obj is Stream stream)) {
                    javax.xml.transform.stream.StreamSource
                        source = new javax.xml.transform.stream.StreamSource(stream);
                    source.setSystemId(abs.ToString());
                    net.sf.saxon.lib.AugmentedSource @as = net.sf.saxon.lib.AugmentedSource.makeAugmentedSource(source);
                    @as.setPleaseCloseAfterUse(true);
                    return @as;
                }
                else {
                    if ((obj is TextReader reader)) {
                        javax.xml.transform.stream.StreamSource source =
                            new javax.xml.transform.stream.StreamSource(reader);
                        source.setSystemId(abs.ToString());
                        return source;
                    }
                    else {
                        if ((obj is javax.xml.transform.Source)) {
                            return ((javax.xml.transform.Source) (obj));
                        }
                        else {
                            if ((obj == null)) {
                                return null;
                            }
                            else {
                                throw new net.sf.saxon.trans.XPathException(
                                    ("Unrecognized object returned by XmlResolver (type " + obj.GetType().Name));
                            }
                        }
                    }
                }
            }
            catch (net.sf.saxon.trans.XPathException e) {
                throw e;
            }
            catch (System.Exception e) {
                throw new net.sf.saxon.trans.XPathException(e.Message, e);
            }
        }
    }
}

