using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.config {
    public class ProfessionalConfiguration : Saxon.Hej.Configuration {
        private System.Collections.Generic.Dictionary<string,Saxon.Hej.functions.FunctionLibrary> extensionBinders;
        private bool licenseFound = false;
        private bool licenseNotFound = false;
        private Saxon.Eej.config.Verifier verifier;
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.NamespaceUri,Saxon.Eej.xsltextn.ExtensionElementCreator> xsltExtensionElementNamespaceMap;
        public ProfessionalConfiguration() : base () {
            verifier = makeVerifier();
            declareBuiltInExtensionElementNamespaces();
            declareBuiltInExtensionFunctions();
            Saxon.Hej.Version.platform.registerAllBuiltInObjectModels(this);
            setSerializerFactory(new Saxon.Eej.serialize.SerializerFactoryPE(this));
            setStaticQueryContextFactory(new Inner_StaticQueryContextFactory_4(this));
            internalSetBooleanProperty(Saxon.Hej.lib.FeatureCode.DISABLE_XSL_EVALUATE, Saxon.Hej.lib.FeatureKeys.DISABLE_XSL_EVALUATE, false);
        }
        private Saxon.Eej.config.Verifier makeVerifier() {
            return new Saxon.Impl.Overrides.Verifier();
        }
        protected internal override Saxon.Hej.trans.ConfigurationReader makeConfigurationReader() {
            return new Saxon.Eej.config.pe.ConfigurationReaderPE();
        }
        public static Saxon.Hej.Configuration makeProfessionalConfiguration(Saxon.Ejava.lang.ClassLoader classLoader, string className) {
            if ((className == null)) {
                className = "com.saxonica.config.ProfessionalConfiguration";
            }
            try {
                return instantiateConfiguration(className, classLoader);
            } catch (System.Exception e) {
                throw new System.Exception(e.Message, e);
            }
        }
        public override string getEditionCode() {
            return "PE";
        }
        public override Saxon.Hej.type.TypeHierarchy getTypeHierarchy() {
            if ((typeHierarchy == null)) {
                typeHierarchy = new Saxon.Hej.type.TypeHierarchy(this);
            }
            return typeHierarchy;
        }
        public override Saxon.Eej.trans.PackageLoaderPE makePackageLoader() {
            return new Saxon.Eej.trans.PackageLoaderPE(this);
        }
        public override void setConfigurationProperty(string name, System.Object value) {
            if (name.Equals("http://saxonica.com/oem-data")) {
                supplyLicenseKey(new System.IO.StringReader((string)(value)));
            } else {
                if (name.Equals(Saxon.Hej.lib.Feature<string>.LICENSE_FILE_LOCATION.name)) {
                    lock (verifier) {
                        verifier.loadLicense((string)(value));
                        licenseFound = true;
                        licenseNotFound = false;
                        setLicensedFeatures();
                    }
                } else {
                    base.setConfigurationProperty(name, value);
                }
            }
        }
        public override  /* REPLACING protected internal override */ void setFeature(Saxon.Hej.lib.FeatureData feature, System.Object value) {
            string name = feature.uri;
            int code = feature.code;
            if (name.Equals("http://saxonica.com/oem-data")) {
                supplyLicenseKey(new System.IO.StringReader((string)(value)));
            } else {
                if ((code == Saxon.Hej.lib.FeatureCode.LICENSE_FILE_LOCATION)) {
                    lock (verifier) {
                        verifier.loadLicense((string)(value));
                        licenseFound = true;
                        licenseNotFound = false;
                        setLicensedFeatures();
                    }
                } else {
                    base.setFeature(feature, value);
                }
            }
        }
        public void supplyLicenseKey(System.IO.TextReader reader) {
            verifier.supplyLicenseKey(reader);
            licenseFound = true;
            licenseNotFound = false;
            setLicensedFeatures();
        }
        protected internal void forceLoadLicense() {
            verifier.loadLicense(this);
        }
        public override void importLicenseDetails(Saxon.Hej.Configuration other) {
            if ((other is Saxon.Eej.config.ProfessionalConfiguration)) {
                Saxon.Eej.config.ProfessionalConfiguration pc = (Saxon.Eej.config.ProfessionalConfiguration)(other);
                this.verifier = pc.verifier;
                this.licenseFound = pc.licenseFound;
                this.licenseNotFound = pc.licenseNotFound;
                setLicensedFeatures();
            }
        }
        protected internal string getFeature(string name) {
            string f = verifier.getFeature(name, this);
            if (!(licenseFound)) {
                licenseFound = true;
                licenseNotFound = false;
                setLicensedFeatures();
            }
            return f;
        }
        public override void checkLicensedFeature(int feature, string name, int localLicenseId) {
            if ((feature != Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                throw new Saxon.Hej.trans.LicenseException(((("Requested feature ("+name)+") requires Saxon-EE, ")+"but the current Configuration is an instance of com.saxonica.config.ProfessionalConfiguration"), Saxon.Hej.trans.LicenseException.WRONG_CONFIGURATION);
            }
            if (((localLicenseId>=0) && isFeatureAllowedBySecondaryLicense(localLicenseId, feature))) {
                return ;
            }
            if (licenseNotFound) {
                try {
                    loadLicense();
                } catch (System.Exception err) {
                    int reason = ((err is Saxon.Hej.trans.LicenseException) ? ((Saxon.Hej.trans.LicenseException)(err)).getReason() : Saxon.Hej.trans.LicenseException.CANNOT_READ);
                    string msg = ((err.Message+". A license is needed to use ")+name);
                    throw new Saxon.Hej.trans.LicenseException(msg, reason);
                }
            }
            if (licenseFound) {
                if (!(testFeature(feature))) {
                    throw new Saxon.Hej.trans.LicenseException(((("Requested feature ("+name)+") requires Saxon-EE, ")+"but the current Configuration is an instance of com.saxonica.config.ProfessionalConfiguration"), Saxon.Hej.trans.LicenseException.WRONG_CONFIGURATION);
                }
            } else {
                lock (verifier) {
                    loadLicense();
                    licenseFound = true;
                    reportIfMissingLicenseFeature(feature, name);
                }
            }
        }
        public override void disableLicensing() {
            verifier.disableLicensing();
            licenseNotFound = true;
            licenseFound = false;
        }
        public override bool isFeatureAllowedBySecondaryLicense(int localLicenseId, int feature) {
            return verifier.isFeatureAllowedBySecondaryLicense(localLicenseId, feature);
        }
        protected internal void reportIfMissingLicenseFeature(int feature, string name) {
            if (!(testFeature(feature))) {
                throw new Saxon.Hej.trans.LicenseException((((("A valid license was found ("+verifier.getSerialNumber())+"), but does not support the requested feature (")+name)+")"), Saxon.Hej.trans.LicenseException.WRONG_FEATURES);
            }
        }
        public override bool isLicensedFeature(int feature) {
            if (licenseNotFound) {
                return false;
            }
            lock (verifier) {
                if (!(licenseFound)) {
                    try {
                        loadLicense();
                    } catch (Saxon.Hej.trans.LicenseException e) {
                        if ((e.getReason() == Saxon.Hej.trans.LicenseException.NOT_FOUND)) {
                            licenseNotFound = true;
                            return false;
                        } else {
                            throw;
                        }
                    }
                }
                licenseFound = true;
                return testFeature(feature);
            }
        }
        public override string getLicenseFeature(string name) {
            return verifier.getFeature(name, this);
        }
        protected internal void loadLicense() {
            verifier.loadPrimaryLicense(this);
            if ((((Saxon.Ejava.lang.System.currentTimeMillis()%100) == 42) && "yes".Equals(verifier.getFeature("Evaluation", this)))) {
                Saxon.Eej.serialize.SerializerFactoryPE sf = new Inner_SerializerFactoryPE_9(this, this);
                setSerializerFactory(sf);
            }
            setLicensedFeatures();
        }
        protected internal virtual void setLicensedFeatures() {
        }
        public override int registerLocalLicense(string dmk) {
            return verifier.registerSecondaryLicense(dmk);
        }
        protected internal virtual bool testFeature(int feature) {
            return (feature == Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION);
        }
        public bool isLicenseNotFound() {
            return licenseNotFound;
        }
        public bool isLicenseFound() {
            return licenseFound;
        }
        public override void displayLicenseMessage() {
            if (licenseNotFound) {
                if (isTiming()) {
                    getLogger().info("No license file found - running with licensable features disabled");
                }
            } else {
                try {
                    verifier.displayLicenseMessage(this);
                    licenseFound = true;
                    setLicensedFeatures();
                } catch (Saxon.Hej.trans.LicenseException err) {
                    licenseFound = false;
                    licenseNotFound = true;
                    if ((err.getReason() == Saxon.Hej.trans.LicenseException.NOT_FOUND)) {
                        getLogger().info("No license file found - running with licensable features disabled");
                    } else {
                        throw;
                    }
                }
            }
        }
        public override Saxon.Hej.style.StyleNodeFactory makeStyleNodeFactory(Saxon.Hej.style.Compilation compilation) {
            Saxon.Hej.expr.PackageData pack = compilation.getPackageData();
            if ((pack != null)) {
                int locLic = pack.getLocalLicenseId();
                int feature = Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION;
                if (!(((isFeatureAllowedBySecondaryLicense(locLic, feature) || isLicensedFeature(feature))))) {
                    return base.makeStyleNodeFactory(compilation);
                }
            }
            return new Saxon.Eej.config.pe.StyleNodeFactoryPE(this, compilation);
        }
        public override Saxon.Hej.expr.parser.XPathParser newExpressionParser(string language, bool updating) {
            if (updating) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("XQuery Update requires Saxon-EE");
            }
            if ("XQ".Equals(language)) {
                Saxon.Hej.query.XQueryParser parser = new Saxon.Hej.query.XQueryParser();
                if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                    parser.setParserExtension(new Saxon.Eej.expr.XQueryParserExtensionPE());
                }
                return parser;
            } else {
                if ("XP".Equals(language)) {
                    Saxon.Hej.expr.parser.XPathParser parser = base.newExpressionParser(language, updating);
                    if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                        parser.setParserExtension(new Saxon.Eej.expr.SaxonSyntaxExtension());
                    }
                    return parser;
                } else {
                    if ("PATTERN".Equals(language)) {
                        Saxon.Hej.pattern.PatternParser parser = new Saxon.Hej.pattern.PatternParser();
                        if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                            parser.setParserExtension(new Saxon.Eej.expr.SaxonSyntaxExtension());
                        }
                        return parser;
                    } else {
                        return base.newExpressionParser(language, updating);
                    }
                }
            }
        }
        public override Saxon.Hej.functions.registry.BuiltInFunctionSet getXSLTFunctionSet(int version) {
            if ((version == 30)) {
                return Saxon.Hej.functions.registry.XSLT30FunctionSet.getInstance();
            } else {
                if ((version == 40)) {
                    return /*134*/Saxon.Eej.functions.registry.XSLT40FunctionSet.getInstance();
                } else {
                    throw new Saxon.Ejava.lang.IllegalArgumentException((("Unsupported XSLT version "+version)+" (must be 30 or 40)"));
                }
            }
        }
        public override Saxon.Hej.functions.registry.BuiltInFunctionSet getXPathFunctionSet(int version) {
            switch (version) {
              case 20:
                return Saxon.Hej.functions.registry.XPath20FunctionSet.getInstance();
              case 30:
              case 305:
                return Saxon.Hej.functions.registry.XPath30FunctionSet.getInstance();
              case 31:
                return Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance();
              case 40:
                return /*134*/Saxon.Eej.functions.registry.XPath40FunctionSet.getInstance();
              default:
                return Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance();
            }
        }
        public override Saxon.Hej.functions.registry.BuiltInFunctionSet getXQueryUpdateFunctionSet() {
            return /*134*/Saxon.Eej.functions.registry.XQUpdate10FunctionSet.getInstance();
        }
        public override Saxon.Hej.functions.SystemFunction makeSystemFunction(string localName, int arity) {
            try {
                return getXSLTFunctionSet(40).makeFunction(localName, arity);
            } catch (Saxon.Hej.trans.XPathException e) {
                return null;
            }
        }
        public void setExtensionBinder(string scheme, Saxon.Hej.functions.FunctionLibrary binder) {
            if ((extensionBinders == null)) {
                extensionBinders = new System.Collections.Generic.Dictionary<string, Saxon.Hej.functions.FunctionLibrary>(4);
            }
            Saxon.Impl.Helpers.MapUtils.Put(extensionBinders, scheme, binder);
        }
        public Saxon.Hej.functions.FunctionLibrary getExtensionBinder(string scheme) {
            if ((extensionBinders == null)) {
                return null;
            }
            return Saxon.Impl.Helpers.MapUtils.Get(extensionBinders, scheme);
        }
        public override Saxon.Hej.functions.SystemFunction bindSaxonExtensionFunction(string localName, int arity) {
            checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, ((("saxon:"+localName)+"#")+arity), -1);
            return getVendorFunctionSet().makeFunction(localName, arity);
        }
        public override void addExtensionBinders(Saxon.Hej.functions.FunctionLibraryList list) {
            if ((getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS) && (extensionBinders != null))) {
                foreach (Saxon.Hej.functions.FunctionLibrary functionLibrary in extensionBinders.Values) {
                    list.addFunctionLibrary(functionLibrary);
                }
            }
        }
        public override Saxon.Hej.om.FunctionItem getSystemFunction(Saxon.Hej.om.StructuredQName name, int arity) {
            Saxon.Hej.om.FunctionItem result = base.getSystemFunction(name, arity);
            if ((result != null)) {
                return result;
            }
            if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                try {
                    if ((staticContextForSystemFunctions == null)) {
                        staticContextForSystemFunctions = new Saxon.Hej.sxpath.IndependentContext(this);
                    }
                    Saxon.Hej.functions.FunctionLibrary lib = getExtensionBinder("java");
                    Saxon.Hej.trans.SymbolicName.F symbolicName = new Saxon.Hej.trans.SymbolicName.F(name, arity);
                    return lib.getFunctionItem(symbolicName, staticContextForSystemFunctions);
                } catch (Saxon.Hej.trans.XPathException e) {
                    return null;
                }
            }
            return null;
        }
        protected internal override Saxon.Hej.functions.FunctionLibraryList makeBuiltInExtensionLibraryList(int version) { lock(this) {
            Saxon.Hej.functions.FunctionLibraryList result = new Saxon.Hej.functions.FunctionLibraryList();
            result.addFunctionLibrary(getVendorFunctionSet());
            result.addFunctionLibrary(Saxon.Hej.functions.MathFunctionSet.getInstance());
            result.addFunctionLibrary(Saxon.Hej.ma.map.MapFunctionSet.getInstance(version));
            result.addFunctionLibrary(Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance(version));
            result.addFunctionLibrary(/*134*/Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.getInstance());
            result.addFunctionLibrary(Saxon.Hej.functions.registry.ExsltCommonFunctionSet.getInstance());
            if (getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS)) {
                result.addFunctionLibrary(/*134*/Saxon.Eej.functions.extfn.EXPathFileFunctionSet.getInstance());
            }
            return result;
        }}
        public override Saxon.Hej.functions.registry.BuiltInFunctionSet getVendorFunctionSet() {
            if (isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                return /*134*/Saxon.Eej.functions.extfn.VendorFunctionSetPE.getInstance();
            } else {
                return base.getVendorFunctionSet();
            }
        }
        public void declareBuiltInExtensionFunctions() {
        }
        public virtual void declareBuiltInExtensionElementNamespaces() {
            try {
                setExtensionElementCreator(Saxon.Hej.om.NamespaceUri.SAXON, Saxon.Eej.xsltextn.style.SaxonElementFactory.FACTORY);
                setExtensionElementCreator(Saxon.Hej.om.NamespaceUri.SAXON6, localName => {
                    if (localName.Equals("output")) {
                        return new Saxon.Hej.style.XSLResultDocument();
                    }
                    return null;
                } );
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.IllegalStateException(e.Message);
            }
        }
        public void setExtensionElementCreator(Saxon.Hej.om.NamespaceUri @namespace, Saxon.Eej.xsltextn.ExtensionElementCreator creator) {
            if (Saxon.Hej.om.NamespaceUri.isReserved(@namespace)) {
                throw new Saxon.Hej.trans.XPathException((("Namespace "+@namespace)+" is reserved: it cannot be used for extension instructions"));
            }
            if ((xsltExtensionElementNamespaceMap == null)) {
                xsltExtensionElementNamespaceMap = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NamespaceUri, Saxon.Eej.xsltextn.ExtensionElementCreator>(4);
            }
            Saxon.Impl.Helpers.MapUtils.Put(xsltExtensionElementNamespaceMap, @namespace, creator);
        }
        public Saxon.Eej.xsltextn.ExtensionElementCreator getExtensionElementCreator(Saxon.Hej.om.NamespaceUri @namespace) {
            if ((xsltExtensionElementNamespaceMap == null)) {
                return null;
            }
            return Saxon.Impl.Helpers.MapUtils.Get(xsltExtensionElementNamespaceMap, @namespace);
        }
        public override bool isExtensionElementAvailable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Eej.xsltextn.ExtensionElementCreator eef = getExtensionElementCreator(qName.getNamespaceUri());
            return ((eef != null) && (/*Delegate.createExtensionElement()*/eef(qName.getLocalPart()) != null));
        }
        public override Saxon.Hej.lib.ActiveSource resolveSource(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.Configuration config) {
            Saxon.Hej.lib.ActiveSource s = base.resolveSource(source, config);
            if ((s != null)) {
                return s;
            }
            if ((source is Saxon.Hej.pull.PullProvider)) {
                checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "pull processing", -1);
                return new Saxon.Hej.pull.PullSource((Saxon.Hej.pull.PullProvider)(source));
            }
            return null;
        }
        public override Saxon.Hej.trans.SimpleMode makeMode(Saxon.Hej.om.StructuredQName modeName, Saxon.Hej.trans.CompilerInfo compilerInfo) {
            return new Saxon.Eej.trans.ModePE(modeName);
        }
    
        private class Inner_StaticQueryContextFactory_4 : Saxon.Hej.lib.StaticQueryContextFactory {
            private readonly ProfessionalConfiguration outerInstance;
            public Inner_StaticQueryContextFactory_4(ProfessionalConfiguration outerInstance) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.query.StaticQueryContext newStaticQueryContext(Saxon.Hej.Configuration config, bool copyFromDefault) {
                return new Saxon.Eej.expr.StaticQueryContextPE(this.outerInstance, /*localparam*/copyFromDefault);
            }
        }

        private class Inner_SerializerFactoryPE_9 : Saxon.Eej.serialize.SerializerFactoryPE {
            private readonly ProfessionalConfiguration outerInstance;
            public Inner_SerializerFactoryPE_9(ProfessionalConfiguration outerInstance, ProfessionalConfiguration arg1) : base(arg1) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ Saxon.Hej.@event.Receiver getReceiver(Saxon.Ejavax.xml.transform.Result result, Saxon.Hej.serialize.SerializationProperties parameters, Saxon.Hej.@event.PipelineConfiguration pipe) {
                Saxon.Hej.@event.Receiver r = base.getReceiver(/*localparam*/result, /*localparam*/parameters, /*localparam*/pipe);
                if ((/*localvar*/r is Saxon.Eej.config.EvaluationNuisance)) {
                    return /*localvar*/r;
                } else {
                    return new Saxon.Eej.config.EvaluationNuisance(/*localvar*/r);
                }
            }
        }

    }
}