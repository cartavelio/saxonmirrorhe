using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class Compilation {
        public static bool TIMING = false;
        private readonly Saxon.Hej.Configuration config;
        private readonly Saxon.Hej.trans.CompilerInfo compilerInfo;
        private Saxon.Hej.style.PrincipalStylesheetModule principalStylesheetModule;
        private int errorCount = 0;
        private bool schemaAware;
        private readonly Saxon.Hej.om.QNameParser qNameParser;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.style.Compilation.ValueAndPrecedence> staticVariables = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.style.Compilation.ValueAndPrecedence>();
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.DocumentKey,Saxon.Hej.om.TreeInfo> stylesheetModules = new System.Collections.Generic.Dictionary<Saxon.Hej.om.DocumentKey, Saxon.Hej.om.TreeInfo>();
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.DocumentKey> importStack = new System.Collections.Generic.Stack<Saxon.Hej.om.DocumentKey>();
        private Saxon.Hej.expr.PackageData packageData;
        private bool preScan = true;
        private bool createsSecondaryResultDocuments = false;
        private bool libraryPackage = false;
        private Saxon.Hej.trans.packages.VersionedPackageName expectedNameAndVersion = null;
        private readonly System.Collections.Generic.IList<Saxon.Hej.trans.packages.UsePack> packageDependencies = new System.Collections.Generic.List<Saxon.Hej.trans.packages.UsePack>();
        private System.Collections.Generic.IList<Saxon.Hej.trans.packages.VersionedPackageName> usingPackages = new System.Collections.Generic.List<Saxon.Hej.trans.packages.VersionedPackageName>();
        private Saxon.Hej.expr.instruct.GlobalParameterSet suppliedParameters;
        private bool fallbackToNonStreaming = false;
        public Saxon.Hej.trans.Timer timer = null;
        private class ValueAndPrecedence {
            public ValueAndPrecedence(Saxon.Hej.om.GroundedValue v, Saxon.Hej.value.NestedIntegerValue p, bool isParam)  {
                this.value = v;
                this.precedence = p;
                this.isParam = isParam;
            }
            public Saxon.Hej.om.GroundedValue value;
            public Saxon.Hej.value.NestedIntegerValue precedence;
            public bool isParam;
        
        }
        public Compilation(Saxon.Hej.Configuration config, Saxon.Hej.trans.CompilerInfo info)  {
            this.config = config;
            this.compilerInfo = info;
            schemaAware = info.isSchemaAware();
            preScan = info.isJustInTimeCompilation();
            suppliedParameters = compilerInfo.getParameters();
            qNameParser = new Saxon.Hej.om.QNameParser(null).withAcceptEQName(true).withErrorOnBadSyntax("XTSE0020").withErrorOnUnresolvedPrefix("XTSE0280");
            if (TIMING) {
                timer = new Saxon.Hej.trans.Timer();
            }
        }
        public static Saxon.Hej.PreparedStylesheet compileSingletonPackage(Saxon.Hej.Configuration config, Saxon.Hej.trans.CompilerInfo compilerInfo, Saxon.Ejavax.xml.transform.Source source) {
            try {
                Saxon.Hej.style.Compilation compilation = new Saxon.Hej.style.Compilation(config, compilerInfo);
                return Saxon.Hej.style.StylesheetModule.loadStylesheet(source, compilation);
            } catch (Saxon.Hej.trans.XPathException err) {
                if (!(err.hasBeenReported())) {
                    compilerInfo.getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(err));
                }
                throw;
            }
        }
        public void setUsingPackages(System.Collections.Generic.IList<Saxon.Hej.trans.packages.VersionedPackageName> users) {
            this.usingPackages = users;
        }
        public void setPackageData(Saxon.Hej.expr.PackageData pack) {
            this.packageData = pack;
        }
        public void setMinimalPackageData() {
            if ((getPackageData() == null)) {
                Saxon.Hej.expr.PackageData pd = new Saxon.Hej.expr.PackageData(getConfiguration());
                pd.setHostLanguage(Saxon.Api.HostLanguage.XSLT, compilerInfo.getXsltVersion());
                pd.setTargetEdition(compilerInfo.getTargetEdition());
                pd.setSchemaAware(schemaAware);
                packageData = pd;
            }
        }
        public void setExpectedNameAndVersion(Saxon.Hej.trans.packages.VersionedPackageName vpn) {
            this.expectedNameAndVersion = vpn;
        }
        public void registerPackageDependency(Saxon.Hej.trans.packages.UsePack use) {
            packageDependencies.Add(use);
        }
        public void satisfyPackageDependencies(Saxon.Hej.style.XSLPackage thisPackage) {
            Saxon.Hej.trans.packages.PackageLibrary library = compilerInfo.getPackageLibrary();
            library.getCompilerInfo().setTargetEdition(compilerInfo.getTargetEdition());
            Saxon.Hej.trans.XPathException lastError = null;
            foreach (Saxon.Hej.trans.packages.UsePack use in packageDependencies) {
                Saxon.Hej.trans.packages.PackageDetails details = library.findPackage(use.packageName, use.ranges);
                if ((details == null)) {
                    throw new Saxon.Hej.trans.XPathException((((("Cannot find package "+use.packageName)+" (version ")+use.ranges)+")"), "XTSE3000", use.location);
                }
                if ((details.loadedPackage != null)) {
                    Saxon.Hej.style.StylesheetPackage used = details.loadedPackage;
                    Saxon.Hej.trans.packages.VersionedPackageName existing = new Saxon.Hej.trans.packages.VersionedPackageName(used.getPackageName(), used.getPackageVersion());
                    if (usingPackages.Contains(existing)) {
                        Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(1024);
                        foreach (Saxon.Hej.trans.packages.VersionedPackageName n in usingPackages) {
                            buffer.append(n.packageName);
                            buffer.append(", ");
                        }
                        buffer.append("and ");
                        buffer.append(thisPackage.getName());
                        throw new Saxon.Hej.trans.XPathException(("There is a cycle of package dependencies involving "+buffer), "XTSE3005");
                    }
                }
                try {
                    System.Collections.Generic.IList<Saxon.Hej.trans.packages.VersionedPackageName> disallowed = new System.Collections.Generic.List<Saxon.Hej.trans.packages.VersionedPackageName>(usingPackages);
                    disallowed.Add(details.nameAndVersion);
                    library.obtainLoadedPackage(details, disallowed);
                } catch (Saxon.Hej.trans.XPathException err) {
                    if (!(err.hasBeenReported())) {
                        reportError(err);
                    }
                    lastError = err;
                }
            }
            if ((lastError != null)) {
                throw lastError;
            }
        }
        public Saxon.Hej.style.PrincipalStylesheetModule compilePackage(Saxon.Ejavax.xml.transform.Source source) {
            setMinimalPackageData();
            Saxon.Hej.om.NodeInfo document;
            Saxon.Hej.om.NodeInfo outermost = null;
            if ((source is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo root = (Saxon.Hej.om.NodeInfo)(source);
                if ((root.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                    document = root;
                    outermost = ((/*1430*/Saxon.Hej.om.NodeInfo)document.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
                } else {
                    if ((root.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                        document = root.getRoot();
                        outermost = root;
                    }
                }
            }
            if (!(((outermost is Saxon.Hej.style.XSLPackage)))) {
                document = Saxon.Hej.style.StylesheetModule.loadStylesheetModule(source, true, this, Saxon.Hej.value.NestedIntegerValue.TWO);
                outermost = ((/*1430*/Saxon.Hej.om.NodeInfo)document.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
            }
            if ((outermost is Saxon.Hej.style.LiteralResultElement)) {
                document = ((Saxon.Hej.style.LiteralResultElement)(outermost)).makeStylesheet(true);
                outermost = ((/*1430*/Saxon.Hej.om.NodeInfo)document.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
            }
            Saxon.Hej.style.XSLPackage xslpackage;
            try {
                if ((outermost is Saxon.Hej.style.XSLPackage)) {
                    xslpackage = (Saxon.Hej.style.XSLPackage)(outermost);
                } else {
                    throw new Saxon.Hej.trans.XPathException("Outermost element must be xsl:package, xsl:stylesheet, or xsl:transform");
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    getCompilerInfo().getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                }
                throw;
            }
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("Built stylesheet documents");
            }
            Saxon.Hej.trans.CompilerInfo info = getCompilerInfo();
            Saxon.Hej.style.StyleNodeFactory factory = getStyleNodeFactory(true);
            Saxon.Hej.style.PrincipalStylesheetModule psm = factory.newPrincipalModule(xslpackage);
            Saxon.Hej.style.StylesheetPackage pack = psm.getStylesheetPackage();
            pack.setLanguageVersion(xslpackage.getVersion());
            pack.setPackageVersion(xslpackage.getPackageVersion());
            pack.setPackageName(xslpackage.getName());
            pack.setSchemaAware((info.isSchemaAware() || isSchemaAware()));
            pack.createFunctionLibrary();
            pack.setLanguageVersion(info.getXsltVersion());
            psm.getRuleManager().setCompilerInfo(info);
            setPrincipalStylesheetModule(psm);
            packageData = null;
            satisfyPackageDependencies(xslpackage);
            if (TIMING) {
                timer.report("Preparing package");
            }
            try {
                psm.preprocess(this);
            } catch (Saxon.Hej.trans.XPathException e) {
                info.getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                throw;
            }
            if ((getErrorCount() == 0)) {
                try {
                    psm.fixup();
                } catch (Saxon.Hej.trans.XPathException e) {
                    reportError(e);
                }
            }
            if (TIMING) {
                timer.report("Fixup");
            }
            if ((getErrorCount() == 0)) {
                try {
                    psm.combineAttributeSets(this);
                } catch (Saxon.Hej.trans.XPathException e) {
                    reportError(e);
                }
            }
            if (TIMING) {
                timer.report("Combine attribute sets");
            }
            if ((getErrorCount() == 0)) {
                try {
                    psm.compile(this);
                } catch (Saxon.Hej.trans.XPathException e) {
                    reportError(e);
                }
            }
            if ((getErrorCount() == 0)) {
                try {
                    psm.complete();
                } catch (Saxon.Hej.trans.XPathException e) {
                    reportError(e);
                }
            }
            if (TIMING) {
                timer.report("Completion");
            }
            psm.getStylesheetPackage().setCreatesSecondaryResultDocuments(createsSecondaryResultDocuments);
            if (isFallbackToNonStreaming()) {
                psm.getStylesheetPackage().setFallbackToNonStreaming();
            }
            if (TIMING) {
                timer.report("Streaming fallback");
            }
            return psm;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public Saxon.Hej.trans.CompilerInfo getCompilerInfo() {
            return compilerInfo;
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            if ((packageData != null)) {
                return packageData;
            }
            return ((principalStylesheetModule == null) ? null : principalStylesheetModule.getStylesheetPackage());
        }
        public bool isSchemaAware() {
            return schemaAware;
        }
        public void setSchemaAware(bool schemaAware) {
            this.schemaAware = schemaAware;
            getPackageData().setSchemaAware(schemaAware);
        }
        public Saxon.Hej.style.StyleNodeFactory getStyleNodeFactory(bool topLevel) {
            Saxon.Hej.style.StyleNodeFactory factory = getConfiguration().makeStyleNodeFactory(this);
            factory.setTopLevelModule(topLevel);
            return factory;
        }
        private void setPrincipalStylesheetModule(Saxon.Hej.style.PrincipalStylesheetModule module) {
            this.principalStylesheetModule = module;
        }
        public Saxon.Hej.style.PrincipalStylesheetModule getPrincipalStylesheetModule() {
            return principalStylesheetModule;
        }
        public void reportError(Saxon.Hej.s9api.XmlProcessingError err) {
            Saxon.Hej.lib.ErrorReporter reporter = compilerInfo.getErrorReporter();
            if ((reporter != null)) {
                reporter.report(err);
            }
            errorCount++;
            if ((err.getTerminationMessage() != null)) {
                throw new Saxon.Hej.trans.XmlProcessingAbort(err.getTerminationMessage());
            }
        }
        public void reportError(Saxon.Hej.trans.XPathException err) {
            err.setHostLanguage(Saxon.Api.HostLanguage.XSLT);
            Saxon.Hej.lib.ErrorReporter el = compilerInfo.getErrorReporter();
            if ((el == null)) {
                el = getConfiguration().makeErrorReporter();
            }
            if (!(err.hasBeenReported())) {
                errorCount++;
                try {
                    el.report(new Saxon.Hej.trans.XmlProcessingException(err));
                    err.setHasBeenReported(true);
                } catch (System.Exception err2) {
                }
            } else {
                if ((errorCount == 0)) {
                    errorCount++;
                }
            }
        }
        public int getErrorCount() {
            return errorCount;
        }
        public void reportWarning(Saxon.Hej.trans.XPathException err) {
            err.setHostLanguage(Saxon.Api.HostLanguage.XSLT);
            Saxon.Hej.lib.ErrorReporter reporter = compilerInfo.getErrorReporter();
            if ((reporter == null)) {
                reporter = getConfiguration().makeErrorReporter();
            }
            if ((reporter != null)) {
                Saxon.Hej.trans.XmlProcessingException error = new Saxon.Hej.trans.XmlProcessingException(err);
                error.setWarning(true);
                reporter.report(error);
            }
        }
        public void reportWarning(string message, string errorCode, Saxon.Hej.s9api.Location location) {
            Saxon.Hej.trans.XmlProcessingIncident error = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(message, errorCode, location).asWarning());
            error.setHostLanguage(Saxon.Api.HostLanguage.XSLT);
            compilerInfo.getErrorReporter().report(error);
        }
        public void declareStaticVariable(Saxon.Hej.om.StructuredQName name, Saxon.Hej.om.GroundedValue value, Saxon.Hej.value.NestedIntegerValue precedence, bool isParam) {
            Saxon.Hej.style.Compilation.ValueAndPrecedence vp = Saxon.Impl.Helpers.MapUtils.Get(staticVariables, name);
            if ((vp != null)) {
                if ((vp.precedence.compareTo(precedence)<0)) {
                    if (!(valuesAreCompatible(value, vp.value))) {
                        throw new Saxon.Hej.trans.XPathException(("Incompatible values assigned for static variable "+name.getDisplayName()), "XTSE3450");
                    }
                    if ((vp.isParam != isParam)) {
                        throw new Saxon.Hej.trans.XPathException((("Static variable "+name.getDisplayName())+" cannot be redeclared as a param"), "XTSE3450");
                    }
                } else {
                    return ;
                }
            }
            Saxon.Impl.Helpers.MapUtils.Put(staticVariables, name, new Saxon.Hej.style.Compilation.ValueAndPrecedence(value, precedence, isParam));
        }
        private bool valuesAreCompatible(Saxon.Hej.om.GroundedValue val0, Saxon.Hej.om.GroundedValue val1) {
            if ((val0.getLength() != val1.getLength())) {
                return false;
            }
            if ((val0.getLength() == 1)) {
                Saxon.Hej.om.Item i0 = val0.head();
                Saxon.Hej.om.Item i1 = val1.head();
                if ((i0 is Saxon.Hej.value.AtomicValue)) {
                    return ((i1 is Saxon.Hej.value.AtomicValue) && ((Saxon.Hej.value.AtomicValue)(i0)).isIdentical((Saxon.Hej.value.AtomicValue)(i1)));
                } else {
                    if ((i0 is Saxon.Hej.om.NodeInfo)) {
                        return ((i1 is Saxon.Hej.om.NodeInfo) && i0.Equals(i1));
                    } else {
                        return (i0 == i1);
                    }
                }
            } else {
                for (int i = 0; (i<val0.getLength()); i++) {
                    if (!(valuesAreCompatible(val0.itemAt(i), val1.itemAt(i)))) {
                        return false;
                    }
                }
                return true;
            }
        }
        public Saxon.Hej.om.GroundedValue getStaticVariable(Saxon.Hej.om.StructuredQName name) {
            Saxon.Hej.style.Compilation.ValueAndPrecedence vp = Saxon.Impl.Helpers.MapUtils.Get(staticVariables, name);
            return ((vp == null) ? null : vp.value);
        }
        public Saxon.Hej.value.NestedIntegerValue getStaticVariablePrecedence(Saxon.Hej.om.StructuredQName name) {
            Saxon.Hej.style.Compilation.ValueAndPrecedence vp = Saxon.Impl.Helpers.MapUtils.Get(staticVariables, name);
            return ((vp == null) ? null : vp.precedence);
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.om.DocumentKey,Saxon.Hej.om.TreeInfo> getStylesheetModules() {
            return stylesheetModules;
        }
        public System.Collections.Generic.Stack<Saxon.Hej.om.DocumentKey> getImportStack() {
            return importStack;
        }
        public Saxon.Hej.om.QNameParser getQNameParser() {
            return qNameParser;
        }
        public bool isPreScan() {
            return preScan;
        }
        public void setPreScan(bool preScan) {
            this.preScan = preScan;
        }
        public bool isCreatesSecondaryResultDocuments() {
            return createsSecondaryResultDocuments;
        }
        public void setCreatesSecondaryResultDocuments(bool createsSecondaryResultDocuments) {
            this.createsSecondaryResultDocuments = createsSecondaryResultDocuments;
        }
        public bool isLibraryPackage() {
            return libraryPackage;
        }
        public void setLibraryPackage(bool libraryPackage) {
            this.libraryPackage = libraryPackage;
        }
        public void setParameter(Saxon.Hej.om.StructuredQName name, Saxon.Hej.om.GroundedValue seq) {
            suppliedParameters.put(name, seq);
        }
        public Saxon.Hej.expr.instruct.GlobalParameterSet getParameters() {
            return suppliedParameters;
        }
        public void clearParameters() {
            suppliedParameters = new Saxon.Hej.expr.instruct.GlobalParameterSet();
        }
        public bool isFallbackToNonStreaming() {
            return fallbackToNonStreaming;
        }
        public void setFallbackToNonStreaming(bool fallbackToNonStreaming) {
            this.fallbackToNonStreaming = fallbackToNonStreaming;
        }
    
    }
}