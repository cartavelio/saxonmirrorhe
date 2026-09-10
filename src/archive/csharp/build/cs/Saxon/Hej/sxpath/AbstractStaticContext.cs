using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.sxpath {
    public abstract class AbstractStaticContext : Saxon.Hej.expr.StaticContext {
        private string baseURI = null;
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.expr.PackageData packageData;
        private Saxon.Hej.s9api.Location containingLocation = Saxon.Hej.expr.parser.Loc.NONE;
        private string defaultCollationName;
        private Saxon.Hej.functions.FunctionLibraryList libraryList = new Saxon.Hej.functions.FunctionLibraryList();
        private string defaultFunctionNamespace = Saxon.Hej.lib.NamespaceConstant.FN;
        private string defaultElementNamespace = Saxon.Hej.lib.NamespaceConstant.NULL;
        private bool backwardsCompatible = false;
        private int xpathLanguageLevel = 31;
        protected internal bool usingDefaultFunctionLibrary;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.type.ItemType> typeAliases = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.type.ItemType>();
        private Saxon.Api.UnprefixedElementMatchingPolicy unprefixedElementPolicy = Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace;
        private Saxon.Ejava.util.function.BiConsumer<string,Saxon.Hej.s9api.Location> warningHandler;
        protected internal void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
            this.defaultCollationName = config.getDefaultCollationName();
            warningHandler = (message, locator) => {
                Saxon.Hej.trans.XmlProcessingIncident incident = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(message, Saxon.Hej.trans.SaxonErrorCode.SXWN9000, locator).asWarning());
                config.makeErrorReporter().report(incident);
            } ;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void setPackageData(Saxon.Hej.expr.PackageData packageData) {
            this.packageData = packageData;
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            return packageData;
        }
        public void setSchemaAware(bool aware) {
            getPackageData().setSchemaAware(aware);
        }
        public virtual Saxon.Hej.expr.parser.RetainedStaticContext makeRetainedStaticContext() {
            return new Saxon.Hej.expr.parser.RetainedStaticContext(this);
        }
        protected internal void setDefaultFunctionLibrary() {
            Saxon.Hej.functions.FunctionLibraryList lib = new Saxon.Hej.functions.FunctionLibraryList();
            lib.addFunctionLibrary(config.getXPath31FunctionSet());
            lib.addFunctionLibrary(getConfiguration().getBuiltInExtensionLibraryList());
            lib.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(getConfiguration()));
            lib.addFunctionLibrary(config.getIntegratedFunctionLibrary());
            config.addExtensionBinders(lib);
            setFunctionLibrary(lib);
        }
        public void setDefaultFunctionLibrary(int version) {
            Saxon.Hej.functions.FunctionLibraryList lib = new Saxon.Hej.functions.FunctionLibraryList();
            switch (version) {
              case 20:
              default:
                lib.addFunctionLibrary(Saxon.Hej.functions.registry.XPath20FunctionSet.getInstance());
                break ;
              case 30:
              case 305:
                lib.addFunctionLibrary(config.getXPath30FunctionSet());
                break ;
              case 31:
                lib.addFunctionLibrary(config.getXPath31FunctionSet());
                break ;
              case 40:
                lib.addFunctionLibrary(config.getXPath40FunctionSet());
                break ;
            }
            lib.addFunctionLibrary(getConfiguration().getBuiltInExtensionLibraryList());
            lib.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(getConfiguration()));
            lib.addFunctionLibrary(config.getIntegratedFunctionLibrary());
            config.addExtensionBinders(lib);
            setFunctionLibrary(lib);
        }
        protected internal void addFunctionLibrary(Saxon.Hej.functions.FunctionLibrary library) {
            libraryList.addFunctionLibrary(library);
        }
        public Saxon.Hej.expr.XPathContext makeEarlyEvaluationContext() {
            return new Saxon.Hej.expr.EarlyEvaluationContext(getConfiguration());
        }
        public Saxon.Hej.s9api.Location getContainingLocation() {
            return containingLocation;
        }
        public void setContainingLocation(Saxon.Hej.s9api.Location location) {
            containingLocation = location;
        }
        public void setBaseURI(string baseURI) {
            this.baseURI = baseURI;
        }
        public string getStaticBaseURI() {
            return ((baseURI == null) ? "" : baseURI);
        }
        public virtual Saxon.Hej.functions.FunctionLibrary getFunctionLibrary() {
            return libraryList;
        }
        public void setFunctionLibrary(Saxon.Hej.functions.FunctionLibraryList lib) {
            libraryList = lib;
            usingDefaultFunctionLibrary = false;
        }
        public void setDefaultCollationName(string collationName) {
            defaultCollationName = collationName;
        }
        public virtual string getDefaultCollationName() {
            return defaultCollationName;
        }
        public void setWarningHandler(Saxon.Ejava.util.function.BiConsumer<string,Saxon.Hej.s9api.Location> handler) {
            warningHandler = handler;
        }
        public Saxon.Ejava.util.function.BiConsumer<string,Saxon.Hej.s9api.Location> getWarningHandler() {
            return warningHandler;
        }
        public virtual void issueWarning(string s, Saxon.Hej.s9api.Location locator) {
            getWarningHandler()(s, locator);
        }
        public virtual string getSystemId() {
            return "";
        }
        public string getDefaultElementNamespace() {
            return defaultElementNamespace;
        }
        public virtual void setDefaultElementNamespace(string uri) {
            defaultElementNamespace = uri;
        }
        public void setDefaultFunctionNamespace(string uri) {
            defaultFunctionNamespace = uri;
        }
        public virtual string getDefaultFunctionNamespace() {
            return defaultFunctionNamespace;
        }
        public void setXPathLanguageLevel(int level) {
            xpathLanguageLevel = level;
        }
        public int getXPathVersion() {
            return xpathLanguageLevel;
        }
        public void setBackwardsCompatibilityMode(bool option) {
            backwardsCompatible = option;
        }
        public virtual bool isInBackwardsCompatibleMode() {
            return backwardsCompatible;
        }
        public void setDecimalFormatManager(Saxon.Hej.trans.DecimalFormatManager manager) {
            getPackageData().setDecimalFormatManager(manager);
        }
        public virtual Saxon.Hej.type.ItemType getRequiredContextItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public virtual Saxon.Hej.trans.DecimalFormatManager getDecimalFormatManager() {
            Saxon.Hej.trans.DecimalFormatManager manager = getPackageData().getDecimalFormatManager();
            if ((manager == null)) {
                manager = new Saxon.Hej.trans.DecimalFormatManager(Saxon.Api.HostLanguage.XPATH, xpathLanguageLevel);
                getPackageData().setDecimalFormatManager(manager);
            }
            return manager;
        }
        public Saxon.Hej.trans.KeyManager getKeyManager() {
            return getPackageData().getKeyManager();
        }
        public void setTypeAlias(Saxon.Hej.om.StructuredQName name, Saxon.Hej.type.ItemType type) {
            Saxon.Impl.Helpers.MapUtils.Put(typeAliases, name, type);
        }
        public virtual Saxon.Hej.type.ItemType resolveTypeAlias(Saxon.Hej.om.StructuredQName typeName) {
            return Saxon.Impl.Helpers.MapUtils.Get(typeAliases, typeName);
        }
        public void setUnprefixedElementMatchingPolicy(Saxon.Api.UnprefixedElementMatchingPolicy policy) {
            this.unprefixedElementPolicy = policy;
        }
        public Saxon.Api.UnprefixedElementMatchingPolicy getUnprefixedElementMatchingPolicy() {
            return unprefixedElementPolicy;
        }
    
        /* Expanding default method getOptimizerOptions from interface StaticContext */
        public virtual Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions() {
            return getConfiguration().getOptimizerOptions();
        }

        /* Creating stub method bindVariable to satisfy interface StaticContext */
        public virtual Saxon.Hej.expr.Expression bindVariable(Saxon.Hej.om.StructuredQName arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method isImportedSchema to satisfy interface StaticContext */
        public virtual bool isImportedSchema(string arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getImportedSchemaNamespaces to satisfy interface StaticContext */
        public virtual System.Collections.Generic.ISet<string> getImportedSchemaNamespaces() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getNamespaceResolver to satisfy interface StaticContext */
        public virtual Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            throw new System.NotImplementedException();
        }

    }
}