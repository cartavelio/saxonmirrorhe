using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StandardModuleURIResolver : Saxon.Hej.lib.ModuleURIResolver {
        internal Saxon.Hej.Configuration config = null;
        public StandardModuleURIResolver()  {
        }
        public StandardModuleURIResolver(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            if ((this.config == null)) {
                this.config = config;
            }
        }
        public Saxon.Ejavax.xml.transform.stream.StreamSource[] resolve(string moduleURI, string baseURI, string[] locations) {
            if ((config == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("No Configuration available in StandardModuleResolver");
            }
            Saxon.Ejavax.xml.transform.stream.StreamSource source = resolveModuleURI(moduleURI, baseURI);
            if ((source != null)) {
                return new Saxon.Ejavax.xml.transform.stream.StreamSource[]{source};
            }
            if ((locations.Length == 0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot locate module for namespace "+moduleURI));
                err.setErrorCode("XQST0059");
                err.setIsStaticError(true);
                throw err;
            }
            System.Collections.Generic.IList<Saxon.Ejavax.xml.transform.stream.StreamSource> moduleSources = new System.Collections.Generic.List<Saxon.Ejavax.xml.transform.stream.StreamSource>();
            foreach (string hint in locations) {
                Saxon.Ejavax.xml.transform.stream.StreamSource ss = resolveLocationHint(baseURI, hint);
                if ((ss != null)) {
                    moduleSources.Add(ss);
                }
            }
            return moduleSources.ToArray();
        }
        protected internal Saxon.Ejavax.xml.transform.stream.StreamSource resolveModuleURI(string moduleURI, string baseURI) {
            try {
                Saxon.Ejavax.xml.transform.Source source = null;
                if ((config != null)) {
                    Saxon.Hej.lib.JJ_ResourceRequest rr = new Saxon.Hej.lib.JJ_ResourceRequest();
                    rr.uri = moduleURI;
                    rr.uriIsNamespace = true;
                    rr.baseUri = baseURI;
                    rr.nature = Saxon.Hej.lib.JJ_ResourceRequest.XQUERY_NATURE;
                    source = config.getResourceResolver().resolve(rr);
                    if (((source == null) && (baseURI != null))) {
                        rr.uri = new System.Uri(new System.Uri(baseURI, System.UriKind.RelativeOrAbsolute), moduleURI).ToString();
                        rr.uriIsNamespace = false;
                        source = config.getResourceResolver().resolve(rr);
                    }
                }
                if ((source != null)) {
                    return toStreamSource(source);
                }
            } catch (Exception ex) when (ex is Saxon.Ejavax.xml.transform.TransformerException || ex is System.UriFormatException) {
            }
            return null;
        }
        private Saxon.Ejavax.xml.transform.stream.StreamSource toStreamSource(Saxon.Ejavax.xml.transform.Source src) {
            return (Saxon.Ejavax.xml.transform.stream.StreamSource)(src);
        }
        protected internal Saxon.Ejavax.xml.transform.stream.StreamSource resolveLocationHint(string baseURI, string locationHint) {
            Saxon.Hej.lib.JJ_ResourceRequest rr = new Saxon.Hej.lib.JJ_ResourceRequest();
            rr.baseUri = baseURI;
            rr.relativeUri = locationHint;
            rr.nature = Saxon.Hej.lib.JJ_ResourceRequest.XQUERY_NATURE;
            try {
                rr.uri = Saxon.Hej.functions.ResolveURI.makeAbsolute(rr.relativeUri, baseURI).ToString();
                return (Saxon.Ejavax.xml.transform.stream.StreamSource)(rr.resolve(config.getResourceResolver(), new Saxon.Callbacks.DirectResourceResolver(config)));
            } catch (System.UriFormatException err) {
                Saxon.Hej.trans.XPathException se = new Saxon.Hej.trans.XPathException(("Cannot resolve relative URI "+rr.relativeUri), err);
                se.setErrorCode("XQST0059");
                se.setIsStaticError(true);
                throw se;
            }
        }
    
    }
}