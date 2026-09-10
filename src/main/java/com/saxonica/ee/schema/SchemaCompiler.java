package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.fsa.FiniteStateMachine;
import com.saxonica.ee.schema.sdoc.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.expr.instruct.GlobalParameterSet;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.ErrorReporter;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingException;
import net.sf.saxon.trans.XmlProcessingIncident;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.AnyURIValue;

import javax.xml.transform.Source;
import java.util.*;

/**
 * A SchemaCompiler is capable of reading source schema documents and converting them into collections
 * of schema components. The Compiler holds all the options relating to schema compilation, including
 * maintaining information about the schema documents involved in a compilation.
 * <p>A default SchemaCompiler is available as part of the EnterpriseConfiguration</p>
 *
 * @since 8.9
 */

//@CSharpInjectMembers(code = {
//        "    public void setErrorReporter(System.Action<net.sf.saxon.s9api.XmlProcessingError> reporter) {"
//                + "        setErrorReporter(new Saxon.Impl.Helpers.ErrorReportingAction(reporter));"
//                + "    }"
//})
public class SchemaCompiler {

    private final EnterpriseConfiguration config;
    // During a schema validation episode, the SchemaCompiler knows which schema it is validating
    private PreparedSchema preparedSchema;
    private int[] occurrenceLimits;
    private final SchemaURIResolver schemaURIResolver;
    private ErrorReporter errorReporter;
    private HashMap<IdentityConstraintReference, SchemaType> identityConstraintsAwaitingTypeChecking = null;
    private int errorCount = 0;
    private int xsdlVersion = Configuration.XSD10;
    private final boolean allowDanglingReferences;


    private final HashMap<String, XSDSchema> documentsAlreadyRead = new HashMap<>(10);
    private final HashSet<String> documentsBeingRead = new HashSet<>(10);
    private PipelineConfiguration pipelineConfiguration;
    private SchemaDocumentTransformer chameleonTransformer = null;
    private SchemaDocumentTransformer overrideTransformer = null;
    private final Set<String> componentsReportedMissing = new HashSet<>();
    private Set<UserComplexType> scheduledForAutomaton = new HashSet<>();
    private Set<UserComplexType[]> scheduledForAutomatonCheck = new HashSet<>();


    /**
     * Create a SchemaCompiler
     *
     * @param config the Saxon Configuration
     */
    public SchemaCompiler(EnterpriseConfiguration config) {
        this.config = config;
        errorReporter = config.makeErrorReporter();
        schemaURIResolver = config.getSchemaURIResolver();
        occurrenceLimits = config.getOccurrenceLimits();
        allowDanglingReferences = config.getBooleanProperty(Feature.ALLOW_UNRESOLVED_SCHEMA_COMPONENTS);
    }

    /**
     * Get the configuration
     *
     * @return the configuration (which is always schema-aware)
     */

    public EnterpriseConfiguration getConfiguration() {
        return config;
    }

    /**
     * Get the NamePool
     *
     * @return the name pool
     */

    public NamePool getNamePool() {
        return config.getNamePool();
    }

    /**
     * Set the XSD language version allowed.
     *
     * @param version the language version allowed. Default is {@link Configuration#XSD10};
     *                to use facilities in XML Schema 1.1, the value must be set to {@link Configuration#XSD11}
     */

    public void setLanguageVersion(int version) {
        xsdlVersion = version;
    }

    /**
     * Get the XSD langauge version allowed
     *
     * @return the XSD language version. Default is {@link Configuration#XSD10}; to use facilities in XML Schema 1.1, the
     *         value must be set to {@link Configuration#XSD11}
     */

    public int getLanguageVersion() {
        return xsdlVersion;
    }

    /**
     * Set the PreparedSchema. Used when validating a schema loaded using the SchemaModelLoader
     *
     * @param schema the schema that has been loaded
     */

    public void setPreparedSchema(PreparedSchema schema) {
        preparedSchema = schema;
    }

    /**
     * Get the PreparedSchema. This is a callback available only during a schema validation episode
     *
     * @return the PreparedSchema containing all the schema components
     */

