using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class UseWhenStaticContext : Saxon.Hej.sxpath.AbstractStaticContext, Saxon.Hej.expr.StaticContext {
        private readonly Saxon.Hej.om.NamespaceResolver namespaceContext;
        private readonly Saxon.Hej.functions.FunctionLibrary functionLibrary;
        private readonly Saxon.Hej.style.Compilation compilation;
        public UseWhenStaticContext(Saxon.Hej.style.Compilation compilation, Saxon.Hej.om.NamespaceResolver namespaceContext)  {
            Saxon.Hej.Configuration config = compilation.getConfiguration();
            setConfiguration(config);
            this.compilation = compilation;
            setPackageData(compilation.getPackageData());
            this.namespaceContext = namespaceContext;
            setXPathLanguageLevel(31);
            Saxon.Hej.functions.FunctionLibraryList lib = new Saxon.Hej.functions.FunctionLibraryList();
            lib.addFunctionLibrary(Saxon.Hej.functions.registry.UseWhen30FunctionSet.getInstance());
            lib.addFunctionLibrary(getConfiguration().getBuiltInExtensionLibraryList(31));
            lib.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(getConfiguration()));
            lib.addFunctionLibrary(config.getIntegratedFunctionLibrary());
            config.addExtensionBinders(lib);
            functionLibrary = lib;
        }
        public override Saxon.Hej.expr.parser.RetainedStaticContext makeRetainedStaticContext() {
            return new Saxon.Hej.expr.parser.RetainedStaticContext(this);
        }
        public Saxon.Hej.style.Compilation getCompilation() {
            return compilation;
        }
        public override void issueWarning(string s, string errorCode, Saxon.Hej.s9api.Location locator) {
            compilation.getCompilerInfo().getErrorReporter().report(((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(s, errorCode, locator).asWarning()));
        }
        public override string getSystemId() {
            return getStaticBaseURI();
        }
        public override Saxon.Hej.expr.Expression bindVariable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.om.GroundedValue val = compilation.getStaticVariable(qName);
            if ((val != null)) {
                return Saxon.Hej.expr.Literal.makeLiteral(val);
            } else {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Variables (other than XSLT 3.0 static variables) cannot be used in a static expression: "+qName.getDisplayName()));
                err.setErrorCode("XPST0008");
                err.setIsStaticError(true);
                throw err;
            }
        }
        public override Saxon.Hej.functions.FunctionLibrary getFunctionLibrary() {
            return functionLibrary;
        }
        public override string getDefaultCollationName() {
            return Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
        }
        public override Saxon.Hej.om.NamespaceUri getDefaultFunctionNamespace() {
            return Saxon.Hej.om.NamespaceUri.FN;
        }
        public override bool isInBackwardsCompatibleMode() {
            return false;
        }
        public override bool isImportedSchema(Saxon.Hej.om.NamespaceUri @namespace) {
            return false;
        }
        public override System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> getImportedSchemaNamespaces() {
            return new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>();
        }
        public override Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return namespaceContext;
        }
        public override Saxon.Hej.trans.DecimalFormatManager getDecimalFormatManager() {
            return null;
        }
        public int getColumnNumber() {
            return 0;
        }
        public string getPublicId() {
            return null;
        }
        public int getLineNumber() {
            return -1;
        }
        public override Saxon.Hej.type.ItemType resolveTypeAlias(Saxon.Hej.om.StructuredQName typeName) {
            return null;
        }
    
    }
}