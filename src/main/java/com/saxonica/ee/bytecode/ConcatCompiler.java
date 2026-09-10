package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.LocalVariableReference;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.SystemFunctionCall;

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
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        //Expression[] arguments = ((Concat) expression).getArguments();
        int numArgs = fnc.getArity();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Concat-item");
        int valVar = methodInfo.allocateLocal(AtomicValue.class);
        int sbVar = methodInfo.allocateLocal(StringBuilder.class);
        ga.newInstance(Type.getType(StringBuilder.class));
        ga.dup();
        ga.push(64);
        ga.invokeConstructor(StringBuilder.class, Integer.TYPE);
        ga.storeLocal(sbVar);
        for (int i = 0; i < numArgs; i++) {
            if (th.isSubType(fnc.getArg(i).getItemType(), BuiltInAtomicType.STRING) &&
                    fnc.getArg(i).getCardinality() == StaticProperty.EXACTLY_ONE) {
                // this path is particularly useful for literal arguments
                ga.loadLocal(sbVar);
                compiler.compileToPrimitive(fnc.getArg(i), CharSequence.class, OnEmpty.RETURN_EMPTY_STRING);
                ga.invokeInstanceMethod(StringBuilder.class, "append", CharSequence.class);
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
                ga.invokeInstanceMethod(AtomicValue.class, "getStringValueCS");
                ga.invokeInstanceMethod(StringBuilder.class, "append", CharSequence.class);
                if (nullLab != null) {
                    methodInfo.placeLabel(nullLab);
                }
            }
        }
        ga.loadLocal(sbVar);
        ga.invokeInstanceMethod(StringBuilder.class, "condense");
        methodInfo.releaseLocal(valVar);
        methodInfo.releaseLocal(sbVar);
        if (requiredClass == String.class) {
            ga.invokeInstanceMethod(Object.class, "toString");
        }
    }

}

// Copyright (c) 2011-2020 Saxonica Limited
