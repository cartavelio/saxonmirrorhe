package com.saxonica.ee.bytecode.util;

import com.saxonica.ee.bytecode.*;
import com.saxonica.ee.bytecode.simtype.AtomicTypeValidator;
import com.saxonica.ee.bytecode.simtype.AtomicTypeValidatorCompiler;
import com.saxonica.ee.schema.UserAtomicType;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ICompilerService;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.value.*;
import org.objectweb.asm.ClassVisitor;
import org.objectweb.asm.ClassWriter;
import org.objectweb.asm.Opcodes;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.Method;
import org.objectweb.asm.util.TraceClassVisitor;

import javax.xml.transform.SourceLocator;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Stack;

/**
 * Controlling/service class for compilation of expressions to Java bytecode
 */
public class CompilerService implements ICompilerService {

    private final Configuration config;
    private int flags = ClassWriter.COMPUTE_MAXS | ClassWriter.COMPUTE_FRAMES;
    private final Stack<GeneratedMethodInfo> currentMethods = new Stack<>();
    private final Stack<GeneratedClassInfo> currentClasses = new Stack<>();
    private ClassLoader myClassLoader = null;

    private int maxClasses = 10000;
    private static int classCounter = 0; // counted for the VM, not for the Configuration
    private final HostLanguage hostLanguage;
    /**
     * Indicate in a map a list of methods to be added in the main class at the end of the class generation
     * ExpressionCompiler is the hashMap key because we only expect one generated method per ExpressionCompiler
     */
    private final HashMap<ExpressionCompiler, GeneratedExpression> addMethods = new HashMap<>();
    public static int tracePoint = 0;
    private static int sequenceNumber = 0;
    private boolean displayByteCode;
    private boolean debugByteCode;
    private String debugByteCodeDirectory;
    public int mostRecentLineNumber = -1;
    private static final HashMap<String, ExpressionCompiler> expressionCompilers = new HashMap<>();

    static {
        expressionCompilers.put("ExistsCompiler", new ExistsCompiler());
        expressionCompilers.put("StringJoinCompiler", new StringJoinCompiler());
        expressionCompilers.put("RoundingCompiler", new RoundingCompiler());
        expressionCompilers.put("StringFnCompiler", new StringFnCompiler());
        expressionCompilers.put("ConcatCompiler", new ConcatCompiler());
        expressionCompilers.put("PositionCompiler", new PositionCompiler());
        expressionCompilers.put("LastCompiler", new LastCompiler());
        expressionCompilers.put("StartsWithCompiler", new StartsWithCompiler());
        expressionCompilers.put("AccessorFnCompiler", new AccessorFnCompiler());
        expressionCompilers.put("NameCompiler", new NameCompiler());
        expressionCompilers.put("LocalNameCompiler", new LocalNameCompiler());
        expressionCompilers.put("ContainsCompiler", new ContainsCompiler());
        expressionCompilers.put("ForceCaseCompiler", new ForceCaseCompiler());
        expressionCompilers.put("SubstringCompiler", new SubstringCompiler());
        expressionCompilers.put("StringLengthCompiler", new StringLengthCompiler());
        expressionCompilers.put("EmptyCompiler", new EmptyCompiler());
        expressionCompilers.put("CountCompiler", new CountCompiler());
        expressionCompilers.put("BooleanFnCompiler", new BooleanFnCompiler());
        expressionCompilers.put("NumberFnCompiler", new NumberFnCompiler());
        expressionCompilers.put("NormalizeSpaceCompiler", new NormalizeSpaceCompiler());
        expressionCompilers.put("NamespaceUriFnCompiler", new NamespaceUriFnCompiler());
        expressionCompilers.put("SubstringBeforeCompiler", new SubstringBeforeCompiler());
        expressionCompilers.put("NotFnCompiler", new NotFnCompiler());
        expressionCompilers.put("TranslateCompiler", new TranslateCompiler());
        expressionCompilers.put("GenerateIdCompiler", new GenerateIdCompiler());
        expressionCompilers.put("NodeNameFnCompiler", new NodeNameFnCompiler());
        expressionCompilers.put("RootFunctionCompiler", new RootFunctionCompiler());
        expressionCompilers.put("MatchesCompiler", new MatchesCompiler());
        expressionCompilers.put("SumCompiler", new SumCompiler());
        expressionCompilers.put("BaseURICompiler", new BaseURICompiler());
        expressionCompilers.put("DateTimeConstructorCompiler", new DateTimeConstructorCompiler());
        expressionCompilers.put("QNameFnCompiler", new QNameFnCompiler());
        expressionCompilers.put("SubstringAfterCompiler", new SubstringAfterCompiler());
    }

    public CompilerService(Configuration config, HostLanguage hostLanguage) {
        this.config = config;
        this.hostLanguage = hostLanguage;
        if (config.getBooleanProperty(Feature.DEBUG_BYTE_CODE)) {
            flags = ClassWriter.COMPUTE_MAXS;
            displayByteCode = true;
            debugByteCode = true;
            debugByteCodeDirectory = config.getConfigurationProperty(Feature.DEBUG_BYTE_CODE_DIR);
            if (debugByteCodeDirectory == null) {
                debugByteCodeDirectory = "saxonByteCode";
            }
        }
        if (config.getBooleanProperty(Feature.DISPLAY_BYTE_CODE)) {
            displayByteCode = true;
        }
        maxClasses = config.getConfigurationProperty(Feature.MAX_COMPILED_CLASSES);
    }

