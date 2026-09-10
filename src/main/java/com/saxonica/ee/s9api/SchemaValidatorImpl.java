package com.saxonica.ee.s9api;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.trans.Outcome;
import com.saxonica.ee.validate.InvalidityReportGeneratorEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.expr.instruct.GlobalParameterSet;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.Action;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.type.ValidationParams;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.Source;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.concurrent.*;

/**
 * A <tt>SchemaValidator</tt> is an object that is used for validating instance documents against a schema.
 * The schema consists of the collection of schema components that are available within the schema
 * cache maintained by the SchemaManager, together with any additional schema components located
 * during the course of validation by means of an xsl:schemaLocation or xsi:noNamespaceSchemaLocation
 * attribute within the instance document.
 * <p>If validation fails, an exception is thrown. If validation succeeds, the validated document
 * can optionally be written to a specified destination. This will be a copy of the original document,
 * augmented with default values for absent elements and attributes, and carrying type annotations
 * derived from the schema processing. Expansion of defaults can be suppressed by means of the method
 * {@link #setExpandAttributeDefaults(boolean)}.</p>
 * <p>A <tt>SchemaValidator</tt> is serially reusable but not thread-safe. That is, it should normally
 * be used in the thread where it is created, but it can be used more than once, to validate multiple
 * input documents.</p>
 * <p>A <tt>SchemaValidator</tt> is a <tt>Destination</tt>, which allows it to receive the output of a
 * query or transformation to be validated.</p>
 * <p>Saxon does not deliver the full PSVI as described in the XML schema specifications,
 * only the subset of the PSVI properties featured in the XDM data model.</p>
 */

@CSharpModifiers(code={"internal"})
public class SchemaValidatorImpl extends SchemaValidator {

    private final Processor processor;
    private boolean lax;
    private InvalidityHandler invalidityHandler;
    private Destination destination;
    private QName documentElementName;
    private SchemaType documentElementType;
    private boolean expandAttributeDefaults = true;
    private boolean useXsiSchemaLocation;
    private boolean keepStatistics;
    //#if CSHARP==false
    private ValidationStatistics latestStatistics = null;
    //#endif
    private final GlobalParameterSet suppliedParams = new GlobalParameterSet();
    private int threadCount = Runtime.getRuntime().availableProcessors();

    @CSharpModifiers(code = {"internal"})
    public SchemaValidatorImpl(Processor processor) {
        this.processor = processor;
        this.useXsiSchemaLocation = processor.getConfigurationProperty(Feature.USE_XSI_SCHEMA_LOCATION);
        this.invalidityHandler = new InvalidityHandlerWrappingErrorReporter(
                processor.getUnderlyingConfiguration().makeErrorReporter());
    }

    /**
     * The validation mode may be either strict or lax. The default is strict; this method may be called
     * to indicate that lax validation is required. With strict validation, validation fails if no element
     * declaration can be located for the outermost element. With lax validation, the absence of an
     * element declaration results in the content being considered valid.
     *
     * @param lax true if validation is to be lax, false if it is to be strict
     */

    @Override
    public void setLax(boolean lax) {
        this.lax = lax;
    }

    /**
     * Ask whether validation is to be in lax mode.
     *
     * @return true if validation is to be in lax mode, false if it is to be in strict mode.
     */

    @Override
    public boolean isLax() {
        return lax;
    }

    //#if CSHARP==false
    /**
     * Set the ErrorListener to be used while validating instance documents.
     *
     * @param listener The error listener to be used. This is notified of all errors detected during the
     *                 validation episode.
     */

    @Override
    public void setErrorListener(ErrorListener listener) {
        this.invalidityHandler = new InvalidityHandlerWrappingErrorListener(listener);
    }



    /**
     * Get the ErrorListener being used while validating instance documents
     *
     * @return listener The error listener in use. This is notified of all errors detected during the
     *         validation episode. Returns null if the invalidity handler is not one that wraps
     *         an error listener.
     */

