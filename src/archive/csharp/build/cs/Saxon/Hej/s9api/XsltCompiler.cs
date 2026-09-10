using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XsltCompiler {
        private readonly Saxon.Hej.s9api.Processor processor;
        private readonly Saxon.Hej.Configuration config;
        private readonly Saxon.Hej.trans.CompilerInfo compilerInfo;
        protected internal XsltCompiler(Saxon.Hej.s9api.Processor processor)  {
            this.processor = processor;
            this.config = processor.getUnderlyingConfiguration();
            compilerInfo = new Saxon.Hej.trans.CompilerInfo(config.getDefaultXsltCompilerInfo());
            compilerInfo.setTargetEdition(config.getEditionCode());
            compilerInfo.setJustInTimeCompilation(config.isJITEnabled());
        }
        public Saxon.Hej.s9api.Processor getProcessor() {
            return processor;
        }
        public void setResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            compilerInfo.setResourceResolver(resolver);
        }
        public void setParameter(Saxon.Api.QName name, Saxon.Api.XdmValue value) {
            try {
                compilerInfo.setParameter(name.GetStructuredQName(), ((Saxon.Hej.om.Sequence)(((Saxon.Hej.om.GroundedValue)value.Unwrap()))).materialize());
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void clearParameters() {
            compilerInfo.clearParameters();
        }
        public Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver() {
            return compilerInfo.getResourceResolver();
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            compilerInfo.setErrorReporter(reporter);
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return compilerInfo.getErrorReporter();
        }
        public void setSchemaAware(bool schemaAware) {
            compilerInfo.setSchemaAware(schemaAware);
        }
        public bool isSchemaAware() {
            return compilerInfo.isSchemaAware();
        }
        public bool isRelocatable() {
            return compilerInfo.isRelocatable();
        }
        public void setRelocatable(bool relocatable) {
            compilerInfo.setRelocatable(relocatable);
        }
        public void setTargetEdition(string edition) {
            switch (edition) {
              case "EE":
              case "PE":
              case "HE":
              case "JS":
              case "JS2":
              case "JS3":
                compilerInfo.setTargetEdition(edition);
                return ;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown Saxon edition "+edition));
            }
        }
        public string getTargetEdition() {
            return compilerInfo.getTargetEdition();
        }
        public void declareDefaultCollation(string uri) {
            Saxon.Hej.lib.StringCollator c;
            try {
                c = getProcessor().getUnderlyingConfiguration().getCollation(uri);
            } catch (Saxon.Hej.trans.XPathException e) {
                c = null;
            }
            if ((c == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException(("Unknown collation "+uri));
            }
            compilerInfo.setDefaultCollation(uri);
        }
        public string getDefaultCollation() {
            return compilerInfo.getDefaultCollation();
        }
        public void setXsltLanguageVersion(string version) {
            switch (version) {
              case "3.0":
                compilerInfo.setXsltVersion(30);
                break ;
              case "4.0":
                compilerInfo.setXsltVersion(40);
                break ;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Language version must be 3.0|4.0");
            }
        }
        public string getXsltLanguageVersion() {
            return ((compilerInfo.getXsltVersion() == 40) ? "4.0" : "3.0");
        }
        public bool isAssertionsEnabled() {
            return compilerInfo.isAssertionsEnabled();
        }
        public void setAssertionsEnabled(bool enabled) {
            compilerInfo.setAssertionsEnabled(enabled);
        }
        public void setFastCompilation(bool fast) {
            if (fast) {
                compilerInfo.setOptimizerOptions(new Saxon.Hej.expr.parser.OptimizerOptions(Saxon.Hej.expr.parser.OptimizerOptions.JIT));
            } else {
                compilerInfo.setOptimizerOptions(getProcessor().getUnderlyingConfiguration().getOptimizerOptions());
            }
        }
        public bool isFastCompilation() {
            return (compilerInfo.getOptimizerOptions().getOptions() == Saxon.Hej.expr.parser.OptimizerOptions.JIT);
        }
        public void setCompileWithTracing(bool option) {
            if (option) {
                compilerInfo.setCodeInjector(new Saxon.Hej.trace.XSLTTraceCodeInjector());
                compilerInfo.setOptimizerOptions(compilerInfo.getOptimizerOptions().except(new Saxon.Hej.expr.parser.OptimizerOptions((((((Saxon.Hej.expr.parser.OptimizerOptions.COMMON_SUBEXPRESSIONS|Saxon.Hej.expr.parser.OptimizerOptions.CONSTANT_FOLDING)|Saxon.Hej.expr.parser.OptimizerOptions.INLINE_FUNCTIONS)|Saxon.Hej.expr.parser.OptimizerOptions.INLINE_VARIABLES)|Saxon.Hej.expr.parser.OptimizerOptions.LOOP_LIFTING)|Saxon.Hej.expr.parser.OptimizerOptions.EXTRACT_GLOBALS))));
            } else {
                compilerInfo.setCodeInjector(null);
            }
        }
        public bool isCompileWithTracing() {
            return compilerInfo.isCompileWithTracing();
        }
        public void setGenerateByteCode(bool option) {
        }
        public bool isGenerateByteCode() {
            return false;
        }
        public void importXQueryEnvironment(Saxon.Hej.s9api.XQueryCompiler queryCompiler) {
            compilerInfo.setXQueryLibraries(queryCompiler.getUnderlyingStaticContext().getCompiledLibraries());
        }
        public Saxon.Ejavax.xml.transform.Source getAssociatedStylesheet(Saxon.Ejavax.xml.transform.Source source, string media, string title, string charset) {
            throw new NotImplementedException();
        }
        public Saxon.Hej.s9api.XsltPackage compilePackage(Saxon.Ejavax.xml.transform.Source source) {
            try {
                Saxon.Hej.style.Compilation compilation = null;
                if (((source is Saxon.Hej.om.NodeSource) && (((Saxon.Hej.om.NodeSource)(source)).getNode() is Saxon.Hej.tree.linked.DocumentImpl))) {
                    Saxon.Hej.tree.linked.ElementImpl elem = ((Saxon.Hej.tree.linked.DocumentImpl)(((Saxon.Hej.om.NodeSource)(source)).getNode())).getDocumentElement();
                    if ((elem is Saxon.Hej.style.StyleElement)) {
                        compilation = ((Saxon.Hej.style.StyleElement)(elem)).getCompilation();
                    }
                }
                if ((compilation == null)) {
                    compilation = new Saxon.Hej.style.Compilation(config, new Saxon.Hej.trans.CompilerInfo(compilerInfo));
                }
                compilation.setLibraryPackage(true);
                Saxon.Hej.s9api.XsltPackage pack = new Saxon.Hej.s9api.XsltPackage(this, compilation.compilePackage(source).getStylesheetPackage());
                int errors = compilation.getErrorCount();
                if ((errors>0)) {
                    string count = ((errors == 1) ? "one error" : (errors+" errors"));
                    throw new Saxon.Api.SaxonApiException((("Package compilation failed: "+count)+" reported"));
                }
                return pack;
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e.getXPathException());
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.s9api.XsltPackage compilePackage(Saxon.Ejava.io.File file) {
            return compilePackage(new Saxon.Ejavax.xml.transform.stream.StreamSource(file));
        }
        private Saxon.Hej.trans.packages.PackageLibrary getPackageLibrary() {
            return compilerInfo.getPackageLibrary();
        }
        public Saxon.Hej.s9api.XsltPackage loadLibraryPackage(System.Uri location) {
            return loadLibraryPackage(new Saxon.Ejavax.xml.transform.stream.StreamSource(location.ToString()));
        }
        public Saxon.Hej.s9api.XsltPackage loadLibraryPackage(Saxon.Ejavax.xml.transform.Source input) {
            try {
                Saxon.Hej.trans.packages.IPackageLoader loader = processor.getUnderlyingConfiguration().makePackageLoader();
                if ((loader != null)) {
                    Saxon.Hej.style.StylesheetPackage pack = loader.loadPackage(input);
                    return new Saxon.Hej.s9api.XsltPackage(this, pack);
                }
                throw new Saxon.Api.SaxonApiException("Loading library package requires Saxon PE or higher");
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.s9api.XsltExecutable loadExecutablePackage(System.Uri location) {
            return loadLibraryPackage(location).link();
        }
        public Saxon.Hej.s9api.XsltExecutable loadExecutablePackage(Saxon.Ejavax.xml.transform.Source source) {
            return loadLibraryPackage(source).link();
        }
        public void importPackage(Saxon.Hej.s9api.XsltPackage thePackage) {
            if ((thePackage.getProcessor() != processor)) {
                throw new Saxon.Api.SaxonApiException("The imported package and the XsltCompiler must belong to the same Processor");
            }
            compilerInfo.getPackageLibrary().addPackage(thePackage.getUnderlyingPreparedPackage());
        }
        public void importPackage(Saxon.Hej.s9api.XsltPackage thePackage, string packageName, string version) {
            try {
                if ((thePackage.getProcessor() != processor)) {
                    throw new Saxon.Api.SaxonApiException("The imported package and the XsltCompiler must belong to the same Processor");
                }
                Saxon.Hej.trans.packages.PackageDetails details = new Saxon.Hej.trans.packages.PackageDetails();
                if ((packageName == null)) {
                    packageName = thePackage.getName();
                }
                if ((version == null)) {
                    version = thePackage.getVersion();
                }
                details.nameAndVersion = new Saxon.Hej.trans.packages.VersionedPackageName(packageName, version);
                details.loadedPackage = thePackage.getUnderlyingPreparedPackage();
                compilerInfo.getPackageLibrary().addPackage(details);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.s9api.XsltPackage obtainPackage(string packageName, string versionRange) {
            try {
                Saxon.Hej.style.PackageVersionRanges pvr = new Saxon.Hej.style.PackageVersionRanges(versionRange);
                Saxon.Hej.trans.packages.PackageDetails details = getPackageLibrary().findPackage(packageName, pvr);
                if ((details != null)) {
                    if ((details.loadedPackage != null)) {
                        return new Saxon.Hej.s9api.XsltPackage(this, details.loadedPackage);
                    } else {
                        if ((details.sourceLocation != null)) {
                            Saxon.Hej.s9api.XsltPackage pack = compilePackage(details.sourceLocation);
                            details.loadedPackage = pack.getUnderlyingPreparedPackage();
                            return pack;
                        }
                    }
                }
                return null;
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.s9api.XsltPackage obtainPackageWithAlias(string alias) {
            Saxon.Hej.trans.packages.PackageDetails details = getPackageLibrary().findDetailsForAlias(alias);
            if ((details == null)) {
                throw new Saxon.Api.SaxonApiException((("No package with alias "+alias)+" found in package library"));
            }
            try {
                System.Collections.Generic.IList<Saxon.Hej.trans.packages.VersionedPackageName> packageNames = new System.Collections.Generic.List<Saxon.Hej.trans.packages.VersionedPackageName>();
                Saxon.Hej.style.StylesheetPackage pack = getPackageLibrary().obtainLoadedPackage(details, packageNames);
                return new Saxon.Hej.s9api.XsltPackage(this, pack);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.s9api.XsltExecutable compile(Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Ejava.util.Objects.requireNonNull(source);
            try {
                Saxon.Hej.trans.CompilerInfo ci2 = new Saxon.Hej.trans.CompilerInfo(compilerInfo);
                Saxon.Hej.PreparedStylesheet pss = Saxon.Hej.style.Compilation.compileSingletonPackage(config, ci2, source);
                return new Saxon.Hej.s9api.XsltExecutable(processor, pss);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e.getXPathException());
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.s9api.XsltExecutable compile(Saxon.Ejava.io.File file) {
            return compile(new Saxon.Ejavax.xml.transform.stream.StreamSource(file));
        }
        public Saxon.Hej.trans.CompilerInfo getUnderlyingCompilerInfo() {
            return compilerInfo;
        }
        public void setJustInTimeCompilation(bool jit) {
            if ((jit && !(config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT)))) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("XSLT just-in-time compilation requires a Saxon-EE license");
            }
            compilerInfo.setJustInTimeCompilation(jit);
        }
        public bool isJustInTimeCompilation() {
            return compilerInfo.isJustInTimeCompilation();
        }
        public string getDefaultElementNamespace() {
            return compilerInfo.getDefaultElementNamespace().ToString();
        }
        public void setDefaultElementNamespace(string defaultNS) {
            compilerInfo.setDefaultElementNamespace(Saxon.Hej.om.NamespaceUri.of(defaultNS));
        }
        public Saxon.Api.UnprefixedElementMatchingPolicy getUnprefixedElementMatchingPolicy() {
            return compilerInfo.getUnprefixedElementMatchingPolicy();
        }
        public void setUnprefixedElementMatchingPolicy(Saxon.Api.UnprefixedElementMatchingPolicy unprefixedElementMatchingPolicy) {
            compilerInfo.setUnprefixedElementMatchingPolicy(unprefixedElementMatchingPolicy);
        }
    
    }
}