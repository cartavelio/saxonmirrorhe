using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class Executable {
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.expr.PackageData topLevelPackage;
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.PackageData> packages = new System.Collections.Generic.List<Saxon.Hej.expr.PackageData>();
        private Saxon.Ejava.util.Properties defaultOutputProperties;
        private Saxon.Hej.serialize.CharacterMapIndex characterMapIndex;
        private System.Collections.Generic.Dictionary<string,System.Collections.Generic.IList<Saxon.Hej.query.QueryModule>> queryLibraryModules;
        private System.Collections.Generic.HashSet<string> queryLocationHintsProcessed;
        private Saxon.Hej.functions.FunctionLibraryList functionLibrary;
        private Saxon.Api.HostLanguage hostLanguage = Saxon.Api.HostLanguage.XSLT;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.GlobalParam> globalParams = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.instruct.GlobalParam>();
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Ejava.util.Properties> outputDeclarations = null;
        private bool _createsSecondaryResult = false;
        protected internal bool schemaAware = false;
        private Saxon.Hej.expr.instruct.GlobalContextRequirement globalContextRequirement = null;
        public Executable(Saxon.Hej.Configuration config)  {
            setConfiguration(config);
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public virtual Saxon.Hej.expr.PackageData getTopLevelPackage() {
            return topLevelPackage;
        }
        public void setTopLevelPackage(Saxon.Hej.expr.PackageData topLevelPackage) {
            this.topLevelPackage = topLevelPackage;
        }
        public void addPackage(Saxon.Hej.expr.PackageData data) {
            packages.Add(data);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.PackageData> getPackages() {
            return packages;
        }
        public void setHostLanguage(Saxon.Api.HostLanguage language) {
            hostLanguage = language;
        }
        public Saxon.Api.HostLanguage getHostLanguage() {
            return hostLanguage;
        }
        public Saxon.Hej.functions.FunctionLibraryList getFunctionLibrary() {
            return functionLibrary;
        }
        public void setFunctionLibrary(Saxon.Hej.functions.FunctionLibraryList functionLibrary) {
            this.functionLibrary = functionLibrary;
        }
        public void setCharacterMapIndex(Saxon.Hej.serialize.CharacterMapIndex cmi) {
            characterMapIndex = cmi;
        }
        public Saxon.Hej.serialize.CharacterMapIndex getCharacterMapIndex() {
            if ((characterMapIndex == null)) {
                characterMapIndex = new Saxon.Hej.serialize.CharacterMapIndex();
            }
            return characterMapIndex;
        }
        public void setDefaultOutputProperties(Saxon.Ejava.util.Properties properties) {
            defaultOutputProperties = properties;
        }
        public Saxon.Hej.serialize.SerializationProperties getPrimarySerializationProperties() {
            if ((defaultOutputProperties == null)) {
                defaultOutputProperties = new Saxon.Ejava.util.Properties();
            }
            Saxon.Ejava.util.Properties props = defaultOutputProperties;
            return new Saxon.Hej.serialize.SerializationProperties(props, getCharacterMapIndex());
        }
        public void setOutputProperties(Saxon.Hej.om.StructuredQName qName, Saxon.Ejava.util.Properties properties) {
            if ((outputDeclarations == null)) {
                outputDeclarations = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Ejava.util.Properties>(5);
            }
            Saxon.Impl.Helpers.MapUtils.Put(outputDeclarations, qName, properties);
        }
        public Saxon.Ejava.util.Properties getOutputProperties() {
            return new Saxon.Ejava.util.Properties(defaultOutputProperties);
        }
        public Saxon.Ejava.util.Properties getOutputProperties(Saxon.Hej.om.StructuredQName qName) {
            if ((outputDeclarations == null)) {
                return null;
            } else {
                return Saxon.Impl.Helpers.MapUtils.Get(outputDeclarations, qName);
            }
        }
        public void addQueryLibraryModule(Saxon.Hej.query.QueryModule module) {
            if ((queryLibraryModules == null)) {
                queryLibraryModules = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IList<Saxon.Hej.query.QueryModule>>(5);
            }
            string uri = module.getModuleNamespace();
            System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> existing = Saxon.Impl.Helpers.MapUtils.Get(queryLibraryModules, uri);
            if ((existing == null)) {
                existing = new System.Collections.Generic.List<Saxon.Hej.query.QueryModule>(5);
                existing.Add(module);
                Saxon.Impl.Helpers.MapUtils.Put(queryLibraryModules, uri, existing);
            } else {
                if (!(existing.Contains(module))) {
                    existing.Add(module);
                }
            }
        }
        public System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> getQueryLibraryModules(string @namespace) {
            if ((queryLibraryModules == null)) {
                return null;
            }
            return Saxon.Impl.Helpers.MapUtils.Get(queryLibraryModules, @namespace);
        }
        public Saxon.Hej.query.QueryModule getQueryModuleWithSystemId(string systemId, Saxon.Hej.query.QueryModule topModule) {
            if (systemId.Equals(topModule.getSystemId())) {
                return topModule;
            }
            foreach (Saxon.Hej.query.QueryModule sqc in getQueryLibraryModules()) {
                string uri = sqc.getSystemId();
                if (((uri != null) && uri.Equals(systemId))) {
                    return sqc;
                }
            }
            return null;
        }
        public System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> getQueryLibraryModules() {
            if ((queryLibraryModules == null)) {
                return new System.Collections.Generic.List<Saxon.Hej.query.QueryModule>();
            } else {
                System.Collections.Generic.List<Saxon.Hej.query.QueryModule> modules = new System.Collections.Generic.List<Saxon.Hej.query.QueryModule>();
                foreach (System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> queryModules in queryLibraryModules.Values) {
                    Saxon.Impl.Helpers.ListUtils.AddAll(modules, queryModules);
                }
                return modules;
            }
        }
        public void addQueryLocationHintProcessed(string uri) {
            if ((queryLocationHintsProcessed == null)) {
                queryLocationHintsProcessed = new System.Collections.Generic.HashSet<string>();
            }
            queryLocationHintsProcessed.Add(uri);
        }
        public bool isQueryLocationHintProcessed(string uri) {
            return ((queryLocationHintsProcessed != null) && queryLocationHintsProcessed.Contains(uri));
        }
        public void fixupQueryModules(Saxon.Hej.query.QueryModule main) {
            main.bindUnboundVariables();
            if ((queryLibraryModules != null)) {
                foreach (System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> queryModules in queryLibraryModules.Values) {
                    foreach (Saxon.Hej.query.QueryModule env in queryModules) {
                        env.bindUnboundVariables();
                    }
                }
            }
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.GlobalVariable> varDefinitions = main.fixupGlobalVariables(main.getGlobalStackFrameMap());
            main.bindUnboundFunctionCalls();
            if ((queryLibraryModules != null)) {
                foreach (System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> queryModules in queryLibraryModules.Values) {
                    foreach (Saxon.Hej.query.QueryModule env in queryModules) {
                        env.bindUnboundFunctionCalls();
                    }
                }
            }
            main.checkForCircularities(varDefinitions, main.getGlobalFunctionLibrary());
            main.fixupGlobalFunctions();
            main.typeCheckGlobalVariables(varDefinitions);
            main.optimizeGlobalFunctions();
        }
        public void explainGlobalVariables(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("globalVariables");
            foreach (Saxon.Hej.expr.PackageData pack in getPackages()) {
                foreach (Saxon.Hej.expr.instruct.GlobalVariable var in pack.getGlobalVariableList()) {
                    var.export(presenter);
                }
            }
            presenter.endElement();
        }
        public void registerGlobalParameter(Saxon.Hej.expr.instruct.GlobalParam param) {
            Saxon.Impl.Helpers.MapUtils.Put(globalParams, param.getVariableQName(), param);
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.GlobalParam> getGlobalParameters() {
            return globalParams;
        }
        public Saxon.Hej.expr.instruct.GlobalParam getGlobalParameter(Saxon.Hej.om.StructuredQName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(globalParams, name);
        }
        public virtual void checkSuppliedParameters(Saxon.Hej.expr.instruct.GlobalParameterSet @params) {
        }
        public void setCreatesSecondaryResult(bool flag) {
            _createsSecondaryResult = flag;
        }
        public bool createsSecondaryResult() {
            return _createsSecondaryResult;
        }
        public void setGlobalContextRequirement(Saxon.Hej.expr.instruct.GlobalContextRequirement requirement) {
            globalContextRequirement = requirement;
        }
        public Saxon.Hej.expr.instruct.GlobalContextRequirement getGlobalContextRequirement() {
            return globalContextRequirement;
        }
        public Saxon.Hej.om.Item checkInitialContextItem(Saxon.Hej.om.Item contextItem, Saxon.Hej.expr.XPathContext context) {
            if ((globalContextRequirement == null)) {
                return contextItem;
            }
            if (((contextItem != null) && globalContextRequirement.isAbsentFocus())) {
                throw new Saxon.Hej.trans.XPathException("The global context item is required to be absent", "XPDY0002");
            }
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if ((contextItem == null)) {
                if (!(globalContextRequirement.isMayBeOmitted())) {
                    throw new Saxon.Hej.trans.XPathException("A global context item is required, but none has been supplied", "XTDE3086");
                }
                if ((globalContextRequirement.getDefaultValue() != null)) {
                    try {
                        contextItem = globalContextRequirement.getDefaultValue().evaluateItem(context);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        if ("XPDY0002".Equals(e.getErrorCodeLocalPart())) {
                            if ((e.Message.Contains("last()") || e.Message.Contains("position()"))) {
                            } else {
                                e.setErrorCode("XQDY0054");
                            }
                        }
                        throw;
                    }
                    if ((contextItem == null)) {
                        throw new Saxon.Hej.trans.XPathException("The context item cannot be initialized to an empty sequence", "XPTY0004");
                    }
                    foreach (Saxon.Hej.type.ItemType type in globalContextRequirement.getRequiredItemTypes()) {
                        if (!(type.matches(contextItem, th))) {
                            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "defaulted global context item", 0);
                            string s = role.composeErrorMessage(type, contextItem, th);
                            throw new Saxon.Hej.trans.XPathException(s, "XPTY0004");
                        }
                    }
                }
            } else {
                foreach (Saxon.Hej.type.ItemType type in globalContextRequirement.getRequiredItemTypes()) {
                    if (!(type.matches(contextItem, config.getTypeHierarchy()))) {
                        Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "supplied global context item", 0);
                        string s = role.composeErrorMessage(type, contextItem, th);
                        throw new Saxon.Hej.trans.XPathException(s, ((getHostLanguage() == Saxon.Api.HostLanguage.XSLT) ? "XTTE0590" : "XPTY0004"));
                    }
                }
            }
            return contextItem;
        }
        public void setSchemaAware(bool aware) {
            schemaAware = aware;
        }
        public bool isSchemaAware() {
            return schemaAware;
        }
    
    }
}