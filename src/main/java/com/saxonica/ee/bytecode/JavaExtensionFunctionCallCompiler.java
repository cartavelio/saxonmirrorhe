package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import com.saxonica.expr.JavaExtensionFunctionCall;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.functions.IntegratedFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.*;
import net.sf.saxon.value.StringValue;
import org.objectweb.asm.Type;

import java.lang.reflect.*;

/**
 * Compiles a call to an old-style "reflexive" Java extension function. When compiled, the call is no
 * longer reflexive, but is a direct call on the underlying method.
 */
public class JavaExtensionFunctionCallCompiler extends ToIteratorCompiler {

    /**
     * Generate bytecode to evaluate the expression as a {@link net.sf.saxon.om.SequenceIterator}
     * Precondition: none.
     * Postcondition: at execution time, the stack contains a SequenceIterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        JavaExtensionFunctionCall jefc = (JavaExtensionFunctionCall) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "JavaExtensionFunctionCallCompiler-Itr " + jefc.getDisplayName());
        visitLineNumber(compiler, ga, expression);

        boolean resultConverterOnStack = false;
        JPConverter resultConverter = jefc.getResultConverter();
        if (!SequenceIterator.class.isAssignableFrom(jefc.getReturnClass()) && !jefc.getReturnClass().isPrimitive()) {
            resultConverterOnStack = true;
            allocateStatic(compiler, resultConverter);
        }

        compileToObject(compiler, expression);

        LabelInfo nonNullResult = methodInfo.newLabel("nonNullResult");
        LabelInfo end = methodInfo.newLabel("endJEFC");

        if (jefc.getReturnClass() == Void.TYPE && !jefc.isReturnVoidAsThis()) {
            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
            ga.checkClass(SequenceIterator.class);
        } else if (jefc.getReturnClass().isPrimitive()) {
            if (jefc.getReturnClass() == Integer.TYPE) {
                ga.cast(Type.INT_TYPE, Type.LONG_TYPE);
                ga.invokeStaticMethod(Int64Value.class, "makeIntegerValue", Long.TYPE);
            } else if (jefc.getReturnClass() == Boolean.TYPE) {
                ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
            } else if (jefc.getReturnClass() == Long.TYPE) {
                ga.invokeStaticMethod(Int64Value.class, "makeIntegerValue", Long.TYPE);
            } else if (jefc.getReturnClass() == Short.TYPE) {
                ga.cast(Type.SHORT_TYPE, Type.LONG_TYPE);
                ga.invokeStaticMethod(Int64Value.class, "makeIntegerValue", Long.TYPE);
            } else if (jefc.getReturnClass() == Byte.TYPE) {
                ga.cast(Type.BYTE_TYPE, Type.LONG_TYPE);
                ga.invokeStaticMethod(Int64Value.class, "makeIntegerValue", Long.TYPE);
            } else if (jefc.getReturnClass() == Character.TYPE) {
                ga.invokeStaticMethod(Character.class, "toString", Character.TYPE);
                ga.invokeStaticMethod(StringValue.class, "makeStringValue", CharSequence.class);
            } else if (jefc.getReturnClass() == Float.TYPE) {
                ga.invokeStaticMethod(FloatValue.class, "makeFloatValue", Float.TYPE);
            } else if (jefc.getReturnClass() == Double.TYPE) {
                ga.invokeStaticMethod(DoubleValue.class, "makeDoubleValue", Double.TYPE);
            } else {
                throw new IllegalStateException();
            }
            ga.invokeInstanceMethod(Sequence.class, "iterate");

        } else {
            ga.dup();
            ga.ifNonNull(nonNullResult.label());
            ga.pop();
            if (resultConverterOnStack) {
                ga.pop();
            }
            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
            ga.checkClass(SequenceIterator.class);
            ga.goTo(end);

            methodInfo.placeLabel(nonNullResult);

            if (SequenceIterator.class.isAssignableFrom(jefc.getReturnClass())) {
                ga.checkClass(SequenceIterator.class);
            } else {
                // assert stack: JPConverter object
                // Value.asIterator(resultConverter.convert(result, context));
                compiler.generateGetContext();
                ga.invokeInstanceMethod(JPConverter.class, "convert", Object.class, XPathContext.class);
                ga.invokeInstanceMethod(Sequence.class, "iterate");
            }

            // assert stack: SequenceIterator

            if (jefc.isNodeCheckRequired()) {
                // return new ItemMappingIterator(resultIterator,
                //        new ConfigurationChecknWingFunction(context.getConfiguration()), true);
                int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
                ga.storeLocal(iterVar);
                ga.newInstance(ItemMappingIterator.class);
                ga.dup();
                ga.loadLocal(iterVar);

                ga.newInstance(IntegratedFunctionCall.ConfigurationCheckingFunction.class);
                ga.dup();
                compiler.generateGetContext();
                ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
                ga.invokeConstructor(IntegratedFunctionCall.ConfigurationCheckingFunction.class, Configuration.class);

                ga.push(true);
                ga.invokeConstructor(ItemMappingIterator.class, SequenceIterator.class, ItemMappingFunction.class, Boolean.TYPE);

                methodInfo.releaseLocal(iterVar);
            }

            // assert stack: SequenceIterator

        }

        methodInfo.placeLabel(end);

    }

    /**
     * Generate code to call the constructor, method, or field, leaving the resulting Java object on the run-time stack
     *
     * @param compiler   the compiler service
     * @param expression the Java extension function call
     * @throws CannotCompileException if the expression cannot be compiled
     */

