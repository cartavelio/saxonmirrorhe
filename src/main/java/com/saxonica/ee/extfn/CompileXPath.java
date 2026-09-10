// Copyright (c) 2018-2020 Saxonica Limited

package com.saxonica.ee.extfn;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Builder;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.flwor.LocalVariableBinding;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.instruct.UserFunctionParameter;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.functions.ExecutableFunctionLibrary;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XPath31FunctionSet;
import net.sf.saxon.functions.registry.XSLT30FunctionSet;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NameTest;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.style.PublicStylesheetFunctionLibrary;
import net.sf.saxon.style.StylesheetFunctionLibrary;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.Visibility;
import net.sf.saxon.trans.VisibilityProvenance;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;


/**
 * This class implements the saxon:compile-XPath() extension function,
 * which is specially-recognized by the system because it needs access
 * to parts of the static context.
 * <p>The compile-XPath() extension function compiles an XPath expression to a SEF
 * representation. It was originally defined for Saxon-JS to allow free-standing compilation
 * and execution of dynamic XPath expressions in the browser; this is a back-port to
 * Saxon-Java so that the same code can be executed in either environment, which
 * facilitates testing.</p>
 */

public class CompileXPath extends SystemFunction {

    private final static String exportURI = "http://ns.saxonica.com/xslt/export";

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
     *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
     *                  If the argument is always a singleton, then the single item may be obtained by calling
     *                  <code>arguments[i].next()</code>. The implementation is not obliged to read all the items in each
     *                  <code>SequenceIterator</code> if they are not required to compute the result; but if any SequenceIterator is not read
     *                  to completion, it is good practice to call its close() method.
     * @return an iterator over the results of the function. If the result is a single item, it can be
     * returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}. If the result is an empty sequence,
     * the method should return <code>EmptyIterator.getInstance()</code>
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                        code will add information about the error location.
     */

    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        Controller controller = context.getController();
        Configuration configuration = controller.getConfiguration();
        AtomicValue content = (AtomicValue) arguments[0].head();
        MapItem options = (MapItem) arguments[1].head();
        MapItem locals = (MapItem) arguments[2].head();

        StringValue type = (StringValue) options.get(StringValue.bmp("type"));
        final String exprText = content.getStringValue();

        IndependentContext env = new IndependentContext(configuration);
        env.setWarningHandler((msg, locator) -> {
            String message = "In dynamic expression {" + exprText + "}: " + msg;
            context.getController().warning(message, null, null);
        });

        BooleanValue backwards = (BooleanValue) options.get(StringValue.bmp("backwardCompatibility"));
        env.setBackwardsCompatibilityMode(backwards != null && backwards.getBooleanValue());

        boolean inCatch;

        BooleanValue inCatchVal = (BooleanValue) options.get(StringValue.bmp("inCatch"));
        inCatch = inCatchVal != null && inCatchVal.getBooleanValue();

        StringValue contextType = (StringValue) options.get(StringValue.bmp("context-type"));
        ItemType contextItemType = AnyItemType.getInstance();

        MapItem namespaces = (MapItem) options.get(StringValue.bmp("namespaces"));
        if (namespaces != null) {
            AtomicIterator keys = namespaces.keys();
            AtomicValue key;
            while ((key = keys.next()) != null) {
                String prefix = key.getStringValue();
                String uri = namespaces.get(key).getUnicodeStringValue().toString();
                if (uri.equals("~")) {
                    uri = NamespaceConstant.getUriForConventionalPrefix(prefix);
                }
                if (uri == null) {
                    switch (prefix) {
                        case "map":
                            uri = NamespaceConstant.MAP_FUNCTIONS;
                            break;
                        case "array":
                            uri = NamespaceConstant.ARRAY_FUNCTIONS;
                            break;
                        default:
                            throw new XPathException("Unknown URI for defaulted prefix " + prefix);
                    }
                }
                env.declareNamespace(prefix, uri);
            }
        }
        MapItem globals = (MapItem) options.get(StringValue.bmp("globals"));
        ArrayList<String> knownGlobals = new ArrayList<>();
        if (globals != null) {
            AtomicIterator keys = globals.keys();
            AtomicValue key;
            while ((key = keys.next()) != null) {
                knownGlobals.add(key.getStringValue());
                NodeInfo n = (NodeInfo) globals.get(key);
                String t = n.getAttributeValue("", "sType");
                XPathVariable v = env.declareVariable(StructuredQName.fromClarkName(key.getStringValue()));
                v.setRequiredType(makeType(t));
            }
        }
        for (StructuredQName errorVariable : StandardNames.errorVariables) {
            env.declareVariable(errorVariable);
        }
        int maxSlot = 0;
        if (locals != null) {
            AtomicIterator keys = locals.keys();
            AtomicValue key;
            while ((key = keys.next()) != null) {
                SequenceExtent s = (SequenceExtent) locals.get(key);
                String staticType = s.itemAt(2).getStringValue();
                int slot = (int) ((Int64Value) s.itemAt(1)).longValue();
                if (slot > maxSlot) {
                    maxSlot = slot;
                }
                XPathVariable v = env.declareVariable(StructuredQName.fromClarkName(key.getStringValue()));
                v.setSlotNumber(slot);
                v.setRequiredType(makeType(staticType));
            }
        }

        FunctionLibraryList libraryList = new FunctionLibraryList();
        ExecutableFunctionLibrary userFunctionLibrary = new LocalFunctionLibrary(context.getConfiguration());


        FunctionLibraryList libraryList0 = ((StylesheetPackage) getRetainedStaticContext().getPackageData()).getFunctionLibrary();
        for (FunctionLibrary lib : libraryList0.getLibraryList()) {
            if (lib instanceof BuiltInFunctionSet && ((BuiltInFunctionSet) lib).getNamespace().equals(NamespaceConstant.FN)) {
                libraryList.addFunctionLibrary(XPath31FunctionSet.getInstance());
                libraryList.addFunctionLibrary(XSLT30FunctionSet.getInstance());
                libraryList.addFunctionLibrary(new VendorFunctionSetEE((EnterpriseConfiguration) configuration));
            } else if (lib instanceof StylesheetFunctionLibrary || lib instanceof ExecutableFunctionLibrary) {
                libraryList.addFunctionLibrary(new PublicStylesheetFunctionLibrary(lib));
            } else {
                libraryList.addFunctionLibrary(lib);
            }
        }

        MapItem globalFunctions = (MapItem) options.get(StringValue.bmp("globalFunctions"));

        XPathParser parser = configuration.newExpressionParser("XP", false, 31);// Used for parsing sequence type
        /* This is a hack at present - really we should use parts of the retained static context,
           specifically namespace bindings in case xs: has been re-prefixed */
        StaticContext parseContext = new IndependentContext(configuration);

        if (globalFunctions != null) {
            /* TODO - this is being set up for every call of saxon:compile-XPath() - should be 'static' */
            NameTest argTest = new NameTest(Type.ELEMENT, exportURI, "arg", configuration.getNamePool());
            StylesheetPackage sheet = new StylesheetPackage(configuration);
            AtomicIterator keys = globalFunctions.keys();
            AtomicValue key;
            while ((key = keys.next()) != null) {
                String nameArity = key.getStringValue();
                NodeInfo decl = (NodeInfo) globalFunctions.get(key);
                decl = decl.getParent(); // TODO - work out whether we need the body or can use the whole definition
                SequenceType reqType = SequenceType.ANY_SEQUENCE;
                String rType = decl.getAttributeValue("", "as");
                if (rType != null) {
                    reqType = parser.parseSequenceType(rType, parseContext);
                }

                String[] parts = nameArity.split("#");
                int arity;

                try {
                    arity = Integer.parseInt(parts[1]);
                } catch (NumberFormatException e) {
                    throw new XPathException("Function arity must be an integer");
                }
                SymbolicName.F functionName = new SymbolicName.F(StructuredQName.fromClarkName(parts[0]), arity);
                UserFunction fn = new UserFunction();
                fn.setFunctionName(functionName.getComponentName());

                Component component = Component.makeComponent(fn, Visibility.PUBLIC, VisibilityProvenance.DEFAULTED, sheet, sheet);
                fn.setDeclaringComponent(component);
                fn.setResultType(reqType);
                /* The function may need a body, which is assumed to have been compiled and is defined in XSLT.
                 * For the present we'll leave a string that identifies the body requirement.
                 * Note that this may fail when there is a type clash when the result is NOT a string */
                fn.setBody(new StringLiteral("BODY(" + nameArity + ")"));

                UserFunctionParameter[] params = new UserFunctionParameter[arity];
                int i = 0;
                NodeInfo arg;
                AxisIterator iter = decl.iterateAxis(AxisInfo.CHILD, argTest);
                while (((arg = iter.next()) != null)) {
                    UserFunctionParameter p = new UserFunctionParameter();
                    String name = arg.getAttributeValue("", "name");
                    reqType = SequenceType.ANY_SEQUENCE;
                    rType = decl.getAttributeValue("", "as");
                    if (rType != null) {
                        reqType = parser.parseSequenceType(rType, parseContext);
                    }
                    p.setRequiredType(reqType);
                    p.setVariableQName(StructuredQName.fromEQName(name));
                    params[i++] = p;
                }
                fn.setParameterDefinitions(params);
                fn.setStackFrameMap(new SlotManager(i));
                userFunctionLibrary.addFunction(fn);
            }
        }

        libraryList.addFunctionLibrary(userFunctionLibrary);
        env.setFunctionLibrary(libraryList);

        // TODO - make sure function not found is detected.
        Expression expr;
        try {
            if (type != null && "pattern".equals(type.getUnicodeStringValue())) {
                expr = Pattern.make(exprText, env, null);
            } else {
                expr = make(exprText, env, 0, Token.EOF, null, inCatch);
            }
        } catch (XPathException e) {
            XPathException err = new XPathException("Static error in XPath expression supplied to saxon:compile-XPath: " +
                                                            e.getMessage() + ". Expression: {" + exprText + "}");
            err.setErrorCode("XTDE3160");
            throw err;
        }

        ExpressionVisitor visitor = new ExpressionVisitor(configuration);
        visitor.setStaticContext(env);
        expr = expr.typeCheck(visitor, new ContextItemStaticInfo(contextItemType, false));
        HashMap<StructuredQName, LocalVariableBinding> internalSlotMap = new HashMap<>();
        expr = setSlots(expr, knownGlobals, internalSlotMap, maxSlot + 1);
        Builder b = controller.makeBuilder();
        b.setTiming(false);
        ExpressionPresenter out = new ExpressionPresenter(configuration, b);
        out.setDefaultNamespace(exportURI);
        expr.export(out);
        NodeInfo node = b.getCurrentRoot().iterateAxis(AxisInfo.CHILD).next();
        StringValue st = new StringValue(expr.getStaticType().toAlphaCode());
        HashTrieMap result = new HashTrieMap();
        result.initialPut(StringValue.bmp("expr"), node);
        result.initialPut(StringValue.bmp("type"), st);

        return result;
    }

    private static class LocalFunctionLibrary extends ExecutableFunctionLibrary {

        final HashMap<SymbolicName.F, UserFunction> lib = new HashMap<>();

        public LocalFunctionLibrary(Configuration config) {
            super(config);
        }

        @Override
        public boolean isAvailable(SymbolicName.F functionName) {
            return lib.containsKey(functionName);
        }

        @Override
        public Expression bind(SymbolicName.F functionName, Expression[] staticArgs, StaticContext
                env, List<String> reasons) {
            UserFunction fn = lib.get(functionName);
            UserFunctionParameter[] params = new UserFunctionParameter[staticArgs.length];
            int i = 0;
            for (Expression s : staticArgs) {
                UserFunctionParameter p = new UserFunctionParameter();
                p.setRequiredType(SequenceType.ANY_SEQUENCE);
                params[i++] = p;
            }
            fn.setParameterDefinitions(params);
            UserFunctionCall call = new UserFunctionCall();
            call.setArguments(staticArgs);
            call.setFunctionName(functionName.getComponentName());
            return call;
        }

        @Override
        public FunctionLibrary copy() {
            return null;
        }

        @Override
        public void addFunction(UserFunction fn) {
            lib.put(new SymbolicName.F(fn.getFunctionName(), fn.getArity()), fn);
        }

        @Override
        public Function getFunctionItem(SymbolicName.F functionName, StaticContext staticContext) {
            return lib.get(functionName);
        }
    }




    private SequenceType makeType(String type) {
        SequenceType result = SequenceType.ANY_SEQUENCE;
        if (type.startsWith("map(")) {
            result = new SequenceType(MapType.ANY_MAP_TYPE, StaticProperty.EXACTLY_ONE);
        }
        return result;
    }

    /**
     * Upgraded from ExpressionTool.make() to permit 'in-catch' conversion of error variables
     * to calls on saxon:dynamic-error-info()
     * <p>
     * Parse an XPath expression. This performs the basic analysis of the expression against the
     * grammar, it binds variable references and function calls to variable definitions and
     * function definitions, and it performs context-independent expression rewriting for
     * optimization purposes.
     *
     * @param expression   The expression (as a character string)
     * @param env          An object giving information about the compile-time
     *                     context of the expression
     * @param start        position of the first significant character in the expression
     * @param terminator   The token that marks the end of this expression; typically
     *                     Token.EOF, but may for example be a right curly brace
     * @param codeInjector true  allows injection of tracing, debugging, or performance monitoring code; null if
     *                     not required
     * @param inCatch      The Xpath is considered to be in the context of an xsl:catch
     * @return an object of type Expression
     * @throws XPathException if the expression contains a static error
     */

    /*@NotNull*/
    public static Expression make(String expression, StaticContext env,
                                  int start, int terminator,
                                  CodeInjector codeInjector, boolean inCatch) throws XPathException {
        int languageLevel = env.getXPathVersion();
        XPathParser parser = env.getConfiguration().newExpressionParser("XP", false, languageLevel);
        if (inCatch) {
            parser.setCatchDepth(1);
        }
        if (codeInjector != null) {
            parser.setCodeInjector(codeInjector);
        }
        if (terminator == -1) {
            terminator = Token.EOF;
        }
        Expression exp = parser.parse(expression, start, terminator, env);
        ExpressionTool.setDeepRetainedStaticContext(exp, env.makeRetainedStaticContext());
        exp = exp.simplify();
        return exp;
    }

    /* TODO - handle local variables that override global references (e.g. $options in the compiler) */
    private Expression setSlots(Expression expr, ArrayList<String> globals, HashMap<StructuredQName, LocalVariableBinding> internalSlotMap, int nextSlot) {
        Expression possible = isGlobalReference(expr, globals);
        if (possible != null) {
            return possible;
        }
        /* TODO - get sequence/instance type for LHS of let and for */
        if (expr instanceof Assignation) {
            Assignation f = (Assignation) expr;
            if (f.getLocalSlotNumber() == -999) {
                StructuredQName name = f.getVariableQName();
                LocalVariableBinding b = new LocalVariableBinding(name, f.getStaticType());
                b.setSlotNumber(nextSlot);
                f.setSlotNumber(nextSlot);
                internalSlotMap.put(name, b);
                nextSlot++;
            }
        }

        if (expr instanceof LocalVariableReference) {
            LocalVariableReference lvr = (LocalVariableReference) expr;
            if (lvr.getBinding().getLocalSlotNumber() == -999) {
                StructuredQName name = lvr.getVariableName();
                /*LocalVariableBinding b = new LocalVariableBinding(name, lvr.getStaticType());
                b.setSlotNumber(777);*/
                lvr.fixup(internalSlotMap.get(name));
            }
        }
        Iterable<Operand> ops = expr.operands();
        for (Operand o : ops) {
            Expression e = o.getChildExpression();
            possible = isGlobalReference(e, globals);
            if (possible != null) {
                o.setChildExpression(possible);
            } else {
                setSlots(e, globals, internalSlotMap, nextSlot);
            }
        }
        return expr;
    }

    private Expression isGlobalReference(Expression expr, ArrayList<String> globals) {
        if (expr instanceof VariableReference) {
            String n = ((VariableReference) expr).getEQName();
            if (!n.startsWith("Q{")) {
                n = "Q{}" + n;
            }
            if (globals.contains(n)) {
                GlobalVariableReference g = new GlobalVariableReference(((VariableReference) expr).getVariableName());
                g.setRetainedStaticContext(expr.getRetainedStaticContext());
                return g;
            }
        }
        return null;
    }
}

