package com.saxonica.expr;


import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.functions.IntegratedFunctionCall;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.type.coercion.CoercionRules;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.SequenceType;

import java.lang.reflect.*;
import java.lang.reflect.Type;
import java.util.function.Supplier;


/**
 * This class acts as a container for an extension function defined to call a method
 * in a user-defined class.
 * <p>Note that the binding of an XPath function call to a Java method is done in
 * class {@link com.saxonica.config.JavaExtensionLibrary}</p>
 */

@SuppressWarnings({"SynchronizationOnLocalVariableOrMethodParameter", "SynchronizeOnNonFinalField"})
public class JavaExtensionFunctionCall extends FunctionCall implements Callable {

    private Configuration config;
    private StructuredQName name;
    private AccessibleObject theMethod;
    private Class<?>[] theParameterTypes;
    private Type[] theGenericParameterTypes;
    private PJConverter[] argumentConverters;
    private JPConverter resultConverter;
    private boolean checkForNodes;
    private Class<?> theClass;
    private JavaExternalObjectType declaredType;
    private boolean returnVoidAsThis = false;
    private SequenceEvaluator[] argumentEvaluators = null;

    /**
     * Default constructor
     */

    public JavaExtensionFunctionCall() {
    }

    /**
     * Initialization: creates an ExtensionFunctionCall
     *
     * @param functionName the name of the function, for display purposes
     * @param theClass     the Java class containing the method to be called
     * @param object       the method, field, or constructor of the Java class to be called
     * @param config       the Configuration
     */

    public void init(StructuredQName functionName, Class<?> theClass, AccessibleObject object, Configuration config) {
        this.config = config;
        this.name = functionName;
        this.theClass = theClass;
        theMethod = object;
        if (theMethod instanceof Constructor) {
            theParameterTypes = ((Constructor<?>) theMethod).getParameterTypes();
        } else if (theMethod instanceof Method) {
            theParameterTypes = ((Method) theMethod).getParameterTypes();
        } else {
            theParameterTypes = null;
        }
        returnVoidAsThis = functionName.getNamespaceUri().toString().endsWith("?void=this");
    }

    /**
     * Get a name identifying the kind of expression, in terms meaningful to a user.
     *
     * @return a name identifying the kind of expression, in terms meaningful to a user.
     * The name will always be in the form of a lexical XML QName, and should match the name used
     * in explain() output displaying the expression.
     */
    @Override
    public String getExpressionName() {
        return "javaCall";
    }

    /**
     * Get the qualified of the function being called
     *
     * @return the qualified name
     */
    @Override
    public StructuredQName getFunctionName() {
        return name;
    }

    /**
     * Get the target function to be called
     *
     * @param context the dynamic evaluation context
     * @return the target function
     * @throws net.sf.saxon.trans.XPathException if the target function cannot be determined
     */
    @Override
    public FunctionItem getTargetFunction(XPathContext context) throws XPathException {
        return null;
    }

    /**
     * preEvaluate: this method suppresses compile-time evaluation by doing nothing
     * (because the external function might have side-effects and might use the context)
     *
     * @param visitor an expression visitor
     */

    @Override
    public Expression preEvaluate(ExpressionVisitor visitor) {
        return this;
    }


    /*@NotNull*/
    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        Expression e2 = super.typeCheck(visitor, contextInfo);
        if (e2 != this) {
            return e2;
        }

        Configuration config = visitor.getConfiguration();