    public static boolean isLanguagePrimitiveType(AtomicType targetType) {
        int prim = targetType.getFingerprint();

        return prim == StandardNames.XS_STRING || prim == StandardNames.XS_BOOLEAN || prim == StandardNames.XS_DOUBLE || prim == StandardNames.XS_FLOAT ||
                prim == StandardNames.XS_UNTYPED_ATOMIC || prim == StandardNames.XS_INTEGER;
    }

    public boolean isDisplayByteCode() {
        return displayByteCode;
    }

    public boolean isDebugByteCode() {
        return debugByteCode;
    }

    public HostLanguage getHostLanguage() {
        return hostLanguage;
    }

    @SuppressWarnings("ResultOfMethodCallIgnored")
    public PrintWriter createPrintWriter(String filename) {
        if (debugByteCodeDirectory != null) {
            File dir = new File(debugByteCodeDirectory);
            dir.mkdirs();
            File file = new File(dir, filename);
            try {
                return new PrintWriter(new FileWriter(file, true));
            } catch (IOException e) {
                return new PrintWriter(System.out);
            }
        } else {
            return new PrintWriter(System.out);
        }
    }

    public ExpressionCompiler getNamedExpressionCompiler(String name) {
        ExpressionCompiler ec = expressionCompilers.get(name);
        if (ec == null) {
            try {
                ec = (ExpressionCompiler) config.getInstance("com.saxonica.ee.bytecode." + name);
                expressionCompilers.put(name, ec);
            } catch (XPathException e) {
                throw new AssertionError(e);
            }
        }
        return ec;
    }


    /**
     * The methods adds the
     *
     * @param ec - The ExpressionCompiler that needs to generate a method
     * @param e  - The expression represented by the Expressioncompiler
     */
    public void addNewMethod(ExpressionCompiler ec, Expression e) {
        addMethods.put(ec, new GeneratedExpression(ec, e));
    }

    /**
     * sub-class GeneratedExpression - used to keep track of ExpressionCompiler classes that need to generate a method
     */
    public class GeneratedExpression {
        ExpressionCompiler exprCompiler;
        Expression expr;

        public GeneratedExpression(ExpressionCompiler ec, Expression e) {
            exprCompiler = ec;
            expr = e;
        }

        public ExpressionCompiler getExpressionCompiler() {
            return exprCompiler;
        }

        public Expression getExpression() {
            return expr;
        }
    }

    /**
     * Compile code to evaluate a variable reference, or any other expresssion,
     * leaving a Sequence on the stack
     *
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled
     */

    public void compileToSequence(Expression expression)
            throws CannotCompileException {
        Generator ga = getCurrentGenerator();
        GeneratedMethodInfo methodInfo = getCurrentMethod();
        if (expression instanceof VariableReference) {
            VariableReferenceCompiler.compileToSequence(this, (VariableReference) expression);
        } else if (expression instanceof Literal) {
            if (Literal.isEmptySequence(expression)) {
                ga.invokeStaticMethod(EmptySequence.class, "getInstance");
            } else {
                ExpressionCompiler.allocateStatic(this, ((Literal) expression).getGroundedValue());
            }
        } else if (Cardinality.allowsMany(expression.getCardinality())) {
            compileToIterator(expression);
            ga.invokeStaticMethod(SequenceTool.class, "toGroundedValue", SequenceIterator.class);
        } else {
            compileToItem(expression);
            if (Cardinality.allowsZero(expression.getCardinality())) {
                LabelInfo nonNull = methodInfo.newLabel("nonNull");
                ga.dup();
                ga.ifNonNull(nonNull.label());
                ga.pop();
                ga.invokeStaticMethod(EmptySequence.class, "getInstance");
                methodInfo.placeLabel(nonNull);
            }
        }
    }


    public static class StaticVariableInfo {
        public String name;
        public Class<?> classVar;
        public Object object;
    }

    public static class StaticSubClasses {
        public String name;
        public Object object;
    }

    public HashMap<ObjectIdentityWrapper, StaticSubClasses> cClassMap = new HashMap<>();

    /*@Nullable*/
    public Class<?> getCompiledClass(Expression expr) {
        ObjectIdentityWrapper oiw = new ObjectIdentityWrapper(expr);
        if (cClassMap.get(oiw) != null) {
            return (Class<?>) cClassMap.get(oiw).object;
        }
        return null;
    }

    public void setCompiledClass(Expression expr, Class<?> clss) {
        StaticSubClasses staticSubClass = new StaticSubClasses();
        staticSubClass.name = expr.getExpressionName();

        staticSubClass.object = clss;
        ObjectIdentityWrapper oiw = new ObjectIdentityWrapper(expr);
        cClassMap.put(oiw, staticSubClass);
    }

    /**
     * The ObjectIdentityWrapper class is a wrapper around an object to perform stronger equality checking
     * than the basic equals() method of the underlying object. This class is used when deciding whether two calls
     * on allocateStatic() can share the same static variable. The equals() test is too weak for this,
     * for example a DecimalValue can be equal() to a DoubleValue.
     * <p>Some examples of how this class is used:</p>
     * <ul>
     * <li>For expression objects, it compares the Java object identity. (Because equals() is expensive, and
     * reports expressions as equal even if they have different source locations)</li>
     * <li>For atomic values, it's generally the same test as an XSD identity test. For QNames, the prefix is
     * significant. For calendar values, the timezone is significant. In all cases, the type annotation is significant.</li>
     * <li>For structured QNames, the prefix is significant.</li>
     * </ul>
     */