    public PreparedSchema getPreparedSchema() {
        return preparedSchema;
    }

    /**
     * Get the ErrorReporter used in this configuration. If no ErrorReporter
     * has been supplied explicitly, the default ErrorReporter is used.
     *
     * @return the ErrorReporter.
     * @since 8.9. Changed in 10.0 to use ErrorReporter rather than ErrorListener
     */

    public ErrorReporter getErrorReporter() {
        if (errorReporter == null) {
            errorReporter = config.makeErrorReporter();
        }
        return errorReporter;
    }

    /**
     * Set the ErrorReporter to be used in this compiler. The ErrorReporter
     * is informed of all errors detected during schema processing.
     *
     * @param reporter the ErrorReporter to be used
     * @since 8.9. Changed in 10.0 to use ErrorReporter rather than ErrorListener
     */

    public void setErrorReporter(ErrorReporter reporter) {
        this.errorReporter = reporter;
    }

    /**
     * Test whether errors have been reported
     *
     * @return the number of errors reported
     */

    public int getErrorCount() {
        return errorCount;
    }


    /**
     * Set the occurrence limits to be used when compiling a finite state automaton.
     *
     * @param minimum The highest value of minOccurs that will be strictly enforced. If the actual
     *                value of minOccurs in a schema is higher than this value, it will be treated as if this value
     *                had been specified. The default is 100.
     * @param maximum The highest value of maxOccurs that will be strictly enforced. If the actual
     *                value of maxOccurs in a schema is higher than this value, it will be treated as if "unbounded"
     *                had been specified. The default is 250.
     * @since 8.9
     */

    public void setOccurrenceLimits(int minimum, int maximum) {
        if (minimum < 1 || minimum >= maximum) {
            throw new IllegalArgumentException("Invalid occurrence limits: min<1 or max<min");
        }
        occurrenceLimits = new int[]{minimum, maximum};
    }

    /**
     * Get the occurrence limits previously set using setOccurrenceLimits (or the default)
     *
     * @return an array of two integers, the minimum occurrence limit and the maximum occurrence limit
     * @since 8.9
     */

    public int[] getOccurrenceLimits() {
        return occurrenceLimits;
    }

    /**
     * Mark a URI as currently being read
     *
     * @param systemId the URI of the schema document being read
     */

    public void setBeingRead(String systemId) {
        documentsBeingRead.add(systemId);
    }

    /**
     * Determine whether a particular URI is currently being read
     *
     * @param systemId the URI of the document in question
     * @return true if the document is currently being read
     */

    public boolean isBeingRead(String systemId) {
        return documentsBeingRead.contains(systemId);
    }

    /**
     * Mark a document as having been read
     *
     * @param systemId the URI of the schema document that has been read
     * @param schema   the root element of the schema document constructed from this URI
     */

    public void markAsRead(String systemId, XSDSchema schema) {
        documentsBeingRead.remove(systemId);
        documentsAlreadyRead.put(systemId + "##" + schema.getTargetNamespace(), schema);
    }

    /**
     * Ask whether dangling references are allowed. Returns false by default; returns true
     * if the configuration property {@link Feature#ALLOW_UNRESOLVED_SCHEMA_COMPONENTS} is set.
     * @return true if the presence of unresolved schema components should not cause the
     * schema to be treated as invalid. Instead, the schema is "repaired" such that it can
     * be used for validation, provided that the absent schema components are not actually
     * required for a particular validation episode.
     */

    public boolean isAllowDanglingReferences() {
        return allowDanglingReferences;
    }

    /**
     * Get the root element of the tree representing a schema document that has already been read
     *
     * @param systemId        the URI of the required schema document
     * @param targetNamespace the expected namespace. If the schema document was previously read
     *                        but chameleoned into a different namespace, we read it again
     * @return the root element of the tree representing this schema document
     */

    public XSDSchema getExistingSchemaDocument(String systemId, NamespaceUri targetNamespace) {
        return documentsAlreadyRead.get(systemId + "##" + targetNamespace);
    }

    /**
     * Get the pipeline configuration
     *
     * @return the pipeline configuration
     */

    public PipelineConfiguration getPipelineConfiguration() {
        return pipelineConfiguration;
    }

    /**
     * Set the pipeline configuration
     *
     * @param pipelineConfiguration the pipeline configuration
     */

    public void setPipelineConfiguration(PipelineConfiguration pipelineConfiguration) {
        this.pipelineConfiguration = pipelineConfiguration;
    }

    /**
     * Get the user-defined SchemaURIResolver for resolving URIs used in "import schema"
     * declarations; if none has been explicitly set, returns the standard schema URI resolver.
     *
     * @return the SchemaURIResolver
     */

    public SchemaURIResolver getSchemaURIResolver() {
        return schemaURIResolver;
    }

    /**
     * Read a schema document from a given schema location. If deferredValidationMode is not
     * set, the schema document will be fully validated and integrated with other schema documents
     * previously loaded
     *
     * @param pipe           the pipeline configuration
     * @param baseURI        the base URI for resolving relative URIs
     * @param schemaLocation the location "hint" where to find the schema document. Must not be null.
     * @param expected       the target namespace expected; null if there is no
     *                       expectation
     * @return the actual target namespace of the schema
     */

    public NamespaceUri readSchema(PipelineConfiguration pipe, String baseURI, String schemaLocation, NamespaceUri expected)
            throws SchemaException {
        Source source = SchemaReader.getSource(baseURI, schemaLocation, this, expected);
        if (source == null) {
            throw new SchemaException(
                    "No schema could be loaded at location " + schemaLocation);
        }
        SchemaDocument schemadoc = SchemaReader.read(source, this, pipe, null);
        if (schemadoc.isCompiledSchemaDocument()) {
            try {
                getConfiguration().importComponents(schemadoc.getDocument().getRootNode().asActiveSource());
                return expected;
            } catch (XPathException e) {
                throw new SchemaException(e);
            }
        } else {
            return processSchemaDocument(schemadoc, schemaLocation, expected);
        }
    }

    /**
     * Process a schema document, adding the relevant components to the schema document maintained by this schema compiler
     * @param schemadoc the schema document
     * @param schemaLocation the location of the schema document (mainly for diagnostics)
     * @param expected the expected namespace of the schema document
     * @return the actual namespace of the schema document
     * @throws SchemaException if an expected namespace was specified and the actual namespace differs; or if the schema
     * is invalid, or inconsistent with already loaded schema components.
     */

    private NamespaceUri processSchemaDocument(SchemaDocument schemadoc, String schemaLocation, NamespaceUri expected)
            throws SchemaException {
        if (schemadoc.isCompiledSchemaDocument()) {
            // the schema will already have been added to the Configuration
            return expected;
        }
        NamespaceUri targetNamespace = schemadoc.getTargetNamespace();
        if (expected != null && !expected.equals(targetNamespace)) {
            SchemaException exception = new SchemaException(
                    "Schema " +
                            (schemaLocation == null ? "" : "at location " + schemaLocation + " ") +
                            "has target namespace \"" +
                            targetNamespace +
                            "\" but requested namespace was \"" + expected + '\"');
            if (schemaLocation == null) {
                exception.setErrorCode("XTSE0215"); // This probably means it is an XSLT inline schema
            }
            throw exception;
        }

        PreparedSchema ps = schemadoc.getXSDSchema().getSchema();
        preparedSchema = ps;
        validate(schemadoc);
        preparedSchema = null;

        Collection<GlobalParam> globalParams = schemadoc.getXSDSchema().getGlobalParams();
        for (GlobalParam param : globalParams) {
            ps.addGlobalParam(param);
        }

        return targetNamespace;
    }

    private void retainSchemaComponents(PreparedSchema ps) throws SchemaException {
        if (preparedSchema == null) {
            preparedSchema = ps;
        } else {
            preparedSchema = PreparedSchema.merge(preparedSchema, ps);
        }
    }