        int firstParam = 0;
        int firstArg = 0;
        if (theMethod instanceof Constructor) {
            if (theParameterTypes == null) {
                theParameterTypes = ((Constructor<?>) theMethod).getParameterTypes();
            }
        } else if (theMethod instanceof Method) {
            if (theParameterTypes == null) {
                theParameterTypes = ((Method) theMethod).getParameterTypes();
            }
            if (theGenericParameterTypes == null) {
                theGenericParameterTypes = ((Method) theMethod).getGenericParameterTypes();
            }
            boolean isStatic = Modifier.isStatic(((Method) theMethod).getModifiers());
            firstArg = isStatic ? 0 : 1;
            boolean usesContext = theParameterTypes.length > 0 &&
                    (theParameterTypes[0] == XPathContext.class);
            firstParam = usesContext ? 1 : 0;
        } else if (theMethod instanceof Field) {
            boolean isStatic = Modifier.isStatic(((Field) theMethod).getModifiers());
            firstArg = isStatic ? 0 : 1;
        }
        argumentConverters = new PJConverter[getArity()];
        TypeChecker tc = config.getTypeChecker(false);
        if (firstArg != 0) {
            JavaExternalObjectType result;
            synchronized(config) {
                result = JavaExternalObjectType.of(theClass);
            }
            SequenceType st = SequenceType.makeSequenceType(result, StaticProperty.EXACTLY_ONE);
            ItemType at = getArg(0).getItemType();
            if (at instanceof JavaExternalObjectType) {
                Supplier<RoleDiagnostic> role =
                        () -> new RoleDiagnostic(RoleDiagnostic.FUNCTION, getFunctionName().getDisplayName(), 0);
                setArg(0, tc.staticTypeCheck(getArg(0), st, role, visitor));
                argumentConverters[0] = PJConverter.UnwrapExternalObject.INSTANCE;
            } else if (theClass.equals(java.lang.Object.class)) {
                // if the declared class is "Object" (bug 5044), then we unwrap the supplied value if it
                // is a wrapped Java external object, or use it as is otherwise
                argumentConverters[0] = PJConverter.ConditionalUnwrapExternalObject.INSTANCE;
            } else {
                st = PJConverter.getEquivalentSequenceType(theClass);
                argumentConverters[0] = PJConverter.allocate(
                    config, at, getArg(0).getCardinality(), theClass);
                if (st != null) {
                    Supplier<RoleDiagnostic> role =
                            () -> new RoleDiagnostic(RoleDiagnostic.FUNCTION, getFunctionName().getDisplayName(), 0);
                    try {
                        setArg(0, tc.staticTypeCheck(
                                getArg(0), st, role, visitor));
                    } catch (XPathException err) {
                        if (err.hasErrorCode("XPTY0004") &&
                                visitor.getStaticContext().getPackageData().isXSLT()) {
                            err.setErrorCode("XTDE1420");
                        }
                        throw err;
                    }
                }
            }
        }

        int j = firstParam;
        for (int i = firstArg; i < getArity(); i++) {
            SequenceType st = null;
            if (theGenericParameterTypes != null) {
                st = PJConverter.getParameterizedSequenceType(theGenericParameterTypes[j]);
            }
            if (st == null) {
                if (getArg(i).getItemType() instanceof JavaExternalObjectType &&
                        ((JavaExternalObjectType) getArg(i).getItemType()).getJavaClass().equals(theParameterTypes[j]) ) {
                    st = SequenceType.makeSequenceType(getArg(i).getItemType(), getArg(i).getCardinality());
                } else {
                    st = PJConverter.getEquivalentSequenceType(theParameterTypes[j]);
                }
            }
            if (st != null) {
                final int pos = i;
                Supplier<RoleDiagnostic> role =
                        () -> new RoleDiagnostic(RoleDiagnostic.FUNCTION, getFunctionName().getDisplayName(), pos);
                try {
                    setArg(i, tc.staticTypeCheck(
                            getArg(i), st, role, visitor));
                } catch (XPathException err) {
                    if (err.hasErrorCode("XPTY0004") &&
                            visitor.getStaticContext().getPackageData().isXSLT()) {
                        err.setErrorCode("XTDE1420");
                    }
                    // prevent repeated errors
                    theParameterTypes[j] = Object.class;
                    throw err;
                }
            }
            argumentConverters[i] = PJConverter.allocate(
                    config, getArg(i).getItemType(), getArg(i).getCardinality(), theParameterTypes[j]);
            j++;
        }

        if (declaredType == null) {
            if (theMethod instanceof Constructor) {
                // Constructors always return wrapped external objects, even if the Java class
                // is one known to Saxon such as java.util.Date
                JavaExternalObjectType result;
                //noinspection SynchronizationOnLocalVariableOrMethodParameter
                synchronized(config) {
                    result = JavaExternalObjectType.of(theClass);
                }
                JavaExternalObjectType resultType = result;
                resultConverter = new JPConverter.ExternalObjectWrapper(resultType);
                //resultConverter = JPConverter.allocate(theClass, config);
            } else if (theMethod instanceof Method) {
                Class<?> resultClass = ((Method) theMethod).getReturnType();
                Type genericResultType = ((Method) theMethod).getGenericReturnType();
                if (genericResultType == Void.TYPE && isReturnVoidAsThis()) {
                    JavaExternalObjectType result;
                    //noinspection SynchronizationOnLocalVariableOrMethodParameter
                    synchronized(config) {
                        result = JavaExternalObjectType.of(theClass);
                    }
                    resultConverter = new JPConverter.ExternalObjectWrapper(result);
                } else {
                    resultConverter = JPConverter.allocate(resultClass, genericResultType, config);
                }
            } else if (theMethod instanceof Field) {
                Class<?> resultClass = ((Field) theMethod).getType();
                Type genericResultType = ((Field) theMethod).getGenericType();
                resultConverter = JPConverter.allocate(resultClass, genericResultType, config);
            } else {
                throw new AssertionError("Unknown component type");
            }
        }

        ItemType resultType = resultConverter.getItemType();
        checkForNodes = resultType == AnyItemType.getInstance() || resultType instanceof NodeTest;
        resetLocalStaticProperties();

        return this;
    }

    public void prepareForDynamicCall(Configuration config) throws XPathException {
        argumentConverters = new PJConverter[getArity()];
        int firstParam = 0;
        int firstArg = 0;

        if (theMethod instanceof Method) {
            if (theParameterTypes == null) {
                theParameterTypes = ((Method) theMethod).getParameterTypes();
            }
            if (theGenericParameterTypes == null) {
                theGenericParameterTypes = ((Method) theMethod).getGenericParameterTypes();
            }
            boolean isStatic = Modifier.isStatic(((Method) theMethod).getModifiers());
            firstArg = isStatic ? 0 : 1;
            boolean usesContext = theParameterTypes.length > 0 &&
                    (theParameterTypes[0] == XPathContext.class);
            firstParam = usesContext ? 1 : 0;
        }
        if (firstArg != 0) {
            argumentConverters[0] = PJConverter.UnwrapExternalObject.INSTANCE;
        }
        int j = firstParam;
        for (int i = firstArg; i < getArity(); i++) {
            argumentConverters[i] = PJConverter.allocate(
                    config, AnyItemType.getInstance(), StaticProperty.ALLOWS_MANY, theParameterTypes[j]);
            j++;
        }

        if (theMethod instanceof Constructor) {
            // Constructors always return wrapped external objects, even if the Java class
            // is one known to Saxon such as java.util.Date
            JavaExternalObjectType result;
            synchronized(config) {
                result = JavaExternalObjectType.of(theClass);
            }
            JavaExternalObjectType resultType = result;
            resultConverter = new JPConverter.ExternalObjectWrapper(resultType);
            //resultConverter = JPConverter.allocate(theClass, config);
        } else if (theMethod instanceof Method) {
            Class<?> resultClass = ((Method) theMethod).getReturnType();
            Type resultType = ((Method) theMethod).getGenericReturnType();
            if (resultType == Void.TYPE && isReturnVoidAsThis()) {
                JavaExternalObjectType result;
                synchronized(config) {
                    result = JavaExternalObjectType.of(theClass);
                }
                resultConverter = new JPConverter.ExternalObjectWrapper(result);
            } else {
                resultConverter = JPConverter.allocate(resultClass, resultType, config);
            }
        } else if (theMethod instanceof Field) {
            Class<?> resultClass = ((Field) theMethod).getType();
            Type resultType = ((Field) theMethod).getGenericType();
            resultConverter = JPConverter.allocate(resultClass, resultType, config);
        } else {
            throw new AssertionError("Unknown component type");
        }

        ItemType resultType = resultConverter.getItemType();
        checkForNodes = resultType == AnyItemType.getInstance() || resultType instanceof NodeTest;
    }

    /**
     * When a call to a Java extension function appears in a context where there the required type
     * is a Java external object (typically an xsl:variable with a declared type, or as an argument
     * to another Java extension function), notify this required type so that the process of converting
     * the result to an XDM value can be short-circuited.
     * @param requiredType the required type of the result of the function, determined by the context
     *                     in which the function call appears
     * @throws XPathException if the required return type is incompatible with the type actually returned
     * by the Java method
     * @return Ok if the type has been successfully adjusted
     */

    @Override
    public boolean adjustRequiredType(JavaExternalObjectType requiredType) throws XPathException {
        Class<?> unconvertedResult = getReturnClass();
        if (requiredType.getJavaClass().isAssignableFrom(unconvertedResult)) {
            resultConverter = new JPConverter.ExternalObjectWrapper(requiredType);
        } else if (unconvertedResult.isAssignableFrom(requiredType.getJavaClass())) {
            // this one will require a run-time type check
            resultConverter = new JPConverter.ExternalObjectWrapper(requiredType);
        } else {
            throw new XPathException(
                "Cannot convert result of extension function to required type " + requiredType, "XPTY0004", getLocation());
        }
        declaredType = requiredType;
        return true;
    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings variables to be re-bound
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        JavaExtensionFunctionCall j = new JavaExtensionFunctionCall();
        j.name = getFunctionName();
        j.returnVoidAsThis = returnVoidAsThis;
        Expression[] newArgs = new Expression[getArity()];
        int i = 0;
        for (Operand o : operands()) {
            newArgs[i++] = o.getChildExpression().copy(rebindings);
        }
        j.setArguments(newArgs);
        j.theMethod = theMethod;
        j.theParameterTypes = theParameterTypes;
        j.theGenericParameterTypes = theGenericParameterTypes;
        j.argumentConverters = argumentConverters;
        j.resultConverter = resultConverter;
        j.checkForNodes = checkForNodes;
        j.theClass = theClass;
        ExpressionTool.copyLocationInfo(this, j);
        return j;
    }

    /**
     * Determine which aspects of the context the expression depends on. The result is
     * a bitwise-or'ed value composed from constants such as XPathContext.VARIABLES and
     * XPathContext.CURRENT_NODE
     */

    @Override
    public int getIntrinsicDependencies() {
        int depend = StaticProperty.HAS_SIDE_EFFECTS;
        if (theMethod instanceof Method) {
            Class<?>[] theParameterTypes = ((Method) theMethod).getParameterTypes();
            if (theParameterTypes.length > 0 && theParameterTypes[0] == XPathContext.class) {
                depend |=
                        StaticProperty.DEPENDS_ON_CONTEXT_ITEM |
                                StaticProperty.DEPENDS_ON_POSITION |
                                StaticProperty.DEPENDS_ON_LAST;
            }
        }
        return depend;
    }

    /**
     * Determine whether the expression can be evaluated without reference to the part of the context
     * document outside the subtree rooted at the context node.
     *
     * @return true if the expression has no dependencies on the context node, or if the only dependencies
     *         on the context node are downward selections using the self, child, descendant, attribute, and namespace
     *         axes.
     */

    @Override
    public boolean isSubtreeExpression() {
        return false;
    }

    /**
     * Add a representation of this expression to a PathMap. The PathMap captures a map of the nodes visited
     * by an expression in a source tree.
     * <p>The default implementation of this method assumes that an expression does no navigation other than
     * the navigation done by evaluating its subexpressions, and that the subexpressions are evaluated in the
     * same context as the containing expression. The method must be overridden for any expression
     * where these assumptions do not hold. For example, implementations exist for AxisExpression, ParentExpression,
     * and RootExpression (because they perform navigation), and for the doc(), document(), and collection()
     * functions because they create a new navigation root. Implementations also exist for PathExpression and
     * FilterExpression because they have subexpressions that are evaluated in a different context from the
     * calling expression.</p>
     *
     * @param pathMap        the PathMap to which the expression should be added
     * @param pathMapNodeSet the PathMapNodeSet to which the paths embodied in this expression should be added
     * @return the pathMapNode representing the focus established by this expression, in the case where this
     *         expression is the first operand of a path expression or filter expression. For an expression that does
     *         navigation, it represents the end of the arc in the path map that describes the navigation route. For other
     *         expressions, it is the same as the input pathMapNode.
     */

    @Override
    public PathMap.PathMapNodeSet addToPathMap(PathMap pathMap, PathMap.PathMapNodeSet pathMapNodeSet) {
        return addExternalFunctionCallToPathMap(pathMap, pathMapNodeSet);
    }


    /**
     * Ask whether the result needs to be checked to ensure it contains no nodes from the wrong Configuration
     *
     * @return true if the result needs to be checked
     */

    public boolean isNodeCheckRequired() {
        return checkForNodes;
    }

    /**
     * Evaluate the function.
     *
     * @param context The context in which the function is to be evaluated
     * @return a Value representing the result of the function.
     * @throws net.sf.saxon.trans.XPathException
     *          if the function cannot be evaluated.
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext context) throws XPathException {
        return makeElaborator().elaborateForPull().iterate(context);
    }

    /**
     * Evaluate the function returning a single item or null.
     *
     * @param context The context in which the function is to be evaluated
     * @return a Value representing the result of the function.
     * @throws net.sf.saxon.trans.XPathException if the function cannot be evaluated.
     */

    /*@NotNull*/
    @Override
    public Item evaluateItem(XPathContext context) throws XPathException {
        return iterate(context).next();
    }


    /**
     * Get the class containing the method being called
     *
     * @return the class containing the target method
     */

    public Class<?> getTargetClass() {
        return theClass;
    }

    /**
     * Get the target method (or field, or constructor) being called
     *
     * @return the target method, field, or constructor
     */

    public AccessibleObject getTargetMethod() {
        return theMethod;
    }

    /**
     * Get the types of the arguments
     *
     * @return an array of classes representing the declared types of the arguments to the method
     *         or constructor
     */

    public Class<?>[] getParameterTypes() {
        return theParameterTypes;
    }


    /**
     * Call an extension function previously identified using the bind() method. A subclass
     * can override this method.
     *
     * @param context   The XPath dynamic evaluation context
     * @param argValues The values of the arguments
     * @return The value returned by the extension function
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] argValues) throws XPathException {

        if (theMethod instanceof Constructor) {
            Constructor<?> constructor = (Constructor<?>) theMethod;
            if (theParameterTypes == null) { // WH
                theParameterTypes = constructor.getParameterTypes();
            }
            Object[] params = new Object[theParameterTypes.length];

            setupParams(argValues, params, theParameterTypes, 0, 0, context);

            try {
                Object result = invokeConstructor(constructor, params);
                return asSequence(result, context);
            } catch (InstantiationException err0) {
                throw new XPathException("Cannot instantiate class", err0);
            } catch (IllegalAccessException err1) {
                throw new XPathException("Constructor access is illegal", err1);
            } catch (IllegalArgumentException err2) {
                throw new XPathException("Argument is of wrong type", err2);
            } catch (NullPointerException err2) {
                throw new XPathException("Object is null");
            } catch (InvocationTargetException err3) {
                Throwable ex = err3.getTargetException();
                if (ex instanceof XPathException) {
                    throw ((XPathException) ex).maybeWithLocation(getLocation());
                } else {
                    if (context.getController().isTracing() ||
                            context.getConfiguration().getBooleanProperty(Feature.TRACE_EXTERNAL_FUNCTIONS)) {
                        err3.getTargetException().printStackTrace();
                    }
                    XPathException xe = new XPathException("Exception thrown by extension function {" + theMethod + "}: " +
                                                                   err3.getTargetException(), ex)
                            .withLocation(getLocation())
                            .withErrorCode(JavaExternalObjectType.classNameToQName(ex.getClass().getName()));
                    xe.setErrorObject(new ObjectValue<>(theClass));
                    throw xe;
                }
            }
        } else if (theMethod instanceof Method) {
            Method method = (Method) theMethod;
            boolean isStatic = Modifier.isStatic(method.getModifiers());
            Object theInstance;
            if (theParameterTypes == null) { // WH
                theParameterTypes = method.getParameterTypes();
            }
            boolean usesContext = theParameterTypes.length > 0 &&
                    (theParameterTypes[0] == XPathContext.class);
            if (isStatic) {
                theInstance = null;
            } else {
                if (argValues.length == 0) {
                    throw new XPathException(
                        "Must supply an argument for a non-static extension function", SaxonErrorCode.SXJE0001, getLocation());
                }
                try {
                    argValues[0] = argValues[0].materialize(); // bug 3723
                    theInstance = getTargetInstance(argValues[0], context);
                    // this fails if the first argument is not of a suitable class
                    if (theInstance == null) {
                        // bug 5044
                        throw new XPathException("First argument to instance method must not be an empty sequence", SaxonErrorCode.SXJE0001, getLocation());
                    }
                } catch (XPathException err) {
                    throw err.maybeWithLocation(getLocation());
                }
            }

            Object[] params = new Object[theParameterTypes.length];

            if (usesContext) {
                params[0] = context;
            }

            setupParams(argValues, params, theParameterTypes,
                    usesContext ? 1 : 0,
                    isStatic ? 0 : 1,
                    context
            );

            try {
                Object result = invokeMethod(method, theInstance, params);
                if (method.getReturnType() == Void.TYPE) {
                    if (returnVoidAsThis) {
                        return argValues[0];
                    } else {
                        return EmptySequence.getInstance();
                    }
                }
                return asSequence(result, context);
            } catch (XPathException xpe) {
                xpe.maybeSetLocation(getLocation());
                throw xpe;
            } catch (IllegalAccessException err1) {
                throw new XPathException("Method access is illegal", err1)
                        .withErrorCode(JavaExternalObjectType.classNameToQName(err1.getClass().getName()))
                        .withLocation(getLocation());
            } catch (IllegalArgumentException err2) {
                throw new XPathException("Argument is of wrong type", err2)
                        .withErrorCode(JavaExternalObjectType.classNameToQName(err2.getClass().getName()))
                        .withLocation(getLocation());
            } catch (NullPointerException err2) {
                err2.printStackTrace();
                throw new XPathException("Object is null", err2)
                        .withErrorCode(JavaExternalObjectType.classNameToQName(err2.getClass().getName()))
                        .withLocation(getLocation());
            } catch (InvocationTargetException err3) {
                Throwable ex = err3.getTargetException();
                if (ex instanceof XPathException) {
                    throw ((XPathException) ex).maybeWithLocation(getLocation());
                } else {
                    if (context.getController().isTracing() ||
                            context.getConfiguration().getBooleanProperty(Feature.TRACE_EXTERNAL_FUNCTIONS)) {
                        err3.getTargetException().printStackTrace();
                    }
                    XPathException xe = new XPathException(
                            "Exception thrown by extension function {" + theMethod + "}: " +
                                    err3.getTargetException(), ex);
                    xe.setErrorCodeQName(JavaExternalObjectType.classNameToQName(ex.getClass().getName()));
                    Object value = isStatic ? theClass : theInstance;
                    xe.setErrorObject(new ObjectValue<>(value));
                    throw xe.withLocation(getLocation());
                }
            }
        } else if (theMethod instanceof Field) {

            Field field = (Field) theMethod;
            boolean isStatic = Modifier.isStatic(field.getModifiers());
            Object theInstance;
            if (isStatic) {
                theInstance = null;
            } else {
                if (argValues.length == 0) {
                    throw new XPathException("Must supply an argument for a non-static extension function");
                }
                try {
                    theInstance = getTargetInstance(argValues[0], context);
                    // this fails if the first argument is not of a suitable class
                } catch (XPathException err) {
                    throw err.maybeWithLocation(getLocation());
                }
            }

            try {
                Object result = getField(field, theInstance);
                return asSequence(result, context);

            } catch (IllegalAccessException err1) {
                throw new XPathException("Field access is illegal", err1);
            } catch (IllegalArgumentException err2) {
                throw new XPathException("Argument is of wrong type", err2);
            }
        } else {
            throw new AssertionError("property " + theMethod + " is neither constructor, method, nor field");
        }

    }

    private Object getTargetInstance(Sequence arg0, XPathContext context) throws XPathException {
        PJConverter converter = argumentConverters[0];
        return converter.convert(arg0, theClass, context);
    }

    /**
     * Convert the extension function result to an XPath value (a sequence)
     *
     * @param result  the result returned by the Java extension function
     * @param context the dynamic context
     * @return the result as a Sequence
     * @throws net.sf.saxon.trans.XPathException if the result cannot be converted
     *
     */

    private Sequence asSequence(Object result, XPathContext context) throws XPathException {
        if (result == null) {
            return EmptySequence.getInstance();
        }
        SequenceIterator resultIterator;
        if (result instanceof SequenceIterator) {
            resultIterator = (SequenceIterator) result;
        } else {
            Sequence resultSeq = resultConverter.convert(result, context);
            if (!checkForNodes) {
                return resultSeq;
            }
            resultIterator = resultSeq.iterate();
        }
        if (checkForNodes) {
            return SequenceTool.toLazySequence(new ItemMappingIterator(resultIterator,
                                                                       new IntegratedFunctionCall.ConfigurationCheckingFunction(context.getConfiguration()), true));
        } else {
            return SequenceTool.toLazySequence(resultIterator);
        }
    }

    /**
     * Set up parameters for the Java method call
     *
     * @param argValues  the supplied XPath argument values
     * @param params     the result of converting the XPath argument values to Java objects
     * @param paramTypes the Java classes defining the types of the arguments in the method signature
     * @param firstParam normally 0, but 1 if the first parameter to the Java method is an XPathContext object
     * @param firstArg   normally 0, but 1 if the first argument in the XPath call is the instance object whose method
     *                   is to be called
     * @param context    The dynamic context, giving access to a NamePool and to schema information
     * @throws net.sf.saxon.trans.XPathException if an error occurs
     *
     */

    private void setupParams(Sequence[] argValues,
                             Object[] params,
                             Class<?>[] paramTypes,
                             int firstParam,
                             int firstArg,
                             XPathContext context) throws XPathException {
        int j = firstParam;

        try {
            for (int i = firstArg; i < argValues.length; i++) {
                params[j] = argumentConverters[i].convert(argValues[i], paramTypes[j], context);
                j++;
            }
        } catch (XPathException e) {
            throw e.withErrorCode("XPTY0004").maybeWithLocation(getLocation());
        }
    }

    /**
     * Determine the data type of the expression, if possible. All expressions return
     * sequences, in general; this method determines the type of the items within the
     * sequence, assuming that (a) this is known in advance, and (b) it is the same for
     * all items in the sequence.
     * <p>This method will always return a result, though it may be the best approximation
     * that is available at the time.</p>
     *
     * @return the item type
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        if (resultConverter == null) {
            return AnyItemType.getInstance();
        } else if (resultConverter instanceof JPConverter.VoidConverter) {
            if (returnVoidAsThis) {
                synchronized(config) {
                    return JavaExternalObjectType.of(theClass);
                }
            } else {
                // The result will be an empty sequence; but we avoid revealing this, to avoid being optimized away
                return AnyItemType.getInstance();
            }
        } else {
            return resultConverter.getItemType();
        }
    }

    @Override
    protected int computeCardinality() {
        Class<?> resultClass = getReturnClass();
        if (resultClass.equals(Void.TYPE)) {
            // this always returns an empty sequence, but we'll model it as
            // zero or one
            return StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
        if (resultConverter == null) {
            return StaticProperty.ALLOWS_ZERO_OR_MORE;
        } else {
            return resultConverter.getCardinality() | StaticProperty.ALLOWS_ZERO;
        }
    }

    /**
     * Get the Java class of the value returned by the method
     *
     * @return the Java class of the value returned by the method
     */

    public Class<?> getReturnClass() {
        if (theMethod instanceof Method) {
            Class<?> returnClass = ((Method) theMethod).getReturnType();
            if (returnClass == Void.TYPE && returnVoidAsThis) {
                return theClass;
            } else {
                return returnClass;
            }
        } else if (theMethod instanceof Field) {
            return ((Field) theMethod).getType();
        } else if (theMethod instanceof Constructor) {
            return theClass;
        } else {
            // cannot happen
            return null;
        }
    }

    /**
     * Get the converters used to convert the arguments from XPath values to Java values
     *
     * @return an array of converters, one per argument
     */

    public PJConverter[] getArgumentConverters() {
        return argumentConverters;
    }

    /**
     * Get the converter used to convert the result from a Java object to an XPath value
     *
     * @return the converter that is used
     */

    public JPConverter getResultConverter() {
        return resultConverter;
    }

    /**
     * Invoke a constructor. This method is provided separately so that it can be refined in a subclass.
     * For example, a subclass might perform tracing of calls, or might trap exceptions.
     *
     * @param constructor The constructor to be invoked
     * @param params      The parameters to be passed to the constructor
     * @return The object returned by the constructor
     * @throws InstantiationException    if the invocation throws an InstantiationException
     * @throws IllegalAccessException    if the invocation throws an IllegalAccessException
     * @throws InvocationTargetException if the invocation throws an InvocationTargetException (which happens
     *                                   when the constructor itself throws an exception)
     */

    protected Object invokeConstructor(Constructor<?> constructor, Object[] params)
            throws java.lang.InstantiationException,
            java.lang.IllegalAccessException,
            java.lang.reflect.InvocationTargetException {
        return constructor.newInstance(params);
    }

    /**
     * Invoke a method. This method is provided separately so that it can be refined in a subclass.
     * For example, a subclass might perform tracing of calls, or might trap exceptions.
     *
     * @param method   The method to be invoked
     * @param instance The object on which the method is to be invoked. This is set to null if the
     *                 method is static.
     * @param params   The parameters to be passed to the method
     * @return The object returned by the method
     * @throws IllegalAccessException    if the invocation throws an IllegalAccessException
     * @throws InvocationTargetException if the invocation throws an InvocationTargetException (which happens
     *                                   when the method itself throws an exception)
     */

    protected Object invokeMethod(Method method, Object instance, Object[] params)
            throws java.lang.IllegalAccessException,
            java.lang.reflect.InvocationTargetException {
        return method.invoke(instance, params);
    }

    /**
     * Access a field. This method is provided separately so that it can be refined in a subclass.
     * For example, a subclass might perform tracing of calls, or might trap exceptions.
     *
     * @param field    The field to be retrieved
     * @param instance The object whose field is to be retrieved. This is set to null if the
     *                 field is static.
     * @return The value of the field
     * @throws IllegalAccessException if the invocation throws an IllegalAccessException
     */

    protected Object getField(Field field, Object instance)
            throws java.lang.IllegalAccessException {
        return field.get(instance);
    }

    /**
     * Utility method available for use in called extension functions to check that
     * a supplied argument conforms to a required type, and perform conversion
     * using the function conversion rules if not. The conversion rules applied
     * are those of the XPath 3.1 specification.
     * @param suppliedValue the value supplied by the caller
     * @param requiredType the type required for the argument
     * @param functionName the name of the function (for diagnostics)
     * @param argPosition the ordinal position of the argument being checked in the XPath function call,
     * starting at 0 for the first argument
     * @param context the XPath dynamic context
     * @return the result of applying the function conversion rules (with XPath 1.0 compatibility mode
     * set to false)
     * @throws XPathException if type checking fails
     */

    public static Sequence applyFunctionConversionRules(
            Sequence suppliedValue, SequenceType requiredType,
            StructuredQName functionName, int argPosition, XPathContext context)
    throws XPathException {
        Supplier<RoleDiagnostic> role =
                () -> new RoleDiagnostic(RoleDiagnostic.FUNCTION, functionName.getDisplayName(), argPosition);
        return CoercionRules.forVersion(context.getConfiguration(), 31).coerce(
                suppliedValue.materialize(), requiredType, SequenceType.ANY_SEQUENCE, context.getConfiguration(), role, Loc.NONE);
    }

    /**
     * Convert a name to camelCase (by removing hyphens and changing the following
     * letter to capitals)
     *
     * @param name  the name to be converted to camelCase
     * @param debug true if tracing is required
     * @param diag  the output stream for diagnostic trace output
     * @return the camelCased name
     */

    public static String toCamelCase(String name, boolean debug, Logger diag) {
        if (name.indexOf('-') >= 0) {
            StringBuilder buff = new StringBuilder(name.length());
            boolean afterHyphen = false;
            for (int n = 0; n < name.length(); n++) {
                char c = name.charAt(n);
                if (c == '-') {
                    afterHyphen = true;
                } else {
                    if (afterHyphen) {
                        buff.append(Character.toUpperCase(c));
                    } else {
                        buff.append(c);
                    }
                    afterHyphen = false;
                }
            }
            name = buff.toString();
            if (debug) {
                diag.info("Seeking a method with adjusted name " + name);
            }
        }
        return name;
    }

    public boolean isReturnVoidAsThis() {
        return returnVoidAsThis;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     *
     * @param out the destination of the output
     */
    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("javaCall", this);
        out.emitAttribute("name", getFunctionName());
        if (declaredType != null) {
            out.emitAttribute("declType", declaredType.toExportString());
        }
        int i = 0;
        for (Operand o : operands()) {
            Expression child = o.getChildExpression();
            SequenceType st = SequenceType.makeSequenceType(child.getItemType(), child.getCardinality());
            out.emitAttribute("arg" + i++ + "type", st.toAlphaCode());
        }
        for (Operand o : operands()) {
            o.getChildExpression().export(out);
        }
        out.endElement();
    }

    /**
     * Allocate evaluation functions for each argument (in field {@code argumentEvaluators})
     */

    protected void allocateArgumentEvaluators() {
        int arity = getArity();
        argumentEvaluators = new SequenceEvaluator[arity];
        for (int i = 0; i < arity; i++) {
            argumentEvaluators[i] = getArg(i).makeElaborator().eagerly();
        }
    }

    public Sequence[] evaluateArguments(XPathContext c) throws XPathException {
        int numArgs = getArity();
        Sequence[] actualArgs = SequenceTool.makeSequenceArray(numArgs);
        synchronized(this) {
            if (argumentEvaluators == null) {
                allocateArgumentEvaluators();
            }
        }
        for (int i = 0; i < numArgs; i++) {
            actualArgs[i] = argumentEvaluators[i].evaluate(c);
        }
        return actualArgs;
    }


    /**
     * Make an elaborator for this expression
     *
     * @return an appropriate {@link Elaborator}
     */
    @Override
    public Elaborator getElaborator() {
        return new JavaExtensionFunctionCallElaborator();
    }

    private static class JavaExtensionFunctionCallElaborator extends PullElaborator {


        @Override
        public PullEvaluator elaborateForPull() {
            JavaExtensionFunctionCall expr = (JavaExtensionFunctionCall)getExpression();
            expr.allocateArgumentEvaluators();
            return context -> {
                Sequence[] actualArgs = expr.evaluateArguments(context);
                return expr.call(context, actualArgs).iterate();
            };
        }

        @Override
        public ItemEvaluator elaborateForItem() {
            JavaExtensionFunctionCall expr = (JavaExtensionFunctionCall) getExpression();
            expr.allocateArgumentEvaluators();
            return context -> {
                Sequence[] actualArgs = expr.evaluateArguments(context);
                return expr.call(context, actualArgs).head();
            };
        }
    }
}

// Copyright (c) 2009-2023 Saxonica Limited