    private void compileToObject(CompilerService compiler, Expression expression) throws CannotCompileException {
        JavaExtensionFunctionCall jefc = (JavaExtensionFunctionCall) expression;
        Generator ga = compiler.getCurrentGenerator();

        GeneratedMethodInfo mi = compiler.getCurrentMethod();
        LabelInfo enterTry = mi.newLabel("enterTry");
        LabelInfo leaveTry = mi.newLabel("leaveTry");
        LabelInfo enterCatch = mi.newLabel("enterCatch");
        LabelInfo leaveCatch = mi.newLabel("leaveCatch");


        Class theClass = jefc.getTargetClass();
        AccessibleObject theMethod = jefc.getTargetMethod();

        if (theMethod instanceof Method || theMethod instanceof Constructor) {
            ga.visitTryCatchBlock(enterTry, leaveTry, enterCatch, "java/lang/Exception");
        }

        if (theMethod instanceof Constructor) {

            ga.newInstance(Type.getType(theClass));
            ga.dup();
            stackConvertedArguments(compiler, ga, jefc, false, true, -1);
            mi.placeLabel(enterTry);
            ga.invokeConstructor(theClass, jefc.getParameterTypes());
            mi.placeLabel(leaveTry);
            ga.goTo(leaveCatch);
            // Catch exceptions thrown by the external function, and convert them to an XPathException
            mi.placeLabel(enterCatch);
            allocateStatic(compiler, jefc);
            ga.invokeStaticMethod(Callback.class, "makeJavaInvocationException", Exception.class, JavaExtensionFunctionCall.class);
            ga.throwException();
            mi.placeLabel(leaveCatch);

        } else if (theMethod instanceof Method) {

//            Method method = (Method) theMethod;
            boolean isStatic = Modifier.isStatic(((Method) theMethod).getModifiers());
//            Object theInstance;
//            if (theParameterTypes == null) { // WH
//            	theParameterTypes = method.getParameterTypes();
//            }
            int instanceVar = -1;
            if (((Method) theMethod).getReturnType() == Void.TYPE && jefc.isReturnVoidAsThis())  {
                instanceVar = ga.newLocal(theClass);
            }
            boolean usesContext = jefc.getParameterTypes().length > 0 &&
                    (jefc.getParameterTypes()[0] == XPathContext.class);
            if (usesContext) {
                if (isStatic) {
                    stackConvertedArguments(compiler, ga, jefc, true, true, instanceVar);
                    mi.placeLabel(enterTry);
                    ga.invokeStaticMethod(theClass, ((Method) theMethod).getName(), jefc.getParameterTypes());
                    mi.placeLabel(leaveTry);
                } else {
                    stackConvertedArguments(compiler, ga, jefc, true, false, instanceVar);
                    mi.placeLabel(enterTry);
                    ga.invokeInstanceMethod(theClass, ((Method) theMethod).getName(), jefc.getParameterTypes());
                    mi.placeLabel(leaveTry);
                }
            } else {
                if (isStatic) {
                    stackConvertedArguments(compiler, ga, jefc, false, true, instanceVar);
                    mi.placeLabel(enterTry);
                    ga.invokeStaticMethod(theClass, ((Method) theMethod).getName(), jefc.getParameterTypes());
                    mi.placeLabel(leaveTry);
                } else {
                    stackConvertedArguments(compiler, ga, jefc, false, false, instanceVar);
                    mi.placeLabel(enterTry);
                    ga.invokeInstanceMethod(theClass, ((Method) theMethod).getName(), jefc.getParameterTypes());
                    mi.placeLabel(leaveTry);
                }
            }
            // Catch exceptions thrown by the external function, and convert them to an XPathException
            ga.goTo(leaveCatch);
            mi.placeLabel(enterCatch);
            allocateStatic(compiler, jefc);
            ga.invokeStaticMethod(Callback.class, "makeJavaInvocationException", Exception.class, JavaExtensionFunctionCall.class);
            ga.throwException();
            mi.placeLabel(leaveCatch);
            if (instanceVar >= 0) {
                ga.loadLocal(instanceVar);
            }

        } else if (theMethod instanceof Field) {
            Field field = (Field) theMethod;
            boolean isStatic = Modifier.isStatic(field.getModifiers());
            Object theInstance;
            if (isStatic) {
                ga.getStaticField(theClass, field.getName(), field.getType());
            } else {
                stackConvertedArguments(compiler, ga, jefc, false, false, -1);
                ga.getInstanceField(theClass, field.getName());
            }

        }

    }

