package com.saxonica.config;

import com.saxonica.expr.JavaExtensionFunctionCall;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.CollectionFn;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.*;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;

import net.sf.saxon.type.Type;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.lang.reflect.*;
import java.math.BigDecimal;
import java.math.BigInteger;
import java.net.URI;
import java.net.URL;
import java.time.*;
import java.util.*;

/**
 * The JavaExtensionLibrary is a FunctionLibrary that binds XPath function calls to
 * calls on Java methods (or constructors, or fields). It performs a mapping from
 * the namespace URI of the function to the Java class (the mapping is partly table
 * driven and partly algorithmic), and maps the local name of the function to the
 * Java method, constructor, or field within the class. If the Java methods are
 * polymorphic, then it tries to select the appropriate method based on the static types
 * of the supplied arguments. Binding is done entirely at XPath compilation time.
 */

public class JavaExtensionLibrary implements FunctionLibrary {

    private final ProfessionalConfiguration config;

    private JavaExtensionFunctionFactory functionFactory;

    // HashMap containing URI->Class mappings. This includes conventional
    // URIs such as the Saxon and EXSLT namespaces, and mappings defined by
    // the user using saxon:script

    private HashMap<String, Class<?>> explicitMappings = new HashMap<>(10);

    // flag to indicate that only the "java:" URI format is recognized

    private boolean strictUriFormat = true;
    //private Boolean debug = null;

    /**
     * Construct a JavaExtensionLibrary and establish the default uri-&gt;class mappings.
     *
     * @param config The Saxon configuration
     */

    public JavaExtensionLibrary(ProfessionalConfiguration config) {
        this.config = config;
        functionFactory = new JavaExtensionFunctionFactory(config);
        //debug = config.getBooleanProperty(Feature.TRACE_EXTERNAL_FUNCTIONS);
    }

    /**
     * Set the ExtensionFunctionFactory to be used by this extension library.
     *
     * @param factory This must be an instanceof JavaExtensionFunctionFactory
     */

    public void setExtensionFunctionFactory(JavaExtensionFunctionFactory factory) {
        functionFactory = factory;
    }

    /**
     * Get the ExtensionFunctionFactory used by this extension library.
     *
     * @return the factory, which will always be an instanceof JavaExtensionFunctionFactory
     */


    public JavaExtensionFunctionFactory getExtensionFunctionFactory() {
        return functionFactory;
    }

    /**
     * Ask whether only the strict "java:" URI format is to be recognized. The default is "true":
     * the value false can be set for backwards compatibility with releases before Saxon 9.2
     *
     * @return true if only the strict format is recognized
     */

    public boolean isStrictJavaUriFormat() {
        return strictUriFormat;
    }

    /**
     * Say that only the strict "java:" URI format is to be recognized. The default is "true":
     * the value false can be set for backwards compatibility with releases before Saxon 9.2
     *
     * @param strict true if only the strict format is recognized
     */

    public void setStrictJavaUriFormat(boolean strict) {
        strictUriFormat = strict;
    }

    /**
     * Declare a mapping from a specific namespace URI to a Java class
     *
     * @param uri      the namespace URI of the function name
     * @param theClass the Java class that implements the functions in this namespace
     */

    public synchronized void declareJavaClass(String uri, Class theClass) {
        explicitMappings.put(uri, theClass);
    }


    /**
     * Test whether a function with a given name and arity is available; if so, return a function
     * item that can be dynamically called.
     * <p>This supports the function-lookup() function in XPath 3.0.</p>
     *
     * @param functionName  the qualified name of the function being called
     * @param staticContext the static context to be used by the function, in the event that
     *                      it is a system function with dependencies on the static context
     * @return if a function of this name and arity is available for calling, then a corresponding
     * function item; or null if the function does not exist
     * @throws net.sf.saxon.trans.XPathException in the event of certain errors, for example attempting to get a function
     *                                           that is private
     */
    @Override
    public Function getFunctionItem(SymbolicName.F functionName, StaticContext staticContext) throws XPathException {
        Configuration config = staticContext.getConfiguration();
        int arity = functionName.getArity();
        SuppliedParameterReference[] params = new SuppliedParameterReference[arity];
        Arrays.fill(params, new SuppliedParameterReference(-1));
        Expression call = bind(functionName, params, staticContext, new ArrayList<>());
        if (call instanceof JavaExtensionFunctionCall) {
            SequenceType[] argTypes = new SequenceType[arity];
            Arrays.fill(argTypes, SequenceType.ANY_SEQUENCE);
            SpecificFunctionType ftype =
                    new SpecificFunctionType(argTypes, SequenceType.ANY_SEQUENCE);
            ((JavaExtensionFunctionCall) call).prepareForDynamicCall(config);
            return new CallableFunction(functionName, (Callable) call, ftype);
        } else if (call instanceof UnresolvedExtensionFunctionCall) {
            SequenceType[] argTypes = new SequenceType[arity];
            Arrays.fill(argTypes, SequenceType.ANY_SEQUENCE);
            net.sf.saxon.type.SpecificFunctionType ftype =
                    new net.sf.saxon.type.SpecificFunctionType(argTypes, SequenceType.ANY_SEQUENCE);
            return new CallableFunction(functionName, (Callable) call, ftype);
        }
        return null;
    }

