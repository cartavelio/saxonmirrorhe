using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class QueryModule : Saxon.Hej.expr.StaticContext {
        private bool moduleIsMainModule;
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Hej.query.StaticQueryContext userQueryContext;
        private readonly Saxon.Hej.query.QueryModule topModule;
        private System.Uri locationURI;
        private string baseURI;
        private Saxon.Hej.om.NamespaceUri moduleNamespace;
        private System.Collections.Generic.Dictionary<string,Saxon.Hej.om.NamespaceUri> explicitPrologNamespaces;
        private Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.om.NamespaceBinding> activeNamespaces;
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.GlobalVariable> variables;
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.GlobalVariable> libraryVariables;
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.query.UndeclaredVariable> undeclaredVariables;
        private System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri> importedSchemata;
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.NamespaceUri,System.Collections.Generic.HashSet<string>> loadedSchemata;
        private Saxon.Hej.expr.instruct.Executable executable;
        private System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> importers;
        private Saxon.Hej.functions.FunctionLibraryList functionLibraryList;
        private Saxon.Hej.query.XQueryFunctionLibrary globalFunctionLibrary;
        private int localFunctionLibraryNr;
        private int importedFunctionLibraryNr;
        private int unboundFunctionLibraryNr;
        private System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> importedModuleNamespaces;
        private bool inheritNamespaces = true;
        private bool preserveNamespaces = true;
        private int constructionMode = Saxon.Hej.lib.Validation.PRESERVE;
        private Saxon.Hej.om.NamespaceUri defaultFunctionNamespace;
        private Saxon.Hej.om.NamespaceUri defaultElementNamespace;
        private bool preserveSpace = false;
        private bool defaultEmptyLeast = true;
        private string defaultCollationName;
        private int revalidationMode = Saxon.Hej.lib.Validation.SKIP;
        private bool updating = false;
        private Saxon.Hej.type.ItemType requiredContextItemType = Saxon.Hej.type.AnyItemType.getInstance();
        private Saxon.Hej.trans.DecimalFormatManager decimalFormatManager = null;
        private Saxon.Hej.expr.parser.CodeInjector codeInjector;
        private Saxon.Hej.expr.PackageData packageData;
        private Saxon.Hej.expr.parser.RetainedStaticContext moduleStaticContext = null;
        private Saxon.Hej.s9api.Location moduleLocation;
        private Saxon.Hej.expr.parser.OptimizerOptions optimizerOptions;
        private int languageLevel;
        public QueryModule(Saxon.Hej.query.StaticQueryContext sqc)  {
            config = sqc.getConfiguration();
            moduleIsMainModule = true;
            topModule = this;
            languageLevel = sqc.getLanguageVersion();
            activeNamespaces = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.om.NamespaceBinding>();
            baseURI = sqc.getBaseURI();
            defaultCollationName = sqc.getDefaultCollationName();
            try {
                locationURI = ((baseURI == null) ? null : new System.Uri(baseURI, System.UriKind.RelativeOrAbsolute));
            } catch (System.UriFormatException err) {
                throw new Saxon.Hej.trans.XPathException(("Invalid location URI: "+baseURI));
            }
            executable = sqc.makeExecutable();
            importers = null;
            init(sqc);
            Saxon.Hej.expr.PackageData pd = new Saxon.Hej.expr.PackageData(config);
            pd.setHostLanguage(Saxon.Api.HostLanguage.XQUERY, getXPathVersion());
            pd.setSchemaAware(isSchemaAware());
            packageData = pd;
            foreach (Saxon.Hej.expr.instruct.GlobalVariable var in sqc.iterateDeclaredGlobalVariables()) {
                declareVariable(var);
                pd.addGlobalVariable(var);
                var.setPackageData(pd);
            }
            executable.setTopLevelPackage(pd);
            executable.addPackage(pd);
            if ((sqc.getModuleLocation() == null)) {
                moduleLocation = new Saxon.Hej.expr.parser.Loc(sqc.getSystemId(), 1, -1);
            } else {
                moduleLocation = sqc.getModuleLocation();
            }
            optimizerOptions = sqc.getOptimizerOptions();
        }
        public QueryModule(Saxon.Hej.Configuration config, Saxon.Hej.query.QueryModule importer)  {
            this.config = config;
            importers = null;
            if ((importer == null)) {
                topModule = this;
            } else {
                topModule = importer.topModule;
                userQueryContext = importer.userQueryContext;
                importers = new System.Collections.Generic.List<Saxon.Hej.query.QueryModule>(2);
                importers.Add(importer);
            }
            init(userQueryContext);
            packageData = importer.getPackageData();
            activeNamespaces = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.om.NamespaceBinding>();
            executable = null;
            optimizerOptions = importer.optimizerOptions;
        }
        private void init(Saxon.Hej.query.StaticQueryContext sqc) {
            userQueryContext = sqc;
            variables = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.instruct.GlobalVariable>(10);
            undeclaredVariables = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.query.UndeclaredVariable>(5);
            if (isTopLevelModule()) {
                libraryVariables = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.instruct.GlobalVariable>(10);
            }
            importedSchemata = new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>(5);
            importedModuleNamespaces = new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>(5);
            moduleNamespace = null;
            activeNamespaces = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.om.NamespaceBinding>();
            explicitPrologNamespaces = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.NamespaceUri>(10);
            if ((sqc != null)) {
                inheritNamespaces = sqc.isInheritNamespaces();
                preserveNamespaces = sqc.isPreserveNamespaces();
                preserveSpace = sqc.isPreserveBoundarySpace();
                defaultEmptyLeast = sqc.isEmptyLeast();
                defaultFunctionNamespace = sqc.getDefaultFunctionNamespace();
                defaultElementNamespace = sqc.getDefaultElementNamespace();
                defaultCollationName = sqc.getDefaultCollationName();
                constructionMode = sqc.getConstructionMode();
                if (((constructionMode == Saxon.Hej.lib.Validation.PRESERVE) && !(sqc.isSchemaAware()))) {
                    constructionMode = Saxon.Hej.lib.Validation.STRIP;
                }
                requiredContextItemType = sqc.getRequiredContextItemType();
                updating = sqc.isUpdatingEnabled();
                codeInjector = sqc.getCodeInjector();
                optimizerOptions = sqc.getOptimizerOptions();
            }
            initializeFunctionLibraries(sqc);
        }
        public static Saxon.Hej.query.QueryModule makeQueryModule(string baseURI, Saxon.Hej.expr.instruct.Executable executable, Saxon.Hej.query.QueryModule importer, string query, Saxon.Hej.om.NamespaceUri namespaceURI) {
            Saxon.Ejava.util.Objects.requireNonNull(baseURI, "Base URI of XQuery module must not be null");
            Saxon.Hej.Configuration config = executable.getConfiguration();
            Saxon.Hej.query.QueryModule module = new Saxon.Hej.query.QueryModule(config, importer);
            try {
                module.setLocationURI(new System.Uri(baseURI, System.UriKind.RelativeOrAbsolute));
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException(("Invalid location URI "+baseURI), e);
            }
            module.setBaseURI(baseURI);
            module.setExecutable(executable);
            module.setModuleNamespace(namespaceURI);
            executable.addQueryLibraryModule(module);
            Saxon.Hej.query.XQueryParser qp = (Saxon.Hej.query.XQueryParser)(config.newExpressionParser("XQ", importer.isUpdating()));
            if ((importer.getCodeInjector() != null)) {
                qp.setCodeInjector(importer.getCodeInjector());
            } else {
                if (config.isCompileWithTracing()) {
                    qp.setCodeInjector(new Saxon.Hej.trace.XQueryTraceCodeInjector());
                }
            }
            Saxon.Hej.om.QNameParser qnp = new Saxon.Hej.om.QNameParser(module.getLiveNamespaceResolver()).withAcceptEQName((importer.getXPathVersion()>=30)).withUnescaper(new Saxon.Hej.query.XQueryParser.Unescaper(config.getValidCharacterChecker()));
            qp.setQNameParser(qnp);
            qp.parseLibraryModule(query, module);
            Saxon.Hej.om.NamespaceUri @namespace = module.getModuleNamespace();
            if ((@namespace == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Imported module must be a library module");
                err.setErrorCode("XQST0059");
                err.setIsStaticError(true);
                throw err;
            }
            if (!(@namespace.Equals(namespaceURI))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Imported module's namespace does not match requested namespace");
                err.setErrorCode("XQST0059");
                err.setIsStaticError(true);
                throw err;
            }
            return module;
        }
        private void initializeFunctionLibraries(Saxon.Hej.query.StaticQueryContext sqc) {
            Saxon.Hej.Configuration config = getConfiguration();
            if (isTopLevelModule()) {
                globalFunctionLibrary = new Saxon.Hej.query.XQueryFunctionLibrary(config);
            }
            functionLibraryList = new Saxon.Hej.functions.FunctionLibraryList();
            functionLibraryList.addFunctionLibrary(getBuiltInFunctionSet());
            functionLibraryList.addFunctionLibrary(config.getBuiltInExtensionLibraryList(sqc.getLanguageVersion()));
            functionLibraryList.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(config));
            localFunctionLibraryNr = functionLibraryList.addFunctionLibrary(new Saxon.Hej.query.XQueryFunctionLibrary(config));
            importedFunctionLibraryNr = functionLibraryList.addFunctionLibrary(new Saxon.Hej.query.ImportedFunctionLibrary(this, getTopLevelModule().getGlobalFunctionLibrary()));
            if ((sqc.getExtensionFunctionLibrary() != null)) {
                functionLibraryList.addFunctionLibrary(sqc.getExtensionFunctionLibrary());
            }
            functionLibraryList.addFunctionLibrary(config.getIntegratedFunctionLibrary());
            config.addExtensionBinders(functionLibraryList);
            unboundFunctionLibraryNr = functionLibraryList.addFunctionLibrary(new Saxon.Hej.query.UnboundFunctionLibrary());
        }
        public Saxon.Hej.functions.registry.BuiltInFunctionSet getBuiltInFunctionSet() {
            if (isUpdating()) {
                return config.getXQueryUpdateFunctionSet();
            } else {
                return config.getXPathFunctionSet(languageLevel);
            }
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            return packageData;
        }
        public void setPackageData(Saxon.Hej.expr.PackageData packageData) {
            this.packageData = packageData;
        }
        public bool isTopLevelModule() {
            return (this == topModule);
        }
        public void setIsMainModule(bool main) {
            moduleIsMainModule = main;
        }
        public bool isMainModule() {
            return moduleIsMainModule;
        }
        public bool mayImportModule(string @namespace) {
            if (@namespace.Equals(moduleNamespace)) {
                return false;
            }
            if ((importers == null)) {
                return true;
            }
            foreach (Saxon.Hej.query.QueryModule importer in importers) {
                if (!(importer.mayImportModule(@namespace))) {
                    return false;
                }
            }
            return true;
        }
        public bool isSchemaAware() {
            return executable.isSchemaAware();
        }
        public Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions() {
            return optimizerOptions;
        }
        public Saxon.Hej.expr.parser.RetainedStaticContext makeRetainedStaticContext() {
            if (activeNamespaces.isEmpty()) {
                if ((moduleStaticContext == null)) {
                    moduleStaticContext = new Saxon.Hej.expr.parser.RetainedStaticContext(this);
                }
                return moduleStaticContext;
            } else {
                return new Saxon.Hej.expr.parser.RetainedStaticContext(this);
            }
        }
        public void setInheritNamespaces(bool inherit) {
            inheritNamespaces = inherit;
        }
        public bool isInheritNamespaces() {
            return inheritNamespaces;
        }
        public void setPreserveNamespaces(bool inherit) {
            preserveNamespaces = inherit;
        }
        public bool isPreserveNamespaces() {
            return preserveNamespaces;
        }
        public void setConstructionMode(int mode) {
            constructionMode = mode;
        }
        public int getConstructionMode() {
            return constructionMode;
        }
        public void setPreserveBoundarySpace(bool preserve) {
            preserveSpace = preserve;
        }
        public bool isPreserveBoundarySpace() {
            return preserveSpace;
        }
        public void setEmptyLeast(bool least) {
            defaultEmptyLeast = least;
        }
        public bool isEmptyLeast() {
            return defaultEmptyLeast;
        }
        public Saxon.Hej.query.XQueryFunctionLibrary getGlobalFunctionLibrary() {
            return globalFunctionLibrary;
        }
        public Saxon.Hej.query.ImportedFunctionLibrary getImportedFunctionLibrary() {
            return (Saxon.Hej.query.ImportedFunctionLibrary)(functionLibraryList.get(importedFunctionLibraryNr));
        }
        public void addImportedNamespace(Saxon.Hej.om.NamespaceUri uri) {
            if ((importedModuleNamespaces == null)) {
                importedModuleNamespaces = new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>(5);
            }
            importedModuleNamespaces.Add(uri);
            getImportedFunctionLibrary().addImportedNamespace(uri);
        }
        public bool importsNamespace(Saxon.Hej.om.NamespaceUri uri) {
            return ((importedModuleNamespaces != null) && importedModuleNamespaces.Contains(uri));
        }
        public Saxon.Hej.query.QueryModule getTopLevelModule() {
            return topModule;
        }
        public Saxon.Hej.expr.instruct.Executable getExecutable() {
            return executable;
        }
        public void setExecutable(Saxon.Hej.expr.instruct.Executable executable) {
            this.executable = executable;
        }
        public Saxon.Hej.query.StaticQueryContext getUserQueryContext() {
            return userQueryContext;
        }
        public Saxon.Hej.s9api.Location getContainingLocation() {
            return moduleLocation;
        }
        public void setModuleNamespace(Saxon.Hej.om.NamespaceUri uri) {
            moduleNamespace = uri;
        }
        public Saxon.Hej.om.NamespaceUri getModuleNamespace() {
            return moduleNamespace;
        }
        public void setLocationURI(System.Uri uri) {
            locationURI = uri;
            moduleLocation = new Saxon.Hej.expr.parser.Loc(locationURI.ToString(), 1, -1);
        }
        public System.Uri getLocationURI() {
            return locationURI;
        }
        public string getSystemId() {
            return ((locationURI == null) ? null : locationURI.ToString());
        }
        public void setBaseURI(string uri) {
            baseURI = uri;
        }
        public string getStaticBaseURI() {
            return baseURI;
        }
        public Saxon.Hej.expr.instruct.SlotManager getGlobalStackFrameMap() {
            return getPackageData().getGlobalSlotManager();
        }
        public void declareVariable(Saxon.Hej.expr.instruct.GlobalVariable var) {
            Saxon.Hej.om.StructuredQName key = var.getVariableQName();
            if ((Saxon.Impl.Helpers.MapUtils.Get(variables, key) != null)) {
                Saxon.Hej.expr.instruct.GlobalVariable oldVar = Saxon.Impl.Helpers.MapUtils.Get(variables, key);
                if (((oldVar == var) || (oldVar.getUltimateOriginalVariable() == var.getUltimateOriginalVariable()))) {
                } else {
                    string oldloc = (" (see line "+oldVar.getLineNumber());
                    string oldSysId = oldVar.getSystemId();
                    if (((oldSysId != null) && !(oldSysId.Equals(var.getSystemId())))) {
                        oldloc += (" in module "+oldVar.getSystemId());
                    }
                    oldloc += ")";
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Duplicate definition of global variable "+var.getVariableQName().getDisplayName())+oldloc));
                    err.setErrorCode("XQST0049");
                    err.setIsStaticError(true);
                    err.setLocation(var);
                    throw err;
                }
            }
            Saxon.Impl.Helpers.MapUtils.Put(variables, key, var);
            getPackageData().addGlobalVariable(var);
            System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.GlobalVariable> libVars = getTopLevelModule().libraryVariables;
            Saxon.Hej.expr.instruct.GlobalVariable old = Saxon.Impl.Helpers.MapUtils.Get(libVars, key);
            if (((old == null) || (old == var))) {
            } else {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((((("Duplicate definition of global variable "+var.getVariableQName().getDisplayName())+" (see line ")+old.getLineNumber())+" in module ")+old.getSystemId())+')'));
                err.setErrorCode("XQST0049");
                err.setIsStaticError(true);
                err.setLocation(var);
                throw err;
            }
            if (!(isMainModule())) {
                Saxon.Impl.Helpers.MapUtils.Put(libVars, key, var);
            }
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.instruct.GlobalVariable> getImportedGlobalVariables() {
            return libraryVariables.Values;
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.instruct.GlobalVariable> getAllGlobalVariables() {
            if (isMainModule()) {
                System.Collections.Generic.IList<Saxon.Hej.expr.instruct.GlobalVariable> allVars = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.GlobalVariable>(libraryVariables.Values);
                Saxon.Impl.Helpers.ListUtils.AddAll(allVars, variables.Values);
                return allVars;
            } else {
                return getTopLevelModule().getAllGlobalVariables();
            }
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.instruct.GlobalVariable> fixupGlobalVariables(Saxon.Hej.expr.instruct.SlotManager globalVariableMap) {
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.GlobalVariable> varDefinitions = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.GlobalVariable>(20);
            System.Collections.Generic.IList<System.Collections.Generic.IEnumerable<Saxon.Hej.expr.instruct.GlobalVariable>> iters = new System.Collections.Generic.List<System.Collections.Generic.IEnumerable<Saxon.Hej.expr.instruct.GlobalVariable>>();
            iters.Add(variables.Values);
            iters.Add(libraryVariables.Values);
            foreach (System.Collections.Generic.IEnumerable<Saxon.Hej.expr.instruct.GlobalVariable> iter in iters) {
                foreach (Saxon.Hej.expr.instruct.GlobalVariable var in iter) {
                    if (!(varDefinitions.Contains(var))) {
                        int slot = globalVariableMap.allocateSlotNumber(var.getVariableQName());
                        var.compile(getExecutable(), slot);
                        varDefinitions.Add(var);
                    }
                }
            }
            return varDefinitions;
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.expr.instruct.GlobalVariable> getModuleVariables() {
            return variables.Values.GetEnumerator();
        }
        public void checkForCircularities(System.Collections.Generic.IList<Saxon.Hej.expr.instruct.GlobalVariable> compiledVars, Saxon.Hej.query.XQueryFunctionLibrary globalFunctionLibrary) {
            System.Collections.Generic.IEnumerator<Saxon.Hej.expr.instruct.GlobalVariable> iter = compiledVars.GetEnumerator();
            Saxon.Hej.tree.util.IndexedStack<System.Object> stack = null;
            while (iter.MoveNext() /*UNSAFE CONVERSION*/) {
                if ((stack == null)) {
                    stack = new Saxon.Hej.tree.util.IndexedStack<System.Object>();
                }
                Saxon.Hej.expr.instruct.GlobalVariable gv = iter.Current;
                if ((gv != null)) {
                    gv.lookForCycles(stack, globalFunctionLibrary);
                }
            }
        }
        public void typeCheckGlobalVariables(System.Collections.Generic.IList<Saxon.Hej.expr.instruct.GlobalVariable> compiledVars) {
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(this);
            foreach (Saxon.Hej.expr.instruct.GlobalVariable compiledVar in compiledVars) {
                compiledVar.typeCheck(visitor);
            }
            if (isMainModule()) {
                Saxon.Hej.expr.instruct.GlobalContextRequirement gcr = executable.getGlobalContextRequirement();
                if (((gcr != null) && (gcr.getDefaultValue() != null))) {
                    Saxon.Hej.expr.parser.ContextItemStaticInfo info = getConfiguration().makeContextItemStaticInfo(Saxon.Hej.type.AnyItemType.getInstance(), true);
                    gcr.setDefaultValue(gcr.getDefaultValue().typeCheck(visitor, info));
                }
            }
        }
        public Saxon.Hej.expr.Expression bindVariable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.expr.instruct.GlobalVariable var = Saxon.Impl.Helpers.MapUtils.Get(variables, qName);
            if ((var == null)) {
                Saxon.Hej.om.NamespaceUri uri = qName.getNamespaceUri();
                if (((((uri.Equals(Saxon.Hej.om.NamespaceUri.NULL) && isMainModule())) || uri.Equals(moduleNamespace)) || importsNamespace(uri))) {
                    Saxon.Hej.query.QueryModule main = getTopLevelModule();
                    var = Saxon.Impl.Helpers.MapUtils.Get(main.libraryVariables, qName);
                    if ((var == null)) {
                        Saxon.Hej.query.UndeclaredVariable uvar = Saxon.Impl.Helpers.MapUtils.Get(undeclaredVariables, qName);
                        if ((uvar != null)) {
                            Saxon.Hej.expr.GlobalVariableReference @ref = new Saxon.Hej.expr.GlobalVariableReference(qName);
                            uvar.registerReference(@ref);
                            return @ref;
                        } else {
                            uvar = new Saxon.Hej.query.UndeclaredVariable();
                            uvar.setPackageData(main.getPackageData());
                            uvar.setVariableQName(qName);
                            Saxon.Hej.expr.GlobalVariableReference @ref = new Saxon.Hej.expr.GlobalVariableReference(qName);
                            uvar.registerReference(@ref);
                            Saxon.Impl.Helpers.MapUtils.Put(undeclaredVariables, qName, uvar);
                            return @ref;
                        }
                    } else {
                        if (var.isPrivate()) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Variable $"+qName.getDisplayName())+" is private"));
                            err.setErrorCode("XPST0008");
                            err.setIsStaticError(true);
                            throw err;
                        }
                    }
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Variable $"+qName.getDisplayName())+" has not been declared"));
                    err.setErrorCode("XPST0008");
                    err.setIsStaticError(true);
                    throw err;
                }
            } else {
                if ((var.isPrivate() && (((var.getSystemId() == null) || !(var.getSystemId().Equals(getSystemId())))))) {
                    string message = (("Variable $"+qName.getDisplayName())+" is private");
                    if ((var.getSystemId() == null)) {
                        message += " (no base URI known)";
                    }
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, "XPST0008");
                    err.setIsStaticError(true);
                    throw err;
                }
            }
            Saxon.Hej.expr.GlobalVariableReference vref = new Saxon.Hej.expr.GlobalVariableReference(qName);
            var.registerReference(vref);
            return vref;
        }
        public Saxon.Hej.functions.FunctionLibrary getFunctionLibrary() {
            return functionLibraryList;
        }
        public Saxon.Hej.query.XQueryFunctionLibrary getLocalFunctionLibrary() {
            return (Saxon.Hej.query.XQueryFunctionLibrary)(functionLibraryList.get(localFunctionLibraryNr));
        }
        public void declareFunction(Saxon.Hej.query.XQueryFunction function) {
            Saxon.Hej.Configuration config = getConfiguration();
            if (((function.getMinimumArity()<=1) && (function.getNumberOfParameters()>=1))) {
                Saxon.Hej.om.StructuredQName name = function.getFunctionName();
                Saxon.Hej.type.SchemaType t = config.getSchemaType(name);
                if (((t != null) && t.isAtomicType())) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Function name "+function.getDisplayName())+" clashes with the name of the constructor function for an atomic type"));
                    err.setErrorCode("XQST0034");
                    err.setIsStaticError(true);
                    throw err;
                }
            }
            Saxon.Hej.query.XQueryFunctionLibrary local = getLocalFunctionLibrary();
            local.declareFunction(function);
            Saxon.Hej.query.QueryModule main = getTopLevelModule();
            main.globalFunctionLibrary.declareFunction(function);
        }
        public void bindUnboundFunctionCalls() {
            Saxon.Hej.query.UnboundFunctionLibrary lib = (Saxon.Hej.query.UnboundFunctionLibrary)(functionLibraryList.get(unboundFunctionLibraryNr));
            lib.bindUnboundFunctionReferences(functionLibraryList, getConfiguration());
        }
        public void fixupGlobalFunctions() {
            globalFunctionLibrary.fixupGlobalFunctions(this);
        }
        public void optimizeGlobalFunctions() {
            globalFunctionLibrary.optimizeGlobalFunctions(this);
        }
        public void explainGlobalFunctions(Saxon.Hej.trace.ExpressionPresenter @out) {
            globalFunctionLibrary.explainGlobalFunctions(@out);
        }
        public Saxon.Hej.expr.instruct.UserFunction getUserDefinedFunction(Saxon.Hej.om.NamespaceUri uri, string localName, int arity) {
            return globalFunctionLibrary.getUserDefinedFunction(uri, localName, arity);
        }
        public void bindUnboundVariables() {
            foreach (Saxon.Hej.query.UndeclaredVariable uv in undeclaredVariables.Values) {
                Saxon.Hej.om.StructuredQName qName = uv.getVariableQName();
                Saxon.Hej.expr.instruct.GlobalVariable var = Saxon.Impl.Helpers.MapUtils.Get(variables, qName);
                if ((var == null)) {
                    Saxon.Hej.om.NamespaceUri uri = qName.getNamespaceUri();
                    if (importsNamespace(uri)) {
                        Saxon.Hej.query.QueryModule main = getTopLevelModule();
                        var = Saxon.Impl.Helpers.MapUtils.Get(main.libraryVariables, qName);
                    }
                }
                if ((var == null)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Unresolved reference to variable $"+uv.getVariableQName().getDisplayName()));
                    err.setErrorCode("XPST0008");
                    err.setIsStaticError(true);
                    throw err;
                } else {
                    if ((var.isPrivate() && !(var.getSystemId().Equals(getSystemId())))) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot reference a private variable in a different module");
                        err.setErrorCode("XPST0008");
                        err.setIsStaticError(true);
                        throw err;
                    } else {
                        uv.transferReferences(var);
                    }
                }
            }
        }
        public void addImportedSchema(Saxon.Hej.om.NamespaceUri targetNamespace, string baseURI, System.Collections.Generic.IList<string> locationURIs) {
            if ((importedSchemata == null)) {
                importedSchemata = new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>(5);
            }
            importedSchemata.Add(targetNamespace);
            System.Collections.Generic.Dictionary<Saxon.Hej.om.NamespaceUri,System.Collections.Generic.HashSet<string>> loadedSchemata = getTopLevelModule().loadedSchemata;
            if ((loadedSchemata == null)) {
                loadedSchemata = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NamespaceUri, System.Collections.Generic.HashSet<string>>(5);
                getTopLevelModule().loadedSchemata = loadedSchemata;
            }
            System.Collections.Generic.HashSet<string> entries = Saxon.Impl.Helpers.MapUtils.Get(loadedSchemata, targetNamespace);
            if ((entries == null)) {
                entries = new System.Collections.Generic.HashSet<string>(locationURIs.Count);
                Saxon.Impl.Helpers.MapUtils.Put(loadedSchemata, targetNamespace, entries);
            }
            foreach (string relative in locationURIs) {
                try {
                    System.Uri abs = Saxon.Hej.functions.ResolveURI.makeAbsolute(relative, baseURI);
                    entries.Add(abs.ToString());
                } catch (System.UriFormatException e) {
                }
            }
        }
        public bool isImportedSchema(Saxon.Hej.om.NamespaceUri @namespace) {
            return ((importedSchemata != null) && importedSchemata.Contains(@namespace));
        }
        public System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> getImportedSchemaNamespaces() {
            if ((importedSchemata == null)) {
                return new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>();
            } else {
                return importedSchemata;
            }
        }
        public void reportStaticError(Saxon.Hej.trans.XPathException err) {
            if (!(err.hasBeenReported())) {
                reportStaticError(new Saxon.Hej.trans.XmlProcessingException(err));
                err.setHasBeenReported(true);
            }
        }
        public void reportStaticError(Saxon.Hej.s9api.XmlProcessingError err) {
            userQueryContext.getErrorReporter().report(err);
            if ((err.getTerminationMessage() != null)) {
                throw new Saxon.Hej.trans.XmlProcessingAbort(err.getTerminationMessage());
            }
        }
        public Saxon.Hej.expr.XPathContext makeEarlyEvaluationContext() {
            return new Saxon.Hej.expr.EarlyEvaluationContext(getConfiguration());
        }
        public string getDefaultCollationName() {
            if ((defaultCollationName == null)) {
                defaultCollationName = Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
            }
            return defaultCollationName;
        }
        public void setDefaultCollationName(string collation) {
            defaultCollationName = collation;
        }
        public void declarePrologNamespace(string prefix, Saxon.Hej.om.NamespaceUri uri) {
            if ((prefix == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("Null prefix supplied to declarePrologNamespace()");
            }
            if ((uri == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("Null namespace URI supplied to declarePrologNamespace()");
            }
            if ((prefix.Equals("xml") != uri.Equals(Saxon.Hej.om.NamespaceUri.XML))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Invalid declaration of the XML namespace");
                err.setErrorCode("XQST0070");
                err.setIsStaticError(true);
                throw err;
            }
            if ((Saxon.Impl.Helpers.MapUtils.Get(explicitPrologNamespaces, prefix) != null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Duplicate declaration of namespace prefix \""+prefix)+'"'));
                err.setErrorCode("XQST0033");
                err.setIsStaticError(true);
                throw err;
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(explicitPrologNamespaces, prefix, uri);
            }
        }
        public void declareActiveNamespace(string prefix, Saxon.Hej.om.NamespaceUri uri) {
            if ((prefix == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("Null prefix supplied to declareActiveNamespace()");
            }
            if ((uri == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("Null namespace URI supplied to declareActiveNamespace()");
            }
            Saxon.Hej.om.NamespaceBinding entry = new Saxon.Hej.om.NamespaceBinding(prefix, uri);
            activeNamespaces.push(entry);
        }
        public void undeclareNamespace() {
            activeNamespaces.pop();
        }
        public Saxon.Hej.om.NamespaceResolver getLiveNamespaceResolver() {
            return new Inner_NamespaceResolver_305(this);
        }
        public Saxon.Hej.om.NamespaceUri checkURIForPrefix(string prefix) {
            if ((activeNamespaces != null)) {
                for (int i = (activeNamespaces.size()-1); (i>=0); i--) {
                    if (activeNamespaces.get(i).getPrefix().Equals(prefix)) {
                        Saxon.Hej.om.NamespaceUri ns = activeNamespaces.get(i).getNamespaceUri();
                        if ((ns.isEmpty() && !(prefix.Equals("")))) {
                            return null;
                        }
                        return ns;
                    }
                }
            }
            if (prefix.Length==0) {
                return defaultElementNamespace;
            }
            Saxon.Hej.om.NamespaceUri uri = Saxon.Impl.Helpers.MapUtils.Get(explicitPrologNamespaces, prefix);
            if ((uri != null)) {
                return (uri.isEmpty() ? null : uri);
            }
            if ((userQueryContext != null)) {
                uri = userQueryContext.getNamespaceForPrefix(prefix);
                if ((uri != null)) {
                    return uri;
                }
            }
            return null;
        }
        public Saxon.Hej.om.NamespaceUri getDefaultElementNamespace() {
            return checkURIForPrefix("");
        }
        public void setDefaultElementNamespace(Saxon.Hej.om.NamespaceUri uri) {
            defaultElementNamespace = uri;
        }
        public Saxon.Hej.om.NamespaceUri getDefaultFunctionNamespace() {
            return defaultFunctionNamespace;
        }
        public void setDefaultFunctionNamespace(Saxon.Hej.om.NamespaceUri uri) {
            defaultFunctionNamespace = uri;
        }
        public void setRevalidationMode(int mode) {
            if ((((mode == Saxon.Hej.lib.Validation.STRICT) || (mode == Saxon.Hej.lib.Validation.LAX)) || (mode == Saxon.Hej.lib.Validation.SKIP))) {
                revalidationMode = mode;
            } else {
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Invalid mode "+mode));
            }
        }
        public int getRevalidationMode() {
            return revalidationMode;
        }
        internal Saxon.Hej.om.NamespaceMap getActiveNamespaceBindings() {
            if ((activeNamespaces == null)) {
                return Saxon.Hej.om.NamespaceMap.emptyMap();
            }
            Saxon.Hej.om.NamespaceMap result = Saxon.Hej.om.NamespaceMap.emptyMap();
            System.Collections.Generic.HashSet<string> prefixes = new System.Collections.Generic.HashSet<string>(10);
            for (int n = (activeNamespaces.size()-1); (n>=0); n--) {
                Saxon.Hej.om.NamespaceBinding an = activeNamespaces.get(n);
                if (!(prefixes.Contains(an.getPrefix()))) {
                    prefixes.Add(an.getPrefix());
                    if (!(an.getNamespaceUri().isEmpty())) {
                        result = result.put(an.getPrefix(), an.getNamespaceUri());
                    }
                }
            }
            return result;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            Saxon.Hej.om.NamespaceMap result = Saxon.Hej.om.NamespaceMap.emptyMap();
            System.Collections.Generic.Dictionary<string,Saxon.Hej.om.NamespaceUri> userDeclaredNamespaces = userQueryContext.getUserDeclaredNamespaces();
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.NamespaceUri> e in userDeclaredNamespaces) {
                result = result.put(e.Key, e.Value);
            }
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.NamespaceUri> e in explicitPrologNamespaces) {
                result = result.put(e.Key, e.Value);
            }
            if (!(defaultElementNamespace.isEmpty())) {
                result = result.put("", defaultElementNamespace);
            }
            if ((activeNamespaces == null)) {
                return result;
            }
            System.Collections.Generic.HashSet<string> prefixes = new System.Collections.Generic.HashSet<string>(10);
            for (int n = (activeNamespaces.size()-1); (n>=0); n--) {
                Saxon.Hej.om.NamespaceBinding an = activeNamespaces.get(n);
                if (!(prefixes.Contains(an.getPrefix()))) {
                    prefixes.Add(an.getPrefix());
                    if (an.getNamespaceUri().isEmpty()) {
                        result = result.remove(an.getPrefix());
                    } else {
                        result = result.put(an.getPrefix(), an.getNamespaceUri());
                    }
                }
            }
            return result;
        }
        public Saxon.Hej.type.ItemType getRequiredContextItemType() {
            return requiredContextItemType;
        }
        public Saxon.Hej.trans.DecimalFormatManager getDecimalFormatManager() {
            if ((decimalFormatManager == null)) {
                decimalFormatManager = new Saxon.Hej.trans.DecimalFormatManager(Saxon.Api.HostLanguage.XQUERY, getXPathVersion());
            }
            return decimalFormatManager;
        }
        public void issueWarning(string s, string errorCode, Saxon.Hej.s9api.Location locator) {
            Saxon.Hej.trans.XmlProcessingIncident err = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(s, errorCode).asWarning());
            err.setLocation(locator);
            err.setHostLanguage(Saxon.Api.HostLanguage.XQUERY);
            userQueryContext.getErrorReporter().report(err);
        }
        public bool isInBackwardsCompatibleMode() {
            return false;
        }
        public bool isUpdating() {
            return updating;
        }
        public void setXPathVersion(int languageLevel) {
            this.languageLevel = languageLevel;
        }
        public int getXPathVersion() {
            return languageLevel;
        }
        public Saxon.Hej.expr.parser.CodeInjector getCodeInjector() {
            return codeInjector;
        }
        public Saxon.Hej.trans.KeyManager getKeyManager() {
            return packageData.getKeyManager();
        }
        public Saxon.Hej.type.ItemType resolveTypeAlias(Saxon.Hej.om.StructuredQName typeName) {
            return getPackageData().obtainTypeAliasManager().getItemType(typeName);
        }
        private class ActiveNamespace {
            public string prefix;
            public string uri;
        
        }
    
        private class Inner_NamespaceResolver_305 : Saxon.Hej.om.NamespaceResolver {
            private readonly QueryModule outerInstance;
            public Inner_NamespaceResolver_305(QueryModule outerInstance) {
                this.outerInstance = outerInstance;
            }
            public Saxon.Hej.om.NamespaceUri getURIForPrefix(string prefix, bool useDefault) {
                return outerInstance.checkURIForPrefix(/*localparam*/prefix);
            }
            public System.Collections.Generic.IEnumerator<string> iteratePrefixes() {
                return outerInstance.getNamespaceResolver().iteratePrefixes();
            }
        }

        /* Expanding default method getUnprefixedElementMatchingPolicy from interface StaticContext */
        public virtual Saxon.Api.UnprefixedElementMatchingPolicy getUnprefixedElementMatchingPolicy() {
            return Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace;
        }

    }
}