    private static class ObjectIdentityWrapper {

        private final Object theObject;

        public ObjectIdentityWrapper(Object obj) {
            theObject = obj;
        }

        public Object getObject() {
            return theObject;
        }

        public int hashCode() {
            if (theObject instanceof IdentityComparable) {
                return ((IdentityComparable) theObject).identityHashCode();
            } else {
                return theObject.hashCode();
            }
        }

        public boolean equals(Object o2) {
            if (o2 instanceof ObjectIdentityWrapper) {
                ObjectIdentityWrapper oiw2 = (ObjectIdentityWrapper) o2;
                if (oiw2.theObject.getClass() == theObject.getClass()) {
                    if (theObject instanceof IdentityComparable) {
                        if (!((IdentityComparable) oiw2.theObject).isIdentical((IdentityComparable) this.theObject)) {
                            return false;
                        }
                        if (theObject instanceof AtomicValue) {
                            // Atomic values can be identical even if their type labels differ
                            return ((AtomicValue) theObject).getItemType().equals(((AtomicValue) oiw2.theObject).getItemType());
                        }
                        return true;
                    } else {
                        return oiw2.theObject.equals(theObject);
                    }
                }
            }
            return false;
        }
    }

    public static class GeneratedClassInfo {
        String className;
        Class<?> superclass;
        ClassWriter cw;
        HashMap<ObjectIdentityWrapper, CompilerService.StaticVariableInfo> staticVarsMap =
                new HashMap<>();

        public String getClassName() {
            return className;
        }

        public ClassWriter getClassWriter() {
            return cw;
        }

        public Class<?> getSuperclass() {
            return superclass;
        }


        public void setClassName(String className) {
            this.className = className;
        }

        public void setSuperClass(Class<?> superclass) {
            this.superclass = superclass;
        }

        public void setClassWriter(ClassWriter cw) {
            this.cw = cw;
        }


        public String getTypeDescriptor() {
            StringBuilder newBuffer = new StringBuilder("L");

            for (int i = 0; i < className.length(); i++) {
                char c = className.charAt(i);
                newBuffer.append(c == '.' ? '/' : c);
            }
            newBuffer.append(';');
            return newBuffer.toString();
        }

    }


    /**
     * Get a unique number which is different from any other number allocated by a call on this method,
     * for the same CompilerService instance
     *
     * @return a unique number
     */

    public static int getUniqueNumber() {
        return sequenceNumber++;
    }

    /**
     * Generate code to set a new current Outputter on the top of the receiver stack.
     * <p>Runtime precondition: the new Outputter is on the top of the bytecode stack</p>
     * <p>Runtime postcondition: the new Outputter is now the current receiver; the previous
     * current receiver has been added to the receiver stack. The top entry from the
     * bytecode stack has been consumed.</p>
     *
     * @param ga the code generator
     */

    public void pushNewOutputterInfo(Generator ga) {
        //if (isDebugByteCode()) {
        //    ga.showObject(this, "Push receiver: ");
        //}
        GeneratedMethodInfo methodInfo = getCurrentMethod();
        int newOutputterPosition = methodInfo.allocateLocal(Outputter.class);
        ga.storeLocal(newOutputterPosition);
        methodInfo.pushOutputterInfo(newOutputterPosition);
    }

    /**
     * Generate code to reinstate the previous current receiver by popping it from the
     * receiver stack.
     * <p>Runtime precondition: there is a non-empty receiver stack</p>
     * <p>Runtime postcondition: the top entry in the receiver stack has been removed from the receiver
     * stack and is now the current receiver.</p>
     */

    public void popOutputterInfo() {
        GeneratedMethodInfo methodInfo = getCurrentMethod();
        methodInfo.popOutputterInfo();
    }


    public void pushNewMethodInfo(Generator ga, boolean contextIsArgument, int contextVariablePosition, int outputterVariablePosition) {
        mostRecentLineNumber = -1;
        GeneratedMethodInfo methodInfo = new GeneratedMethodInfo();
        methodInfo.currentGenerator = ga;
        methodInfo.debug = isDebugByteCode();
        methodInfo.pushContextVariableInfo(contextVariablePosition, contextIsArgument);
        methodInfo.pushOutputterInfo(outputterVariablePosition);
        currentMethods.push(methodInfo);
    }

    /**
     * Initialize context information for a new method. On entry, the XPathContext object must
     * be on the top of stack.
     *
     * @param ga                  the code generator
     * @param holdReceiverAsLocal true if a local variable on the bytecode stack is to be
     *                            allocated to hold the current receiver; if false, the receiver is obtained via the context when required
     */

    public void initNewMethod(Generator ga, boolean holdReceiverAsLocal) {
        GeneratedMethodInfo info = currentMethods.peek();
        ga.storeLocal(info.getContextVariablePosition());
        info.pushOutputterInfo(-1);
    }

    public void pushNewClassInfo(String className, Class<?> superclass, ClassWriter cw) {
        GeneratedClassInfo generatedClassInfo = new GeneratedClassInfo();
        generatedClassInfo.setClassWriter(cw);
        generatedClassInfo.setSuperClass(superclass);
        generatedClassInfo.setClassName(className);
        currentClasses.push(generatedClassInfo);
    }

    public Stack<GeneratedMethodInfo> getCurrentMethods() {
        return currentMethods;

    }

    public GeneratedMethodInfo getCurrentMethod() {
        if (currentMethods.isEmpty()) {
            return null;
        }
        GeneratedMethodInfo m = currentMethods.peek();
        if (m.getCurrentGenerator() != getCurrentGenerator()) {
            throw new AssertionError("Generator mismatch");
        }
        return m;

    }

    public GeneratedClassInfo getCurrentClass() {
        return currentClasses.peek();

    }

    /**
     * Get the Configuration used by this CompilerService
     *
     * @return the Saxon Configuration
     */

    public Configuration getConfiguration() {
        return config;
    }


    /**
     * Get the flag options used by the ClassWriter
     *
     * @return the flags set
     */
    public int getFlags() {

        return flags;
    }


    /**
     * Compile a top-level expression to bytecode, and return the resulting expression
     *
     * @param expr            The expression to be compiled
     * @param objectName      The name of the object (e.g. function) being compiled
     * @param evaluationModes The evaluation modes for which code is generated. Currently a subset of
     *                        {@link net.sf.saxon.expr.Expression#PROCESS_METHOD},
     *                        {@link net.sf.saxon.expr.Expression#ITERATE_METHOD},
     *                        {@link net.sf.saxon.expr.Expression#EVALUATE_METHOD}. If no code is generated for
     *                        a given evaluation method, the fallback implementation from the superclass is used.
     * @return The compiled expression if compilation was possible; otherwise null
     */

