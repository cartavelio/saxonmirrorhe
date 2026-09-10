using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.config {
    public class StandardSchemaResolver : Saxon.Hej.lib.SchemaURIResolver {
        private Saxon.Eej.config.EnterpriseConfiguration config;
        private static readonly Saxon.Ejavax.xml.transform.stream.StreamSource[] EMPTY_SOURCE_ARRAY = new Saxon.Ejavax.xml.transform.stream.StreamSource[0];
        private Saxon.Ejavax.xml.transform.URIResolver uriResolver;
        public StandardSchemaResolver()  {
        }
        public StandardSchemaResolver(Saxon.Eej.config.EnterpriseConfiguration config)  {
            setConfiguration(config);
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = (Saxon.Eej.config.EnterpriseConfiguration)(config);
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void setURIResolver(Saxon.Ejavax.xml.transform.URIResolver resolver) {
            this.uriResolver = resolver;
        }
        public Saxon.Ejavax.xml.transform.Source[] resolve(string targetNamespace, string baseURI, string[] locations) {
            if ((config == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("No configuration supplied to schema URI resolver");
            }
            if ((((targetNamespace != null) && config.isSchemaAvailable(targetNamespace)) && !(config.getConfigurationProperty(Saxon.Hej.lib.Feature<bool>.MULTIPLE_SCHEMA_IMPORTS)))) {
                return EMPTY_SOURCE_ARRAY;
            }
            if (Saxon.Hej.lib.NamespaceConstant.XLINK.Equals(targetNamespace)) {
                return new Saxon.Ejavax.xml.transform.Source[]{fetchResource("w3c/xlink/xlink.xsd", config)};
            } else {
                if (Saxon.Hej.lib.NamespaceConstant.FN.Equals(targetNamespace)) {
                    return new Saxon.Ejavax.xml.transform.Source[]{fetchResource("xpath-functions.scm", config)};
                } else {
                    try {
                        Saxon.Ejavax.xml.transform.Source[] sources = new Saxon.Ejavax.xml.transform.Source[locations.Length];
                        for (int i = 0; (i<locations.Length); i++) {
                            if ((uriResolver == null)) {
                                uriResolver = config.getURIResolver();
                            }
                            if ((uriResolver != null)) {
                                sources[i] = uriResolver.resolve(locations[i], baseURI);
                            }
                            if ((sources[i] == null)) {
                                sources[i] = config.getSystemURIResolver().resolve(locations[i], baseURI);
                            }
                        }
                        return sources;
                    } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                        throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                    }
                }
            }
        }
        public static Saxon.Ejavax.xml.transform.Source fetchResource(string pathName, Saxon.Hej.Configuration config) {
            System.IO.Stream inputStream = Saxon.Hej.Version.platform.locateResource(pathName, new System.Collections.Generic.List<string>());
            if ((inputStream == null)) {
                throw new Saxon.Ejava.lang.AssertionError((("Failed to load "+pathName)+" from the Saxon library"));
            }
            if (config.isTiming()) {
                config.getLogger().info(("Loading internal schema: "+pathName));
            }
            return new Saxon.Ejavax.xml.transform.stream.StreamSource(inputStream);
        }
    
    }
}