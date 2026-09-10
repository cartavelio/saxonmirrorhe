using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

namespace Saxon.Callbacks {
    public class StandardModuleURIResolver : net.sf.saxon.lib.ModuleURIResolver {
        private XmlResolver resolver;
        private net.sf.saxon.Configuration config;

        public StandardModuleURIResolver() {
        }

        public StandardModuleURIResolver(net.sf.saxon.Configuration config) {
            this.config = config;
        }

        public StandardModuleURIResolver(XmlResolver resolver) {
            this.resolver = resolver;
        }

        public void setConfiguration(net.sf.saxon.Configuration config) {
            this.config = config;
        }

        public net.sf.saxon.Configuration getConfiguration() {
            return config;
        }

        public javax.xml.transform.stream.StreamSource[] resolve(string moduleURI, string baseURI, string[] locations) {
            if ((locations.Length == 0)) {
                net.sf.saxon.trans.XPathException err =
                    new net.sf.saxon.trans.XPathException(("Cannot locate module for namespace " + moduleURI));
                err.setErrorCode("XQST0059");
                err.setIsStaticError(true);
                throw err;
            }
            else {
                if ((baseURI == null)) {
                    baseURI = this.GetType().Assembly.Location;
                }

                Uri @base = new Uri(baseURI);
                javax.xml.transform.stream.StreamSource[] sources =
                    new javax.xml.transform.stream.StreamSource[locations.Length];
                for (int m = 0; (m < locations.Length); m++) {
                    string href = locations[m];
                    Uri absoluteURI;
                    try {
                        absoluteURI = resolver.ResolveUri(@base, href);
                    }
                    catch (System.Exception err) {
                        net.sf.saxon.trans.XPathException se =
                            new net.sf.saxon.trans.XPathException(("Cannot resolve relative URI " + href), err);
                        se.setErrorCode("XQST0059");
                        se.setIsStaticError(true);
                        throw se;
                    }

                    sources[m] = getQuerySource(absoluteURI);
                }

                return sources;
            }
        }

        private javax.xml.transform.stream.StreamSource getQuerySource(Uri abs) {
            try {
                System.Object obj = resolver.GetEntity(abs, "application/xquery", Type.GetType("System.IO.Stream"));
                if ((obj is Stream stream)) {
                    javax.xml.transform.stream.StreamSource
                        source = new javax.xml.transform.stream.StreamSource(stream);
                    source.setSystemId(abs.ToString());
                    return source;
                }
                else {
                    if (obj is TextReader reader) {
                        javax.xml.transform.stream.StreamSource source =
                            new javax.xml.transform.stream.StreamSource(reader);
                        source.setSystemId(abs.ToString());
                        return source;
                    }
                    else {
                        if ((obj is javax.xml.transform.stream.StreamSource)) {
                            return ((javax.xml.transform.stream.StreamSource) (obj));
                        }
                        else {
                            throw new net.sf.saxon.trans.XPathException(
                                ("Unrecognized object returned by XmlResolver (type " + obj.GetType().Name));
                        }
                    }
                }
            }
            catch (net.sf.saxon.trans.XPathException e) {
                throw;
            }
            catch (System.Exception e) {
                throw new net.sf.saxon.trans.XPathException(e.Message, e);
            }
        }
    }
}