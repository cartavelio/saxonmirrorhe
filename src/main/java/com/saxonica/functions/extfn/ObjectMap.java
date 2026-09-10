package com.saxonica.functions.extfn;

import com.saxonica.config.JavaExtensionLibrary;
import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.hof.CurriedFunction;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyFunctionType;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.SequenceType;

import java.lang.reflect.Method;
import java.lang.reflect.Modifier;
import java.util.*;

/**
 * Extension function saxon:object-map(), which converts an external object to a map
 * <p>The current implementation physically creates a new map, though there's some optimization
 * if it is known that only one of the entries will be needed. There would be scope for
 * a faster implementation, creating a virtual map backed by the Java object, if this
 * proves to be needed.</p>
 * @since 9.9
 */

public class ObjectMap extends SystemFunction {

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
     *                                           code will add information about the error location.
     */

    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        ObjectValue<?> value = (ObjectValue<?>)arguments[0].head();
        return toMap(context.getConfiguration(), value, null);
    }

    /**
     * Convert a Java object to a map
     * @param config the Saxon Configuration
     * @param value the (wrapped) Java object to be converted
     * @param required if non-null, indicates the key of the entry that is required in the map. If
     *                 this parameter is supplied, then the map will be limited to a single entry
     *                 with this key, since it is known that the other entries would never be used.
     * @return an XDM map containing entries representing the public instance-level methods
     * available in the object, to the extent that they have unique names.
     */

    public static MapItem toMap(Configuration config, ObjectValue<?> value, String required) {
        Object obj = value.getObject();
        DictionaryMap mapView = new DictionaryMap();
        if (!config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION) ||
                !config.getBooleanProperty(Feature.ALLOW_EXTERNAL_FUNCTIONS)) {
            return mapView; // empty map; method calls not available
        }

        // See bug 6286. To try and prevent reflexive use of methods on an inaccessible class, we
        // first gather methods that are available on the object's interface if this is known;
        // then we supplement this with methods from the instance class (which may turn out
        // to be inaccessible, but this only matters if they are called).

        Map<String, FunctionItem> interfaceMap = null;
        if (value.getInterface() != null) {
            interfaceMap = makeMethodMap(config, value.getInterface(), required);
            for (String key : interfaceMap.keySet()) {
                FunctionItem method = interfaceMap.get(key);
                Sequence[] boundArgs = new Sequence[method.getArity()];
                boundArgs[0] = value;
                int[] unboundArgs = new int[method.getArity() - 1];
                for (int i = 0; i < unboundArgs.length; i++) {
                    unboundArgs[i] = i + 1;
                }
                CurriedFunction fn = new CurriedFunction(method, boundArgs, unboundArgs);
                mapView.initialPut(key, fn);
            }
        }

        Map<String, FunctionItem> methodMap = makeMethodMap(config, obj.getClass(), required);
        for (String key : methodMap.keySet()) {
            if (interfaceMap == null || !interfaceMap.containsKey(key)) {
                FunctionItem method = methodMap.get(key);
                Sequence[] boundArgs = new Sequence[method.getArity()];
                boundArgs[0] = value;
                int[] unboundArgs = new int[method.getArity() - 1];
                for (int i = 0; i < unboundArgs.length; i++) {
                    unboundArgs[i] = i + 1;
                }
                CurriedFunction fn = new CurriedFunction(method, boundArgs, unboundArgs);
                mapView.initialPut(key, fn);
            }
        }


        CallableDelegate returnThis = new CallableDelegate((context, args) -> value);
        mapView.initialPut("this", new CallableFunction(0, returnThis, AnyFunctionType.getInstance()));
        return mapView;
    }

    /**
     * Make a map representing the methods defined in a class. This map is specific to the class, not to
     * a particular instance. The functions present in this map take an extra first argument representing
     * the target instance; the functions returned in the final instance-level map will be partial applications
     * of the functions in the class-level map
     * @param config the Saxon configuration
     * @param javaClass the Java class whose methods are required
     * @param required if non-null, indicates the key of the entry that is required in the map. If
     *              this parameter is supplied, then the map will be limited to a single entry
     *              with this key, since it is known that the other entries would never be used
     * @return a map whose entries represent public instance-level methods in the supplied Java class,
     * to the extent that these methods have unique names.
     */

    public static Map<String, FunctionItem> makeMethodMap(Configuration config, Class<?> javaClass, String required) {
        HashMap<String, FunctionItem> methodMap = new HashMap<>();

        Set<String> methodsFound = new HashSet<>();
        Map<String, Method> retainedMethods = new HashMap<>();

        // look through the methods of this class adding map entries for each one

        Method[] methods = javaClass.getMethods();
        for (Method theMethod : methods) {
            if (Modifier.isPublic(theMethod.getModifiers()) &&
                    !Modifier.isStatic(theMethod.getModifiers()) &&
                    !theMethod.isSynthetic()) {

                for (String name : new String[]{theMethod.getName(), theMethod.getName() + "_" + theMethod.getParameterCount()}) {
                    if (required == null || required.equals(name)) {
                        if (methodsFound.contains(name)) {
                            // overloaded method: discard it
                            retainedMethods.remove(name);
                        } else {
                            methodsFound.add(name);
                            retainedMethods.put(name, theMethod);
                        }
                    }
                }
            }

        }
        JavaExtensionLibrary lib = new JavaExtensionLibrary((ProfessionalConfiguration) config);
        StaticContext env = new IndependentContext(config);

        for (Map.Entry<String, Method> entry : retainedMethods.entrySet()) {
            FunctionItem fn = makeFunction(lib, env, javaClass, entry.getValue());
            if (fn != null) {
                methodMap.put(entry.getKey(), fn);
            }
        }
        return methodMap;
    }

    private static FunctionItem makeFunction(JavaExtensionLibrary library, StaticContext env, Class javaClass, Method theMethod) {
        try {
            StructuredQName functionName =
                    new StructuredQName("java", NamespaceUri.of("java:" + javaClass.getName() + "?void=this"), theMethod.getName());
            int methodArgs = theMethod.getParameterCount();
            int arity = 1 + methodArgs;
            if (methodArgs >= 1 && theMethod.getParameterTypes()[0].equals(XPathContext.class)) {
                arity--;
            }
            SymbolicName.F sName = new SymbolicName.F(functionName, arity);
            return library.getFunctionItem(sName, env);
        } catch (XPathException e) {
            throw new AssertionError(e);
        }
    }

    /**
     * Get the type of a given field
     *
     * @param field the name of the field
     * @return the type of the field if it is defined, or null otherwise
     */
    private static SequenceType getFieldType(Map<String, FunctionItem> methodMap, String field) {

        FunctionItem fn = methodMap.get(field);
        if (fn == null) {
            return null;
        }
        FunctionItemType base = fn.getFunctionItemType();
        SequenceType resultType = base.getResultType();
        SequenceType[] baseArgTypes = base.getArgumentTypes();
        SequenceType[] curriedArgTypes = Arrays.copyOfRange(baseArgTypes, 1, baseArgTypes.length);
        FunctionItemType curried = new SpecificFunctionType(curriedArgTypes, resultType);
        return SequenceType.makeSequenceType(curried, StaticProperty.EXACTLY_ONE);
    }


}

// Copyright (c) 2018-2023 Saxonica Limited
