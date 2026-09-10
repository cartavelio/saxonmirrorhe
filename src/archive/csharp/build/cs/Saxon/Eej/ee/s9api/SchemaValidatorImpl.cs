using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.s9api {
    internal class SchemaValidatorImpl : Saxon.Hej.s9api.SchemaValidator {
        private readonly Saxon.Hej.s9api.Processor processor;
        private bool lax;
        private Saxon.Hej.lib.InvalidityHandler invalidityHandler;
        private Saxon.Hej.s9api.Destination destination;
        private Saxon.Api.QName documentElementName;
        private Saxon.Hej.type.SchemaType documentElementType;
        private bool expandAttributeDefaults = true;
        private bool useXsiSchemaLocation;
        private bool keepStatistics;
        private readonly Saxon.Hej.expr.instruct.GlobalParameterSet suppliedParams = new Saxon.Hej.expr.instruct.GlobalParameterSet();
        private int threadCount = Saxon.Ejava.lang.Runtime.getRuntime().availableProcessors();
        public SchemaValidatorImpl(Saxon.Hej.s9api.Processor processor)  {
            this.processor = processor;
            this.useXsiSchemaLocation = processor.getConfigurationProperty(Saxon.Hej.lib.Feature<bool>.USE_XSI_SCHEMA_LOCATION);
            this.invalidityHandler = new Saxon.Hej.lib.InvalidityHandlerWrappingErrorReporter(processor.getUnderlyingConfiguration().makeErrorReporter());
        }
        public override void setLax(bool lax) {
            this.lax = lax;
        }
        public override bool isLax() {
            return lax;
        }
        public override void setValidityReporting(Saxon.Hej.s9api.Destination destination) {
            invalidityHandler = processor.getUnderlyingConfiguration().createValidityReporter();
            ((Saxon.Eej.ee.validate.InvalidityReportGeneratorEE)(invalidityHandler)).setDestination(destination);
        }
        public override Saxon.Hej.lib.InvalidityHandler getInvalidityHandler() {
            return invalidityHandler;
        }
        public override void setInvalidityHandler(Saxon.Hej.lib.InvalidityHandler handler) {
            this.invalidityHandler = handler;
        }
        public override void setCollectStatistics(bool collect) {
        }
        public override bool isCollectStatistics() {
            return this.keepStatistics;
        }
        public override void reportValidationStatistics(Saxon.Hej.s9api.Destination destination) {
        }
        public override void setUseXsiSchemaLocation(bool recognize) {
            useXsiSchemaLocation = recognize;
        }
        public override bool isUseXsiSchemaLocation() {
            return useXsiSchemaLocation;
        }
        public override void setDestination(Saxon.Hej.s9api.Destination destination) {
            this.destination = destination;
        }
        public override Saxon.Hej.s9api.Destination getDestination() {
            return destination;
        }
        public override void setDocumentElementName(Saxon.Api.QName name) {
            documentElementName = name;
        }
        public override Saxon.Api.QName getDocumentElementName() {
            return documentElementName;
        }
        public override void setDocumentElementTypeName(Saxon.Api.QName name) {
            Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
            documentElementType = config.getSchemaType(name.GetStructuredQName());
            if ((documentElementType == null)) {
                throw new Saxon.Api.SaxonApiException(("Unknown type "+name.ClarkName));
            }
        }
        public override Saxon.Api.QName getDocumentElementTypeName() {
            if ((documentElementType == null)) {
                return null;
            } else {
                return new Saxon.Api.QName(documentElementType.getStructuredQName());
            }
        }
        protected internal override Saxon.Hej.type.SchemaType getDocumentElementType() {
            return documentElementType;
        }
        public override void setExpandAttributeDefaults(bool expand) {
            expandAttributeDefaults = expand;
        }
        public override bool isExpandAttributeDefaults() {
            return expandAttributeDefaults;
        }
        public override void setParameter(Saxon.Api.QName name, Saxon.Api.XdmValue value) {
            try {
                suppliedParams.put(name.GetStructuredQName(), ((value == null) ? null : ((Saxon.Hej.om.Sequence)(((Saxon.Hej.om.GroundedValue)value.Unwrap()))).materialize()));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public override Saxon.Api.XdmValue getParameter(Saxon.Api.QName name) {
            Saxon.Hej.om.GroundedValue value = suppliedParams.get(name.GetStructuredQName());
            return ((value == null) ? null : Saxon.Api.XdmValue.Wrap(value));
        }
        public override Saxon.Hej.type.ValidationParams getValidationParameters() {
            try {
                return convertParams(suppliedParams, processor.getUnderlyingConfiguration().getDeclaredSchemaParameters());
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void setThreadCount(int tc) {
            threadCount = tc;
        }
        public override void validateMultiple(System.Collections.Generic.IEnumerable<Saxon.Ejavax.xml.transform.Source> sources) {
            Saxon.Eej.config.EnterpriseConfiguration config = (Saxon.Eej.config.EnterpriseConfiguration)(processor.getUnderlyingConfiguration());
            if (!(config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_MULTITHREADING))) {
                threadCount = 1;
            }
            if ((threadCount<1)) {
                threadCount = 1;
            }
            if ((destination != null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Cannot validate multiple documents when destination is set externally");
            }
            Saxon.Hej.s9api.Destination reportDestination = null;
            Saxon.Eej.ee.s9api.SchemaValidatorImpl.ReportList reportDocs = new Saxon.Eej.ee.s9api.SchemaValidatorImpl.ReportList();
            if ((invalidityHandler is Saxon.Eej.ee.validate.InvalidityReportGeneratorEE)) {
                reportDestination = ((Saxon.Eej.ee.validate.InvalidityReportGeneratorEE)(invalidityHandler)).getDestination();
            }
            doParallelValidation(sources, config, reportDocs);
            Saxon.Hej.s9api.Location loc = Saxon.Hej.expr.parser.Loc.NONE;
            if ((reportDestination != null)) {
                Saxon.Hej.@event.Receiver receiver = reportDestination.getReceiver(config.makePipelineConfiguration(), config.obtainDefaultSerializationProperties());
                try {
                    receiver.open();
                    receiver.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                    receiver.startElement(new Saxon.Hej.om.FingerprintedQName("", Saxon.Hej.lib.InvalidityReportGenerator.REPORT_NS, "validation-reports"), Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.om.EmptyAttributeMap.getInstance(), Saxon.Hej.om.NamespaceMap.of("", Saxon.Hej.lib.InvalidityReportGenerator.REPORT_NS), loc, Saxon.Hej.@event.ReceiverOption.NONE);
                    foreach (Saxon.Api.XdmNode node in reportDocs) {
                        foreach (Saxon.Api.XdmNode de in node.Children()) {
                            ((Saxon.Hej.om.NodeInfo)de.Unwrap()).copy(receiver, 0, loc);
                        }
                    }
                    receiver.endElement();
                    receiver.endDocument();
                    receiver.close();
                } catch (Saxon.Hej.trans.XPathException e) {
                    System.Console.Error.WriteLine(e);
                }
            }
        }
        private void doParallelValidation(System.Collections.Generic.IEnumerable<Saxon.Ejavax.xml.transform.Source> sources, Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Eej.ee.s9api.SchemaValidatorImpl.ReportList reportDocs) {
            Saxon.Impl.Parallel.Validation.ValidateMultiple(processor, sources, invalidityHandler, reportDocs);
        }
        public class ReportList : System.Collections.Generic.IEnumerable<Saxon.Api.XdmNode> {
            private readonly System.Collections.Generic.IList<Saxon.Api.XdmNode> backing = new System.Collections.Generic.List<Saxon.Api.XdmNode>();
            public void add(Saxon.Api.XdmNode node) { lock(this) {
                backing.Add(node);
            }}
            public System.Collections.Generic.IEnumerator<Saxon.Api.XdmNode> GetEnumerator() {
                return backing.GetEnumerator();
            }
            // Extra method to make the class IEnumerable as well as IEnumerable<T>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        
        }
        public override void validate(Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
            Saxon.Hej.@event.Receiver receiver = getReceiver(config, source.getSystemId());
            try {
                if ((invalidityHandler is Saxon.Hej.lib.InvalidityReportGenerator)) {
                    ((Saxon.Hej.lib.InvalidityReportGenerator)(invalidityHandler)).setXsdVersion(((config.getXsdVersion() == Saxon.Hej.Configuration.XSD11) ? "1.1" : "1.0"));
                }
                invalidityHandler.startReporting(source.getSystemId());
                Saxon.Hej.@event.Sender.send(source, receiver, new Saxon.Hej.lib.ParseOptions());
            } catch (Saxon.Hej.trans.QuitParsingException qpe) {
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            } finally {
                if ((invalidityHandler != null)) {
                    try {
                        invalidityHandler.endReporting();
                    } catch (Saxon.Hej.trans.XPathException e1) {
                    }
                }
            }
        }
        protected internal Saxon.Hej.type.ValidationParams convertParams(Saxon.Hej.expr.instruct.GlobalParameterSet suppliedParams, System.Collections.Generic.ICollection<Saxon.Hej.expr.instruct.GlobalParam> declaredParams) {
            Saxon.Hej.type.ValidationParams vp = new Saxon.Hej.type.ValidationParams();
            Saxon.Hej.expr.XPathContext context = new Saxon.Hej.Controller(processor.getUnderlyingConfiguration()).newXPathContext();
            foreach (Saxon.Hej.expr.instruct.GlobalParam cp in declaredParams) {
                Saxon.Hej.om.GroundedValue value = suppliedParams.convertParameterValue(cp.getVariableQName(), cp.getRequiredType(), true, context);
                if ((value != null)) {
                    Saxon.Impl.Helpers.MapUtils.Put(vp, cp.getVariableQName(), value);
                }
            }
            return vp;
        }
        public override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            return getReceiver(pipe.getConfiguration(), null);
        }
        private Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.Configuration config, string systemId) {
            Saxon.Hej.Controller controller = new Saxon.Hej.Controller(config);
            Saxon.Hej.expr.instruct.Executable exec = controller.getExecutable();
            Saxon.Hej.expr.PackageData pack = new Saxon.Hej.expr.PackageData(config);
            pack.setLocalLicenseId(((Saxon.Eej.config.EnterpriseConfiguration)(config)).getSuperSchema().getLocalLicenseId());
            pack.setSchemaAware(true);
            exec.setTopLevelPackage(pack);
            exec.setSchemaAware(true);
            Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
            pipe.setUseXsiSchemaLocation(useXsiSchemaLocation);
            pipe.setHostLanguage(Saxon.Api.HostLanguage.XML_SCHEMA);
            Saxon.Hej.lib.ParseOptions options = pipe.getParseOptions().withCheckEntityReferences(true).withExpandAttributeDefaults(expandAttributeDefaults).withSchemaValidationMode((lax ? Saxon.Hej.lib.Validation.LAX : Saxon.Hej.lib.Validation.STRICT)).withSpaceStrippingRule(Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance()).withTopLevelType(documentElementType).withInvalidityHandler(invalidityHandler);
            if ((documentElementName != null)) {
                options = options.withTopLevelElement(new Saxon.Hej.om.StructuredQName(documentElementName.Prefix, documentElementName.NamespaceUri, documentElementName.LocalName));
            }
            try {
                options = options.withValidationParams(convertParams(suppliedParams, config.getDeclaredSchemaParameters()));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
            options = options.withCheckEntityReferences(true);
            pipe.setParseOptions(options);
            Saxon.Hej.@event.Receiver output = ((destination == null) ? new Saxon.Hej.@event.Sink(pipe) : destination.getReceiver(pipe, config.obtainDefaultSerializationProperties()));
            output.setPipelineConfiguration(pipe);
            return config.getDocumentValidator(output, systemId, options, null);
        }
        public override void close() {
            if ((destination != null)) {
                destination.close();
                destination = null;
            }
        }
    
    }
}