    private void stackConvertedArguments(CompilerService compiler, Generator ga, JavaExtensionFunctionCall jfc,
                                         boolean usesContext, boolean isStatic, int instanceVar) throws CannotCompileException {
        int offset = 0;
        for (int i = 0; i < jfc.getArity(); i++) {
            // params[j] = argumentConverters[i].convert(argValues[i], paramTypes[j], context);
            if (usesContext && i == (isStatic ? 0 : 1)) {
                compiler.generateGetContext();
                offset++;
            }
            PJConverter converter = jfc.getArgumentConverters()[i];
            Class requiredClass;
            if (i == 0 && !isStatic) {
                requiredClass = jfc.getTargetClass();
                offset--;
            } else {
                requiredClass = jfc.getParameterTypes()[i + offset];
            }
            if (!(converter instanceof PJConverter.Identity)) {
                boolean done = false;
                if (jfc.getArg(i) instanceof Literal) {
                    try {
                        // try to do the conversion now, without supplying any context; but recover if this fails
                        Object obj = converter.convert(((Literal) jfc.getArg(i)).getGroundedValue(), requiredClass, null);
                        allocateStatic(compiler, obj);
                        done = true;
                    } catch (Exception err) {
                        done = false;
                    }
                }
                if (!done) {
                    allocateStatic(compiler, converter);
                    compiler.compileToSequence(jfc.getArg(i));

                    allocateStatic(compiler, requiredClass);
                    compiler.generateGetContext();
                    ga.invokeInstanceMethod(PJConverter.class, "convert", Sequence.class, Class.class, XPathContext.class);
                }
                if (i == 0 && !isStatic && instanceVar >= 0) {
                    // Save the "this" value so it can be used as the return value
                    ga.dup();
                    ga.storeLocal(instanceVar);
                }
                if (requiredClass.isPrimitive()) {
                    // need to unbox the boxed value returned by the PJConverter
                    unbox(ga, requiredClass);
                } else {
                    ga.checkClass(requiredClass);
                }
            } else if (requiredClass == Item.class) {
                compiler.compileToItem(jfc.getArg(i));
                ga.checkClass(requiredClass);
            } else {
                allocateStatic(compiler, converter);
                compiler.compileToSequence(jfc.getArg(i));
                allocateStatic(compiler, requiredClass);
                compiler.generateGetContext();
                ga.invokeInstanceMethod(PJConverter.class, "convert", Sequence.class, Class.class, XPathContext.class);
                ga.checkClass(requiredClass);
            }

        }

        // if the XPathContext is the only argument, we need to stack it now
        if (usesContext && jfc.getArity() == (isStatic ? 0 : 1)) {
            compiler.generateGetContext();
        }

        // TODO: avoid conversion if the identity converter is used.
    }

    private void unbox(Generator ga, Class requiredClass) {
        if (requiredClass == Integer.TYPE) {
            ga.checkClass(Integer.class);
            ga.invokeInstanceMethod(Integer.class, "intValue");
        } else if (requiredClass == Boolean.TYPE) {
            ga.checkClass(Boolean.class);
            ga.invokeInstanceMethod(Boolean.class, "booleanValue");
        } else if (requiredClass == Long.TYPE) {
            ga.checkClass(Long.class);
            ga.invokeInstanceMethod(Long.class, "longValue");
        } else if (requiredClass == Short.TYPE) {
            ga.checkClass(Short.class);
            ga.invokeInstanceMethod(Short.class, "shortValue");
        } else if (requiredClass == Byte.TYPE) {
            ga.checkClass(Byte.class);
            ga.invokeInstanceMethod(Byte.class, "byteValue");
        } else if (requiredClass == Character.TYPE) {
            ga.checkClass(Character.class);
            ga.invokeInstanceMethod(Character.class, "charValue");
        } else if (requiredClass == Float.TYPE) {
            ga.checkClass(Float.class);
            ga.invokeInstanceMethod(Float.class, "floatValue");
        } else if (requiredClass == Double.TYPE) {
            ga.checkClass(Double.class);
            ga.invokeInstanceMethod(Double.class, "doubleValue");
        } else {
            throw new IllegalStateException();
        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited


