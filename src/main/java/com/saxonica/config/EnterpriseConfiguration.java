package com.saxonica.config;

import com.saxonica.ee.config.*;
import com.saxonica.ee.extfn.VendorFunctionSetEE;
import com.saxonica.ee.extfn.js.IXSLElementFactory;
import com.saxonica.ee.optim.*;
import com.saxonica.ee.parallel.ThreadManagerEE;
import com.saxonica.ee.s9api.XsdCompilerEE;
import com.saxonica.ee.schema.*;
import com.saxonica.ee.schema.sdoc.SchemaDocument;
import com.saxonica.ee.schema.sdoc.SchemaReader;
import com.saxonica.ee.schema.sdoc.XMLNamespaceSchema;
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
import com.saxonica.ee.validate.AssertionTreeBuilder;
import com.saxonica.ee.validate.InvalidityReportGeneratorEE;
import com.saxonica.ee.validate.SkipValidator;
import com.saxonica.ee.validate.ValidationContext;
import com.saxonica.expr.XQueryParserExtensionPE;
import com.saxonica.trans.ModePE;
import com.saxonica.xsltextn.style.Saxon6ElementFactory;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.event.FilterFactory;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.accum.Accumulator;
import net.sf.saxon.expr.accum.AccumulatorRegistry;
import net.sf.saxon.expr.elab.PushEvaluator;
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
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XsdCompiler;
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
import java.io.OutputStream;
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
    private ConcurrentHashMap<NamespaceUri, PreparedSchema> schemaCache = new ConcurrentHashMap<>();
    private ConcurrentHashMap<String, Schema> namedSchemaCache = new ConcurrentHashMap<>();
    private int[] occurrenceLimits = {100, 250};
    private SchemaURIResolver schemaURIResolver = null;
    private final SchemaURIResolver standardSchemaResolver;
    private int resultDocumentThreadsLimit = Runtime.getRuntime().availableProcessors();
    private int thresholdForCompilingTypes = 100;
    private int thresholdForInliningFunctions = 100;
    private int maxCompiledClasses = 10000;
    private Optional<Boolean> streamabilityEnabled = Optional.empty();
    private final ConcurrentHashMap<String, RegularExpression> regexCache = new ConcurrentHashMap<>();
    private long regexAttempts = 0;
    private MultithreadingFactory multithreadingFactory;


    /**
     * Create an EnterpriseConfiguration
     */

    public EnterpriseConfiguration() {
        super();
        multithreadingFactory = new MultithreadingFactory(this);
        internalSetBooleanProperty(FeatureCode.ALLOW_MULTITHREADING, FeatureKeys.ALLOW_MULTITHREADING, true);
        setStaticQueryContextFactory(new StaticQueryContextFactory() {
            @Override
            public StaticQueryContext newStaticQueryContext(Configuration config, boolean copyFromDefault) {
                return new StaticQueryContextEE(EnterpriseConfiguration.this, copyFromDefault);
            }
        });
        optimizerOptions = OptimizerOptions.FULL_EE_OPTIMIZATION;
        defaultXsltCompilerInfo.setOptimizerOptions(optimizerOptions);
        standardSchemaResolver = new StandardSchemaResolver(this);
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
                // no action, obsolete
                break;
            }
            case FeatureCode.THRESHOLD_FOR_HOTSPOT_BYTE_CODE: {
                // no action, obsolete
                break;
            }
            case FeatureCode.DEBUG_BYTE_CODE_DIR: {
                // no action, obsolete
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
                return ""; // obsolete
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
            try {
                loadLicense();
            } catch (Exception err) {
                int reason = err instanceof LicenseException ? ((LicenseException) err).getReason() : LicenseException.CANNOT_READ;
                String msg = err.getMessage() + ". A license is needed to use " + name;
                throw new LicenseException(msg, reason);
            }
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
     * declarations. This acts as a default for any {@link XsdCompiler} created from
     * this point on.
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
     * This returns the default setting used when creating a new {@link XsdCompiler}; it is not
     * affected by subsequent calls to {@link XsdCompiler#setSchemaURIResolver(SchemaURIResolver)}.
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
     * Simple interface to load a schema document
     *
     * @param absoluteURI the absolute URI of the location of the schema document
     * @throws net.sf.saxon.type.SchemaException if the schema document at the given location cannot be read or is invalid
     */

    public Schema loadSchema(String absoluteURI) throws SchemaException {
        SchemaCompiler compiler = new SchemaCompiler(this);
        List<Source> sources = new ArrayList<>(1);
        sources.add(new StreamSource(absoluteURI));
        return compiler.loadSources(sources);
    }

    /**
     * Create an empty schema. This may be useful as a basis for validating a document using a schema loaded
     * using xsi:schemaLocation and xsi:noNamespaceSchemaLocation attributes.
     *
     * @return an empty schema containing only the built-in components.
     */
    public Schema emptySchema() {
        return new PreparedSchema(this);
    }

//    /**
//     * Read a schema from a given schema location
//     *
//     * @param pipe           the PipelineConfiguration
//     * @param baseURI        the base URI for resolving relative URIs
//     * @param schemaLocation the location "hint" where to find the schema document. Must not be null.
//     *                       The document located may be either an XSD schema document or an SCM compiled schema.
//     * @param expected       the target namespace expected; null if there is no
//     *                       expectation
//     * @return the actual target namespace of the schema
//     */
//
//    @Override
//    public Schema readSchema(PipelineConfiguration pipe, String baseURI, String schemaLocation, NamespaceUri expected)
//            throws SchemaException {
//        int status = getNamespaceStatus(expected);
//        if (status == PreparedSchema.NAMESPACE_UNKNOWN || (status == PreparedSchema.NAMESPACE_KNOWN && getConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS))) {
//            SchemaCompiler schemaCompiler = newSchemaCompiler();
//            if (pipe.getErrorReporter() != null) {
//                schemaCompiler.setErrorReporter(pipe.getErrorReporter());
//            }
//            PreparedSchema schema = schemaCompiler.readSchema(baseURI, schemaLocation, expected);
//            schemaCompiler.setPreparedSchema(schema);
//            schemaCompiler.makeAllAutomata();
//            schemaCompiler.checkAllAutomata();
//            return schema;
//        } else {
//            return null;
//        }
//    }

//    /**
//     * Read an inline schema from a stylesheet
//     *
//     * @param root          the xs:schema element in the stylesheet
//     * @param expected      the target namespace expected; "" if there is no
//     *                      expectation
//     * @param errorReporter The destination for error messages. May be null, in which case
//     *                      the errorListener registered with this Configuration is used.
//     * @return the actual target namespace of the schema
//     */
//
//    @Override
//    public NamespaceUri readInlineSchema(NodeInfo root, NamespaceUri expected, ErrorReporter errorReporter)
//            throws SchemaException {
//        Schema schema = newSchemaCompiler().readInlineSchema(root, expected, errorReporter);
//        return NamespaceUri.NULL; // TODO
//    }

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

    public synchronized void addSchemaSource(Source schemaSource, /*@Nullable*/ ErrorReporter errorReporter, SchemaCompiler compiler) throws SchemaException {
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
            SchemaDocument schemadoc = SchemaReader.read(schemaSource, compiler, null);
//            if (schemadoc.isCompiledSchemaDocument()) {
//                try {
//                    importComponents(schemadoc.getDocument().getRootNode().asActiveSource());
//                } catch (XPathException e) {
//                    throw new SchemaException(e);
//                }
//            } else {
                compiler.validate(schemadoc);
                setNamespaceStatus(schemadoc.getTargetNamespace(), PreparedSchema.NAMESPACE_KNOWN);
//            }

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
     * Add a built-in schema for a given namespace. This is a no-op if the configuration is not schema-aware
     *
     * @param namespace the namespace. Currently built-in schemas are available for the XML and FN namespaces
     */

    public Schema getSchemaForBuiltInNamespace(NamespaceUri namespace) {
        PreparedSchema ps = getCachedSchema(namespace);
        if (ps != null) {
            return ps;
        }
        SchemaCompiler compiler = new SchemaCompiler(this);
        return compiler.getSchemaForBuiltInNamespace(namespace);
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

    public PreparedSchema getCachedSchema(NamespaceUri namespace) {
        return schemaCache.get(namespace);
    }

    public void setCachedSchema(NamespaceUri namespace, PreparedSchema schema) {
        schemaCache.put(namespace, schema);
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
        return Collections.emptySet();  // TODO: reinstate schema paramters
    }

    public Schema getSchemaForNamespace(NamespaceUri uri) {
        return schemaCache.get(uri);
    }

    public void registerSchemaForNamespace(NamespaceUri uri, PreparedSchema schema) {
        schemaCache.put(uri, schema);
    }

    public void registerNamedSchema(String name, Schema schema) {
        namedSchemaCache.put(name, schema);
    }

    public PreparedSchema getBuiltInSchema(NamespaceUri uri) {
        PreparedSchema schema = schemaCache.get(uri);
        if (schema == null) {
            if (uri.equals(NamespaceUri.XML)) {
                schema = XMLNamespaceSchema.buildSchema(this);
                schemaCache.put(uri, schema);
            }
        }
        return schema;
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
     * @param language set to "XP" (XPath) or "XQ" (XQuery) or "PATTERN" (XSLT Patterns)
     * @param updating indicates whether or not XQuery update syntax may be used. Note that XQuery Update
     *                 is supported only in Saxon-EE
     * @param env
     * @return the QueryParser
     * @throws net.sf.saxon.trans.XPathException if this version of Saxon does not support the
     *                                           requested options
     */

    @Override
    public XPathParser newExpressionParser(String language, boolean updating, StaticContext env) throws XPathException {
        if (updating && !isLicensedFeature(LicenseFeature.ENTERPRISE_XQUERY)) {
            throw new IllegalArgumentException("XQuery Update requires a Saxon-EE license");
        }
        if ("XQ".equals(language)) {
            XQueryParser parser = new XQueryParser(env);
            if (isLicensedFeature(LicenseFeature.ENTERPRISE_XQUERY)) {
                SchemaCompiler schemaCompiler = env.getPackageData().isSchemaAware() ? new SchemaCompiler(this) : null;
                parser.setParserExtension(new XQueryParserExtensionEE(updating, schemaCompiler));
            } else if (isLicensedFeature(LicenseFeature.PROFESSIONAL_EDITION)) {
                parser.setParserExtension(new XQueryParserExtensionPE());
            }
            return parser;
        } else {
            XPathParser parser = super.newExpressionParser(language, updating, env);
            parser.setAccelerator(new QuickXPathParser());
            return parser;
        }
    }

    /**
     * Get a new ExpressionPresenter capable of exporting a compiled stylesheet
     *
     * @param target      the target environment, e.g. EE, JS. For export to SaxonJS
     *                    the values JS2 and JS3 are recognised to distinguish the target
     *                    version
     * @param destination the destination for the output file
     * @param rootPackage the compiled stylesheet package
     * @return a suitable Expression Presenter
     * @throws XPathException if none is available (e.g. because this is not Saxon-EE)
     */

    @Override
    public ExpressionPresenter newExpressionExporter(String target, OutputStream destination, StylesheetPackage rootPackage) throws XPathException {
        checkLicensedFeature(LicenseFeature.ENTERPRISE_XSLT, "stylesheeet export", -1);
        ExpressionPresenter presenter = new ExpressionPresenter();
        ExpressionPresenter.ExportOptions options = new ExpressionPresenter.ExportOptions();
        options.rootPackage = rootPackage;
        presenter.setOptions(options);
        switch (target) {
            case "JS":
            case "JS2":
            case "JS3":
                options.target = "JS";
                options.targetVersion = target.equals("JS3") ? 3 : 2;
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
    public PendingUpdateList newPendingUpdateList(Schema schema) {
        return new PendingUpdateListImpl(schema);
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
            return CSharp.constructorRef(com.saxonica.ee.parallel.MultithreadedFocusTrackingIterator::new, 1);
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
     * Get the optimization options permitted in this configuration
     *
     * @return the permitted optimization options
     */

    public OptimizerOptions getPermittedOptimizerOptions() {
        return OptimizerOptions.FULL_EE_OPTIMIZATION;
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
    public void processResultDocument(final ResultDocument instruction, final PushEvaluator content, final XPathContext context) throws XPathException {
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


//    /**
//     * Make a Closure, given the expected reference count
//     */
//
//    @Override
//    public Sequence makeClosure(Expression expression, int ref, XPathContext context) throws XPathException {
//        if (ref == FilterExpression.FILTERED) {
//            return SearchableValue.makeSearchableValue(expression.iterate(context));
//        } else {
//            return super.makeClosure(expression, ref, context);
//        }
//    }

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
            try {
                return SequenceTool.toGroundedValue(expression.iterate(context));
            } catch (UncheckedXPathException e) {
                throw e.getXPathException();
            }
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

    @Override
    public XsdCompiler makeXsdCompiler(Processor proc) {
        return new XsdCompilerEE(proc);
    }


}

// Copyright (c) 2009-2023 Saxonica Limited