    public CompiledExpression compileToByteCode(Expression expr, String objectName, int evaluationModes) throws CannotCompileException {
        //System.err.println("compileToByteCode " + Thread.currentThread().getId() + " compilerService " + this);

        try {
            checkMaxClassesLimit();
            ClassWriter cw = new ClassWriter(flags);

            ClassVisitor cv = cw;
            String className = "EE_" + makeValidJavaName(objectName) + "_" + getUniqueNumber() + (expr.hashCode() & 0x7fffffff);
            cv = makeAnnotatedTraceClassVisitor(cv, className);

            cw.visitSource(expr.getLocation().getSystemId(), null);
            cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                     "com/saxonica/ee/bytecode/GeneratedCode", new String[]{});
            pushNewClassInfo(className, GeneratedCode.class, cw);

            Method m = Method.getMethod("void <init> ()");
            Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
            ExpressionCompiler.visitLineNumber(this, ga, expr);
            ga.loadThis();
            ga.invokeConstructor(Type.getType(GeneratedCode.class), m);
            ga.returnValue();
            ga.endMethod();

            if ((evaluationModes & Expression.ITERATE_METHOD) != 0) {
                m = Method.getMethod("net.sf.saxon.om.SequenceIterator iterate(net.sf.saxon.expr.XPathContext)");
                ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
                pushNewMethodInfo(ga, true, 0, -1);
                try {
                    compileToIterator(expr);
                } catch (CannotCompileException e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }
                ga.returnValue();
                try {
                    ga.endMethod();
                } catch (Exception e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }
                popCurrentMethodInfo();
            }

            if ((evaluationModes & Expression.EVALUATE_METHOD) != 0) {
                m = Method.getMethod("net.sf.saxon.om.Item evaluateItem(net.sf.saxon.expr.XPathContext)");
                ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
                pushNewMethodInfo(ga, true, 0, -1);
                try {
                    compileToItem(expr);
                } catch (CannotCompileException e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }
                ga.returnValue();
                try {
                    ga.endMethod();
                } catch (Exception e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }
                popCurrentMethodInfo();

                if ((evaluationModes & Expression.ITERATE_METHOD) == 0) {
                    // If we're not generating an explicit iterate method, create one that invokes evaluateItem
                    m = Method.getMethod("net.sf.saxon.om.SequenceIterator iterate(net.sf.saxon.expr.XPathContext)");
                    ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
                    pushNewMethodInfo(ga, true, 0, -1);
                    // return SingletonIterator.makeIterator(evaluateItem(context))
                    ga.loadThis();
                    ga.loadArg(0);
                    ga.invokeInstanceMethod(GeneratedCode.class, "evaluateItem", XPathContext.class);
                    ga.invokeStaticMethod(SingletonIterator.class, "makeIterator", Item.class);
                    ga.returnValue();
                    try {
                        ga.endMethod();
                    } catch (Exception e) {
                        handleCodeGeneratorException(objectName, e);
                        return null;
                    }
                    popCurrentMethodInfo();
                }
            }

            if (evaluationModes == Expression.EFFECTIVE_BOOLEAN_VALUE) {
                m = Method.getMethod("boolean effectiveBooleanValue(net.sf.saxon.expr.XPathContext)");
                ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
                pushNewMethodInfo(ga, true, 0, -1);
                try {
                    compileToBoolean(expr);
                } catch (CannotCompileException e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }
                ga.returnValue();
                try {
                    ga.endMethod();
                } catch (Exception e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }
                popCurrentMethodInfo();

                m = Method.getMethod("net.sf.saxon.om.Item evaluateItem(net.sf.saxon.expr.XPathContext)");
                ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
                pushNewMethodInfo(ga, true, 0, -1);
                ga.loadThis();
                ga.loadArg(0);
                ga.invokeInstanceMethod(GeneratedCode.class, "effectiveBooleanValue", XPathContext.class);
                ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
                ga.returnValue();
                try {
                    ga.endMethod();
                } catch (Exception e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }
                popCurrentMethodInfo();

                m = Method.getMethod("net.sf.saxon.om.SequenceIterator iterate(net.sf.saxon.expr.XPathContext)");
                ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
                pushNewMethodInfo(ga, true, 0, -1);
                // return SingletonIterator.makeIterator(evaluateItem(context))
                ga.loadThis();
                ga.loadArg(0);
                ga.invokeInstanceMethod(GeneratedCode.class, "evaluateItem", XPathContext.class);
                ga.invokeStaticMethod(SingletonIterator.class, "makeIterator", Item.class);
                ga.returnValue();
                try {
                    ga.endMethod();
                } catch (Exception e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }
                popCurrentMethodInfo();

            }

            if ((evaluationModes & Expression.PROCESS_METHOD) != 0) {
                m = Method.getMethod("void process(net.sf.saxon.event.Outputter, net.sf.saxon.expr.XPathContext)");
                ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
                pushNewMethodInfo(ga, true, 1, 0);
                ExpressionCompiler.visitLineNumber(this, ga, expr);
                // mg.loadArg(0);
                try {
                    compileToPush(expr);
                } catch (CannotCompileException e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }
                ga.returnValue();
                try {
                    ga.endMethod();
                } catch (Exception e) {
                    handleCodeGeneratorException(objectName, e);
                    return null;
                }

                popCurrentMethodInfo();
            }

            //Generate any additional methods indicated by the generateMethod hashMap of ExpressionCompiler classes
            for (GeneratedExpression ge : addMethods.values()) {
                ge.getExpressionCompiler().generateMethod(this, ge.getExpression(), cv);
            }

            //pw.flush();
            cv.visitEnd();

            String message = objectName + " at line " + expr.getLocation().getLineNumber() + " in " + expr.getLocation().getSystemId();
            try {
                ExpressionCompiler.verify(cw, message, isDebugByteCode());
            } catch (IllegalStateException e) {
                // For diagnostics, try to drill down to compiling smaller expressions
                for (Operand o : expr.operands()) {
                    Expression child = o.getChildExpression();
                    System.err.println("Trying to compile " + child.toString());
                    compileToByteCode(child, child.toString(), evaluationModes);
                }
            }

            Class<?> generated = makeClass(cw, className);
            GeneratedCode gc;
            try {
                gc = (GeneratedCode) generated.newInstance();
            } catch (InstantiationException | IllegalAccessException e) {
                e.printStackTrace();
                return null;
            }
            gc.setConfiguration(config);
            CompiledExpression compiledExpression = new CompiledExpression(expr, gc);
            compiledExpression.setClassName(className);
            return compiledExpression;
        } catch (VerifyError | ClassFormatError | RuntimeException e) {
            handleCodeGeneratorException(objectName, e);
            currentClasses.pop();
            return null;
        }
    }

    private void handleCodeGeneratorException(String objectName, Throwable e) {
        // If assertions are enabled, we crash out; otherwise we recover
        String message = e.getMessage();
        if (e instanceof CannotCompileException) {
            Expression exp = ((CannotCompileException) e).getExpression();
            if (exp != null) {
                Location loc = exp.getLocation();
                message += " at " + loc.getLineNumber() + " of " + loc.getSystemId();
            }
        }
        if (e instanceof CannotCompileException && ((CannotCompileException) e).isRecoverable()) {
            // No action
        } else if (Configuration.isAssertionsEnabled() || isDebugByteCode()) {
            System.err.println("Cannot compile " + objectName + ": " + message);
            e.printStackTrace(System.err);
            throw new AssertionError(e);
        } else if (config.isTiming()) {
            config.getLogger().warning(
                    "Cannot compile " + objectName + "(" + message + ") - using interpreter");
        }
    }

    /**
     * Generate bytecode to evaluate an expression leaving a SequenceIterator on the stack
     *
     * @param expr the expression to be evaluated
     * @return details of the SequenceIterator that will be found on the stack on completion
     * @throws CannotCompileException if the expression cannot be compiled
     */

    public IteratorDetails compileToIterator(Expression expr) throws CannotCompileException {
        ExpressionCompiler comp = getExpressionCompiler(expr);
        if (expr instanceof ByteCodeCandidate) {
            expr = ((ByteCodeCandidate) expr).getBaseExpression();
        }
        IteratorDetails details = comp.compileToIterator(this, expr);
        //System.err.println("Returning " + details.iteratorClass);
        return details;
    }

    /**
     * Generate bytecode to evaluate an expression by writing events to the current
     * Outputter (leaving nothing on the stack at execution time)
     *
     * @param expr the expression to be evaluated
     * @throws CannotCompileException if the expression cannot be compiled
     */

    public void compileToPush(Expression expr) throws CannotCompileException {
        ExpressionCompiler comp = getExpressionCompiler(expr);
        if (expr instanceof ByteCodeCandidate) {
            expr = ((ByteCodeCandidate) expr).getBaseExpression();
        }
        comp.compileToPush(this, expr);
    }

    /**
     * Generate bytecode to evaluate an expression leaving either an Item or null on the stack
     * (null indicating that the expression evaluated to an empty sequence).
     *
     * @param expr the expression to be evaluated
     * @throws CannotCompileException if the expression cannot be compiled
     * @return the class of Item that will be left on the stack after execution of the generated code
     */

    public Class<? extends Item> compileToItem(Expression expr) throws CannotCompileException {
        ExpressionCompiler comp = getExpressionCompiler(expr);
        if (expr instanceof ByteCodeCandidate) {
            expr = ((ByteCodeCandidate) expr).getBaseExpression();
        }
        return comp.compileToItem(this, expr);
    }

    /**
     * Generate bytecode to evaluate an expression leaving a boolean on the stack.
     * The boolean represents the effective boolean value of the result of the evaluation.
     * This method must be implemented for all expressions except where it is statically known
     * that the expression has no effective boolean value
     *
     * @param expr the expression to be evaluated
     * @throws CannotCompileException if the expression cannot be compiled
     */

    public void compileToBoolean(Expression expr) throws CannotCompileException {
        ExpressionCompiler comp = getExpressionCompiler(expr);
        if (expr instanceof ByteCodeCandidate) {
            expr = ((ByteCodeCandidate) expr).getBaseExpression();
        }
        comp.compileToBoolean(this, expr);
    }

    /**
     * Generate bytecode to evaluate an expression leaving a primitive on the stack.
     * The primitive represents the effective value of the result of the evaluation.
     *
     * @param expr the expression to be evaluated
     * @throws CannotCompileException if the expression cannot be compiled
     */

    public void compileToPrimitive(Expression expr, Class<?> c) throws CannotCompileException {
        ExpressionCompiler comp = getExpressionCompiler(expr);
        if (expr instanceof ByteCodeCandidate) {
            expr = ((ByteCodeCandidate) expr).getBaseExpression();
        }
        comp.compileToPrimitive(this, expr, c, null);
    }


    private ExpressionCompiler getExpressionCompiler(Expression expr) throws CannotCompileException {
        return ExpressionCompilerMap.getExpressionCompiler(expr, this);
    }


    public boolean isInRangeForInt(Expression exp) {
        IntegerValue[] bounds = exp.getIntegerBounds();
        return bounds != null && bounds[0].compareTo(Integer.MIN_VALUE) >= 0 && bounds[1].compareTo(Integer.MAX_VALUE) <= 0;
    }

    public boolean isInRangeForLong(Expression exp) {
        IntegerValue[] bounds = exp.getIntegerBounds();
        return bounds != null && bounds[0].compareTo(Long.MIN_VALUE) >= 0 && bounds[1].compareTo(Long.MAX_VALUE) <= 0;
    }

    /**
     * Compile an expression that returns a singleton (or empty) result of type xs:double, xs:float, xs:string,
     * xs:untypedAtomic.
     * An ExpressionCompiler must support this method if the expression is known statically to return one of these
     * types.
     * The code generated will leave an "unboxed" result on the bytecode stack: specifically a double, a float,
     * a String or CharSequence.
     *
     * @param expr          the expression to be compiled
     * @param requiredClass the required class. For an expression with static type xs:double, this will be
     *                      Double.TYPE; for xs:float it will be Float.TYPE; for xs:string and xs:untypedAtomic it may be either
     *                      String.class or UnicodeString.class. The expression compiler MUST generate code that leaves this class
     *                      of value on the top of the bytecode stack.
     *                      For an expression whose static type is xs:integer, the requiredClass may be Integer.TYPE or Long.TYPE,
     *                      provided that either (a) the expression is known always to deliver a value in the range of an int or long
     *                      respectively, or (b) that any other value would exceed system limits in the context where it is used (for
     *                      example, if it is used as an index into a sequence then it cannot exceed a 32-bit integer). All expressions
     *                      that have a static type of xs:integer must therefore implement this method.
     * @param onEmpty       action to be taken when the expression returns an empty result.
     * @throws CannotCompileException if the expression cannot be compiled
     */

    public void compileToPrimitive(Expression expr, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        ExpressionCompiler comp = getExpressionCompiler(expr);
        if (expr instanceof ByteCodeCandidate) {
            expr = ((ByteCodeCandidate) expr).getBaseExpression();
        }
        comp.compileToPrimitive(this, expr, requiredClass, onEmpty);
    }


    public Generator getCurrentGenerator() {
        return currentMethods.peek().currentGenerator;
    }

    public void popCurrentMethodInfo() {
        currentMethods.peek().checkLabels();
        currentMethods.pop();
    }


    /**
     * Generate code to load the current XPathContext, leaving it on the top of the stack
     */

    public void generateGetContext() {
        GeneratedMethodInfo gci = currentMethods.peek();
        if (gci.isContextVariableAnArgument()) {
            getCurrentGenerator().loadArg(gci.getContextVariablePosition());
        } else {
            getCurrentGenerator().loadLocal(gci.getContextVariablePosition());
        }
    }

    /**
     * Generate code to load the current Outputter, leaving it on the top of the stack
     */

    public void generateGetOutputter() {
        GeneratedMethodInfo gci = currentMethods.peek();
        if (gci.getOutputterPosition() == -1) {
            throw new AssertionError();
        } else {
            getCurrentGenerator().loadArgOrLocal(gci.getOutputterPosition());
        }
    }

    /**
     * Generate code to throw an XPathException. This can only be used if all properties of the exception
     * including the message are known statically.
     *
     * @param message     the error message
     * @param errorCode   the (local part of the) error code to be generated
     * @param locator     the location in the source code where the error occurs
     * @param isTypeError true if this is a type error
     */

    public void generateDynamicError(
            String message,
            String errorCode,
            SourceLocator locator,
            boolean isTypeError) {
        Generator ga = getCurrentGenerator();
        ga.push(message);
        ga.push(errorCode);
        pushLocation(locator, ga);
        ga.push(isTypeError);
        ga.invokeStaticMethod(Callback.class, "makeXPathException", String.class, String.class, String.class, Integer.TYPE, Boolean.TYPE);
        ga.throwException();
    }

    /**
     * Generate code to throw an XPath Exception. The message is constructed at runtime using the MessageBuilder.
     *
     * @param builder     the error message builder. This generates code to build the error message at run-time and leave
     *                    it on the stack
     * @param errorCode   the (local part of the) error code to be generated
     * @param locator     the location in the source code where the error occurs
     * @param isTypeError true if this is a type error
     */

    public void generateParameterizedDynamicError(
            MessageBuilder builder,
            String errorCode,
            SourceLocator locator,
            boolean isTypeError) {
        Generator ga = getCurrentGenerator();
        builder.generateErrorMessage();
        ga.push(errorCode);
        pushLocation(locator, ga);
        ga.push(isTypeError);
        ga.invokeStaticMethod(Callback.class, "makeXPathException", String.class, String.class, String.class, Integer.TYPE, Boolean.TYPE);
        ga.throwException();
    }

    private void pushLocation(SourceLocator locator, Generator ga) {
        if (locator != null) {
            ga.push(locator.getSystemId());
            ga.push(locator.getLineNumber());
        } else {
            ga.pushNull();
            ga.push(-1);
        }
    }


    /**
     * Having written the bytecode for all the methods for a class, create the Class itself so it can be used
     *
     * @param writer    the writer that was used to create the class
     * @param className the name of the class
     * @return the constructed Class object
     */

    public Class<?> makeClass(ClassWriter writer, String className) {
        // System.err.println("Make class " + className);
        ClassVisitor cv = writer;
        cv = makeAnnotatedTraceClassVisitor(cv, className);

        if (!getCurrentClass().className.equals(className)) {
            throw new AssertionError(getCurrentClass().className + " != " + className);
        }
        for (StaticVariableInfo staticVar : getCurrentClass().staticVarsMap.values()) {

            cv.visitField(Opcodes.ACC_PUBLIC + Opcodes.ACC_STATIC, staticVar.name, Type.getType(staticVar.classVar).getDescriptor(), null, null).visitEnd();

        }
        //pw.flush();
        cv.visitEnd();
        final byte[] classFile = writer.toByteArray();

        if (myClassLoader == null) {
            myClassLoader = Version.platform.makeGeneratedClassLoader(config, getClass());
        }

        try {
            ((GeneratedClassLoader) myClassLoader).registerClass(className, classFile);
            //System.err.println("registerClass " + className);
            Class<?> myClass = myClassLoader.loadClass(className);
            myClass.newInstance();
            for (StaticVariableInfo staticVar : getCurrentClass().staticVarsMap.values()) {

                try {
                    myClass.getField(staticVar.name).set(null, staticVar.object);
                } catch (NoSuchFieldException ex) {
                    System.out.println(staticVar.name + " NoSuchException error");
                    String classStr = new String(classFile, StandardCharsets.UTF_8);

                    System.out.println(classStr);
                    throw new AssertionError(ex);

                }
            }
            currentClasses.pop();
            return myClass;
        } catch (Exception e) {
            throw new AssertionError(e);
        }
    }


    /**
     * Create a StaticVariableInfo containing the information needed to allow a static reference
     * to a given object to be added as a static field in the currently-being-compiled class
     *
     * @param value the object to be referenced
     * @return a StaticVariableInfo object, which will have been queued for later addition to the class
     */

    public StaticVariableInfo allocateStaticVariableInfo(Object value) {
        ObjectIdentityWrapper oiw = new ObjectIdentityWrapper(value);
        if (getCurrentClass().staticVarsMap.get(oiw) != null) {
            return getCurrentClass().staticVarsMap.get(oiw);
        }

        String name = value.getClass().getSimpleName();
        if (value.getClass().getSimpleName().endsWith("[]")) {
            name = "n" + name.substring(0, name.length() - 2) + getUniqueNumber();
        } else {
            name = "n" + name + getUniqueNumber();
        }


        StaticVariableInfo staticVariableInfo = new StaticVariableInfo();
        staticVariableInfo.name = name;
        Class<?> staticClass = value.getClass();

        while (staticClass.getName().startsWith("gen_")) {
            staticClass = staticClass.getSuperclass();
        }

        staticVariableInfo.classVar = staticClass;
        staticVariableInfo.object = value;


        getCurrentClass().staticVarsMap.put(oiw, staticVariableInfo);
        return staticVariableInfo;
    }

    private void appendJavaName(String local, StringBuilder sb) {
        for (int i = 0; i < local.length(); i++) {
            char c = local.charAt(i);
            if (!Character.isJavaIdentifierPart(c)) {
                if (c == '/') {
                    sb.append("_Slash_");
                } else {
                    sb.append('_');
                }
            } else {
                sb.append(c);
            }
        }
    }

    /**
     * Given a candidate Java name, make it into a valid Java name by removing surplus white spaces
     * and truncating it to a maximum of 256 characters if length is greater than 256 characters.
     *
     * @param name a candidate Java name
     * @return a valid Java name
     */

    public String makeValidJavaName(String name) {
        name = name.replace(" ", "");
        if (name.length() > 256) {
            name = name.substring(0, 256);
        }
        StringBuilder sb = new StringBuilder(name.length());
        appendJavaName(name, sb);
        return sb.toString();
    }

    /**
     * Make a class visitor for displaying bytecode
     *
     * @param cv the class visitor
     */

    public ClassVisitor makeAnnotatedTraceClassVisitor(ClassVisitor cv, String className) {
        if (isDisplayByteCode()) {
            //cv = new AnnotatedTraceClassVisitor(cv, pw);
            // AnnotatedTraceClassVisitor no longer works with ASM 5.0.3, as TraceClassVisitor has become final
            PrintWriter pw = createPrintWriter(className + ".txt");
            return new TraceClassVisitor(cv, pw);
        } else {
            return cv;
        }
    }

    /**
     * Make a validator for user-defined atomic types
     *
     * @param type  the user-defined atomic type for which a validator is required
     * @param rules the conversion rules to be used by the validator
     * @return a custom implementation of {@link AtomicTypeValidator} specialised to this atomic type
     * @throws CannotCompileException if bytecode generation for this type is not possible for any reason
     */

    public AtomicTypeValidator compileAtomicValidator(UserAtomicType type, ConversionRules rules) throws CannotCompileException {

        checkMaxClassesLimit();
        String objectName;
        if (type.isAnonymousType()) {
            objectName = type.getDescription();
        } else {
            objectName = type.getTypeName().getDisplayName();
        }
        try {
            ClassWriter cw = new ClassWriter(flags);
            ClassVisitor cv = cw;
            String className = "TYPE_" + makeValidJavaName(objectName) + "_" + getUniqueNumber();
            cv = makeAnnotatedTraceClassVisitor(cv, className);

            //cw.visitSource(expr.getSystemId(), null);
            cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                     "com/saxonica/ee/bytecode/simtype/AtomicTypeValidator", new String[]{});
            pushNewClassInfo(className, GeneratedCode.class, cw);

            Method m = Method.getMethod("void <init> ()");
            Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
            ga.loadThis();
            ga.invokeConstructor(Type.getType(AtomicTypeValidator.class), m);
            ga.returnValue();
            ga.endMethod();


            m = Method.getMethod("net.sf.saxon.type.ValidationFailure validate(net.sf.saxon.str.UnicodeString)");
            ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
            pushNewMethodInfo(ga, false, 0, -1);
            try {
                new AtomicTypeValidatorCompiler(type).compileAtomicValidator(this, rules);
                //System.err.println("Compiled atomic type " + objectName);
            } catch (CannotCompileException e) {
                handleCodeGeneratorException(objectName, e);
                return null;
            }
            ga.returnValue();
            try {
                ga.endMethod();
            } catch (Exception e) {
                handleCodeGeneratorException(objectName, e);
                return null;
            }
            popCurrentMethodInfo();


            //pw.flush();
            cv.visitEnd();

            ExpressionCompiler.verify(cw, objectName, isDebugByteCode());

            Class<?> generated = makeClass(cw, className);
            AtomicTypeValidator gc;
            try {
                gc = (AtomicTypeValidator) generated.newInstance();
            } catch (InstantiationException | IllegalAccessException e) {
                e.printStackTrace();
                return null;
            }
            return gc;

        } catch (UnsupportedOperationException e) {
            if (isDebugByteCode()) {
                System.err.println("Cannot compile " + objectName + ": " + e.getMessage());
                e.printStackTrace(System.err);
            } else if (config.isTiming()) {
                config.getLogger().warning(
                        "Cannot compile " + objectName + "(" + e.getMessage() + ") - using interpreter");
            }
            //System.err.println("Warning: Bytecode generation failed for " + objectName + ": " + e.getMessage());
            currentClasses.pop();
            return null;
        } catch (VerifyError | ClassFormatError | RuntimeException e) {
            if (isDebugByteCode()) {
                System.err.println("Cannot compile " + objectName + ": " + e.getMessage());
                e.printStackTrace(System.err);
                throw e;
            } else if (config.isTiming()) {
                config.getLogger().warning(
                        "Cannot compile " + objectName + "(" + e.getMessage() + ") - using interpreter");
            }
            //System.err.println("Warning: Bytecode generation failed for " + objectName + ": " + e.getMessage());
            currentClasses.pop();
            return null;
        }

    }

    public void checkMaxClassesLimit() throws CannotCompileException {
//        if (classCounter%100 == 0) {
//            System.err.println(classCounter);
//        }
        if (classCounter++ > maxClasses) {
            throw new CannotCompileException("Too many compiled classes", true);
        }
    }


}

// Copyright (c) 2011-2022 Saxonica Limited


