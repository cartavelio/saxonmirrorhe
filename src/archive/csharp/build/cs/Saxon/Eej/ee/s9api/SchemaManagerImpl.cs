using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.s9api {
    internal class SchemaManagerImpl : Saxon.Hej.s9api.SchemaManager {
        private readonly Saxon.Hej.s9api.Processor processor;
        private Saxon.Hej.lib.ErrorReporter errorReporter;
        public SchemaManagerImpl(Saxon.Hej.s9api.Processor processor)  {
            this.processor = processor;
            errorReporter = processor.getUnderlyingConfiguration().makeErrorReporter();
            if ((errorReporter is Saxon.Hej.lib.StandardErrorReporter)) {
                ((Saxon.Hej.lib.StandardErrorReporter)(errorReporter)).setOutputErrorCodes(false);
            }
        }
        public override void setXsdVersion(string version) {
            if ((version.Equals("1.0") || version.Equals("1.1"))) {
                processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.XSD_VERSION, version);
            } else {
                throw new Saxon.Ejava.lang.IllegalArgumentException("XsdVersion");
            }
        }
        public override string getXsdVersion() {
            return ((processor.getUnderlyingConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10) ? "1.0" : "1.1");
        }
        public override void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            this.errorReporter = reporter;
        }
        public override Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return errorReporter;
        }
        public override void setSchemaURIResolver(Saxon.Hej.lib.SchemaURIResolver resolver) {
            ((Saxon.Eej.config.EnterpriseConfiguration)(processor.getUnderlyingConfiguration())).setSchemaURIResolver(resolver);
        }
        public override Saxon.Hej.lib.SchemaURIResolver getSchemaURIResolver() {
            return ((Saxon.Eej.config.EnterpriseConfiguration)(processor.getUnderlyingConfiguration())).getSchemaURIResolver();
        }
        public override void load(Saxon.Ejavax.xml.transform.Source source) {
            try {
                if ((source is Saxon.Ejavax.xml.transform.stream.StreamSource)) {
                    if ((((((Saxon.Ejavax.xml.transform.stream.StreamSource)(source)).getInputStream() == null) && (((Saxon.Ejavax.xml.transform.stream.StreamSource)(source)).getReader() == null)) && (source.getSystemId() != null))) {
                        Saxon.Hej.lib.JJ_ResourceRequest request = new Saxon.Hej.lib.JJ_ResourceRequest();
                        request.uri = source.getSystemId();
                        request.nature = Saxon.Hej.lib.JJ_ResourceRequest.XSD_NATURE;
                        Saxon.Ejavax.xml.transform.Source s2 = processor.getUnderlyingConfiguration().getResourceResolver().resolve(request);
                        if ((s2 != null)) {
                            source = s2;
                        }
                    }
                }
                processor.getUnderlyingConfiguration().addSchemaSource(source, errorReporter);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public override void importComponents(Saxon.Ejavax.xml.transform.Source source) {
            try {
                processor.getUnderlyingConfiguration().importComponents(source);
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Api.SaxonApiException(err);
            }
        }
        public override void exportComponents(Saxon.Hej.s9api.Destination destination) {
            try {
                Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
                if (config.getConfigurationProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_UNRESOLVED_SCHEMA_COMPONENTS)) {
                    throw new Saxon.Api.SaxonApiException("Exporting to an SCM file is not possible when unresolved references to schema components have been permitted");
                }
                Saxon.Hej.@event.Receiver @out = destination.getReceiver(config.makePipelineConfiguration(), config.obtainDefaultSerializationProperties());
                config.exportComponents(@out);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public override Saxon.Hej.s9api.SchemaValidator newSchemaValidator() {
            return new Saxon.Eej.ee.s9api.SchemaValidatorImpl(processor);
        }
    
    }
}