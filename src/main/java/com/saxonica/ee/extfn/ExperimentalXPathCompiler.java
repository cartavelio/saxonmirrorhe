package com.saxonica.ee.extfn;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.JavaExtensionLibrary;
import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.ee.schema.UserAtomicType;
import com.saxonica.functions.extfn.EXPathArchive.Archive;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Builder;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.flwor.Clause;
import net.sf.saxon.expr.flwor.FLWORExpression;
import net.sf.saxon.expr.flwor.LocalVariableBinding;
import net.sf.saxon.expr.instruct.LocalParam;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.functions.registry.ConstructorFunctionLibrary;
import net.sf.saxon.functions.registry.XSLT30FunctionSet;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StandardDiagnostics;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.*;
import net.sf.saxon.om.*;
import net.sf.saxon.option.exslt.Random;
import net.sf.saxon.option.exslt.Sets;
import net.sf.saxon.pattern.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.DecimalFormatManager;
import net.sf.saxon.trans.KeyManager;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.*;

/**
 * This class provides support to the xslt-in-xslt compiler. The XSLT compiler obtains a single
 * instance of the ExperimentalXPathCompiler for a compilation episode, initializes it with global
 * context information that applies to the package-being-compiled as a whole, and then invokes its
 * compileXPath() method to compile each XPath expression encountered.
 */

public class ExperimentalXPathCompiler {

    private Configuration config;
    private FunctionLibrary globalFunctionLibrary;
    private Map<StructuredQName, GroundedValue> globalVariables = new HashMap<>();
    private SEFStaticContext uniformStaticContext;
    private final Map<String, Map<String, String>> namespaceBindingsCache = new HashMap<>();
    private NodeInfo mostRecentNsAttribute = null;
    private Map<String, String> mostRecentNsBindings = null;

    private ExperimentalXPathCompiler() {

    }

    private ExperimentalXPathCompiler(ExperimentalXPathCompiler original) {
        config = original.config;
        globalFunctionLibrary = original.globalFunctionLibrary;
        globalVariables = original.globalVariables;
    }

    public static ExperimentalXPathCompiler make(
            XPathContext context,
            MapItem globalFunctions,
            MapItem globalVariables) {

        ExperimentalXPathCompiler compiler = new ExperimentalXPathCompiler();
        compiler.config = context.getConfiguration();
        for (KeyValuePair entry : globalVariables.keyValuePairs()) {
            String name = entry.key.getStringValue();
            GroundedValue value = entry.value;
            compiler.globalVariables.put(StructuredQName.fromEQName((name)), value);
        }

        JavaExtensionLibrary javaLib = new JavaExtensionLibrary((ProfessionalConfiguration) context.getConfiguration());
        //javaLib.declareJavaClass(NamespaceConstant.SAXON, Extensions.class);
        javaLib.declareJavaClass(NamespaceConstant.EXSLT_SETS, Sets.class);
        javaLib.declareJavaClass(NamespaceConstant.EXSLT_MATH, net.sf.saxon.option.exslt.Math.class);
        javaLib.declareJavaClass(NamespaceConstant.EXSLT_DATES_AND_TIMES, net.sf.saxon.option.exslt.Date.class);
        javaLib.declareJavaClass(NamespaceConstant.EXSLT_RANDOM, Random.class);
        //javaLib.declareJavaClass(EXPathFile.NAMESPACE, EXPathFile.class);
        //javaLib.declareJavaClass(EXPathBinary.NAMESPACE, EXPathBinary.class);
        javaLib.declareJavaClass(Archive.NAMESPACE, Archive.class);

        FunctionLibraryList libraryList = new FunctionLibraryList();
        libraryList.addFunctionLibrary(XSLT30FunctionSet.getInstance());
        libraryList.addFunctionLibrary(MapFunctionSet.getInstance());
        libraryList.addFunctionLibrary(ArrayFunctionSet.getInstance());
        libraryList.addFunctionLibrary(new ConstructorFunctionLibrary(context.getConfiguration()));
        libraryList.addFunctionLibrary(new VendorFunctionSetEE((EnterpriseConfiguration) context.getConfiguration()));
        libraryList.addFunctionLibrary(javaLib);
        libraryList.addFunctionLibrary(compiler.setupUserFunctionLibrary(globalFunctions));
        compiler.globalFunctionLibrary = libraryList;
        return compiler;

    }