    @Override
    public ErrorListener getErrorListener() {
        if (invalidityHandler instanceof InvalidityHandlerWrappingErrorReporter) {
            ErrorReporter reporter = ((InvalidityHandlerWrappingErrorReporter)invalidityHandler).getErrorReporter();
            if (reporter instanceof ErrorReporterToListener) {
                return ((ErrorReporterToListener) reporter).getErrorListener();
            }
        }
        return null;
    }
    //#endif

    /**
     * Setup Validation Reporting feature which saves the validation errors in an XML file
     *
     * @param destination where XML will be sent
     * @throws SaxonApiException if the destination cannot be configured for this purpose
     */
    @Override
    public void setValidityReporting(Destination destination) throws SaxonApiException {
        invalidityHandler = processor.getUnderlyingConfiguration().createValidityReporter();
        ((InvalidityReportGeneratorEE) invalidityHandler).setDestination(destination);
    }

    /**
     * Get the InvalidityHandler used when validating instance documents
     *
     * @return the InvalidityHandler being used
     */
    @Override
    public InvalidityHandler getInvalidityHandler() {
        return invalidityHandler;
    }

    /**
     * Set the InvalidityHandler to be used when validating instance documents
     *
     * @param handler the InvalidityHandler to be used
     */
    @Override
    public void setInvalidityHandler(InvalidityHandler handler) {
        this.invalidityHandler = handler;
    }

    /**
     * Say whether validation statistics are to be collected. Calling this method
     * has the side-effect of deleting any statistics previously collected.
     *
     * @param collect true if validation statistics are to be collected (default is false)
     * @since 9.6
     */

    @Override
    public void setCollectStatistics(boolean collect) {
        //#if CSHARP==false
        this.keepStatistics = collect;
        latestStatistics = null;
        //#endif
    }

    /**
     * Ask whether validation statistics are to be collected
     *
     * @return true if validation statistics are to be collected (default is false)
     * @since 9.6
     */

    @Override
    public boolean isCollectStatistics() {
        return this.keepStatistics;
    }

    /**
     * Report the validation statistics from the most recent validation episode
     * Does nothing if no validation statistics have been collected.
     *
     * @param destination the destination to which the statistics will be written.
     *                    The XML format of the destination is not guaranteed to be stable across
     *                    Saxon releases.
     * @since Saxon 9.6
     */

    @Override
    public void reportValidationStatistics(Destination destination) throws SaxonApiException {
        //#if CSHARP==false
        if (latestStatistics != null) {
            latestStatistics.report(processor, destination);
        }
        //#endif
    }

    /**
     * Say whether the schema processor is to take account of any xsi:schemaLocation and
     * xsi:noNamespaceSchemaLocation attributes encountered while validating an instance document
     *
     * @param recognize true if these two attributes are to be recognized; false if they are to
     *                  be ignored. Default is true.
     */

    @Override
    public void setUseXsiSchemaLocation(boolean recognize) {
        useXsiSchemaLocation = recognize;
    }

    /**
     * Ask whether the schema processor is to take account of any xsi:schemaLocation and
     * xsi:noNamespaceSchemaLocation attributes encountered while validating an instance document
     *
     * @return true if these two attributes are to be recognized; false if they are to
     *         be ignored. Default is true.
     */

    @Override
    public boolean isUseXsiSchemaLocation() {
        return useXsiSchemaLocation;
    }


    /**
     * Set the Destination to receive the validated document. If no destination is supplied, the
     * validated document is discarded.
     *
     * @param destination the destination to receive the validated document
     */

    @Override
    public void setDestination(/*@Nullable*/ Destination destination) {
        this.destination = destination;
    }

    /**
     * Get the Destination that will receive the validated document. Return null if no destination
     * has been set.
     *
     * @return the destination to receive the validated document, or null if none has been supplied
     */


