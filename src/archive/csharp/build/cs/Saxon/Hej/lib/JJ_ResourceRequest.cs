using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class JJ_ResourceRequest {
        public const string TEXT_NATURE = "https://www.iana.org/assignments/media-types/text/plain";
        public const string BINARY_NATURE = "https://www.iana.org/assignments/media-types/application/binary";
        public const string XQUERY_NATURE = "https://www.iana.org/assignments/media-types/application/xquery";
        public static readonly string XSLT_NATURE = Saxon.Hej.lib.NamespaceConstant.XSLT;
        public static readonly string XSD_NATURE = Saxon.Hej.lib.NamespaceConstant.SCHEMA;
        public const string XML_NATURE = "https://www.iana.org/assignments/media-types/application/xml";
        public const string DTD_NATURE = "https://www.iana.org/assignments/media-types/application/xml-dtd";
        public const string EXTERNAL_ENTITY_NATURE = "https://www.iana.org/assignments/media-types/application/xml-external-parsed-entity";
        public const string ANY_PURPOSE = null;
        public const string SCHEMA_NATURE = "http://www.w3.org/2001/XMLSchema";
        public const string VALIDATION_PURPOSE = "http://www.rddl.org/purposes#validation";
        public const string ANY_NATURE = null;
        public string uri;
        public string baseUri;
        public string relativeUri;
        public string publicId;
        public string entityName;
        public string nature;
        public string purpose;
        public bool uriIsNamespace;
        public bool streamable;
        public string requestedEncoding;
        public Saxon.Hej.lib.JJ_ResourceRequest copy() {
            Saxon.Hej.lib.JJ_ResourceRequest rr = new Saxon.Hej.lib.JJ_ResourceRequest();
            rr.relativeUri = relativeUri;
            rr.baseUri = baseUri;
            rr.uri = uri;
            rr.uriIsNamespace = uriIsNamespace;
            rr.publicId = publicId;
            rr.purpose = purpose;
            rr.nature = nature;
            rr.entityName = entityName;
            rr.streamable = streamable;
            rr.requestedEncoding = requestedEncoding;
            return rr;
        }
        public Saxon.Ejavax.xml.transform.Source resolve(params Saxon.Hej.lib.JJ_ResourceResolver[]  resolvers) {
            string requestedUri = relativeUri;
            if ((requestedUri == null)) {
                requestedUri = uri;
            }
            string id = null;
            Saxon.Hej.lib.JJ_ResourceRequest adjustedRequest = this;
            int hash = requestedUri.IndexOf('#');
            if ((hash>=0)) {
                adjustedRequest = copy();
                adjustedRequest.relativeUri = requestedUri.Substring(0, hash - 0);
                id = requestedUri.Substring((hash+1));
            }
            Saxon.Ejavax.xml.transform.Source resolvedSource = null;
            foreach (Saxon.Hej.lib.JJ_ResourceResolver resolver in resolvers) {
                if ((resolver != null)) {
                    Saxon.Ejavax.xml.transform.Source s = null;
                    try {
                        s = resolver.resolve(this);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        System.Exception cause = e.InnerException;
                        if ((cause is Saxon.Ejava.lang.IllegalArgumentException)) {
                            Saxon.Ejava.lang.IllegalArgumentException iae = (Saxon.Ejava.lang.IllegalArgumentException)(cause);
                            if ((iae.InnerException is System.UriFormatException)) {
                                throw new Saxon.Hej.trans.XPathException(("Invalid URI "+uri), iae.InnerException);
                            }
                        } else {
                            throw;
                        }
                    }
                    if ((s != null)) {
                        resolvedSource = s;
                        break ;
                    }
                }
            }
            if ((resolvedSource != null)) {
                if ((id != null)) {
                    string idFinal = id;
                    Saxon.Hej.@event.FilterFactory factory = next => new Saxon.Hej.@event.IDFilter(next, idFinal);
                    resolvedSource = Saxon.Hej.lib.AugmentedSource.makeAugmentedSource(resolvedSource);
                    ((Saxon.Hej.lib.AugmentedSource)(resolvedSource)).addFilter(factory);
                }
            }
            return resolvedSource;
        }
    
    }
}