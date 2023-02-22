using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class ExpressionContext : Saxon.Hej.expr.StaticContext {
        private readonly Saxon.Hej.style.StyleElement element;
        private readonly Saxon.Hej.om.StructuredQName attributeName;
        private Saxon.Hej.s9api.Location containingLocation = null;
        private Saxon.Hej.expr.parser.RetainedStaticContext retainedStaticContext = null;
        public ExpressionContext(Saxon.Hej.style.StyleElement styleElement, Saxon.Hej.om.StructuredQName attributeName)  {
            element = styleElement;
            this.attributeName = attributeName;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return element.getConfiguration();
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            return element.getPackageData();
        }
        public bool isSchemaAware() {
            return element.isSchemaAware();
        }
        public Saxon.Hej.expr.XPathContext makeEarlyEvaluationContext() {
            return new Saxon.Hej.expr.EarlyEvaluationContext(getConfiguration());
        }
        public Saxon.Hej.expr.parser.RetainedStaticContext makeRetainedStaticContext() {
            if ((retainedStaticContext == null)) {
                if ((element.changesRetainedStaticContext() || !(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)element.getParent()) is Saxon.Hej.style.StyleElement))))) {
                    retainedStaticContext = new Saxon.Hej.expr.parser.RetainedStaticContext(this);
                } else {
                    retainedStaticContext = ((Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)element.getParent()))).getStaticContext().makeRetainedStaticContext();
                }
            }
            return retainedStaticContext;
        }
        public Saxon.Hej.s9api.Location getContainingLocation() {
            if ((containingLocation == null)) {
                if ((attributeName == null)) {
                    containingLocation = element;
                } else {
                    containingLocation = new Saxon.Hej.tree.AttributeLocation(element, attributeName);
                }
            }
            return containingLocation;
        }
        public void issueWarning(string s, Saxon.Hej.s9api.Location locator) {
            element.compileWarning(s, Saxon.Hej.trans.SaxonErrorCode.SXWN9000, locator);
        }
        public string getSystemId() {
            return element.getSystemId();
        }
        public string getStaticBaseURI() {
            return element.getBaseURI();
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return element.getAllNamespaces();
        }
        public Saxon.Hej.type.ItemType getRequiredContextItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public Saxon.Hej.trans.DecimalFormatManager getDecimalFormatManager() {
            return element.getCompilation().getPrincipalStylesheetModule().getDecimalFormatManager();
        }
        public Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions() {
            return element.getCompilation().getCompilerInfo().getOptimizerOptions();
        }
        public virtual Saxon.Hej.expr.Expression bindVariable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.style.SourceBinding sourceBinding = element.bindVariable(qName);
            if ((sourceBinding == null)) {
                if ((qName.hasURI(Saxon.Hej.lib.NamespaceConstant.XSLT) && qName.getLocalPart().Equals("original"))) {
                    element.getXslOriginal(Saxon.Hej.om.StandardNames.XSL_VARIABLE);
                    return new Saxon.Hej.expr.GlobalVariableReference(qName);
                }
                Saxon.Hej.trans.SymbolicName sn = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_VARIABLE, qName);
                Saxon.Hej.expr.Component comp = element.getCompilation().getPrincipalStylesheetModule().getComponent(sn);
                if ((comp != null)) {
                    Saxon.Hej.om.SequenceTool.supply(element.iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((parent => {
                        if (((parent is Saxon.Hej.style.XSLGlobalVariable) && ((Saxon.Hej.style.XSLGlobalVariable)(parent)).getVariableQName().Equals(qName))) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Variable "+qName.getDisplayName())+" cannot be used within its own declaration"), "XPST0008");
                            err.setIsStaticError(true);
                            throw err;
                        }
                    } )));
                    Saxon.Hej.expr.instruct.GlobalVariable globalVar = (Saxon.Hej.expr.instruct.GlobalVariable)(comp.getActor());
                    Saxon.Hej.expr.GlobalVariableReference vref = new Saxon.Hej.expr.GlobalVariableReference(globalVar);
                    vref.setStaticType(globalVar.getRequiredType(), null, 0);
                    return vref;
                }
                if (((getXPathVersion()>=30) && qName.hasURI(Saxon.Hej.lib.NamespaceConstant.ERR))) {
                    Saxon.Hej.tree.iter.AxisIterator catchers = element.iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.om.StandardNames.XSL_CATCH, element.getNamePool()));
                    Saxon.Hej.style.StyleElement catcher = (Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.om.NodeInfo)catchers.next()));
                    if ((catcher != null)) {
                        foreach (Saxon.Hej.om.StructuredQName errorVariable in Saxon.Hej.om.StandardNames.errorVariables) {
                            if (errorVariable.getLocalPart().Equals(qName.getLocalPart())) {
                                Saxon.Hej.functions.SystemFunction f = Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance().makeFunction("dynamic-error-info", 1);
                                return f.makeFunctionCall(new Saxon.Hej.expr.StringLiteral(qName.getLocalPart()));
                            }
                        }
                    }
                }
                Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException((("Variable "+qName.getDisplayName())+" has not been declared (or its declaration is not in scope)"), "XPST0008");
                error.setIsStaticError(true);
                throw error;
            }
            Saxon.Hej.expr.VariableReference var;
            if (sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.IMPLICITLY_DECLARED)) {
                Saxon.Hej.expr.SuppliedParameterReference supRef = new Saxon.Hej.expr.SuppliedParameterReference(0);
                supRef.setSuppliedType(sourceBinding.getDeclaredType());
                return supRef;
            }
            if (sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.GLOBAL)) {
                var = new Saxon.Hej.expr.GlobalVariableReference(qName);
                Saxon.Hej.expr.instruct.GlobalVariable compiledVar = ((Saxon.Hej.style.XSLGlobalVariable)(sourceBinding.getSourceElement())).getCompiledVariable();
                if (((compiledVar != null) && element.getCompilation().getCompilerInfo().isJustInTimeCompilation())) {
                    var.fixup(compiledVar);
                    var.setStaticType(compiledVar.getRequiredType(), sourceBinding.getConstantValue(), 0);
                } else {
                    sourceBinding.registerReference(var);
                }
                return var;
            } else {
                var = new Saxon.Hej.expr.LocalVariableReference(qName);
                sourceBinding.registerReference(var);
                return var;
            }
        }
        public Saxon.Hej.functions.FunctionLibrary getFunctionLibrary() {
            Saxon.Hej.functions.FunctionLibrary lib = element.getContainingPackage().getFunctionLibrary();
            Saxon.Hej.style.StyleElement containingOverride = element.findAncestorElement(Saxon.Hej.om.StandardNames.XSL_OVERRIDE);
            if ((containingOverride != null)) {
                Saxon.Hej.functions.FunctionLibraryList libList = new Saxon.Hej.functions.FunctionLibraryList();
                libList.addFunctionLibrary(lib);
                ((Saxon.Hej.style.XSLOverride)(containingOverride)).addXSLOverrideFunctionLibrary(libList);
                return libList;
            }
            return lib;
        }
        public string getDefaultCollationName() {
            return element.getDefaultCollationName();
        }
        public string getDefaultElementNamespace() {
            return element.getDefaultXPathNamespace();
        }
        public string getDefaultFunctionNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.FN;
        }
        public bool isInBackwardsCompatibleMode() {
            return element.xPath10ModeIsEnabled();
        }
        public int getXPathVersion() {
            if (((element.getEffectiveVersion() == 40) || (element.getCompilation().getCompilerInfo().getXsltVersion() == 40))) {
                return 40;
            } else {
                return getConfiguration().getConfigurationProperty(Saxon.Hej.lib.Feature<int>.XPATH_VERSION_FOR_XSLT);
            }
        }
        public bool isImportedSchema(string @namespace) {
            return element.getPrincipalStylesheetModule().isImportedSchema(@namespace);
        }
        public System.Collections.Generic.ISet<string> getImportedSchemaNamespaces() {
            return element.getPrincipalStylesheetModule().getImportedSchemaTable();
        }
        public Saxon.Hej.trans.KeyManager getKeyManager() {
            return element.getCompilation().getPrincipalStylesheetModule().getKeyManager();
        }
        public Saxon.Hej.style.StyleElement getStyleElement() {
            return element;
        }
        public virtual Saxon.Hej.type.ItemType resolveTypeAlias(Saxon.Hej.om.StructuredQName typeName) {
            return ((/*1430*/Saxon.Hej.style.StylesheetPackage)getPackageData()).obtainTypeAliasManager().getItemType(typeName);
        }
        public Saxon.Api.UnprefixedElementMatchingPolicy getUnprefixedElementMatchingPolicy() {
            return element.getCompilation().getCompilerInfo().getUnprefixedElementMatchingPolicy();
        }
    
    }
}