    /*@Nullable*/
    @Override
    public Destination getDestination() {
        return destination;
    }

    /**
     * Set the name of the required top-level element of the document to be validated (that is, the
     * name of the outermost element of the document). If no value is supplied, there is no constraint
     * on the required element name
     *
     * @param name the name of the document element, as a QName; or null to remove a previously-specified
     *             value.
     */

    @Override
    public void setDocumentElementName(QName name) {
        documentElementName = name;
    }

    /**
     * Get the name of the required top-level element of the document to be validated.
     *
     * @return the name of the required document element, or null if no value has been set.
     */

    @Override
    public QName getDocumentElementName() {
        return documentElementName;
    }

    /**
     * Set the name of the required type of the top-level element of the document to be validated.
     * If no value is supplied, there is no constraint on the required type
     *
     * @param name the name of the type of the document element, as a QName;
     *             or null to remove a previously-specified value. This must be the name of a type in the
     *             schema (typically but not necessarily a complex type).
     * @throws net.sf.saxon.s9api.SaxonApiException
     *          if there is no known type with this name
     */

    @Override
    public void setDocumentElementTypeName(QName name) throws SaxonApiException {
        Configuration config = processor.getUnderlyingConfiguration();
        documentElementType = config.getSchemaType(name.getStructuredQName());
        if (documentElementType == null) {
            throw new SaxonApiException("Unknown type " + name.getClarkName());
        }
    }

    /**
     * Get the name of the required type of the top-level element of the document to be validated.
     *
     * @return the name of the required type of the document element, or null if no value has been set.
     */

    @Override
    public QName getDocumentElementTypeName() {
        if (documentElementType == null) {
            return null;
        } else {
            return new QName(documentElementType.getStructuredQName());
        }
    }

    /**
     * Get the schema type against which the document element is to be validated
     *
     * @return the schema type
     */

    @Override
    protected SchemaType getDocumentElementType() {
        return documentElementType;
    }

    /**
     * Set whether attribute defaults defined in a schema are to be expanded or not
     * (by default, fixed and default attribute values are expanded, that is, they are inserted
     * into the document during validation as if they were present in the instance being validated)
     *
     * @param expand true if defaults are to be expanded, false if not
     */

    @Override
    public void setExpandAttributeDefaults(boolean expand) {
        expandAttributeDefaults = expand;
    }

    /**
     * Ask whether attribute defaults defined in a schema are to be expanded or not
     * (by default, fixed and default attribute values are expanded, that is, they are inserted
     * into the document during validation as if they were present in the instance being validated)
     *
     * @return true if defaults are to be expanded, false if not
     */

    @Override
    public boolean isExpandAttributeDefaults() {
        return expandAttributeDefaults;
    }

    /**
     * Set the value of a schema parameter (a parameter defined in the schema using
     * the <code>saxon:param</code> extension)
     *
     * @param name  the name of the schema parameter, as a QName
     * @param value the value of the schema parameter, or null to clear a previously set value
     * @throws SaxonApiUncheckedException if the value is lazily evaluated and evaluation fails
     * @since 9.5
     */

    @Override
    public void setParameter(QName name, XdmValue value) {
        try {
            suppliedParams.put(name.getStructuredQName(), value == null ? null : ((Sequence) value.getUnderlyingValue()).materialize());
        } catch (UncheckedXPathException e) {
            throw new SaxonApiUncheckedException(e);
        }
    }

    /**
     * Get the value that has been set for a schema parameter (a parameter defined in the schema using
     * the <code>saxon:param</code> extension)
     *
     * @param name the parameter whose name is required
     * @return the value that has been set for the parameter, or null if no value has been set
     * @since 9.5
     */

