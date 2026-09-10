using System;
using System.Xml;
using System.IO;
using System.Net.Http;
using System.Xml.XPath;
using Saxon.Ejava.lang;
using Saxon.Ejavax.xml.transform.sax;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.lib;
using Saxon.Hej.resource;

namespace Saxon.Callbacks {
    public class StandardURIResolver : Saxon.Hej.lib.RelativeURIResolver {
        private readonly System.Xml.XmlResolver resolver;
        private Configuration config;

        public StandardURIResolver(Configuration config) {
            this.config = config;
            this.resolver = config.getCommonResourceResolver();
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
                    string expandedBase = Saxon.Hej.functions.ResolveURI.tryToExpand(@base);
                    if (!(expandedBase.Equals(@base))) {
                        return makeAbsolute(href, expandedBase);
                    }
                    else {
                        Saxon.Hej.trans.XPathException de1 =
                            new Saxon.Hej.trans.XPathException("Invalid URI: '" + href + "'. " + e.Message);
                        de1.setErrorCode("FODC0005");
                        throw de1;
                    }
                }
                catch (System.Exception e) {
                    Saxon.Hej.trans.XPathException xpe =
                        new Saxon.Hej.trans.XPathException("Invalid URI: '" + href + "'. " + e.Message);
                    xpe.setErrorCode("FODC0005");
                    throw xpe;
                }

                Saxon.Hej.trans.XPathException de =
                    new Saxon.Hej.trans.XPathException("No base URI available to resolve '" + href + "'");
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
                    Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(
                        ((((("Failure making absolute URI (base=" + @base) + ", relative=") + href) + "): ") +
                         e.Message));
                    de.setErrorCode("FODC0005");
                    throw de;
                }
                catch (System.Exception e) {
                    Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(
                        ((((("Failure making absolute URI (base=" + @base) + ", relative=") + href) + "): ") +
                         e.Message));
                    de.setErrorCode("FODC0005");
                    throw de;
                }
            }
        }

        public Saxon.Ejavax.xml.transform.Source resolve(string href, string @base) {
            return dereference(makeAbsolute(href, @base));
        }

        public Saxon.Ejavax.xml.transform.Source dereference(string uri) {
            try {
                KnownResourceFinder.NamedInputStream known = KnownResourceFinder.fetchSystemId(uri, config);
                if (known != null) {
                    return new StreamSource(known.stream, uri);
                }

                Uri abs = new Uri(uri);

                // Handle a URI using the data: URI scheme
                if (abs.Scheme == "data") {
                    Resource resource;
                    try {
                        resource = DataURIScheme.decode(abs);
                    }
                    catch (IllegalArgumentException e) {
                        throw new XPathException("Invalid URI using 'data' scheme: " + e.getMessage());
                    }

                    if (resource is BinaryResource binaryResource) {
                        byte[] contents = binaryResource.getData();
                        return new StreamSource(new MemoryStream(contents), abs.OriginalString);
                    }
                    else {
                        //resource is UnparsedTextResource;
                        TextReader reader = new StringReader(((UnparsedTextResource) resource).getContent());
                        return new StreamSource(reader, abs.OriginalString);
                    }
                }
                else {

                    System.Object obj = resolver.GetEntity(abs, null, Type.GetType("System.IO.Stream"));
                    if ((obj is Stream stream)) {
                        Saxon.Ejavax.xml.transform.stream.StreamSource source = new(stream);
                        source.setSystemId(abs.ToString());
                        Saxon.Hej.lib.AugmentedSource @as =
                            Saxon.Hej.lib.AugmentedSource.makeAugmentedSource(source);
                        @as.setPleaseCloseAfterUse(true);
                        return @as;
                    }
                    else if ((obj is TextReader reader)) {
                        Saxon.Ejavax.xml.transform.stream.StreamSource source = new(reader);
                        source.setSystemId(abs.ToString());
                        return source;
                    }
                    else if ((obj is Saxon.Ejavax.xml.transform.Source)) {
                        return ((Saxon.Ejavax.xml.transform.Source) obj);
                    }
                    else if ((obj == null)) {
                        throw new Saxon.Hej.trans.XPathException("XmlResolver returned null");
                    }
                    else {
                        throw new Saxon.Hej.trans.XPathException(
                            ("Unrecognized object returned by XmlResolver (type " + obj.GetType().Name));
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