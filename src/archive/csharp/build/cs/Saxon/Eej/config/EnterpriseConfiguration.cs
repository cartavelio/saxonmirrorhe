using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.config {
    public sealed class EnterpriseConfiguration : Saxon.Eej.config.ProfessionalConfiguration {
        private int licensedFeatures = 0;
        private System.Collections.Generic.Dictionary<string,int> namespaceStatus = new System.Collections.Generic.Dictionary<string, int>();
        private int[] occurrenceLimits = {100, 250};
        private Saxon.Hej.lib.SchemaURIResolver schemaURIResolver = null;
        private readonly Saxon.Hej.lib.SchemaURIResolver standardSchemaResolver;
        private Saxon.Eej.ee.schema.PreparedSchema superSchema;
        private string debugByteCodeDirectory = "Saxon-bytecode";
        private int resultDocumentThreadsLimit = Saxon.Ejava.lang.Runtime.getRuntime().availableProcessors();
        private int thresholdForCompilingTypes = 100;
        private int thresholdForInliningFunctions = 100;
        private int maxCompiledClasses = 10000;
        private Saxon.Ejava.util.Optional<bool> streamabilityEnabled = Saxon.Ejava.util.Optional<bool>.empty();
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string,Saxon.Hej.regex.RegularExpression> regexCache = new System.Collections.Concurrent.ConcurrentDictionary<string, Saxon.Hej.regex.RegularExpression>();
        private long regexAttempts = 0;
        private Saxon.Eej.ee.config.MultithreadingFactory multithreadingFactory;
        public EnterpriseConfiguration() : base () {
            multithreadingFactory = new Saxon.Eej.ee.config.MultithreadingFactory(this);
            internalSetBooleanProperty(Saxon.Hej.lib.FeatureCode.ALLOW_MULTITHREADING, Saxon.Hej.lib.FeatureKeys.ALLOW_MULTITHREADING, true);
            internalSetBooleanProperty(Saxon.Hej.lib.FeatureCode.GENERATE_BYTE_CODE, Saxon.Hej.lib.FeatureKeys.GENERATE_BYTE_CODE, true);
            setStaticQueryContextFactory(new Inner_StaticQueryContextFactory_8(this));
            optimizerOptions = Saxon.Hej.expr.parser.OptimizerOptions.FULL_EE_OPTIMIZATION;
            defaultXsltCompilerInfo.setOptimizerOptions(optimizerOptions);
            standardSchemaResolver = new Saxon.Eej.ee.config.StandardSchemaResolver(this);
            superSchema = new Saxon.Eej.ee.schema.PreparedSchema(this);
        }
        protected internal override Saxon.Hej.query.StaticQueryContext makeStaticQueryContext(bool copyFromDefault) {
            Saxon.Hej.query.StaticQueryContext sqc = base.makeStaticQueryContext(copyFromDefault);
            if ((defaultStaticQueryContext != null)) {
                sqc.setSchemaAware(defaultStaticQueryContext.isSchemaAware());
            }
            return sqc;
        }
        public override string getEditionCode() {
            return "EE";
        }
        public override  /* REPLACING protected internal override */ void setFeature(Saxon.Hej.lib.FeatureData feature, System.Object value) {
            int code = feature.code;
            Saxon.Hej.lib.FeatureData featureData = Saxon.Hej.lib.FeatureIndex.getData(code);
            string name = featureData.uri;
            switch (code) {
              case Saxon.Hej.lib.FeatureCode.GENERATE_BYTE_CODE:
                {
                    if ((value is bool)) {
                        internalSetBooleanProperty(code, name, value);
                    } else {
                        if (((("true".Equals(value) || "on".Equals(value)) || "yes".Equals(value)) || "1".Equals(value))) {
                            internalSetBooleanProperty(code, name, true);
                            byteCodeThreshold = 0;
                        } else {
                            if (((("false".Equals(value) || "off".Equals(value)) || "no".Equals(value)) || "0".Equals(value))) {
                                internalSetBooleanProperty(code, name, false);
                            } else {
                                if ((value is int)) {
                                    internalSetBooleanProperty(code, name, true);
                                    byteCodeThreshold = (int)(value);
                                } else {
                                    internalSetBooleanProperty(code, name, true);
                                    try {
                                        byteCodeThreshold = Saxon.Impl.Helpers.Utils.ParseInt(value.ToString());
                                    } catch (Saxon.Ejava.lang.NumberFormatException e) {
                                        throw new Saxon.Ejava.lang.IllegalArgumentException((("Value of "+name)+" must be a boolean or an integer"));
                                    }
                                }
                            }
                        }
                    }
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.THRESHOLD_FOR_HOTSPOT_BYTE_CODE:
                {
                    int val = requireInteger(name, value);
                    if ((val<=0)) {
                        internalSetBooleanProperty(code, name, false);
                    } else {
                        internalSetBooleanProperty(code, name, true);
                        byteCodeThreshold = val;
                    }
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.DEBUG_BYTE_CODE_DIR:
                {
                    debugByteCodeDirectory = value.ToString();
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.OCCURRENCE_LIMITS:
                {
                    string val = value.ToString();
                    int comma = val.IndexOf(',');
                    if ((comma<1)) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException((("Value of "+name)+"must be two integers, comma-separated"));
                    }
                    try {
                        int min = Saxon.Impl.Helpers.Utils.ParseInt(val.Substring(0, comma - 0));
                        int max = Saxon.Impl.Helpers.Utils.ParseInt(val.Substring((comma+1)));
                        setOccurrenceLimits(min, max);
                    } catch (Saxon.Ejava.lang.NumberFormatException e) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException((("Value of "+name)+"must be two integers, comma-separated"));
                    }
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.RESULT_DOCUMENT_THREADS:
                {
                    resultDocumentThreadsLimit = requireInteger(name, value);
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.SCHEMA_URI_RESOLVER:
                {
                    if (!(((value is Saxon.Hej.lib.SchemaURIResolver)))) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException("SCHEMA_URI_RESOLVER value must be an instance of net.sf.saxon.lib.SchemaURIResolver");
                    }
                    setSchemaURIResolver((Saxon.Hej.lib.SchemaURIResolver)(value));
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.SCHEMA_URI_RESOLVER_CLASS:
                {
                    setSchemaURIResolver((Saxon.Hej.lib.SchemaURIResolver)(instantiateClassName(name, value, typeof(Saxon.Hej.lib.SchemaURIResolver))));
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.STREAMABILITY:
                {
                    string s = requireString(name, value);
                    switch (s) {
                      case "off":
                        setStreamabilityEnabled(false);
                        break ;
                      case "standard":
                        setStreamabilityEnabled(true);
                        break ;
                      case "extended":
                        throw new Saxon.Ejava.lang.IllegalArgumentException("The value STREAMABILITY = 'extended' is no longer recognized");
                      default:
                        throw new Saxon.Ejava.lang.IllegalArgumentException((("Unrecognized value STREAMABILITY = '"+value)+"': must be 'off' or 'standard'"));
                    }
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.THRESHOLD_FOR_COMPILING_TYPES:
                {
                    thresholdForCompilingTypes = requireInteger(name, value);
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.THRESHOLD_FOR_FUNCTION_INLINING:
                {
                    thresholdForInliningFunctions = requireInteger(name, value);
                    break ;
                }
              case Saxon.Hej.lib.FeatureCode.MAX_COMPILED_CLASSES:
                {
                    maxCompiledClasses = requireInteger(name, value);
                    break ;
                }
              default:
                {
                    base.setFeature(feature, value);
                    break ;
                }
            }
        }
        protected internal override System.Object getFeature(Saxon.Hej.lib.FeatureData feature) {
            int code = feature.code;
            if (booleanFeatures.contains(code)) {
                return base.getFeature(feature);
            }
            switch (code) {
              case Saxon.Hej.lib.FeatureCode.DEBUG_BYTE_CODE_DIR:
                return debugByteCodeDirectory;
              case Saxon.Hej.lib.FeatureCode.OCCURRENCE_LIMITS:
                return (((occurrenceLimits[0]+",")+occurrenceLimits[1]));
              case Saxon.Hej.lib.FeatureCode.THRESHOLD_FOR_COMPILING_TYPES:
                return thresholdForCompilingTypes;
              case Saxon.Hej.lib.FeatureCode.THRESHOLD_FOR_FUNCTION_INLINING:
                return thresholdForInliningFunctions;
              case Saxon.Hej.lib.FeatureCode.MAX_COMPILED_CLASSES:
                return maxCompiledClasses;
              case Saxon.Hej.lib.FeatureCode.RESULT_DOCUMENT_THREADS:
                return resultDocumentThreadsLimit;
              case Saxon.Hej.lib.FeatureCode.SCHEMA_URI_RESOLVER:
                return getSchemaURIResolver();
              case Saxon.Hej.lib.FeatureCode.SCHEMA_URI_RESOLVER_CLASS:
                return getSchemaURIResolver().GetType().Name;
              case Saxon.Hej.lib.FeatureCode.STREAMABILITY:
                return (isStreamabilityEnabled() ? "standard" : "off");
              default:
                return base.getFeature(feature);
            }
        }
        public override void checkLicensedFeature(int feature, string name, int localLicenseId) {
            if (((localLicenseId>=0) && isFeatureAllowedBySecondaryLicense(localLicenseId, feature))) {
                return ;
            }
            if (isLicenseNotFound()) {
                try {
                    forceLoadLicense();
                } catch (System.Exception err) {
                    int reason = ((err is Saxon.Hej.trans.LicenseException) ? ((Saxon.Hej.trans.LicenseException)(err)).getReason() : Saxon.Hej.trans.LicenseException.CANNOT_READ);
                    string msg = ((err.Message+". A license is needed to use ")+name);
                    throw new Saxon.Hej.trans.LicenseException(msg, reason);
                }
            }
            if (!(isLicenseFound())) {
                loadLicense();
            }
            reportIfMissingLicenseFeature(feature, name);
        }
        protected internal override void setLicensedFeatures() {
            licensedFeatures = Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION;
            if ("yes".Equals(getFeature("SAV"))) {
                licensedFeatures |= Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION;
            }
            if ("yes".Equals(getFeature("SAQ"))) {
                licensedFeatures |= Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY;
            }
            if ("yes".Equals(getFeature("SAT"))) {
                licensedFeatures |= Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT;
            }
        }
        protected internal override bool testFeature(int feature) {
            return (((licensedFeatures&feature)) != 0);
        }
        protected internal override void needEnterpriseEdition() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("This operation requires a valid Saxon-EE license file");
        }
        public override Saxon.Hej.style.StyleNodeFactory makeStyleNodeFactory(Saxon.Hej.style.Compilation compilation) {
            Saxon.Hej.expr.PackageData pack = compilation.getPackageData();
            if ((pack != null)) {
                int locLic = pack.getLocalLicenseId();
                int feature = Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION;
                if (!(((isFeatureAllowedBySecondaryLicense(locLic, feature) || isLicensedFeature(feature))))) {
                    return base.makeStyleNodeFactory(compilation);
                }
            }
            return new Saxon.Eej.ee.config.StyleNodeFactoryEE(this, compilation);
        }
        public override Saxon.Hej.style.StylesheetPackage makeStylesheetPackage() {
            return (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT) ? new Saxon.Eej.ee.trans.StylesheetPackageEE(this) : new Saxon.Hej.style.StylesheetPackage(this));
        }
        public override Saxon.Hej.expr.accum.AccumulatorRegistry makeAccumulatorRegistry() {
            return new Saxon.Eej.ee.stream.AccumulatorRegistryEE();
        }
        protected internal override Saxon.Hej.functions.registry.BuiltInFunctionSet makeVendorFunctionSet() {
            if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION)) {
                return new Saxon.Eej.ee.extfn.VendorFunctionSetEE(this);
            } else {
                return null;
            }
        }
        public override Saxon.Hej.expr.instruct.UserFunction newUserFunction(bool memoFunction, Saxon.Hej.trans.FunctionStreamability streamability) {
            if (memoFunction) {
                return new Saxon.Hej.expr.instruct.MemoFunction();
            } else {
                if ((streamability != Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED)) {
                    return new Saxon.Eej.ee.stream.StreamableUserFunction();
                } else {
                    return new Saxon.Hej.expr.instruct.UserFunction();
                }
            }
        }
        public override Saxon.Hej.type.TypeHierarchy getTypeHierarchy() {
            if ((typeHierarchy == null)) {
                typeHierarchy = new Saxon.Eej.ee.config.TypeHierarchyEE(this);
            }
            return typeHierarchy;
        }
        public void setSchemaURIResolver(Saxon.Hej.lib.SchemaURIResolver resolver) {
            schemaURIResolver = resolver;
            resolver.setConfiguration(this);
        }
        public override Saxon.Eej.ee.validate.InvalidityReportGeneratorEE createValidityReporter() {
            return new Saxon.Eej.ee.validate.InvalidityReportGeneratorEE(this);
        }
        public override Saxon.Eej.ee.trans.PackageLoaderEE makePackageLoader() {
            return new Saxon.Eej.ee.trans.PackageLoaderEE(this);
        }
        public Saxon.Eej.ee.schema.SchemaCompiler newSchemaCompiler() {
            Saxon.Eej.ee.schema.SchemaCompiler compiler = new Saxon.Eej.ee.schema.SchemaCompiler(this);
            compiler.setLanguageVersion(xsdVersion);
            return compiler;
        }
        public void setOccurrenceLimits(int minimum, int maximum) {
            if (((minimum<1) || (minimum>=maximum))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Invalid occurrence limits: min<1 or max<min");
            }
            occurrenceLimits = new int[]{minimum, maximum};
        }
        public int[] getOccurrenceLimits() {
            return occurrenceLimits;
        }
        public Saxon.Hej.lib.SchemaURIResolver getSchemaURIResolver() {
            Saxon.Hej.lib.SchemaURIResolver r = schemaURIResolver;
            if ((r == null)) {
                r = standardSchemaResolver;
            }
            return r;
        }
        public override string readSchema(Saxon.Hej.@event.PipelineConfiguration pipe, string baseURI, string schemaLocation, string expected) {
            lock (superSchema) {
                int status = getNamespaceStatus(expected);
                if (((status == Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNKNOWN) || (((status == Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_KNOWN) && getConfigurationProperty(Saxon.Hej.lib.Feature<bool>.MULTIPLE_SCHEMA_IMPORTS))))) {
                    Saxon.Eej.ee.schema.SchemaCompiler schemaCompiler = newSchemaCompiler();
                    if ((pipe.getErrorReporter() != null)) {
                        schemaCompiler.setErrorReporter(pipe.getErrorReporter());
                    }
                    string ns = schemaCompiler.readSchema(pipe, baseURI, schemaLocation, expected);
                    schemaCompiler.makeAllAutomata();
                    schemaCompiler.checkAllAutomata();
                    return ns;
                } else {
                    return expected;
                }
            }
        }
        public override void readMultipleSchemas(Saxon.Hej.@event.PipelineConfiguration pipe, string baseURI, System.Collections.Generic.IList<string> schemaLocations, string expected) {
            newSchemaCompiler().readMultipleSchemas(pipe, baseURI, schemaLocations, expected);
        }
        public override string readInlineSchema(Saxon.Hej.om.NodeInfo root, string expected, Saxon.Hej.lib.ErrorReporter errorReporter) {
            return newSchemaCompiler().readInlineSchema(root, expected, errorReporter);
        }
        public override void addSchemaSource(Saxon.Ejavax.xml.transform.Source schemaSource, Saxon.Hej.lib.ErrorReporter errorReporter) {
            addSchemaSource(schemaSource, errorReporter, newSchemaCompiler());
        }
        public void addSchemaSource(Saxon.Ejavax.xml.transform.Source schemaSource, Saxon.Hej.lib.ErrorReporter errorReporter, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            try {
                if ((errorReporter != null)) {
                    compiler.setErrorReporter(errorReporter);
                }
                Saxon.Hej.@event.PipelineConfiguration pipe = makePipelineConfiguration();
                pipe.setErrorReporter(((errorReporter == null) ? makeErrorReporter() : errorReporter));
                pipe.setHostLanguage(Saxon.Api.HostLanguage.XML_SCHEMA);
                Saxon.Eej.ee.schema.sdoc.SchemaDocument schemadoc = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.read(schemaSource, compiler, pipe, null);
                if (schemadoc.isCompiledSchemaDocument()) {
                    try {
                        importComponents(schemadoc.getDocument().getRootNode().asActiveSource());
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.type.SchemaException(e);
                    }
                } else {
                    compiler.validate(schemadoc);
                    setNamespaceStatus(schemadoc.getTargetNamespace(), Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_KNOWN);
                }
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                Saxon.Hej.trans.XPathException xe = e.getXPathException();
                if ((xe is Saxon.Hej.type.SchemaException)) {
                    throw (Saxon.Hej.type.SchemaException)(xe);
                } else {
                    throw new Saxon.Hej.type.SchemaException(xe);
                }
            }
        }
        public void addSchema(Saxon.Eej.ee.schema.PreparedSchema pschema) { lock(this) {
            checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION, "schema processing", pschema.getLocalLicenseId());
            if ((pschema == superSchema)) {
                return ;
            }
            if ((pschema is Saxon.Eej.ee.schema.SingleNamespaceSchema)) {
                string targetNamespace = ((Saxon.Eej.ee.schema.SingleNamespaceSchema)(pschema)).getTargetNamespace();
                setNamespaceStatus(targetNamespace, Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_KNOWN);
            } else {
                foreach (string ns in pschema.getTargetNamespaces()) {
                    setNamespaceStatus(ns, Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_KNOWN);
                }
            }
            pschema.copyTo(superSchema, newSchemaCompiler());
            superSchema.setLocalLicenseId(pschema.getLocalLicenseId());
        }}
        public void addSurrogateSchema(string @namespace) { lock(this) {
            checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION, "schema processing", -1);
            setNamespaceStatus(@namespace, Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNDER_CONSTRUCTION);
        }}
        public override void addSchemaForBuiltInNamespace(string @namespace) {
            if ((getNamespaceStatus(@namespace) != Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNKNOWN)) {
                return ;
            }
            switch (@namespace) {
              case Saxon.Hej.lib.NamespaceConstant.XML:
                addSchemaForXmlNamespace();
                break ;
              case Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE:
                addSchemaForXsiNamespace();
                break ;
              case Saxon.Hej.lib.NamespaceConstant.FN:
                System.IO.Stream @in = Saxon.Hej.Version.platform.locateResource("xpath-functions.scm", new System.Collections.Generic.List<string>());
                if (isTiming()) {
                    getLogger().info(("Loading schema from resources for: "+Saxon.Hej.lib.NamespaceConstant.FN));
                }
                try {
                    addSchemaSource(new Saxon.Ejavax.xml.transform.stream.StreamSource(@in), makeErrorReporter());
                } catch (Saxon.Hej.type.SchemaException e) {
                    throw new Saxon.Ejava.lang.IllegalStateException("Failed to load internal copy of xpath-functions.scm");
                }
                break ;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(@namespace);
            }
        }
        private void addSchemaForXmlNamespace() { lock(this) {
            if ((getNamespaceStatus(Saxon.Hej.lib.NamespaceConstant.XML) == Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNKNOWN)) {
                try {
                    addSchema(/*134*/Saxon.Eej.ee.schema.sdoc.XMLNamespaceSchema.buildSchema(this));
                } catch (Saxon.Hej.type.SchemaException err) {
                    throw new Saxon.Ejava.lang.AssertionError(err);
                }
                setNamespaceStatus(Saxon.Hej.lib.NamespaceConstant.XML, Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_KNOWN);
            }
        }}
        private void addSchemaForXsiNamespace() { lock(this) {
            if ((getNamespaceStatus(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE) == Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNKNOWN)) {
                try {
                    addSchema(/*134*/Saxon.Eej.ee.schema.sdoc.XSINamespaceSchema.buildSchema(this));
                } catch (Saxon.Hej.type.SchemaException err) {
                    throw new Saxon.Ejava.lang.AssertionError(err);
                }
                setNamespaceStatus(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE, Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_KNOWN);
            }
        }}
        public int getNamespaceStatus(string @namespace) { lock(this) {
            return Saxon.Impl.Helpers.MapUtils.GetOrDefault(namespaceStatus, @namespace, Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNKNOWN);
        }}
        public void setNamespaceStatus(string @namespace, int status) { lock(this) {
            if ((status == Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNKNOWN)) {
                namespaceStatus.Remove(@namespace);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(namespaceStatus, @namespace, status);
            }
        }}
        public override bool isSchemaAvailable(string targetNamespace) {
            return (getNamespaceStatus(targetNamespace) != Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNKNOWN);
        }
        public void removeSchema(string @namespace) { lock(this) {
            namespaceStatus.Remove(@namespace);
        }}
        public override System.Collections.Generic.ISet<string> getImportedNamespaces() { lock(this) {
            return Saxon.Impl.Helpers.MapUtils.KeySet(namespaceStatus);
        }}
        public Saxon.Eej.ee.schema.PreparedSchema getSuperSchema() {
            return superSchema;
        }
        public override void sealNamespace(string @namespace) {
            if (!(@namespace.Equals(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS))) {
                setNamespaceStatus(@namespace, Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_SEALED);
            }
        }
        public bool isSealedNamespace(string @namespace) {
            return (getNamespaceStatus(@namespace) == Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_SEALED);
        }
        public override System.Collections.Generic.ICollection<Saxon.Hej.expr.instruct.GlobalParam> getDeclaredSchemaParameters() {
            return getSuperSchema().getGlobalParams();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.type.SchemaType> getExtensionsOfType(Saxon.Hej.type.SchemaType type) {
            if ((type is Saxon.Eej.ee.schema.UserDefinedType)) {
                return ((Saxon.Eej.ee.schema.UserDefinedType)(type)).getExtensionTypes();
            } else {
                return getSuperSchema().getExtensionsOfBuiltInType(type.getFingerprint());
            }
        }
        public override void importComponents(Saxon.Ejavax.xml.transform.Source source) {
            try {
                Saxon.Eej.ee.schema.SchemaModelLoader loader = new Saxon.Eej.ee.schema.SchemaModelLoader(this);
                Saxon.Eej.ee.schema.PreparedSchema schema = loader.load(source);
                addSchema(schema);
                schema.copyTo(superSchema, newSchemaCompiler());
                foreach (string ns in schema.getTargetNamespaces()) {
                    setNamespaceStatus(ns, Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_KNOWN);
                }
            } catch (Saxon.Hej.type.SchemaException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        public override void exportComponents(Saxon.Hej.@event.Receiver @out) {
            Saxon.Eej.ee.schema.SchemaModelSerializer serializer = new Saxon.Eej.ee.schema.SchemaModelSerializer(this, @out);
            serializer.serialize();
        }
        public override void clearSchemaCache() {
            superSchema = new Saxon.Eej.ee.schema.PreparedSchema(this);
            namespaceStatus = new System.Collections.Generic.Dictionary<string, int>();
        }
        public override Saxon.Hej.type.SchemaDeclaration getElementDeclaration(int fingerprint) {
            return superSchema.getElementDecl(fingerprint);
        }
        public override Saxon.Hej.type.SchemaDeclaration getElementDeclaration(Saxon.Hej.om.StructuredQName qName) {
            return superSchema.getElementDecl(qName);
        }
        public override Saxon.Hej.type.SchemaDeclaration getAttributeDeclaration(int fingerprint) {
            return superSchema.getAttributeDecl(fingerprint);
        }
        public override Saxon.Hej.type.SchemaDeclaration getAttributeDeclaration(Saxon.Hej.om.StructuredQName attributeName) {
            return superSchema.getAttributeDecl(attributeName);
        }
        public override Saxon.Hej.type.SchemaType getSchemaType(Saxon.Hej.om.StructuredQName name) {
            if (name.hasURI(Saxon.Hej.lib.NamespaceConstant.SCHEMA)) {
                Saxon.Hej.type.SchemaType t = Saxon.Hej.type.BuiltInType.getSchemaTypeByLocalName(name.getLocalPart());
                if ((t != null)) {
                    return t;
                }
            }
            return superSchema.getType(name);
        }
        public override Saxon.Eej.ee.schema.UserUnionType makeUserUnionType(System.Collections.Generic.IList<Saxon.Hej.type.AtomicType> memberTypes) {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.TypeReference> memberRefs = new System.Collections.Generic.List<Saxon.Eej.ee.schema.TypeReference>(memberTypes.Count);
            foreach (Saxon.Hej.type.AtomicType at in memberTypes) {
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(at.getFingerprint(), this, Saxon.Hej.expr.parser.Loc.NONE);
                memberRefs.Add(@ref);
            }
            Saxon.Eej.ee.schema.UserUnionType uut = new Saxon.Eej.ee.schema.UserUnionType(this);
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            string sep = "";
            foreach (Saxon.Hej.type.AtomicType at in memberTypes) {
                fsb.append(sep);
                sep = "-";
                fsb.append(at.getName());
            }
            Saxon.Hej.om.StructuredQName name = new Saxon.Hej.om.StructuredQName("anon", Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, fsb.ToString());
            uut.setTypeName(name, getNamePool().allocateFingerprint(name.getURI(), name.getLocalPart()));
            uut.setMemberTypeReferences(memberRefs);
            return uut;
        }
        public override bool isDeclaredNotation(string uri, string local) {
            return (superSchema.getNotation(new Saxon.Hej.om.StructuredQName("", uri, local)) != null);
        }
        public override void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType derived, Saxon.Hej.type.SchemaType @base, int block) {
            if (derived.isSimpleType()) {
                string s = /*134*/Saxon.Eej.ee.schema.UserSimpleType.isTypeDerivationOK((Saxon.Hej.type.SimpleType)(derived), @base, block);
                if ((s != null)) {
                    throw new Saxon.Hej.type.SchemaException(s);
                }
            } else {
                /*134*/Saxon.Eej.ee.schema.UserComplexType.checkTypeDerivation((Saxon.Hej.type.ComplexType)(derived), @base, block);
            }
        }
        public override void prepareValidationReporting(Saxon.Hej.expr.XPathContext context, Saxon.Hej.lib.ParseOptions options) {
        }
        public override Saxon.Hej.@event.Receiver getDocumentValidator(Saxon.Hej.@event.Receiver receiver, string systemId, Saxon.Hej.lib.ParseOptions validationOptions, Saxon.Hej.s9api.Location initiatingLocation) {
            int validationMode = validationOptions.getSchemaValidationMode();
            Saxon.Hej.om.SpaceStrippingRule stripRules = validationOptions.getSpaceStrippingRule();
            Saxon.Hej.type.SchemaType schemaType = validationOptions.getTopLevelType();
            Saxon.Hej.om.StructuredQName topLevelElementName = validationOptions.getTopLevelElement();
            Saxon.Hej.type.ValidationParams @params = validationOptions.getValidationParams();
            Saxon.Hej.@event.PipelineConfiguration pipe = receiver.getPipelineConfiguration();
            if ((((validationMode == Saxon.Hej.lib.Validation.STRICT) || (validationMode == Saxon.Hej.lib.Validation.LAX)) || (schemaType != null))) {
                Saxon.Hej.@event.Receiver destination = receiver;
                if ((stripRules == null)) {
                    stripRules = getParseOptions().getSpaceStrippingRule();
                }
                if ((stripRules == Saxon.Hej.om.IgnorableSpaceStrippingRule.getInstance())) {
                    destination = stripRules.makeStripper(destination);
                }
                Saxon.Eej.ee.validate.ValidationContext vc = new Saxon.Eej.ee.validate.ValidationContext(getConversionRules());
                vc.setInitiatingLocation(initiatingLocation);
                vc.setErrorLimit(validationOptions.getValidationErrorLimit());
                if ((pipe.getController() == null)) {
                    pipe.setController(new Saxon.Hej.Controller(this));
                    pipe.getController().getExecutable().setSchemaAware(true);
                }
                vc.setController(pipe.getController());
                vc.setValidationParams(@params);
                if (validationOptions.isCheckEntityReferences()) {
                    Saxon.Eej.ee.validate.EntityValidator ev = new Saxon.Eej.ee.validate.EntityValidator(destination);
                    ev.setValidationContext(vc);
                    destination = ev;
                }
                allocateInvalidityHandler(validationOptions, vc);
                Saxon.Eej.ee.validate.IdValidator idval = new Saxon.Eej.ee.validate.IdValidator(destination);
                idval.setValidationContext(vc);
                Saxon.Eej.ee.validate.ConstraintChecker checker = new Saxon.Eej.ee.validate.ConstraintChecker(pipe);
                checker.setValidationContext(vc);
                destination = new Saxon.Hej.@event.TeeOutputter(idval, checker);
                destination = allocateAssertionTreeBuilder(destination, pipe, vc);
                Saxon.Eej.ee.validate.ValidationStack val = new Saxon.Eej.ee.validate.ValidationStack(destination, validationMode, schemaType);
                val.setPipelineConfiguration(pipe);
                val.setValidationContext(vc);
                vc.setConstraintChecker(checker);
                if ((topLevelElementName != null)) {
                    val.setTopLevelElement(new Saxon.Hej.om.FingerprintedQName(topLevelElementName.getPrefix(), topLevelElementName.getURI(), topLevelElementName.getLocalPart()));
                }
                Saxon.Api.HostLanguage hostLanguage = pipe.getHostLanguage();
                if ((hostLanguage == Saxon.Api.HostLanguage.XSLT)) {
                    if ((schemaType != null)) {
                        vc.setErrorCode("XTTE1540");
                    } else {
                        if ((validationMode == Saxon.Hej.lib.Validation.STRICT)) {
                            vc.setErrorCode("XTTE1510");
                        } else {
                            vc.setErrorCode("XTTE1515");
                        }
                    }
                } else {
                    if ((hostLanguage == Saxon.Api.HostLanguage.XQUERY)) {
                        vc.setErrorCode("XQDY0027");
                    } else {
                        if ((hostLanguage == Saxon.Api.HostLanguage.XML_SCHEMA)) {
                            vc.setErrorCode(null);
                        }
                    }
                }
                Saxon.Eej.ee.validate.XSIAttributeHandler xsi = new Saxon.Eej.ee.validate.XSIAttributeHandler(val);
                xsi.setSystemId(systemId);
                Saxon.Eej.ee.validate.AttributeInheritor stb = new Saxon.Eej.ee.validate.AttributeInheritor(xsi);
                vc.setAttributeInheritor(stb);
                xsi.setValidationContext(vc);
                Saxon.Hej.@event.PathMaintainer pathMaintainer = new Saxon.Hej.@event.PathMaintainer(stb);
                pipe.setComponent("net.sf.saxon.event.PathMaintainer", pathMaintainer);
                Saxon.Hej.@event.DocumentValidator dv = new Saxon.Hej.@event.DocumentValidator(pathMaintainer, ((hostLanguage == Saxon.Api.HostLanguage.XSLT) ? "XTTE1550" : "XQDY0061"));
                Saxon.Hej.@event.LocationCopier copier = new Saxon.Hej.@event.LocationCopier(true, systemId);
                pipe.setCopyInformee((element) => copier.notifyElementNode(element));
                vc.startValidation(systemId);
                return dv;
            } else {
                if ((validationMode == Saxon.Hej.lib.Validation.STRIP)) {
                    Saxon.Eej.ee.validate.SkipValidator a = new Saxon.Eej.ee.validate.SkipValidator(receiver);
                    Saxon.Eej.ee.validate.ValidationContext vc = new Saxon.Eej.ee.validate.ValidationContext(getConversionRules());
                    a.setValidationContext(vc);
                    a.setSystemId(systemId);
                    return a;
                } else {
                    return receiver;
                }
            }
        }
        private void allocateInvalidityHandler(Saxon.Hej.lib.ParseOptions validationOptions, Saxon.Eej.ee.validate.ValidationContext vc) {
            if ((validationOptions.getInvalidityHandler() == null)) {
                Saxon.Hej.lib.ErrorReporter reporter = validationOptions.getErrorReporter();
                if ((reporter == null)) {
                    Saxon.Hej.Controller controller = vc.getController();
                    if ((controller != null)) {
                        reporter = controller.getErrorReporter();
                    } else {
                        reporter = makeErrorReporter();
                    }
                }
                vc.setInvalidityHandler(new Saxon.Hej.lib.InvalidityHandlerWrappingErrorReporter(reporter));
            } else {
                vc.setInvalidityHandler(validationOptions.getInvalidityHandler());
            }
        }
        public override Saxon.Hej.@event.Receiver getElementValidator(Saxon.Hej.@event.Receiver receiver, Saxon.Hej.lib.ParseOptions options, Saxon.Hej.s9api.Location location) {
            int validation = options.getSchemaValidationMode();
            Saxon.Hej.type.SchemaType schemaType = options.getTopLevelType();
            Saxon.Hej.om.StructuredQName elemName = options.getTopLevelElement();
            if ((validation == Saxon.Hej.lib.Validation.STRIP)) {
                if (receiver.usesTypeAnnotations()) {
                    return new Saxon.Eej.ee.validate.SkipValidator(receiver);
                } else {
                    return receiver;
                }
            }
            Saxon.Hej.@event.Receiver destination = receiver;
            Saxon.Hej.@event.PipelineConfiguration pipe = receiver.getPipelineConfiguration();
            pipe.setRecoverFromValidationErrors((isValidationWarnings() || options.isContinueAfterValidationErrors()));
            Saxon.Eej.ee.validate.ValidationContext vc = new Saxon.Eej.ee.validate.ValidationContext(getConversionRules());
            vc.setErrorLimit(options.getValidationErrorLimit());
            vc.setController(pipe.getController());
            vc.setValidationParams(options.getValidationParams());
            vc.setInitiatingLocation(location);
            allocateInvalidityHandler(options, vc);
            Saxon.Eej.ee.validate.ConstraintChecker checker = new Saxon.Eej.ee.validate.ConstraintChecker(pipe);
            checker.setValidationContext(vc);
            destination = new Saxon.Hej.@event.TeeOutputter(destination, checker);
            vc.setConstraintChecker(checker);
            destination = allocateAssertionTreeBuilder(destination, pipe, vc);
            Saxon.Eej.ee.validate.ContentValidator contentValidator = null;
            if ((validation == Saxon.Hej.lib.Validation.BY_TYPE)) {
                System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> failures = new System.Collections.Generic.List<Saxon.Hej.type.ValidationFailure>(1);
                contentValidator = /*134*/Saxon.Eej.ee.validate.ContentValidator.makeValidatorForType(null, schemaType, pipe, destination, failures);
                if (!(failures.Count==0)) {
                    throw failures[0].makeException();
                }
                System.Diagnostics.Debug.Assert((contentValidator != null));
                contentValidator.setContainingElement(options.getTopLevelElement(), location);
                contentValidator.setValidationContext(vc);
            } else {
                if ((validation != Saxon.Hej.lib.Validation.PRESERVE)) {
                    if ((elemName == null)) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException("When validating an element, either a type or an element name must be supplied");
                    }
                    Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(getElementDeclaration(elemName));
                    if (((decl == null) || !(decl.hasTypeAlternatives()))) {
                        System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> failures = new System.Collections.Generic.List<Saxon.Hej.type.ValidationFailure>(1);
                        contentValidator = /*134*/Saxon.Eej.ee.validate.ContentValidator.makeValidator(decl, elemName, location, vc, schemaType, validation, pipe, destination, failures);
                        if (!(failures.Count==0)) {
                            throw failures[0].makeException();
                        }
                    }
                }
            }
            Saxon.Eej.ee.validate.ValidationStack vs = new Saxon.Eej.ee.validate.ValidationStack(destination, Saxon.Hej.lib.Validation.STRICT, null);
            vs.setValidationContext(vc);
            vs.setPipelineConfiguration(pipe);
            Saxon.Api.HostLanguage hostLanguage = pipe.getHostLanguage();
            if ((hostLanguage == Saxon.Api.HostLanguage.XSLT)) {
                if ((validation == Saxon.Hej.lib.Validation.BY_TYPE)) {
                    vc.setErrorCode("XTTE1540");
                } else {
                    if ((validation == Saxon.Hej.lib.Validation.STRICT)) {
                        vc.setErrorCode("XTTE1510");
                    } else {
                        vc.setErrorCode("XTTE1515");
                    }
                }
            } else {
                if ((hostLanguage == Saxon.Api.HostLanguage.XQUERY)) {
                    vc.setErrorCode("XQDY0027");
                } else {
                    if ((hostLanguage == Saxon.Api.HostLanguage.XML_SCHEMA)) {
                        vc.setErrorCode(null);
                    }
                }
            }
            if ((contentValidator != null)) {
                vs.setInitialValidator(contentValidator);
            }
            Saxon.Eej.ee.validate.XSIAttributeHandler xsi = new Saxon.Eej.ee.validate.XSIAttributeHandler(vs);
            Saxon.Eej.ee.validate.AttributeInheritor inheritor = new Saxon.Eej.ee.validate.AttributeInheritor(xsi);
            vc.setAttributeInheritor(inheritor);
            xsi.setValidationContext(vc);
            destination = inheritor;
            vc.startValidation(location.getSystemId());
            return destination;
        }
        private Saxon.Hej.@event.Receiver allocateAssertionTreeBuilder(Saxon.Hej.@event.Receiver destination, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Eej.ee.validate.ValidationContext vc) {
            if ((xsdVersion == XSD11)) {
                bool retainComments = getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ASSERTIONS_CAN_SEE_COMMENTS);
                Saxon.Eej.ee.validate.AssertionTreeBuilder atb = new Saxon.Eej.ee.validate.AssertionTreeBuilder(destination, retainComments);
                atb.setPipelineConfiguration(pipe);
                vc.setAssertionTreeBuilder(atb);
                pipe.setComponent("com.saxonica.ee.validate.AssertionTreeBuilder", atb);
                destination = atb;
            }
            return destination;
        }
        public override Saxon.Hej.type.SimpleType validateAttribute(Saxon.Hej.om.StructuredQName nodeName, Saxon.Hej.str.UnicodeString value, int validation) {
            if (isLicenseNotFound()) {
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
            }
            Saxon.Eej.ee.schema.AttributeDecl decl = (Saxon.Eej.ee.schema.AttributeDecl)(getAttributeDeclaration(nodeName));
            if ((decl == null)) {
                if ((validation == Saxon.Hej.lib.Validation.STRICT)) {
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(("No global attribute declaration found for attribute "+nodeName.getDisplayName()));
                    ve.setErrorCode("XTTE1512");
                    throw ve.makeException();
                } else {
                    return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                }
            }
            Saxon.Hej.type.SimpleType type = decl.getSimpleType();
            if (type.isNamespaceSensitive()) {
                Saxon.Hej.type.ValidationFailure nsErr = new Saxon.Hej.type.ValidationFailure(("Cannot create a parentless attribute whose "+"type is namespace-sensitive (such as xs:QName)"));
                nsErr.setSchemaType(type);
                nsErr.setErrorCode("XTTE1545");
                throw nsErr.makeException();
            }
            Saxon.Hej.type.ValidationFailure contentErr = type.validateContent(value, Saxon.Hej.expr.instruct.DummyNamespaceResolver.getInstance(), getConversionRules());
            if ((contentErr != null)) {
                contentErr.setSchemaType(type);
                contentErr.setErrorCode(((validation == Saxon.Hej.lib.Validation.STRICT) ? "XTTE1510" : "XTTE1515"));
                throw contentErr.makeException();
            }
            return type;
        }
        public override Saxon.Hej.@event.Receiver getAnnotationStripper(Saxon.Hej.@event.Receiver destination) {
            return new Saxon.Eej.ee.validate.SkipValidator(destination);
        }
        public override Saxon.Hej.expr.parser.XPathParser newExpressionParser(string language, bool updating, int languageVersion) {
            if ((updating && !(isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("XQuery Update requires a Saxon-EE license");
            }
            if ("XQ".Equals(language)) {
                if (((((languageVersion == 40) || (languageVersion == 31)) || (languageVersion == 30)) || (languageVersion == 10))) {
                    Saxon.Hej.query.XQueryParser parser = new Saxon.Hej.query.XQueryParser(System.Math.Max(languageVersion, 31));
                    if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)) {
                        parser.setParserExtension(new Saxon.Eej.ee.update.XQueryParserExtensionEE(updating));
                    } else {
                        if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                            parser.setParserExtension(new Saxon.Eej.expr.XQueryParserExtensionPE());
                        }
                    }
                    return parser;
                } else {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown XQuery version "+languageVersion));
                }
            } else {
                Saxon.Hej.expr.parser.XPathParser parser = base.newExpressionParser(language, updating, languageVersion);
                if ((languageVersion>=30)) {
                    parser.setAccelerator(new Saxon.Eej.ee.optim.QuickXPathParser());
                }
                return parser;
            }
        }
        public override Saxon.Hej.trace.ExpressionPresenter newExpressionExporter(string target, System.IO.Stream destination, Saxon.Hej.style.StylesheetPackage rootPackage) {
            if (!(isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT))) {
                throw new Saxon.Hej.trans.XPathException("Stylesheet export requires a Saxon-EE license");
            }
            Saxon.Hej.trace.ExpressionPresenter presenter = new Saxon.Hej.trace.ExpressionPresenter();
            Saxon.Hej.trace.ExpressionPresenter.ExportOptions options = new Saxon.Hej.trace.ExpressionPresenter.ExportOptions();
            options.rootPackage = rootPackage;
            presenter.setOptions(options);
            switch (target) {
              case "JS":
                options.target = "JS";
                options.targetVersion = 2;
                Saxon.Hej.@event.PipelineConfiguration pipe = makePipelineConfiguration();
                Saxon.Hej.serialize.JSONEmitter writer = new Saxon.Hej.serialize.JSONEmitter(pipe, new Saxon.Hej.serialize.UTF8Writer(destination), new Saxon.Ejava.util.Properties());
                presenter.init(this, new Saxon.Eej.ee.trans.ExpressionPresenterToJson(writer, pipe), true);
                break ;
              default:
                options.target = target;
                options.targetVersion = 1;
                presenter.init(this, new Saxon.Ejavax.xml.transform.stream.StreamResult(destination), true);
                break ;
            }
            return presenter;
        }
        public override Saxon.Hej.expr.PendingUpdateList newPendingUpdateList() {
            return new Saxon.Eej.ee.update.PendingUpdateListImpl();
        }
        public override Saxon.Hej.lib.SchemaURIResolver makeSchemaURIResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            Saxon.Eej.ee.config.StandardSchemaResolver schemaResolver = new Saxon.Eej.ee.config.StandardSchemaResolver(this);
            schemaResolver.setResourceResolver(resolver);
            return schemaResolver;
        }
        public override Saxon.Hej.@event.FilterFactory makeDocumentProjector(Saxon.Hej.expr.parser.PathMap.PathMapRoot map) {
            checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY, "document projection", -1);
            return next => new Saxon.Eej.ee.stream.Projector(map, next);
        }
        public override Saxon.Hej.@event.FilterFactory makeDocumentProjector(Saxon.Hej.query.XQueryExpression exp) {
            if ((exp is Saxon.Eej.ee.optim.XQueryExpressionEE)) {
                Saxon.Hej.expr.parser.PathMap map = ((Saxon.Eej.ee.optim.XQueryExpressionEE)(exp)).getPathMap();
                Saxon.Hej.expr.parser.PathMap.PathMapRoot contextRoot = map.getContextDocumentRoot();
                if ((contextRoot != null)) {
                    if (!(contextRoot.hasUnknownDependencies())) {
                        return makeDocumentProjector(contextRoot);
                    }
                }
            }
            return null;
        }
        public override System.Func<Saxon.Hej.om.SequenceIterator,Saxon.Hej.om.FocusTrackingIterator> getFocusTrackerFactory(Saxon.Hej.expr.instruct.Executable exec, bool multithreaded) {
            if ((multithreaded || (((getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_MULTITHREADING) && exec.createsSecondaryResult()) && isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT))))) {
                return (iter) => new Saxon.Eej.ee.parallel.MultithreadedFocusTrackingIterator(iter);
            } else {
                return (iter) => new Saxon.Hej.om.FocusTrackingIterator(iter);
            }
        }
        public void setStreamabilityEnabled(bool enabled) {
            this.streamabilityEnabled = Saxon.Ejava.util.Optional<bool>.of(enabled);
        }
        public override bool isStreamabilityEnabled() {
            if (!(streamabilityEnabled.isPresent())) {
                streamabilityEnabled = Saxon.Ejava.util.Optional<bool>.of((isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT) || isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)));
            }
            return streamabilityEnabled.get();
        }
        public override Saxon.Hej.@event.Receiver makeStreamingTransformer(Saxon.Hej.trans.Mode mode, Saxon.Hej.expr.instruct.ParameterSet ordinaryParams, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context) {
            checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT, "streaming", -1);
            Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
            Saxon.Eej.ee.stream.AccumulatorRegistryEE am = (Saxon.Eej.ee.stream.AccumulatorRegistryEE)(((Saxon.Hej.PreparedStylesheet)(context.getController().getExecutable())).getTopLevelPackage().getAccumulatorRegistry());
            System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators = null;
            if ((mode is Saxon.Eej.ee.trans.ModeEE)) {
                System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> modeAccs = ((Saxon.Eej.ee.trans.ModeEE)(mode)).getAccumulators();
                if ((modeAccs != null)) {
                    accumulators = modeAccs;
                }
            }
            if ((accumulators == null)) {
                accumulators = new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
            }
            Saxon.Eej.ee.stream.watch.WatchManager wm = new Saxon.Eej.ee.stream.watch.WatchManager(pipe);
            wm.setXPathContext(context);
            wm.setOutputter(@out);
            if ((am != null)) {
                /*134*/Saxon.Eej.ee.stream.AccumulatorRegistryEE.registerSelectedAccumulators(wm, accumulators);
            }
            Saxon.Eej.ee.stream.feed.DecomposingFeed feed = new Saxon.Eej.ee.stream.feed.DecomposingFeed(@out, context);
            Saxon.Eej.ee.stream.watch.ApplyTemplatesAction docWatch = new Saxon.Eej.ee.stream.watch.ApplyTemplatesAction(null, feed, context);
            docWatch.setMode(mode.getDeclaringComponent());
            docWatch.setWatchManager(wm);
            docWatch.setActualParams(ordinaryParams);
            docWatch.setTunnelParams(tunnelParams);
            Saxon.Eej.ee.stream.watch.Watch watch = new Saxon.Eej.ee.stream.watch.Trigger(new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.NodeKindTest.DOCUMENT), docWatch, context);
            wm.addWatch(watch, true);
            return new Saxon.Eej.ee.stream.ContentDetector(wm);
        }
        public override Saxon.Hej.expr.Expression makeStreamInstruction(Saxon.Hej.expr.Expression hrefExp, Saxon.Hej.expr.Expression body, bool streaming, Saxon.Hej.lib.ParseOptions options, Saxon.Hej.expr.PackageData packageData, Saxon.Hej.s9api.Location location, Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            if ((streaming && isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT))) {
                Saxon.Eej.ee.stream.StreamInstr si = new Saxon.Eej.ee.stream.StreamInstr(hrefExp, body, options);
                si.setLocation(location);
                si.setRetainedStaticContext(rsc);
                if (getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STRICT_STREAMABILITY)) {
                    System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
                    si.checkStreamability(reasons, isStreamabilityEnabled(), true);
                    Saxon.Hej.expr.parser.ExpressionTool.clearStreamabilityData(si);
                }
                if ((packageData != null)) {
                    return si.prepareForStreaming(packageData);
                }
                return si;
            } else {
                return base.makeStreamInstruction(hrefExp, body, streaming, options, packageData, location, rsc);
            }
        }
        public override void checkStrictStreamability(Saxon.Hej.style.XSLTemplate template, Saxon.Hej.expr.Expression body) {
            Saxon.Eej.ee.trans.ContextItemStaticInfoEE info = (Saxon.Eej.ee.trans.ContextItemStaticInfoEE)(makeContextItemStaticInfo(template.getMatch().getItemType(), false));
            info.setContextPostureStriding();
            System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
            Saxon.Eej.ee.stream.PostureAndSweep s = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(body, info, reasons);
            if ((s.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder("Template rule is not guaranteed-streamable");
                foreach (string r in reasons) {
                    message.append("\n  *  ").append(r);
                }
                template.compileError(message.ToString(), "XTSE3430");
            }
            Saxon.Hej.expr.parser.ExpressionTool.clearStreamabilityData(body);
        }
        public override bool isStreamedNode(Saxon.Hej.om.NodeInfo node) {
            return (node is Saxon.Eej.ee.stream.om.FleetingNode);
        }
        public override Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions() {
            return optimizerOptions.intersect(Saxon.Hej.expr.parser.OptimizerOptions.FULL_EE_OPTIMIZATION);
        }
        public override Saxon.Hej.expr.parser.Optimizer obtainOptimizer() {
            if (!(isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION))) {
                return base.obtainOptimizer();
            }
            if ((optimizer == null)) {
                optimizer = new Saxon.Eej.ee.optim.OptimizerEE(this);
                optimizer.setOptimizerOptions(optimizerOptions.intersect(Saxon.Hej.expr.parser.OptimizerOptions.FULL_EE_OPTIMIZATION));
            }
            return optimizer;
        }
        public override Saxon.Hej.expr.parser.Optimizer obtainOptimizer(Saxon.Hej.expr.parser.OptimizerOptions options) {
            if (!(isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION))) {
                return base.obtainOptimizer(options);
            }
            Saxon.Hej.expr.parser.Optimizer optimizer = new Saxon.Eej.ee.optim.OptimizerEE(this);
            optimizer.setOptimizerOptions(options.intersect(Saxon.Hej.expr.parser.OptimizerOptions.FULL_EE_OPTIMIZATION));
            return optimizer;
        }
        public override bool isGenerateByteCode(Saxon.Api.HostLanguage hostLanguage) {
            return (getBooleanProperty(Saxon.Hej.lib.Feature<bool>.GENERATE_BYTE_CODE) && isLicensedFeature(((hostLanguage == Saxon.Api.HostLanguage.XSLT) ? Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT : Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)));
        }
        public override bool isDeferredByteCode(Saxon.Api.HostLanguage hostLanguage) {
            return ((byteCodeThreshold>0) && isLicensedFeature(((hostLanguage == Saxon.Api.HostLanguage.XSLT) ? Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT : Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)));
        }
        public override bool isJITEnabled() {
            return (optimizerOptions.isSet(Saxon.Hej.expr.parser.OptimizerOptions.JIT) && isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT));
        }
        public override Saxon.Hej.query.XQueryExpression makeXQueryExpression(Saxon.Hej.expr.Expression exp, Saxon.Hej.query.QueryModule mainModule, bool streaming) {
            if (!(isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY))) {
                return base.makeXQueryExpression(exp, mainModule, streaming);
            } else {
                Saxon.Eej.ee.optim.XQueryExpressionEE xqe = new Saxon.Eej.ee.optim.XQueryExpressionEE(exp, mainModule, streaming);
                if ((mainModule.getCodeInjector() != null)) {
                    mainModule.getCodeInjector().process(xqe);
                }
                return xqe;
            }
        }
        public override Saxon.Hej.regex.RegularExpression compileRegularExpression(Saxon.Hej.str.UnicodeString regex, string flags, string hostLanguage, System.Collections.Generic.IList<string> warnings) {
            if (optimizerOptions.isSet(Saxon.Hej.expr.parser.OptimizerOptions.REGEX_CACHE)) {
                regexAttempts++;
                string key = (((hostLanguage+flags)+'=')+regex.ToString());
                Saxon.Hej.regex.RegularExpression re = Saxon.Impl.Helpers.MapUtils.Get(regexCache, key);
                if ((re == null)) {
                    re = base.compileRegularExpression(regex, flags, hostLanguage, warnings);
                    Saxon.Impl.Helpers.MapUtils.Put(regexCache, key, re);
                    if (((regexCache.Count == 1000) && (regexAttempts<5000))) {
                        optimizerOptions = new Saxon.Hej.expr.parser.OptimizerOptions((optimizerOptions.getOptions()&~(Saxon.Hej.expr.parser.OptimizerOptions.REGEX_CACHE)));
                        regexCache.Clear();
                    }
                }
                return re;
            } else {
                return base.compileRegularExpression(regex, flags, hostLanguage, warnings);
            }
        }
        public override Saxon.Hej.expr.instruct.SlotManager makeSlotManager() {
            if ((getDebugger() != null)) {
                return base.makeSlotManager();
            } else {
                if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION)) {
                    return new Saxon.Eej.ee.config.SlotManagerEE();
                } else {
                    return new Saxon.Hej.expr.instruct.SlotManager();
                }
            }
        }
        public int getResultDocumentThreadsLimit() {
            return resultDocumentThreadsLimit;
        }
        public Saxon.Eej.ee.config.MultithreadingFactory getMultithreadingFactory() {
            return this.multithreadingFactory;
        }
        public void setMultithreadingFactory(Saxon.Eej.ee.config.MultithreadingFactory factory) {
            this.multithreadingFactory = factory;
        }
        public override Saxon.Hej.om.SequenceIterator getMultithreadedItemMappingIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.ItemMappingFunction action) {
            if ((isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION) && getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_MULTITHREADING))) {
                return multithreadingFactory.makeMultithreadedItemMappingIterator(@base, action);
            }
            return base.getMultithreadedItemMappingIterator(@base, action);
        }
        public override Saxon.Hej.om.Sequence makeClosure(Saxon.Hej.expr.Expression expression, int @ref, Saxon.Hej.expr.XPathContext context) {
            if ((@ref == Saxon.Hej.expr.FilterExpression.FILTERED)) {
                return /*134*/Saxon.Eej.ee.optim.SearchableValue.makeSearchableValue(expression.iterate(context));
            } else {
                return base.makeClosure(expression, @ref, context);
            }
        }
        public override Saxon.Hej.om.GroundedValue makeSequenceExtent(Saxon.Hej.expr.Expression expression, int @ref, Saxon.Hej.expr.XPathContext context) {
            if ((@ref == Saxon.Hej.expr.FilterExpression.FILTERED)) {
                return /*134*/Saxon.Eej.ee.optim.SearchableValue.makeSearchableValue(expression.iterate(context));
            } else {
                return Saxon.Hej.om.SequenceTool.toGroundedValue(expression.iterate(context));
            }
        }
        public override void declareBuiltInExtensionElementNamespaces() {
            try {
                setExtensionElementNamespace(Saxon.Hej.lib.NamespaceConstant.SAXON, new Saxon.Eej.ee.trans.SaxonElementFactoryEE());
                setExtensionElementNamespace(Saxon.Hej.lib.NamespaceConstant.SAXON6, new Saxon.Eej.xsltextn.style.Saxon6ElementFactory());
                setExtensionElementNamespace("http://saxonica.com/ns/interactiveXSLT", new Saxon.Eej.ee.extfn.js.IXSLElementFactory());
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.IllegalStateException(e.Message);
            }
        }
        public override Saxon.Hej.expr.parser.ContextItemStaticInfo makeContextItemStaticInfo(Saxon.Hej.type.ItemType itemType, bool maybeUndefined) {
            if ((isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT) || isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY))) {
                Saxon.Eej.ee.trans.ContextItemStaticInfoEE cit = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(itemType, maybeUndefined);
                cit.setStrictStreamabilityRules(getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STRICT_STREAMABILITY));
                return cit;
            } else {
                return base.makeContextItemStaticInfo(itemType, maybeUndefined);
            }
        }
        public override Saxon.Hej.expr.parser.ContextItemStaticInfo getDefaultContextItemStaticInfo() {
            return Saxon.Eej.ee.trans.ContextItemStaticInfoEE.DEFAULT;
        }
        public override Saxon.Hej.trans.SimpleMode makeMode(Saxon.Hej.om.StructuredQName modeName, Saxon.Hej.trans.CompilerInfo compilerInfo) {
            if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT)) {
                return new Saxon.Eej.ee.trans.ModeEE(modeName, compilerInfo);
            } else {
                return new Saxon.Eej.trans.ModePE(modeName);
            }
        }
        public override Saxon.Eej.ee.trans.TemplateRuleEE makeTemplateRule() {
            return new Saxon.Eej.ee.trans.TemplateRuleEE();
        }
        public override Saxon.Hej.expr.XPathContextMajor.ThreadManager makeThreadManager() {
            if (getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_MULTITHREADING)) {
                return multithreadingFactory.makeThreadManager();
            } else {
                return null;
            }
        }
        public override Saxon.Hej.trans.CompilerInfo makeCompilerInfo() {
            return base.makeCompilerInfo();
        }
    
        private class Inner_StaticQueryContextFactory_8 : Saxon.Hej.lib.StaticQueryContextFactory {
            private readonly EnterpriseConfiguration outerInstance;
            public Inner_StaticQueryContextFactory_8(EnterpriseConfiguration outerInstance) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.query.StaticQueryContext newStaticQueryContext(Saxon.Hej.Configuration config, bool copyFromDefault) {
                return new Saxon.Eej.ee.optim.StaticQueryContextEE(this.outerInstance, /*localparam*/copyFromDefault);
            }
        }

    }
}