    @Override
    public XdmValue getParameter(QName name) {
        GroundedValue value = suppliedParams.get(name.getStructuredQName());
        return value ==  null ? null : XdmValue.wrap(value);
    }

//    /**
//     * A task to be performed when validation completes
//     */

//    @FunctionalInterface
//    @CSharpDelegate
//    interface ValidationCompletionAction {
//        void finish() throws SaxonApiException;
//    }

//    class ValidationTask implements java.util.concurrent.Callable<Outcome<Boolean>> {
//        private final Source source;
//        private ValidationCompletionAction action = null;
//        private InvalidityHandler handler = null;
//
//        public ValidationTask(Source s, InvalidityHandler handler, ValidationCompletionAction action) {
//            source = s;
//            this.handler = handler;
//            this.action = action;
//        }
//
//        /**
//         * Computes a result, or throws an exception if unable to do so.
//         *
//         * @return computed result
//         * @throws Exception if unable to compute a result
//         */
//        @Override
//        public Outcome<Boolean> call() throws Exception {
//            Configuration config = processor.getUnderlyingConfiguration();
//
//            try {
//                Controller controller = new Controller(config);
//                controller.getExecutable().setSchemaAware(true);
//                PipelineConfiguration pipe = controller.makePipelineConfiguration();
//                pipe.setUseXsiSchemaLocation(useXsiSchemaLocation);
//                pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
//
//                ParseOptions options = new ParseOptions();
//
//                options.setCheckEntityReferences(true);
//                options.setValidationParams(convertParams(suppliedParams, config.getDeclaredSchemaParameters()));
//                options.setInvalidityHandler(handler);
//
//
//                options.setCheckEntityReferences(true);
//                options.setExpandAttributeDefaults(expandAttributeDefaults);
//                options.setSchemaValidationMode(lax ? Validation.LAX : Validation.STRICT);
//                options.setSpaceStrippingRule(NoElementsSpaceStrippingRule.getInstance());
//                options.setTopLevelType(documentElementType);
//                if (documentElementName != null) {
//                    options.setTopLevelElement(new StructuredQName(
//                            documentElementName.getPrefix(), documentElementName.getNamespaceURI(), documentElementName.getLocalName()));
//                }
//
//                Receiver output = destination == null ? new Sink(pipe) : destination.getReceiver(pipe, null);
//                output.setPipelineConfiguration(pipe);
//
//                Receiver receiver = config.getDocumentValidator(output, source.getSystemId(), options, null);
//
//                /*if (keepStatistics) {
//                    latestStatistics = new ValidationStatistics();
//                    options.setValidationStatisticsRecipient(latestStatistics.recipient);
//                } */
//                Sender.send(source, receiver, options);
//            } catch (XPathException | SaxonApiException e1) {
//                //e1.printStackTrace();
//                return new Outcome<>(e1);
//            } finally {
//                if (action != null) {
//                    action.finish();
//                }
//
//            }
//            return new Outcome<>(true);
//        }
//
//    }


    public void setThreadCount(int tc) {
        threadCount = tc;
    }

