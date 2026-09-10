package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.LocalVariableReference;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for evaluation of a Concat function
 */
public class ConcatCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {
        Class<?> builderClass;
        if (requiredClass == String.class) {
            builderClass = StringBuilder.class;
        } else if (requiredClass == UnicodeString.class) {
            builderClass = UnicodeBuilder.class;
        } else {
            throw new IllegalArgumentException(requiredClass.toString());
        }
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        //Expression[] arguments = ((Concat) expression).getArguments();
        int numArgs = fnc.getArity();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Concat-item");
        int valVar = methodInfo.allocateLocal(AtomicValue.class);
        int sbVar = methodInfo.allocateLocal(builderClass);
        ga.newInstance(Type.getType(builderClass));
        ga.dup();
        ga.push(64);
        ga.invokeConstructor(builderClass, Integer.TYPE);
        ga.storeLocal(sbVar);
        for (int i = 0; i < numArgs; i++) {
            if (th.isSubType(fnc.getArg(i).getItemType(), BuiltInAtomicType.STRING) &&
                    fnc.getArg(i).getCardinality() == StaticProperty.EXACTLY_ONE) {
                // this path is particularly useful for literal arguments
                ga.loadLocal(sbVar);
                compiler.compileToPrimitive(fnc.getArg(i), requiredClass, OnEmpty.RETURN_EMPTY_STRING);
                ga.invokeInstanceMethod(builderClass, "append", requiredClass==String.class ? String.class : UnicodeString.class);
            } else {
                compiler.compileToItem(fnc.getArg(i));
                ga.checkClass(AtomicValue.class);
                ga.storeLocal(valVar);
                LabelInfo nullLab = null;
                if (Cardinality.allowsZero(fnc.getArg(i).getCardinality()) || fnc.getArg(i) instanceof LocalVariableReference) {
                    nullLab = methodInfo.newLabel("notNullCc" + i);
                    ga.loadLocal(valVar);
                    ga.ifNull(nullLab.label());
                }
                ga.loadLocal(sbVar);
                ga.loadLocal(valVar);
                ga.invokeInstanceMethod(AtomicValue.class, requiredClass == String.class ? "getStringValue" : "getUnicodeStringValue");
                ga.invokeInstanceMethod(builderClass, "append", requiredClass == String.class ? String.class : UnicodeString.class);
                if (nullLab != null) {
                    methodInfo.placeLabel(nullLab);
                }
            }
        }
        ga.loadLocal(sbVar);
        ga.invokeInstanceMethod(builderClass, "trimToSize");
        methodInfo.releaseLocal(valVar);
        methodInfo.releaseLocal(sbVar);
        if (requiredClass == String.class) {
            ga.invokeInstanceMethod(Object.class, "toString");
        }
    }

}

// Copyright (c) 2011-2020 Saxonica Limited