    /**
     * Test whether a function with a given name and arity is available
     * <p>This supports the function-available() function in XSLT.</p>
     *
     * @param functionName the qualified name of the function being called
     * @param languageLevel the XPath language level times 10 (31=XPath 3.1)
     * @return true if a function of this name and arity is available for calling
     */
    @Override
    public boolean isAvailable(SymbolicName.F functionName, int languageLevel) {
        SuppliedParameterReference[] params = new SuppliedParameterReference[functionName.getArity()];
        Arrays.fill(params, new SuppliedParameterReference(-1));
        StaticContext staticContext = new IndependentContext(config);
        Expression call = bind(functionName, params, staticContext, new ArrayList<>());
        return call != null;
    }

    /**
     * Bind an extension function, given the URI and local parts of the function name,
     * and the list of expressions supplied as arguments. This method is called at compile
     * time.
     *
     * @param symbolicName the qualified name and arity of the function being called
     * @param staticArgs   The expressions supplied statically in the function call. The intention is
     *                     that the static type of the arguments (obtainable via getItemType() and getCardinality()) may
     *                     be used as part of the binding algorithm.
     * @param env          the static context
     * @param reasons      If no matching function is found by the function library, it may add
     *                     a diagnostic explanation to this list explaining why none of the available
     *                     functions could be used.
     * @return An object representing the extension function to be called, if one is found;
     * null if no extension function was found matching the required name, arity, or signature.
     */

