using System;
using System.Xml;
using System.IO;
using System.Net.Http;
using javax.xml.transform.stream;
using net.sf.saxon;
using net.sf.saxon.lib;

namespace Saxon.Callbacks {
    public class StandardURIResolver : net.sf.saxon.lib.RelativeURIResolver {
        private readonly System.Xml.XmlResolver resolver;
        private Configuration config;

        public StandardURIResolver(Configuration config) {
            this.config = config;
            this.resolver = new StandardXmlResolver();
        }
        public StandardURIResolver(Configuration config, XmlResolver resolver) {
            this.config = config;
            this.resolver = resolver;
        }

        public XmlResolver getXmlResolver() {
            return resolver;
        }

        public void setConfiguration(Configuration config) {
            this.config = config;
        }

        public string makeAbsolute(string href, string @base) {
            if (((@base == null) || (@base.Length == 0))) {
                try {
                    Uri u = new Uri(href, UriKind.RelativeOrAbsolute);
                    if (u.IsAbsoluteUri) {
                        return u.ToString();
                    }
                }
                catch (UriFormatException e) {
                    string expandedBase = net.sf.saxon.functions.ResolveURI.tryToExpand(@base);
                    if (!(expandedBase.Equals(@base))) {
                        return makeAbsolute(href, expandedBase);
                    }
                    else {
                        net.sf.saxon.trans.XPathException de1 =
                            new net.sf.saxon.trans.XPathException("Invalid URI: '" + href + "'. " + e.Message);
                        de1.setErrorCode("FODC0005");
                        throw de1;
                    }
                }
                catch (System.Exception e) {
                    net.sf.saxon.trans.XPathException xpe =
                        new net.sf.saxon.trans.XPathException("Invalid URI: '" + href + "'. " + e.Message);
                    xpe.setErrorCode("FODC0005");
                    throw xpe;
                }
                net.sf.saxon.trans.XPathException de =
                    new net.sf.saxon.trans.XPathException("No base URI available to resolve '" + href + "'");
                de.setErrorCode("FODC0005");
                throw de;
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
                KnownResourceFinder.NamedInputStream known = KnownResourceFinder.fetchSystemId(uri, config);
                if (known != null) {
                    return new StreamSource(known.stream, uri);
                }
                
                Uri abs = new Uri(uri);
                System.Object obj = resolver.GetEntity(abs, null, Type.GetType("System.IO.Stream"));
                if ((obj is Stream)) {
                    javax.xml.transform.stream.StreamSource source =
                        new javax.xml.transform.stream.StreamSource(((Stream) obj));
                    source.setSystemId(abs.ToString());
                    net.sf.saxon.lib.AugmentedSource @as = net.sf.saxon.lib.AugmentedSource.makeAugmentedSource(source);
                    @as.setPleaseCloseAfterUse(true);
                    return @as;
                }
                else {
                    if ((obj is TextReader)) {
                        javax.xml.transform.stream.StreamSource source =
                            new javax.xml.transform.stream.StreamSource(((TextReader) obj));
                        source.setSystemId(abs.ToString());
                        return source;
                    }
                    else {
                        if ((obj is javax.xml.transform.Source)) {
                            return ((javax.xml.transform.Source) obj);
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