    public ExperimentalXPathCompiler withUniformStaticContext(XPathContext context, NodeInfo sefElement, NodeInfo nsAttribute) {
        Objects.requireNonNull(sefElement);
        Objects.requireNonNull(nsAttribute);
        ExperimentalXPathCompiler newComp = new ExperimentalXPathCompiler(this);
        newComp.uniformStaticContext = new SEFStaticContext(sefElement, null, nsAttribute);
        return newComp;
    }

    private static Optional<Map<String, String>> makeNamespaceMap(MapItem namespaceContext) {
        Map<String, String> bindings = new HashMap<>();
        for (KeyValuePair kvp : namespaceContext.keyValuePairs()) {
            bindings.put(kvp.key.getStringValue(), ((StringValue) kvp.value).getStringValue());
        }
        bindings.put("xml", NamespaceConstant.XML);
        return Optional.of(bindings);
    }

    private FunctionLibrary setupUserFunctionLibrary(MapItem globalFunctions) {

        Map<SymbolicName.F, UserFunction> library = new HashMap<>();
        if (globalFunctions != null) {
            for (KeyValuePair kvp : globalFunctions.keyValuePairs()) {
                AtomicValue key = kvp.key;
                String nameArity = key.getStringValue();
                String[] parts = nameArity.split("#");
                int arity = Integer.parseInt(parts[1]);
                StructuredQName name = StructuredQName.fromEQName((parts[0]));
                MapItem typeAsMap = (MapItem) kvp.value;
                SpecificFunctionType ft = (SpecificFunctionType) fromMap(typeAsMap).getPrimaryType();
                library.put(new SymbolicName.F(name, arity),
                            new ProvisionalSEFFunction(name, arity, ft));
            }
        }


        return new FunctionLibrary() {

            @Override
            public boolean isAvailable(SymbolicName.F functionName, int languageLevel) {
                return library.containsKey(functionName);
            }


            @Override
            public Expression bind(SymbolicName.F functionName, Expression[] staticArgs, StaticContext env, List<String> reasons) {
                if (isAvailable(functionName, env.getXPathVersion())) {
                    UserFunction fn = library.get(functionName);
                    fn.setFunctionName(functionName.getComponentName());
                    //lib.put(functionName, fn);
                    UserFunctionCall call = new UserFunctionCall();
                    call.setArguments(staticArgs);
                    call.setFunctionName(functionName.getComponentName());
                    call.setFunction(fn);
                    return call;
                } else {
                    return null;
                }
            }

            @Override
            public FunctionLibrary copy() {
                return null;
            }

            @Override
            public Function getFunctionItem(SymbolicName.F functionName, StaticContext staticContext) {
                return library.get(functionName);
            }
        };


    }

    /**
     * Compile an XPath expression on behalf of the XX XSLT compiler
     *
     * @param context          the evaluation context
     * @param exprText         the text of the XPath expression
     * @param sefElement       the element node in the SEF tree containing the XPath expression
     * @param localVariableMap map containing information about local variables in the static context.
     *                         The key of the map is the name of the variable as a QName. The associated
     *                         value is a sequence of three items: the declaring element in the SEF
     *                         tree, the slot number, and the declared type of the variable.
     * @param nsAttribute      the nearest in-scope ns attribute, containing a whitespace-separated
     *                         list of prefix=uri namespace bindings
     * @return a map representing the compiled expression, with two entries: "expr" for the SEF
     * tree of the expression, and "type" for the static type of the expression.
     * @throws XPathException if XPath parsing fails with a static error
     */