    @Override
    public Expression bind(SymbolicName.F symbolicName, Expression[] staticArgs, StaticContext env, List<String> reasons) {
        boolean debug = config.getBooleanProperty(Feature.TRACE_EXTERNAL_FUNCTIONS);
        StructuredQName functionName = symbolicName.getComponentName();
        String uri = functionName.getURI();
        String path = uri;
        int q = uri.indexOf('?');
        if (q > 0) {
            path = uri.substring(0, q);
        }
        Class reqClass = getExternalJavaClass(path, functionName.getLocalPart(), debug);
        if (reqClass == null) {
            if (uri.startsWith("java:")) {
                reasons.add("Cannot load Java class " + path);
            }
            return null;
        }

        Logger diag = config.getLogger();

        if (debug) {
            diag.info("Looking for method " + functionName.getLocalPart() + " in namespace " + functionName.getURI());
            diag.info("Number of actual arguments = " + staticArgs.length);
        }

        if (!config.getBooleanProperty(Feature.ALLOW_EXTERNAL_FUNCTIONS)) {
            if (debug) {
                diag.info("Calls to extension functions have been disabled");
            }
            reasons.add("Calls to extension functions have been disabled");
            return null;
        }

        if (!config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            if (debug) {
                diag.info("Calls to extension functions are not permitted without a Saxon license");
            }
            reasons.add("Calls to extension functions are not permitted without a Saxon license");
            return null;
        }

        //Exception theException = null;
        String problem = null;
        ArrayList<AccessibleObject> candidateMethods = new ArrayList<>(10);
        Class resultClass = null;

        if (debug) {
            diag.info("Looking in Java class " + reqClass);
        }

        int numArgs = staticArgs.length;
        int significantArgs;

        String methodName = getMethodNameFromLocalName(functionName.getURI(), functionName.getLocalPart(), debug);

        // if the method name is "new", look for a matching constructor

        if ("new".equals(methodName)) {

            if (debug) {
                diag.info("Looking for a constructor");
            }

            int mod = reqClass.getModifiers();
            if (Modifier.isAbstract(mod)) {
                problem = "Class " + reqClass + " is abstract";
            } else if (Modifier.isInterface(mod)) {
                problem = reqClass + " is an interface";
            } else if (Modifier.isPrivate(mod)) {
                problem = "Class " + reqClass + " is private";
            } else if (Modifier.isProtected(mod)) {
                problem = "Class " + reqClass + " is protected";
            }

            if (problem != null) {
                if (debug) {
                    diag.info("Cannot construct an instance: " + problem);
                }
                reasons.add(problem);
                return null;
            }

            Constructor[] constructors = reqClass.getConstructors();
            for (Constructor theConstructor : constructors) {
                if (debug) {
                    diag.info("Found a constructor with " + theConstructor.getParameterTypes().length + " arguments");
                }
                if (theConstructor.getParameterTypes().length == numArgs) {
                    candidateMethods.add(theConstructor);
                }
            }
            if (candidateMethods.isEmpty()) {
                problem = "No constructor with " + numArgs +
                        (numArgs == 1 ? " parameter" : " parameters") +
                        " found in class " + reqClass.getName();
                if (debug) {
                    diag.info(problem);
                }
                reasons.add(problem);
                return null;
            }
        } else {

            // look through the methods of this class to find one that matches the local name

            Method[] methods = reqClass.getMethods();
            boolean consistentReturnType = true;
            for (Method theMethod : methods) {

                if (debug) {
                    if (theMethod.getName().equals(methodName)) {
                        diag.info("Trying method " + theMethod.getName() + ": name matches");
                        if (!Modifier.isPublic(theMethod.getModifiers())) {
                            diag.info(" -- but the method is not public");
                        } else if (theMethod.isSynthetic()) {
                            diag.info(" -- but the method is synthetic");
                        }
                    } else {
                        diag.info("Trying method " + theMethod.getName() + ": name does not match");
                    }
                }

                if (theMethod.getName().equals(methodName) &&
                        Modifier.isPublic(theMethod.getModifiers()) &&
                        !theMethod.isSynthetic()) {

                    if (consistentReturnType) {
                        if (resultClass == null) {
                            resultClass = theMethod.getReturnType();
                        } else {
                            consistentReturnType =
                                    theMethod.getReturnType() == resultClass;
                        }
                    }
                    Class[] theParameterTypes = theMethod.getParameterTypes();
                    boolean isStatic = Modifier.isStatic(theMethod.getModifiers());

                    // if the method is not static, the first supplied argument is the instance, so
                    // discount it

                    if (debug) {
                        diag.info("Method is " + (isStatic ? "" : "not ") + "static");
                    }

                    significantArgs = isStatic ? numArgs : numArgs - 1;

                    if (significantArgs >= 0) {

                        if (debug) {
                            if (isStatic) {
                                diag.info("Method has " + theParameterTypes.length + " argument" +
                                                  (theParameterTypes.length == 1 ? "" : "s") +
                                                  "; expecting " + significantArgs);
                            } else {
                                diag.info("Method has " + theParameterTypes.length + " argument" +
                                                  (theParameterTypes.length == 1 ? "" : "s") +
                                                  "; expecting " + numArgs + " plus one for the target object");
                            }
                        }

                        if (theParameterTypes.length == significantArgs &&
                                (significantArgs == 0 || theParameterTypes[0] != XPathContext.class)) {
                            if (debug) {
                                diag.info("Found a candidate method:");
                                diag.info("    " + theMethod);
                            }
                            candidateMethods.add(theMethod);
                        }

                        // we allow the method to have an extra parameter if the first parameter is XPathContext

                        if (theParameterTypes.length == significantArgs + 1 &&
                                theParameterTypes[0] == XPathContext.class) {
                            if (debug) {
                                diag.info("Method is a candidate because first argument is XPathContext");
                            }
                            candidateMethods.add(theMethod);
                        }
                    }
                }
            }

            // look through the fields of this class to find those that matches the local name

            Field[] fields = reqClass.getFields();
            for (Field theField : fields) {

                if (debug) {
                    if (theField.getName().equals(methodName)) {
                        diag.info("Trying field " + theField.getName() + ": name matches");
                        if (!Modifier.isPublic(theField.getModifiers())) {
                            diag.info(" -- but the field is not public");
                        }
                    } else {
                        diag.info("Trying field " + theField.getName() + ": name does not match");
                    }
                }

                if (theField.getName().equals(methodName) &&
                        Modifier.isPublic(theField.getModifiers())) {
                    if (consistentReturnType) {
                        if (resultClass == null) {
                            resultClass = theField.getType();
                        } else {
                            consistentReturnType =
                                    theField.getType() == resultClass;
                        }
                    }
                    boolean isStatic = Modifier.isStatic(theField.getModifiers());

                    // if the field is not static, the first supplied argument is the instance, so
                    // discount it

                    if (debug) {
                        diag.info("Field is " + (isStatic ? "" : "not ") + "static");
                    }

                    significantArgs = isStatic ? numArgs : numArgs - 1;

                    if (significantArgs == 0) {
                        if (debug) {
                            diag.info("Found a candidate field:");
                            diag.info("    " + theField);
                        }
                        candidateMethods.add(theField);
                    }
                }
            }

            // No method found?

            if (candidateMethods.isEmpty()) {
                problem = "No method or field matching " + methodName +
                        " with " + numArgs +
                        (numArgs == 1 ? " parameter" : " parameters") +
                        " found in class " + reqClass.getName();
                if (debug) {
                    diag.info(problem);
                }
                reasons.add(problem);
                return null;
            }
        }

        if (candidateMethods.size() > 1) {
            // There was more than one candidate method, and we can't decide which to use.
            // This may be because insufficient type information is available at this stage.
            // Return an UnresolvedExtensionFunction, and try to resolve it later when more
            // type information is known.
            if (debug) {
                diag.info("Postponing decision until after type-checking");
            }
            return new UnresolvedExtensionFunctionCall(
                    functionName, reqClass, candidateMethods, staticArgs, config, debug);
        } else {
            return functionFactory.makeExtensionFunctionCall(
                    functionName, reqClass, candidateMethods.get(0), staticArgs, config);
        }
    }


    /**
     * Get the best fit amongst all the candidate methods, constructors, or fields, based on the static types
     * of the supplied arguments
     *
     * @param candidateMethods a list of all the methods, fields, and constructors that match the extension
     *                         function call in name and arity (but not necessarily in the types of the arguments)
     * @param argTypes         the types of the expressions supplied as arguments.
     * @param theClass         the class that implements the extension function
     * @return the result is either a Method or a Constructor or a Field, or null if no unique best fit
     * method could be found.
     */

