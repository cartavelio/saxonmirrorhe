using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class Processor : Saxon.Hej.Configuration.ApiProvider {
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.s9api.SchemaManager schemaManager;
        public Processor() : this (Saxon.Hej.Configuration.newLicensedConfiguration()) {
        }
        public Processor(bool licensedEdition)  {
            if (licensedEdition) {
                config = Saxon.Hej.Configuration.newConfiguration();
                if (config.getEditionCode().Equals("EE")) {
                    schemaManager = makeSchemaManager();
                }
            } else {
                config = new Saxon.Hej.Configuration();
            }
            config.setProcessor(this);
        }
        public Processor(Saxon.Hej.Configuration config)  {
            this.config = config;
            if (config.getEditionCode().Equals("EE")) {
                schemaManager = makeSchemaManager();
            }
        }
        public Processor(Saxon.Ejavax.xml.transform.Source source)  {
            try {
                config = Saxon.Hej.Configuration.readConfiguration(source);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
            config.setProcessor(this);
        }
        public Saxon.Hej.s9api.DocumentBuilder newDocumentBuilder() {
            return new Saxon.Hej.s9api.DocumentBuilder(config);
        }
        public Saxon.Hej.s9api.XPathCompiler newXPathCompiler() {
            return new Saxon.Hej.s9api.XPathCompiler(this);
        }
        public Saxon.Hej.s9api.XsltCompiler newXsltCompiler() {
            return new Saxon.Hej.s9api.XsltCompiler(this);
        }
        public Saxon.Hej.s9api.XQueryCompiler newXQueryCompiler() {
            return new Saxon.Hej.s9api.XQueryCompiler(this);
        }
        public Saxon.Hej.s9api.Serializer newSerializer() {
            return new Saxon.Hej.s9api.Serializer(this);
        }
        public Saxon.Hej.s9api.Serializer newSerializer(System.IO.Stream stream) {
            Saxon.Hej.s9api.Serializer s = new Saxon.Hej.s9api.Serializer(this);
            s.setOutputStream(stream);
            return s;
        }
        public Saxon.Hej.s9api.Serializer newSerializer(System.IO.TextWriter writer) {
            Saxon.Hej.s9api.Serializer s = new Saxon.Hej.s9api.Serializer(this);
            s.setOutputWriter(writer);
            return s;
        }
        public Saxon.Hej.s9api.Serializer newSerializer(Saxon.Ejava.io.File file) {
            Saxon.Hej.s9api.Serializer s = new Saxon.Hej.s9api.Serializer(this);
            s.setOutputFile(file);
            return s;
        }
        public Saxon.Hej.s9api.SchemaManager getSchemaManager() {
            return schemaManager;
        }
        public bool isSchemaAware() {
            return config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION);
        }
        public string getSaxonProductVersion() {
            return Saxon.Hej.Version.getProductVersion();
        }
        public string getSaxonEdition() {
            return config.getEditionCode();
        }
        public void setXmlVersion(string version) {
            switch (version) {
              case "1.0":
                config.setXMLVersion(Saxon.Hej.Configuration.XML10);
                break ;
              case "1.1":
                config.setXMLVersion(Saxon.Hej.Configuration.XML11);
                break ;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("XmlVersion");
            }
        }
        public string getXmlVersion() {
            if ((config.getXMLVersion() == Saxon.Hej.Configuration.XML10)) {
                return "1.0";
            } else {
                return "1.1";
            }
        }
        public void setConfigurationProperty(string name, System.Object value) {
            if (name.Equals(Saxon.Hej.lib.FeatureKeys.CONFIGURATION)) {
                config = (Saxon.Hej.Configuration)(value);
            } else {
                config.setConfigurationProperty(name, value);
            }
        }
        public System.Object getConfigurationProperty(string name) {
            return config.getConfigurationProperty(name);
        }
        public void setConfigurationProperty<T> (Saxon.Hej.lib.Feature<T> feature, T value) {
                 if ((feature.code==Saxon.Hej.lib.FeatureCode.CONFIGURATION)) {         config = (Saxon.Hej.Configuration)(object)value;     } else {
         config.setConfigurationProperty(feature, value);     }
        }
        public T getConfigurationProperty<T> (Saxon.Hej.lib.Feature<T> feature) {
            return config.getConfigurationProperty(feature);
        }
        public void declareCollation(string uri, System.Collections.Generic.Comparer<string> collation) {
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Cannot redeclare the Unicode codepoint collation URI");
            }
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.HTML5_CASE_BLIND_COLLATION_URI)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Cannot redeclare the HTML5 caseblind collation URI");
            }
            Saxon.Hej.lib.StringCollator saxonCollation = makeStringCollator(uri, collation);
            config.registerCollation(uri, saxonCollation);
        }
        private static Saxon.Hej.lib.StringCollator makeStringCollator(string uri, System.Collections.Generic.Comparer<string> collation) {
            return new Saxon.Hej.expr.sort.SimpleCollation(uri, collation);
        }
        public void registerCollection(string collectionURI, Saxon.Hej.lib.ResourceCollection collection) {
            config.registerCollection(collectionURI, collection);
        }
        public void setCatalogFiles(params string[]  fileNames) {
            if ((config.getResourceResolver() is Saxon.Callbacks.ConfigurableResourceResolver)) {
                Saxon.Hej.trans.CommandLineOptions.setCatalogFiles(((Saxon.Callbacks.ConfigurableResourceResolver)(config.getResourceResolver())), Saxon.Ejava.util.Arrays.asList(fileNames));
            }
        }
        public Saxon.Hej.Configuration getUnderlyingConfiguration() {
            return config;
        }
        public void writeXdmValue(Saxon.Api.XdmValue value, Saxon.Hej.s9api.Destination destination) {
            Saxon.Ejava.util.Objects.requireNonNull(value);
            Saxon.Ejava.util.Objects.requireNonNull(destination);
            try {
                if ((destination is Saxon.Hej.s9api.Serializer)) {
                    ((Saxon.Hej.s9api.Serializer)(destination)).serializeXdmValue(value);
                } else {
                    Saxon.Hej.@event.Receiver @out = destination.getReceiver(config.makePipelineConfiguration(), config.obtainDefaultSerializationProperties());
                    Saxon.Hej.@event.ComplexContentOutputter tree = new Saxon.Hej.@event.ComplexContentOutputter(@out);
                    tree.open();
                    tree.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                    foreach (Saxon.Api.XdmItem item in value) {
                        tree.append(((Saxon.Hej.om.Item)item.Unwrap()), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                    }
                    tree.endDocument();
                    tree.close();
                    destination.closeAndNotify();
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Api.SaxonApiException(err);
            }
        }
        private Saxon.Hej.s9api.SchemaManager makeSchemaManager() {
            Saxon.Hej.s9api.SchemaManager manager = null;
            manager = new Saxon.Eej.ee.s9api.SchemaManagerImpl(this);
            return manager;
        }
    
    }
}