    /**
     * Validate multiple instance documents supplied as a Iterable collection of Source objects
     *
     * @param sources the Iterable collection of instance documents to be validated. The call getSystemId() applied to
     *                the source objects must return the base URI used for dereferencing any xsi:schemaLocation
     *                or xsi:noNamespaceSchemaLocation attributes
     * @throws SaxonApiException if the source document is found to be invalid, or if error conditions
     *                           occur that prevented validation from taking place (such as failure to read or parse the input
     *                           document). The wrapped exception acting as the cause of the SaxonApiException can be used to
     *                           distinguish these failure conditions.
     */
    @Override
    public void validateMultiple(Iterable<Source> sources) throws SaxonApiException {
        EnterpriseConfiguration config = (EnterpriseConfiguration)processor.getUnderlyingConfiguration();
        if (!config.getBooleanProperty(Feature.ALLOW_MULTITHREADING)) {
            threadCount = 1;
        }

        if (threadCount < 1) {
            threadCount = 1;
        }
        if (destination != null) {
            throw new IllegalStateException("Cannot validate multiple documents when destination is set externally");
        }
        Destination reportDestination = null;
        final ReportList reportDocs = new ReportList();
        if (invalidityHandler instanceof InvalidityReportGeneratorEE) {
            reportDestination = ((InvalidityReportGeneratorEE) invalidityHandler).getDestination();
        }

        doParallelValidation(sources, config, reportDocs);

        final Location loc = Loc.NONE;
        if (reportDestination != null) {
            Receiver receiver = reportDestination.getReceiver(config.makePipelineConfiguration(), config.obtainDefaultSerializationProperties());
            try {
                receiver.open();
                receiver.startDocument(ReceiverOption.NONE);
                receiver.startElement(new FingerprintedQName("", InvalidityReportGenerator.REPORT_NS, "validation-reports"),
                                      Untyped.getInstance(),
                                      EmptyAttributeMap.getInstance(),
                                      NamespaceMap.of("", InvalidityReportGenerator.REPORT_NS),
                                      loc, ReceiverOption.NONE);

                for (XdmNode node : reportDocs) {
                    for (XdmNode de : node.children()) {
                        de.getUnderlyingNode().copy(receiver, 0, loc);
                    }
                }

                receiver.endElement();
                receiver.endDocument();
                receiver.close();
            } catch (XPathException e) {
                e.printStackTrace();
            }
        }
    }

    @CSharpReplaceBody(code = "Saxon.Impl.Parallel.Validation.ValidateMultiple(processor, sources, invalidityHandler, reportDocs);")
    private void doParallelValidation(Iterable<Source> sources, EnterpriseConfiguration config, ReportList reportDocs) throws SaxonApiException {
        InvalidityReportGeneratorEE singleReporter = null;
        Action completionAction = null;
        BlockingQueue<Future<Outcome<Boolean>>> queue = new LinkedBlockingQueue<>();
        ExecutorService executorService = config.getMultithreadingFactory().makeExecutorService(threadCount);

        for (Source source : sources) {
            if (invalidityHandler != null) {
                XdmDestination dest = new XdmDestination();

                singleReporter = (InvalidityReportGeneratorEE)processor.getUnderlyingConfiguration().createValidityReporter();
                singleReporter.setDestination(dest);
                final InvalidityReportGeneratorEE singleReportHandler = (InvalidityReportGeneratorEE) singleReporter;
                singleReporter.setXsdVersion(processor.getUnderlyingConfiguration().getXsdVersion() == Configuration.XSD11 ? "1.1" : "1.0");
                try {
                    singleReporter.startReporting(source.getSystemId());

                } catch (XPathException e) {
                    e.printStackTrace();
                }

                completionAction = () -> {
                    singleReportHandler.endReporting();
                    reportDocs.add(((XdmDestination) singleReportHandler.getDestination()).getXdmNode());
                };
            }
            final Action action = completionAction;
            final InvalidityHandler handler = singleReporter;
            //Future<Outcome<Boolean>> result = executorService.submit(new ValidationTask(s, singleTaskHandler, completionAction));
            Future<Outcome<Boolean>> result = executorService.submit(() -> {
                //Configuration config = processor.getUnderlyingConfiguration();
                Outcome<Boolean> outcome = new Outcome<Boolean>(true);
                try {
                    Controller controller = new Controller(config);
                    controller.getExecutable().setSchemaAware(true);
                    PipelineConfiguration pipe = controller.makePipelineConfiguration();
                    pipe.setUseXsiSchemaLocation(useXsiSchemaLocation);
                    pipe.setHostLanguage(HostLanguage.XML_SCHEMA);

                    ParseOptions options = new ParseOptions();

                    options.setCheckEntityReferences(true);
                    options.setValidationParams(convertParams(suppliedParams, config.getDeclaredSchemaParameters()));
                    options.setInvalidityHandler(handler);


                    options.setCheckEntityReferences(true);
                    options.setExpandAttributeDefaults(expandAttributeDefaults);
                    options.setSchemaValidationMode(lax ? Validation.LAX : Validation.STRICT);
                    options.setSpaceStrippingRule(NoElementsSpaceStrippingRule.getInstance());
                    options.setTopLevelType(documentElementType);
                    if (documentElementName != null) {
                        options.setTopLevelElement(new StructuredQName(
                                documentElementName.getPrefix(), documentElementName.getNamespaceURI(), documentElementName.getLocalName()));
                    }

                    Receiver output = destination == null ? new Sink(pipe) : destination.getReceiver(pipe, null);
                    output.setPipelineConfiguration(pipe);

                    Receiver receiver = config.getDocumentValidator(output, source.getSystemId(), options, null);
                    Sender.send(source, receiver, options);
                } catch (XPathException | SaxonApiException e1) {
                    //e1.printStackTrace();
                    outcome = new Outcome<Boolean>(e1);
                } finally {
                    if (action != null) {
                        try {
                            action.doAction();
                        } catch (XPathException e) {
                            outcome = new Outcome<Boolean>(e);
                        }
                    }
                }
                return outcome;
            });
            queue.add(result);
        }

        executorService.shutdown();
        boolean finish = tryToFinish(executorService);
        if (!finish) {
            throw new SaxonApiException("Some validation task(s) failed to complete within 24 hours");
        }

        for (Future<Outcome<Boolean>> futureOutcome : queue) {
            try {
                Outcome<Boolean> outcome = futureOutcome.get();
                Exception ex = outcome.getException();
                if (ex != null && !(ex instanceof XPathException)) {
                    throw new SaxonApiException(ex);
                }

            } catch (InterruptedException | ExecutionException e) {
                e.printStackTrace();
            }
        }
    }

