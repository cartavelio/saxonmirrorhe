using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class EvaluateInstr : Saxon.Hej.expr.Expression {
        private Saxon.Hej.expr.Operand xpathOp;
        private readonly Saxon.Hej.value.SequenceType requiredType;
        private Saxon.Hej.expr.Operand contextItemOp;
        private Saxon.Hej.expr.Operand baseUriOp;
        private Saxon.Hej.expr.Operand namespaceContextOp;
        private Saxon.Hej.expr.Operand schemaAwareOp;
        private Saxon.Hej.expr.Operand optionsOp;
        private System.Collections.Generic.ISet<string> importedSchemaNamespaces = new System.Collections.Generic.HashSet<string>();
        private Saxon.Hej.expr.instruct.WithParam[] actualParams;
        private Saxon.Hej.expr.Operand dynamicParamsOp;
        private string defaultXPathNamespace = null;
        public EvaluateInstr(Saxon.Hej.expr.Expression xpath, Saxon.Hej.value.SequenceType requiredType, Saxon.Hej.expr.Expression contextItemExpr, Saxon.Hej.expr.Expression baseUriExpr, Saxon.Hej.expr.Expression namespaceContextExpr, Saxon.Hej.expr.Expression schemaAwareExpr)  {
            if ((xpath != null)) {
                xpathOp = new Saxon.Hej.expr.Operand(this, xpath, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            if ((contextItemExpr != null)) {
                contextItemOp = new Saxon.Hej.expr.Operand(this, contextItemExpr, Saxon.Hej.expr.OperandRole.NAVIGATE);
            }
            if ((baseUriExpr != null)) {
                baseUriOp = new Saxon.Hej.expr.Operand(this, baseUriExpr, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            if ((namespaceContextExpr != null)) {
                namespaceContextOp = new Saxon.Hej.expr.Operand(this, namespaceContextExpr, Saxon.Hej.expr.OperandRole.INSPECT);
            }
            if ((schemaAwareExpr != null)) {
                schemaAwareOp = new Saxon.Hej.expr.Operand(this, schemaAwareExpr, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            this.requiredType = requiredType;
        }
        public void setOptionsExpression(Saxon.Hej.expr.Expression options) {
            optionsOp = new Saxon.Hej.expr.Operand(this, options, Saxon.Hej.expr.OperandRole.ABSORB);
        }
        public void setActualParameters(Saxon.Hej.expr.instruct.WithParam[] @params) {
            setActualParams(@params);
        }
        public void setDefaultXPathNamespace(string defaultXPathNamespace) {
            this.defaultXPathNamespace = defaultXPathNamespace;
        }
        public override bool isInstruction() {
            return true;
        }
        public void importSchemaNamespace(string ns) {
            if ((importedSchemaNamespaces == null)) {
                importedSchemaNamespaces = new System.Collections.Generic.HashSet<string>();
            }
            importedSchemaNamespaces.Add(ns);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            importedSchemaNamespaces = visitor.getStaticContext().getImportedSchemaNamespaces();
            typeCheckChildren(visitor, contextInfo);
            Saxon.Hej.expr.instruct.WithParam.typeCheck(getActualParams(), visitor, contextInfo);
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            optimizeChildren(visitor, contextItemType);
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return requiredType.getPrimaryType();
        }
        protected internal override int computeCardinality() {
            return requiredType.getCardinality();
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot do document projection when xsl:evaluate is used");
        }
        public override int getIntrinsicDependencies() {
            return (Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_XSLT_CONTEXT);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> sub = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(8);
            if ((xpathOp != null)) {
                sub.Add(xpathOp);
            }
            if ((contextItemOp != null)) {
                sub.Add(contextItemOp);
            }
            if ((baseUriOp != null)) {
                sub.Add(baseUriOp);
            }
            if ((namespaceContextOp != null)) {
                sub.Add(namespaceContextOp);
            }
            if ((schemaAwareOp != null)) {
                sub.Add(schemaAwareOp);
            }
            if ((dynamicParamsOp != null)) {
                sub.Add(dynamicParamsOp);
            }
            if ((optionsOp != null)) {
                sub.Add(optionsOp);
            }
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(this, getActualParams(), sub);
            return sub;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.EvaluateInstr e2 = new Saxon.Hej.expr.instruct.EvaluateInstr(getXpath().copy(rebindings), requiredType, getContextItemExpr().copy(rebindings), ((getBaseUriExpr() == null) ? null : getBaseUriExpr().copy(rebindings)), ((getNamespaceContextExpr() == null) ? null : getNamespaceContextExpr().copy(rebindings)), ((getSchemaAwareExpr() == null) ? null : getSchemaAwareExpr().copy(rebindings)));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e2);
            e2.setRetainedStaticContext(getRetainedStaticContext());
            e2.importedSchemaNamespaces = importedSchemaNamespaces;
            e2.setActualParams(Saxon.Hej.expr.instruct.WithParam.copy(e2, getActualParams(), rebindings));
            if ((optionsOp != null)) {
                e2.setOptionsExpression(optionsOp.getChildExpression().copy(rebindings));
            }
            if ((dynamicParamsOp != null)) {
                e2.setDynamicParams(dynamicParamsOp.getChildExpression().copy(rebindings));
            }
            return e2;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Configuration config = context.getConfiguration();
            if (config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.DISABLE_XSL_EVALUATE)) {
                throw new Saxon.Hej.trans.XPathException("xsl:evaluate has been disabled", "XTDE3175");
            }
            string exprText = getXpath().evaluateAsString(context).ToString();
            string baseUri = ((getBaseUriExpr() == null) ? getStaticBaseURIString() : Saxon.Hej.value.Whitespace.trim(getBaseUriExpr().evaluateAsString(context).ToString()));
            Saxon.Hej.om.Item focus = getContextItemExpr().evaluateItem(context);
            Saxon.Hej.om.NodeInfo namespaceContextBase = null;
            if ((getNamespaceContextExpr() != null)) {
                namespaceContextBase = (Saxon.Hej.om.NodeInfo)(getNamespaceContextExpr().evaluateItem(context));
            }
            string schemaAwareAttr = Saxon.Hej.value.Whitespace.trim(getSchemaAwareExpr().evaluateAsString(context).ToString());
            bool isSchemaAware;
            if ((("yes".Equals(schemaAwareAttr) || "true".Equals(schemaAwareAttr)) || "1".Equals(schemaAwareAttr))) {
                isSchemaAware = true;
            } else {
                if ((("no".Equals(schemaAwareAttr) || "false".Equals(schemaAwareAttr)) || "0".Equals(schemaAwareAttr))) {
                    isSchemaAware = false;
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The schema-aware attribute of xsl:evaluate must be yes|no|true|false|0|1");
                    err.setErrorCode("XTDE0030");
                    err.setLocation(getLocation());
                    err.setXPathContext(context);
                    throw err;
                }
            }
            Saxon.Hej.expr.Expression expr = null;
            Saxon.Hej.expr.instruct.SlotManager slotMap = null;
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(((exprText.Length+(((baseUri == null) ? 4 : baseUri.Length)))+40));
            fsb.append(baseUri);
            fsb.append("##");
            fsb.append(schemaAwareAttr);
            fsb.append("##");
            fsb.append(exprText);
            if ((namespaceContextBase != null)) {
                fsb.append("##");
                namespaceContextBase.generateId(fsb);
            }
            string cacheKey = fsb.ToString();
            System.Collections.Generic.ICollection<Saxon.Hej.sxpath.XPathVariable> declaredVars = null;
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.expr.sort.LFUCache<string,System.Object[]> cache;
            lock (controller) {
                cache = (Saxon.Hej.expr.sort.LFUCache<string,System.Object[]>)(controller.getUserData(this.getLocation(), "xsl:evaluate"));
                if ((cache == null)) {
                    cache = new Saxon.Hej.expr.sort.LFUCache<string, System.Object[]>(100);
                    controller.setUserData(this.getLocation(), "xsl:evaluate", cache);
                } else {
                    System.Object[] o = cache.get(cacheKey);
                    if ((o != null)) {
                        expr = (Saxon.Hej.expr.Expression)(o[0]);
                        slotMap = (Saxon.Hej.expr.instruct.SlotManager)(o[1]);
                        declaredVars = (System.Collections.Generic.ICollection<Saxon.Hej.sxpath.XPathVariable>)(o[2]);
                    }
                }
            }
            Saxon.Hej.ma.map.MapItem dynamicParams = null;
            if ((dynamicParamsOp != null)) {
                dynamicParams = (Saxon.Hej.ma.map.MapItem)(dynamicParamsOp.getChildExpression().evaluateItem(context));
            }
            if ((expr == null)) {
                int version = getRetainedStaticContext().getPackageData().getHostLanguageVersion();
                Saxon.Hej.ma.map.MapItem options = (((optionsOp == null) ? new Saxon.Impl.DataModel.ImmMap() : (Saxon.Hej.ma.map.MapItem)(optionsOp.getChildExpression().evaluateItem(context))));
                Saxon.Hej.sxpath.IndependentContext env = new Saxon.Hej.sxpath.IndependentContext(config);
                env.setWarningHandler((str, loc) => {
                    string message = ((("In dynamic expression {"+exprText)+"}: ")+str);
                    context.getController().warning(message, null, loc);
                } );
                env.setBaseURI(baseUri);
                env.setExecutable(context.getController().getExecutable());
                env.setXPathLanguageLevel(((version == 40) ? 40 : config.getConfigurationProperty(Saxon.Hej.lib.Feature<int>.XPATH_VERSION_FOR_XSLT)));
                env.setDefaultCollationName(getRetainedStaticContext().getDefaultCollationName());
                if ((getNamespaceContextExpr() != null)) {
                    env.setNamespaces(namespaceContextBase);
                } else {
                    env.setNamespaceResolver(getRetainedStaticContext());
                    env.setDefaultElementNamespace(getRetainedStaticContext().getDefaultElementNamespace());
                }
                Saxon.Hej.functions.FunctionLibraryList libraryList0 = ((Saxon.Hej.style.StylesheetPackage)(getRetainedStaticContext().getPackageData())).getFunctionLibrary();
                Saxon.Hej.functions.FunctionLibraryList libraryList1 = new Saxon.Hej.functions.FunctionLibraryList();
                foreach (Saxon.Hej.functions.FunctionLibrary lib in libraryList0.getLibraryList()) {
                    if (((lib is Saxon.Hej.functions.registry.BuiltInFunctionSet) && ((Saxon.Hej.functions.registry.BuiltInFunctionSet)(lib)).getNamespace().Equals(Saxon.Hej.lib.NamespaceConstant.FN))) {
                        libraryList1.addFunctionLibrary(((version == 40) ? Saxon.Hej.functions.registry.XPath40FunctionSet.getInstance() : Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance()));
                    } else {
                        if (((lib is Saxon.Hej.style.StylesheetFunctionLibrary) || (lib is Saxon.Hej.functions.ExecutableFunctionLibrary))) {
                            libraryList1.addFunctionLibrary(new Saxon.Hej.style.PublicStylesheetFunctionLibrary(lib));
                        } else {
                            libraryList1.addFunctionLibrary(lib);
                        }
                    }
                }
                env.setFunctionLibrary(libraryList1);
                env.setDecimalFormatManager(getRetainedStaticContext().getDecimalFormatManager());
                if (isSchemaAware) {
                    Saxon.Hej.om.GroundedValue allowAny = options.get(Saxon.Hej.value.StringValue.bmp("allow-any-namespace"));
                    if (((allowAny != null) && allowAny.effectiveBooleanValue())) {
                        env.setImportedSchemaNamespaces(config.getImportedNamespaces());
                    } else {
                        env.setImportedSchemaNamespaces(importedSchemaNamespaces);
                    }
                }
                Saxon.Hej.om.GroundedValue defaultCollation = options.get(Saxon.Hej.value.StringValue.bmp("default-collation"));
                if ((defaultCollation != null)) {
                    env.setDefaultCollationName(defaultCollation.head().getStringValue());
                }
                System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> locals = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, int>();
                if ((dynamicParams != null)) {
                    Saxon.Hej.om.SequenceTool.supply(dynamicParams.keys(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((paramName => {
                        if (!(((paramName is Saxon.Hej.value.QNameValue)))) {
                            Saxon.Hej.type.AtomicType primitiveItemType = ((Saxon.Hej.value.AtomicValue)(paramName)).getPrimitiveType();
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Parameter names supplied to xsl:evaluate must have type xs:QName, not "+primitiveItemType.getDisplayName()), "XTTE3165");
                            err.setIsTypeError(true);
                            throw err;
                        }
                        Saxon.Hej.sxpath.XPathVariable var = env.declareVariable((Saxon.Hej.value.QNameValue)(paramName));
                        Saxon.Impl.Helpers.MapUtils.Put(locals, ((Saxon.Hej.value.QNameValue)(paramName)).getStructuredQName(), var.getLocalSlotNumber());
                    } )));
                }
                if ((getActualParams() != null)) {
                    foreach (Saxon.Hej.expr.instruct.WithParam actualParam in getActualParams()) {
                        Saxon.Hej.om.StructuredQName name = actualParam.getVariableQName();
                        if (!(locals.ContainsKey(name))) {
                            Saxon.Hej.sxpath.XPathVariable var = env.declareVariable(name);
                            Saxon.Impl.Helpers.MapUtils.Put(locals, name, var.getLocalSlotNumber());
                        }
                    }
                }
                try {
                    expr = Saxon.Hej.expr.parser.ExpressionTool.make(exprText, env, 0, Saxon.Hej.expr.parser.Token.EOF, null);
                } catch (Saxon.Hej.trans.XPathException e) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Static error in XPath expression supplied to xsl:evaluate: "+e.Message)+". Expression: {")+exprText)+"}"));
                    err.setErrorCode("XTDE3160");
                    err.setLocation(getLocation());
                    throw err;
                }
                expr.setRetainedStaticContext(env.makeRetainedStaticContext());
                Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.EVALUATE_RESULT, exprText, 0);
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(env);
                Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
                expr = tc.staticTypeCheck(expr, requiredType, role, visitor);
                Saxon.Hej.type.ItemType contextItemType = Saxon.Hej.type.Type.ITEM_TYPE;
                expr = Saxon.Hej.expr.parser.ExpressionTool.resolveCallsToCurrentFunction(expr);
                Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(contextItemType, (context.getContextItem() == null));
                expr = expr.typeCheck(visitor, cit).optimize(visitor, cit);
                slotMap = env.getStackFrameMap();
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(expr, slotMap.getNumberOfVariables(), slotMap);
                if ((cacheKey != null)) {
                    declaredVars = env.getDeclaredVariables();
                    cache.put(cacheKey, new System.Object[]{expr, slotMap, declaredVars});
                }
            }
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            if ((focus == null)) {
                c2.setCurrentIterator(null);
            } else {
                Saxon.Hej.tree.iter.ManualIterator mono = new Saxon.Hej.tree.iter.ManualIterator(focus);
                c2.setCurrentIterator(mono);
            }
            c2.openStackFrame(slotMap);
            if ((getActualParams() != null)) {
                for (int i = 0; (i<getActualParams().Length); i++) {
                    int slot = slotMap.getVariableMap().IndexOf(getActualParams()[i].getVariableQName());
                    c2.setLocalVariable(slot, getActualParams()[i].getSelectValue(context));
                }
            }
            if ((dynamicParams != null)) {
                Saxon.Hej.tree.iter.AtomicIterator iter = dynamicParams.keys();
                Saxon.Hej.value.QNameValue paramName;
                while (((paramName = (Saxon.Hej.value.QNameValue)(((/*1430*/Saxon.Hej.value.AtomicValue)iter.next()))) != null)) {
                    int slot = slotMap.getVariableMap().IndexOf(paramName.getStructuredQName());
                    if ((slot>=0)) {
                        c2.setLocalVariable(slot, dynamicParams.get(paramName));
                    }
                }
            }
            foreach (Saxon.Hej.sxpath.XPathVariable var in declaredVars) {
                Saxon.Hej.om.StructuredQName name = var.getVariableQName();
                System.Predicate<Saxon.Hej.expr.Expression> nameMatch = e => (((e is Saxon.Hej.expr.LocalVariableReference) && ((Saxon.Hej.expr.LocalVariableReference)(e)).getVariableName().Equals(name)) && (((Saxon.Hej.expr.LocalVariableReference)(e)).getBinding() is Saxon.Hej.sxpath.XPathVariable));
                if (((((dynamicParams != null) && (dynamicParams.get(new Saxon.Hej.value.QNameValue(name, Saxon.Hej.type.BuiltInAtomicType.QNAME)) == null)) && !(isActualParam(name))) && Saxon.Hej.expr.parser.ExpressionTool.contains(expr, false, nameMatch))) {
                    throw new Saxon.Hej.trans.XPathException(("No value has been supplied for variable "+name.getDisplayName()), "XPST0008");
                }
            }
            try {
                return expr.iterate(c2);
            } catch (Saxon.Hej.trans.XPathException err) {
                Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException((("Dynamic error in expression {"+exprText)+"} called using xsl:evaluate"), err);
                e2.setLocation(getLocation());
                e2.setErrorCodeQName(err.getErrorCodeQName());
                throw e2;
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("evaluate", this);
            if (!(Saxon.Hej.value.SequenceType.ANY_SEQUENCE.Equals(requiredType))) {
                @out.emitAttribute("as", requiredType.toAlphaCode());
            }
            if (((importedSchemaNamespaces != null) && !(importedSchemaNamespaces.Count==0))) {
                Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(256);
                foreach (string s in importedSchemaNamespaces) {
                    buff.append((s.Length==0 ? "##" : s));
                    buff.append(' ');
                }
                buff.setLength((buff.length()-1));
                @out.emitAttribute("schNS", buff.ToString());
            }
            if ((defaultXPathNamespace != null)) {
                @out.emitAttribute("dxns", defaultXPathNamespace);
            }
            @out.setChildRole("xpath");
            getXpath().export(@out);
            if ((getContextItemExpr() != null)) {
                @out.setChildRole("cxt");
                getContextItemExpr().export(@out);
            }
            if ((getBaseUriExpr() != null)) {
                @out.setChildRole("baseUri");
                getBaseUriExpr().export(@out);
            }
            if ((getNamespaceContextExpr() != null)) {
                @out.setChildRole("nsCxt");
                getNamespaceContextExpr().export(@out);
            }
            if ((getSchemaAwareExpr() != null)) {
                @out.setChildRole("sa");
                getSchemaAwareExpr().export(@out);
            }
            if ((optionsOp != null)) {
                @out.setChildRole("options");
                optionsOp.getChildExpression().export(@out);
            }
            Saxon.Hej.expr.instruct.WithParam.exportParameters(actualParams, @out, false);
            if ((dynamicParamsOp != null)) {
                @out.setChildRole("wp");
                getDynamicParams().export(@out);
            }
            @out.endElement();
        }
        public Saxon.Hej.expr.Expression getXpath() {
            return xpathOp.getChildExpression();
        }
        public void setXpath(Saxon.Hej.expr.Expression xpath) {
            if ((xpathOp == null)) {
                xpathOp = new Saxon.Hej.expr.Operand(this, xpath, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            } else {
                xpathOp.setChildExpression(xpath);
            }
        }
        public Saxon.Hej.expr.Expression getContextItemExpr() {
            return ((contextItemOp == null) ? null : contextItemOp.getChildExpression());
        }
        public void setContextItemExpr(Saxon.Hej.expr.Expression contextItemExpr) {
            if ((contextItemOp == null)) {
                contextItemOp = new Saxon.Hej.expr.Operand(this, contextItemExpr, Saxon.Hej.expr.OperandRole.NAVIGATE);
            } else {
                contextItemOp.setChildExpression(contextItemExpr);
            }
        }
        public Saxon.Hej.expr.Expression getBaseUriExpr() {
            return ((baseUriOp == null) ? null : baseUriOp.getChildExpression());
        }
        public void setBaseUriExpr(Saxon.Hej.expr.Expression baseUriExpr) {
            if ((baseUriOp == null)) {
                baseUriOp = new Saxon.Hej.expr.Operand(this, baseUriExpr, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            } else {
                baseUriOp.setChildExpression(baseUriExpr);
            }
        }
        public Saxon.Hej.expr.Expression getNamespaceContextExpr() {
            return ((namespaceContextOp == null) ? null : namespaceContextOp.getChildExpression());
        }
        public void setNamespaceContextExpr(Saxon.Hej.expr.Expression namespaceContextExpr) {
            if ((namespaceContextOp == null)) {
                namespaceContextOp = new Saxon.Hej.expr.Operand(this, namespaceContextExpr, Saxon.Hej.expr.OperandRole.INSPECT);
            } else {
                namespaceContextOp.setChildExpression(namespaceContextExpr);
            }
        }
        public Saxon.Hej.expr.Expression getSchemaAwareExpr() {
            return ((schemaAwareOp == null) ? null : schemaAwareOp.getChildExpression());
        }
        public void setSchemaAwareExpr(Saxon.Hej.expr.Expression schemaAwareExpr) {
            if ((schemaAwareOp == null)) {
                schemaAwareOp = new Saxon.Hej.expr.Operand(this, schemaAwareExpr, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            } else {
                schemaAwareOp.setChildExpression(schemaAwareExpr);
            }
        }
        public Saxon.Hej.expr.instruct.WithParam[] getActualParams() {
            return actualParams;
        }
        public void setActualParams(Saxon.Hej.expr.instruct.WithParam[] actualParams) {
            this.actualParams = actualParams;
        }
        public bool isActualParam(Saxon.Hej.om.StructuredQName name) {
            foreach (Saxon.Hej.expr.instruct.WithParam wp in actualParams) {
                if (wp.getVariableQName().Equals(name)) {
                    return true;
                }
            }
            return false;
        }
        public void setDynamicParams(Saxon.Hej.expr.Expression @params) {
            if ((dynamicParamsOp == null)) {
                dynamicParamsOp = new Saxon.Hej.expr.Operand(this, @params, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            } else {
                dynamicParamsOp.setChildExpression(@params);
            }
        }
        public Saxon.Hej.expr.Expression getDynamicParams() {
            return dynamicParamsOp.getChildExpression();
        }
    
    }
}