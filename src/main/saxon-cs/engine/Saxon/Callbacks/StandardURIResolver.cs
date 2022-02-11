using System;
using System.Impl.Helpers;
using System.Xml;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml.XPath;
using Saxon.Api;
using Saxon.Ejava.lang;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.sax;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.lib;
using Saxon.Hej.pull;
using Saxon.Hej.resource;

namespace Saxon.Callbacks {
    public class StandardURIResolver : Saxon.Hej.lib.RelativeURIResolver {
        private readonly CommonResourceResolver resolver;
        private Configuration config;

        public StandardURIResolver(Configuration config) {
            this.config = config;
            this.resolver = config.getCommonResourceResolver();
        }

        public StandardURIResolver(Configuration config, XmlResolver resolver) {
            this.config = config;
            this.resolver = new XmlResolverWrapper(resolver);
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

        public Saxon.Ejavax.xml.transform.Source dereference(string uri) {
            return resolve(uri, uri);
        }

        public Saxon.Ejavax.xml.transform.Source resolve(string href, string @base) {
            try {

                Uri abs = @base == null ? new Uri(href, UriKind.RelativeOrAbsolute) : new Uri(new Uri(@base), href);

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
                    ResourceRequest request = new() {
                        Uri = abs,
                        RelativeUri = href,
                        Nature = ResourceRequest.XML_NATURE
                    };
                    if (@base != null) {
                        request.BaseUri = new Uri(@base);
                    }
                    IResource result = resolver.DereferenceLocation(request);
                    if (result != null) {
                        if (result is TextResource text) {
                            XmlReaderSettings settings = new () {
                                DtdProcessing = DtdProcessing.Parse, // must expand entity references
                                XmlResolver = config.getCommonResourceResolver()
                            };
                            XmlReader parser;
                            if (text.hasReader()) {
                                parser = XmlReader.Create(text.TextReader, settings, abs.ToString());
                            } else if (text.hasStream()) {
                                parser = XmlReader.Create(text.InputStream, settings, abs.ToString());
                            } else {
                                parser = XmlReader.Create(new StringReader(text.Content), settings, abs.ToString());
                            }
                            DotNetPullProvider provider = new DotNetPullProvider(parser);
                            provider.setBaseURI(text.ResourceUri.ToString());
                            return new PullSource(provider);    
                               
                        } else if (result is NodeResource nr) {
                            return ((XdmNode)nr.GetXdmItem()).Implementation.asActiveSource();
                        } else if (result is XmlReaderResource xrr) {
                            DotNetPullProvider provider = new DotNetPullProvider(xrr.XmlReader);
                            provider.setBaseURI(xrr.ResourceUri.ToString());
                            return new PullSource(provider);
                        }
                        else {
                            return null;
                        }
                    }
                    else {
                        XmlUrlResolver resolver = new XmlUrlResolver();
                        resolver.Credentials = CredentialCache.DefaultCredentials;
                        object entity = resolver.GetEntity(abs, null, typeof(Stream));
                        if (entity is Stream stream) {
                            return new StreamSource(stream, abs.ToString());
                        }

                        return null;
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