    /**
     * A list to hold the individual validation reports, with thread-safe list append operation.
     * It's assumed that there's a concurrent phase during which nodes are added to the list,
     * followed by a single-threaded phase in which the list is enumerated.
     */
    @CSharpModifiers(code={"public"})
    private static class ReportList implements Iterable<XdmNode> {

        private final List<XdmNode> backing = new ArrayList<>();

        public synchronized void add(XdmNode node) {
            backing.add(node);
        }

        @Override
        public Iterator<XdmNode> iterator() {
            return backing.iterator();
        }
    }

    //#if CSHARP==false
    private boolean tryToFinish(ExecutorService executorService) {
        try {
            return executorService.awaitTermination(24, TimeUnit.HOURS);
        } catch (InterruptedException e) {
            executorService.shutdownNow();
            Thread.currentThread().interrupt();
            return false;
        }
    }
    //#endif

    /**
     * Validate an instance document supplied as a Source object
     *
     * @param source the instance document to be validated. The call getSystemId() applied to
     *               this source object must return the base URI used for dereferencing any xsi:schemaLocation
     *               or xsi:noNamespaceSchemaLocation attributes
     * @throws net.sf.saxon.s9api.SaxonApiException
     *          if the source document is found to be invalid, or if error conditions
     *          occur that prevented validation from taking place (such as failure to read or parse the input
     *          document). The wrapped exception acting as the cause of the SaxonApiException can be used to
     *          distinguish these failure conditions.
     */

    @Override
    public void validate(Source source) throws SaxonApiException {
        Configuration config = processor.getUnderlyingConfiguration();
        Receiver receiver = getReceiver(config, source.getSystemId());
        try {
            if (invalidityHandler instanceof InvalidityReportGenerator) {
                ((InvalidityReportGenerator) invalidityHandler).setXsdVersion(config.getXsdVersion() == Configuration.XSD11 ? "1.1" : "1.0");
            }
            invalidityHandler.startReporting(source.getSystemId());
            Sender.send(source, receiver, new ParseOptions());
        } catch (QuitParsingException qpe) {
            // This happens when the result of validation is fed into a streaming transformer, and the transformer
            // decides it doesn't need the whole input. Although we haven't actually validated the whole input, we
            // report success: the transformer will already have emitted a warning.
            // NO ACTION
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        } finally {
            if (invalidityHandler != null) {
                try {
                    invalidityHandler.endReporting();
                } catch (XPathException e1) {
                    // no action
                }
            }
        }
    }

