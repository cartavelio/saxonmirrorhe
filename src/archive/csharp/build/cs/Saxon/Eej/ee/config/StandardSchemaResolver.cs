using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.config {
    public class StandardSchemaResolver : Saxon.Hej.lib.SchemaURIResolver {
        public const string SCHEMA_NATURE = "http://www.w3.org/2001/XMLSchema";
        public const string VALIDATION_PURPOSE = "http://www.rddl.org/purposes#validation";
        private Saxon.Eej.config.EnterpriseConfiguration config;
        private static readonly Saxon.Ejavax.xml.transform.stream.StreamSource[] EMPTY_SOURCE_ARRAY = new Saxon.Ejavax.xml.transform.stream.StreamSource[0];
        private Saxon.Hej.lib.JJ_ResourceResolver resourceResolver;
        public StandardSchemaResolver(Saxon.Eej.config.EnterpriseConfiguration config)  {
            setConfiguration(config);
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = (Saxon.Eej.config.EnterpriseConfiguration)(config);
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        internal  /* REPLACING public */ void setResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            this.resourceResolver = resolver;
        }
        public Saxon.Ejavax.xml.transform.Source[] resolve(string targetNamespace, string baseURI, string[] locations) {
            if ((config == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("No configuration supplied to schema URI resolver");
            }
            if ((((targetNamespace != null) && config.isSchemaAvailable(targetNamespace)) && !(config.getConfigurationProperty(Saxon.Hej.lib.Feature<bool>.MULTIPLE_SCHEMA_IMPORTS)))) {
                return EMPTY_SOURCE_ARRAY;
            }
            if (Saxon.Hej.lib.NamespaceConstant.FN.Equals(targetNamespace)) {
                return new Saxon.Ejavax.xml.transform.Source[]{fetchResource("xpath-functions.scm", config)};
            }
            if (Saxon.Hej.lib.NamespaceConstant.XLINK.Equals(targetNamespace)) {
                locations = new string[]{"https://www.w3.org/1999/xlink.xsd"};
            }
            if (((locations == null) || (locations.Length == 0))) {
                return EMPTY_SOURCE_ARRAY;
            }
            try {
                Saxon.Ejavax.xml.transform.Source[] sources = new Saxon.Ejavax.xml.transform.Source[locations.Length];
                for (int i = 0; (i<locations.Length); i++) {
                    sources[i] = resolveLocationHint(baseURI, locations[i]);
                }
                return sources;
            } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                throw Saxon.Hej.trans.XPathException.makeXPathException(e);
            }
        }
        private Saxon.Ejavax.xml.transform.Source resolveLocationHint(string baseURI, string location) {
            Saxon.Hej.lib.JJ_ResourceRequest rr = new Saxon.Hej.lib.JJ_ResourceRequest();
            rr.relativeUri = location;
            rr.baseUri = baseURI;
            rr.nature = Saxon.Hej.lib.JJ_ResourceRequest.XSD_NATURE;
            rr.purpose = "schema module";
            try {
                rr.uri = Saxon.Hej.functions.ResolveURI.makeAbsolute(location, baseURI).ToString();
            } catch (System.UriFormatException err) {
                throw new Saxon.Hej.trans.XPathException(("Unable to reserve schemaLocation URI "+location), err);
            }
            return rr.resolve(config.getResourceResolver(), new Saxon.Callbacks.DirectResourceResolver(config));
        }
        private Saxon.Ejavax.xml.transform.Source resolveNamespaceURI(string moduleURI, string baseURI) {
            Saxon.Hej.lib.JJ_ResourceRequest rr = new Saxon.Hej.lib.JJ_ResourceRequest();
            rr.uri = moduleURI;
            rr.baseUri = baseURI;
            rr.uriIsNamespace = true;
            rr.nature = Saxon.Hej.lib.JJ_ResourceRequest.XSD_NATURE;
            rr.purpose = "schema module";
            try {
                return rr.resolve(config.getResourceResolver());
            } catch (Saxon.Ejavax.xml.transform.TransformerException ex) {
            }
            return null;
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