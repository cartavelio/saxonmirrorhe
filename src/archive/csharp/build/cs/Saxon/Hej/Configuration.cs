using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej {
    public class Configuration : Saxon.Hej.lib.SourceResolver, Saxon.Hej.om.NotationSet {

        static Configuration() {
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.ALLOW_EXTERNAL_FUNCTIONS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.ALLOW_MULTITHREADING);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.ALLOW_SYNTAX_EXTENSIONS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.ASSERTIONS_CAN_SEE_COMMENTS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.COMPILE_WITH_TRACING);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.DEBUG_BYTE_CODE);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.DISABLE_XSL_EVALUATE);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.DISPLAY_BYTE_CODE);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.DTD_VALIDATION);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.EAGER_EVALUATION);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.EXPAND_ATTRIBUTE_DEFAULTS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.EXPATH_FILE_DELETE_TEMPORARY_FILES);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.GENERATE_BYTE_CODE);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.IGNORE_SAX_SOURCE_PARSER);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.IMPLICIT_SCHEMA_IMPORTS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.MARK_DEFAULTED_ATTRIBUTES);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.MONITOR_HOT_SPOT_BYTE_CODE);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.MULTIPLE_SCHEMA_IMPORTS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.PRE_EVALUATE_DOC_FUNCTION);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.RECOGNIZE_URI_QUERY_PARAMETERS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.RETAIN_DTD_ATTRIBUTE_TYPES);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.STABLE_COLLECTION_URI);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.STABLE_UNPARSED_TEXT);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.STREAMING_FALLBACK);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.STRICT_STREAMABILITY);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.SUPPRESS_EVALUATION_EXPIRY_WARNING);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.SUPPRESS_XPATH_WARNINGS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.SUPPRESS_XSLT_NAMESPACE_CHECK);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.TRACE_EXTERNAL_FUNCTIONS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.TRACE_OPTIMIZER_DECISIONS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.USE_PI_DISABLE_OUTPUT_ESCAPING);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.USE_TYPED_VALUE_CACHE);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.XQUERY_MULTIPLE_MODULE_IMPORTS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.RETAIN_NODE_FOR_DIAGNOSTICS);
            booleanFeatures.add(Saxon.Hej.lib.FeatureCode.ALLOW_UNRESOLVED_SCHEMA_COMPONENTS);
            stringFeatures.add(Saxon.Hej.lib.FeatureCode.ZIP_URI_PATTERN);
        }
        protected internal static Saxon.Hej.z.IntSet booleanFeatures = new Saxon.Hej.z.IntHashSet(40);
        protected internal static Saxon.Hej.z.IntSet stringFeatures = new Saxon.Hej.z.IntHashSet(40);
        private Saxon.Hej.Configuration.ApiProvider apiProcessor = null;
        private Saxon.Hej.serialize.charcode.CharacterSetFactory characterSetFactory;
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.lib.StringCollator> collationMap = new System.Collections.Generic.Dictionary<string, Saxon.Hej.lib.StringCollator>(10);
        private Saxon.Hej.lib.CollationURIResolver collationResolver = new Saxon.Hej.lib.StandardCollationURIResolver();
        private string defaultCollationName = Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
        private System.Collections.Generic.IDictionary<string,Saxon.Hej.lib.ResourceCollection> registeredCollections = new System.Collections.Generic.Dictionary<string, Saxon.Hej.lib.ResourceCollection>();
        private Saxon.Hej.lib.CollectionFinder collectionFinder;
        private Saxon.Hej.lib.EnvironmentVariableResolver environmentVariableResolver = new Saxon.Hej.lib.StandardEnvironmentVariableResolver();
        private string defaultCollection = null;
        private Saxon.Hej.lib.ParseOptions defaultParseOptions = new Saxon.Hej.lib.ParseOptions();
        protected internal Saxon.Hej.query.StaticQueryContext defaultStaticQueryContext;
        private Saxon.Hej.lib.StaticQueryContextFactory staticQueryContextFactory = new Saxon.Hej.lib.StaticQueryContextFactory();
        protected internal Saxon.Hej.expr.parser.OptimizerOptions optimizerOptions = Saxon.Hej.expr.parser.OptimizerOptions.FULL_HE_OPTIMIZATION;
        protected internal Saxon.Hej.trans.CompilerInfo defaultXsltCompilerInfo;
        private System.Func<Saxon.Hej.Configuration,Saxon.Hej.lib.ErrorReporter> errorReporterFactory;
        protected internal Saxon.Hej.sxpath.IndependentContext staticContextForSystemFunctions;
        private string label = null;
        private Saxon.Hej.tree.util.DocumentNumberAllocator documentNumberAllocator = new Saxon.Hej.tree.util.DocumentNumberAllocator();
        private Saxon.Hej.expr.instruct.Debugger debugger = null;
        private string defaultLanguage = Saxon.Hej.Version.platform.getDefaultLanguage();
        private string defaultCountry = Saxon.Hej.Version.platform.getDefaultCountry();
        private Saxon.Ejava.util.Properties defaultOutputProperties = new Saxon.Ejava.util.Properties();
        private Saxon.Hej.lib.IDynamicLoader dynamicLoader = Saxon.Hej.Version.platform.getDefaultDynamicLoader();
        private readonly Saxon.Hej.z.IntSet enabledProperties = new Saxon.Hej.z.IntHashSet(64);
        private readonly Saxon.Hej.z.IntHashMap<string> stringProperties = new Saxon.Hej.z.IntHashMap<string>();
        private readonly Saxon.Hej.om.DocumentPool globalDocumentPool = new Saxon.Hej.om.DocumentPool();
        private readonly Saxon.Hej.functions.IntegratedFunctionLibrary integratedFunctionLibrary = new Saxon.Hej.functions.IntegratedFunctionLibrary();
        private Saxon.Hej.lib.LocalizerFactory localizerFactory;
        private Saxon.Hej.om.NamePool namePool = new Saxon.Hej.om.NamePool();
        protected internal Saxon.Hej.expr.parser.Optimizer optimizer = null;
        private Saxon.Hej.lib.SerializerFactory serializerFactory;
        private string sourceParserClass;
        private Saxon.Hej.lib.SourceResolver sourceResolver;
        private Saxon.Hej.lib.Logger traceOutput = new Saxon.Callbacks.StandardLogger();
        private Saxon.Hej.lib.ModuleURIResolver standardModuleURIResolver;
        private string styleParserClass;
        private Saxon.Hej.lib.UnparsedTextURIResolver unparsedTextURIResolver;
        private Saxon.Hej.expr.XPathContext theConversionContext = null;
        private Saxon.Hej.lib.ConversionRules theConversionRules = null;
        private Saxon.Hej.lib.TraceListener traceListener = null;
        private string traceListenerClass = null;
        private string traceListenerOutput = null;
        private string defaultRegexEngine = "S";
        protected internal Saxon.Hej.type.TypeHierarchy typeHierarchy;
        private readonly Saxon.Hej.expr.parser.TypeChecker typeChecker = new Saxon.Hej.expr.parser.TypeChecker();
        private readonly Saxon.Hej.expr.compat.TypeChecker10 typeChecker10 = new Saxon.Hej.expr.compat.TypeChecker10();
        private Saxon.Hej.lib.JJ_ResourceResolver commonResolver;
        private Saxon.Hej.lib.ProtocolRestrictor protocolRestrictor = new Saxon.Hej.lib.ProtocolRestrictor("all");
        protected internal Saxon.Hej.z.IntHashMap<Saxon.Hej.functions.FunctionLibraryList> builtInExtensionLibraryList = new Saxon.Hej.z.IntHashMap<Saxon.Hej.functions.FunctionLibraryList>(4);
        protected internal int xsdVersion = XSD11;
        private int xmlVersion = XML10;
        private int xpathVersionForXsd = 20;
        private int xpathVersionForXslt = 31;
        private System.Collections.Generic.Comparer<string> mediaQueryEvaluator;
        private readonly System.Collections.Generic.IDictionary<string,string> fileExtensions = new System.Collections.Generic.Dictionary<string, string>();
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.lib.ResourceFactory> resourceFactoryMapping = new System.Collections.Generic.Dictionary<string, Saxon.Hej.lib.ResourceFactory>();
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.NamespaceUri,Saxon.Hej.lib.FunctionAnnotationHandler> functionAnnotationHandlers = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NamespaceUri, Saxon.Hej.lib.FunctionAnnotationHandler>();
        private int regexBacktrackingLimit = 10000000;
        private readonly Saxon.Hej.tree.tiny.TreeStatistics treeStatistics = new Saxon.Hej.tree.tiny.TreeStatistics();
        public const int XML10 = 10;
        public const int XML11 = 11;
        public const int XSD10 = 10;
        public const int XSD11 = 11;
        public Configuration()  {
            init();
        }
        public static Saxon.Hej.Configuration newConfiguration() {
            System.Type configurationClass = typeof(Saxon.Hej.Configuration);
            configurationClass = typeof(Saxon.Eej.config.EnterpriseConfiguration);
            try {
                return (Saxon.Hej.Configuration)System.Activator.CreateInstance(configurationClass);
            } catch (System.Exception e) {
                System.Console.Error.WriteLine(e);
                throw new System.Exception("Cannot instantiate a Configuration", e);
            }
        }
        public static Saxon.Hej.Configuration newLicensedConfiguration() {
            Saxon.Eej.config.EnterpriseConfiguration eeConfig = new Saxon.Eej.config.EnterpriseConfiguration();
            if (eeConfig.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION)) {
                return eeConfig;
            } else {
                if (eeConfig.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                    Saxon.Eej.config.ProfessionalConfiguration peConfig = new Saxon.Eej.config.ProfessionalConfiguration();
                    peConfig.importLicenseDetails(eeConfig);
                    return peConfig;
                }
            }
            return new Saxon.Hej.Configuration();
        }
        public static Saxon.Hej.Configuration readConfiguration(Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Hej.Configuration tempConfig = newConfiguration();
            return tempConfig.readConfigurationFile(source);
        }
        public static Saxon.Hej.Configuration readConfiguration(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.Configuration baseConfiguration) {
            Saxon.Hej.Configuration tempConfig = newConfiguration();
            return tempConfig.readConfigurationFile(source, baseConfiguration);
        }
        public static Saxon.Hej.Configuration instantiateConfiguration(string className, Saxon.Ejava.lang.ClassLoader classLoader) {
            return new Saxon.Eej.config.EnterpriseConfiguration();
        }
        public static bool isAssertionsEnabled() {
            bool assertsEnabled = false;
            System.Diagnostics.Debug.Assert(assertsEnabled = true);
            return assertsEnabled;
        }
        protected internal Saxon.Hej.Configuration readConfigurationFile(Saxon.Ejavax.xml.transform.Source source) {
            return makeConfigurationReader().makeConfiguration(source);
        }
        protected internal Saxon.Hej.Configuration readConfigurationFile(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.Configuration baseConfiguration) {
            Saxon.Hej.trans.ConfigurationReader reader = makeConfigurationReader();
            reader.setBaseConfiguration(baseConfiguration);
            return reader.makeConfiguration(source);
        }
        protected internal virtual Saxon.Hej.trans.ConfigurationReader makeConfigurationReader() {
            return new Saxon.Hej.trans.ConfigurationReader();
        }
        protected internal void init() {
            commonResolver = new Saxon.Callbacks.CatalogResourceResolver();
            defaultXsltCompilerInfo = makeCompilerInfo();
            standardModuleURIResolver = Saxon.Hej.Version.platform.makeStandardModuleURIResolver(this);
            serializerFactory = new Saxon.Hej.lib.SerializerFactory(this);
            sourceResolver = this;
            unparsedTextURIResolver = new Saxon.Hej.lib.StandardUnparsedTextResolver();
            Saxon.Hej.Version.platform.initialize(this);
            if ((standardModuleURIResolver is Saxon.Hej.lib.StandardModuleURIResolver)) {
                ((Saxon.Hej.lib.StandardModuleURIResolver)(standardModuleURIResolver)).setConfiguration(this);
            }
            internalSetBooleanProperty(Saxon.Hej.lib.FeatureCode.ALLOW_EXTERNAL_FUNCTIONS, Saxon.Hej.lib.FeatureKeys.ALLOW_EXTERNAL_FUNCTIONS, true);
            internalSetBooleanProperty(Saxon.Hej.lib.FeatureCode.DISABLE_XSL_EVALUATE, Saxon.Hej.lib.FeatureKeys.DISABLE_XSL_EVALUATE, false);
            string initializationClass = Saxon.Ejava.lang.System.getProperty("SAXON_INITIALIZER");
            if ((initializationClass != null)) {
                try {
                    Saxon.Hej.lib.Initializer initializer = (Saxon.Hej.lib.Initializer)(getInstance(initializationClass));
                    initializer.initialize(this);
                } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                    System.Console.Error.WriteLine(((("Warning: Failed to invoke Saxon Initializer "+initializationClass)+": ")+e.Message));
                }
            }
            registerFileExtension("xml", "application/xml");
            registerFileExtension("html", "application/html");
            registerFileExtension("atom", "application/atom");
            registerFileExtension("xsl", "application/xml+xslt");
            registerFileExtension("xslt", "application/xml+xslt");
            registerFileExtension("xsd", "application/xml+xsd");
            registerFileExtension("txt", "text/plain");
            registerFileExtension("MF", "text/plain");
            registerFileExtension("class", "application/java");
            registerFileExtension("json", "application/json");
            registerFileExtension("", "application/unknown");
            registerMediaType("application/xml", Saxon.Hej.resource.XmlResource.FACTORY);
            registerMediaType("text/xml", Saxon.Hej.resource.XmlResource.FACTORY);
            registerMediaType("application/html", Saxon.Hej.resource.XmlResource.FACTORY);
            registerMediaType("text/html", Saxon.Hej.resource.XmlResource.FACTORY);
            registerMediaType("application/atom", Saxon.Hej.resource.XmlResource.FACTORY);
            registerMediaType("application/xml+xslt", Saxon.Hej.resource.XmlResource.FACTORY);
            registerMediaType("application/xml+xsd", Saxon.Hej.resource.XmlResource.FACTORY);
            registerMediaType("application/rdf+xml", Saxon.Hej.resource.XmlResource.FACTORY);
            registerMediaType("text/plain", Saxon.Hej.resource.UnparsedTextResource.FACTORY);
            registerMediaType("application/java", Saxon.Hej.resource.BinaryResource.FACTORY);
            registerMediaType("application/binary", Saxon.Hej.resource.BinaryResource.FACTORY);
            registerMediaType("application/json", Saxon.Hej.resource.JSONResource.FACTORY);
            registerMediaType("application/unknown", Saxon.Hej.resource.UnknownResource.FACTORY);
            registerFunctionAnnotationHandler(new Saxon.Hej.lib.XQueryFunctionAnnotationHandler());
        }
        public static Saxon.Hej.Configuration makeLicensedConfiguration(Saxon.Ejava.lang.ClassLoader classLoader, string className) {
            return new Saxon.Eej.config.EnterpriseConfiguration();
        }
        public virtual void importLicenseDetails(Saxon.Hej.Configuration config) {
        }
        public virtual string getEditionCode() {
            return "HE";
        }
        public interface ApiProvider {
        
        }
        public void setProcessor(Saxon.Hej.Configuration.ApiProvider processor) {
            this.apiProcessor = processor;
        }
        public Saxon.Hej.Configuration.ApiProvider getProcessor() {
            return apiProcessor;
        }
        public string getProductTitle() {
            return (((((("Saxon"+Saxon.Hej.Version.platform.getPlatformSuffix())+"-")+getEditionCode())+" ")+Saxon.Hej.Version.getProductVersion())+" from Saxonica");
        }
        public virtual void checkLicensedFeature(int feature, string name, int localLicenseId) {
            string require = ((feature == Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION) ? "PE" : "EE");
            string message = ((("Requested feature ("+name)+") requires Saxon-")+require);
            if (!(Saxon.Hej.Version.softwareEdition.Equals("HE"))) {
                string packageNs = (Saxon.Hej.Version.platform.isDotNet() ? "Saxon.Eej.config" : "com.saxonica.config");
                message += (((((". You are using Saxon-"+Saxon.Hej.Version.softwareEdition)+" software, but the Configuration is an instance of ")+this.GetType())+"; to use this feature you need to create an instance of ")+(((feature == Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION) ? (packageNs+".ProfessionalConfiguration") : (packageNs+".EnterpriseConfiguration"))));
            }
            throw new Saxon.Hej.trans.LicenseException(message, Saxon.Hej.trans.LicenseException.WRONG_CONFIGURATION);
        }
        public virtual void disableLicensing() {
        }
        public virtual bool isFeatureAllowedBySecondaryLicense(int localLicenseId, int feature) {
            return false;
        }
        public virtual bool isLicensedFeature(int feature) {
            return false;
        }
        public void requireProfessionalLicense(string featureName) {
            if (!(isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION))) {
                throw new Saxon.Hej.trans.LicenseException((("Use of "+featureName)+" requires a license key for Saxon-PE or Saxon-EE"), Saxon.Hej.trans.LicenseException.NOT_FOUND);
            }
        }
        public virtual string getLicenseFeature(string name) {
            return null;
        }
        public virtual void displayLicenseMessage() {
        }
        public virtual int registerLocalLicense(string dmk) {
            return -1;
        }
        public void setDynamicLoader(Saxon.Hej.lib.IDynamicLoader dynamicLoader) {
            this.dynamicLoader = dynamicLoader;
        }
        public Saxon.Hej.lib.IDynamicLoader getDynamicLoader() {
            return dynamicLoader;
        }
        public System.Type getClass(string className, bool tracing) {
            return dynamicLoader.getClass(className, (tracing ? traceOutput : null), null);
        }
        public System.Object getInstance(string className) {
            return dynamicLoader.getInstance(className, (isTiming() ? traceOutput : null), null);
        }
        public Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver() {
            if ((commonResolver == null)) {
                setResourceResolver(new Saxon.Callbacks.CatalogResourceResolver());
            }
            return commonResolver;
        }
        public void setResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            commonResolver = resolver;
        }
        public void setParameterizedURIResolver() {
        }
        public Saxon.Hej.lib.ProtocolRestrictor getProtocolRestrictor() {
            return protocolRestrictor;
        }
        public Saxon.Hej.lib.JJ_ResourceResolver makeResourceResolver(string className) {
            System.Object obj = dynamicLoader.getInstance(className, null);
            if ((obj is Saxon.Hej.lib.JJ_ResourceResolver)) {
                return (Saxon.Hej.lib.JJ_ResourceResolver)(obj);
            }
            throw new Saxon.Hej.trans.XPathException((("Class "+className)+" is not a ResourceResolver"));
        }
        public void setErrorReporterFactory(System.Func<Saxon.Hej.Configuration,Saxon.Hej.lib.ErrorReporter> factory) {
            errorReporterFactory = factory;
        }
        public Saxon.Hej.lib.ErrorReporter makeErrorReporter() {
            if ((errorReporterFactory == null)) {
                errorReporterFactory = config => {
                    Saxon.Hej.lib.StandardErrorReporter reporter = new Saxon.Hej.lib.StandardErrorReporter();
                    reporter.setLogger(config.getLogger());
                    return reporter;
                } ;
            }
            return errorReporterFactory(this);
        }
        public Saxon.Hej.lib.Logger getLogger() {
            return traceOutput;
        }
        public void reportFatalError(Saxon.Hej.trans.XPathException err) {
            if (!(err.hasBeenReported())) {
                makeErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(err));
                err.setHasBeenReported(true);
            }
        }
        public void setStandardErrorOutput(Saxon.Ejava.io.PrintStream @out) {
            if ((traceOutput is Saxon.Callbacks.StandardLogger)) {
                ((Saxon.Callbacks.StandardLogger)(traceOutput)).setPrintStream(@out);
            }
        }
        public void setLogger(Saxon.Hej.lib.Logger logger) {
            traceOutput = logger;
        }
        public void setXMLVersion(int version) {
            xmlVersion = version;
            theConversionRules = null;
        }
        public int getXMLVersion() {
            return xmlVersion;
        }
        public Saxon.Hej.lib.ParseOptions getParseOptions() {
            return defaultParseOptions;
        }
        public void setParseOptions(Saxon.Hej.lib.ParseOptions options) {
            defaultParseOptions = options;
        }
        public void setMediaQueryEvaluator(System.Collections.Generic.Comparer<string> comparator) {
            this.mediaQueryEvaluator = comparator;
        }
        public System.Collections.Generic.Comparer<string> getMediaQueryEvaluator() {
            return mediaQueryEvaluator;
        }
        public void setConversionRules(Saxon.Hej.lib.ConversionRules rules) {
            this.theConversionRules = rules;
        }
        public Saxon.Hej.lib.ConversionRules getConversionRules() {
            if ((theConversionRules == null)) {
                lock (this) {
                    Saxon.Hej.lib.ConversionRules cv = new Saxon.Hej.lib.ConversionRules();
                    cv.setTypeHierarchy(getTypeHierarchy());
                    cv.setNotationSet(this);
                    if ((xsdVersion == XSD10)) {
                        cv.setStringToDoubleConverter(Saxon.Hej.type.StringToDouble.getInstance());
                        cv.setURIChecker(Saxon.Hej.lib.StandardURIChecker.getInstance());
                    } else {
                        cv.setStringToDoubleConverter(Saxon.Hej.value.StringToDouble11.getInstance());
                    }
                    cv.setAllowYearZero((xsdVersion != XSD10));
                    return theConversionRules = cv;
                }
            } else {
                return theConversionRules;
            }
        }
        public int getXsdVersion() {
            return xsdVersion;
        }
        public Saxon.Hej.expr.XPathContext getConversionContext() {
            if ((theConversionContext == null)) {
                theConversionContext = new Saxon.Hej.expr.EarlyEvaluationContext(this);
            }
            return theConversionContext;
        }
        public Saxon.Hej.z.IntPredicateProxy getValidCharacterChecker() {
            if ((xmlVersion == XML10)) {
                return Saxon.Hej.z.IntPredicateLambda.of(Saxon.Hej.serialize.charcode.XMLCharacterData.isValid10 /* static method reference */);
            } else {
                return Saxon.Hej.z.IntPredicateLambda.of(Saxon.Hej.serialize.charcode.XMLCharacterData.isValid11 /* static method reference */);
            }
        }
        public int getTreeModel() {
            return defaultParseOptions.getModel().getSymbolicValue();
        }
        public void setTreeModel(int treeModel) {
            defaultParseOptions = defaultParseOptions.withModel(Saxon.Hej.om.TreeModel.getTreeModel(treeModel));
        }
        public bool isLineNumbering() {
            return defaultParseOptions.isLineNumbering();
        }
        public void setLineNumbering(bool lineNumbering) {
            defaultParseOptions = defaultParseOptions.withLineNumbering(lineNumbering);
        }
        public void setXIncludeAware(bool state) {
            defaultParseOptions = defaultParseOptions.withXIncludeAware(state);
        }
        public bool isXIncludeAware() {
            return defaultParseOptions.isXIncludeAware();
        }
        public Saxon.Hej.lib.TraceListener getTraceListener() {
            return traceListener;
        }
        public Saxon.Hej.lib.TraceListener makeTraceListener() {
            if ((traceListener != null)) {
                return traceListener;
            } else {
                if ((traceListenerClass != null)) {
                    try {
                        return makeTraceListener(traceListenerClass);
                    } catch (System.InvalidCastException e) {
                        throw new Saxon.Hej.trans.XPathException(e);
                    }
                } else {
                    return null;
                }
            }
        }
        public void setTraceListener(Saxon.Hej.lib.TraceListener traceListener) {
            this.traceListener = traceListener;
            setCompileWithTracing((traceListener != null));
            internalSetBooleanProperty(Saxon.Hej.lib.FeatureCode.ALLOW_MULTITHREADING, Saxon.Hej.lib.FeatureKeys.ALLOW_MULTITHREADING, false);
        }
        public void setTraceListenerClass(string className) {
            if ((className == null)) {
                traceListenerClass = null;
                setCompileWithTracing(false);
            } else {
                try {
                    makeTraceListener(className);
                } catch (Saxon.Hej.trans.XPathException err) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(((className+": ")+err.Message));
                }
                this.traceListenerClass = className;
                setCompileWithTracing(true);
            }
        }
        public string getTraceListenerClass() {
            return traceListenerClass;
        }
        public void setTraceListenerOutputFile(string filename) {
            traceListenerOutput = filename;
        }
        public string getTraceListenerOutputFile() {
            return traceListenerOutput;
        }
        public bool isCompileWithTracing() {
            return getBooleanProperty(Saxon.Hej.lib.Feature<bool>.COMPILE_WITH_TRACING);
        }
        public void setCompileWithTracing(bool trace) {
            internalSetBooleanProperty(Saxon.Hej.lib.FeatureCode.COMPILE_WITH_TRACING, Saxon.Hej.lib.FeatureKeys.COMPILE_WITH_TRACING, trace);
            if ((defaultXsltCompilerInfo != null)) {
                if (trace) {
                    defaultXsltCompilerInfo.setCodeInjector(new Saxon.Hej.trace.XSLTTraceCodeInjector());
                } else {
                    defaultXsltCompilerInfo.setCodeInjector(null);
                }
            }
            getDefaultStaticQueryContext().setCodeInjector((trace ? new Saxon.Hej.trace.XQueryTraceCodeInjector() : null));
        }
        public Saxon.Hej.lib.TraceListener makeTraceListener(string className) {
            System.Object obj = dynamicLoader.getInstance(className, null);
            if ((obj is Saxon.Hej.lib.TraceListener)) {
                string destination = getTraceListenerOutputFile();
                if ((destination != null)) {
                    try {
                        ((Saxon.Hej.lib.TraceListener)(obj)).setOutputDestination(new Saxon.Callbacks.StandardLogger(new Saxon.Ejava.io.PrintStream(destination)));
                    } catch (Saxon.Ejava.io.FileNotFoundException e) {
                        throw new Saxon.Hej.trans.XPathException(e);
                    }
                }
                return (Saxon.Hej.lib.TraceListener)(obj);
            }
            throw new Saxon.Hej.trans.XPathException((("Class "+className)+" is not a TraceListener"));
        }
        public virtual Saxon.Hej.functions.registry.BuiltInFunctionSet getXSLTFunctionSet(int version) {
            if ((version == 30)) {
                return Saxon.Hej.functions.registry.XSLT30FunctionSet.getInstance();
            } else {
                if ((version == 40)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("XSLT 4.0 requires Saxon-PE or higher");
                } else {
                    throw new Saxon.Ejava.lang.IllegalArgumentException((("Unsupported XSLT version "+version)+" (must be 30)"));
                }
            }
        }
        public virtual Saxon.Hej.functions.registry.BuiltInFunctionSet getXPathFunctionSet(int version) {
            switch (version) {
              case 20:
                return Saxon.Hej.functions.registry.XPath20FunctionSet.getInstance();
              case 30:
              case 305:
                return Saxon.Hej.functions.registry.XPath30FunctionSet.getInstance();
              case 31:
                return Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance();
              case 40:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Version 4.0 requires Saxon-PE or higher");
              default:
                return Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance();
            }
        }
        public virtual Saxon.Hej.functions.registry.BuiltInFunctionSet getXQueryUpdateFunctionSet() {
            return null;
        }
        public virtual Saxon.Hej.functions.SystemFunction makeSystemFunction(string localName, int arity) {
            try {
                return getXSLTFunctionSet(30).makeFunction(localName, arity);
            } catch (Saxon.Hej.trans.XPathException e) {
                return null;
            }
        }
        public void registerExtensionFunction(Saxon.Hej.lib.ExtensionFunctionDefinition function) {
            integratedFunctionLibrary.registerFunction(function);
        }
        public Saxon.Hej.functions.IntegratedFunctionLibrary getIntegratedFunctionLibrary() {
            return integratedFunctionLibrary;
        }
        public Saxon.Hej.functions.FunctionLibraryList getBuiltInExtensionLibraryList(int version) { lock(this) {
            Saxon.Hej.functions.FunctionLibraryList result = builtInExtensionLibraryList.get(version);
            if ((result == null)) {
                result = makeBuiltInExtensionLibraryList(version);
                builtInExtensionLibraryList.put(version, result);
            }
            return result;
        }}
        protected internal virtual Saxon.Hej.functions.FunctionLibraryList makeBuiltInExtensionLibraryList(int version) {
            Saxon.Hej.functions.FunctionLibraryList result = new Saxon.Hej.functions.FunctionLibraryList();
            result.addFunctionLibrary(Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance());
            result.addFunctionLibrary(Saxon.Hej.functions.MathFunctionSet.getInstance());
            result.addFunctionLibrary(Saxon.Hej.ma.map.MapFunctionSet.getInstance(version));
            result.addFunctionLibrary(Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance(version));
            result.addFunctionLibrary(Saxon.Hej.functions.registry.ExsltCommonFunctionSet.getInstance());
            return result;
        }
        public virtual Saxon.Hej.functions.registry.BuiltInFunctionSet getVendorFunctionSet() {
            return Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance();
        }
        public virtual Saxon.Hej.functions.SystemFunction bindSaxonExtensionFunction(string localName, int arity) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException((((("The extension function saxon:"+localName)+"#")+arity)+" requires Saxon-PE or higher"));
        }
        public virtual void addExtensionBinders(Saxon.Hej.functions.FunctionLibraryList list) {
        }
        public virtual Saxon.Hej.om.FunctionItem getSystemFunction(Saxon.Hej.om.StructuredQName name, int arity) {
            try {
                if ((staticContextForSystemFunctions == null)) {
                    staticContextForSystemFunctions = new Saxon.Hej.sxpath.IndependentContext(this);
                }
                Saxon.Hej.functions.FunctionLibrary lib;
                Saxon.Hej.om.NamespaceUri ns = name.getNamespaceUri();
                int version = getDefaultStaticQueryContext().getLanguageVersion();
                if (ns.Equals(Saxon.Hej.om.NamespaceUri.FN)) {
                    lib = getXPathFunctionSet(version);
                } else {
                    if (ns.Equals(Saxon.Hej.om.NamespaceUri.SCHEMA)) {
                        lib = new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(this);
                    } else {
                        if (ns.Equals(Saxon.Hej.om.NamespaceUri.MATH)) {
                            lib = Saxon.Hej.functions.MathFunctionSet.getInstance();
                        } else {
                            if (ns.Equals(Saxon.Hej.om.NamespaceUri.MAP_FUNCTIONS)) {
                                lib = Saxon.Hej.ma.map.MapFunctionSet.getInstance(version);
                            } else {
                                if (ns.Equals(Saxon.Hej.om.NamespaceUri.ARRAY_FUNCTIONS)) {
                                    lib = Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance(version);
                                } else {
                                    Saxon.Hej.functions.FunctionLibraryList fll = new Saxon.Hej.functions.FunctionLibraryList();
                                    fll.addFunctionLibrary(getBuiltInExtensionLibraryList(31));
                                    fll.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(this));
                                    fll.addFunctionLibrary(getIntegratedFunctionLibrary());
                                    lib = fll;
                                }
                            }
                        }
                    }
                }
                Saxon.Hej.trans.SymbolicName.F symbolicName = new Saxon.Hej.trans.SymbolicName.F(name, arity);
                return lib.getFunctionItem(symbolicName, staticContextForSystemFunctions);
            } catch (Saxon.Hej.trans.XPathException e) {
                return null;
            }
        }
        public virtual Saxon.Hej.expr.instruct.UserFunction newUserFunction(bool memoFunction, Saxon.Hej.trans.FunctionStreamability streamability) {
            if (memoFunction) {
                return new Saxon.Hej.expr.instruct.MemoFunction();
            } else {
                return new Saxon.Hej.expr.instruct.UserFunction();
            }
        }
        public void registerCollation(string collationURI, Saxon.Hej.lib.StringCollator collator) {
            Saxon.Impl.Helpers.MapUtils.Put(collationMap, collationURI, collator);
        }
        public void setCollationURIResolver(Saxon.Hej.lib.CollationURIResolver resolver) {
            collationResolver = resolver;
        }
        public Saxon.Hej.lib.CollationURIResolver getCollationURIResolver() {
            return collationResolver;
        }
        public Saxon.Hej.lib.StringCollator getCollation(string collationName) {
            if (((collationName == null) || collationName.Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI))) {
                return Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            }
            if (collationName.Equals(Saxon.Hej.lib.NamespaceConstant.HTML5_CASE_BLIND_COLLATION_URI)) {
                return Saxon.Hej.expr.sort.HTML5CaseBlindCollator.getInstance();
            }
            if (collationName.StartsWith(Saxon.Hej.expr.sort.AlphanumericCollator.PREFIX)) {
                return new Saxon.Hej.expr.sort.AlphanumericCollator(getCollation(collationName.Substring(Saxon.Hej.expr.sort.AlphanumericCollator.PREFIX.Length)));
            }
            Saxon.Hej.lib.StringCollator collator = Saxon.Impl.Helpers.MapUtils.Get(collationMap, collationName);
            if ((collator == null)) {
                collator = getCollationURIResolver().resolve(collationName, this);
            }
            return collator;
        }
        public Saxon.Hej.lib.StringCollator getCollation(string collationURI, string baseURI) {
            if (collationURI.Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI)) {
                return Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            }
            try {
                string absoluteURI = Saxon.Hej.functions.ResolveURI.makeAbsolute(collationURI, baseURI).ToString();
                return getCollation(absoluteURI);
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException((((("Collation name is not a valid URI: "+collationURI)+" (base = ")+baseURI)+")"), "FOCH0002");
            }
        }
        public Saxon.Hej.lib.StringCollator getCollation(string collationURI, string baseURI, string errorCode) {
            if (collationURI.Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI)) {
                return Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            }
            try {
                string absoluteURI = collationURI;
                if ((baseURI != null)) {
                    absoluteURI = Saxon.Hej.functions.ResolveURI.makeAbsolute(collationURI, baseURI).ToString();
                }
                Saxon.Hej.lib.StringCollator collator = getCollation(absoluteURI);
                if ((collator == null)) {
                    throw new Saxon.Hej.trans.XPathException(("Unknown collation "+absoluteURI), errorCode);
                }
                return collator;
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException((((("Collation name is not a valid URI: "+collationURI)+" (base = ")+baseURI)+")"), errorCode);
            }
        }
        public string getDefaultCollationName() {
            return defaultCollationName;
        }
        public void setDefaultCollection(string uri) {
            defaultCollection = uri;
        }
        public string getDefaultCollection() {
            return defaultCollection;
        }
        public void setCollectionFinder(Saxon.Hej.lib.CollectionFinder cf) {
            collectionFinder = cf;
        }
        public Saxon.Hej.lib.CollectionFinder getCollectionFinder() {
            return collectionFinder;
        }
        public void registerCollection(string collectionURI, Saxon.Hej.lib.ResourceCollection collection) {
            Saxon.Impl.Helpers.MapUtils.Put(registeredCollections, collectionURI, collection);
        }
        public Saxon.Hej.lib.ResourceCollection getRegisteredCollection(string uri) {
            return Saxon.Impl.Helpers.MapUtils.Get(registeredCollections, uri);
        }
        public void registerFileExtension(string extension, string mediaType) {
            Saxon.Impl.Helpers.MapUtils.Put(fileExtensions, extension, mediaType);
        }
        public void registerMediaType(string contentType, Saxon.Hej.lib.ResourceFactory factory) {
            Saxon.Impl.Helpers.MapUtils.Put(resourceFactoryMapping, contentType, factory);
        }
        public string getMediaTypeForFileExtension(string extension) {
            string mediaType = Saxon.Impl.Helpers.MapUtils.Get(fileExtensions, extension);
            if ((mediaType == null)) {
                mediaType = Saxon.Impl.Helpers.MapUtils.Get(fileExtensions, "");
            }
            return mediaType;
        }
        public Saxon.Hej.lib.ResourceFactory getResourceFactoryForMediaType(string mediaType) {
            return Saxon.Impl.Helpers.MapUtils.Get(resourceFactoryMapping, mediaType);
        }
        public void setLocalizerFactory(Saxon.Hej.lib.LocalizerFactory factory) {
            this.localizerFactory = factory;
        }
        public Saxon.Hej.lib.LocalizerFactory getLocalizerFactory() {
            return localizerFactory;
        }
        public void setDefaultLanguage(string language) {
            Saxon.Hej.type.ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(Saxon.Hej.str.StringView.of(language).tidy());
            if ((vf != null)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("The default language must be a valid language code");
            }
            defaultLanguage = language;
        }
        public string getDefaultLanguage() {
            return defaultLanguage;
        }
        public void setDefaultCountry(string country) {
            defaultCountry = country;
        }
        public string getDefaultCountry() {
            return defaultCountry;
        }
        public void setDefaultRegexEngine(string engine) {
            if (!(((("J".Equals(engine) || "N".Equals(engine)) || "S".Equals(engine))))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Regex engine must be S|J|N");
            }
            defaultRegexEngine = engine;
        }
        public string getDefaultRegexEngine() {
            return defaultRegexEngine;
        }
        public virtual Saxon.Hej.regex.RegularExpression compileRegularExpression(Saxon.Hej.str.UnicodeString regex, string flags, string hostLanguage, System.Collections.Generic.IList<string> warnings) {
            return Saxon.Hej.Version.platform.compileRegularExpression(this, regex, flags, hostLanguage, warnings);
        }
        public Saxon.Hej.lib.Numberer makeNumberer(string language, string country) {
            if ((localizerFactory == null)) {
                Saxon.Hej.expr.number.Numberer_en numberer = new Saxon.Hej.expr.number.Numberer_en();
                if ((language != null)) {
                    numberer.setLanguage(language);
                }
                if ((country != null)) {
                    numberer.setCountry(country);
                }
                return numberer;
            } else {
                Saxon.Hej.lib.Numberer numberer = localizerFactory.getNumberer(language, country);
                if ((numberer == null)) {
                    numberer = new Saxon.Hej.expr.number.Numberer_en();
                }
                return numberer;
            }
        }
        public void setModuleURIResolver(Saxon.Hej.lib.ModuleURIResolver resolver) {
            getDefaultStaticQueryContext().setModuleURIResolver(resolver);
        }
        public void setModuleURIResolver(string className) {
            System.Object obj = dynamicLoader.getInstance(className, null);
            if ((obj is Saxon.Hej.lib.ModuleURIResolver)) {
                if ((obj is Saxon.Hej.lib.StandardModuleURIResolver)) {
                    ((Saxon.Hej.lib.StandardModuleURIResolver)(obj)).setConfiguration(this);
                }
                setModuleURIResolver((Saxon.Hej.lib.ModuleURIResolver)(obj));
            } else {
                throw new Saxon.Hej.trans.XPathException((("Class "+className)+" is not a ModuleURIResolver"));
            }
        }
        public Saxon.Hej.lib.ModuleURIResolver getModuleURIResolver() {
            return getDefaultStaticQueryContext().getModuleURIResolver();
        }
        public Saxon.Hej.lib.ModuleURIResolver getStandardModuleURIResolver() {
            return standardModuleURIResolver;
        }
        public Saxon.Hej.lib.UnparsedTextURIResolver getUnparsedTextURIResolver() {
            return unparsedTextURIResolver;
        }
        public void setUnparsedTextURIResolver(Saxon.Hej.lib.UnparsedTextURIResolver resolver) {
            this.unparsedTextURIResolver = resolver;
        }
        public Saxon.Hej.trans.CompilerInfo getDefaultXsltCompilerInfo() {
            return defaultXsltCompilerInfo;
        }
        public Saxon.Hej.query.StaticQueryContext getDefaultStaticQueryContext() {
            if ((defaultStaticQueryContext == null)) {
                defaultStaticQueryContext = makeStaticQueryContext(false);
            }
            return defaultStaticQueryContext;
        }
        protected internal virtual Saxon.Hej.query.StaticQueryContext makeStaticQueryContext(bool copyFromDefault) {
            return staticQueryContextFactory.newStaticQueryContext(this, copyFromDefault);
        }
        public void registerFunctionAnnotationHandler(Saxon.Hej.lib.FunctionAnnotationHandler handler) {
            Saxon.Impl.Helpers.MapUtils.Put(functionAnnotationHandlers, handler.getAssertionNamespace(), handler);
        }
        public Saxon.Hej.lib.FunctionAnnotationHandler getFunctionAnnotationHandler(Saxon.Hej.om.NamespaceUri @namespace) {
            return Saxon.Impl.Helpers.MapUtils.Get(functionAnnotationHandlers, @namespace);
        }
        public virtual bool isStreamabilityEnabled() {
            return false;
        }
        public string getSourceParserClass() {
            return sourceParserClass;
        }
        public void setSourceParserClass(string sourceParserClass) {
            this.sourceParserClass = sourceParserClass;
        }
        public string getStyleParserClass() {
            return styleParserClass;
        }
        public void setStyleParserClass(string parser) {
            this.styleParserClass = parser;
        }
        public void setSerializerFactory(Saxon.Hej.lib.SerializerFactory factory) {
            serializerFactory = factory;
        }
        public Saxon.Hej.lib.SerializerFactory getSerializerFactory() {
            return serializerFactory;
        }
        public Saxon.Hej.serialize.charcode.CharacterSetFactory getCharacterSetFactory() {
            if ((characterSetFactory == null)) {
                characterSetFactory = new Saxon.Hej.serialize.charcode.CharacterSetFactory();
            }
            return characterSetFactory;
        }
        public void setDefaultSerializationProperties(Saxon.Ejava.util.Properties props) {
            defaultOutputProperties = props;
        }
        public Saxon.Ejava.util.Properties getDefaultSerializationProperties() {
            return defaultOutputProperties;
        }
        public Saxon.Hej.serialize.SerializationProperties obtainDefaultSerializationProperties() {
            return new Saxon.Hej.serialize.SerializationProperties(defaultOutputProperties);
        }
        public void processResultDocument(Saxon.Hej.expr.instruct.ResultDocument instruction, Saxon.Hej.expr.elab.PushEvaluator content, Saxon.Hej.expr.XPathContext context) {
            instruction.processInstruction(content, context);
        }
        public virtual Saxon.Hej.om.SequenceIterator getMultithreadedItemMappingIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.ItemMappingFunction action) {
            return new Saxon.Hej.expr.ItemMappingIterator(@base, action);
        }
        public bool isTiming() {
            return enabledProperties.contains(Saxon.Hej.lib.FeatureCode.TIMING);
        }
        public void setTiming(bool timing) {
            if (timing) {
                enabledProperties.add(Saxon.Hej.lib.FeatureCode.TIMING);
            } else {
                enabledProperties.remove(Saxon.Hej.lib.FeatureCode.TIMING);
            }
        }
        public bool isVersionWarning() {
            return false;
        }
        public void setVersionWarning(bool warn) {
        }
        public bool isValidation() {
            return ((defaultParseOptions.getDTDValidationMode() == Saxon.Hej.lib.Validation.STRICT) || (defaultParseOptions.getDTDValidationMode() == Saxon.Hej.lib.Validation.LAX));
        }
        public void setValidation(bool validation) {
            defaultParseOptions = defaultParseOptions.withDTDValidationMode((validation ? Saxon.Hej.lib.Validation.STRICT : Saxon.Hej.lib.Validation.STRIP));
        }
        public virtual Saxon.Hej.@event.FilterFactory makeDocumentProjector(Saxon.Hej.expr.parser.PathMap.PathMapRoot map) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Document projection requires Saxon-EE");
        }
        public virtual Saxon.Hej.@event.FilterFactory makeDocumentProjector(Saxon.Hej.query.XQueryExpression exp) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Document projection requires Saxon-EE");
        }
        public int getSchemaValidationMode() {
            return defaultParseOptions.getSchemaValidationMode();
        }
        public void setSchemaValidationMode(int validationMode) {
            defaultParseOptions = defaultParseOptions.withSchemaValidationMode(validationMode);
        }
        public void setValidationWarnings(bool warn) {
            defaultParseOptions = defaultParseOptions.withContinueAfterValidationErrors(warn);
        }
        public bool isValidationWarnings() {
            return defaultParseOptions.isContinueAfterValidationErrors();
        }
        public void setExpandAttributeDefaults(bool expand) {
            defaultParseOptions = defaultParseOptions.withExpandAttributeDefaults(expand);
        }
        public bool isExpandAttributeDefaults() {
            return defaultParseOptions.isExpandAttributeDefaults();
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return namePool;
        }
        public void setNamePool(Saxon.Hej.om.NamePool targetNamePool) {
            namePool = targetNamePool;
        }
        public virtual Saxon.Hej.type.TypeHierarchy getTypeHierarchy() {
            if ((typeHierarchy == null)) {
                typeHierarchy = new Saxon.Hej.type.TypeHierarchy(this);
            }
            return typeHierarchy;
        }
        public Saxon.Hej.expr.parser.TypeChecker getTypeChecker(bool backwardsCompatible) {
            if (backwardsCompatible) {
                return typeChecker10;
            } else {
                return typeChecker;
            }
        }
        public Saxon.Hej.trans.TypeAliasManager makeTypeAliasManager() {
            return new Saxon.Hej.trans.TypeAliasManager();
        }
        public Saxon.Hej.tree.util.DocumentNumberAllocator getDocumentNumberAllocator() {
            return documentNumberAllocator;
        }
        public void setDocumentNumberAllocator(Saxon.Hej.tree.util.DocumentNumberAllocator allocator) {
            documentNumberAllocator = allocator;
        }
        public bool isCompatible(Saxon.Hej.Configuration other) {
            return ((namePool == other.namePool) && (documentNumberAllocator == other.documentNumberAllocator));
        }
        public Saxon.Hej.om.DocumentPool getGlobalDocumentPool() {
            return globalDocumentPool;
        }
        public bool isStripsAllWhiteSpace() {
            return (defaultParseOptions.getSpaceStrippingRule() == Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance());
        }
        public void loadSchema(string absoluteURI) {
            readSchema(makePipelineConfiguration(), "", absoluteURI, null);
        }
        public virtual Saxon.Hej.om.NamespaceUri readSchema(Saxon.Hej.@event.PipelineConfiguration pipe, string baseURI, string schemaLocation, Saxon.Hej.om.NamespaceUri expected) {
            needEnterpriseEdition();
            return null;
        }
        public virtual void readMultipleSchemas(Saxon.Hej.@event.PipelineConfiguration pipe, string baseURI, System.Collections.Generic.IList<string> schemaLocations, Saxon.Hej.om.NamespaceUri expected) {
            needEnterpriseEdition();
        }
        public virtual Saxon.Hej.om.NamespaceUri readInlineSchema(Saxon.Hej.om.NodeInfo root, Saxon.Hej.om.NamespaceUri expected, Saxon.Hej.lib.ErrorReporter errorReporter) {
            needEnterpriseEdition();
            return null;
        }
        protected internal virtual void needEnterpriseEdition() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("You need the Enterprise Edition of Saxon (with an EnterpriseConfiguration) for this operation");
        }
        public void addSchemaSource(Saxon.Ejavax.xml.transform.Source schemaSource) {
            addSchemaSource(schemaSource, makeErrorReporter());
        }
        public virtual void addSchemaSource(Saxon.Ejavax.xml.transform.Source schemaSource, Saxon.Hej.lib.ErrorReporter errorReporter) {
            needEnterpriseEdition();
        }
        public virtual void addSchemaForBuiltInNamespace(Saxon.Hej.om.NamespaceUri @namespace) {
        }
        public virtual bool isSchemaAvailable(Saxon.Hej.om.NamespaceUri targetNamespace) {
            return false;
        }
        public virtual void clearSchemaCache() {
        }
        public virtual System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> getImportedNamespaces() {
            return new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>();
        }
        public virtual void sealNamespace(Saxon.Hej.om.NamespaceUri @namespace) {
        }
        public virtual System.Collections.Generic.ICollection<Saxon.Hej.expr.instruct.GlobalParam> getDeclaredSchemaParameters() {
            return null;
        }
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.type.SchemaType> getExtensionsOfType(Saxon.Hej.type.SchemaType type) {
            return new System.Collections.Generic.List<Saxon.Hej.type.SchemaType>();
        }
        public virtual void importComponents(Saxon.Ejavax.xml.transform.Source source) {
            needEnterpriseEdition();
        }
        public virtual void exportComponents(Saxon.Hej.@event.Receiver @out) {
            needEnterpriseEdition();
        }
        public virtual Saxon.Hej.type.SchemaDeclaration getElementDeclaration(int fingerprint) {
            return null;
        }
        public virtual Saxon.Hej.type.SchemaDeclaration getElementDeclaration(Saxon.Hej.om.StructuredQName qName) {
            return null;
        }
        public virtual Saxon.Hej.type.SchemaDeclaration getAttributeDeclaration(int fingerprint) {
            return null;
        }
        public virtual Saxon.Hej.type.SchemaDeclaration getAttributeDeclaration(Saxon.Hej.om.StructuredQName attributeName) {
            return null;
        }
        public virtual Saxon.Hej.type.SchemaType getSchemaType(Saxon.Hej.om.StructuredQName name) {
            if (name.hasURI(Saxon.Hej.om.NamespaceUri.SCHEMA)) {
                return Saxon.Hej.type.BuiltInType.getSchemaTypeByLocalName(name.getLocalPart());
            }
            return null;
        }
        public virtual Saxon.Hej.type.ItemType makeUserUnionType(System.Collections.Generic.IList<Saxon.Hej.type.AtomicType> memberTypes) {
            return null;
        }
        public virtual bool isDeclaredNotation(Saxon.Hej.om.NamespaceUri uri, string local) {
            return false;
        }
        public virtual void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType derived, Saxon.Hej.type.SchemaType @base, int block) {
        }
        public virtual void prepareValidationReporting(Saxon.Hej.expr.XPathContext context, Saxon.Hej.lib.ParseOptions options) {
        }
        public virtual Saxon.Hej.@event.Receiver getDocumentValidator(Saxon.Hej.@event.Receiver receiver, string systemId, Saxon.Hej.lib.ParseOptions validationOptions, Saxon.Hej.s9api.Location initiatingLocation) {
            return receiver;
        }
        public virtual Saxon.Hej.@event.Receiver getElementValidator(Saxon.Hej.@event.Receiver receiver, Saxon.Hej.lib.ParseOptions validationOptions, Saxon.Hej.s9api.Location locationId) {
            return receiver;
        }
        public virtual Saxon.Hej.type.SimpleType validateAttribute(Saxon.Hej.om.StructuredQName nodeName, Saxon.Hej.str.UnicodeString value, int validation) {
            return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
        }
        public virtual Saxon.Hej.@event.Receiver getAnnotationStripper(Saxon.Hej.@event.Receiver destination) {
            return destination;
        }
        public virtual Saxon.Hej.expr.parser.XPathParser newExpressionParser(string language, bool updating) {
            if ("XQ".Equals(language)) {
                if (updating) {
                    throw new Saxon.Hej.trans.XPathException("XQuery Update is supported only in Saxon-EE");
                } else {
                    return new Saxon.Hej.query.XQueryParser();
                }
            } else {
                if ("XP".Equals(language)) {
                    return new Saxon.Hej.expr.parser.XPathParser();
                } else {
                    if ("PATTERN".Equals(language)) {
                        return new Saxon.Hej.pattern.PatternParser();
                    } else {
                        throw new Saxon.Hej.trans.XPathException(("Unknown expression language "+language));
                    }
                }
            }
        }
        public virtual Saxon.Hej.trace.ExpressionPresenter newExpressionExporter(string target, System.IO.Stream destination, Saxon.Hej.style.StylesheetPackage rootPackage) {
            throw new Saxon.Hej.trans.XPathException("Exporting a stylesheet requires Saxon-EE");
        }
        public void setDebugger(Saxon.Hej.expr.instruct.Debugger debugger) {
            this.debugger = debugger;
        }
        public Saxon.Hej.expr.instruct.Debugger getDebugger() {
            return debugger;
        }
        public virtual Saxon.Hej.expr.instruct.SlotManager makeSlotManager() {
            if ((debugger == null)) {
                return new Saxon.Hej.expr.instruct.SlotManager();
            } else {
                return debugger.makeSlotManager();
            }
        }
        public virtual Saxon.Hej.@event.Receiver makeStreamingTransformer(Saxon.Hej.trans.Mode mode, Saxon.Hej.expr.instruct.ParameterSet ordinaryParams, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            throw new Saxon.Hej.trans.XPathException("Streaming is only available in Saxon-EE");
        }
        public virtual Saxon.Hej.expr.Expression makeStreamInstruction(Saxon.Hej.expr.Expression hrefExp, Saxon.Hej.expr.Expression body, bool streaming, Saxon.Hej.lib.ParseOptions options, Saxon.Hej.expr.PackageData packageData, Saxon.Hej.s9api.Location location, Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            Saxon.Hej.expr.instruct.SourceDocument si = new Saxon.Hej.expr.instruct.SourceDocument(hrefExp, body, options);
            si.setLocation(location);
            si.setRetainedStaticContext(rsc);
            return si;
        }
        public virtual System.Func<Saxon.Hej.om.SequenceIterator,Saxon.Hej.om.FocusTrackingIterator> getFocusTrackerFactory(Saxon.Hej.expr.instruct.Executable exec, bool multithreaded) {
            return /* ctor#1 */ (a) => new Saxon.Hej.om.FocusTrackingIterator(a);
        }
        public virtual void checkStrictStreamability(Saxon.Hej.style.XSLTemplate template, Saxon.Hej.expr.Expression body) {
        }
        public virtual bool isStreamedNode(Saxon.Hej.om.NodeInfo node) {
            return false;
        }
        public virtual Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions() {
            return optimizerOptions.intersect(Saxon.Hej.expr.parser.OptimizerOptions.FULL_HE_OPTIMIZATION);
        }
        public virtual Saxon.Hej.expr.parser.OptimizerOptions getPermittedOptimizerOptions() {
            return Saxon.Hej.expr.parser.OptimizerOptions.FULL_HE_OPTIMIZATION;
        }
        public virtual Saxon.Hej.expr.parser.Optimizer obtainOptimizer() {
            if ((optimizer == null)) {
                optimizer = new Saxon.Hej.expr.parser.Optimizer(this);
                optimizer.setOptimizerOptions(optimizerOptions.intersect(Saxon.Hej.expr.parser.OptimizerOptions.FULL_HE_OPTIMIZATION));
                return optimizer;
            } else {
                return optimizer;
            }
        }
        public virtual Saxon.Hej.expr.parser.Optimizer obtainOptimizer(Saxon.Hej.expr.parser.OptimizerOptions options) {
            Saxon.Hej.expr.parser.Optimizer optimizer = new Saxon.Hej.expr.parser.Optimizer(this);
            optimizer.setOptimizerOptions(options.intersect(Saxon.Hej.expr.parser.OptimizerOptions.FULL_HE_OPTIMIZATION));
            return optimizer;
        }
        public virtual Saxon.Hej.expr.parser.ContextItemStaticInfo makeContextItemStaticInfo(Saxon.Hej.type.ItemType itemType, bool maybeUndefined) {
            return new Saxon.Hej.expr.parser.ContextItemStaticInfo(itemType, maybeUndefined);
        }
        public virtual Saxon.Hej.expr.parser.ContextItemStaticInfo getDefaultContextItemStaticInfo() {
            return Saxon.Hej.expr.parser.ContextItemStaticInfo.DEFAULT;
        }
        public virtual Saxon.Hej.query.XQueryExpression makeXQueryExpression(Saxon.Hej.expr.Expression exp, Saxon.Hej.query.QueryModule mainModule, bool streaming) {
            Saxon.Hej.query.XQueryExpression xqe = new Saxon.Hej.query.XQueryExpression(exp, mainModule, false);
            if ((mainModule.getCodeInjector() != null)) {
                mainModule.getCodeInjector().process(xqe);
            }
            return xqe;
        }
        public virtual Saxon.Hej.om.GroundedValue makeSequenceExtent(Saxon.Hej.expr.Expression expression, int @ref, Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(expression.iterate(context));
        }
        public virtual Saxon.Hej.style.StyleNodeFactory makeStyleNodeFactory(Saxon.Hej.style.Compilation compilation) {
            return new Saxon.Hej.style.StyleNodeFactory(this, compilation);
        }
        public Saxon.Hej.expr.Expression makeEvaluateInstruction(Saxon.Hej.style.XSLEvaluate source, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression xpath = source.getTargetExpression();
            Saxon.Hej.value.SequenceType requiredType = source.getRequiredType();
            Saxon.Hej.expr.Expression contextItem = source.getContextItemExpression();
            Saxon.Hej.expr.Expression baseUri = source.getBaseUriExpression();
            Saxon.Hej.expr.Expression namespaceContext = source.getNamespaceContextExpression();
            Saxon.Hej.expr.Expression schemaAware = source.getSchemaAwareExpression();
            Saxon.Hej.expr.Expression withParams = source.getWithParamsExpression();
            Saxon.Hej.expr.instruct.EvaluateInstr inst = new Saxon.Hej.expr.instruct.EvaluateInstr(xpath, requiredType, contextItem, baseUri, namespaceContext, schemaAware);
            Saxon.Hej.expr.instruct.WithParam[] @params = source.getWithParamInstructions(inst, source.getCompilation(), decl, false);
            inst.setActualParams(@params);
            inst.setDynamicParams(withParams);
            inst.setDefaultXPathNamespace(source.getDefaultXPathNamespace());
            inst.setOptionsExpression(source.getOptionsExpression());
            return inst;
        }
        public virtual Saxon.Hej.style.StylesheetPackage makeStylesheetPackage() {
            return new Saxon.Hej.style.StylesheetPackage(this);
        }
        public virtual Saxon.Hej.expr.accum.AccumulatorRegistry makeAccumulatorRegistry() {
            return new Saxon.Hej.expr.accum.AccumulatorRegistry();
        }
        public Saxon.Hej.ma.map.MapItem externalObjectAsMap(Saxon.Hej.value.ObjectValue<object> value, string required) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public Saxon.Hej.expr.Expression makeObjectLookupExpression(Saxon.Hej.expr.Expression lhs, Saxon.Hej.expr.Expression rhs) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public Saxon.Hej.om.NodeInfo unravel(Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Hej.om.NodeInfo suppliedNode = null;
            if ((source is Saxon.Hej.om.NodeSource)) {
                suppliedNode = ((Saxon.Hej.om.NodeSource)(source)).getNode();
            }
            if ((suppliedNode != null)) {
                if (!(suppliedNode.getConfiguration().isCompatible(this))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Externally supplied NodeInfo belongs to the wrong Configuration");
                }
                return suppliedNode;
            }
            throw new Saxon.Ejava.lang.IllegalArgumentException((("A source of class "+source.GetType())+" is not recognized by any registered object model"));
        }
        public virtual bool isExtensionElementAvailable(Saxon.Hej.om.StructuredQName qName) {
            return false;
        }
        public void setStaticQueryContextFactory(Saxon.Hej.lib.StaticQueryContextFactory factory) {
            staticQueryContextFactory = factory;
        }
        public Saxon.Hej.query.StaticQueryContext newStaticQueryContext() {
            return makeStaticQueryContext(true);
        }
        public virtual Saxon.Hej.expr.PendingUpdateList newPendingUpdateList() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("XQuery update is supported only in Saxon-EE");
        }
        public Saxon.Hej.@event.PipelineConfiguration makePipelineConfiguration() {
            Saxon.Hej.@event.PipelineConfiguration pipe = new Saxon.Hej.@event.PipelineConfiguration(this, defaultParseOptions);
            pipe.setErrorReporter(makeErrorReporter());
            return pipe;
        }
        public virtual Saxon.Hej.lib.SchemaURIResolver makeSchemaURIResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            return null;
        }
        public static Saxon.Hej.Configuration getConfiguration(Saxon.Hej.expr.XPathContext context) {
            return context.getConfiguration();
        }
        public void setSourceResolver(Saxon.Hej.lib.SourceResolver resolver) {
            sourceResolver = resolver;
        }
        public Saxon.Hej.lib.SourceResolver getSourceResolver() {
            return sourceResolver;
        }
        public virtual Saxon.Hej.lib.ActiveSource resolveSource(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.Configuration config) {
            if ((source is Saxon.Hej.lib.ActiveSource)) {
                return (Saxon.Hej.lib.ActiveSource)(source);
            }
            if ((source is Saxon.Hej.lib.AugmentedSource)) {
                return new Inner_ActiveSource_440(this, source, config);
            }
            Saxon.Hej.lib.ActiveSource activeSource = Saxon.Hej.Version.platform.resolveSource(source, config);
            if ((activeSource != null)) {
                return activeSource;
            }
            return null;
        }
        public Saxon.Hej.om.TreeInfo buildDocumentTree(Saxon.Ejavax.xml.transform.Source source) {
            if ((source == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("source");
            }
            if ((source is Saxon.Hej.lib.AugmentedSource)) {
                return buildDocumentTree(((Saxon.Hej.lib.AugmentedSource)(source)).getContainedSource(), ((Saxon.Hej.lib.AugmentedSource)(source)).getParseOptions());
            } else {
                return buildDocumentTree(source, defaultParseOptions);
            }
        }
        public Saxon.Hej.om.TreeInfo buildDocumentTree(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.lib.ParseOptions parseOptions) {
            if ((source == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("source");
            }
            bool finallyClose = false;
            try {
                Saxon.Hej.lib.ParseOptions options = parseOptions;
                Saxon.Ejavax.xml.transform.Source src2 = resolveSource(source, this);
                if ((src2 == null)) {
                    throw new Saxon.Hej.trans.XPathException(("Unknown source class "+source.GetType().Name));
                }
                source = src2;
                if ((source is Saxon.Hej.lib.AugmentedSource)) {
                    options = options.merge(((Saxon.Hej.lib.AugmentedSource)(source)).getParseOptions());
                }
                options = options.applyDefaults(this);
                finallyClose = options.isPleaseCloseAfterUse();
                Saxon.Hej.om.TreeModel treeModel = options.getModel();
                bool lineNumbering = options.isLineNumbering();
                Saxon.Hej.@event.PipelineConfiguration pipe = makePipelineConfiguration();
                pipe.setParseOptions(options);
                Saxon.Hej.@event.Builder builder = treeModel.makeBuilder(pipe);
                builder.setTiming(isTiming());
                builder.setLineNumbering(lineNumbering);
                builder.setPipelineConfiguration(pipe);
                builder.setSystemId(source.getSystemId());
                Saxon.Hej.@event.Sender.send(source, builder, options);
                Saxon.Hej.om.NodeInfo newdoc = builder.getCurrentRoot();
                if ((newdoc.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT)) {
                    throw new Saxon.Hej.trans.XPathException("Source object represents a node other than a document node");
                }
                builder.reset();
                return newdoc.getTreeInfo();
            } finally {
                if (finallyClose) {
                    Saxon.Hej.lib.ParseOptions.close(source);
                }
            }
        }
        public Saxon.Hej.tree.tiny.TreeStatistics getTreeStatistics() {
            return treeStatistics;
        }
        public Saxon.Hej.@event.Receiver makeEmitter(string eqName, Saxon.Ejava.util.Properties props) {
            Saxon.Hej.om.StructuredQName sqName = Saxon.Hej.om.StructuredQName.fromEQName(eqName);
            string className = sqName.getLocalPart();
            System.Object handler;
            try {
                handler = dynamicLoader.getInstance(className, null);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.XPathException(("Cannot create user-supplied output method. "+e.Message), Saxon.Hej.trans.SaxonErrorCode.SXCH0004);
            }
            if ((handler is Saxon.Hej.@event.Receiver)) {
                return (Saxon.Hej.@event.Receiver)(handler);
            } else {
                throw new Saxon.Hej.trans.XPathException((("Output method "+className)+" is neither a Receiver nor a SAX2 ContentHandler"));
            }
        }
        public virtual void setConfigurationProperty(string name, System.Object value) {
            if (Saxon.Hej.lib.FeatureIndex.exists(name)) {
                setFeature(Saxon.Hej.lib.FeatureIndex.getData(name), value);
            } else {
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unrecognized configuration feature: "+name));
            }
        }
        public void setConfigurationProperty<T> (Saxon.Hej.lib.Feature<T> feature, T value) {
            setFeature(Saxon.Hej.lib.FeatureIndex.getData(feature.code), value);
        }
        public virtual  /* REPLACING protected internal virtual */ void setFeature(Saxon.Hej.lib.FeatureData feature, System.Object value) {
            string name = feature.uri;
            int code = feature.code;
            if (booleanFeatures.contains(code)) {
                if ((code == Saxon.Hej.lib.FeatureCode.COMPILE_WITH_TRACING)) {
                    bool b = requireBoolean(name, value);
                    setCompileWithTracing(b);
                } else {
                    if ((code == Saxon.Hej.lib.FeatureCode.DTD_VALIDATION)) {
                        bool b = requireBoolean(name, value);
                        setValidation(b);
                    } else {
                        if ((code == Saxon.Hej.lib.FeatureCode.EXPAND_ATTRIBUTE_DEFAULTS)) {
                            bool b = requireBoolean(name, value);
                            setExpandAttributeDefaults(b);
                        } else {
                            if ((code == Saxon.Hej.lib.FeatureCode.ALLOW_SYNTAX_EXTENSIONS)) {
                                bool b = requireBoolean(name, value);
                                defaultXsltCompilerInfo.setXsltVersion((b ? 40 : 30));
                                if ((defaultStaticQueryContext != null)) {
                                    defaultStaticQueryContext.setLanguageVersion((b ? 40 : 30));
                                }
                            }
                        }
                    }
                }
                internalSetBooleanProperty(code, name, value);
            } else {
                if (stringFeatures.contains(code)) {
                    stringProperties.put(code, requireString(name, value));
                } else {
                    switch (code) {
                      case Saxon.Hej.lib.FeatureCode.ALLOWED_PROTOCOLS:
                        protocolRestrictor = new Saxon.Hej.lib.ProtocolRestrictor((string)(value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.COLLATION_URI_RESOLVER:
                        if (!(((value is Saxon.Hej.lib.CollationURIResolver)))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("COLLATION_URI_RESOLVER value must be an instance of net.sf.saxon.lib.CollationURIResolver");
                        }
                        setCollationURIResolver((Saxon.Hej.lib.CollationURIResolver)(value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.COLLATION_URI_RESOLVER_CLASS:
                        setCollationURIResolver((Saxon.Hej.lib.CollationURIResolver)(instantiateClassName(name, value, typeof(Saxon.Hej.lib.CollationURIResolver))));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.COLLECTION_FINDER:
                        if (!(((value is Saxon.Hej.lib.CollectionFinder)))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("COLLECTION_FINDER value must be an instance of net.sf.saxon.lib.ICollectionFinder");
                        }
                        setCollectionFinder((Saxon.Hej.lib.CollectionFinder)(value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.COLLECTION_FINDER_CLASS:
                        setCollectionFinder((Saxon.Hej.lib.CollectionFinder)(instantiateClassName(name, value, typeof(Saxon.Hej.lib.CollectionFinder))));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.DEFAULT_COLLATION:
                        defaultCollationName = value.ToString();
                        break ;
                      case Saxon.Hej.lib.FeatureCode.DEFAULT_COLLECTION:
                        setDefaultCollection(value.ToString());
                        break ;
                      case Saxon.Hej.lib.FeatureCode.DEFAULT_COUNTRY:
                        setDefaultCountry(value.ToString());
                        break ;
                      case Saxon.Hej.lib.FeatureCode.DEFAULT_LANGUAGE:
                        setDefaultLanguage(value.ToString());
                        break ;
                      case Saxon.Hej.lib.FeatureCode.DEFAULT_REGEX_ENGINE:
                        setDefaultRegexEngine(value.ToString());
                        break ;
                      case Saxon.Hej.lib.FeatureCode.DTD_VALIDATION_RECOVERABLE:
                        {
                            bool b = requireBoolean(name, value);
                            if (b) {
                                defaultParseOptions = defaultParseOptions.withDTDValidationMode(Saxon.Hej.lib.Validation.LAX);
                            } else {
                                defaultParseOptions = defaultParseOptions.withDTDValidationMode((isValidation() ? Saxon.Hej.lib.Validation.STRICT : Saxon.Hej.lib.Validation.SKIP));
                            }
                            internalSetBooleanProperty(code, name, b);
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.ENVIRONMENT_VARIABLE_RESOLVER:
                        if (!(((value is Saxon.Hej.lib.EnvironmentVariableResolver)))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("ENVIRONMENT_VARIABLE_RESOLVER value must be an instance of net.sf.saxon.lib.EnvironmentVariableResolver");
                        }
                        environmentVariableResolver = (Saxon.Hej.lib.EnvironmentVariableResolver)(value);
                        break ;
                      case Saxon.Hej.lib.FeatureCode.ENVIRONMENT_VARIABLE_RESOLVER_CLASS:
                        environmentVariableResolver = (Saxon.Hej.lib.EnvironmentVariableResolver)(instantiateClassName(name, value, typeof(Saxon.Hej.lib.EnvironmentVariableResolver)));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.ERROR_LISTENER_CLASS:
                        break ;
                      case Saxon.Hej.lib.FeatureCode.LINE_NUMBERING:
                        {
                            bool b = requireBoolean(name, value);
                            setLineNumbering(b);
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.MODULE_URI_RESOLVER:
                        if (!(((value is Saxon.Hej.lib.ModuleURIResolver)))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("MODULE_URI_RESOLVER value must be an instance of net.sf.saxon.lib.ModuleURIResolver");
                        }
                        setModuleURIResolver((Saxon.Hej.lib.ModuleURIResolver)(value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.MODULE_URI_RESOLVER_CLASS:
                        Saxon.Hej.lib.ModuleURIResolver resolver = (Saxon.Hej.lib.ModuleURIResolver)(instantiateClassName(name, value, typeof(Saxon.Hej.lib.ModuleURIResolver)));
                        if ((resolver is Saxon.Hej.lib.StandardModuleURIResolver)) {
                            ((Saxon.Hej.lib.StandardModuleURIResolver)(resolver)).setConfiguration(this);
                        }
                        setModuleURIResolver(resolver);
                        break ;
                      case Saxon.Hej.lib.FeatureCode.NAME_POOL:
                        if (!(((value is Saxon.Hej.om.NamePool)))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("NAME_POOL value must be an instance of net.sf.saxon.om.NamePool");
                        }
                        setNamePool((Saxon.Hej.om.NamePool)(value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.OPTIMIZATION_LEVEL:
                        if ((value is int)) {
                            int v = (int)(value);
                            optimizerOptions = ((v == 0) ? new Saxon.Hej.expr.parser.OptimizerOptions(0) : Saxon.Hej.expr.parser.OptimizerOptions.FULL_EE_OPTIMIZATION.intersect(getPermittedOptimizerOptions()));
                        } else {
                            string s = requireString(name, value);
                            if (System.Text.RegularExpressions.Regex.IsMatch(s, "[0-9]+")) {
                                optimizerOptions = ("0".Equals(s) ? new Saxon.Hej.expr.parser.OptimizerOptions(0) : Saxon.Hej.expr.parser.OptimizerOptions.FULL_EE_OPTIMIZATION.intersect(getPermittedOptimizerOptions()));
                            } else {
                                optimizerOptions = new Saxon.Hej.expr.parser.OptimizerOptions(s).intersect(getPermittedOptimizerOptions());
                            }
                        }
                        if ((optimizer != null)) {
                            optimizer.setOptimizerOptions(optimizerOptions);
                        }
                        defaultXsltCompilerInfo.setOptimizerOptions(optimizerOptions);
                        break ;
                      case Saxon.Hej.lib.FeatureCode.RECOVERY_POLICY:
                        break ;
                      case Saxon.Hej.lib.FeatureCode.RECOVERY_POLICY_NAME:
                        break ;
                      case Saxon.Hej.lib.FeatureCode.REGEX_BACKTRACKING_LIMIT:
                        regexBacktrackingLimit = requireInteger(name, value);
                        break ;
                      case Saxon.Hej.lib.FeatureCode.SERIALIZER_FACTORY_CLASS:
                        setSerializerFactory((Saxon.Hej.lib.SerializerFactory)(instantiateClassName(name, value, typeof(Saxon.Hej.lib.SerializerFactory))));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.SCHEMA_VALIDATION:
                        {
                            setSchemaValidationMode(requireInteger(name, value));
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.SCHEMA_VALIDATION_MODE:
                        string mode = requireString(name, value);
                        setSchemaValidationMode(Saxon.Hej.lib.Validation.getCode(mode));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.SOURCE_PARSER_CLASS:
                        setSourceParserClass(requireString(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.SOURCE_RESOLVER_CLASS:
                        setSourceResolver((Saxon.Hej.lib.SourceResolver)(instantiateClassName(name, value, typeof(Saxon.Hej.lib.SourceResolver))));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.STANDARD_ERROR_OUTPUT_FILE:
                        try {
                            bool append = true;
                            bool autoFlush = true;
                            setStandardErrorOutput(new Saxon.Ejava.io.PrintStream(new System.IO.FileStream((string)(value), append? System.IO.FileMode.Append : System.IO.FileMode.Create), autoFlush));
                        } catch (Saxon.Ejava.io.FileNotFoundException fnf) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException(fnf);
                        }
                        break ;
                      case Saxon.Hej.lib.FeatureCode.STRIP_WHITESPACE:
                        {
                            string s = requireString(name, value);
                            Saxon.Hej.om.SpaceStrippingRule rule;
                            switch (s) {
                              case "all":
                                rule = Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance();
                                break ;
                              case "none":
                                rule = Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance();
                                break ;
                              case "ignorable":
                                rule = Saxon.Hej.om.IgnorableSpaceStrippingRule.getInstance();
                                break ;
                              default:
                                throw new Saxon.Ejava.lang.IllegalArgumentException((("Unrecognized value STRIP_WHITESPACE = '"+value)+"': must be 'all', 'none', or 'ignorable'"));
                            }
                            defaultParseOptions = defaultParseOptions.withSpaceStrippingRule(rule);
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.STYLE_PARSER_CLASS:
                        setStyleParserClass(requireString(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.TIMING:
                        setTiming(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.TRACE_LISTENER:
                        if (!(((value is Saxon.Hej.lib.TraceListener)))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("TRACE_LISTENER is of wrong class");
                        }
                        setTraceListener((Saxon.Hej.lib.TraceListener)(value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.TRACE_LISTENER_CLASS:
                        setTraceListenerClass(requireString(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.TRACE_LISTENER_OUTPUT_FILE:
                        setTraceListenerOutputFile(requireString(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.TREE_MODEL:
                        setTreeModel(requireInteger(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.TREE_MODEL_NAME:
                        {
                            string s = requireString(name, value);
                            switch (s) {
                              case "tinyTree":
                                setTreeModel(Saxon.Hej.@event.Builder.TINY_TREE);
                                break ;
                              case "tinyTreeCondensed":
                                setTreeModel(Saxon.Hej.@event.Builder.TINY_TREE_CONDENSED);
                                break ;
                              case "linkedTree":
                                setTreeModel(Saxon.Hej.@event.Builder.LINKED_TREE);
                                break ;
                              case "jdom":
                                setTreeModel(Saxon.Hej.@event.Builder.JDOM_TREE);
                                break ;
                              case "jdom2":
                                setTreeModel(Saxon.Hej.@event.Builder.JDOM2_TREE);
                                break ;
                              default:
                                throw new Saxon.Ejava.lang.IllegalArgumentException((("Unrecognized value TREE_MODEL_NAME = '"+value)+"': must be linkedTree|tinyTree|tinyTreeCondensed"));
                            }
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.UNPARSED_TEXT_URI_RESOLVER:
                        setUnparsedTextURIResolver((Saxon.Hej.lib.UnparsedTextURIResolver)(value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.UNPARSED_TEXT_URI_RESOLVER_CLASS:
                        setUnparsedTextURIResolver((Saxon.Hej.lib.UnparsedTextURIResolver)(instantiateClassName(name, value, typeof(Saxon.Hej.lib.UnparsedTextURIResolver))));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.USE_XSI_SCHEMA_LOCATION:
                        defaultParseOptions = defaultParseOptions.withUseXsiSchemaLocation(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.VALIDATION_COMMENTS:
                        defaultParseOptions = defaultParseOptions.withAddCommentsAfterValidationErrors(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.VALIDATION_WARNINGS:
                        setValidationWarnings(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.VERSION_WARNING:
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XINCLUDE:
                        setXIncludeAware(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XPATH_VERSION_FOR_XSD:
                        {
                            int val = requireInteger(name, value);
                            if ((((val != 20) && (val != 30)) && (val != 31))) {
                                throw new Saxon.Ejava.lang.IllegalArgumentException("XPath version for XSD must be 20 (XPath 2.0), 30 (XPath 3.0), or 31 (XPath 3.1)");
                            }
                            xpathVersionForXsd = val;
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.XPATH_VERSION_FOR_XSLT:
                        {
                            int val = requireInteger(name, value);
                            if ((((((val != 20) && (val != 30)) && (val != 305)) && (val != 31)) && (val != 40))) {
                                throw new Saxon.Ejava.lang.IllegalArgumentException("XPath version for XSLT must be 20 (XPath 2.0), 30 (XPath 3.0), 31 (XPath 3.1), or 305 (XPath 3.0 with XSLT-defined extensions), or 40 (XPath 4.0 proposal)");
                            }
                            xpathVersionForXslt = val;
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.XQUERY_ALLOW_UPDATE:
                        getDefaultStaticQueryContext().setUpdatingEnabled(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_CONSTRUCTION_MODE:
                        getDefaultStaticQueryContext().setConstructionMode(Saxon.Hej.lib.Validation.getCode(value.ToString()));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_DEFAULT_ELEMENT_NAMESPACE:
                        getDefaultStaticQueryContext().setDefaultElementNamespace(Saxon.Hej.om.NamespaceUri.of(value.ToString()));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_DEFAULT_FUNCTION_NAMESPACE:
                        getDefaultStaticQueryContext().setDefaultFunctionNamespace(Saxon.Hej.om.NamespaceUri.of(value.ToString()));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_EMPTY_LEAST:
                        getDefaultStaticQueryContext().setEmptyLeast(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_INHERIT_NAMESPACES:
                        getDefaultStaticQueryContext().setInheritNamespaces(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_PRESERVE_BOUNDARY_SPACE:
                        getDefaultStaticQueryContext().setPreserveBoundarySpace(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_PRESERVE_NAMESPACES:
                        getDefaultStaticQueryContext().setPreserveNamespaces(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_REQUIRED_CONTEXT_ITEM_TYPE:
                        Saxon.Hej.expr.parser.XPathParser parser = new Saxon.Hej.expr.parser.XPathParser();
                        Saxon.Hej.sxpath.IndependentContext env = new Saxon.Hej.sxpath.IndependentContext(this);
                        env.setXPathLanguageLevel(31);
                        try {
                            Saxon.Hej.value.SequenceType type = parser.parseSequenceType(value.ToString(), env);
                            if ((type.getCardinality() != Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                                throw new Saxon.Ejava.lang.IllegalArgumentException("Context item type must have no occurrence indicator");
                            }
                            getDefaultStaticQueryContext().setRequiredContextItemType(type.getPrimaryType());
                        } catch (Saxon.Hej.trans.XPathException err) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException(err);
                        }
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_SCHEMA_AWARE:
                        getDefaultStaticQueryContext().setSchemaAware(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XQUERY_VERSION:
                        {
                            int qvn;
                            switch (value.ToString()) {
                              case "3.1":
                                qvn = 31;
                                break ;
                              case "4.0":
                                qvn = 40;
                                break ;
                              default:
                                makeErrorReporter().report(((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident("XQuery version ignored: only \"3.1\" and \"4.0\" are recognized", Saxon.Hej.trans.SaxonErrorCode.SXWN9049).asWarning()));
                                qvn = 40;
                                break ;
                            }
                            getDefaultStaticQueryContext().setLanguageVersion(qvn);
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.XML_VERSION:
                        string xv = requireString(name, value);
                        if (!(((xv.Equals("1.0") || xv.Equals("1.1"))))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("XML_VERSION value must be \"1.0\" or \"1.1\" as a String");
                        }
                        setXMLVersion((xv.Equals("1.0") ? XML10 : XML11));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XSD_VERSION:
                        {
                            string xsdVn = requireString(name, value);
                            if (!(((xsdVn.Equals("1.0") || xsdVn.Equals("1.1"))))) {
                                throw new Saxon.Ejava.lang.IllegalArgumentException("XSD_VERSION value must be \"1.0\" or \"1.1\" as a String");
                            }
                            xsdVersion = (xsdVn.Equals("1.0") ? XSD10 : XSD11);
                            theConversionRules = null;
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.XSLT_ENABLE_ASSERTIONS:
                        getDefaultXsltCompilerInfo().setAssertionsEnabled(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XSLT_INITIAL_MODE:
                        {
                            string s = requireString(name, value);
                            getDefaultXsltCompilerInfo().setDefaultInitialMode(Saxon.Hej.om.StructuredQName.fromClarkName(s));
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.XSLT_INITIAL_TEMPLATE:
                        {
                            string s = requireString(name, value);
                            getDefaultXsltCompilerInfo().setDefaultInitialTemplate(Saxon.Hej.om.StructuredQName.fromClarkName(s));
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.XSLT_SCHEMA_AWARE:
                        getDefaultXsltCompilerInfo().setSchemaAware(requireBoolean(name, value));
                        break ;
                      case Saxon.Hej.lib.FeatureCode.XSLT_VERSION:
                        {
                            int xsltVersion;
                            switch (value.ToString()) {
                              case "3.0":
                                xsltVersion = 30;
                                break ;
                              case "4.0":
                                xsltVersion = 40;
                                break ;
                              default:
                                makeErrorReporter().report(((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident("XSLT version ignored: only \"3.0\" and \"4.0\" are recognized", Saxon.Hej.trans.SaxonErrorCode.SXWN9020).asWarning()));
                                xsltVersion = 30;
                                break ;
                            }
                            getDefaultXsltCompilerInfo().setXsltVersion(xsltVersion);
                            break ;
                        }
                      case Saxon.Hej.lib.FeatureCode.RESOURCE_RESOLVER:
                        if (!(((value is Saxon.Hej.lib.JJ_ResourceResolver)))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("RESOURCE_RESOLVER value must be an instance of net.sf.saxon.lib.ResourceResolver");
                        }
                        setResourceResolver((Saxon.Hej.lib.JJ_ResourceResolver)(value));
                        break ;
                      default:
                        throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown configuration property "+name));
                    }
                }
            }
        }
        public static bool requireBoolean(string propertyName, System.Object value) {
            if ((value is bool)) {
                return (bool)(value);
            } else {
                if ((value is string)) {
                    value = ((string)(value)).Trim();
                    if (((("true".Equals(value) || "on".Equals(value)) || "yes".Equals(value)) || "1".Equals(value))) {
                        return true;
                    } else {
                        if (((("false".Equals(value) || "off".Equals(value)) || "no".Equals(value)) || "0".Equals(value))) {
                            return false;
                        } else {
                            throw new Saxon.Ejava.lang.IllegalArgumentException((propertyName+" must be 'true' or 'false' (or on|off, yes|no, 1|0)"));
                        }
                    }
                } else {
                    throw new Saxon.Ejava.lang.IllegalArgumentException((propertyName+" must be a boolean (or a string representing a boolean)"));
                }
            }
        }
        protected internal int requireInteger(string propertyName, System.Object value) {
            if ((value is int)) {
                return (int)(value);
            } else {
                if ((value is string)) {
                    try {
                        return Saxon.Impl.Helpers.Utils.ParseInt((string)(value));
                    } catch (Saxon.Ejava.lang.NumberFormatException nfe) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException((propertyName+" must be an integer"));
                    }
                } else {
                    throw new Saxon.Ejava.lang.IllegalArgumentException((propertyName+" must be an integer (or a string representing an integer)"));
                }
            }
        }
        protected internal void internalSetBooleanProperty(int code, string name, System.Object value) {
            bool b = requireBoolean(name, value);
            if (b) {
                enabledProperties.add(code);
            } else {
                enabledProperties.remove(code);
            }
        }
        public bool getBooleanProperty(Saxon.Hej.lib.Feature<bool> feature) {
            return enabledProperties.contains(feature.code);
        }
        public void setBooleanProperty(string propertyName, bool value) {
            setConfigurationProperty(propertyName, value);
        }
        public void setBooleanProperty(Saxon.Hej.lib.Feature<bool> feature, bool value) {
            setConfigurationProperty(feature, value);
        }
        protected internal string requireString(string propertyName, System.Object value) {
            if ((value is string)) {
                return (string)(value);
            } else {
                throw new Saxon.Ejava.lang.IllegalArgumentException((("The value of "+propertyName)+" must be a string"));
            }
        }
        protected internal System.Object instantiateClassName(string propertyName, System.Object value, System.Type requiredClass) {
            if (!(((value is string)))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException((propertyName+" must be a String"));
            }
            try {
                System.Object obj = getInstance((string)(value));
                if (!(requiredClass.IsAssignableFrom(obj.GetType()))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(((((("Error in "+propertyName)+": Class ")+value)+" does not implement ")+requiredClass.Name));
                }
                return obj;
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Ejava.lang.IllegalArgumentException(((((("Cannot use "+value)+" as the value of ")+propertyName)+". ")+err.Message));
            }
        }
        public System.Object getConfigurationProperty(string name) {
            if (Saxon.Hej.lib.FeatureIndex.exists(name)) {
                return getFeature(Saxon.Hej.lib.FeatureIndex.getData(name));
            } else {
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown configuration property "+name));
            }
        }
        public T getConfigurationProperty<T> (Saxon.Hej.lib.Feature<T> feature) {
            Saxon.Hej.lib.FeatureData data = Saxon.Hej.lib.FeatureIndex.getData(feature.code);
            return (T)(getFeature(data));
        }
        protected internal virtual System.Object getFeature(Saxon.Hej.lib.FeatureData feature) {
            int code = feature.code;
            if (booleanFeatures.contains(code)) {
                return enabledProperties.contains(code);
            }
            if (stringFeatures.contains(code)) {
                string value = stringProperties.get(code);
                if ((value == null)) {
                    return feature.defaultValue;
                } else {
                    return value;
                }
            }
            switch (code) {
              case Saxon.Hej.lib.FeatureCode.ALLOWED_PROTOCOLS:
                return protocolRestrictor.ToString();
              case Saxon.Hej.lib.FeatureCode.COLLATION_URI_RESOLVER:
                return getCollationURIResolver();
              case Saxon.Hej.lib.FeatureCode.COLLATION_URI_RESOLVER_CLASS:
                return getCollationURIResolver().GetType().Name;
              case Saxon.Hej.lib.FeatureCode.CONFIGURATION:
                return this;
              case Saxon.Hej.lib.FeatureCode.DEFAULT_COLLATION:
                return defaultCollationName;
              case Saxon.Hej.lib.FeatureCode.DEFAULT_COLLECTION:
                return getDefaultCollection();
              case Saxon.Hej.lib.FeatureCode.DEFAULT_COUNTRY:
                return getDefaultCountry();
              case Saxon.Hej.lib.FeatureCode.DEFAULT_LANGUAGE:
                return getDefaultLanguage();
              case Saxon.Hej.lib.FeatureCode.DTD_VALIDATION:
                return isValidation();
              case Saxon.Hej.lib.FeatureCode.DTD_VALIDATION_RECOVERABLE:
                return (defaultParseOptions.getDTDValidationMode() == Saxon.Hej.lib.Validation.LAX);
              case Saxon.Hej.lib.FeatureCode.ERROR_LISTENER_CLASS:
                return null;
              case Saxon.Hej.lib.FeatureCode.ENVIRONMENT_VARIABLE_RESOLVER:
                return environmentVariableResolver;
              case Saxon.Hej.lib.FeatureCode.ENVIRONMENT_VARIABLE_RESOLVER_CLASS:
                return environmentVariableResolver.GetType().Name;
              case Saxon.Hej.lib.FeatureCode.EXPAND_ATTRIBUTE_DEFAULTS:
                return isExpandAttributeDefaults();
              case Saxon.Hej.lib.FeatureCode.LINE_NUMBERING:
                return isLineNumbering();
              case Saxon.Hej.lib.FeatureCode.MODULE_URI_RESOLVER:
                return getModuleURIResolver();
              case Saxon.Hej.lib.FeatureCode.MODULE_URI_RESOLVER_CLASS:
                return getModuleURIResolver().GetType().Name;
              case Saxon.Hej.lib.FeatureCode.NAME_POOL:
                return getNamePool();
              case Saxon.Hej.lib.FeatureCode.OPTIMIZATION_LEVEL:
                return optimizerOptions.ToString();
              case Saxon.Hej.lib.FeatureCode.RECOVERY_POLICY:
                return 0;
              case Saxon.Hej.lib.FeatureCode.RECOVERY_POLICY_NAME:
                return "recoverWithWarnings";
              case Saxon.Hej.lib.FeatureCode.REGEX_BACKTRACKING_LIMIT:
                return regexBacktrackingLimit;
              case Saxon.Hej.lib.FeatureCode.SCHEMA_VALIDATION:
                return getSchemaValidationMode();
              case Saxon.Hej.lib.FeatureCode.SCHEMA_VALIDATION_MODE:
                return Saxon.Hej.lib.Validation.describe(getSchemaValidationMode());
              case Saxon.Hej.lib.FeatureCode.SERIALIZER_FACTORY_CLASS:
                return getSerializerFactory().GetType().Name;
              case Saxon.Hej.lib.FeatureCode.SOURCE_PARSER_CLASS:
                return getSourceParserClass();
              case Saxon.Hej.lib.FeatureCode.SOURCE_RESOLVER_CLASS:
                return getSourceResolver().GetType().Name;
              case Saxon.Hej.lib.FeatureCode.STRIP_WHITESPACE:
                Saxon.Hej.om.SpaceStrippingRule rule = getParseOptions().getSpaceStrippingRule();
                if ((rule == Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance())) {
                    return "all";
                } else {
                    if (((rule == null) || (rule == Saxon.Hej.om.IgnorableSpaceStrippingRule.getInstance()))) {
                        return "ignorable";
                    } else {
                        return "none";
                    }
                }
              case Saxon.Hej.lib.FeatureCode.STYLE_PARSER_CLASS:
                return getStyleParserClass();
              case Saxon.Hej.lib.FeatureCode.TIMING:
                return isTiming();
              case Saxon.Hej.lib.FeatureCode.TRACE_LISTENER:
                return traceListener;
              case Saxon.Hej.lib.FeatureCode.TRACE_LISTENER_CLASS:
                return traceListenerClass;
              case Saxon.Hej.lib.FeatureCode.TRACE_LISTENER_OUTPUT_FILE:
                return traceListenerOutput;
              case Saxon.Hej.lib.FeatureCode.TREE_MODEL:
                return getTreeModel();
              case Saxon.Hej.lib.FeatureCode.TREE_MODEL_NAME:
                switch (getTreeModel()) {
                  case Saxon.Hej.@event.Builder.TINY_TREE:
                  default:
                    return "tinyTree";
                  case Saxon.Hej.@event.Builder.TINY_TREE_CONDENSED:
                    return "tinyTreeCondensed";
                  case Saxon.Hej.@event.Builder.LINKED_TREE:
                    return "linkedTree";
                }
              case Saxon.Hej.lib.FeatureCode.UNPARSED_TEXT_URI_RESOLVER:
                return getUnparsedTextURIResolver();
              case Saxon.Hej.lib.FeatureCode.UNPARSED_TEXT_URI_RESOLVER_CLASS:
                return getUnparsedTextURIResolver().GetType().Name;
              case Saxon.Hej.lib.FeatureCode.USE_XSI_SCHEMA_LOCATION:
                return defaultParseOptions.isUseXsiSchemaLocation();
              case Saxon.Hej.lib.FeatureCode.VALIDATION_COMMENTS:
                return defaultParseOptions.isAddCommentsAfterValidationErrors();
              case Saxon.Hej.lib.FeatureCode.VALIDATION_WARNINGS:
                return isValidationWarnings();
              case Saxon.Hej.lib.FeatureCode.VERSION_WARNING:
                return false;
              case Saxon.Hej.lib.FeatureCode.XINCLUDE:
                return isXIncludeAware();
              case Saxon.Hej.lib.FeatureCode.XML_VERSION:
                return ((getXMLVersion() == XML10) ? "1.0" : "1.1");
              case Saxon.Hej.lib.FeatureCode.XQUERY_ALLOW_UPDATE:
                return getDefaultStaticQueryContext().isUpdatingEnabled();
              case Saxon.Hej.lib.FeatureCode.XQUERY_CONSTRUCTION_MODE:
                return getDefaultStaticQueryContext().getConstructionMode();
              case Saxon.Hej.lib.FeatureCode.XQUERY_DEFAULT_ELEMENT_NAMESPACE:
                return getDefaultStaticQueryContext().getDefaultElementNamespace();
              case Saxon.Hej.lib.FeatureCode.XQUERY_DEFAULT_FUNCTION_NAMESPACE:
                return getDefaultStaticQueryContext().getDefaultFunctionNamespace();
              case Saxon.Hej.lib.FeatureCode.XQUERY_EMPTY_LEAST:
                return getDefaultStaticQueryContext().isEmptyLeast();
              case Saxon.Hej.lib.FeatureCode.XQUERY_INHERIT_NAMESPACES:
                return getDefaultStaticQueryContext().isInheritNamespaces();
              case Saxon.Hej.lib.FeatureCode.XQUERY_PRESERVE_BOUNDARY_SPACE:
                return getDefaultStaticQueryContext().isPreserveBoundarySpace();
              case Saxon.Hej.lib.FeatureCode.XQUERY_PRESERVE_NAMESPACES:
                return getDefaultStaticQueryContext().isPreserveNamespaces();
              case Saxon.Hej.lib.FeatureCode.XQUERY_REQUIRED_CONTEXT_ITEM_TYPE:
                return getDefaultStaticQueryContext().getRequiredContextItemType();
              case Saxon.Hej.lib.FeatureCode.XQUERY_SCHEMA_AWARE:
                return getDefaultStaticQueryContext().isSchemaAware();
              case Saxon.Hej.lib.FeatureCode.XQUERY_VERSION:
                return "3.1";
              case Saxon.Hej.lib.FeatureCode.XPATH_VERSION_FOR_XSD:
                return xpathVersionForXsd;
              case Saxon.Hej.lib.FeatureCode.XPATH_VERSION_FOR_XSLT:
                return xpathVersionForXslt;
              case Saxon.Hej.lib.FeatureCode.XSD_VERSION:
                return ((xsdVersion == XSD10) ? "1.0" : "1.1");
              case Saxon.Hej.lib.FeatureCode.XSLT_ENABLE_ASSERTIONS:
                return getDefaultXsltCompilerInfo().isAssertionsEnabled();
              case Saxon.Hej.lib.FeatureCode.XSLT_INITIAL_MODE:
                return getDefaultXsltCompilerInfo().getDefaultInitialMode().getClarkName();
              case Saxon.Hej.lib.FeatureCode.XSLT_INITIAL_TEMPLATE:
                return getDefaultXsltCompilerInfo().getDefaultInitialTemplate().getClarkName();
              case Saxon.Hej.lib.FeatureCode.XSLT_SCHEMA_AWARE:
                return getDefaultXsltCompilerInfo().isSchemaAware();
              case Saxon.Hej.lib.FeatureCode.XSLT_STATIC_URI_RESOLVER_CLASS:
                return null;
              case Saxon.Hej.lib.FeatureCode.XSLT_VERSION:
                {
                    int vn = getDefaultXsltCompilerInfo().getXsltVersion();
                    return ((vn == 40) ? "4.0" : "3.0");
                }
              case Saxon.Hej.lib.FeatureCode.RESOURCE_RESOLVER:
                return getResourceResolver();
            }
            throw new Saxon.Ejava.lang.IllegalArgumentException("Unknown configuration property ");
        }
        public virtual bool isJITEnabled() {
            return false;
        }
        public void close() {
            if ((traceOutput != null)) {
                traceOutput.close();
            }
        }
        public virtual Saxon.Hej.trans.packages.IPackageLoader makePackageLoader() {
            return new Saxon.Hej.trans.PackageLoaderHE(this);
        }
        public virtual Saxon.Hej.lib.InvalidityReportGenerator createValidityReporter() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Schema validation requires Saxon-EE");
        }
        public class LicenseFeature {
            public const int SCHEMA_VALIDATION = 1;
            public const int ENTERPRISE_XSLT = 2;
            public const int ENTERPRISE_XQUERY = 4;
            public const int PROFESSIONAL_EDITION = 8;
        
        }
        public virtual Saxon.Hej.trans.SimpleMode makeMode(Saxon.Hej.om.StructuredQName modeName, Saxon.Hej.trans.CompilerInfo compilerInfo) {
            return new Saxon.Hej.trans.SimpleMode(modeName);
        }
        public virtual Saxon.Hej.expr.instruct.TemplateRule makeTemplateRule() {
            return new Saxon.Hej.expr.instruct.TemplateRule();
        }
        public virtual Saxon.Hej.expr.XPathContextMajor.ThreadManager makeThreadManager() {
            return null;
        }
        public virtual Saxon.Hej.trans.CompilerInfo makeCompilerInfo() {
            return new Saxon.Hej.trans.CompilerInfo(this);
        }
        public Saxon.Hej.expr.parser.ICompilerService makeCompilerService(Saxon.Api.HostLanguage hostLanguage) {
            return null;
        }
        public void setLabel(string label) {
            this.label = label;
        }
        public string getLabel() {
            return label;
        }
    
        private class Inner_ActiveSource_440 : Saxon.Hej.lib.ActiveSource {
            private readonly Configuration outerInstance;
            public Inner_ActiveSource_440(Configuration outerInstance, Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.Configuration config) {
                this.outerInstance = outerInstance;
                this.source = source;
                this.config = config;
            }
            private Saxon.Ejavax.xml.transform.Source source;
            private Saxon.Hej.Configuration config;
            public void deliver(Saxon.Hej.@event.Receiver receiver, Saxon.Hej.lib.ParseOptions options) {
                /*localparam*/options = /*localparam*/options.merge(((Saxon.Hej.lib.AugmentedSource)(/*extraParam*/source)).getParseOptions());
                outerInstance.resolveSource(((Saxon.Hej.lib.AugmentedSource)(/*extraParam*/source)).getContainedSource(), /*extraParam*/config).deliver(/*localparam*/receiver, /*localparam*/options);
            }
            public void setSystemId(string systemId) {
                /*extraParam*/source.setSystemId(/*localparam*/systemId);
            }
            public string getSystemId() {
                return /*extraParam*/source.getSystemId();
            }
        }

    }
}