    protected ValidationParams convertParams(GlobalParameterSet suppliedParams, java.util.Collection<GlobalParam> declaredParams)
            throws XPathException {
        ValidationParams vp = new ValidationParams();
        XPathContext context = new Controller(processor.getUnderlyingConfiguration()).newXPathContext();
        for (GlobalParam cp : declaredParams) {
            GroundedValue value = suppliedParams.convertParameterValue(
                    cp.getVariableQName(), cp.getRequiredType(), true, context);
            if (value != null) {
                vp.put(cp.getVariableQName(), value);
            }
        }
        return vp;
    }

    /**
     * A SchemaValidator is a Destination (for example, it can act as the destination of a query
     * or transformation). The getReceiver() method is used internally to provide the receiver
     * to which events can be sent; the effect is that the SchemaValidator validates the document
     * represented by this sequence of events
     * @param pipe the Saxon Configuration
     * @param params
     * @return a Receiver to which events can be sent
     * @throws SaxonApiException if a failure occurs
     */

    @Override
    public Receiver getReceiver(PipelineConfiguration pipe, SerializationProperties params) throws SaxonApiException {
        return getReceiver(pipe.getConfiguration(), null);
    }

    private Receiver getReceiver(Configuration config, /*@Nullable*/ String systemId) throws SaxonApiException {
        Controller controller = new Controller(config);
        Executable exec = controller.getExecutable();
        PackageData pack = new PackageData(config);
        pack.setLocalLicenseId(((EnterpriseConfiguration)config).getSuperSchema().getLocalLicenseId());
        pack.setSchemaAware(true);
        exec.setTopLevelPackage(pack);
        exec.setSchemaAware(true);
        PipelineConfiguration pipe = controller.makePipelineConfiguration();
        pipe.setUseXsiSchemaLocation(useXsiSchemaLocation);
        pipe.setHostLanguage(HostLanguage.XML_SCHEMA);

        ParseOptions options = pipe.getParseOptions();
        options.setCheckEntityReferences(true);
        options.setExpandAttributeDefaults(expandAttributeDefaults);
        options.setSchemaValidationMode(lax ? Validation.LAX : Validation.STRICT);
        options.setSpaceStrippingRule(NoElementsSpaceStrippingRule.getInstance());
        options.setTopLevelType(documentElementType);
        options.setInvalidityHandler(invalidityHandler);
        if (documentElementName != null) {
            options.setTopLevelElement(new StructuredQName(
                    documentElementName.getPrefix(), documentElementName.getNamespaceURI(), documentElementName.getLocalName()));
        }
        try {
            options.setValidationParams(convertParams(suppliedParams, config.getDeclaredSchemaParameters()));
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
        options.setCheckEntityReferences(true);
        //#if CSHARP==false
        if (keepStatistics) {
            latestStatistics = new ValidationStatistics();
            options.setValidationStatisticsRecipient(latestStatistics.getRecipient());
        }
        //#endif
        Receiver output = destination == null ? new Sink(pipe) : destination.getReceiver(pipe, config.obtainDefaultSerializationProperties());
        output.setPipelineConfiguration(pipe);

        return config.getDocumentValidator(output, systemId, options, null);
    }

    /**
     * Close the destination, allowing resources to be released. Saxon calls this method when
     * it has finished writing to the destination.
     */

    @Override
    public void close() throws SaxonApiException {
        if (destination != null) {
            destination.close();
            destination = null;
        }
    }

}

// Copyright (c) 2018-2022 Saxonica Limited