    /**
     * Read schemas from a list of schema locations. Note, these must contain source XSD documents (compiled SCM
     * documents are not acceptable)
     *
     * @param pipe            the PipelineConfiguration
     * @param baseURI         the base URI for resolving relative URIs
     * @param schemaLocations the list of location "hints" where to find the schema document. Must not be null.
     * @param expected        the target namespace expected in each of the schema documents; null if there is no
     *                        expected namespace
     * @throws SchemaException if reading a schema document fails
     */

    public void readMultipleSchemas(PipelineConfiguration pipe, String baseURI, List<String> schemaLocations, NamespaceUri expected)
            throws SchemaException {
        String[] locations = new String[schemaLocations.size()];
        int loc = 0;
        for (String schemaLocation : schemaLocations) {
            locations[loc++] = schemaLocation;
        }
        Source[] sources = SchemaReader.getSources(baseURI, locations, this, expected);
        if (sources.length == 0) {
            // the requested schemas were already loaded
            return;
        }
        boolean success = false;
        SchemaException lastException = null;
        PreparedSchema mergedSchema = null;
        for (int i = 0; i < sources.length; i++) {

            SchemaDocument schemadoc;
            try {
                Source source = sources[i];
                if (source == null) {
                    throw new SchemaException("Unable to load schema at " + schemaLocations.get(i));
                }
                schemadoc = SchemaReader.read(source, this, pipe, null);
                if (schemadoc.isCompiledSchemaDocument()) {
                    throw new SchemaException("Only source schema documents (not SCM compiled schemas) are accepted here");
                }
                success = true;
                PreparedSchema ps = schemadoc.getXSDSchema().getSchema();
                if (mergedSchema == null) {
                    mergedSchema = ps;
                } else {
                    mergedSchema = PreparedSchema.merge(mergedSchema, ps);
                }
                NamespaceUri targetNamespace = schemadoc.getTargetNamespace();
                if (expected != null && !expected.equals(targetNamespace)) {
                    throw new SchemaException(
                            "Schema at " + locations[i] +
                                    " has target namespace \"" + targetNamespace +
                                    "\" but requested namespace was \"" + expected + '\"');
                }

            } catch (SchemaException e) {
                if (e.getException() instanceof java.io.IOException) {
                    XmlProcessingException ee = new XmlProcessingException(e);
                    ee.setWarning(true);
                    pipe.getErrorReporter().report(ee);
                } else {
                    lastException = e;
                }
            }
        }

        if (lastException != null) {
            throw lastException;
        }
        if (!success) {
            throw new SchemaException("No valid schema was found");
        }

        preparedSchema = mergedSchema;
        mergedSchema.validate(this);
        makeAllAutomata();
        checkAllAutomata();
        preparedSchema = null;

    }

    /**
     * Read an inline schema from a stylesheet
     *
     * @param root          the xs:schema element in the stylesheet
     * @param expected      the target namespace expected; "" if there is no
     *                      expectation
     * @param errorReporter The destination for error messages. May be null, in which case
     *                      the errorListener registered with this Configuration is used.
     * @return the actual target namespace of the schema
     * @throws SchemaException if the inline schema is in error
     */

    public NamespaceUri readInlineSchema(NodeInfo root, NamespaceUri expected, ErrorReporter errorReporter)
            throws SchemaException {
        ErrorReporter listener = errorReporter == null ? getErrorReporter() : errorReporter;
        setErrorReporter(listener);
        PipelineConfiguration pipe = config.makePipelineConfiguration();
        pipe.setErrorReporter(listener);
        pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
        SchemaDocument schemadoc = SchemaReader.read(root.asActiveSource(), this, pipe, null);
        return processSchemaDocument(schemadoc, null, expected);
    }

    /**
     * In deferred validation mode, validate and compile the accumulated schema components. If validation
     * succeeds, the schema components will be added to the cache held by the Configuration.
     *
     * @throws SchemaException if any of the schema components is invalid, or if the collection of
     *                         schema components as a whole is invalid, or if there are components present that conflict with components
     *                         already present in the cache held by the Configuration.
     */

    public void compile() throws SchemaException {
        boolean valid = preparedSchema.validate(this);
        if (!valid) {
            throw new SchemaException("The schema is invalid");
        }
    }

    /**
     * Validate the schema represented by a schema document. This method validates the schema components
     * associated with this schema document: that is, the components defined in this document, together with
     * any components referenced by these components if they have not already been validated.
     *
     * @param doc the schema document to be validated
     * @return schema the schema, if it is valid
     * @throws SchemaException if the schema is invalid
     */

    /*@Nullable*/
    public PreparedSchema validate(SchemaDocument doc) throws SchemaException {
        if (doc.isCompiledSchemaDocument()) {
            throw new SchemaException("Only source schema documents (not SCM compiled schemas) are accepted here");
        }
        XSDSchema schemaRoot = doc.getXSDSchema();
        preparedSchema = schemaRoot.getSchema();
        int errors = schemaRoot.getErrorCount();
        if (errors > 0) {
            String msg = errors == 1 ? "One error was" : errors + " errors were";
            throw new SchemaException(msg + " found while processing the schema");
        }

        // copy over the saxon:param declarations
        for (GlobalParam param : schemaRoot.getGlobalParams()) {
            preparedSchema.addGlobalParam(param);
        }

        compile();
        makeAllAutomata();
        checkAllAutomata();
        PreparedSchema ps = preparedSchema;
        preparedSchema = null;
        return ps;
    }

    /**
     * Register that there is an unresolved component reference
     * @param componentType the type of component, for example attributeGroup
     * @param componentName the name of the component
     * @return true if this is the first report that this component is missing
     */

    public boolean reportMissingComponent(String componentType, StructuredQName componentName) {
        String key = componentType + " " + componentName.getEQName();
        return componentsReportedMissing.add(key);
    }

    /**
     * Report an error
     *
     * @param message the error message
     * @param locator the location of the error
     */

    public void error(String message, Location locator) {
        errorCount++;
        getErrorReporter().report(new XmlProcessingIncident(message, SaxonErrorCode.SXXP0006, locator));
    }

    /**
     * Report a warning
     * @param message the warning message
     * @param errorCode a code associated with the message
     * @param locator the location of the error
     */

    public void warning(String message, String errorCode, Location locator) {
        if (locator.getSystemId() !=null && locator.getSystemId().endsWith("override.xsl")) {
            locator = new Loc("schema_document_resulting_from_override_processing", -1, -1);
        }
        XmlProcessingIncident exception = new XmlProcessingIncident(message, errorCode, locator).asWarning();
        getErrorReporter().report(exception);
    }

    /**
     * Register an identity constraint that needs to be type-checked
     *
     * @param constraint      the constraint to be checked
     * @param contextItemType the type of the context node for the XPath expression to be checked
     */

    public void addPendingTypeCheck(IdentityConstraintReference constraint, SchemaType contextItemType) {
        if (identityConstraintsAwaitingTypeChecking == null) {
            identityConstraintsAwaitingTypeChecking = new HashMap<IdentityConstraintReference, SchemaType>(20);
        }
        identityConstraintsAwaitingTypeChecking.put(constraint, contextItemType);
    }

    /**
     * Do all the pending type-checking
     *
     * @throws SchemaException if any of the checks fail
     */

    public void typeCheckIdentityConstraints() throws SchemaException {
        try {
            if (identityConstraintsAwaitingTypeChecking != null) {
                for (Map.Entry<IdentityConstraintReference, SchemaType> e : identityConstraintsAwaitingTypeChecking.entrySet()) {
                    IdentityConstraint constraint = e.getKey().getTarget();
                    SchemaType contextItemType = e.getValue();
                    constraint.typeCheck(this, contextItemType);
                }
                identityConstraintsAwaitingTypeChecking = null;
            }
        } catch (XPathException e) {
            throw new SchemaException(e);
        }
    }

    /**
     * Do the chameleon transformation on a source schema document
     *
     * @param input    the included schema document to be transformeed
     * @param referrer the xs:include element referencing this schema document
     * @return the transformed schema document
     * @throws SchemaException if the transformation fails
     */

