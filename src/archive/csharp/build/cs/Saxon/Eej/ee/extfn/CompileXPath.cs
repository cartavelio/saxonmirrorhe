using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn {
    public class CompileXPath : Saxon.Hej.functions.SystemFunction {
        private const string exportURI = "http://ns.saxonica.com/xslt/export";
        public override Saxon.Hej.ma.map.MapItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.Configuration configuration = controller.getConfiguration();
            Saxon.Hej.value.AtomicValue content = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            Saxon.Hej.ma.map.MapItem options = (Saxon.Hej.ma.map.MapItem)(arguments[1].head());
            Saxon.Hej.ma.map.MapItem locals = (Saxon.Hej.ma.map.MapItem)(arguments[2].head());
            Saxon.Hej.value.StringValue type = (Saxon.Hej.value.StringValue)(options.get(Saxon.Hej.value.StringValue.bmp("type")));
            string exprText = content.getStringValue();
            Saxon.Hej.sxpath.IndependentContext env = new Saxon.Hej.sxpath.IndependentContext(configuration);
            env.setWarningHandler((msg, locator) => {
                string message = ((("In dynamic expression {"+exprText)+"}: ")+msg);
                context.getController().warning(message, null, null);
            } );
            Saxon.Hej.value.BooleanValue backwards = (Saxon.Hej.value.BooleanValue)(options.get(Saxon.Hej.value.StringValue.bmp("backwardCompatibility")));
            env.setBackwardsCompatibilityMode(((backwards != null) && backwards.getBooleanValue()));
            bool inCatch;
            Saxon.Hej.value.BooleanValue inCatchVal = (Saxon.Hej.value.BooleanValue)(options.get(Saxon.Hej.value.StringValue.bmp("inCatch")));
            inCatch = ((inCatchVal != null) && inCatchVal.getBooleanValue());
            Saxon.Hej.value.StringValue contextType = (Saxon.Hej.value.StringValue)(options.get(Saxon.Hej.value.StringValue.bmp("context-type")));
            Saxon.Hej.type.ItemType contextItemType = Saxon.Hej.type.AnyItemType.getInstance();
            Saxon.Hej.ma.map.MapItem namespaces = (Saxon.Hej.ma.map.MapItem)(options.get(Saxon.Hej.value.StringValue.bmp("namespaces")));
            if ((namespaces != null)) {
                Saxon.Hej.tree.iter.AtomicIterator keys = namespaces.keys();
                Saxon.Hej.value.AtomicValue key;
                while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keys.next())) != null)) {
                    string prefix = key.getStringValue();
                    string uri = namespaces.get(key).getUnicodeStringValue().ToString();
                    if (uri.Equals("~")) {
                        uri = Saxon.Hej.lib.NamespaceConstant.getUriForConventionalPrefix(prefix);
                    }
                    if ((uri == null)) {
                        switch (prefix) {
                          case "map":
                            uri = Saxon.Hej.lib.NamespaceConstant.MAP_FUNCTIONS;
                            break ;
                          case "array":
                            uri = Saxon.Hej.lib.NamespaceConstant.ARRAY_FUNCTIONS;
                            break ;
                          default:
                            throw new Saxon.Hej.trans.XPathException(("Unknown URI for defaulted prefix "+prefix));
                        }
                    }
                    env.declareNamespace(prefix, uri);
                }
            }
            Saxon.Hej.ma.map.MapItem globals = (Saxon.Hej.ma.map.MapItem)(options.get(Saxon.Hej.value.StringValue.bmp("globals")));
            System.Collections.Generic.List<string> knownGlobals = new System.Collections.Generic.List<string>();
            if ((globals != null)) {
                Saxon.Hej.tree.iter.AtomicIterator keys = globals.keys();
                Saxon.Hej.value.AtomicValue key;
                while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keys.next())) != null)) {
                    knownGlobals.Add(key.getStringValue());
                    Saxon.Hej.om.NodeInfo n = (Saxon.Hej.om.NodeInfo)(globals.get(key));
                    string t = n.getAttributeValue("", "sType");
                    Saxon.Hej.sxpath.XPathVariable v = env.declareVariable(Saxon.Hej.om.StructuredQName.fromClarkName(key.getStringValue()));
                    v.setRequiredType(makeType(t));
                }
            }
            foreach (Saxon.Hej.om.StructuredQName errorVariable in Saxon.Hej.om.StandardNames.errorVariables) {
                env.declareVariable(errorVariable);
            }
            int maxSlot = 0;
            if ((locals != null)) {
                Saxon.Hej.tree.iter.AtomicIterator keys = locals.keys();
                Saxon.Hej.value.AtomicValue key;
                while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keys.next())) != null)) {
                    Saxon.Hej.value.SequenceExtent s = (Saxon.Hej.value.SequenceExtent)(locals.get(key));
                    string staticType = s.itemAt(2).getStringValue();
                    int slot = (int)(((Saxon.Hej.value.Int64Value)(s.itemAt(1))).longValue());
                    if ((slot>maxSlot)) {
                        maxSlot = slot;
                    }
                    Saxon.Hej.sxpath.XPathVariable v = env.declareVariable(Saxon.Hej.om.StructuredQName.fromClarkName(key.getStringValue()));
                    v.setSlotNumber(slot);
                    v.setRequiredType(makeType(staticType));
                }
            }
            Saxon.Hej.functions.FunctionLibraryList libraryList = new Saxon.Hej.functions.FunctionLibraryList();
            Saxon.Hej.functions.ExecutableFunctionLibrary userFunctionLibrary = new Saxon.Eej.ee.extfn.CompileXPath.LocalFunctionLibrary(context.getConfiguration());
            Saxon.Hej.functions.FunctionLibraryList libraryList0 = ((Saxon.Hej.style.StylesheetPackage)(getRetainedStaticContext().getPackageData())).getFunctionLibrary();
            foreach (Saxon.Hej.functions.FunctionLibrary lib in libraryList0.getLibraryList()) {
                if (((lib is Saxon.Hej.functions.registry.BuiltInFunctionSet) && ((Saxon.Hej.functions.registry.BuiltInFunctionSet)(lib)).getNamespace().Equals(Saxon.Hej.lib.NamespaceConstant.FN))) {
                    libraryList.addFunctionLibrary(Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance());
                    libraryList.addFunctionLibrary(Saxon.Hej.functions.registry.XSLT30FunctionSet.getInstance());
                    libraryList.addFunctionLibrary(new Saxon.Eej.ee.extfn.VendorFunctionSetEE((Saxon.Eej.config.EnterpriseConfiguration)(configuration)));
                } else {
                    if (((lib is Saxon.Hej.style.StylesheetFunctionLibrary) || (lib is Saxon.Hej.functions.ExecutableFunctionLibrary))) {
                        libraryList.addFunctionLibrary(new Saxon.Hej.style.PublicStylesheetFunctionLibrary(lib));
                    } else {
                        libraryList.addFunctionLibrary(lib);
                    }
                }
            }
            Saxon.Hej.ma.map.MapItem globalFunctions = (Saxon.Hej.ma.map.MapItem)(options.get(Saxon.Hej.value.StringValue.bmp("globalFunctions")));
            Saxon.Hej.expr.parser.XPathParser parser = configuration.newExpressionParser("XP", false, 31);
            Saxon.Hej.expr.StaticContext parseContext = new Saxon.Hej.sxpath.IndependentContext(configuration);
            if ((globalFunctions != null)) {
                Saxon.Hej.pattern.NameTest argTest = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, exportURI, "arg", configuration.getNamePool());
                Saxon.Hej.style.StylesheetPackage sheet = new Saxon.Hej.style.StylesheetPackage(configuration);
                Saxon.Hej.tree.iter.AtomicIterator keys = globalFunctions.keys();
                Saxon.Hej.value.AtomicValue key;
                while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keys.next())) != null)) {
                    string nameArity = key.getStringValue();
                    Saxon.Hej.om.NodeInfo decl = (Saxon.Hej.om.NodeInfo)(globalFunctions.get(key));
                    decl = decl.getParent();
                    Saxon.Hej.value.SequenceType reqType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                    string rType = decl.getAttributeValue("", "as");
                    if ((rType != null)) {
                        reqType = parser.parseSequenceType(rType, parseContext);
                    }
                    string[] parts = System.Text.RegularExpressions.Regex.Split(nameArity, "#");
                    int arity;
                    try {
                        arity = Saxon.Impl.Helpers.Utils.ParseInt(parts[1]);
                    } catch (Saxon.Ejava.lang.NumberFormatException e) {
                        throw new Saxon.Hej.trans.XPathException("Function arity must be an integer");
                    }
                    Saxon.Hej.trans.SymbolicName.F functionName = new Saxon.Hej.trans.SymbolicName.F(Saxon.Hej.om.StructuredQName.fromClarkName(parts[0]), arity);
                    Saxon.Hej.expr.instruct.UserFunction fn = new Saxon.Hej.expr.instruct.UserFunction();
                    fn.setFunctionName(functionName.getComponentName());
                    Saxon.Hej.expr.Component component = Saxon.Hej.expr.Component.makeComponent(fn, Saxon.Hej.trans.Visibility.PUBLIC, Saxon.Hej.trans.VisibilityProvenance.DEFAULTED, sheet, sheet);
                    fn.setDeclaringComponent(component);
                    fn.setResultType(reqType);
                    fn.setBody(new Saxon.Hej.expr.StringLiteral((("BODY("+nameArity)+")")));
                    Saxon.Hej.expr.instruct.UserFunctionParameter[] @params = new Saxon.Hej.expr.instruct.UserFunctionParameter[arity];
                    int i = 0;
                    Saxon.Hej.om.NodeInfo arg;
                    Saxon.Hej.tree.iter.AxisIterator iter = decl.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, argTest);
                    while ((((arg = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null))) {
                        Saxon.Hej.expr.instruct.UserFunctionParameter p = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                        string name = arg.getAttributeValue("", "name");
                        reqType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                        rType = decl.getAttributeValue("", "as");
                        if ((rType != null)) {
                            reqType = parser.parseSequenceType(rType, parseContext);
                        }
                        p.setRequiredType(reqType);
                        p.setVariableQName(Saxon.Hej.om.StructuredQName.fromEQName(name));
                        @params[i++] = p;
                    }
                    fn.setParameterDefinitions(@params);
                    fn.setStackFrameMap(new Saxon.Hej.expr.instruct.SlotManager(i));
                    userFunctionLibrary.addFunction(fn);
                }
            }
            libraryList.addFunctionLibrary(userFunctionLibrary);
            env.setFunctionLibrary(libraryList);
            Saxon.Hej.expr.Expression expr;
            try {
                if (((type != null) && "pattern".Equals(type.getUnicodeStringValue()))) {
                    expr = Saxon.Hej.pattern.Pattern.make(exprText, env, null);
                } else {
                    expr = make(exprText, env, 0, Saxon.Hej.expr.parser.Token.EOF, null, inCatch);
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Static error in XPath expression supplied to saxon:compile-XPath: "+e.Message)+". Expression: {")+exprText)+"}"));
                err.setErrorCode("XTDE3160");
                throw err;
            }
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = new Saxon.Hej.expr.parser.ExpressionVisitor(configuration);
            visitor.setStaticContext(env);
            expr = expr.typeCheck(visitor, new Saxon.Hej.expr.parser.ContextItemStaticInfo(contextItemType, false));
            System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.flwor.LocalVariableBinding> internalSlotMap = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.flwor.LocalVariableBinding>();
            expr = setSlots(expr, knownGlobals, internalSlotMap, (maxSlot+1));
            Saxon.Hej.@event.Builder b = controller.makeBuilder();
            b.setTiming(false);
            Saxon.Hej.trace.ExpressionPresenter @out = new Saxon.Hej.trace.ExpressionPresenter(configuration, b);
            @out.setDefaultNamespace(exportURI);
            expr.export(@out);
            Saxon.Hej.om.NodeInfo node = ((/*1430*/Saxon.Hej.om.NodeInfo)b.getCurrentRoot().iterateAxis(Saxon.Hej.om.AxisInfo.CHILD).next());
            Saxon.Hej.value.StringValue st = new Saxon.Hej.value.StringValue(expr.getStaticType().toAlphaCode());
            Saxon.Impl.DataModel.ImmMap result = new Saxon.Impl.DataModel.ImmMap();
            result.initialPut(Saxon.Hej.value.StringValue.bmp("expr"), node);
            result.initialPut(Saxon.Hej.value.StringValue.bmp("type"), st);
            return result;
        }
        private class LocalFunctionLibrary : Saxon.Hej.functions.ExecutableFunctionLibrary {
            readonly System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName.F,Saxon.Hej.expr.instruct.UserFunction> lib = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName.F, Saxon.Hej.expr.instruct.UserFunction>();
            public LocalFunctionLibrary(Saxon.Hej.Configuration config) : base (config) {
            }
            public override bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
                return lib.ContainsKey(functionName);
            }
            public override Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
                Saxon.Hej.expr.instruct.UserFunction fn = Saxon.Impl.Helpers.MapUtils.Get(lib, functionName);
                Saxon.Hej.expr.instruct.UserFunctionParameter[] @params = new Saxon.Hej.expr.instruct.UserFunctionParameter[staticArgs.Length];
                int i = 0;
                foreach (Saxon.Hej.expr.Expression s in staticArgs) {
                    Saxon.Hej.expr.instruct.UserFunctionParameter p = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                    p.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
                    @params[i++] = p;
                }
                fn.setParameterDefinitions(@params);
                Saxon.Hej.expr.UserFunctionCall call = new Saxon.Hej.expr.UserFunctionCall();
                call.setArguments(staticArgs);
                call.setFunctionName(functionName.getComponentName());
                return call;
            }
            public override Saxon.Hej.functions.FunctionLibrary copy() {
                return null;
            }
            public override void addFunction(Saxon.Hej.expr.instruct.UserFunction fn) {
                Saxon.Impl.Helpers.MapUtils.Put(lib, new Saxon.Hej.trans.SymbolicName.F(fn.getFunctionName(), fn.getArity()), fn);
            }
            public override Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
                return Saxon.Impl.Helpers.MapUtils.Get(lib, functionName);
            }
        
        }
        private Saxon.Hej.value.SequenceType makeType(string type) {
            Saxon.Hej.value.SequenceType result = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            if (type.StartsWith("map(")) {
                result = new Saxon.Hej.value.SequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            }
            return result;
        }
        public static Saxon.Hej.expr.Expression make(string expression, Saxon.Hej.expr.StaticContext env, int start, int terminator, Saxon.Hej.expr.parser.CodeInjector codeInjector, bool inCatch) {
            int languageLevel = env.getXPathVersion();
            Saxon.Hej.expr.parser.XPathParser parser = env.getConfiguration().newExpressionParser("XP", false, languageLevel);
            if (inCatch) {
                parser.setCatchDepth(1);
            }
            if ((codeInjector != null)) {
                parser.setCodeInjector(codeInjector);
            }
            if ((terminator == -1)) {
                terminator = Saxon.Hej.expr.parser.Token.EOF;
            }
            Saxon.Hej.expr.Expression exp = parser.parse(expression, start, terminator, env);
            Saxon.Hej.expr.parser.ExpressionTool.setDeepRetainedStaticContext(exp, env.makeRetainedStaticContext());
            exp = exp.simplify();
            return exp;
        }
        private Saxon.Hej.expr.Expression setSlots(Saxon.Hej.expr.Expression expr, System.Collections.Generic.List<string> globals, System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.flwor.LocalVariableBinding> internalSlotMap, int nextSlot) {
            Saxon.Hej.expr.Expression possible = isGlobalReference(expr, globals);
            if ((possible != null)) {
                return possible;
            }
            if ((expr is Saxon.Hej.expr.Assignation)) {
                Saxon.Hej.expr.Assignation f = (Saxon.Hej.expr.Assignation)(expr);
                if ((f.getLocalSlotNumber() == -999)) {
                    Saxon.Hej.om.StructuredQName name = f.getVariableQName();
                    Saxon.Hej.expr.flwor.LocalVariableBinding b = new Saxon.Hej.expr.flwor.LocalVariableBinding(name, f.getStaticType());
                    b.setSlotNumber(nextSlot);
                    f.setSlotNumber(nextSlot);
                    Saxon.Impl.Helpers.MapUtils.Put(internalSlotMap, name, b);
                    nextSlot++;
                }
            }
            if ((expr is Saxon.Hej.expr.LocalVariableReference)) {
                Saxon.Hej.expr.LocalVariableReference lvr = (Saxon.Hej.expr.LocalVariableReference)(expr);
                if ((lvr.getBinding().getLocalSlotNumber() == -999)) {
                    Saxon.Hej.om.StructuredQName name = lvr.getVariableName();
                    lvr.fixup(Saxon.Impl.Helpers.MapUtils.Get(internalSlotMap, name));
                }
            }
            System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> ops = expr.operands();
            foreach (Saxon.Hej.expr.Operand o in ops) {
                Saxon.Hej.expr.Expression e = o.getChildExpression();
                possible = isGlobalReference(e, globals);
                if ((possible != null)) {
                    o.setChildExpression(possible);
                } else {
                    setSlots(e, globals, internalSlotMap, nextSlot);
                }
            }
            return expr;
        }
        private Saxon.Hej.expr.Expression isGlobalReference(Saxon.Hej.expr.Expression expr, System.Collections.Generic.List<string> globals) {
            if ((expr is Saxon.Hej.expr.VariableReference)) {
                string n = ((Saxon.Hej.expr.VariableReference)(expr)).getEQName();
                if (!(n.StartsWith("Q{"))) {
                    n = ("Q{}"+n);
                }
                if (globals.Contains(n)) {
                    Saxon.Hej.expr.GlobalVariableReference g = new Saxon.Hej.expr.GlobalVariableReference(((Saxon.Hej.expr.VariableReference)(expr)).getVariableName());
                    g.setRetainedStaticContext(expr.getRetainedStaticContext());
                    return g;
                }
            }
            return null;
        }
    
    }
}