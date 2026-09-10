package com.saxonica.config;

import com.saxonica.expr.JavaExtensionFunctionCall;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.functions.registry.ExtensionFunctionFactory;
import net.sf.saxon.om.StructuredQName;

import java.lang.reflect.AccessibleObject;

/**
 * This class acts as a factory for creating expressions that call Java extension functions.
 * A different factory may be registered with the Configuration in order to customize the
 * behaviour. Alternatively, this factory class can be customized by calling setExtensionFunctionClass
 * to nominate a subclass of ExtensionFunctionCall to be used to implement calls on extension functions.
 * <p>Note that this class handles Java extension functions only; a different class,
 * DotNetExtensionFunctionFactory, is used for .NET extensions.
 */

public class JavaExtensionFunctionFactory implements ExtensionFunctionFactory {

    private Class extensionFunctionCallClass = JavaExtensionFunctionCall.class;

    public JavaExtensionFunctionFactory(Configuration config) {
        //
    }

    /**
     * Set the class to be used to represent extension function calls. This must be a subclass
     * of {@link com.saxonica.expr.JavaExtensionFunctionCall}
     *
     * @param subclass the subclass of ExtensionFunctionCall to be used
     */

    public void setExtensionFunctionClass(Class subclass) {
        extensionFunctionCallClass = subclass;
    }

    /**
     * Factory method to create an expression that calls a Java extension function.
     * This is always called at XPath compile time.
     *
     * @param functionName the name of the function
     * @param theClass     the Java class containing the extension function
     * @param method       The "accessibleObject" representing a constructor, method, or field corresponding
     *                     to the extension function
     * @param arguments    Array containing the expressions supplied as arguments to the function call.
     * @param config       the Configuration
     * @return the constructed ExtensionFunctionCall object (a subclass might return any expression
     *         representing the extension function call).
     */

    public Expression makeExtensionFunctionCall(
            StructuredQName functionName, Class theClass, AccessibleObject method, Expression[] arguments, Configuration config) {
        JavaExtensionFunctionCall fn;
        try {
            fn = (JavaExtensionFunctionCall) extensionFunctionCallClass.newInstance();
        } catch (InstantiationException e) {
            throw new IllegalArgumentException(e.getMessage());
        } catch (IllegalAccessException e) {
            throw new IllegalArgumentException(e.getMessage());
        }
        fn.init(functionName, theClass, method, config);
        fn.setArguments(arguments);
        return fn;
    }

}

// Copyright (c) 2009-2022 Saxonica Limited