    private AccessibleObject getBestFit(List<AccessibleObject> candidateMethods, SequenceType[] argTypes, Class theClass, boolean debug) {
        Logger diag = config.getLogger();
        int candidates = candidateMethods.size();

        if (candidates == 1) {
            // short cut: there is only one candidate method
            return candidateMethods.get(0);

        } else {
            // choose the best fit method or constructor or field
            // for each pair of candidate methods, eliminate either or both of the pair
            // if one argument is less-preferred

            if (debug) {
                diag.info("Finding best fit method for arguments");
            }

            boolean[] eliminated = new boolean[candidates];
            Arrays.fill(eliminated, false);

            if (debug) {
                for (int i = 0; i < candidates; i++) {
                    int[] pref_i = getConversionPreferences(
                            argTypes, candidateMethods.get(i), theClass);
                    diag.info("Trying option " + (i + 1) + ": " + candidateMethods.get(i));
                    if (pref_i == null) {
                        diag.info("Arguments cannot be converted to required types");
                    } else {
                        StringBuilder fsb = new StringBuilder(100);
                        fsb.append('[');
                        for (int p = 0; p < pref_i.length; p++) {
                            if (p != 0) {
                                fsb.append(", ");
                            }
                            fsb.append(Integer.toString(pref_i[p]));
                        }
                        fsb.append("]");
                        diag.info("Conversion preferences are " + fsb.toString());
                    }
                }
            }

            for (int i = 0; i < candidates; i++) {
                int[] pref_i = getConversionPreferences(
                        argTypes,
                        candidateMethods.get(i), theClass);

                if (pref_i == null) {
                    eliminated[i] = true;
                }
                if (!eliminated[i]) {
                    for (int j = i + 1; j < candidates; j++) {
                        if (!eliminated[j]) {
                            int[] pref_j = getConversionPreferences(
                                    argTypes,
                                    candidateMethods.get(j), theClass);
                            if (pref_j == null) {
                                eliminated[j] = true;
                            } else {
                                for (int k = 0; k < pref_j.length; k++) {
                                    if (pref_i[k] > pref_j[k] && !eliminated[i]) { // high number means less preferred
                                        eliminated[i] = true;
                                        if (debug) {
                                            diag.info("Eliminating option " + i);
                                        }
                                    }
                                    if (pref_i[k] < pref_j[k] && !eliminated[j]) {
                                        eliminated[j] = true;
                                        if (debug) {
                                            diag.info("Eliminating option " + j);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            int remaining = 0;
            AccessibleObject theMethod = null;
            for (int r = 0; r < candidates; r++) {
                if (!eliminated[r]) {
                    theMethod = candidateMethods.get(r);
                    remaining++;
                }
            }

            if (debug) {
                diag.info("Number of candidate methods remaining: " + remaining);
            }

            if (remaining == 0) {
                if (debug) {
                    diag.info("There are " + candidates +
                                      " candidate Java methods matching the function name, but none is a unique best match");
                }
                return null;
            }

            if (remaining > 1) {
                if (debug) {
                    diag.info("There are several Java methods that match the function name equally well");
                }
                // If only one of them is a static method, choose that one in preference
                int countStatic = 0;
                for (int r = 0; r < candidates; r++) {
                    if (!eliminated[r] &&
                            candidateMethods.get(r) instanceof Method &&
                            Modifier.isStatic(((Method) candidateMethods.get(r)).getModifiers())) {
                        theMethod = candidateMethods.get(r);
                        countStatic++;
                    }
                }
                if (countStatic == 1) {
                    if (debug) {
                        diag.info("Choosing the static method in preference");
                    }
                } else {
                    return null;
                }
            }

            return theMethod;
        }
    }

    /**
     * Get an array of integers representing the conversion distances of each "real" argument
     * to a given method
     *
     * @param argTypes the types of the expressions supplied in the function call
     * @param method   the method or constructor.
     * @param theClass the class that implements the extension function
     * @return an array of integers, one for each argument, indicating the conversion
     * distances. A high number indicates low preference. If any of the arguments cannot
     * be converted to the corresponding type defined in the method signature, return null.
     */

    private int[] getConversionPreferences(SequenceType[] argTypes, AccessibleObject method, Class theClass) {

        Class[] params;
        int firstArg;
        if (method instanceof Constructor) {
            firstArg = 0;
            params = ((Constructor) method).getParameterTypes();
        } else if (method instanceof Method) {
            boolean isStatic = Modifier.isStatic(((Method) method).getModifiers());
            firstArg = isStatic ? 0 : 1;
            params = ((Method) method).getParameterTypes();
        } else if (method instanceof Field) {
            boolean isStatic = Modifier.isStatic(((Field) method).getModifiers());
            firstArg = isStatic ? 0 : 1;
            params = NO_PARAMS;
        } else {
            throw new AssertionError("property " + method + " was neither constructor, method, nor field");
        }

        int noOfArgs = argTypes.length;
        int[] preferences = new int[noOfArgs];
        int firstParam = 0;

        if (params.length > 0 && params[0] == XPathContext.class) {
            firstParam = 1;
        }
        for (int i = firstArg; i < noOfArgs; i++) {
            preferences[i] = getConversionPreference(argTypes[i], params[i + firstParam - firstArg]);
            if (preferences[i] == -1) {
                return null;
            }
        }

        if (firstArg == 1) {
            preferences[0] = getConversionPreference(argTypes[0], theClass);
            if (preferences[0] == -1) {
                return null;
            }
        }

        return preferences;
    }

    /**
     * Get the conversion preference from a given XPath type to a given Java class
     *
     * @param argType  the static type of the supplied XPath expression
     * @param required the Java class of the relevant argument of the Java method
     * @return the conversion preference. A high number indicates a low preference;
     * -1 indicates that conversion is not possible.
     */

    private int getConversionPreference(SequenceType argType, Class<?> required) {
        ItemType itemType = argType.getPrimaryType();
        int cardinality = argType.getCardinality();
        if (required == Object.class) {
            return 100;
        } else if (required.isAssignableFrom(SequenceIterator.class)) {
            return 26;
        } else if (required.isAssignableFrom(Sequence.class)) {
            return 25;
        } else if (required.isAssignableFrom(GroundedValue.class)) {
            return 24;
        } else if (required.isAssignableFrom(Item.class)) {
            return 23;
        } else if (required.isAssignableFrom(NodeInfo.class)) {
            return 22;
        } else if (required.isAssignableFrom(TreeInfo.class)) {
            return 21;
        } else if (required.isAssignableFrom(AtomicValue.class)) {
            return 20;
        } else if (Cardinality.allowsMany(cardinality)) {
            if (CollectionFn.class.isAssignableFrom(required)) {  // TODO: surely nonsense! MHK 2021-05-29
                return 30;
            } else if (required.isArray()) {
                Class arrayMember = required.getComponentType();
                return atomicConversionPreference(itemType.getPrimitiveType(), arrayMember);
                //return 31;
                // sort out at run-time whether the component type of the array is actually suitable
            } else {
                return 80;    // conversion possible only if external object model supports it
            }
        } else {

            if (Type.isNodeType(itemType)) {
                return 80;
            } else if (itemType instanceof JavaExternalObjectType) {
                Class ext = ((JavaExternalObjectType) itemType).getJavaClass();
                if (required.isAssignableFrom(ext)) {
                    return 10;
                } else {
                    return -1;
                }
            } else {
                int primitiveType = itemType.getPrimitiveType();
                return atomicConversionPreference(primitiveType, required);
            }
        }
    }

    private static final Class[] NO_PARAMS = new Class[0];


    /**
     * Get the conversion preference from an XPath primitive atomic type to a Java class
     *
     * @param primitiveType integer code identifying the XPath primitive type, for example
     *                      {@link StandardNames#XS_STRING} or {@link StandardNames#XS_STRING}
     * @param required      The Java Class named in the method signature
     * @return an integer indicating the relative preference for converting this primitive type
     * to this Java class. A high number indicates a low preference. All values are in the range
     * 50 to 100. For example, the conversion of an XPath String to {@link StringValue} is 50, while
     * XPath String to {@link java.lang.String} is 51. The value -1 indicates that the conversion is not allowed.
     */

    protected int atomicConversionPreference(int primitiveType, Class<?> required) {
        if (required == Object.class) {
            return 100;
        }
        switch (primitiveType) {
            case StandardNames.XS_STRING:
                if (required.isAssignableFrom(StringValue.class)) {
                    return 50;
                }
                if (required == String.class) {
                    return 51;
                }
                if (required == CharSequence.class) {
                    return 52;
                }
                return -1;
            case StandardNames.XS_DOUBLE:
                if (required.isAssignableFrom(DoubleValue.class)) {
                    return 50;
                }
                if (required == double.class) {
                    return 50;
                }
                if (required == Double.class) {
                    return 51;
                }
                return -1;
            case StandardNames.XS_FLOAT:
                if (required.isAssignableFrom(FloatValue.class)) {
                    return 50;
                }
                if (required == float.class) {
                    return 50;
                }
                if (required == Float.class) {
                    return 51;
                }
                if (required == double.class) {
                    return 52;
                }
                if (required == Double.class) {
                    return 53;
                }
                return -1;
            case StandardNames.XS_DECIMAL:
                if (required.isAssignableFrom(BigDecimalValue.class)) {
                    return 50;
                }
                if (required == BigDecimal.class) {
                    return 50;
                }
                if (required == double.class) {
                    return 51;
                }
                if (required == Double.class) {
                    return 52;
                }
                if (required == float.class) {
                    return 53;
                }
                if (required == Float.class) {
                    return 54;
                }
                return -1;
            case StandardNames.XS_INTEGER:
                if (required.isAssignableFrom(Int64Value.class)) {
                    return 50;
                }
                if (required == BigInteger.class) {
                    return 51;
                }
                if (required == BigDecimal.class) {
                    return 52;
                }
                if (required == long.class) {
                    return 53;
                }
                if (required == Long.class) {
                    return 54;
                }
                if (required == int.class) {
                    return 55;
                }
                if (required == Integer.class) {
                    return 56;
                }
                if (required == short.class) {
                    return 57;
                }
                if (required == Short.class) {
                    return 58;
                }
                if (required == byte.class) {
                    return 59;
                }
                if (required == Byte.class) {
                    return 60;
                }
                if (required == double.class) {
                    return 61;
                }
                if (required == Double.class) {
                    return 62;
                }
                if (required == float.class) {
                    return 63;
                }
                if (required == Float.class) {
                    return 64;
                }
                return -1;
            case StandardNames.XS_BOOLEAN:
                if (required.isAssignableFrom(BooleanValue.class)) {
                    return 50;
                }
                if (required == boolean.class) {
                    return 51;
                }
                if (required == Boolean.class) {
                    return 52;
                }
                return -1;
            case StandardNames.XS_DATE:
            case StandardNames.XS_G_DAY:
            case StandardNames.XS_G_MONTH_DAY:
            case StandardNames.XS_G_MONTH:
            case StandardNames.XS_G_YEAR_MONTH:
            case StandardNames.XS_G_YEAR:
                if (required.isAssignableFrom(DateValue.class)) {
                    return 50;
                }
                if (required.isAssignableFrom(LocalDate.class)) {
                    return 51;
                }
                if (required.isAssignableFrom(OffsetDateTime.class)) {
                    return 52;
                }
                if (required.isAssignableFrom(LocalDateTime.class)) {
                    return 53;
                }
                if (required.isAssignableFrom(ZonedDateTime.class)) {
                    return 54;
                }
                if (required.isAssignableFrom(Instant.class)) {
                    return 55;
                }
                if (required.isAssignableFrom(Date.class)) {
                    return 56;
                }
                return -1;
            case StandardNames.XS_DATE_TIME:
                if (required.isAssignableFrom(DateTimeValue.class)) {
                    return 50;
                }
                if (required.isAssignableFrom(OffsetDateTime.class)) {
                    return 51;
                }
                if (required.isAssignableFrom(LocalDateTime.class)) {
                    return 52;
                }
                if (required.isAssignableFrom(ZonedDateTime.class)) {
                    return 53;
                }
                if (required.isAssignableFrom(Instant.class)) {
                    return 54;
                }
                if (required.isAssignableFrom(Date.class)) {
                    return 55;
                }
                return -1;
            case StandardNames.XS_TIME:
                if (required.isAssignableFrom(TimeValue.class)) {
                    return 50;
                }
                if (required.isAssignableFrom(OffsetDateTime.class)) {
                    return 51;
                }
                if (required.isAssignableFrom(ZonedDateTime.class)) {
                    return 52;
                }
                if (required.isAssignableFrom(Instant.class)) {
                    return 53;
                }
                if (required.isAssignableFrom(Date.class)) {
                    return 54;
                }
                return -1;
            case StandardNames.XS_DURATION:
            case StandardNames.XS_YEAR_MONTH_DURATION:
            case StandardNames.XS_DAY_TIME_DURATION:
                if (required.isAssignableFrom(DurationValue.class)) {
                    return 50;
                }
                return -1;
            case StandardNames.XS_ANY_URI:
                if (required.isAssignableFrom(AnyURIValue.class)) {
                    return 50;
                }
                if (required == URI.class) {
                    return 51;
                }
                if (required == URL.class) {
                    return 52;
                }
                if (required == String.class) {
                    return 53;
                }
                if (required == CharSequence.class) {
                    return 53;
                }
                return -1;
            case StandardNames.XS_QNAME:
                if (required.isAssignableFrom(QualifiedNameValue.class)) {
                    return 50;
                }
                if (required.isAssignableFrom(javax.xml.namespace.QName.class)) {
                    return 51;
                }
                // if (required.getClass().getName().equals("javax.xml.namespace.QName")) return 51;
                return -1;
            case StandardNames.XS_BASE64_BINARY:
                if (required.isAssignableFrom(Base64BinaryValue.class)) {
                    return 50;
                }
                return -1;
            case StandardNames.XS_HEX_BINARY:
                if (required.isAssignableFrom(HexBinaryValue.class)) {
                    return 50;
                }
                return -1;
            case StandardNames.XS_UNTYPED_ATOMIC:
                return 50;
            default:
                if (required.isAssignableFrom(AtomicValue.class)) {
                    return 50;
                }
                if (required == String.class) {
                    return 99;
                }
                return -1;
        }
    }

    /**
     * Get an external Java class corresponding to a given namespace URI, if there is
     * one. The method is protected so it can be overridden in a user-defined subclass.
     *
     * @param uri       The namespace URI corresponding to the prefix used in the function call.
     * @param localName The local name of the function, in case this contributes to determining the class
     * @param debug     true if tracing information should be output
     * @return the Java class name if a suitable class exists, otherwise return null.
     */

    protected Class getExternalJavaClass(String uri, String localName, boolean debug) {

        // First see if an explicit mapping has been registered for this URI

        Class c = explicitMappings.get(uri);
        if (c != null) {
            return c;
        }

        // Failing that, try to identify a class directly from the URI

        try {

            if (uri.startsWith("java:")) {

                // support the URN format java:full.class.Name (discarding any query part)
                String path = uri.substring(5);
                int q = path.indexOf('?');
                if (q > 0) {
                    path = path.substring(0, q);
                }
                c = config.getClass(path, debug);

            } else if (uri.equals(NamespaceConstant.JAVA_TYPE)) {

                // support the URI java:http://saxon.sf.net/java-type
                // with the class name being the part of the local name of the function up to the last "."

                int lastDot = localName.lastIndexOf(".");
                if (lastDot < 1) {
                    return null;
                }
                String typename = localName.substring(0, lastDot);
                String className = JavaExternalObjectType.localNameToClassName(typename);
                c = config.getClass(className, debug);

            } else if (strictUriFormat) {
                c = null;

            } else {

                // extract the class name as anything in the URI after the last "/"
                // if there is one, or the whole class name otherwise

                int slash = uri.lastIndexOf('/');
                if (slash < 0) {
                    c = config.getClass(uri, debug);
                } else if (slash == uri.length() - 1) {
                    c = null;
                } else {
                    c = config.getClass(uri.substring(slash + 1), debug);
                }
                if (c != null) {
                    declareJavaClass(uri, c);
                }
            }
            return c;
        } catch (XPathException err) {
            return null;
        }
    }

    /**
     * Get the Java method/field name from the local name used in the function call. This may be overridden
     * in a subclass. The default implementation is as follows: if the URI is NamespaceConstant.JAVA_TYPE,
     * the method name is that part of the local name appearing after the last dot/period. Otherwise,
     * it is the local-name, converted to camel-case by removing hyphens and converting any character
     * immediately following a hyphen to upper case.
     *
     * @param uri       the namespace URI used in the function call
     * @param localName the local name used in the function call
     * @param debug     true if tracing is requested
     * @return the local name of the method or field
     */

    protected String getMethodNameFromLocalName(String uri, String localName, boolean debug) {
        if (uri.equals(NamespaceConstant.JAVA_TYPE)) {
            int lastDot = localName.lastIndexOf('.');
            return localName.substring(lastDot + 1);
        } else {
            return JavaExtensionFunctionCall.toCamelCase(localName, debug, config.getLogger());
        }
    }

    /**
     * Inner class representing an unresolved extension function call. This arises when there is insufficient
     * static type information available at the time the function call is parsed to determine which of several
     * candidate Java methods to invoke. The function call cannot be executed; it must be resolved to an
     * actual Java method during the analysis phase.
     */

    public class UnresolvedExtensionFunctionCall extends FunctionCall implements Callable {

        private final StructuredQName name;
        private final transient List<AccessibleObject> candidateMethods;
        private final Class theClass;
        private final boolean debug;

        private final BuiltInFunctionSet.Entry details = new BuiltInFunctionSet.Entry();


        public UnresolvedExtensionFunctionCall(StructuredQName functionName, Class theClass,
                                               List<AccessibleObject> candidateMethods, Expression[] staticArgs,
                                               Configuration config, boolean debug) {
            details.name = functionName;
            details.arity = staticArgs.length;
            details.implementationClass = theClass;
            details.argumentTypes = new SequenceType[staticArgs.length];
            details.itemType = AnyItemType.getInstance();
            details.cardinality = StaticProperty.ALLOWS_ZERO_OR_MORE;
            details.usage = new OperandUsage[staticArgs.length];
            details.resultIfEmpty = SequenceTool.makeSequenceArray(staticArgs.length);

            //setDetails(details);
            setArguments(staticArgs);
            this.name = functionName;
            this.theClass = theClass;
            this.candidateMethods = candidateMethods;
            this.debug = debug;

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
         * Type-check the expression.
         */

        /*@NotNull*/
        @Override
        public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
            typeCheckChildren(visitor, contextInfo);
            SequenceType[] argTypes = new SequenceType[getArity()];
            for (int i = 0; i < getArity(); i++) {
                Expression arg = getArg(i);
                ItemType itemType = arg.getItemType();
                int card = arg.getCardinality();
                if (itemType == ErrorType.getInstance() && arg instanceof VariableReference) {
                    // typically happens when a variable is initialized to (). In this case
                    // try using the declared type
                    SequenceType requiredType = ((VariableReference) arg).getBinding().getRequiredType();
                    itemType = requiredType.getPrimaryType();
                    card = requiredType.getCardinality();
                }
                details.arg(i, itemType, StaticProperty.ALLOWS_ZERO_OR_MORE, null);
                argTypes[i] = SequenceType.makeSequenceType(itemType, card);
            }
            // First use the type information in the arguments as supplied
            AccessibleObject method = getBestFit(candidateMethods, argTypes, theClass, debug);

            // Now typecheck the arguments. In some cases, notably when the argument reduces to an empty
            // sequence, this can lose type information, which is why we tried to resolve the method earlier.
            typeCheckChildren(visitor, contextInfo);

            // If the method hasn't been resolved yet, try again with refined type information
            if (method == null) {
                for (int i = 0; i < getArity(); i++) {
                    Expression arg = getArg(i);
                    ItemType itemType = arg.getItemType();
                    argTypes[i] = SequenceType.makeSequenceType(itemType, arg.getCardinality());
                }
                method = getBestFit(candidateMethods, argTypes, theClass, debug);
            }
            if (method == null) {
                XPathException err = new XPathException("There is more than one method matching the function call " +
                                                                getFunctionName().getEQName() + "#" + getArity() +
                                                                ", and there is insufficient type information to determine which one should be used", "XPST0017");
                err.setLocation(getLocation());
                err.setIsStaticError(true);
                throw err;
            } else {
                Expression call = functionFactory.makeExtensionFunctionCall(getFunctionName(), theClass, method, getArguments(), config);
                ExpressionTool.copyLocationInfo(this, call);
                return call.typeCheck(visitor, contextInfo);
            }
        }

        public Expression resolve(SequenceType[] argTypes) throws XPathException {
            AccessibleObject method = getBestFit(candidateMethods, argTypes, theClass, debug);
            if (method == null) {
                XPathException err = new XPathException("There is more than one method matching the function call " +
                                                                getFunctionName().getEQName() + "#" + getArity() +
                                                                ", and there is insufficient type information to determine which one should be used", "XPST0017");
                err.setLocation(getLocation());
                err.setIsStaticError(true);
                throw err;
            } else {
                Expression call = functionFactory.makeExtensionFunctionCall(getFunctionName(), theClass, method, getArguments(), config);
                ExpressionTool.copyLocationInfo(this, call);
                return call;
            }
        }

        /**
         * Evaluate the expression
         *
         * @param context   the dynamic evaluation context
         * @param arguments the values of the arguments, supplied as Sequences
         * @return the result of the evaluation, in the form of a Sequence
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during the evaluation of the expression
         */
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Literal[] args = new Literal[arguments.length];
            TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            SequenceType[] argTypes = new SequenceType[arguments.length];
            for (int i = 0; i < arguments.length; i++) {
                GroundedValue val = arguments[i].materialize();
                ItemType itemType = SequenceTool.getItemType(val, th);
                int cardinality = SequenceTool.getCardinality(val);
                argTypes[i] = SequenceType.makeSequenceType(itemType, cardinality);
                args[i] = Literal.makeLiteral(val);
            }
            AccessibleObject method = getBestFit(candidateMethods, argTypes, theClass, false);
            if (method == null) {
                XPathException err = new XPathException("There is more than one method matching the dynamic function call " +
                                                                getFunctionName().getEQName() + "#" + getArity() +
                                                                ", and there is insufficient type information to determine which one should be used", "XPST0017");
                //err.setLocation(getLocation());
                err.setIsStaticError(true);
                throw err;
            } else {
                Expression call = functionFactory.makeExtensionFunctionCall(getFunctionName(), theClass, method, args, config);
                //ExpressionTool.copyLocationInfo(this, call);
                if (call instanceof JavaExtensionFunctionCall) {
                    ((JavaExtensionFunctionCall) call).prepareForDynamicCall(context.getConfiguration());
                }
                return SequenceTool.toGroundedValue(call.iterate(context));
            }
            //throw new XPathException("Cannot make a dynamic call to an overloaded external Java method");
        }

        /**
         * Get the target function to be called
         *
         * @param context the dynamic evaluation context
         * @return the target function
         */
        @Override
        public Function getTargetFunction(XPathContext context) {
            SequenceType[] argTypes = new SequenceType[getArity()];
            Arrays.fill(argTypes, SequenceType.ANY_SEQUENCE);
            net.sf.saxon.type.SpecificFunctionType ftype =
                    new net.sf.saxon.type.SpecificFunctionType(argTypes, SequenceType.ANY_SEQUENCE);
            SymbolicName.F sName = new SymbolicName.F(getFunctionName(), getArity());
            return new CallableFunction(sName, this, ftype);
        }

        /**
         * Determine the data type of the expression, if possible. All expression return
         * sequences, in general; this method determines the type of the items within the
         * sequence, assuming that (a) this is known in advance, and (b) it is the same for
         * all items in the sequence.
         * <p>This method should always return a result, though it may be the best approximation
         * that is available at the time.</p>
         *
         * @return a value such as Type.STRING, Type.BOOLEAN, Type.NUMBER,
         * Type.NODE, or Type.ITEM (meaning not known at compile time)
         */
        @Override
        public ItemType getItemType() {
            return AnyItemType.getInstance();
        }

        /**
         * Compute the static cardinality of this expression
         *
         * @return the computed cardinality, as one of the values {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO_OR_ONE},
         * {@link net.sf.saxon.expr.StaticProperty#EXACTLY_ONE}, {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ONE_OR_MORE},
         * {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO_OR_MORE}
         */
        @Override
        protected int computeCardinality() {
            return StaticProperty.ALLOWS_ZERO_OR_MORE;
        }

        /**
         * Copy an expression. This makes a deep copy.
         *
         * @param rebindings variables to be re-bound
         * @return the copy of the original expression
         */
        @Override
        public Expression copy(RebindingMap rebindings) {
            return this;
        }
    }

    /**
     * This method creates a copy of a FunctionLibrary: if the original FunctionLibrary allows
     * new functions to be added, then additions to this copy will not affect the original, or
     * vice versa.
     *
     * @return a copy of this function library. This must be an instance of the original class.
     */

    @Override
    public FunctionLibrary copy() {
        JavaExtensionLibrary jel = new JavaExtensionLibrary(config);
        jel.explicitMappings = new HashMap<>(explicitMappings);
        return jel;
    }

}

// Copyright (c) 2009-2022 Saxonica Limited