    public SchemaDocument doChameleonTransformation(NodeInfo input, SchemaElement referrer) throws SchemaException {
        if (chameleonTransformer == null) {
            chameleonTransformer = new SchemaDocumentTransformer(config, "chameleon.xsl");
        }
        HashMap<StructuredQName, Sequence> params = new HashMap<StructuredQName, Sequence>();
        params.put(new StructuredQName("", NamespaceUri.NULL, "newTargetNamespace"),
                   new AnyURIValue((referrer.getXSDSchema().getTargetNamespace().toUnicodeString())));
        GlobalParameterSet parameterSet = new GlobalParameterSet();
        parameterSet.put(new StructuredQName("", NamespaceUri.NULL, "newTargetNamespace"),
                         new AnyURIValue((referrer.getXSDSchema().getTargetNamespace().toUnicodeString())));
        TreeInfo doc = chameleonTransformer.transform(this, pipelineConfiguration, referrer, input, parameterSet);
        return new SchemaDocument(doc);
    }

    /**
     * Do the override transformation on a source schema document
     *
     * @param input    the schema document being overridden
     * @param referrer the xs:override element referencing this schema document
     * @return the transformed schema document
     * @throws SchemaException if the transformation fails
     */

    public SchemaDocument doOverrideTransformation(NodeInfo input, SchemaElement referrer) throws SchemaException {
        if (overrideTransformer == null) {
            overrideTransformer = new SchemaDocumentTransformer(config, "override.xsl");
        }
        GlobalParameterSet params = new GlobalParameterSet();
        params.put(new StructuredQName("", NamespaceUri.NULL, "overrideElement"), referrer);
        TreeInfo doc = overrideTransformer.transform(this, pipelineConfiguration, referrer, input, params);
        return new SchemaDocument(doc);
    }

    /**
     * Schedule the task of creating a finite state automaton for a given complex type.
     * This is done at the end, because there may be multiple requests for the same complex type,
     * and doing it once saves effort
     * @param type the complex type for which an FSA is needed
     */

    public void scheduleAutomaton(UserComplexType type) {
        scheduledForAutomaton.add(type);
    }

    /**
     * Schedule a subsumption check: that is, checking that the finite state automaton for one
     * complex type subsumes the FSA for another complex type
     *
     * @param base the base (super-) complex type
     * @param sub the derived complex type
     */
    public void scheduleAutomatonCheck(UserComplexType base, UserComplexType sub) {
        scheduledForAutomatonCheck.add(new UserComplexType[]{base, sub});
    }

    /**
     * Create determinized finite state automata for all complex types for which this has
     * been previously requested
     * @throws SchemaException if creation of any FSA fails
     */

    public void makeAllAutomata() throws SchemaException {
        for (UserComplexType type : scheduledForAutomaton) {
            type.makeAutomaton(this);
        }
    }

    /**
     * Perform all subsumption checks that have been previously requested. This can only
     * be done after the determinized FSA's have been constructed.
     * @throws SchemaException if any restricted type is not validly derived from its base type
     */
    public void checkAllAutomata() throws SchemaException {
        for (UserComplexType[] pair : scheduledForAutomatonCheck) {
            UserComplexType sub = pair[1];
            UserComplexType base = pair[0];
            String subsumeError = FiniteStateMachine.subsumesMachine(
                    base.getFiniteStateMachine(), sub.getFiniteStateMachine(), this);
            if (subsumeError != null) {
                String message;
                if (subsumeError.startsWith("?")) {
                    message = "Saxon is unable to determine whether or not the" +
                                           " content model of the complex type " + sub.getDescription() +
                                           " is a valid restriction of the content model of the type " + base.getDescription() + ". "
                                           + subsumeError.substring(1);
                } else {
                    message = "The content model of the complex type " + sub.getDescription() +
                                           " is not a valid restriction of the content model of the type " + base.getDescription()
                                           + ". " + subsumeError;
                }
                error(message, sub);
                throw new SchemaException(message);
            }
        }
    }

}


// Copyright (c) 2006-2023 Saxonica Limited
