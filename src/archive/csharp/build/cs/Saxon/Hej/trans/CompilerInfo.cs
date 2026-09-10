using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class CompilerInfo {
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.lib.JJ_ResourceResolver resourceResolver;
        private Saxon.Hej.lib.ErrorReporter errorReporter = new Saxon.Hej.lib.StandardErrorReporter();
        private Saxon.Hej.expr.parser.CodeInjector codeInjector;
        private int recoveryPolicy = Saxon.Hej.trans.Mode.RECOVER_WITH_WARNINGS;
        private bool schemaAware;
        private Saxon.Hej.om.StructuredQName defaultInitialMode;
        private Saxon.Hej.om.StructuredQName defaultInitialTemplate;
        private Saxon.Hej.expr.instruct.GlobalParameterSet suppliedParameters = new Saxon.Hej.expr.instruct.GlobalParameterSet();
        private string defaultCollation;
        private Saxon.Hej.trans.packages.PackageLibrary packageLibrary;
        private bool assertionsEnabled = false;
        private string targetEdition = "HE";
        private bool relocatable = false;
        private System.Collections.Generic.IEnumerable<Saxon.Hej.query.QueryLibrary> queryLibraries;
        private Saxon.Hej.expr.parser.OptimizerOptions optimizerOptions;
        private string defaultNamespaceForElementsAndTypes = "";
        private Saxon.Api.UnprefixedElementMatchingPolicy unprefixedElementMatchingPolicy = Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace;
        private int languageVersion = 30;
        public CompilerInfo(Saxon.Hej.Configuration config)  {
            this.config = config;
            errorReporter = config.makeErrorReporter();
            packageLibrary = new Saxon.Hej.trans.packages.PackageLibrary(this);
            optimizerOptions = config.getOptimizerOptions();
        }
        public CompilerInfo(Saxon.Hej.trans.CompilerInfo info)  {
            copyFrom(info);
        }
        public void copyFrom(Saxon.Hej.trans.CompilerInfo info) {
            config = info.config;
            resourceResolver = info.resourceResolver;
            errorReporter = info.errorReporter;
            codeInjector = info.codeInjector;
            recoveryPolicy = info.recoveryPolicy;
            schemaAware = info.schemaAware;
            defaultInitialMode = info.defaultInitialMode;
            defaultInitialTemplate = info.defaultInitialTemplate;
            suppliedParameters = new Saxon.Hej.expr.instruct.GlobalParameterSet(info.suppliedParameters);
            defaultCollation = info.defaultCollation;
            assertionsEnabled = info.assertionsEnabled;
            targetEdition = info.targetEdition;
            packageLibrary = new Saxon.Hej.trans.packages.PackageLibrary(info.packageLibrary);
            relocatable = info.relocatable;
            optimizerOptions = info.optimizerOptions;
            queryLibraries = info.queryLibraries;
            defaultNamespaceForElementsAndTypes = info.defaultNamespaceForElementsAndTypes;
            unprefixedElementMatchingPolicy = info.unprefixedElementMatchingPolicy;
            languageVersion = info.languageVersion;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void setJustInTimeCompilation(bool jit) {
            if (jit) {
                optimizerOptions = optimizerOptions.union(new Saxon.Hej.expr.parser.OptimizerOptions(Saxon.Hej.expr.parser.OptimizerOptions.JIT));
            } else {
                optimizerOptions = optimizerOptions.except(new Saxon.Hej.expr.parser.OptimizerOptions(Saxon.Hej.expr.parser.OptimizerOptions.JIT));
            }
        }
        public bool isJustInTimeCompilation() {
            return optimizerOptions.isSet(Saxon.Hej.expr.parser.OptimizerOptions.JIT);
        }
        public void setURIResolver(Saxon.Ejavax.xml.transform.URIResolver resolver) {
            resourceResolver = new Saxon.Hej.lib.ResourceResolverWrappingURIResolver(resolver);
        }
        public void setResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            resourceResolver = resolver;
        }
        public void setParameter(Saxon.Hej.om.StructuredQName name, Saxon.Hej.om.GroundedValue seq) {
            suppliedParameters.put(name, seq);
        }
        public Saxon.Hej.expr.instruct.GlobalParameterSet getParameters() {
            return suppliedParameters;
        }
        public void clearParameters() {
            suppliedParameters.clear();
        }
        public void setTargetEdition(string edition) {
            this.targetEdition = edition;
        }
        public string getTargetEdition() {
            return targetEdition;
        }
        public bool isRelocatable() {
            return relocatable;
        }
        public void setRelocatable(bool relocatable) {
            this.relocatable = relocatable;
        }
        public void setPackageLibrary(Saxon.Hej.trans.packages.PackageLibrary library) {
            packageLibrary = library;
        }
        public Saxon.Hej.trans.packages.PackageLibrary getPackageLibrary() {
            return packageLibrary;
        }
        public bool isAssertionsEnabled() {
            return assertionsEnabled;
        }
        public void setAssertionsEnabled(bool enabled) {
            this.assertionsEnabled = enabled;
        }
        public void setOptimizerOptions(Saxon.Hej.expr.parser.OptimizerOptions options) {
            this.optimizerOptions = options;
        }
        public Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions() {
            return this.optimizerOptions;
        }
        public void setGenerateByteCode(bool option) {
            if (option) {
                optimizerOptions = optimizerOptions.union(new Saxon.Hej.expr.parser.OptimizerOptions(Saxon.Hej.expr.parser.OptimizerOptions.BYTE_CODE));
            } else {
                optimizerOptions = optimizerOptions.except(new Saxon.Hej.expr.parser.OptimizerOptions(Saxon.Hej.expr.parser.OptimizerOptions.BYTE_CODE));
            }
        }
        public bool isGenerateByteCode() {
            return optimizerOptions.isSet(Saxon.Hej.expr.parser.OptimizerOptions.BYTE_CODE);
        }
        public Saxon.Ejavax.xml.transform.URIResolver getURIResolver() {
            if ((resourceResolver is Saxon.Hej.lib.ResourceResolverWrappingURIResolver)) {
                return ((Saxon.Hej.lib.ResourceResolverWrappingURIResolver)(resourceResolver)).getWrappedURIResolver();
            } else {
                return null;
            }
        }
        public Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver() {
            return resourceResolver;
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            this.errorReporter = reporter;
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return this.errorReporter;
        }
        public void setDefaultCollation(string collation) {
            this.defaultCollation = collation;
        }
        public string getDefaultCollation() {
            return this.defaultCollation;
        }
        public void setCodeInjector(Saxon.Hej.expr.parser.CodeInjector injector) {
            codeInjector = injector;
        }
        public Saxon.Hej.expr.parser.CodeInjector getCodeInjector() {
            return codeInjector;
        }
        public bool isCompileWithTracing() {
            return (codeInjector != null);
        }
        public void setSchemaAware(bool schemaAware) {
            this.schemaAware = schemaAware;
        }
        public bool isSchemaAware() {
            return schemaAware;
        }
        public void setDefaultInitialTemplate(Saxon.Hej.om.StructuredQName initialTemplate) {
            defaultInitialTemplate = initialTemplate;
        }
        public Saxon.Hej.om.StructuredQName getDefaultInitialTemplate() {
            return defaultInitialTemplate;
        }
        public void setDefaultInitialMode(Saxon.Hej.om.StructuredQName initialMode) {
            defaultInitialMode = initialMode;
        }
        public Saxon.Hej.om.StructuredQName getDefaultInitialMode() {
            return defaultInitialMode;
        }
        public void setXsltVersion(int version) {
            this.languageVersion = version;
        }
        public int getXsltVersion() {
            return languageVersion;
        }
        public string getDefaultElementNamespace() {
            return defaultNamespaceForElementsAndTypes;
        }
        public void setDefaultElementNamespace(string defaultNamespaceForElementsAndTypes) {
            this.defaultNamespaceForElementsAndTypes = defaultNamespaceForElementsAndTypes;
        }
        public Saxon.Api.UnprefixedElementMatchingPolicy getUnprefixedElementMatchingPolicy() {
            return unprefixedElementMatchingPolicy;
        }
        public void setUnprefixedElementMatchingPolicy(Saxon.Api.UnprefixedElementMatchingPolicy unprefixedElementMatchingPolicy) {
            this.unprefixedElementMatchingPolicy = unprefixedElementMatchingPolicy;
        }
        public void setXQueryLibraries(System.Collections.Generic.IEnumerable<Saxon.Hej.query.QueryLibrary> libraries) {
            this.queryLibraries = libraries;
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.query.QueryLibrary> getQueryLibraries() {
            return queryLibraries;
        }
    
    }
}