    public MapItem compileXPath(XPathContext context, String exprText, NodeInfo sefElement, MapItem localVariableMap, NodeInfo nsAttribute) throws XPathException {
        try {
            //System.err.println("Compile " + sefElement.getDisplayName() + ": " + exprText);
            Controller controller = context.getController();
            SEFStaticContext env;
            if (uniformStaticContext != null) {
                env = uniformStaticContext;
                env.setLocalVariableMap(localVariableMap);
            } else {
                env = new SEFStaticContext(sefElement, localVariableMap, nsAttribute);
            }
            //System.err.println("Compiling: " + exprText + " with ns=\"" + nsAttribute.getStringValue() + "\" declared on " + nsAttribute.getParent().getDisplayName());
            Expression expr = null;
            try {
                if (sefElement.getLocalPart().equals("pattern")) {
                    expr = Pattern.make(exprText, env, null);
                } else {
                    boolean inCatch = sefElement.iterateAxis(
                            AxisInfo.ANCESTOR,
                            new LocalNameTest(config.getNamePool(), Type.ELEMENT, "catch")).next() != null;
                    boolean allowEmpty = "true".equals(sefElement.getAttributeValue("", "allowEmpty"));
                    expr = make(exprText, env, inCatch, allowEmpty);
                }
            } catch (XPathException e) {
                XPathException err = new XPathException("Error in XPath expression {" + exprText + "}: " + e.getMessage());
                err.setErrorCode("XTDE3160");
                throw err;
            }

            ExpressionVisitor visitor = new ExpressionVisitor(config);
            visitor.setStaticContext(env);
            ItemType contextItemType = AnyItemType.getInstance(); // TODO: is that the best we can do?
            expr = expr.typeCheck(visitor, new ContextItemStaticInfo(contextItemType, false));
            int used = localVariableMap.size();
            int allocated = allocateSlots(expr, used, config.makeSlotManager());
            //HashMap<StructuredQName, LocalVariableBinding> internalSlotMap = new HashMap<>();
            //expr = setSlots(expr, knownGlobals, internalSlotMap, slotMap.size()+1);

            // Create a SEF tree representation of the expression
            Builder b = controller.makeBuilder();
            b.setTiming(false);
            ExpressionPresenter out = new ExpressionPresenter(config, b);
            out.setDefaultNamespace(NamespaceConstant.SAXON_XSLT_EXPORT);
            ExpressionPresenter.ExportOptions options = new ExpressionPresenter.ExportOptions();
            options.suppressStaticContext = true;
            options.target = "JS";
            options.targetVersion = 2;
            options.addStaticType = true;
            out.setOptions(options);
            expr.export(out);
            b.close();

            // Return a tuple containing the SEF tree and the inferred static type
            NodeInfo node = b.getCurrentRoot().iterateAxis(AxisInfo.CHILD).next();
            StringValue st = ExperimentalSequenceTypeMarshaller.unmarshal(makeMap(expr.getStaticType()));
            DictionaryMap result = new DictionaryMap();
            result.initialPut("expr", node);
            result.initialPut("type", st);

            return result;
        } catch (Throwable e) {
            throw e;
        }
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
     * @param expression The expression (as a character string)
     * @param env        An object giving information about the compile-time
     *                   context of the expression
     * @param inCatch    The Xpath is considered to be in the context of an xsl:catch
     * @param allowEmpty
     * @return an object of type Expression
     * @throws XPathException if the expression contains a static error
     */

    /*@NotNull*/
    private static Expression make(String expression, StaticContext env,
                                   boolean inCatch, boolean allowEmpty) throws XPathException {
        try {
            int languageLevel = env.getXPathVersion();
            XPathParser parser = env.getConfiguration().newExpressionParser("XP", false, languageLevel);
            if (inCatch) {
                parser.setCatchDepth(1);
            }
            parser.setAllowAbsentExpression(allowEmpty);
            //System.err.println("EXPR: " + expression);
            Expression exp = parser.parse(expression, 0, Token.EOF, env);
            ExpressionTool.setDeepRetainedStaticContext(exp, env.makeRetainedStaticContext());
            exp = exp.simplify();
            return exp;
        } catch (Throwable e) {
            e.printStackTrace();
            throw e;
        }
    }


    private class SEFStaticContext implements StaticContext, NamespaceResolver {

        private final NodeInfo sefElement;
        private RetainedStaticContext retainedStaticContext;
        private Map<String, String> staticNamespaceBindings;
        private final PackageData packageData;
        private MapItem localVariableMap;
        private final NodeInfo nsAttribute;
        private DecimalFormatManager decimalFormatManager;

        public SEFStaticContext(NodeInfo sefElement, MapItem localVariableMap, NodeInfo nsAttribute) {
            this.sefElement = sefElement;
            this.localVariableMap = localVariableMap;
            this.nsAttribute = nsAttribute;
            PackageData pd = new PackageData(config);
            pd.setHostLanguage(HostLanguage.XPATH, getXPathVersion());
            pd.setSchemaAware(false);
            packageData = pd;
        }

        @Override
        public Configuration getConfiguration() {
            return sefElement.getConfiguration();
        }

        @Override
        public PackageData getPackageData() {
            return packageData;
        }

        @Override
        public XPathContext makeEarlyEvaluationContext() {
            return new EarlyEvaluationContext(getConfiguration());
        }

        @Override
        public RetainedStaticContext makeRetainedStaticContext() {
            if (retainedStaticContext == null) {
                retainedStaticContext = new RetainedStaticContext(this);
            }
            return retainedStaticContext;
        }

        @Override
        public Location getContainingLocation() {
            return sefElement;
        }

        @Override
        public void issueWarning(String message, Location locator) {
            System.err.println("WARNING: " + message);
        }

        @Override
        public String getSystemId() {
            return sefElement.getSystemId();
        }

        @Override
        public String getStaticBaseURI() {
            // We're calling this while creating the RetainedStaticContext. Ideally we would do it lazily
            return Navigator.getInheritedAttributeValue(sefElement, "", "baseUri");
        }

        public void setLocalVariableMap(MapItem variableMap) {
            this.localVariableMap = variableMap;
        }

        @Override
        public Expression bindVariable(StructuredQName qName) throws XPathException {
            String eqName = qName.getEQName();
            StringValue eqNameVal = new StringValue(eqName);
            GroundedValue localVariableDetails = localVariableMap.get(eqNameVal);
            if (localVariableDetails != null) {
                NumericValue slotVal = (NumericValue) localVariableDetails.itemAt(1);
                SEFLocalVariableReference var = new SEFLocalVariableReference(qName);
                var.setSlotNumber((int) slotVal.longValue());
                StringValue type = (StringValue) localVariableDetails.itemAt(2);
                var.setDeclaredType(type.getStringValue());
                NodeInfo decl = (NodeInfo) localVariableDetails.itemAt(0);
                if (decl.getLocalPart().equals("accRule") &&
                        decl.getURI().equals(NamespaceConstant.SAXON_XSLT_EXPORT) &&
                        eqName.equals("Q{}value")) {
                    var.setSuppliedParameter(true);
                }
                return var;
            }
            if (globalVariables.containsKey(qName)) {
                return new GlobalVariableReference(qName);
            }
            throw new XPathException("Variable " + qName.getDisplayName() + " has not been declared");
        }

        @Override
        public FunctionLibrary getFunctionLibrary() {
            return globalFunctionLibrary;
        }

        @Override
        public String getDefaultCollationName() {
            String att = sefElement.getAttributeValue("", "default-collation");
            if (att == null) {
                return NamespaceConstant.CODEPOINT_COLLATION_URI;
            } else {
                return att; // TODO: tokenize on whitespace
            }
        }

        @Override
        public String getDefaultElementNamespace() {
            String att = sefElement.getAttributeValue("", "xpath-default-namespace");
            if (att == null) {
                return NamespaceConstant.NULL;
            } else {
                return att;
            }
        }

        @Override
        public String getDefaultFunctionNamespace() {
            return NamespaceConstant.FN;
        }

        @Override
        public boolean isInBackwardsCompatibleMode() {
            return sefElement.getAttributeValue("", "BC") != null;
        }

        @Override
        public boolean isImportedSchema(String namespace) {
            return false;
        }

        @Override
        public Set<String> getImportedSchemaNamespaces() {
            return Collections.emptySet();
        }

        @Override
        public NamespaceResolver getNamespaceResolver() {
            return this;
        }

        private void makeNamespaceBindings() {
            if (staticNamespaceBindings == null) {
                assert nsAttribute != null;
                if (mostRecentNsAttribute != null && nsAttribute.isSameNodeInfo(mostRecentNsAttribute)) {
                    staticNamespaceBindings = mostRecentNsBindings;
                } else {
                    mostRecentNsAttribute = nsAttribute;
                    String ns = nsAttribute.getStringValue();
                    Map<String, String> cachedValue = namespaceBindingsCache.get(ns);
                    if (cachedValue != null) {
                        staticNamespaceBindings = mostRecentNsBindings = cachedValue;
                    } else {
                        staticNamespaceBindings = mostRecentNsBindings = new HashMap<>();
                        String[] bindings = ns.split(" ");
                        for (String binding : bindings) {
                            int eq = binding.indexOf("=");
                            assert eq >= 0;
                            String prefix = binding.substring(0, eq);
                            String uri = binding.substring(eq + 1);
                            if (uri.equals("~")) {
                                uri = NamespaceConstant.getUriForConventionalPrefix(prefix);
                                assert uri != null;
                            }
                            staticNamespaceBindings.put(prefix, uri);
                        }
                        namespaceBindingsCache.put(ns, staticNamespaceBindings);
                    }
                }
            }
        }

        @Override
        public String getURIForPrefix(String prefix, boolean useDefault) {
            makeNamespaceBindings();
            return staticNamespaceBindings.get(prefix);
        }

        @Override
        public Iterator<String> iteratePrefixes() {
            makeNamespaceBindings();
            return staticNamespaceBindings.keySet().iterator();
        }


        @Override
        public ItemType getRequiredContextItemType() {
            return AnyItemType.getInstance();
        }

        @Override
        public DecimalFormatManager getDecimalFormatManager() {
            if (decimalFormatManager == null) {
                decimalFormatManager = new DecimalFormatManager(HostLanguage.XSLT, 30);
            }
            return decimalFormatManager;
        }

        @Override
        public int getXPathVersion() {
            return 31;
        }

        @Override
        public KeyManager getKeyManager() {
            return null;
        }

        @Override
        public ItemType resolveTypeAlias(StructuredQName typeName) {
            return null;
        }
    }

    private static class ProvisionalSEFFunction extends UserFunction {

        StructuredQName name;
        int arity;
        SpecificFunctionType functionType;

        public ProvisionalSEFFunction(StructuredQName name, int arity, SpecificFunctionType functionType) {
            this.name = name;
            this.arity = arity;
            this.functionType = functionType;
        }

        @Override
        public FunctionItemType getFunctionItemType() {
            return functionType;
        }

        @Override
        public StructuredQName getFunctionName() {
            return name;
        }

        @Override
        public int getArity() {
            return arity;
        }

        @Override
        public Sequence call(XPathContext context, Sequence[] args) throws XPathException {
            return null;
        }

        @Override
        public String getDescription() {
            return name.getDisplayName() + "#" + arity;
        }

        @Override
        public void export(ExpressionPresenter out) throws XPathException {
            //no action
        }
    }

    /**
     * Allocate slot numbers to range variables. This is a variant of {@link ExpressionTool#allocateSlots(Expression, int, SlotManager)},
     * which takes into account that variable references may have no target binding, and that some
     * variables will already have slot numbers.
     *
     * <p>This method is only concerned with allocating slot numbers to variables that are declared
     * within the expression itself.</p>
     *
     * @param exp      the expression whose range variables need to have slot numbers assigned
     * @param nextFree the next slot number that is available for allocation
     * @param frame    a SlotManager object that is used to track the mapping of slot numbers
     *                 to variable names for debugging purposes. May be null.
     * @return the next unallocated slot number.
     */

    public static int allocateSlots(Expression exp, int nextFree, SlotManager frame) {

        if (exp instanceof Assignation) {
            ((Assignation) exp).setSlotNumber(nextFree);
            int count = ((Assignation) exp).getRequiredSlots();
            nextFree += count;
            if (frame != null) {
                frame.allocateSlotNumber(((Assignation) exp).getVariableQName());
            }
        }
        if (exp instanceof LocalParam && ((LocalParam) exp).getSlotNumber() < 0) {
            ((LocalParam) exp).setSlotNumber(nextFree++);
        }

        if (exp instanceof FLWORExpression) {
            for (Clause c : ((FLWORExpression) exp).getClauseList()) {
                for (LocalVariableBinding b : c.getRangeVariables()) {
                    b.setSlotNumber(nextFree++);
                    frame.allocateSlotNumber(b.getVariableQName());
                }
            }
        }
        if (exp instanceof LocalVariableReference) {
            LocalVariableReference var = (LocalVariableReference) exp;
            LocalBinding binding = var.getBinding();
            if (binding != null) {
                var.setSlotNumber(binding.getLocalSlotNumber());
            }
        }

        for (Operand o : exp.operands()) {
            nextFree = allocateSlots(o.getChildExpression(), nextFree, frame);
        }

        return nextFree;

        // Note, we allocate a distinct slot to each range variable, even if the
        // scopes don't overlap. This isn't strictly necessary, but might help
        // debugging.
    }

    private static class SEFLocalVariableReference extends LocalVariableReference {

        private String declaredType;
        private boolean isSuppliedParameter;

        public SEFLocalVariableReference(StructuredQName name) {
            super(name);
        }

        public void setDeclaredType(String type) {
            this.declaredType = type;
        }

        public void setSuppliedParameter(boolean isSupplied) {
            isSuppliedParameter = isSupplied;
        }

        @Override
        public void export(ExpressionPresenter destination) throws XPathException {
            if (isSuppliedParameter) {
                destination.startElement("supplied");
            } else {
                destination.startElement("varRef", this);
                destination.emitAttribute("name", getVariableName());
            }
            destination.emitAttribute("slot", "" + getSlotNumber());
            if (declaredType != null) {
                destination.emitAttribute("type", declaredType);
            }
            destination.endElement();
        }

    }

    /**
     * Method that can be called from the XSLT code to trigger a debugger breakpoint
     *
     * @param s an identifying string
     */
    public static void breakpoint(String s) {
        System.err.println(s);
    }

    /**
     * Method to output a stack trace
     */

    public static void stackTrace(XPathContext context) {
        new StandardDiagnostics().logStackTrace(context, context.getConfiguration().getLogger(), 2);
    }

    /**
     * Parse a SequenceType
     */

    public static MapItem parseSequenceType(
            XPathContext context, String input, NodeInfo nsContext, String xpathDefaultNamespace)
            throws XPathException {
        try {
            IndependentContext env = new IndependentContext(context.getConfiguration());
            env.setNamespaceResolver(new NamespaceResolver() {
                final String defaultNS = xpathDefaultNamespace == null ? "" : xpathDefaultNamespace;
                final NamespaceResolver supplied = nsContext.getAllNamespaces();

                @Override
                public String getURIForPrefix(String prefix, boolean useDefault) {
                    if (prefix.isEmpty() && useDefault) {
                        return defaultNS;
                    } else {
                        return supplied.getURIForPrefix(prefix, useDefault);
                    }
                }

                @Override
                public Iterator<String> iteratePrefixes() {
                    return supplied.iteratePrefixes();
                }
            });
            XPathParser parser = new XPathParser();
            parser.setLanguage(XPathParser.ParsedLanguage.XPATH, 31);
            SequenceType st = parser.parseSequenceType(input, env);
            return makeMap(st);

        } catch (XPathException err) {
            throw err;
        }
    }

    public static MapItem makeMap(SequenceType st) {
        ItemType primary = st.getPrimaryType();

        DictionaryMap result = new DictionaryMap();
        //result.initialPut("t", new StringValue(st.toExportString()));

        String occurrenceIndicator = Cardinality.getOccurrenceIndicator(st.getCardinality());
        if (!occurrenceIndicator.isEmpty()) {
            result.initialPut("o", new StringValue(occurrenceIndicator));
        }

        if (st == SequenceType.EMPTY_SEQUENCE) {
            return result;
        }

        String alphaCode = primary.getBasicAlphaCode();
        result.initialPut("p", new StringValue(alphaCode));

        if (primary instanceof UserAtomicType) {
            result.initialPut("t", new StringValue(((UserAtomicType) primary).getEQName()));
        } else if (primary instanceof UnionType) {
            try {
                List<GroundedValue> memberMaps = new ArrayList<>();
                for (PlainType pt : ((UnionType) primary).getPlainMemberTypes()) {
                    memberMaps.add(makeMap(SequenceType.makeSequenceType(pt, StaticProperty.EXACTLY_ONE)));
                }
                result.initialPut("u", new SimpleArrayItem(memberMaps));
            } catch (MissingComponentException e) {
                // no action
            }
        } else if (primary instanceof NameTest) {
            String name = ((NameTest) primary).getMatchingNodeName().getEQName();
            result.initialPut("n", new StringValue(name));
        } else if (primary instanceof LocalNameTest) {
            result.initialPut("n", StringValue.bmp("*:" + ((LocalNameTest) primary).getLocalName()));
        } else if (primary instanceof NamespaceTest) {
            result.initialPut("n", StringValue.bmp("Q{" + ((NamespaceTest) primary).getNamespaceURI() + "}*"));
        } else if (primary instanceof CombinedNodeTest) {
            ((CombinedNodeTest) primary).addTypeDetails(result);
        } else if (primary instanceof ContentTypeTest) {
            String annotation = ((ContentTypeTest)primary).getContentType().getEQName();
            result.initialPut("c", new StringValue(annotation));
        } else if (primary instanceof DocumentNodeTest) {
            ItemType content = ((DocumentNodeTest) primary).getElementTest();
            MapItem contentMap = makeMap(SequenceType.makeSequenceType(content, StaticProperty.EXACTLY_ONE));
            result.initialPut("e", contentMap);
        } else if (primary instanceof FunctionItemType) {
            if (primary instanceof ArrayItemType) {
                SequenceType memberType = ((ArrayItemType) primary).getMemberType();
                if (memberType != SequenceType.ANY_SEQUENCE) {
                    MapItem contentMap = makeMap(memberType);
                    result.initialPut("v", contentMap);
                }
            } else if (primary instanceof MapType) {
                AtomicType keyType = ((MapType) primary).getKeyType();
                if (keyType != BuiltInAtomicType.ANY_ATOMIC) {
                    MapItem keyMap = makeMap(new SequenceType(keyType, StaticProperty.EXACTLY_ONE));
                    result.initialPut("k", keyMap);
                }
                SequenceType valueType = ((MapType) primary).getValueType();
                if (valueType != SequenceType.ANY_SEQUENCE) {
                    MapItem contentMap = makeMap(valueType);
                    result.initialPut("v", contentMap);
                }
            } else {
                SequenceType valueType = ((FunctionItemType) primary).getResultType();
                if (valueType != SequenceType.ANY_SEQUENCE) {
                    MapItem contentMap = makeMap(valueType);
                    result.initialPut("r", contentMap);
                }
                SequenceType[] argTypes = ((FunctionItemType) primary).getArgumentTypes();
                if (argTypes != null) {
                    List<GroundedValue> argMaps = new ArrayList<>();
                    for (SequenceType at : argTypes) {
                        argMaps.add(makeMap(at));
                    }
                    ArrayItem arrayItem = new SimpleArrayItem(argMaps);
                    result.initialPut("a", arrayItem);
                }
            }
        }

        return result;
    }

    public static SequenceType fromMap(MapItem typeAsMap) {
        // TODO: this is incomplete, for example it doesn't handle union types, array member types, etc
        String occ = ((StringValue) typeAsMap.get(StringValue.bmp("o"))).getStringValue();
        int cardinality = Cardinality.fromOccurrenceIndicator(occ.substring(0, 1));
        String primary = ((StringValue) typeAsMap.get(StringValue.bmp("p"))).getStringValue();
        if (primary.isEmpty()) {
            return SequenceType.makeSequenceType(AnyItemType.getInstance(), cardinality);
        }
        switch (primary.charAt(0)) {
            case 'A':
                BuiltInAtomicType bat = BuiltInAtomicType.fromAlphaCode(primary);
                assert bat != null;
                return SequenceType.makeSequenceType(bat, cardinality);
            case 'N':
                return SequenceType.makeSequenceType(nodeTestFromAlphaCode(primary), cardinality);
            case 'F':
                if (primary.length() == 1) {
                    MapItem resultType = (MapItem) typeAsMap.get(StringValue.bmp("r"));
                    ArrayItem argTypes = (ArrayItem) typeAsMap.get(StringValue.bmp("a"));
                    assert resultType != null;
                    assert argTypes != null;
                    SequenceType resultType1 = fromMap(resultType);
                    int arity = argTypes.arrayLength();
                    SequenceType[] argTypeArray = new SequenceType[arity];
                    for (int i = 0; i < arity; i++) {
                        argTypeArray[i] = fromMap((MapItem) argTypes.get(i));
                    }
                    SpecificFunctionType sft = new SpecificFunctionType(argTypeArray, resultType1);
                    return SequenceType.makeSequenceType(sft, cardinality);
                } else if (primary.charAt(1) == 'M') {
                    return SequenceType.makeSequenceType(MapType.ANY_MAP_TYPE, cardinality);
                } else if (primary.charAt(1) == 'A') {
                    return SequenceType.makeSequenceType(ArrayItemType.ANY_ARRAY_TYPE, cardinality);
                } else {
                    throw new AssertionError();
                }
            default:
                throw new AssertionError();
        }
    }

    private static NodeTest nodeTestFromAlphaCode(String code) {
        switch (code) {
            case "NE":
                return NodeKindTest.ELEMENT;
            case "NA":
                return NodeKindTest.ATTRIBUTE;
            case "NT":
                return NodeKindTest.TEXT;
            case "NC":
                return NodeKindTest.COMMENT;
            case "NP":
                return NodeKindTest.PROCESSING_INSTRUCTION;
            case "ND":
                return NodeKindTest.DOCUMENT;
            case "NN":
                return NodeKindTest.NAMESPACE;
            case "N":
            default:
                return AnyNodeTest.getInstance();
        }
    }

}

// Copyright (c) 2020-2022 Saxonica Limited

