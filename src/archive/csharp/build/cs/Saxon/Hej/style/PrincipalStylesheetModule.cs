using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class PrincipalStylesheetModule : Saxon.Hej.style.StylesheetModule, Saxon.Hej.trans.GlobalVariableManager {
        private readonly Saxon.Hej.style.StylesheetPackage stylesheetPackage;
        private readonly bool declaredModes;
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.style.ComponentDeclaration> globalVariableIndex = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.style.ComponentDeclaration>(20);
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.style.ComponentDeclaration> templateIndex = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.style.ComponentDeclaration>(20);
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.style.ComponentDeclaration> functionIndex = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName, Saxon.Hej.style.ComponentDeclaration>(8);
        private readonly Saxon.Hej.trans.KeyManager keyManager;
        private readonly Saxon.Hej.trans.DecimalFormatManager decimalFormatManager;
        private readonly Saxon.Hej.trans.rules.RuleManager ruleManager;
        private Saxon.Hej.expr.accum.AccumulatorRegistry accumulatorManager = null;
        private int numberOfAliases = 0;
        private System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> namespaceAliasList = new System.Collections.Generic.List<Saxon.Hej.style.ComponentDeclaration>(5);
        private System.Collections.Generic.Dictionary<string,Saxon.Hej.om.NamespaceBinding> namespaceAliasMap;
        private System.Collections.Generic.ISet<string> aliasResultUriSet;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration>> attributeSetDeclarations = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration>>();
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.om.DocumentKey,Saxon.Hej.style.XSLModuleRoot> moduleCache = new System.Collections.Generic.Dictionary<Saxon.Hej.om.DocumentKey, Saxon.Hej.style.XSLModuleRoot>(4);
        private readonly Saxon.Hej.trans.TypeAliasManager typeAliasManager;
        private readonly Saxon.Hej.serialize.CharacterMapIndex characterMapIndex;
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.Action> fixupActions = new System.Collections.Generic.List<Saxon.Hej.om.Action>();
        private bool needsDynamicOutputProperties = false;
        public PrincipalStylesheetModule(Saxon.Hej.style.XSLPackage sourceElement) : base (sourceElement, 0) {
            declaredModes = sourceElement.isDeclaredModes();
            stylesheetPackage = getConfiguration().makeStylesheetPackage();
            Saxon.Hej.trans.CompilerInfo compilerInfo = sourceElement.getCompilation().getCompilerInfo();
            stylesheetPackage.setTargetEdition(compilerInfo.getTargetEdition());
            stylesheetPackage.setRelocatable(compilerInfo.isRelocatable());
            stylesheetPackage.setJustInTimeCompilation(compilerInfo.isJustInTimeCompilation());
            stylesheetPackage.setImplicitPackage(!(sourceElement.getLocalPart().Equals("package")));
            keyManager = stylesheetPackage.getKeyManager();
            decimalFormatManager = stylesheetPackage.getDecimalFormatManager();
            ruleManager = new Saxon.Hej.trans.rules.RuleManager(stylesheetPackage, compilerInfo);
            ruleManager.getUnnamedMode().makeDeclaringComponent(Saxon.Hej.trans.Visibility.PRIVATE, stylesheetPackage);
            stylesheetPackage.setRuleManager(ruleManager);
            stylesheetPackage.setDeclaredModes(declaredModes);
            Saxon.Hej.om.StructuredQName defaultMode = sourceElement.getDefaultMode();
            stylesheetPackage.setDefaultMode(sourceElement.getDefaultMode());
            if ((defaultMode != null)) {
                ruleManager.obtainMode(defaultMode, !(declaredModes));
            }
            characterMapIndex = new Saxon.Hej.serialize.CharacterMapIndex();
            stylesheetPackage.setCharacterMapIndex(characterMapIndex);
            typeAliasManager = getConfiguration().makeTypeAliasManager();
            stylesheetPackage.setTypeAliasManager(typeAliasManager);
            try {
                setInputTypeAnnotations(sourceElement.getInputTypeAnnotationsAttribute());
            } catch (Saxon.Hej.trans.XPathException err) {
            }
        }
        public Saxon.Hej.expr.Component getComponent(Saxon.Hej.trans.SymbolicName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(stylesheetPackage.getComponentIndex(), name);
        }
        public override Saxon.Hej.style.PrincipalStylesheetModule getPrincipalStylesheetModule() {
            return this;
        }
        public Saxon.Hej.style.StylesheetPackage getStylesheetPackage() {
            return stylesheetPackage;
        }
        public Saxon.Hej.trans.KeyManager getKeyManager() {
            return keyManager;
        }
        public Saxon.Hej.trans.DecimalFormatManager getDecimalFormatManager() {
            return decimalFormatManager;
        }
        public Saxon.Hej.trans.rules.RuleManager getRuleManager() {
            return ruleManager;
        }
        public bool isDeclaredModes() {
            return declaredModes;
        }
        public void addFixupAction(Saxon.Hej.om.Action action) {
            fixupActions.Add(action);
        }
        public void setNeedsDynamicOutputProperties(bool b) {
            needsDynamicOutputProperties = b;
        }
        public Saxon.Hej.serialize.CharacterMapIndex getCharacterMapIndex() {
            return characterMapIndex;
        }
        public Saxon.Hej.trans.TypeAliasManager getTypeAliasManager() {
            return typeAliasManager;
        }
        public void declareXQueryFunction(Saxon.Hej.query.XQueryFunction function) {
            Saxon.Hej.query.XQueryFunctionLibrary lib = getStylesheetPackage().getXQueryFunctionLibrary();
            if ((getStylesheetPackage().getFunction(function.getUserFunction().getSymbolicName()) != null)) {
                throw new Saxon.Hej.trans.XPathException(("Duplicate declaration of "+function.getUserFunction().getSymbolicName()), "XQST0034");
            }
            lib.declareFunction(function);
        }
        public void putStylesheetDocument(Saxon.Hej.om.DocumentKey key, Saxon.Hej.style.XSLStylesheet module) {
            Saxon.Impl.Helpers.MapUtils.Put(moduleCache, key, module);
        }
        public Saxon.Hej.style.XSLModuleRoot getStylesheetDocument(Saxon.Hej.om.DocumentKey key) {
            Saxon.Hej.style.XSLModuleRoot sheet = Saxon.Impl.Helpers.MapUtils.Get(moduleCache, key);
            if ((sheet != null)) {
                Saxon.Hej.trans.XPathException warning = new Saxon.Hej.trans.XPathException(((("Stylesheet module "+key)+" is included or imported more than once. ")+"This is permitted, but may lead to errors or unexpected behavior"));
                sheet.issueWarning(warning);
            }
            return sheet;
        }
        public void preprocess(Saxon.Hej.style.Compilation compilation) {
            Saxon.Hej.trans.Timer timer = compilation.timer;
            spliceUsePackages((Saxon.Hej.style.XSLPackage)(getRootElement()), getRootElement().getCompilation());
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("spliceIncludes");
            }
            importSchemata();
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("importSchemata");
            }
            getTypeAliasManager().processAllDeclarations(topLevel);
            buildIndexes();
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("buildIndexes");
            }
            checkForSchemaAwareness();
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("checkForSchemaAwareness");
            }
            processAllAttributes();
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("processAllAttributes");
            }
            collectNamespaceAliases();
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("collectNamespaceAliases");
            }
            foreach (Saxon.Hej.style.ComponentDeclaration topLevelDecl in topLevel) {
                Saxon.Hej.style.StyleElement inst = topLevelDecl.getSourceElement();
                if (!(inst.isActionCompleted(Saxon.Hej.style.StyleElement.ACTION_FIXUP))) {
                    inst.setActionCompleted(Saxon.Hej.style.StyleElement.ACTION_FIXUP);
                    inst.fixupReferences();
                }
            }
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("fixupReferences");
            }
            Saxon.Hej.style.XSLPackage top = (Saxon.Hej.style.XSLPackage)(getStylesheetElement());
            if (!(top.isActionCompleted(Saxon.Hej.style.StyleElement.ACTION_VALIDATE))) {
                top.setActionCompleted(Saxon.Hej.style.StyleElement.ACTION_VALIDATE);
                top.validate(null);
                foreach (Saxon.Hej.style.ComponentDeclaration d in topLevel) {
                    d.getSourceElement().validateSubtree(d, false);
                }
            }
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("validate");
            }
            Saxon.Ejava.util.Properties props = gatherOutputProperties(null);
            props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.STYLESHEET_VERSION, (getStylesheetPackage().getHostLanguageVersion()+""));
            getStylesheetPackage().setDefaultOutputProperties(props);
            System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName> outputNames = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>(5);
            foreach (Saxon.Hej.style.ComponentDeclaration outputDecl in topLevel) {
                if ((outputDecl.getSourceElement() is Saxon.Hej.style.XSLOutput)) {
                    Saxon.Hej.style.XSLOutput @out = (Saxon.Hej.style.XSLOutput)(outputDecl.getSourceElement());
                    Saxon.Hej.om.StructuredQName qName = @out.getFormatQName();
                    if ((qName != null)) {
                        outputNames.Add(qName);
                    }
                }
            }
            if (outputNames.Count==0) {
                if (needsDynamicOutputProperties) {
                    throw new Saxon.Hej.trans.XPathException(("The stylesheet contains xsl:result-document instructions that calculate the output "+"format name at run-time, but there are no named xsl:output declarations"), "XTDE1460");
                }
            } else {
                foreach (Saxon.Hej.om.StructuredQName qName in outputNames) {
                    Saxon.Ejava.util.Properties oprops = gatherOutputProperties(qName);
                    getStylesheetPackage().setNamedOutputProperties(qName, oprops);
                }
            }
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("Register output formats");
            }
            foreach (Saxon.Hej.style.ComponentDeclaration d in topLevel) {
                Saxon.Hej.style.StyleElement inst = d.getSourceElement();
                if ((inst is Saxon.Hej.style.XSLCharacterMap)) {
                    Saxon.Hej.style.XSLCharacterMap xcm = (Saxon.Hej.style.XSLCharacterMap)(inst);
                    Saxon.Hej.om.StructuredQName qn = xcm.getCharacterMapName();
                    Saxon.Hej.z.IntHashMap<string> map = new Saxon.Hej.z.IntHashMap<string>();
                    xcm.assemble(map);
                    characterMapIndex.putCharacterMap(xcm.getCharacterMapName(), new Saxon.Hej.serialize.CharacterMap(qn, map));
                }
            }
            if (Saxon.Hej.style.Compilation.TIMING) {
                timer.report("Index character maps");
            }
        }
        protected internal void spliceUsePackages(Saxon.Hej.style.XSLPackage xslpackage, Saxon.Hej.style.Compilation compilation) {
            System.Collections.Generic.IList<Saxon.Hej.style.XSLUsePackage> useDeclarations = new System.Collections.Generic.List<Saxon.Hej.style.XSLUsePackage>();
            gatherUsePackageDeclarations(compilation, xslpackage, useDeclarations);
            System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> overrides = new System.Collections.Generic.HashSet<Saxon.Hej.trans.SymbolicName>();
            foreach (Saxon.Hej.style.XSLUsePackage use in useDeclarations) {
                gatherOverridingDeclarations(use, compilation, overrides);
            }
            Saxon.Hej.style.StylesheetPackage thisPackage = getStylesheetPackage();
            foreach (Saxon.Hej.style.XSLUsePackage use in useDeclarations) {
                System.Collections.Generic.IList<Saxon.Hej.style.XSLAccept> acceptors = use.getAcceptors();
                thisPackage.addComponentsFromUsedPackage(use.getUsedPackage(), acceptors, overrides);
            }
            foreach (Saxon.Hej.style.XSLUsePackage use in useDeclarations) {
                use.gatherRuleOverrides(this, overrides);
            }
            spliceIncludes();
        }
        private static void gatherUsePackageDeclarations(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.StyleElement wrapper, System.Collections.Generic.IList<Saxon.Hej.style.XSLUsePackage> declarations) {
            foreach (Saxon.Hej.om.NodeInfo use in wrapper.children()) {
                if ((use is Saxon.Hej.style.XSLUsePackage)) {
                    declarations.Add((Saxon.Hej.style.XSLUsePackage)(use));
                } else {
                    if ((use is Saxon.Hej.style.XSLInclude)) {
                        string href = Saxon.Hej.value.Whitespace.trim(use.getAttributeValue("", "href"));
                        Saxon.Hej.om.DocumentKey key = Saxon.Hej.functions.DocumentFn.computeDocumentKey(href, use.getBaseURI(), compilation.getPackageData(), false);
                        Saxon.Hej.om.TreeInfo includedTree = Saxon.Impl.Helpers.MapUtils.Get(compilation.getStylesheetModules(), key);
                        Saxon.Hej.style.StyleElement incWrapper = (Saxon.Hej.style.StyleElement)(((Saxon.Hej.tree.linked.DocumentImpl)(includedTree.getRootNode())).getDocumentElement());
                        gatherUsePackageDeclarations(compilation, incWrapper, declarations);
                    }
                }
            }
        }
        private void gatherOverridingDeclarations(Saxon.Hej.style.XSLUsePackage use, Saxon.Hej.style.Compilation compilation, System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> overrides) {
            use.findUsedPackage(compilation.getCompilerInfo());
            use.gatherNamedOverrides(this, topLevel, overrides);
        }
        protected internal virtual void importSchemata() {
            for (int i = (topLevel.Count-1); (i>=0); i--) {
                Saxon.Hej.style.ComponentDeclaration decl = topLevel[i];
                if ((decl.getSourceElement() is Saxon.Hej.style.XSLImportSchema)) {
                    Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException("xsl:import-schema requires Saxon-EE");
                    xe.setErrorCode("XTSE1650");
                    xe.setLocator(decl.getSourceElement());
                    throw xe;
                }
            }
        }
        private void buildIndexes() {
            for (int i = (topLevel.Count-1); (i>=0); i--) {
                Saxon.Hej.style.ComponentDeclaration decl = topLevel[i];
                decl.getSourceElement().index(decl, this);
            }
        }
        public void processAllAttributes() {
            getRootElement().processDefaultCollationAttribute();
            getRootElement().processDefaultMode();
            getRootElement().prepareAttributes();
            foreach (Saxon.Hej.style.XSLModuleRoot xss in moduleCache.Values) {
                xss.prepareAttributes();
            }
            foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                Saxon.Hej.style.StyleElement inst = decl.getSourceElement();
                if (!(inst.isActionCompleted(Saxon.Hej.style.StyleElement.ACTION_PROCESS_ATTRIBUTES))) {
                    inst.setActionCompleted(Saxon.Hej.style.StyleElement.ACTION_PROCESS_ATTRIBUTES);
                    try {
                        inst.processAllAttributes();
                    } catch (Saxon.Hej.trans.XPathException err) {
                        decl.getSourceElement().compileError(err);
                    }
                }
            }
        }
        protected internal void indexFunction(Saxon.Hej.style.ComponentDeclaration decl) {
            System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = stylesheetPackage.getComponentIndex();
            Saxon.Hej.style.XSLFunction sourceFunction = (Saxon.Hej.style.XSLFunction)(decl.getSourceElement());
            Saxon.Hej.expr.instruct.UserFunction compiledFunction = sourceFunction.getCompiledFunction();
            Saxon.Hej.expr.Component declaringComponent = compiledFunction.obtainDeclaringComponent(sourceFunction);
            Saxon.Hej.trans.SymbolicName.F sName = ((/*1430*/Saxon.Hej.trans.SymbolicName.F)sourceFunction.getSymbolicName());
            if ((sName != null)) {
                Saxon.Hej.expr.Component other = Saxon.Impl.Helpers.MapUtils.Get(componentIndex, sName);
                if ((other == null)) {
                    Saxon.Impl.Helpers.MapUtils.Put(componentIndex, sName, declaringComponent);
                    Saxon.Impl.Helpers.MapUtils.Put(functionIndex, sName, decl);
                } else {
                    if ((other.getDeclaringPackage() == getStylesheetPackage())) {
                        int thisPrecedence = decl.getPrecedence();
                        Saxon.Hej.style.ComponentDeclaration otherFunction = Saxon.Impl.Helpers.MapUtils.Get(functionIndex, sName);
                        int otherPrecedence = otherFunction.getPrecedence();
                        if ((thisPrecedence == otherPrecedence)) {
                            sourceFunction.compileError((((("Duplicate named function (see line "+otherFunction.getSourceElement().getLineNumber())+" of ")+otherFunction.getSourceElement().getSystemId())+')'), "XTSE0770");
                        } else {
                            if ((thisPrecedence<otherPrecedence)) {
                            } else {
                                Saxon.Impl.Helpers.MapUtils.Put(componentIndex, sName, declaringComponent);
                                Saxon.Impl.Helpers.MapUtils.Put(functionIndex, sName, decl);
                            }
                        }
                    } else {
                        if ((sourceFunction.findAncestorElement(Saxon.Hej.om.StandardNames.XSL_OVERRIDE) != null)) {
                            Saxon.Impl.Helpers.MapUtils.Put(componentIndex, sName, declaringComponent);
                            Saxon.Impl.Helpers.MapUtils.Put(functionIndex, sName, decl);
                        } else {
                            sourceFunction.compileError(((("Function "+sName.getShortName())+" conflicts with a public function in package ")+other.getDeclaringPackage().getPackageName()), "XTSE3050");
                        }
                    }
                }
            }
        }
        protected internal void indexVariableDeclaration(Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.style.XSLGlobalVariable varDecl = (Saxon.Hej.style.XSLGlobalVariable)(decl.getSourceElement());
            Saxon.Hej.om.StructuredQName qName = varDecl.getSourceBinding().getVariableQName();
            Saxon.Hej.expr.instruct.GlobalVariable compiledVariable = (Saxon.Hej.expr.instruct.GlobalVariable)(varDecl.getActor());
            Saxon.Hej.expr.Component declaringComponent = compiledVariable.obtainDeclaringComponent(varDecl);
            System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = stylesheetPackage.getComponentIndex();
            if ((qName != null)) {
                Saxon.Hej.trans.SymbolicName sName = varDecl.getSymbolicName();
                Saxon.Hej.expr.Component other = Saxon.Impl.Helpers.MapUtils.Get(componentIndex, sName);
                if ((other == null)) {
                    Saxon.Impl.Helpers.MapUtils.Put(globalVariableIndex, qName, decl);
                    Saxon.Impl.Helpers.MapUtils.Put(componentIndex, new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_VARIABLE, qName), varDecl.getActor().getDeclaringComponent());
                } else {
                    if ((other.getDeclaringPackage() == getStylesheetPackage())) {
                        int thisPrecedence = decl.getPrecedence();
                        Saxon.Hej.style.ComponentDeclaration otherVarDecl = Saxon.Impl.Helpers.MapUtils.Get(globalVariableIndex, sName.getComponentName());
                        int otherPrecedence = otherVarDecl.getPrecedence();
                        if ((thisPrecedence == otherPrecedence)) {
                            Saxon.Hej.style.StyleElement v2 = otherVarDecl.getSourceElement();
                            if ((v2 == varDecl)) {
                                varDecl.compileError(((("Global variable or parameter $"+qName.getDisplayName())+" is declared more than once ")+"(caused by including the containing module more than once)"), "XTSE0630");
                            } else {
                                varDecl.compileError((((("Duplicate global variable/parameter declaration (see line "+v2.getLineNumber())+" of ")+v2.getSystemId())+')'), "XTSE0630");
                            }
                        } else {
                            if (((thisPrecedence<otherPrecedence) && (varDecl != otherVarDecl.getSourceElement()))) {
                                varDecl.setRedundant(true);
                            } else {
                                if ((varDecl != otherVarDecl.getSourceElement())) {
                                    ((Saxon.Hej.style.XSLGlobalVariable)(otherVarDecl.getSourceElement())).setRedundant(true);
                                    Saxon.Impl.Helpers.MapUtils.Put(globalVariableIndex, qName, decl);
                                    Saxon.Impl.Helpers.MapUtils.Put(componentIndex, new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_VARIABLE, qName), varDecl.getActor().getDeclaringComponent());
                                }
                            }
                        }
                    } else {
                        if ((varDecl.findAncestorElement(Saxon.Hej.om.StandardNames.XSL_OVERRIDE) != null)) {
                            Saxon.Impl.Helpers.MapUtils.Put(componentIndex, sName, declaringComponent);
                            Saxon.Impl.Helpers.MapUtils.Put(globalVariableIndex, sName.getComponentName(), decl);
                        } else {
                            string kind = ((varDecl is Saxon.Hej.style.XSLGlobalParam) ? "parameter" : "variable");
                            varDecl.compileError(((((("Global "+kind)+" $")+sName.getComponentName().getDisplayName())+" conflicts with a public variable/parameter in package ")+other.getDeclaringPackage().getPackageName()), "XTSE3050");
                        }
                    }
                }
            }
        }
        public Saxon.Hej.style.SourceBinding getGlobalVariableBinding(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.style.ComponentDeclaration decl = Saxon.Impl.Helpers.MapUtils.Get(globalVariableIndex, qName);
            return ((decl == null) ? null : ((Saxon.Hej.style.XSLGlobalVariable)(decl.getSourceElement())).getSourceBinding());
        }
        protected internal void indexNamedTemplate(Saxon.Hej.style.ComponentDeclaration decl) {
            System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = stylesheetPackage.getComponentIndex();
            Saxon.Hej.style.XSLTemplate sourceTemplate = (Saxon.Hej.style.XSLTemplate)(decl.getSourceElement());
            Saxon.Hej.trans.SymbolicName sName = sourceTemplate.getSymbolicName();
            if ((sName != null)) {
                Saxon.Hej.expr.Component other = Saxon.Impl.Helpers.MapUtils.Get(componentIndex, sName);
                if ((other == null)) {
                    Saxon.Hej.expr.instruct.NamedTemplate compiledTemplate = ((Saxon.Hej.style.XSLTemplate)(decl.getSourceElement())).getCompiledNamedTemplate();
                    Saxon.Hej.expr.Component declaringComponent = compiledTemplate.obtainDeclaringComponent(sourceTemplate);
                    Saxon.Impl.Helpers.MapUtils.Put(componentIndex, sName, declaringComponent);
                    setLocalParamDetails(sourceTemplate, compiledTemplate);
                    Saxon.Impl.Helpers.MapUtils.Put(templateIndex, sName.getComponentName(), decl);
                } else {
                    if ((other.getDeclaringPackage() == getStylesheetPackage())) {
                        int thisPrecedence = decl.getPrecedence();
                        Saxon.Hej.style.ComponentDeclaration otherTemplate = Saxon.Impl.Helpers.MapUtils.Get(templateIndex, sName.getComponentName());
                        int otherPrecedence = otherTemplate.getPrecedence();
                        if ((thisPrecedence == otherPrecedence)) {
                            string errorCode = ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)sourceTemplate.getParent()) is Saxon.Hej.style.XSLOverride) ? "XTSE3055" : "XTSE0660");
                            sourceTemplate.compileError((((("Duplicate named template (see line "+otherTemplate.getSourceElement().getLineNumber())+" of ")+otherTemplate.getSourceElement().getSystemId())+')'), errorCode);
                        } else {
                            if ((thisPrecedence<otherPrecedence)) {
                            } else {
                                Saxon.Hej.expr.instruct.NamedTemplate compiledTemplate = new Saxon.Hej.expr.instruct.NamedTemplate(sName.getComponentName());
                                Saxon.Hej.expr.Component declaringComponent = compiledTemplate.obtainDeclaringComponent(sourceTemplate);
                                Saxon.Impl.Helpers.MapUtils.Put(componentIndex, sName, declaringComponent);
                                Saxon.Impl.Helpers.MapUtils.Put(templateIndex, sName.getComponentName(), decl);
                                setLocalParamDetails(sourceTemplate, compiledTemplate);
                            }
                        }
                    } else {
                        if ((sourceTemplate.findAncestorElement(Saxon.Hej.om.StandardNames.XSL_OVERRIDE) != null)) {
                            Saxon.Hej.expr.instruct.NamedTemplate compiledTemplate = sourceTemplate.getCompiledNamedTemplate();
                            Saxon.Hej.expr.Component declaringComponent = compiledTemplate.obtainDeclaringComponent(sourceTemplate);
                            Saxon.Impl.Helpers.MapUtils.Put(componentIndex, sName, declaringComponent);
                            Saxon.Impl.Helpers.MapUtils.Put(templateIndex, sName.getComponentName(), decl);
                        } else {
                            sourceTemplate.compileError(((("Named template "+sName.getComponentName().getDisplayName())+" conflicts with a public named template in package ")+other.getDeclaringPackage().getPackageName()), "XTSE3050");
                        }
                    }
                }
            }
        }
        private static void setLocalParamDetails(Saxon.Hej.style.XSLTemplate source, Saxon.Hej.expr.instruct.NamedTemplate nt) {
            Saxon.Hej.tree.iter.AxisIterator kids = source.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo> details = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo>();
            Saxon.Hej.om.SequenceTool.supply(kids, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((child => {
                if ((child is Saxon.Hej.style.XSLLocalParam)) {
                    Saxon.Hej.style.XSLLocalParam lp = (Saxon.Hej.style.XSLLocalParam)(child);
                    lp.prepareTemplateSignatureAttributes();
                    Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo info = new Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo();
                    info.name = lp.getVariableQName();
                    info.requiredType = lp.getRequiredType();
                    info.isRequired = lp.isRequiredParam();
                    info.isTunnel = lp.isTunnelParam();
                    details.Add(info);
                }
            } )));
            nt.setLocalParamDetails(details);
        }
        public Saxon.Hej.expr.instruct.NamedTemplate getNamedTemplate(Saxon.Hej.om.StructuredQName name) {
            System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = stylesheetPackage.getComponentIndex();
            Saxon.Hej.expr.Component component = Saxon.Impl.Helpers.MapUtils.Get(componentIndex, new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_TEMPLATE, name));
            return ((component == null) ? null : (Saxon.Hej.expr.instruct.NamedTemplate)(component.getActor()));
        }
        protected internal void indexAttributeSet(Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.style.XSLAttributeSet sourceAttributeSet = (Saxon.Hej.style.XSLAttributeSet)(decl.getSourceElement());
            Saxon.Hej.om.StructuredQName name = sourceAttributeSet.getAttributeSetName();
            System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> entries = Saxon.Impl.Helpers.MapUtils.Get(attributeSetDeclarations, name);
            if ((entries == null)) {
                entries = new System.Collections.Generic.List<Saxon.Hej.style.ComponentDeclaration>();
                Saxon.Impl.Helpers.MapUtils.Put(attributeSetDeclarations, name, entries);
            } else {
                string thisVis = Saxon.Hej.value.Whitespace.trim(sourceAttributeSet.getAttributeValue("", "visibility"));
                string firstVis = Saxon.Hej.value.Whitespace.trim(entries[0].getSourceElement().getAttributeValue("", "visibility"));
                if (((thisVis == null) ? (firstVis != null) : !(thisVis.Equals(firstVis)))) {
                    throw new Saxon.Hej.trans.XPathException("Visibility attributes on attribute-sets sharing the same name must all be the same", "XTSE0010");
                }
            }
            entries.Insert(0, decl);
        }
        public System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> getAttributeSetDeclarations(Saxon.Hej.om.StructuredQName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(attributeSetDeclarations, name);
        }
        public void combineAttributeSets(Saxon.Hej.style.Compilation compilation) {
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.AttributeSet> index = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.instruct.AttributeSet>();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration>> entry in attributeSetDeclarations) {
                Saxon.Hej.expr.instruct.AttributeSet @as = new Saxon.Hej.expr.instruct.AttributeSet();
                @as.setName(entry.Key);
                @as.setPackageData(stylesheetPackage);
                Saxon.Hej.style.StyleElement firstDecl = entry.Value[0].getSourceElement();
                @as.setSystemId(firstDecl.getSystemId());
                @as.setLineNumber(firstDecl.getLineNumber());
                @as.setColumnNumber(firstDecl.getColumnNumber());
                Saxon.Impl.Helpers.MapUtils.Put(index, entry.Key, @as);
                Saxon.Hej.expr.Component declaringComponent = @as.getDeclaringComponent();
                if ((declaringComponent == null)) {
                    declaringComponent = @as.makeDeclaringComponent(Saxon.Hej.trans.Visibility.PRIVATE, stylesheetPackage);
                }
                stylesheetPackage.addComponent(declaringComponent);
            }
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration>> entry in attributeSetDeclarations) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> content = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
                Saxon.Hej.trans.Visibility vis = Saxon.Hej.trans.Visibility.UNDEFINED;
                bool explicitVisibility = false;
                bool streamable = false;
                System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> entries = new System.Collections.Generic.List<Saxon.Hej.style.ComponentDeclaration>();
                System.Collections.Generic.ISet<Saxon.Hej.style.XSLAttributeSet> elements = new System.Collections.Generic.HashSet<Saxon.Hej.style.XSLAttributeSet>();
                for (int i = (entry.Value.Count-1); (i>=0); i--) {
                    Saxon.Hej.style.ComponentDeclaration attSetDecl = entry.Value[i];
                    Saxon.Hej.style.XSLAttributeSet src = (Saxon.Hej.style.XSLAttributeSet)(attSetDecl.getSourceElement());
                    if (!(elements.Contains(src))) {
                        entries.Insert(0, attSetDecl);
                        elements.Add(src);
                    }
                }
                foreach (Saxon.Hej.style.ComponentDeclaration decl in entries) {
                    Saxon.Hej.style.XSLAttributeSet src = (Saxon.Hej.style.XSLAttributeSet)(decl.getSourceElement());
                    streamable |= src.isDeclaredStreamable();
                    src.compileDeclaration(compilation, decl);
                    Saxon.Impl.Helpers.ListUtils.AddAll(content, src.getContainedInstructions());
                    vis = src.getVisibility();
                    explicitVisibility = (explicitVisibility || (src.getAttributeValue("", "visibility") != null));
                }
                Saxon.Hej.expr.instruct.AttributeSet aSet = Saxon.Impl.Helpers.MapUtils.Get(index, entry.Key);
                aSet.setDeclaredStreamable(streamable);
                Saxon.Hej.expr.Expression block = Saxon.Hej.expr.instruct.Block.makeBlock(content);
                aSet.setBody(block);
                Saxon.Hej.expr.instruct.SlotManager frame = getConfiguration().makeSlotManager();
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(block, 0, frame);
                aSet.setStackFrameMap(frame);
                Saxon.Hej.trans.VisibilityProvenance provenance = (explicitVisibility ? Saxon.Hej.trans.VisibilityProvenance.EXPLICIT : Saxon.Hej.trans.VisibilityProvenance.DEFAULTED);
                aSet.getDeclaringComponent().setVisibility(vis, provenance);
                if (streamable) {
                    checkStreamability(aSet);
                }
            }
        }
        protected internal virtual void checkStreamability(Saxon.Hej.expr.instruct.AttributeSet aSet) {
        }
        protected internal void getAttributeSets(Saxon.Hej.om.StructuredQName name, System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> list) {
            foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                if ((decl.getSourceElement() is Saxon.Hej.style.XSLAttributeSet)) {
                    Saxon.Hej.style.XSLAttributeSet t = (Saxon.Hej.style.XSLAttributeSet)(decl.getSourceElement());
                    if (t.getAttributeSetName().Equals(name)) {
                        list.Add(decl);
                    }
                }
            }
        }
        public void indexMode(Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.style.XSLMode sourceMode = (Saxon.Hej.style.XSLMode)(decl.getSourceElement());
            Saxon.Hej.om.StructuredQName modeName = sourceMode.getObjectName();
            if ((modeName == null)) {
                return ;
            }
            Saxon.Hej.trans.SymbolicName sName = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_MODE, modeName);
            Saxon.Hej.trans.Mode other = getStylesheetPackage().getRuleManager().obtainMode(modeName, false);
            if (((other != null) && (other.getDeclaringComponent().getDeclaringPackage() != getStylesheetPackage()))) {
                sourceMode.compileError(((("Mode "+sName.getComponentName().getDisplayName())+" conflicts with a public mode declared in package ")+other.getDeclaringComponent().getDeclaringPackage().getPackageName()), "XTSE3050");
            }
        }
        public bool checkAcceptableModeForPackage(Saxon.Hej.style.XSLTemplate template, Saxon.Hej.trans.Mode mode) {
            Saxon.Hej.style.StylesheetPackage templatePack = template.getPackageData();
            if ((mode.getDeclaringComponent() == null)) {
                return true;
            }
            Saxon.Hej.style.StylesheetPackage modePack = mode.getDeclaringComponent().getDeclaringPackage();
            if ((templatePack != modePack)) {
                Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)template.getParent());
                bool bad = false;
                if (!(((parent is Saxon.Hej.style.XSLOverride)))) {
                    bad = true;
                } else {
                    Saxon.Hej.om.NodeInfo grandParent = parent.getParent();
                    if (!(((grandParent is Saxon.Hej.style.XSLUsePackage)))) {
                        bad = true;
                    } else {
                        Saxon.Hej.trans.SymbolicName modeName = mode.getSymbolicName();
                        Saxon.Hej.expr.Component.M usedMode = (Saxon.Hej.expr.Component.M)(((Saxon.Hej.style.XSLUsePackage)(grandParent)).getUsedPackage().getComponent(modeName));
                        if ((usedMode == null)) {
                            bad = true;
                        } else {
                            if ((usedMode.getVisibility() == Saxon.Hej.trans.Visibility.FINAL)) {
                                bad = true;
                            }
                        }
                    }
                }
                if (bad) {
                    template.compileError(("A template rule cannot be added to a mode declared in a used package "+"unless the xsl:template declaration appears within an xsl:override child of the appropriate xsl:use-package element"), "XTSE3050");
                    return false;
                }
            }
            return true;
        }
        private void checkForSchemaAwareness() {
            Saxon.Hej.style.Compilation compilation = getRootElement().getCompilation();
            if ((!(compilation.isSchemaAware()) && getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT))) {
                foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                    Saxon.Hej.style.StyleElement node = decl.getSourceElement();
                    if ((node is Saxon.Hej.style.XSLImportSchema)) {
                        compilation.setSchemaAware(true);
                        return ;
                    }
                }
            }
        }
        public Saxon.Hej.expr.accum.AccumulatorRegistry getAccumulatorManager() {
            return accumulatorManager;
        }
        public void setAccumulatorManager(Saxon.Hej.expr.accum.AccumulatorRegistry accumulatorManager) {
            this.accumulatorManager = accumulatorManager;
            stylesheetPackage.setAccumulatorRegistry(accumulatorManager);
        }
        protected internal void addNamespaceAlias(Saxon.Hej.style.ComponentDeclaration node) {
            namespaceAliasList.Add(node);
            numberOfAliases++;
        }
        protected internal Saxon.Hej.om.NamespaceBinding getNamespaceAlias(string uri) {
            return Saxon.Impl.Helpers.MapUtils.Get(namespaceAliasMap, uri);
        }
        protected internal bool isAliasResultNamespace(string uri) {
            return aliasResultUriSet.Contains(uri);
        }
        private void collectNamespaceAliases() {
            namespaceAliasMap = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.NamespaceBinding>(numberOfAliases);
            aliasResultUriSet = new System.Collections.Generic.HashSet<string>(numberOfAliases);
            System.Collections.Generic.HashSet<string> aliasesAtThisPrecedence = new System.Collections.Generic.HashSet<string>();
            int currentPrecedence = -1;
            for (int i = 0; (i<numberOfAliases); i++) {
                Saxon.Hej.style.ComponentDeclaration decl = namespaceAliasList[i];
                Saxon.Hej.style.XSLNamespaceAlias xna = (Saxon.Hej.style.XSLNamespaceAlias)(decl.getSourceElement());
                string scode = xna.getStylesheetURI();
                Saxon.Hej.om.NamespaceBinding resultBinding = xna.getResultNamespaceBinding();
                int prec = decl.getPrecedence();
                if ((currentPrecedence != prec)) {
                    currentPrecedence = prec;
                    aliasesAtThisPrecedence.Clear();
                }
                if (aliasesAtThisPrecedence.Contains(scode)) {
                    if (!(Saxon.Impl.Helpers.MapUtils.Get(namespaceAliasMap, scode).getURI().Equals(resultBinding.getURI()))) {
                        xna.compileError("More than one alias is defined for the same namespace", "XTSE0810");
                    }
                }
                if ((Saxon.Impl.Helpers.MapUtils.Get(namespaceAliasMap, scode) == null)) {
                    Saxon.Impl.Helpers.MapUtils.Put(namespaceAliasMap, scode, resultBinding);
                    aliasResultUriSet.Add(resultBinding.getURI());
                }
                aliasesAtThisPrecedence.Add(scode);
            }
            namespaceAliasList = null;
        }
        protected internal bool hasNamespaceAliases() {
            return (numberOfAliases>0);
        }
        public Saxon.Ejava.util.Properties gatherOutputProperties(Saxon.Hej.om.StructuredQName formatQName) {
            bool found = (formatQName == null);
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Ejava.util.Properties details = new Saxon.Ejava.util.Properties(config.getDefaultSerializationProperties());
            System.Collections.Generic.Dictionary<string,int> precedences = new System.Collections.Generic.Dictionary<string, int>(10);
            for (int i = (topLevel.Count-1); (i>=0); i--) {
                Saxon.Hej.style.ComponentDeclaration decl = topLevel[i];
                if ((decl.getSourceElement() is Saxon.Hej.style.XSLOutput)) {
                    Saxon.Hej.style.XSLOutput xo = (Saxon.Hej.style.XSLOutput)(decl.getSourceElement());
                    if (((formatQName == null) ? (xo.getFormatQName() == null) : formatQName.Equals(xo.getFormatQName()))) {
                        found = true;
                        xo.gatherOutputProperties(details, precedences, decl.getPrecedence());
                    }
                }
            }
            if (!(found)) {
                compileError((("Requested output format "+formatQName.getDisplayName())+" has not been defined"), "XTDE1460");
            }
            return details;
        }
        protected internal void compile(Saxon.Hej.style.Compilation compilation) {
            try {
                Saxon.Hej.trans.Timer timer = compilation.timer;
                Saxon.Hej.Configuration config = getConfiguration();
                Saxon.Hej.query.XQueryFunctionLibrary queryFunctions = stylesheetPackage.getXQueryFunctionLibrary();
                foreach (Saxon.Hej.query.XQueryFunction f in queryFunctions.getFunctionDefinitions()) {
                    f.fixupReferences();
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("fixup Query functions");
                }
                bool allowImplicit = !(getStylesheetPackage().isDeclaredModes());
                foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                    Saxon.Hej.style.StyleElement snode = decl.getSourceElement();
                    if ((snode is Saxon.Hej.style.XSLMode)) {
                        getRuleManager().obtainMode(snode.getObjectName(), true);
                    }
                    if (allowImplicit) {
                        registerImplicitModes(snode, getRuleManager());
                    }
                }
                getRuleManager().checkConsistency();
                foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                    Saxon.Hej.style.StyleElement snode = decl.getSourceElement();
                    if ((snode is Saxon.Hej.style.XSLTemplate)) {
                        ((Saxon.Hej.style.XSLTemplate)(snode)).register(decl);
                    }
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("register templates");
                }
                adjustExposedVisibility();
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("adjust exposed visibility");
                }
                foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                    Saxon.Hej.style.StyleElement snode = decl.getSourceElement();
                    if (!(snode.isActionCompleted(Saxon.Hej.style.StyleElement.ACTION_COMPILE))) {
                        snode.setActionCompleted(Saxon.Hej.style.StyleElement.ACTION_COMPILE);
                        snode.compileDeclaration(compilation, decl);
                    }
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report((("compile top-level objects ("+topLevel.Count)+")"));
                }
                foreach (Saxon.Hej.style.ComponentDeclaration decl in functionIndex.Values) {
                    Saxon.Hej.style.StyleElement node = decl.getSourceElement();
                    if (!(node.isActionCompleted(Saxon.Hej.style.StyleElement.ACTION_TYPECHECK))) {
                        node.setActionCompleted(Saxon.Hej.style.StyleElement.ACTION_TYPECHECK);
                        if ((node.getVisibility() != Saxon.Hej.trans.Visibility.ABSTRACT)) {
                            ((Saxon.Hej.style.XSLFunction)(node)).getCompiledFunction().typeCheck(node.makeExpressionVisitor());
                        }
                    }
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report((("typeCheck functions ("+functionIndex.Count)+")"));
                }
                if ((compilation.getErrorCount()>0)) {
                    return ;
                }
                optimizeTopLevel();
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("optimize top level");
                }
                foreach (Saxon.Hej.style.ComponentDeclaration decl in functionIndex.Values) {
                    Saxon.Hej.style.StyleElement node = decl.getSourceElement();
                    if (!(node.isActionCompleted(Saxon.Hej.style.StyleElement.ACTION_OPTIMIZE))) {
                        node.setActionCompleted(Saxon.Hej.style.StyleElement.ACTION_OPTIMIZE);
                        ((Saxon.Hej.style.StylesheetComponent)(node)).optimize(decl);
                    }
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("optimize functions");
                }
                getDecimalFormatManager().checkConsistency();
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("check decimal formats");
                }
                Saxon.Hej.trans.rules.RuleManager ruleManager = getRuleManager();
                ruleManager.computeRankings();
                if (!(compilation.isFallbackToNonStreaming())) {
                    ruleManager.invertStreamableTemplates();
                }
                if (config.obtainOptimizer().isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.RULE_SET)) {
                    ruleManager.optimizeRules();
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("build template rule tables");
                }
                Saxon.Hej.functions.ExecutableFunctionLibrary overriding = new Saxon.Hej.functions.ExecutableFunctionLibrary(config);
                Saxon.Hej.functions.ExecutableFunctionLibrary underriding = new Saxon.Hej.functions.ExecutableFunctionLibrary(config);
                foreach (Saxon.Hej.expr.Component decl in stylesheetPackage.getComponentIndex().Values) {
                    Saxon.Hej.trans.Visibility vis = decl.getVisibility();
                    if ((decl.getActor() is Saxon.Hej.expr.instruct.UserFunction)) {
                        Saxon.Hej.expr.instruct.UserFunction f = (Saxon.Hej.expr.instruct.UserFunction)(decl.getActor());
                        if (f.isOverrideExtensionFunction()) {
                            overriding.addFunction(f);
                        } else {
                            underriding.addFunction(f);
                        }
                    }
                }
                getStylesheetPackage().setFunctionLibraryDetails(null, overriding, underriding);
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("build runtime function tables");
                }
                foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                    Saxon.Hej.style.StyleElement inst = decl.getSourceElement();
                    if ((inst is Saxon.Hej.style.XSLTemplate)) {
                        Saxon.Hej.expr.instruct.NamedTemplate proc = ((/*1430*/Saxon.Hej.expr.instruct.NamedTemplate)((Saxon.Hej.style.XSLTemplate)(inst)).getActor());
                        if (((proc != null) && (proc.getTemplateName() == null))) {
                            proc.allocateAllBindingSlots(stylesheetPackage);
                        }
                    }
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("allocate binding slots to named templates");
                }
                System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = stylesheetPackage.getComponentIndex();
                foreach (Saxon.Hej.expr.Component decl in componentIndex.Values) {
                    Saxon.Hej.expr.instruct.Actor proc = decl.getActor();
                    if ((proc != null)) {
                        proc.allocateAllBindingSlots(stylesheetPackage);
                    }
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("allocate binding slots to component references");
                }
                Saxon.Hej.trans.KeyManager keyMan = getKeyManager();
                foreach (Saxon.Hej.trans.KeyDefinitionSet keySet in keyMan.getAllKeyDefinitionSets()) {
                    foreach (Saxon.Hej.trans.KeyDefinition keyDef in keySet.getKeyDefinitions()) {
                        keyDef.makeDeclaringComponent(Saxon.Hej.trans.Visibility.PRIVATE, getStylesheetPackage());
                        keyDef.allocateAllBindingSlots(stylesheetPackage);
                    }
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("allocate binding slots to key definitions");
                }
                Saxon.Hej.expr.accum.AccumulatorRegistry accMan = getAccumulatorManager();
                if ((accMan != null)) {
                    foreach (Saxon.Hej.expr.accum.Accumulator acc in accMan.getAllAccumulators()) {
                        acc.allocateAllBindingSlots(stylesheetPackage);
                    }
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("allocate binding slots to accumulators");
                }
                if ((compilation.getCompilerInfo().isGenerateByteCode() && !(config.isDeferredByteCode(Saxon.Api.HostLanguage.XSLT)))) {
                    if (Saxon.Hej.style.Compilation.TIMING) {
                        config.getLogger().info("Generating byte code...");
                    }
                    Saxon.Hej.expr.parser.Optimizer opt = config.obtainOptimizer();
                    foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                        Saxon.Hej.style.StyleElement inst = decl.getSourceElement();
                        if ((inst is Saxon.Hej.style.StylesheetComponent)) {
                            ((Saxon.Hej.style.StylesheetComponent)(inst)).generateByteCode(opt);
                        }
                    }
                }
                if (Saxon.Hej.style.Compilation.TIMING) {
                    timer.report("inject byte code candidates");
                    timer.reportCumulative("total compile time");
                }
            } catch (System.Exception err) {
                if ((compilation.getErrorCount() == 0)) {
                    throw;
                }
            }
        }
        private void registerImplicitModes(Saxon.Hej.style.StyleElement element, Saxon.Hej.trans.rules.RuleManager manager) {
            if (((element is Saxon.Hej.style.XSLApplyTemplates) || (element is Saxon.Hej.style.XSLTemplate))) {
                string modeAtt = element.getAttributeValue("mode");
                if ((modeAtt != null)) {
                    string[] tokens = System.Text.RegularExpressions.Regex.Split(Saxon.Hej.value.Whitespace.trim(modeAtt), "[ \t\n\r]+");
                    foreach (string s in tokens) {
                        if (!(s.StartsWith("#"))) {
                            Saxon.Hej.om.StructuredQName modeName = element.makeQName(s, null, "mode");
                            Saxon.Hej.trans.SymbolicName sName = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_MODE, modeName);
                            System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = getStylesheetPackage().getComponentIndex();
                            Saxon.Hej.expr.Component existing = Saxon.Impl.Helpers.MapUtils.Get(componentIndex, sName);
                            if (((existing != null) && (existing.getDeclaringPackage() != getStylesheetPackage()))) {
                                if (((element is Saxon.Hej.style.XSLTemplate) && !(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)element.getParent()) is Saxon.Hej.style.XSLOverride))))) {
                                    element.compileError(("A template rule cannot be added to a mode declared in a used package "+"unless the xsl:template declaration appears within an xsl:override child of the appropriate xsl:use-package element"), "XTSE3050");
                                }
                            } else {
                                manager.obtainMode(modeName, true);
                            }
                        }
                    }
                }
            }
            Saxon.Hej.om.NodeInfo child;
            Saxon.Hej.tree.iter.AxisIterator kids = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
            while (((child = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next())) != null)) {
                if ((child is Saxon.Hej.style.StyleElement)) {
                    registerImplicitModes((Saxon.Hej.style.StyleElement)(child), manager);
                }
            }
        }
        public void optimizeTopLevel() {
            foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                Saxon.Hej.style.StyleElement node = decl.getSourceElement();
                if ((((node is Saxon.Hej.style.StylesheetComponent) && !(((node is Saxon.Hej.style.XSLFunction)))) && !(node.isActionCompleted(Saxon.Hej.style.StyleElement.ACTION_OPTIMIZE)))) {
                    node.setActionCompleted(Saxon.Hej.style.StyleElement.ACTION_OPTIMIZE);
                    ((Saxon.Hej.style.StylesheetComponent)(node)).optimize(decl);
                }
                if ((node is Saxon.Hej.style.XSLTemplate)) {
                    ((Saxon.Hej.style.XSLTemplate)(node)).allocatePatternSlotNumbers();
                }
            }
        }
        protected internal bool isImportedSchema(string targetNamespace) {
            return stylesheetPackage.getSchemaNamespaces().Contains(targetNamespace);
        }
        protected internal void addImportedSchema(string targetNamespace) {
            stylesheetPackage.getSchemaNamespaces().Add(targetNamespace);
        }
        protected internal System.Collections.Generic.ISet<string> getImportedSchemaTable() {
            return stylesheetPackage.getSchemaNamespaces();
        }
        public Saxon.Hej.style.ComponentDeclaration getCharacterMap(Saxon.Hej.om.StructuredQName name) {
            for (int i = (topLevel.Count-1); (i>=0); i--) {
                Saxon.Hej.style.ComponentDeclaration decl = topLevel[i];
                if ((decl.getSourceElement() is Saxon.Hej.style.XSLCharacterMap)) {
                    Saxon.Hej.style.XSLCharacterMap t = (Saxon.Hej.style.XSLCharacterMap)(decl.getSourceElement());
                    if (t.getCharacterMapName().Equals(name)) {
                        return decl;
                    }
                }
            }
            return null;
        }
        public void adjustExposedVisibility() {
            System.Collections.Generic.IList<Saxon.Hej.style.XSLExpose> exposeDeclarations = new System.Collections.Generic.List<Saxon.Hej.style.XSLExpose>();
            foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                if ((decl.getSourceElement() is Saxon.Hej.style.XSLExpose)) {
                    exposeDeclarations.Insert(0, (Saxon.Hej.style.XSLExpose)(decl.getSourceElement()));
                }
            }
            if (exposeDeclarations.Count==0) {
                return ;
            }
            Saxon.Hej.om.NamePool pool = getConfiguration().getNamePool();
            System.Collections.Generic.IDictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = stylesheetPackage.getComponentIndex();
            foreach (Saxon.Hej.expr.Component component in componentIndex.Values) {
                int fp = component.getComponentKind();
                if (((fp == Saxon.Hej.om.StandardNames.XSL_MODE) && ((Saxon.Hej.trans.Mode)(component.getActor())).isUnnamedMode())) {
                    continue ;
                }
                Saxon.Hej.trans.ComponentTest exactNameTest = new Saxon.Hej.trans.ComponentTest(fp, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, new Saxon.Hej.om.FingerprintedQName(component.getActor().getComponentName(), pool), pool), -1);
                Saxon.Hej.trans.ComponentTest exactFunctionTest = null;
                if ((fp == Saxon.Hej.om.StandardNames.XSL_FUNCTION)) {
                    Saxon.Hej.om.Function fn = (Saxon.Hej.om.Function)(component.getActor());
                    exactFunctionTest = new Saxon.Hej.trans.ComponentTest(fp, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, new Saxon.Hej.om.FingerprintedQName(fn.getFunctionName(), pool), pool), fn.getArity());
                }
                bool matched = false;
                foreach (Saxon.Hej.style.XSLExpose exposure in exposeDeclarations) {
                    System.Collections.Generic.ISet<Saxon.Hej.trans.ComponentTest> explicitComponentTests = exposure.getExplicitComponentTests();
                    if ((explicitComponentTests.Contains(exactNameTest) || (((exactFunctionTest != null) && explicitComponentTests.Contains(exactFunctionTest))))) {
                        component.setVisibility(exposure.getVisibility(), Saxon.Hej.trans.VisibilityProvenance.EXPOSED);
                        matched = true;
                        break ;
                    }
                }
                if ((!(matched) && (component.getVisibilityProvenance() == Saxon.Hej.trans.VisibilityProvenance.DEFAULTED))) {
                    matched = lookForMatchingWildcard(exposeDeclarations, component, matched);
                    if (!(matched)) {
                        lookForAnyWildcard(exposeDeclarations, component);
                    }
                }
            }
        }
        private void lookForAnyWildcard(System.Collections.Generic.IList<Saxon.Hej.style.XSLExpose> exposeDeclarations, Saxon.Hej.expr.Component component) {
            foreach (Saxon.Hej.style.XSLExpose exposure in exposeDeclarations) {
                foreach (Saxon.Hej.trans.ComponentTest test in exposure.getWildcardComponentTests()) {
                    if (test.matches(component.getActor())) {
                        if (((exposure.getVisibility() == Saxon.Hej.trans.Visibility.ABSTRACT) && (component.getVisibility() != Saxon.Hej.trans.Visibility.ABSTRACT))) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The non-abstract component "+component.getActor().getSymbolicName())+" cannot be made abstract by means of xsl:expose"), "XTSE3025");
                            err.setLocation(exposure);
                            throw err;
                        }
                        component.setVisibility(exposure.getVisibility(), Saxon.Hej.trans.VisibilityProvenance.EXPOSED);
                        return ;
                    }
                }
            }
        }
        private bool lookForMatchingWildcard(System.Collections.Generic.IList<Saxon.Hej.style.XSLExpose> exposeDeclarations, Saxon.Hej.expr.Component component, bool matched) {
            foreach (Saxon.Hej.style.XSLExpose exposure in exposeDeclarations) {
                foreach (Saxon.Hej.trans.ComponentTest test in exposure.getWildcardComponentTests()) {
                    if ((test.isPartialWildcard() && test.matches(component.getActor()))) {
                        if (((exposure.getVisibility() == Saxon.Hej.trans.Visibility.ABSTRACT) && (component.getVisibility() != Saxon.Hej.trans.Visibility.ABSTRACT))) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The non-abstract component "+component.getActor().getSymbolicName())+" cannot be made abstract by means of xsl:expose"), "XTSE3025");
                            err.setLocation(exposure);
                            throw err;
                        }
                        component.setVisibility(exposure.getVisibility(), Saxon.Hej.trans.VisibilityProvenance.EXPOSED);
                        return true;
                    }
                }
            }
            return matched;
        }
        protected internal void compileError(string message, string errorCode) {
            Saxon.Hej.trans.XPathException tce = new Saxon.Hej.trans.XPathException(message, errorCode);
            compileError(tce);
        }
        protected internal void compileError(Saxon.Hej.trans.XPathException error) {
            error.setIsStaticError(true);
            getRootElement().compileError(error);
        }
        protected internal void fixup() {
            foreach (Saxon.Hej.om.Action a in fixupActions) {
                /*Delegate.doAction()*/a();
            }
        }
        protected internal void complete() {
            stylesheetPackage.complete();
        }
        public Saxon.Hej.expr.instruct.SlotManager getSlotManager() {
            return null;
        }
        public virtual Saxon.Hej.expr.instruct.GlobalVariable getEquivalentVariable(Saxon.Hej.expr.Expression select) {
            return null;
        }
        public virtual void addGlobalVariable(Saxon.Hej.expr.instruct.GlobalVariable variable) {
            addGlobalVariable(variable, Saxon.Hej.trans.Visibility.PRIVATE);
        }
        public void addGlobalVariable(Saxon.Hej.expr.instruct.GlobalVariable variable, Saxon.Hej.trans.Visibility visibility) {
            Saxon.Hej.expr.Component component = variable.makeDeclaringComponent(visibility, getStylesheetPackage());
            if ((variable.getPackageData() == null)) {
                variable.setPackageData(stylesheetPackage);
            }
            if ((visibility == Saxon.Hej.trans.Visibility.HIDDEN)) {
                stylesheetPackage.addHiddenComponent(component);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(stylesheetPackage.getComponentIndex(), new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_VARIABLE, variable.getVariableQName()), component);
            }
        }
    
    }
}