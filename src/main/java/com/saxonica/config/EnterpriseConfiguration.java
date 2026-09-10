package com.saxonica.config;

import com.saxonica.ee.bytecode.ByteCodeMonitor;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.config.*;
import com.saxonica.ee.extfn.VendorFunctionSetEE;
import com.saxonica.ee.extfn.js.IXSLElementFactory;
import com.saxonica.ee.optim.*;
import com.saxonica.ee.parallel.MultithreadedFocusTrackingIterator;
import com.saxonica.ee.parallel.ThreadManagerEE;
import com.saxonica.ee.schema.*;
import com.saxonica.ee.schema.sdoc.SchemaDocument;
import com.saxonica.ee.schema.sdoc.SchemaReader;
import com.saxonica.ee.schema.sdoc.XMLNamespaceSchema;
import com.saxonica.ee.schema.sdoc.XSINamespaceSchema;
import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.DecomposingFeed;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.watch.ApplyTemplatesAction;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.Watch;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.*;
import com.saxonica.ee.update.PendingUpdateListImpl;
import com.saxonica.ee.update.XQueryParserExtensionEE;
import com.saxonica.ee.validate.*;
import com.saxonica.expr.XQueryParserExtensionPE;
import com.saxonica.trans.ModePE;
import com.saxonica.xsltextn.style.Saxon6ElementFactory;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.Version;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.accum.Accumulator;
import net.sf.saxon.expr.accum.AccumulatorRegistry;
import net.sf.saxon.expr.instruct.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.expr.sort.GroupIterator;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.regex.RegexIterator;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.serialize.JSONEmitter;
import net.sf.saxon.serialize.UTF8Writer;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.StyleNodeFactory;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.style.XSLTemplate;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.*;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.type.*;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.*;
import java.util.concurrent.Callable;
import java.util.concurrent.ConcurrentHashMap;

/**
 * A subclass of Configuration that defines a processor with Saxon-EE capabilities. This contains
 * helper methods invoked both at compile time and at run-time to perform operations
 * that are allowed only in a schema-aware processor; the superclass Configuration
 * contains the dummy versions of these methods used in a non-schema-aware processor.
 * The interface is designed so that the calling classes contain no references to classes
 * that are available only in the schema-aware product.
 * <p>Applications should avoid using this class directly, and should instead access all the
 * necessary functionality through its superclass, {@link Configuration}.</p>
 */
public final class EnterpriseConfiguration extends ProfessionalConfiguration {

    private int licensedFeatures = 0;
    private HashMap<NamespaceUri, Integer> namespaceStatus = new HashMap<>();
    private int[] occurrenceLimits = {100, 250};
    private SchemaURIResolver schemaURIResolver = null;
    private final SchemaURIResolver standardSchemaResolver;
    private transient PreparedSchema superSchema;
    private String debugByteCodeDirectory = "Saxon-bytecode";
    private int resultDocumentThreadsLimit = Runtime.getRuntime().availableProcessors();
    private int thresholdForCompilingTypes = 100;
    private int thresholdForInliningFunctions = 100;
    private int maxCompiledClasses = 10000;
    private Optional<Boolean> streamabilityEnabled = Optional.empty();
    private final ConcurrentHashMap<String, RegularExpression> regexCache = new ConcurrentHashMap<>();
    private long regexAttempts = 0;
    private MultithreadingFactory multithreadingFactory;
    //#if BYTECODE == true
        private ByteCodeMonitor byteCodeMonitor;
    //#endif


    /**
     * Create an EnterpriseConfiguration
     */

    public EnterpriseConfiguration() {
        super();
        multithreadingFactory = new MultithreadingFactory(this);
        internalSetBooleanProperty(FeatureCode.ALLOW_MULTITHREADING, FeatureKeys.ALLOW_MULTITHREADING, true);
        internalSetBooleanProperty(FeatureCode.GENERATE_BYTE_CODE, FeatureKeys.GENERATE_BYTE_CODE, true);
        setStaticQueryContextFactory(new StaticQueryContextFactory() {
            @Override
            public StaticQueryContext newStaticQueryContext(Configuration config, boolean copyFromDefault) {
                return new StaticQueryContextEE(EnterpriseConfiguration.this, copyFromDefault);
            }
        });
        optimizerOptions = OptimizerOptions.FULL_EE_OPTIMIZATION;
        defaultXsltCompilerInfo.setOptimizerOptions(optimizerOptions);
        standardSchemaResolver = new StandardSchemaResolver(this);
        superSchema = new PreparedSchema(this);
    }

    @Override
    protected StaticQueryContext makeStaticQueryContext(boolean copyFromDefault) {
        StaticQueryContext sqc = super.makeStaticQueryContext(copyFromDefault);
        if (defaultStaticQueryContext != null) {
            sqc.setSchemaAware(defaultStaticQueryContext.isSchemaAware());
        }
        return sqc;
    }

    /**
     * Get the edition code identifying this configuration: "HE", "PE" or "EE"
     */

    @Override
    public String getEditionCode() {
        return "EE";
    }


    /**
     * Set a property of the configuration. This method underpins the setAttribute() method of the
     * TransformerFactory implementation, and is provided
     * to enable setting of Configuration properties using URIs without instantiating a TransformerFactory:
     * specifically, this may be useful when running XQuery, and it is also used by the Validator API
     *
     * @param feature  the property to be set. See the class {@link net.sf.saxon.lib.Feature} for
     *              constants representing the properties that can be set.
     * @param value the value of the property
     * @throws IllegalArgumentException if the property name is not recognized or if the value is not
     *                                  a valid value for the named property
     */

    @CSharpModifiers(code = {"public", "override"})
    @Override
    protected void setFeature(FeatureData feature, Object value) {
        int code = feature.code;
        FeatureData featureData = FeatureIndex.getData(code);
        String name = featureData.uri;
        switch (code) {
            case FeatureCode.GENERATE_BYTE_CODE: {
                if (value instanceof Boolean) {
                    internalSetBooleanProperty(code, name, value);
                } else if ("true".equals(value) || "on".equals(value) || "yes".equals(value) || "1".equals(value)) {
                    internalSetBooleanProperty(code, name, true);
                    byteCodeThreshold = 0;
                } else if ("false".equals(value) || "off".equals(value) || "no".equals(value) || "0".equals(value)) {
                    internalSetBooleanProperty(code, name, false);
                } else if (value instanceof Integer) {
                    internalSetBooleanProperty(code, name, true);
                    byteCodeThreshold = (Integer) value;
                } else {
                    internalSetBooleanProperty(code, name, true);
                    try {
                        byteCodeThreshold = Integer.parseInt(value.toString());
                    } catch (NumberFormatException e) {
                        throw new IllegalArgumentException("Value of " + name + " must be a boolean or an integer");
                    }
                }
                break;
            }
            case FeatureCode.THRESHOLD_FOR_HOTSPOT_BYTE_CODE: {

                int val = requireInteger(name, value);
                if (val <= 0) {
                    internalSetBooleanProperty(code, name, false);
                } else {
                    internalSetBooleanProperty(code, name, true);
                    byteCodeThreshold = val;
                }
                break;
            }
            case FeatureCode.DEBUG_BYTE_CODE_DIR: {
                debugByteCodeDirectory = value.toString();
                break;
            }
            case FeatureCode.OCCURRENCE_LIMITS: {
                String val = value.toString();
                int comma = val.indexOf(',');
                if (comma < 1) {
                    throw new IllegalArgumentException(
                            "Value of " + name + "must be two integers, comma-separated");
                }
                try {
                    int min = Integer.parseInt(val.substring(0, comma));
                    int max = Integer.parseInt(val.substring(comma + 1));
                    setOccurrenceLimits(min, max);
                } catch (NumberFormatException e) {
                    throw new IllegalArgumentException(
                            "Value of " + name + "must be two integers, comma-separated");
                }
                break;
            }
            case FeatureCode.RESULT_DOCUMENT_THREADS: {
                resultDocumentThreadsLimit = requireInteger(name, value);
                break;
            }
            case FeatureCode.SCHEMA_URI_RESOLVER: {
                if (!(value instanceof SchemaURIResolver)) {
                    throw new IllegalArgumentException(
                            "SCHEMA_URI_RESOLVER value must be an instance of net.sf.saxon.lib.SchemaURIResolver");
                }
                setSchemaURIResolver((SchemaURIResolver) value);
                break;
            }
            case FeatureCode.SCHEMA_URI_RESOLVER_CLASS: {
                setSchemaURIResolver(
                        (SchemaURIResolver) instantiateClassName(name, value, SchemaURIResolver.class));
                break;
            }
            case FeatureCode.STREAMABILITY: {
                String s = requireString(name, value);
                switch (s) {
                    case "off":
                        setStreamabilityEnabled(false);
                        break;
                    case "standard":
                        setStreamabilityEnabled(true);
                        break;
                    case "extended":
                        throw new IllegalArgumentException("The value STREAMABILITY = 'extended' is no longer recognized");
                    default:
                        throw new IllegalArgumentException(
                                "Unrecognized value STREAMABILITY = '" + value +
                                        "': must be 'off' or 'standard'");
                }
                break;
            }
            case FeatureCode.THRESHOLD_FOR_COMPILING_TYPES: {
                thresholdForCompilingTypes = requireInteger(name, value);
                break;
            }
            case FeatureCode.THRESHOLD_FOR_FUNCTION_INLINING: {
                thresholdForInliningFunctions = requireInteger(name, value);
                break;
            }
            case FeatureCode.MAX_COMPILED_CLASSES: {
                maxCompiledClasses = requireInteger(name, value);
                break;
            }
            default: {
                super.setFeature(feature, value);
                break;
            }

        }
    }

    /**
     * Get a property of the configuration
     *
     * @param feature the required property. See the class {@link net.sf.saxon.lib.Feature} for
     *             constants representing the property names that can be requested.
     * @return the value of the property
     * @throws IllegalArgumentException thrown if the property is not one that Saxon recognizes.
     */

    protected Object getFeature (FeatureData feature) {
        int code = feature.code;
        if (booleanFeatures.contains(code)) {
            return super.getFeature(feature);
        }
        switch (code) {
            case FeatureCode.DEBUG_BYTE_CODE_DIR:
                return debugByteCodeDirectory;
            case FeatureCode.OCCURRENCE_LIMITS:
                return (occurrenceLimits[0] + "," + occurrenceLimits[1]);
            case FeatureCode.THRESHOLD_FOR_COMPILING_TYPES:
                return thresholdForCompilingTypes;
            case FeatureCode.THRESHOLD_FOR_FUNCTION_INLINING:
                return thresholdForInliningFunctions;
            case FeatureCode.MAX_COMPILED_CLASSES:
                return maxCompiledClasses;
            case FeatureCode.RESULT_DOCUMENT_THREADS:
                return resultDocumentThreadsLimit;
            case FeatureCode.SCHEMA_URI_RESOLVER:
                return getSchemaURIResolver();
            case FeatureCode.SCHEMA_URI_RESOLVER_CLASS:
                return getSchemaURIResolver().getClass().getName();
            case FeatureCode.STREAMABILITY:
                return isStreamabilityEnabled() ? "standard" : "off";
            default:
                return super.getFeature(feature);
        }
    }

    /**
     * Check whether a particular feature is licensed, with a fatal error if it is not
     *
     * @param feature        the feature in question, identified by a constant in class {@link net.sf.saxon.Configuration.LicenseFeature}
     * @param name           the name of the required feature, for use in diagnostics
     * @param localLicenseId identifies a license key embedded in the SEF file
     * @throws net.sf.saxon.trans.LicenseException if the feature is not licensed
     */

    @Override
    public void checkLicensedFeature(int feature, String name, int localLicenseId) throws LicenseException {
        if (localLicenseId >= 0 && isFeatureAllowedBySecondaryLicense(localLicenseId, feature)) {
            return;
        }
        if (isLicenseNotFound()) {
            try {
                forceLoadLicense(); // We know this will fail, but we want the capture the reason
            } catch (Exception err) {
                int reason = err instanceof LicenseException ? ((LicenseException) err).getReason() : LicenseException.CANNOT_READ;
                String msg = err.getMessage() + ". A license is needed to use " + name;
                throw new LicenseException(msg, reason);
            }
        }
        if (!isLicenseFound()) {
            loadLicense();
        }
        reportIfMissingLicenseFeature(feature, name);
    }


    @Override
    protected void setLicensedFeatures() {
        licensedFeatures = LicenseFeature.PROFESSIONAL_EDITION;
        if ("yes".equals(getFeature("SAV"))) {
            licensedFeatures |= LicenseFeature.SCHEMA_VALIDATION;
        }
        if ("yes".equals(getFeature("SAQ"))) {
            licensedFeatures |= LicenseFeature.ENTERPRISE_XQUERY;
        }
        if ("yes".equals(getFeature("SAT"))) {
            licensedFeatures |= LicenseFeature.ENTERPRISE_XSLT;
        }
    }

    /**
     * Determine if a particular feature is licensed. Note, this method should only be called if it is
     * known that the license file has been loaded. If it is not known whether the license file has
     * been loaded, use the method {@link net.sf.saxon.Configuration#isLicensedFeature} which will load the license when necessary.
     *
     * @param feature the feature in question, identified by a constant in class {@link net.sf.saxon.Configuration.LicenseFeature}
     */

    @Override
    protected boolean testFeature(int feature) {
        return (licensedFeatures & feature) != 0;
    }

    /**
     * Throw an error indicating that a request cannot be satisfied because it requires
     * the enterprise edition of Saxon. If we get this far, we've got Saxon-EE loaded, and an
     * EnterpriseConfiguration; the problem must therefore be with the license file.
     */

    @Override
    protected void needEnterpriseEdition() {
        throw new UnsupportedOperationException(
                "This operation requires a valid Saxon-EE license file");
    }

    /**
     * Factory method to make a StyleNodeFactory, used for constructing elements
     * in a stylesheet document
     *
     * @param compilation the compilation episode (compiling one XSLT package)
     * @return the StyleNodeFactory used in this Configuration
     */

    @Override
    public StyleNodeFactory makeStyleNodeFactory(Compilation compilation) {
        PackageData pack = compilation.getPackageData();
        if (pack != null) {
            int locLic = pack.getLocalLicenseId();
            int feature = LicenseFeature.SCHEMA_VALIDATION;
            if (!(isFeatureAllowedBySecondaryLicense(locLic, feature) || isLicensedFeature(feature))) {
                return super.makeStyleNodeFactory(compilation);
            }
        }
        return new StyleNodeFactoryEE(this, compilation);
    }

    /**
     * Factory method to make a StylesheetPackage
     *
     * @return a StylesheetPackage suitable for use in this Configuration
     */

    @Override
    public StylesheetPackage makeStylesheetPackage() {
        return isLicensedFeature(LicenseFeature.ENTERPRISE_XSLT) ? new StylesheetPackageEE(this) : new StylesheetPackage(this);
    }


    /**
     * Factory method to make the AccumulatorManager, used for static information
     * about the accumulators used in a particular package
     *
     * @return a new AccumulatorManager appropriate to this Configuration
     */
    @Override
    public AccumulatorRegistry makeAccumulatorRegistry() {
        return new AccumulatorRegistryEE();
    }

    /**
     * Create a function library for Saxon-defined extension functions available in Enterprise Edition
     *
     * @return the new function library
     */
    public BuiltInFunctionSet getVendorFunctionSet() {
        if (isLicensedFeature(LicenseFeature.SCHEMA_VALIDATION)) {
            return VendorFunctionSetEE.getInstance();
        } else {
            return super.getVendorFunctionSet();
        }
    }

    /**
     * Make a UserFunction object.
     * This method is for internal use.
     *
     * @param memoFunction  true if the function is to be a memo function, This option is ignored
     *                      in Saxon-HE.
     * @param streamability the declared streamability of the function
     */

    @Override
    public UserFunction newUserFunction(boolean memoFunction, FunctionStreamability streamability) {
        if (memoFunction) {
            return new MemoFunction();
        } else if (streamability != FunctionStreamability.UNCLASSIFIED) {
            return new StreamableUserFunction();
        } else {
            return new UserFunction();
        }
    }

    /**
     * Get the TypeHierarchy: a cache holding type information
     *
     * @return the type hierarchy cache
     */

    @Override
    public TypeHierarchy getTypeHierarchy() {
        if (typeHierarchy == null) {
            typeHierarchy = new TypeHierarchyEE(this);
        }
        return typeHierarchy;
    }

    /**
     * Set a user-defined SchemaURIResolver for resolving URIs used in "import schema"
     * declarations.
     *
     * @param resolver the URI resolver used for import schema declarations
     */

    public void setSchemaURIResolver(SchemaURIResolver resolver) {
        schemaURIResolver = resolver;
        resolver.setConfiguration(this);
    }

    @Override
    public InvalidityReportGeneratorEE createValidityReporter() {
        return new InvalidityReportGeneratorEE(this);
    }


    @Override
    public PackageLoaderEE makePackageLoader() {
        return new PackageLoaderEE(this);
    }

    /**
     * Make a SchemaCompiler which can be used to compile schema documents. The initial settings of the SchemaCompiler
     * are a snapshot of the settings in the EnterpriseConfiguration. Other methods in EnterpriseConfiguration
     * provided for the purpose of compiling schemas are retained for compatibility reasons.
     *
     * @return a new SchemaCompiler object
     * @since 8.9
     */

    public SchemaCompiler newSchemaCompiler() {
//        checkLicensedFeature(LicenseFeature.SCHEMA_VALIDATION, "schema processing", -1);
        SchemaCompiler compiler = new SchemaCompiler(this);
        compiler.setLanguageVersion(xsdVersion);
        return compiler;
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
     */

    public int[] getOccurrenceLimits() {
        return occurrenceLimits;
    }

    /**
     * Get the user-defined SchemaURIResolver for resolving URIs used in "import schema"
     * declarations; if none has been explicitly set, returns the standard schema URI resolver.
     *
     * @return the user-defined SchemaURIResolver if there is one; otherwise the standard
     * schema URI resolver.
     */

    public SchemaURIResolver getSchemaURIResolver() {
        SchemaURIResolver r = schemaURIResolver;
        if (r == null) {
            r = standardSchemaResolver;
        }
        return r;
    }

    /**
     * Read a schema from a given schema location
     *
     * @param pipe           the PipelineConfiguration
     * @param baseURI        the base URI for resolving relative URIs
     * @param schemaLocation the location "hint" where to find the schema document. Must not be null.
     *                       The document located may be either an XSD schema document or an SCM compiled schema.
     * @param expected       the target namespace expected; null if there is no
     *                       expectation
     * @return the actual target namespace of the schema
     */

    @Override
    public NamespaceUri readSchema(PipelineConfiguration pipe, String baseURI, String schemaLocation, NamespaceUri expected)
            throws SchemaException {
        //noinspection SynchronizeOnNonFinalField
        synchronized(superSchema) {
            int status = getNamespaceStatus(expected);
            if (status == PreparedSchema.NAMESPACE_UNKNOWN || (status == PreparedSchema.NAMESPACE_KNOWN && getConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS))) {
                SchemaCompiler schemaCompiler = newSchemaCompiler();
                if (pipe.getErrorReporter() != null) {
                    schemaCompiler.setErrorReporter(pipe.getErrorReporter());
                }
                NamespaceUri ns = schemaCompiler.readSchema(pipe, baseURI, schemaLocation, expected);
                schemaCompiler.makeAllAutomata();
                schemaCompiler.checkAllAutomata();
                return ns;
            } else {
                return expected;
            }
        }
    }

