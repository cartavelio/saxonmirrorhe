using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class ResourceResolverWrappingURIResolver : Saxon.Hej.lib.JJ_ResourceResolver {
        private readonly Saxon.Ejavax.xml.transform.URIResolver uriResolver;
        public ResourceResolverWrappingURIResolver(Saxon.Ejavax.xml.transform.URIResolver uriResolver)  {
            Saxon.Ejava.util.Objects.requireNonNull(uriResolver);
            this.uriResolver = uriResolver;
        }
        public Saxon.Ejavax.xml.transform.Source resolve(Saxon.Hej.lib.JJ_ResourceRequest request) {
            if (((request.relativeUri != null) && (request.baseUri != null))) {
                try {
                    return uriResolver.resolve(request.relativeUri, request.baseUri);
                } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
            }
            if ((request.uri != null)) {
                try {
                    return uriResolver.resolve(request.uri, request.baseUri);
                } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
            }
            return null;
        }
        public Saxon.Ejavax.xml.transform.URIResolver getWrappedURIResolver() {
            return uriResolver;
        }
    
    }
}