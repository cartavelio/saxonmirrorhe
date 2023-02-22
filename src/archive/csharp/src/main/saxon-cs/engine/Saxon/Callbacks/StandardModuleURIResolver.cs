using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

namespace Saxon.Callbacks {
    public class StandardModuleURIResolver : Saxon.Hej.lib.ModuleURIResolver {
        private XmlResolver resolver;
        private Saxon.Hej.Configuration config;
        

        public StandardModuleURIResolver(Saxon.Hej.Configuration config) {
            this.config = config;
        }

        public StandardModuleURIResolver(XmlResolver resolver) {
            this.resolver = resolver;
        }

        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
        }

        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }

        public Saxon.Ejavax.xml.transform.stream.StreamSource[] resolve(string moduleURI, string baseURI, string[] locations) {
            if ((locations.Length == 0)) {
                Saxon.Hej.trans.XPathException err =
                    new Saxon.Hej.trans.XPathException(("Cannot locate module for namespace " + moduleURI));
                err.setErrorCode("XQST0059");
                err.setIsStaticError(true);
                throw err;
            }
            else {
                if ((baseURI == null)) {
                    baseURI = this.GetType().Assembly.Location;
                }

                Uri @base = new Uri(baseURI);
                Saxon.Ejavax.xml.transform.stream.StreamSource[] sources =
                    new Saxon.Ejavax.xml.transform.stream.StreamSource[locations.Length];
                for (int m = 0; (m < locations.Length); m++) {
                    string href = locations[m];
                    Uri absoluteURI;
                    try {
                        absoluteURI = resolver.ResolveUri(@base, href);
                    }
                    catch (System.Exception err) {
                        Saxon.Hej.trans.XPathException se =
                            new Saxon.Hej.trans.XPathException(("Cannot resolve relative URI " + href), err);
                        se.setErrorCode("XQST0059");
                        se.setIsStaticError(true);
                        throw se;
                    }

                    sources[m] = getQuerySource(absoluteURI);
                }

                return sources;
            }
        }

        private Saxon.Ejavax.xml.transform.stream.StreamSource getQuerySource(Uri abs) {
            try {
                System.Object obj = resolver.GetEntity(abs, "application/xquery", Type.GetType("System.IO.Stream"));
                if ((obj is Stream stream)) {
                    Saxon.Ejavax.xml.transform.stream.StreamSource
                        source = new Saxon.Ejavax.xml.transform.stream.StreamSource(stream);
                    source.setSystemId(abs.ToString());
                    return source;
                }
                else {
                    if (obj is TextReader reader) {
                        Saxon.Ejavax.xml.transform.stream.StreamSource source =
                            new Saxon.Ejavax.xml.transform.stream.StreamSource(reader);
                        source.setSystemId(abs.ToString());
                        return source;
                    }
                    else {
                        if ((obj is Saxon.Ejavax.xml.transform.stream.StreamSource)) {
                            return ((Saxon.Ejavax.xml.transform.stream.StreamSource) (obj));
                        }
                        else {
                            throw new Saxon.Hej.trans.XPathException(
                                ("Unrecognized object returned by XmlResolver (type " + obj.GetType().Name));
                        }
                    }
                }
            }
            catch (Saxon.Hej.trans.XPathException e) {
                throw;
            }
            catch (System.Exception e) {
                throw new Saxon.Hej.trans.XPathException(e.Message, e);
            }
        }
    }
}

// Copyright (c) 2021 Saxonica Limited