    /**
     * Read schema documents from a list of schema locations
     * @param pipe            the pipeline configuration
     * @param baseURI         the base URI for resolving relative URIs
     * @param schemaLocations the list of location "hints" where to find the schema document. Must not be null.
     * @param expected        the target namespace expected in each of the schema documents; null if there is no
     */

    @Override
    public void readMultipleSchemas(PipelineConfiguration pipe, String baseURI, List<String> schemaLocations, NamespaceUri expected)
            throws SchemaException {
        newSchemaCompiler().readMultipleSchemas(pipe, baseURI, schemaLocations, expected);
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
     */

    @Override
    public NamespaceUri readInlineSchema(NodeInfo root, NamespaceUri expected, ErrorReporter errorReporter)
            throws SchemaException {
        return newSchemaCompiler().readInlineSchema(root, expected, errorReporter);
    }

    /**
     * Load a schema, which will be available for use by all subsequent operations using
     * this EnterpriseConfiguration.
     *
     * @param schemaSource  the JAXP Source object identifying the schema document to be loaded. This may be either
     *                      a source schema document in XSD format, or a compiled schema in Saxon's SCM format
     * @param errorReporter the ErrorReporter to be notified of any errors in the schema. If null,
     *                      the configuration's ErrorReporter is used
     * @throws SchemaException if the schema cannot be read or parsed or if it is invalid
     */

    @Override
    public void addSchemaSource(Source schemaSource, /*@Nullable*/ ErrorReporter errorReporter) throws SchemaException {
        addSchemaSource(schemaSource, errorReporter, newSchemaCompiler());
    }


    /**
     * Load a schema, which will be available for use by all subsequent operations using
     * this EnterpriseConfiguration.
     *
     * @param schemaSource  the JAXP Source object identifying the schema document to be loaded. This may be either
     *                      a source schema document in XSD format, or a compiled schema in Saxon's SCM format
     * @param errorReporter the ErrorListener to be notified of any errors in the schema. If null,
     *                      the configuration's errorListener is used
     * @param compiler      the schema compiler to be used. This allows a cache of schema documents (retained by the
     *                      schema compiler) to be reused when compiling multiple schema documents. See bug #4982.
     * @throws SchemaException if the schema cannot be read or parsed or if it is invalid
     */

    public void addSchemaSource(Source schemaSource, /*@Nullable*/ ErrorReporter errorReporter, SchemaCompiler compiler) throws SchemaException {
        try {
            //#if CSHARP==false
            if (schemaSource instanceof SAXSource && getBooleanProperty(Feature.IGNORE_SAX_SOURCE_PARSER)) {
                // This option is provided to allow the parser set by applications such as Ant to be overridden by
                // the parser requested using FeatureKeys.SOURCE_PARSER or FeatureKeys.STYLE_PARSER
                ((SAXSource) schemaSource).setXMLReader(null);
            }
            //#endif
            if (errorReporter != null) {
                compiler.setErrorReporter(errorReporter);
            }
            final PipelineConfiguration pipe = makePipelineConfiguration();
            pipe.setErrorReporter(errorReporter == null ? makeErrorReporter() : errorReporter);
            pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
            SchemaDocument schemadoc = SchemaReader.read(schemaSource, compiler, pipe, null);
            if (schemadoc.isCompiledSchemaDocument()) {
                try {
                    importComponents(schemadoc.getDocument().getRootNode().asActiveSource());
                } catch (XPathException e) {
                    throw new SchemaException(e);
                }
            } else {
                compiler.validate(schemadoc);
                setNamespaceStatus(schemadoc.getTargetNamespace(), PreparedSchema.NAMESPACE_KNOWN);
            }

        } catch (UncheckedXPathException e) {
            // indicates that the error reporter asked for termination
            XPathException xe = e.getXPathException();
            if (xe instanceof SchemaException) {
                throw (SchemaException)xe;
            } else {
                throw new SchemaException(xe);
            }
        }
    }

    /**
     * Add a schema (a collection of schema components) to the cache
     *
     * @param pschema The schema to be added
     * @throws net.sf.saxon.type.SchemaException if the schema cannot be added because components in the schema
     *                                           are incompatible with components already in the cache
     */

    public synchronized void addSchema(PreparedSchema pschema) throws SchemaException {
        checkLicensedFeature(LicenseFeature.SCHEMA_VALIDATION, "schema processing", pschema.getLocalLicenseId());
        if (pschema == superSchema) {
            return;
        }
        if (pschema instanceof SingleNamespaceSchema) {
            NamespaceUri targetNamespace = ((SingleNamespaceSchema) pschema).getTargetNamespace();
            setNamespaceStatus(targetNamespace, PreparedSchema.NAMESPACE_KNOWN);
        } else {
            for (NamespaceUri ns : pschema.getTargetNamespaces()) {
                setNamespaceStatus(ns, PreparedSchema.NAMESPACE_KNOWN);
            }
        }
        pschema.copyTo(superSchema, newSchemaCompiler());
        superSchema.setLocalLicenseId(pschema.getLocalLicenseId());

    }

    /**
     * Put a temporary schema marker in the cache, to indicate that a schema for a given
     * namespace is being processed. Used to detect circularities.
     * <p>This method is intended for internal use</p>
     *
     * @param namespace the target namespace of the schema
     */

    public synchronized void addSurrogateSchema(NamespaceUri namespace) {
        checkLicensedFeature(LicenseFeature.SCHEMA_VALIDATION, "schema processing", -1);
        setNamespaceStatus(namespace, PreparedSchema.NAMESPACE_UNDER_CONSTRUCTION);
    }

    /**
     * Add a built-in schema for a given namespace. This is a no-op if the configuration is not schema-aware
     *
     * @param namespace the namespace. Currently built-in schemas are available for the XML, XSI, and FN namespaces
     */

    @Override
    public void addSchemaForBuiltInNamespace(NamespaceUri namespace) {
        if (getNamespaceStatus(namespace) != PreparedSchema.NAMESPACE_UNKNOWN) {
            return;
        }
        if (namespace == NamespaceUri.XML) {
            addSchemaForXmlNamespace();
        } else if (namespace == NamespaceUri.SCHEMA_INSTANCE) {
            addSchemaForXsiNamespace();
        } else if (namespace == NamespaceUri.FN) {
            InputStream in = Version.platform.locateResource("xpath-functions.scm", new ArrayList<String>());
            if (isTiming()) {
                getLogger().info("Loading schema from resources for: " + NamespaceConstant.FN);
            }
            try {
                addSchemaSource(new StreamSource(in), makeErrorReporter());
            } catch (SchemaException e) {
                throw new IllegalStateException("Failed to load internal copy of xpath-functions.scm");
            }
        } else {
            throw new IllegalArgumentException(namespace.toString());
        }
    }

    private synchronized void addSchemaForXmlNamespace() {
        if (getNamespaceStatus(NamespaceUri.XML) == PreparedSchema.NAMESPACE_UNKNOWN) {
            try {
                addSchema(XMLNamespaceSchema.buildSchema(this));
            } catch (SchemaException err) {
                throw new AssertionError(err);
            }
            setNamespaceStatus(NamespaceUri.XML, PreparedSchema.NAMESPACE_KNOWN);
        }
    }

    private synchronized void addSchemaForXsiNamespace() {
        if (getNamespaceStatus(NamespaceUri.SCHEMA_INSTANCE) == PreparedSchema.NAMESPACE_UNKNOWN) {
            try {
                addSchema(XSINamespaceSchema.buildSchema(this));
            } catch (SchemaException err) {
                throw new AssertionError(err);
            }
            setNamespaceStatus(NamespaceUri.SCHEMA_INSTANCE, PreparedSchema.NAMESPACE_KNOWN);
        }
    }

    /**
     * Get the status of a particular schema namespace.
     *
     * @param namespace the Namespace URI whose status is required (supply "" for the non-namespace)
     * @return one of {@link PreparedSchema#NAMESPACE_KNOWN}, {@link PreparedSchema#NAMESPACE_UNKNOWN},
     * {@link PreparedSchema#NAMESPACE_UNDER_CONSTRUCTION}, {@link PreparedSchema#NAMESPACE_SEALED}
     */

    public synchronized int getNamespaceStatus(NamespaceUri namespace) {
        return namespaceStatus.getOrDefault(namespace, PreparedSchema.NAMESPACE_UNKNOWN);
    }

    /**
     * Set the status of a particular schema namespace.
     *
     * @param namespace the Namespace URI whose status is required (supply "" for the non-namespace)
     * @param status    one of {@link PreparedSchema#NAMESPACE_KNOWN}, {@link PreparedSchema#NAMESPACE_UNKNOWN},
     *                  {@link PreparedSchema#NAMESPACE_UNDER_CONSTRUCTION}, {@link PreparedSchema#NAMESPACE_SEALED}
     */

    public synchronized void setNamespaceStatus(NamespaceUri namespace, int status) {
        if (status == PreparedSchema.NAMESPACE_UNKNOWN) {
            namespaceStatus.remove(namespace);
        } else {
            namespaceStatus.put(namespace, status);
        }
    }

    /**
     * Determine whether the Configuration contains a cached schema for a given target namespace
     *
     * @param targetNamespace the target namespace of the schema being sought (supply "" for the
     *                        unnamed namespace)
     * @return true if the schema for this namespace is available, false if not.
     */

    @Override
    public boolean isSchemaAvailable(NamespaceUri targetNamespace) {
        return getNamespaceStatus(targetNamespace) != PreparedSchema.NAMESPACE_UNKNOWN;
    }

    /**
     * Remove the schema for a given namespace. This is done if it is found to be invalid.
     * Note that this does not actually remove the schema components in this namespace; all it
     * does is to mark the namespace as removed, which means that a new attempt to load a schema
     * for this namespace is able to succeed.
     *
     * @param namespace the namespace of the schema to be removed
     */

    public synchronized void removeSchema(NamespaceUri namespace) {
        namespaceStatus.remove(namespace);
    }

    /**
     * Get the set of namespaces of imported schemas
     * @return the set of namespaces of imported schemas
     */

    @Override
    public synchronized Set<NamespaceUri> getImportedNamespaces() {
        return namespaceStatus.keySet();
    }

    /**
     * Get the SuperSchema, containing references to all names defined in all
     * known schemas
     *
     * @return the PreparedSchema containing the schema components for all loaded target namespaces
     */

    public PreparedSchema getSuperSchema() {
        return superSchema;
    }

    /**
     * Mark a schema namespace as being sealed. This is done when components from this namespace
     * are first used for validating a source document or compiling a source document or query. Once
     * a namespace has been sealed, it is not permitted to change the schema components in that namespace
     * by redefining them, deriving new types by extension, or adding to their substitution groups.
     *
     * @param namespace the namespace URI of the components to be sealed
     */

    @Override
    public void sealNamespace(NamespaceUri namespace) {
        if (!namespace.equals(NamespaceUri.ANONYMOUS)) {
            setNamespaceStatus(namespace, PreparedSchema.NAMESPACE_SEALED);
        }
    }

    /**
     * Test whether a schema namespace is sealed. Once
     * a namespace has been sealed, it is not permitted to change the schema components in that namespace
     * by redefining them, deriving new types by extension, or adding to their substitution groups.
     *
     * @param namespace the namespace URI of the components to be tested. Supply "" for the no-namespace.
     * @return true if this namespace has been sealed, otherwise false
     */

    public boolean isSealedNamespace(NamespaceUri namespace) {
        return getNamespaceStatus(namespace) == PreparedSchema.NAMESPACE_SEALED;
    }

    /**
     * Get the set of saxon:param schema parameters declared in the schema held by this Configuration.
     *
     * @return the set of parameters. May return null if none have been declared.
     */

    @Override
    public Collection<GlobalParam> getDeclaredSchemaParameters() {
        return getSuperSchema().getGlobalParams();
    }

    /**
     * Get the set of complex types that have been defined as extensions of a given type.
     * Note that we do not seal the schema namespace, so this list is not necessarily final; we must
     * assume that new extensions of built-in simple types can be added at any time
     *
     * @param type the type whose extensions are required
     */

    @Override
    public Iterable<? extends SchemaType> getExtensionsOfType(SchemaType type) {
        if (type instanceof UserDefinedType) {
            return ((UserDefinedType) type).getExtensionTypes();
        } else {
            return getSuperSchema().getExtensionsOfBuiltInType(type.getFingerprint());
        }
    }

    /**
     * Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
     * document are added to the cache of schema components maintained by this SchemaManager
     *
     * @param source the XML file containing the schema component model, as generated by a previous call on
     *               {@link Configuration#exportComponents}
     */

    @Override
    public void importComponents(Source source) throws XPathException {
        try {
            SchemaModelLoader loader = new SchemaModelLoader(this);
            PreparedSchema schema = loader.load(source);
            addSchema(schema);
            schema.copyTo(superSchema, newSchemaCompiler());
            for (NamespaceUri ns : schema.getTargetNamespaces()) {
                setNamespaceStatus(ns, PreparedSchema.NAMESPACE_KNOWN);
            }
        } catch (SchemaException e) {
            throw new XPathException(e);
        }
    }

    /**
     * Export a precompiled Schema Component Model containing all the components (except built-in components)
     * that have been loaded into this Processor.
     *
     * @param out the destination to recieve the precompiled Schema Component Model in the form of an
     *            XML document
     */

    @Override
    public void exportComponents(Receiver out) throws XPathException {
        SchemaModelSerializer serializer = new SchemaModelSerializer(this, out);
        serializer.serialize();
    }

    /**
     * Remove all schema components that have been loaded into this Configuration.
     * This method must not be used if any processes (such as stylesheet or query compilations
     * or executions) are currently active. In a multi-threaded environment, it is the user's
     * responsibility to ensure that this method is not called unless it is safe to do so.
     */

    @Override
    public void clearSchemaCache() {
        superSchema = new PreparedSchema(this);
        namespaceStatus = new HashMap<>();
    }


    /**
     * Get a global element declaration by fingerprint
     *
     * @param fingerprint the NamePool fingerprint of the element name
     * @return the element declaration whose name matches the given
     * fingerprint, or null if no element declaration with this name has
     * been registered.
     */

    @Override
    public SchemaDeclaration getElementDeclaration(int fingerprint) {
        return superSchema.getElementDecl(fingerprint);
    }

    /**
     * Get a global element declaration by name.
     *
     * @param qName the name of the required
     *              element declaration
     * @return the element declaration whose name matches the given
     * name, or null if no element declaration with this name has
     * been registered.
     */


    @Override
    public SchemaDeclaration getElementDeclaration(StructuredQName qName) {
        return superSchema.getElementDecl(qName);
    }

    /**
     * Get a global attribute declaration by fingerprint
     *
     * @param fingerprint the NamePool fingerprint of the element name
     * @return the attribute declaration whose name matches the given
     * fingerprint, or null if no element declaration with this name has
     * been registered.
     */

    @Override
    public SchemaDeclaration getAttributeDeclaration(int fingerprint) {
        return superSchema.getAttributeDecl(fingerprint);
    }

    /**
     * Get a global attribute declaration by name
     *
     * @param attributeName the name of the required attribute declaration
     * @return the attribute declaration whose name matches the given
     * fingerprint, or null if no element declaration with this name has
     * been registered.
     */

    @Override
    public SchemaDeclaration getAttributeDeclaration(StructuredQName attributeName) {
        return superSchema.getAttributeDecl(attributeName);
    }

    /**
     * Get the top-level schema type definition with a given QName.
     *
     * @param name the name of the required schema type
     * @return the schema type , or null if there is none
     * with this name.
     * @since 9.7
     */

    /*@Nullable*/
    @Override
    public SchemaType getSchemaType(StructuredQName name) {
        if (name.hasURI(NamespaceUri.SCHEMA)) {
            SchemaType t = BuiltInType.getSchemaTypeByLocalName(name.getLocalPart());
            if (t != null) {
                return t;
            }
            // Otherwise fall through. This can happen when validating against the schema for schemas:
            // there are types such as xs:attribute whose names are built-in names with codes < 1024,
            // but which are not built-in types.
        }
        return superSchema.getType(name);
    }

    /**
     * Make a union type with a given list of member types
     *
     * @param memberTypes the member types
     * @return the union type with these member types
     */

    @Override
    public UserUnionType makeUserUnionType(List<AtomicType> memberTypes) {
        List<TypeReference> memberRefs = new ArrayList<>(memberTypes.size());
        for (AtomicType at : memberTypes) {
            TypeReference ref = new TypeReference(at.getFingerprint(), this, Loc.NONE);
            memberRefs.add(ref);
        }
        UserUnionType uut = new UserUnionType(this);
        StringBuilder fsb = new StringBuilder(256);
        String sep = "";
        for (AtomicType at : memberTypes) {
            fsb.append(sep);
            sep = "-";
            fsb.append(at.getName());
        }
        String localName = fsb.toString();
        StructuredQName name = new StructuredQName("anon", NamespaceUri.ANONYMOUS, localName);
        uut.setTypeName(name, getNamePool().allocateFingerprint(NamespaceUri.ANONYMOUS, localName));
        uut.setMemberTypeReferences(memberRefs);
        return uut;
    }

    /**
     * Ask whether a given notation has been declared in the schema
     *
     * @param uri   the targetNamespace of the notation
     * @param local the local part of the notation name
     * @return true if the notation has been declared, false if not
     * @since 9.3
     */

    @Override
    public boolean isDeclaredNotation(NamespaceUri uri, String local) {
        return superSchema.getNotation(new StructuredQName("", uri, local)) != null;
    }

    /**
     * Check that a type is validly derived from another type, following the rules for the Schema Component
     * Constraint "Is Type Derivation OK (Simple)" (3.14.6) or "Is Type Derivation OK (Complex)" (3.4.6) as
     * appropriate.
     *
     * @param derived the derived type
     * @param base    the base type; the algorithm tests whether derivation from this type is permitted
     * @param block   the derivations that are blocked by the relevant element declaration
     * @throws SchemaException if the derivation is not allowed
     */

    @Override
    public void checkTypeDerivationIsOK(SchemaType derived, SchemaType base, int block)
            throws SchemaException {
        if (derived.isSimpleType()) {
            String s = UserSimpleType.isTypeDerivationOK((SimpleType) derived, base, block);
            if (s != null) {
                throw new SchemaException(s);
            }
        } else {
            UserComplexType.checkTypeDerivation((ComplexType) derived, base, block);
        }
    }

    /**
     * Set validation reporting options. Called by instructions that invoke validation
     * to set up an appropriate invalidity handler
     *
     * @param context the XPath evaluation context
     * @param options the parser options, to be updated
     */

    @Override
    public void prepareValidationReporting(XPathContext context, ParseOptions options)  {
//        if (context.getThreadManager() != null) {
//            // we're in a try/catch
//            Builder builder = context.getController().makeBuilder();
//            options.setInvalidityHandler(new InvalidityReportGeneratorEE(context.getConfiguration(), builder));
//        }
    }

    /**
     * Get a document-level validator to add to a Receiver pipeline
     *
     * @param receiver           The receiver to which events should be sent after validation
     * @param systemId           the base URI of the document being validated
     * @param validationOptions  Supplies options relevant to XSD validation
     * @param initiatingLocation (optional) Location in query or stylesheet of instruction
     *                           that requested validation
     * @return A Receiver to which events can be sent for validation
     */

    @Override
    public Receiver getDocumentValidator(Receiver receiver,
                                         String systemId,
                                         ParseOptions validationOptions,
                                         Location initiatingLocation) {
//        if (isLicenseNotFound()) {
//            return receiver;
//        }
        int validationMode = validationOptions.getSchemaValidationMode();
        //int stripSpace = validationOptions.getStripSpace();
        SpaceStrippingRule stripRules = validationOptions.getSpaceStrippingRule();
        SchemaType schemaType = validationOptions.getTopLevelType();
        StructuredQName topLevelElementName = validationOptions.getTopLevelElement();
        ValidationParams params = validationOptions.getValidationParams();

        PipelineConfiguration pipe = receiver.getPipelineConfiguration();
        if (validationMode == Validation.STRICT || validationMode == Validation.LAX || schemaType != null) {
            Receiver destination = receiver;
            if (stripRules == null) {
                stripRules = getParseOptions().getSpaceStrippingRule();
            }
            if (stripRules == IgnorableSpaceStrippingRule.getInstance()) {
                destination = stripRules.makeStripper(destination);
            }

            ValidationContext vc = new ValidationContext(getConversionRules());
            vc.setInitiatingLocation(initiatingLocation);
            vc.setErrorLimit(validationOptions.getValidationErrorLimit());
            if (pipe.getController() == null) {
                pipe.setController(new Controller(this));
                pipe.getController().getExecutable().setSchemaAware(true);
            }
            vc.setController(pipe.getController());
            vc.setValidationParams(params);

            if (validationOptions.isCheckEntityReferences()) {
                EntityValidator ev = new EntityValidator(destination);
                ev.setValidationContext(vc);
                destination = ev;
            }

            allocateInvalidityHandler(validationOptions, vc);

            IdValidator idval = new IdValidator(destination);
            idval.setValidationContext(vc);
            ConstraintChecker checker = new ConstraintChecker(pipe /*idval*/);
            checker.setValidationContext(vc);

            destination = new TeeOutputter(idval, checker);

            // If running XSD 1.1, add an assertion tree builder before the constraint checker
            destination = allocateAssertionTreeBuilder(destination, pipe, vc);


            ValidationStack val = new ValidationStack(destination, validationMode, schemaType);
            val.setPipelineConfiguration(pipe);
            val.setValidationContext(vc);

            vc.setConstraintChecker(checker);
            if (topLevelElementName != null) {
                val.setTopLevelElement(new FingerprintedQName(
                        topLevelElementName.getPrefix(),
                        topLevelElementName.getNamespaceUri(),
                        topLevelElementName.getLocalPart()));
            }
            HostLanguage hostLanguage = pipe.getHostLanguage();
            if (hostLanguage == HostLanguage.XSLT) {
                if (schemaType != null) {
                    vc.setErrorCode("XTTE1540");
                } else if (validationMode == Validation.STRICT) {
                    vc.setErrorCode("XTTE1510");
                } else {
                    vc.setErrorCode("XTTE1515");
                }
            } else if (hostLanguage == HostLanguage.XQUERY) {
                vc.setErrorCode("XQDY0027");
            } else if (hostLanguage == HostLanguage.XML_SCHEMA) {
                vc.setErrorCode(null);
            }

            XSIAttributeHandler xsi = new XSIAttributeHandler(val);
            xsi.setSystemId(systemId);

            AttributeInheritor stb = new AttributeInheritor(xsi);
            vc.setAttributeInheritor(stb);
            xsi.setValidationContext(vc);

            PathMaintainer pathMaintainer = new PathMaintainer(stb);
            pipe.setComponent("net.sf.saxon.event.PathMaintainer", pathMaintainer);

            DocumentValidator dv = new DocumentValidator(pathMaintainer, hostLanguage == HostLanguage.XSLT ? "XTTE1550" : "XQDY0061");

            LocationCopier copier = new LocationCopier(true, systemId);
            pipe.setCopyInformee(CSharp.methodRef(copier::notifyElementNode));

            vc.startValidation(systemId);

            return dv;
            //return new TracingFilter(dv);

        } else if (validationMode == Validation.STRIP) {
            SkipValidator a = new SkipValidator(receiver);
            ValidationContext vc = new ValidationContext(getConversionRules());
            a.setValidationContext(vc);
            a.setSystemId(systemId);
            return a;
        } else {
            return receiver;
        }
    }

    private void allocateInvalidityHandler(ParseOptions validationOptions, ValidationContext vc) {
        if (validationOptions.getInvalidityHandler() == null) {
            ErrorReporter reporter = validationOptions.getErrorReporter();
            if (reporter == null) {
                Controller controller = vc.getController();
                if (controller != null) {
                    reporter = controller.getErrorReporter();
                } else {
                    reporter = makeErrorReporter();
                }
            }
            vc.setInvalidityHandler(new InvalidityHandlerWrappingErrorReporter(reporter));
        } else {
            vc.setInvalidityHandler(validationOptions.getInvalidityHandler());
        }
    }

    /**
     * Get a Receiver that can be used to validate an element, and that passes the validated
     * element on to a target receiver. If validation is not supported, the returned receiver
     * will be the target receiver.
     *
     * @param receiver the target receiver to receive the validated element; its pipelineConfiguration
     *                 must be initialized.
     * @param options  options affecting the way schema validation is done
     * @param location identifies the location of the element within its source document (or, if
     *                 preferred, the location of the instruction requesting validation), relative to the LocationProvider
     *                 in the PipelineConfiguration of the receiver
     * @return The newly created receiver, which performs the validation and passes the validated
     * elements on to the target receiver.
     */

    @Override
    public Receiver getElementValidator(Receiver receiver,
                                        ParseOptions options,
                                        Location location) throws XPathException {

//        if (isLicenseNotFound()) {
//            return receiver;
//        }

        int validation = options.getSchemaValidationMode();
        SchemaType schemaType = options.getTopLevelType();
        StructuredQName elemName = options.getTopLevelElement();

        if (validation == Validation.STRIP) {
            if (receiver.usesTypeAnnotations()) {
                return new SkipValidator(receiver);
            } else {
                return receiver;
            }
        }

        Receiver destination = receiver;

        PipelineConfiguration pipe = receiver.getPipelineConfiguration();
        pipe.setRecoverFromValidationErrors(isValidationWarnings() || options.isContinueAfterValidationErrors());

        ValidationContext vc = new ValidationContext(getConversionRules());
        vc.setErrorLimit(options.getValidationErrorLimit());
        vc.setController(pipe.getController());
        vc.setValidationParams(options.getValidationParams());
        vc.setInitiatingLocation(location);
        allocateInvalidityHandler(options, vc);

        // Checker for uniqueness constraints added in Saxon 9.3: see bug 4353
        ConstraintChecker checker = new ConstraintChecker(pipe /*destination*/);
        checker.setValidationContext(vc);
        destination = new TeeOutputter(destination, checker);
        vc.setConstraintChecker(checker);
        //destination = new TracingFilter(destination);

        // If running XSD 1.1, add an assertion tree builder before the constraint checker
        destination = allocateAssertionTreeBuilder(destination, pipe, vc);

        // Add a validator to the output pipeline if required

        ContentValidator contentValidator = null;
        if (validation == Validation.BY_TYPE) {
            List<ValidationFailure> failures = new ArrayList<>(1);
            contentValidator = ContentValidator.makeValidatorForType(null, schemaType, pipe, destination, failures);
            if (!failures.isEmpty()) {
                throw failures.get(0).makeException();
            }
            assert contentValidator != null;
            contentValidator.setContainingElement(options.getTopLevelElement(), location);
            contentValidator.setValidationContext(vc);

        } else if (validation != Validation.PRESERVE) {
            if (elemName == null) {
                throw new IllegalArgumentException("When validating an element, either a type or an element name must be supplied");
            }
            ElementDecl decl = (ElementDecl) getElementDeclaration(elemName);
            if (decl == null || !decl.hasTypeAlternatives()) {
                List<ValidationFailure> failures = new ArrayList<>(1);
                contentValidator = ContentValidator.makeValidator(decl, elemName, location,
                                                                  vc, schemaType,
                                                                  validation,
                                                                  pipe, destination, failures);
                if (!failures.isEmpty()) {
                    throw failures.get(0).makeException();
                }
            }
        }

        ValidationStack vs = new ValidationStack(destination, Validation.STRICT, null);
        vs.setValidationContext(vc);
        vs.setPipelineConfiguration(pipe);
        HostLanguage hostLanguage = pipe.getHostLanguage();
        if (hostLanguage == HostLanguage.XSLT) {
            if (validation == Validation.BY_TYPE) {
                vc.setErrorCode("XTTE1540");
            } else if (validation == Validation.STRICT) {
                vc.setErrorCode("XTTE1510");
            } else {
                vc.setErrorCode("XTTE1515");
            }
        } else if (hostLanguage == HostLanguage.XQUERY) {
            vc.setErrorCode("XQDY0027");
        } else if (hostLanguage == HostLanguage.XML_SCHEMA) {
            vc.setErrorCode(null);
        }

        if (contentValidator != null) {
            vs.setInitialValidator(contentValidator);
        }

        XSIAttributeHandler xsi = new XSIAttributeHandler(vs);
        AttributeInheritor inheritor = new AttributeInheritor(xsi);

        vc.setAttributeInheritor(inheritor);
        xsi.setValidationContext(vc);

        destination = inheritor;
        vc.startValidation(location.getSystemId());
        return destination;
    }

    private Receiver allocateAssertionTreeBuilder(Receiver destination, PipelineConfiguration pipe, ValidationContext vc) {
        if (xsdVersion == XSD11) {
            boolean retainComments = getBooleanProperty(Feature.ASSERTIONS_CAN_SEE_COMMENTS);
            AssertionTreeBuilder atb = new AssertionTreeBuilder(destination, retainComments);
            atb.setPipelineConfiguration(pipe);
            vc.setAssertionTreeBuilder(atb);
            pipe.setComponent("com.saxonica.ee.validate.AssertionTreeBuilder", atb);
            destination = atb;
        }
        return destination;
    }

    /**
     * Validate an attribute value (XSLT only)
     *
     * @param nodeName   the name of the attribute
     * @param value      the value of the attribute as a string
     * @param validation STRICT or LAX
     * @return an int, containing the type annotation to apply to the attribute node
     * @throws ValidationException if the value is invalid
     */

    @Override
    public SimpleType validateAttribute(StructuredQName nodeName, UnicodeString value, int validation)
            throws ValidationException, MissingComponentException {
        if (isLicenseNotFound()) {
            return BuiltInAtomicType.UNTYPED_ATOMIC;
        }
        AttributeDecl decl = (AttributeDecl) getAttributeDeclaration(nodeName);
        if (decl == null) {
            if (validation == Validation.STRICT) {
                ValidationFailure ve = new ValidationFailure(
                        "No global attribute declaration found for attribute " +
                                nodeName.getDisplayName());
                ve.setErrorCode("XTTE1512");
                throw ve.makeException();
            } else {
                // lax validation
                return BuiltInAtomicType.UNTYPED_ATOMIC;
            }
        }
        SimpleType type = decl.getSimpleType();
        if (type.isNamespaceSensitive()) {
            ValidationFailure nsErr = new ValidationFailure("Cannot create a parentless attribute whose " +
                                                                  "type is namespace-sensitive (such as xs:QName)");
            nsErr.setSchemaType(type);
            nsErr.setErrorCode("XTTE1545");
            throw nsErr.makeException();
        }
        ValidationFailure contentErr = type.validateContent(value, DummyNamespaceResolver.getInstance(), getConversionRules());
        if (contentErr != null) {
            contentErr.setSchemaType(type);
            contentErr.setErrorCode(validation == Validation.STRICT ? "XTTE1510" : "XTTE1515");
            throw contentErr.makeException();
        }
        return type;
    }

    /**
     * Add to a pipeline a receiver that strips all type annotations. This
     * has a null implementation in the Saxon-HE product, because type annotations
     * can never arise.
     * @param destination the destination for events after stripping type annotations
     * @return the type-stripping receiver
     */

    @Override
    public Receiver getAnnotationStripper(Receiver destination) {
        return new SkipValidator(destination);
    }

    /**
     * Make an expression Parser for a specified version of XPath or XQuery
     *
     * @param language        set to "XP" (XPath) or "XQ" (XQuery) or "PATTERN" (XSLT Patterns)
     * @param updating        indicates whether or not XQuery update syntax may be used. Note that XQuery Update
     *                        is supported only in Saxon-EE
     * @return the QueryParser
     * @throws net.sf.saxon.trans.XPathException if this version of Saxon does not support the
     *                                           requested options
     */

    @Override
    public XPathParser newExpressionParser(String language, boolean updating) throws XPathException {
        if (updating && !isLicensedFeature(LicenseFeature.ENTERPRISE_XQUERY)) {
            throw new IllegalArgumentException("XQuery Update requires a Saxon-EE license");
        }
        if ("XQ".equals(language)) {
            XQueryParser parser = new XQueryParser();
            if (isLicensedFeature(LicenseFeature.ENTERPRISE_XQUERY)) {
                parser.setParserExtension(new XQueryParserExtensionEE(updating));
            } else if (isLicensedFeature(LicenseFeature.PROFESSIONAL_EDITION)) {
                parser.setParserExtension(new XQueryParserExtensionPE());
            }
            return parser;
        } else {
            XPathParser parser = super.newExpressionParser(language, updating);
            parser.setAccelerator(new QuickXPathParser());
            return parser;
        }
    }

    /**
     * Get a new ExpressionPresenter capable of exporting a compiled stylesheet
     *
     * @param target      the target environment, e.g. EE, JS
     * @param destination the destination for the output file
     * @param rootPackage the compiled stylesheet package
     * @return a suitable Expression Presenter
     * @throws XPathException if none is available (e.g. because this is not Saxon-EE)
     */

    @Override
    public ExpressionPresenter newExpressionExporter(String target, OutputStream destination, StylesheetPackage rootPackage) throws XPathException {
        if (!isLicensedFeature(LicenseFeature.ENTERPRISE_XSLT)) {
            throw new XPathException("Stylesheet export requires a Saxon-EE license");
        }
        ExpressionPresenter presenter = new ExpressionPresenter();
        ExpressionPresenter.ExportOptions options = new ExpressionPresenter.ExportOptions();
        options.rootPackage = rootPackage;
        presenter.setOptions(options);
        switch (target) {
            case "JS":
                options.target = "JS";
                options.targetVersion = 2;
                PipelineConfiguration pipe = makePipelineConfiguration();
                JSONEmitter writer = new JSONEmitter(pipe, new UTF8Writer(destination), new Properties());
                presenter.init(this, new ExpressionPresenterToJson(writer, pipe), true);
                break;
            default:
                options.target = target;
                options.targetVersion = 1;
                presenter.init(this, new StreamResult(destination), true);
                break;
        }
        return presenter;
    }

    /**
     * Get a new Pending Update List
     */

    @Override
    public PendingUpdateList newPendingUpdateList() {
        return new PendingUpdateListImpl();
    }

    /**
     * Make a SchemaURIResolver that wraps a supplied URIResolver
     * @param resolver the resolver to be used
     * @return a new SchemaURIResolver (or null if this is not an EnterpriseConfiguration)
     * @since 10.0
     */
    @Override
    public SchemaURIResolver makeSchemaURIResolver(ResourceResolver resolver) {
        StandardSchemaResolver schemaResolver = new StandardSchemaResolver(this);
        schemaResolver.setResourceResolver(resolver);
        return schemaResolver;
    }


    /**
     * Create a document projector for a given path map root. Document projection is available only
     * in Saxon-EE, so the Saxon-HE version of this method throws an exception
     *
     * @param map a path map root in a path map. This might represent the call to the initial
     *            context item for a query, or it might represent a call on the doc() function. The path map
     *            contains information about the paths that the query uses within this document.
     * @return a push filter that implements document projection
     * @throws UnsupportedOperationException if this is not a schema-aware configuration, or
     *                                       if no Saxon-EE license is available
     */


    @Override
    public FilterFactory makeDocumentProjector(final PathMap.PathMapRoot map) {
        checkLicensedFeature(LicenseFeature.ENTERPRISE_XQUERY, "document projection", -1);
        return next -> new Projector(map, next);
    }

    /**
     * Create a document projector for the document supplied as the initial context item
     * in a query. Document projection is available only
     * in Saxon-EE, so the Saxon-HE version of this method throws an exception
     *
     * @param exp an XQuery expression. The document projector that is returned will
     *            be for the document supplied as the context item to this query.
     * @return a push filter that implements document projection
     * @throws UnsupportedOperationException if this is not a schema-aware configuration, or
     *                                       if no Saxon-EE license is available
     */
    @Override
    public FilterFactory makeDocumentProjector(XQueryExpression exp) {
        if (exp instanceof XQueryExpressionEE) {
            PathMap map = ((XQueryExpressionEE) exp).getPathMap();
            PathMap.PathMapRoot contextRoot = map.getContextDocumentRoot();
            if (contextRoot != null) {
                if (!contextRoot.hasUnknownDependencies()) {
                    return makeDocumentProjector(contextRoot);
                }
            }
        }
        return null;
    }

    /**
     * Get a factory function that can be used to wrap a SequenceIterator in a FocusTrackingIterator. This
     * is called by the {@link Controller} to get a default factory function; the value can be overridden
     * by the application using {@link Controller#setFocusTrackerFactory(java.util.function.Function)}.
     * The {@link FocusTrackingIterator} that is created by the factory must be thread-safe if it is
     * to be used for iterating over a sequence where the items might be processed asynchronously using
     * <code>xsl:result-document</code>; for this reason this method is overridden for a Saxon-EE configuration.
     *
     * @param exec the executable; supplied so that the factory can be sensitive to whether calls on xsl:result-document
     *             are possible
     * @return a suitable factory function
     */


    @Override
    public java.util.function.Function<SequenceIterator, FocusTrackingIterator> getFocusTrackerFactory(
            Executable exec, boolean multithreaded) {
        if (multithreaded || (getBooleanProperty(Feature.ALLOW_MULTITHREADING) &&
                exec.createsSecondaryResult() &&
                isLicensedFeature(LicenseFeature.ENTERPRISE_XSLT))) {
            return CSharp.constructorRef(MultithreadedFocusTrackingIterator::new, 1);
        } else {
            return CSharp.constructorRef(FocusTrackingIterator::new, 1);
        }
    }

    /**
     * Set the streamability rules to be applied. Requires an EET license.
     *
     * @param enabled set to true if streamability checking (using W3C standard rules)
     *                is enabled for this configuration. Must be false under Saxon-HE
     * @since 9.6. Changed in 9.8 to drop support for streamability="extended".
     * Simplified and renamed (from {@code setStreamability}) in 10.0
     */

    public void setStreamabilityEnabled(boolean enabled) {
        this.streamabilityEnabled = Optional.of(enabled);
    }

    /**
     * Ask whether streamability checking is enabled for this configuration
     * @return always false for Saxon-HE; for Saxon-EE, true if an appropriate license is installed
     */

    @Override
    public boolean isStreamabilityEnabled() {
        if (!streamabilityEnabled.isPresent()) {
            streamabilityEnabled = Optional.of(isLicensedFeature(LicenseFeature.ENTERPRISE_XSLT)
                    || isLicensedFeature(LicenseFeature.ENTERPRISE_XQUERY));
        }
        return streamabilityEnabled.get();
    }

    /**
     * Create a streaming transformer
     *
     * @param mode    the initial mode, which must be a streaming mode
     * @param ordinaryParams the parameters that are not tunnel parameters
     * @param tunnelParams the tunnel parameters
     * @param out the destination for the transformed document
     * @param context the initial XPath context
     * @return a Receiver to which the streamed input document will be pushed
     */

    /*@NotNull*/
    @Override
    public Receiver makeStreamingTransformer(Mode mode, ParameterSet ordinaryParams, ParameterSet tunnelParams,
                                             Outputter out, XPathContext context) throws XPathException {
        checkLicensedFeature(LicenseFeature.ENTERPRISE_XSLT, "streaming", -1);
        PipelineConfiguration pipe = context.getController().makePipelineConfiguration();

        AccumulatorRegistryEE am = (AccumulatorRegistryEE)
                ((PreparedStylesheet) context.getController().getExecutable()).getTopLevelPackage().getAccumulatorRegistry();


        Set<? extends Accumulator> accumulators = null;

        if (mode instanceof ModeEE) {
            Set<? extends Accumulator> modeAccs = ((ModeEE) mode).getAccumulators();
            if (modeAccs != null) {
                accumulators = modeAccs;
            }
        }
        if (accumulators == null) {
            accumulators = Collections.emptySet();
        }

        WatchManager wm = new WatchManager(pipe);
        wm.setXPathContext(context);
        wm.setOutputter(out);

        if (am != null) {
            AccumulatorRegistryEE.registerSelectedAccumulators(wm, accumulators);
        }


        DecomposingFeed feed = new DecomposingFeed(out, context);
        ApplyTemplatesAction docWatch = new ApplyTemplatesAction(null, feed, context);
        docWatch.setMode(mode.getDeclaringComponent());
        docWatch.setWatchManager(wm);
        docWatch.setActualParams(ordinaryParams);
        docWatch.setTunnelParams(tunnelParams);

        Watch watch = new Trigger(new NodeTestPattern(NodeKindTest.DOCUMENT), docWatch, context);
        wm.addWatch(watch, true);

        return new ContentDetector(wm);
    }

    @Override
    public Expression makeStreamInstruction(Expression hrefExp, Expression body, boolean streaming, ParseOptions options,
                                            PackageData packageData, Location location, RetainedStaticContext rsc) throws XPathException {
        if (streaming && isLicensedFeature(LicenseFeature.ENTERPRISE_XSLT)) {
            StreamInstr si = new StreamInstr(hrefExp, body, options);
            si.setLocation(location);
            si.setRetainedStaticContext(rsc);
            if (getBooleanProperty(Feature.STRICT_STREAMABILITY)) {
                List<String> reasons = new ArrayList<>();
                si.checkStreamability(reasons, isStreamabilityEnabled(), true);
                ExpressionTool.clearStreamabilityData(si);
            }
            if (packageData != null) {
                // this happens when reloading a package from an SEF export
                return si.prepareForStreaming(packageData);
            }
            return si;
        } else {
            return super.makeStreamInstruction(hrefExp, body, streaming, options, packageData, location, rsc);
        }
    }

    /**
     * Check the streamability of a template rule
     */

    @Override
    public void checkStrictStreamability(XSLTemplate template, Expression body) throws XPathException {
        ContextItemStaticInfoEE info = (ContextItemStaticInfoEE)makeContextItemStaticInfo(template.getMatch().getItemType(), false);
        info.setContextPostureStriding();
        // TODO: we should be getting the type-adjusted posture and sweep
        List<String> reasons = new ArrayList<>();
        PostureAndSweep s = Streamability.getStreamability(body, info, reasons);
        if (s.getPosture() != Posture.GROUNDED) {
            StringBuilder message = new StringBuilder("Template rule is not guaranteed-streamable");
            for (String r : reasons) {
                message.append("\n  *  ").append(r);
            }
            template.compileError(message.toString(), "XTSE3430");
        }
        // Clear the streamability data because the tree is going to be rewritten
        ExpressionTool.clearStreamabilityData(body);
    }

    /**
     * Ask whether a given node is a streamed node
     *
     * @param node the node in question
     * @return true if the node is a node in a streamed document
     */

    @Override
    public boolean isStreamedNode(NodeInfo node) {
        return node instanceof FleetingNode;
    }

    /**
     * Get the optimization options in use
     *
     * @return the configured optimization options
     */

    @Override
    public OptimizerOptions getOptimizerOptions() {
        return optimizerOptions.intersect(OptimizerOptions.FULL_EE_OPTIMIZATION);
    }

    /**
     * Factory method to get an Optimizer
     */

    /*@NotNull*/
    @Override
    public Optimizer obtainOptimizer() {
        if (!isLicensedFeature(LicenseFeature.SCHEMA_VALIDATION)) {
            return super.obtainOptimizer();
        }
        if (optimizer == null) {
            optimizer = new OptimizerEE(this);
            optimizer.setOptimizerOptions(
                    optimizerOptions.intersect(OptimizerOptions.FULL_EE_OPTIMIZATION));
        }
        return optimizer;
    }

    /**
     * Factory method to get an Optimizer with specified optimizer options.
     * <p>This method is intended for internal use only.</p>
     *
     * @param options the optimizer options
     * @return a new optimizer with the specified options set (provided the optimizations
     * are available in this Saxon configuration)
     */

    /*@NotNull*/
    @Override
    public Optimizer obtainOptimizer(OptimizerOptions options) {
        if (!isLicensedFeature(LicenseFeature.SCHEMA_VALIDATION)) {
            return super.obtainOptimizer(options);
        }
        Optimizer optimizer = new OptimizerEE(this);
        optimizer.setOptimizerOptions(
                options.intersect(OptimizerOptions.FULL_EE_OPTIMIZATION));
        return optimizer;
    }


    /**
     * Ask whether bytecode should be generated. The default setting
     * is true in Saxon Enterprise Edition and false in all other cases. Setting the option to
     * true has no effect if Saxon-EE is not available (but if it is set to true, this method will
     * return true). Setting the option to false in Saxon-EE
     * is permitted if for some reason bytecode generation is to be suppressed (one possible reason
     * is to improve compilation performance at the expense of evaluation performance).
     *
     * @param hostLanguage one of XSLT or XQUERY
     * @return true if the option is switched on
     */

    @Override
    public boolean isGenerateByteCode(HostLanguage hostLanguage) {
        return getBooleanProperty(Feature.GENERATE_BYTE_CODE) &&
                isLicensedFeature(hostLanguage == HostLanguage.XSLT ?
                                          LicenseFeature.ENTERPRISE_XSLT :
                                          LicenseFeature.ENTERPRISE_XQUERY);
    }

    /**
     * Ask whether bytecode should be generated in Just-In-time compilation and therefore deferring the byte code generation. The default setting
     * is false. Setting the option to
     * true has no effect if Saxon-EE is not available (but if it is set to true, this method will
     * return true). Setting the option to false in Saxon-EE
     * is permitted and therefore byte code generation will be generated in the compile phase.
     *
     * @param hostLanguage one of XSLT or XQUERY
     * @return true if the option is switched on
     */
    @Override
    public boolean isDeferredByteCode(HostLanguage hostLanguage) {
        return byteCodeThreshold > 0 &&
                isLicensedFeature(hostLanguage == HostLanguage.XSLT ?
                                          LicenseFeature.ENTERPRISE_XSLT :
                                          LicenseFeature.ENTERPRISE_XQUERY);
    }

    @Override
    public boolean isJITEnabled() {
        return optimizerOptions.isSet(OptimizerOptions.JIT) && isLicensedFeature(LicenseFeature.ENTERPRISE_XSLT);
    }

    /**
     * Factory method to make an XQueryExpression
     */

    @Override
    public XQueryExpression makeXQueryExpression(Expression exp, QueryModule mainModule, boolean streaming) throws XPathException {
        if (!isLicensedFeature(LicenseFeature.ENTERPRISE_XQUERY)) {
            return super.makeXQueryExpression(exp, mainModule, streaming);
        } else {
            XQueryExpressionEE xqe = new XQueryExpressionEE(exp, mainModule, streaming);
            if (mainModule.getCodeInjector() != null) {
                mainModule.getCodeInjector().process(xqe);
            }
            return xqe;
        }
    }

    @Override
    public RegularExpression compileRegularExpression(UnicodeString regex, String flags, String hostLanguage, List<String> warnings) throws XPathException {
        if (optimizerOptions.isSet(OptimizerOptions.REGEX_CACHE)) {
            regexAttempts++;
            String key = hostLanguage + flags + '=' + regex.toString();
            RegularExpression re = regexCache.get(key);
            if (re == null) {
                re = super.compileRegularExpression(regex, flags, hostLanguage, warnings);
                regexCache.put(key, re);
                if (regexCache.size() == 1000 && regexAttempts < 5000) {
                    // poor cache hit rate so abandon caching
                    optimizerOptions = new OptimizerOptions(optimizerOptions.getOptions() & ~OptimizerOptions.REGEX_CACHE);
                    regexCache.clear();
                }
                //System.err.println("Regex cache size " + regexCache.size());
            }
            return re;
        } else {
            return super.compileRegularExpression(regex, flags, hostLanguage, warnings);
        }
    }

    @Override
    public SlotManager makeSlotManager() {
        if (getDebugger() != null) {
            return super.makeSlotManager();
        } else if (isLicensedFeature(LicenseFeature.SCHEMA_VALIDATION)) {
            return new SlotManagerEE();
        } else {
            return new SlotManager();
        }
    }

    /**
     * Get the number of threads to be used for xsl:result-document instructions
     *
     * @return the concurrency of asynchronous xsl:result-document threads, defaulting to the number of
     * (hardware) processors/CPUs available
     */

    public int getResultDocumentThreadsLimit() {
        return resultDocumentThreadsLimit;
    }


    //#if CSHARP==false
    /**
     * Process an xsl:result-document instruction. The Saxon-HE version of this method simply executes the instruction.
     * The Saxon-EE submits the instruction for execution asynchronously in an ExecutorService.
     *
     * @param instruction the instruction to be executed
     * @param content     the expression that defines the content of the new result document
     * @param context     the evaluation context
     * @throws XPathException if any dynamic error occurs
     */

    @Override
    public void processResultDocument(final ResultDocument instruction, final Expression content, final XPathContext context) throws XPathException {
        if (resultDocumentThreadsLimit <= 1 ||
                !getBooleanProperty(Feature.ALLOW_MULTITHREADING) ||
                !instruction.isAsynchronous() ||
                !isLicensedFeature(LicenseFeature.ENTERPRISE_XSLT) ||
                instruction.getHref() == null ||
                context.getContextItem() instanceof FleetingNode ||
                context.getThreadManager() == null) {
            super.processResultDocument(instruction, content, context);
            return;
        }

        // We need to create a snapshot of the current iterator (representing the focus) in the context, so that the context
        // item and position are not affected by things happening in other threads. The way we do this is to create a
        // "ManualIterator", which satisfies the SequenceIterator interface, but is not really an iterator at all, because it cannot
        // be repositioned. We also create a closure that can be used to evaluate last(), should the need arise.

        final ThreadManagerEE manager = (ThreadManagerEE) context.getThreadManager();
        final XPathContextMajor c3 = XPathContextMajor.newThreadContext((XPathContextMinor) context);
        c3.setOrigin(instruction);

        if (context.getCurrentIterator() != null) {
            ManualIterator manualIterator =
                    new ManualIterator(context.getContextItem(), context.getCurrentIterator().position());
            manualIterator.setLengthFinder(CSharp.methodRef(context::getLast));
            c3.setCurrentIterator(manualIterator);
        }

        // We similarly need to create a snapshot of the information used to evaluate current-group() and current-grouping-key()

        final GroupIterator groupItr = context.getCurrentGroupIterator();
        if (groupItr != null) {
            ManualGroupIterator manualGroupIterator = groupItr.getSnapShot(context);
            c3.setCurrentGroupIterator(manualGroupIterator);
        }

        // And the information used for regex-group() - see bug 3083

        final RegexIterator regexItr = context.getCurrentRegexIterator();
        if (regexItr != null) {
            ManualRegexIterator manualRegexIterator = new ManualRegexIterator(regexItr);
            c3.setCurrentRegexIterator(manualRegexIterator);
        }

        // Now create a task to execute the instruction, in the form of a Callable

        Callable<Outcome<Boolean>> task = () -> {
            try {
                //System.err.println("Processing xsl:result-document at " + instruction.getLineNumber());
                instruction.processInstruction(content, c3);
                return new Outcome<Boolean>(true);
            } catch (Exception e) {
                //e.printStackTrace();
                return new Outcome<Boolean>(e);
                //manager.notifyException(e);
            }
        };

        // And submit this callable to the CompletionService. This allows the original client task
        // to wait until all the child tasks have finished execution, and to check that they have succeeded.

        manager.getCompletionService().submit(task);

    }
    //#endif

    /**
     * Get the factory class used to handle creation of objects that support multithreaded
     * execution.
     *
     * @return a MultithreadingFactory, which might be subclassed by advanced applications
     */

    public MultithreadingFactory getMultithreadingFactory() {
        return this.multithreadingFactory;
    }

    /**
     * Set a factory class to handle creation of objects that support multithreaded
     * execution.
     * @param factory a MultithreadingFactory, which can be subclassed by advanced applications
     */

    public void setMultithreadingFactory(MultithreadingFactory factory) {
        this.multithreadingFactory = factory;
    }

    /**
     * Get an item mapping iterator suitable for multi-threaded execution, if this is permitted
     *
     * @param base   iterator over the input sequence
     * @param action mapping function to be applied to each item in the input sequence.
     * @return an iterator over the result sequence
     */

    @Override
    public SequenceIterator getMultithreadedItemMappingIterator(
            SequenceIterator base, ItemMappingFunction action) throws XPathException {
        if (isLicensedFeature(LicenseFeature.SCHEMA_VALIDATION) && getBooleanProperty(Feature.ALLOW_MULTITHREADING)) {
            return multithreadingFactory.makeMultithreadedItemMappingIterator(base, action);
        }
        return super.getMultithreadedItemMappingIterator(base, action);
    }


    /**
     * Make a Closure, given the expected reference count
     */

    @Override
    public Sequence makeClosure(Expression expression, int ref, XPathContext context) throws XPathException {
        if (ref == FilterExpression.FILTERED) {
            return SearchableValue.makeSearchableValue(expression.iterate(context));
        } else {
            return super.makeClosure(expression, ref, context);
        }
    }

    /**
     * Make a SequenceExtent, given the expected reference count
     *
     * @param expression the expression to be evaluated
     * @param ref        the (nominal) number of times the value of the expression is required
     * @param context    the XPath dynamic evaluation context
     * @return the constructed Closure
     */

    @Override
    public GroundedValue makeSequenceExtent(Expression expression, int ref, XPathContext context) throws XPathException {
        if (ref == FilterExpression.FILTERED) {
            return SearchableValue.makeSearchableValue(expression.iterate(context));
        } else {
            return SequenceTool.toGroundedValue(expression.iterate(context));
        }
    }
    
    /**
     * Declare the built-in XSLT extension element namespaces
     */

    @Override
    public void declareBuiltInExtensionElementNamespaces() {
        try {
            setExtensionElementCreator(NamespaceUri.SAXON, SaxonElementFactoryEE.FACTORY);
            setExtensionElementCreator(NamespaceUri.SAXON6, Saxon6ElementFactory.FACTORY);
            setExtensionElementCreator(NamespaceUri.of("http://saxonica.com/ns/interactiveXSLT"), IXSLElementFactory.FACTORY);
        } catch (XPathException e) {
            throw new IllegalStateException(e.getMessage());
        }
    }

    /**
     * Factory method to make a ContextItemStaticInfo
     *
     * @param itemType       the item type of the context item. If the context item is absent, set this to
     *                       {@link net.sf.saxon.type.ErrorType#getInstance()}.
     * @param maybeUndefined set to true if it is possible (or certain) that the context item will be absent.
     * @return the ContextItemStaticInfo
     */

    @Override
    public ContextItemStaticInfo makeContextItemStaticInfo(ItemType itemType, boolean maybeUndefined) {
        if (isLicensedFeature(LicenseFeature.ENTERPRISE_XSLT) || isLicensedFeature(LicenseFeature.ENTERPRISE_XQUERY)) {
            ContextItemStaticInfoEE cit = new ContextItemStaticInfoEE(itemType, maybeUndefined);
            cit.setStrictStreamabilityRules(getBooleanProperty(Feature.STRICT_STREAMABILITY));
            return cit;
        } else {
            return super.makeContextItemStaticInfo(itemType, maybeUndefined);
        }
    }

    /**
     * Get a default ContextItemStaticInfo, indication no information is available about the context item
     * type
     *
     * @return the default ContextItemStaticInfo
     */

    @Override
    public ContextItemStaticInfo getDefaultContextItemStaticInfo() {
        return ContextItemStaticInfoEE.DEFAULT;
    }

    /**
     * Make a new Mode - this can be overridden in subclasses to produce optimized variants
     *
     * @param modeName     the name of the mode
     * @param compilerInfo information on the compiler, that can alter rule optimization
     * @return an instantiated Mode
     */
    @Override
    public SimpleMode makeMode(StructuredQName modeName, CompilerInfo compilerInfo) {
        if (isLicensedFeature(LicenseFeature.ENTERPRISE_XSLT)) {
//            PatternOptimization pOpt = compilerInfo.getPatternOptimization();
//            if (pOpt != null && pOpt.isOptimize()) {
//                return new ModeExperimental(modeName, compilerInfo);
//            }
            return new ModeEE(modeName, compilerInfo);
        } else {
            return new ModePE(modeName);
        }
    }

    /**
     * Factory method to create a Template Rule
     *
     * @return a new TemplateRule appropriate to this Configuration
     */

    @Override
    public TemplateRuleEE makeTemplateRule() {
        return new TemplateRuleEE();
    }

    /**
     * Make a ThreadManager for asynchronous xsl:result-document instructions
     *
     * @return a new ThreadManager (or null in the case of Saxon-HE, or if multithreading
     * is disabled)
     */
    @Override
    public XPathContextMajor.ThreadManager makeThreadManager() {
        if (getBooleanProperty(Feature.ALLOW_MULTITHREADING)) {
            return multithreadingFactory.makeThreadManager();
        } else {
            return null;
        }
    }

    /**
     * Make an XSLT CompilerInfo object - can be overridden in a subclass to produce variants
     * capable of optimization
     *
     * @return a new CompilerInfo object
     */
    @Override
    public CompilerInfo makeCompilerInfo() {
        //PatternOptimizationEE optimization = new PatternOptimizationEE();
//        optimization.setOptimize();
//        optimization.setIndexRules();
//        optimization.setIndexComparisonPredicates(true);
        //info.setPatternOptimization(optimization);
        return super.makeCompilerInfo();
    }

//#if BYTECODE == true

    /**
     * Make a CompilerService object, to handle byte code generation, or null if byte code
     * generation is not available
     *
     * @param hostLanguage for example Configuration.XSLT
     * @return a CompilerService, or null
     */
    @Override
    public CompilerService makeCompilerService(HostLanguage hostLanguage) {
        return new CompilerService(this, hostLanguage);
    }

    /**
     * Obtain a ByteCodeMonitor for monitoring hotspot bytecode generation
     *
     * @return the existing ByteCodeMonitor if there is one, or a new one if not
     */

    public ByteCodeMonitor obtainByteCodeMonitor() {
        if (byteCodeMonitor == null) {
            byteCodeMonitor = new ByteCodeMonitor();
        }
        return byteCodeMonitor;
    }

    /**
     * Generate a report on byte code instrumentation to a specified file
     *
     * @param fileName the specified file name
     */

    @Override
    public void createByteCodeReport(String fileName) {
        try {
            PrintWriter w = new PrintWriter(new File(fileName));
            if (byteCodeMonitor != null) {
                byteCodeMonitor.report(w);
            } else {
                w.println("<no-hot-spot-byte-code-generated/>");
            }
            w.close();
        } catch (FileNotFoundException e) {
            System.err.println("Unable to produce bytecode monitoring report: " + e.getMessage());
        }
    }
//#endif

}

// Copyright (c) 2009-2022 Saxonica Limited

