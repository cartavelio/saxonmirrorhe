using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class StaticQueryContext {
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.om.NamePool namePool;
        private string baseURI;
        private System.Collections.Generic.Dictionary<string,string> userDeclaredNamespaces;
        private System.Collections.Generic.ISet<Saxon.Hej.expr.instruct.GlobalVariable> userDeclaredVariables;
        private bool inheritNamespaces = true;
        private bool preserveNamespaces = true;
        private int constructionMode = Saxon.Hej.lib.Validation.PRESERVE;
        private string defaultFunctionNamespace = Saxon.Hej.lib.NamespaceConstant.FN;
        private string defaultElementNamespace = Saxon.Hej.lib.NamespaceConstant.NULL;
        private Saxon.Hej.type.ItemType requiredContextItemType = Saxon.Hej.type.AnyItemType.getInstance();
        private bool preserveSpace = false;
        private bool defaultEmptyLeast = true;
        private Saxon.Hej.lib.ModuleURIResolver moduleURIResolver;
        private Saxon.Hej.lib.ErrorReporter errorReporter = new Saxon.Hej.lib.StandardErrorReporter();
        private Saxon.Hej.expr.parser.CodeInjector codeInjector;
        private bool updating = false;
        private string defaultCollationName;
        private Saxon.Hej.s9api.Location moduleLocation;
        private Saxon.Hej.expr.parser.OptimizerOptions optimizerOptions;
        private int languageVersion = 31;
        protected internal StaticQueryContext()  {
        }
        public StaticQueryContext(Saxon.Hej.Configuration config) : this (config, true) {
        }
        public StaticQueryContext(Saxon.Hej.Configuration config, bool initialize)  {
            this.config = config;
            this.namePool = config.getNamePool();
            if (initialize) {
                copyFrom(config.getDefaultStaticQueryContext());
            } else {
                userDeclaredNamespaces = new System.Collections.Generic.Dictionary<string, string>();
                userDeclaredVariables = new System.Collections.Generic.HashSet<Saxon.Hej.expr.instruct.GlobalVariable>();
                optimizerOptions = config.getOptimizerOptions();
                clearNamespaces();
            }
        }
        public StaticQueryContext(Saxon.Hej.query.StaticQueryContext c)  {
            copyFrom(c);
        }
        public virtual void copyFrom(Saxon.Hej.query.StaticQueryContext c) {
            config = c.config;
            namePool = c.namePool;
            baseURI = c.baseURI;
            moduleURIResolver = c.moduleURIResolver;
            if ((c.userDeclaredNamespaces != null)) {
                userDeclaredNamespaces = new System.Collections.Generic.Dictionary<string, string>(c.userDeclaredNamespaces);
            }
            if ((c.userDeclaredVariables != null)) {
                userDeclaredVariables = new System.Collections.Generic.HashSet<Saxon.Hej.expr.instruct.GlobalVariable>(c.userDeclaredVariables);
            }
            inheritNamespaces = c.inheritNamespaces;
            preserveNamespaces = c.preserveNamespaces;
            constructionMode = c.constructionMode;
            defaultElementNamespace = c.defaultElementNamespace;
            defaultFunctionNamespace = c.defaultFunctionNamespace;
            requiredContextItemType = c.requiredContextItemType;
            preserveSpace = c.preserveSpace;
            defaultEmptyLeast = c.defaultEmptyLeast;
            moduleURIResolver = c.moduleURIResolver;
            errorReporter = c.errorReporter;
            codeInjector = c.codeInjector;
            updating = c.updating;
            optimizerOptions = c.optimizerOptions;
        }
        public void reset() {
            userDeclaredNamespaces = new System.Collections.Generic.Dictionary<string, string>(10);
            errorReporter = new Saxon.Hej.lib.StandardErrorReporter();
            constructionMode = (getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY) ? Saxon.Hej.lib.Validation.PRESERVE : Saxon.Hej.lib.Validation.STRIP);
            preserveSpace = false;
            defaultEmptyLeast = true;
            requiredContextItemType = Saxon.Hej.type.AnyItemType.getInstance();
            defaultFunctionNamespace = Saxon.Hej.lib.NamespaceConstant.FN;
            defaultElementNamespace = Saxon.Hej.lib.NamespaceConstant.NULL;
            moduleURIResolver = null;
            defaultCollationName = config.getDefaultCollationName();
            clearNamespaces();
            updating = false;
            optimizerOptions = config.getOptimizerOptions();
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            if (((this.config != null) && (this.config != config))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Configuration cannot be changed dynamically");
            }
            this.config = config;
            namePool = config.getNamePool();
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public Saxon.Hej.expr.instruct.Executable makeExecutable() {
            Saxon.Hej.expr.instruct.Executable executable = new Saxon.Hej.expr.instruct.Executable(config);
            executable.setSchemaAware(isSchemaAware());
            executable.setHostLanguage(Saxon.Api.HostLanguage.XQUERY);
            return executable;
        }
        public virtual void setSchemaAware(bool aware) {
            if (aware) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Schema-awareness requires Saxon-EE");
            }
        }
        public virtual bool isSchemaAware() {
            return false;
        }
        public virtual void setStreaming(bool option) {
            if (option) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Streaming requires Saxon-EE");
            }
        }
        public virtual bool isStreaming() {
            return false;
        }
        public void setBaseURI(string baseURI) {
            this.baseURI = baseURI;
        }
        public void setLanguageVersion(int version) {
            if ((((version == 10) || (version == 30)) || (version == 31))) {
            } else {
                if ((version == 40)) {
                    languageVersion = 40;
                } else {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("languageVersion = "+version));
                }
            }
        }
        public int getLanguageVersion() {
            return languageVersion;
        }
        public virtual Saxon.Hej.functions.FunctionLibrary getExtensionFunctionLibrary() {
            return null;
        }
        public bool isCompileWithTracing() {
            return (codeInjector is Saxon.Hej.trace.TraceCodeInjector);
        }
        public void setCompileWithTracing(bool trace) {
            if (trace) {
                codeInjector = new Saxon.Hej.trace.TraceCodeInjector();
            } else {
                codeInjector = null;
            }
        }
        public void setCodeInjector(Saxon.Hej.expr.parser.CodeInjector injector) {
            this.codeInjector = injector;
        }
        public Saxon.Hej.expr.parser.CodeInjector getCodeInjector() {
            return codeInjector;
        }
        public bool isUpdating() {
            return updating;
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
        public void setModuleLocation(Saxon.Hej.s9api.Location location) {
            this.moduleLocation = location;
        }
        public Saxon.Hej.s9api.Location getModuleLocation() {
            return moduleLocation;
        }
        public void setOptimizerOptions(Saxon.Hej.expr.parser.OptimizerOptions options) {
            this.optimizerOptions = options;
        }
        public Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions() {
            return this.optimizerOptions;
        }
        public Saxon.Hej.query.XQueryExpression compileQuery(string query) {
            Saxon.Hej.query.XQueryParser qp = (Saxon.Hej.query.XQueryParser)(config.newExpressionParser("XQ", updating, languageVersion));
            if ((codeInjector != null)) {
                qp.setCodeInjector(codeInjector);
            } else {
                if (config.isCompileWithTracing()) {
                    qp.setCodeInjector(new Saxon.Hej.trace.TraceCodeInjector());
                }
            }
            qp.setStreaming(isStreaming());
            Saxon.Hej.query.QueryModule mainModule = new Saxon.Hej.query.QueryModule(this);
            return qp.makeXQueryExpression(query, mainModule, config);
        }
        public Saxon.Hej.query.XQueryExpression compileQuery(System.IO.TextReader source) { lock(this) {
            char[] buffer = new char[4096];
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(4096);
            while (true) {
                int n = source.Read(buffer);
                if ((n>0)) {
                    sb.append(buffer, 0, n);
                } else {
                    break ;
                }
            }
            return compileQuery(sb.ToString());
        }}
        public Saxon.Hej.query.XQueryExpression compileQuery(System.IO.Stream source, string encoding) { lock(this) {
            try {
                string query = Saxon.Hej.query.QueryReader.readInputStream(source, encoding, config.getValidCharacterChecker());
                return compileQuery(query);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }}
        public virtual void compileLibrary(string query) {
            throw new Saxon.Hej.trans.XPathException("Separate compilation of query libraries requires Saxon-EE");
        }
        public virtual void compileLibrary(System.IO.TextReader source) {
            throw new Saxon.Hej.trans.XPathException("Separate compilation of query libraries requires Saxon-EE");
        }
        public virtual void compileLibrary(System.IO.Stream source, string encoding) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Separate compilation of query libraries requires Saxon-EE");
        }
        public virtual Saxon.Hej.query.QueryLibrary getCompiledLibrary(string @namespace) {
            return null;
        }
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.query.QueryLibrary> getCompiledLibraries() {
            return new System.Collections.Generic.HashSet<Saxon.Hej.query.QueryLibrary>();
        }
        public void declareNamespace(string prefix, string uri) {
            if ((prefix == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("Null prefix supplied to declareNamespace()");
            }
            if ((uri == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("Null namespace URI supplied to declareNamespace()");
            }
            if ((prefix.Equals("xml") != uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Misdeclaration of XML namespace");
            }
            if ((prefix.Equals("xmlns") || uri.Equals(Saxon.Hej.lib.NamespaceConstant.XMLNS))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Misdeclaration of xmlns namespace");
            }
            if (prefix.Length==0) {
                defaultElementNamespace = uri;
            }
            if (uri.Length==0) {
                userDeclaredNamespaces.Remove(prefix);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(userDeclaredNamespaces, prefix, uri);
            }
        }
        public void clearNamespaces() {
            userDeclaredNamespaces.Clear();
            declareNamespace("xml", Saxon.Hej.lib.NamespaceConstant.XML);
            declareNamespace("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA);
            declareNamespace("xsi", Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE);
            declareNamespace("fn", Saxon.Hej.lib.NamespaceConstant.FN);
            declareNamespace("local", Saxon.Hej.lib.NamespaceConstant.LOCAL);
            declareNamespace("err", Saxon.Hej.lib.NamespaceConstant.ERR);
            declareNamespace("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON);
            declareNamespace("", "");
        }
        protected internal System.Collections.Generic.Dictionary<string,string> getUserDeclaredNamespaces() {
            return userDeclaredNamespaces;
        }
        public System.Collections.Generic.IEnumerator<string> iterateDeclaredPrefixes() {
            return Saxon.Impl.Helpers.MapUtils.KeySet(userDeclaredNamespaces).GetEnumerator();
        }
        public string getNamespaceForPrefix(string prefix) {
            return Saxon.Impl.Helpers.MapUtils.Get(userDeclaredNamespaces, prefix);
        }
        public string getDefaultFunctionNamespace() {
            return defaultFunctionNamespace;
        }
        public void setDefaultFunctionNamespace(string defaultFunctionNamespace) {
            this.defaultFunctionNamespace = defaultFunctionNamespace;
        }
        public void setDefaultElementNamespace(string uri) {
            defaultElementNamespace = uri;
            declareNamespace("", uri);
        }
        public string getDefaultElementNamespace() {
            return defaultElementNamespace;
        }
        public void declareGlobalVariable(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.value.SequenceType type, Saxon.Hej.om.GroundedValue value, bool external) {
            if (((value == null) && !(external))) {
                throw new Saxon.Ejava.lang.NullPointerException("No initial value for declared variable");
            }
            if (((value != null) && !(type.matches(value, getConfiguration().getTypeHierarchy())))) {
                throw new Saxon.Hej.trans.XPathException("Value of declared variable does not match its type");
            }
            Saxon.Hej.expr.instruct.GlobalVariable var = (external ? new Saxon.Hej.expr.instruct.GlobalParam() : new Saxon.Hej.expr.instruct.GlobalVariable());
            var.setVariableQName(qName);
            var.setRequiredType(type);
            if ((value != null)) {
                var.setBody(Saxon.Hej.expr.Literal.makeLiteral(value.materialize()));
            }
            if ((userDeclaredVariables == null)) {
                userDeclaredVariables = new System.Collections.Generic.HashSet<Saxon.Hej.expr.instruct.GlobalVariable>();
            }
            userDeclaredVariables.Add(var);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.instruct.GlobalVariable> iterateDeclaredGlobalVariables() {
            if ((userDeclaredVariables == null)) {
                return new System.Collections.Generic.List<Saxon.Hej.expr.instruct.GlobalVariable>();
            } else {
                return userDeclaredVariables;
            }
        }
        public void clearDeclaredGlobalVariables() {
            userDeclaredVariables = null;
        }
        public void setModuleURIResolver(Saxon.Hej.lib.ModuleURIResolver resolver) {
            moduleURIResolver = resolver;
        }
        public Saxon.Hej.lib.ModuleURIResolver getModuleURIResolver() {
            return moduleURIResolver;
        }
        public void declareDefaultCollation(string name) {
            if ((name == null)) {
                throw new Saxon.Ejava.lang.NullPointerException();
            }
            Saxon.Hej.lib.StringCollator c;
            try {
                c = getConfiguration().getCollation(name);
            } catch (Saxon.Hej.trans.XPathException e) {
                c = null;
            }
            if ((c == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException(("Unknown collation "+name));
            }
            this.defaultCollationName = name;
        }
        public string getDefaultCollationName() {
            return defaultCollationName;
        }
        public void setRequiredContextItemType(Saxon.Hej.type.ItemType type) {
            requiredContextItemType = type;
        }
        public Saxon.Hej.type.ItemType getRequiredContextItemType() {
            return requiredContextItemType;
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return namePool;
        }
        public string getSystemId() {
            return baseURI;
        }
        public string getBaseURI() {
            return baseURI;
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
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            this.errorReporter = reporter;
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return this.errorReporter;
        }
        public void setUpdatingEnabled(bool updating) {
            this.updating = updating;
        }
        public bool isUpdatingEnabled() {
            return updating;
        }
    
    }
}