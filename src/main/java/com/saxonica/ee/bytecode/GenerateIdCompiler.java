package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for evaluation of a generate-id() function
 */
public class GenerateIdCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        final SystemFunctionCall fnc = (SystemFunctionCall) expression;
        visitAnnotation(compiler, "GenerateId-String");
        LabelInfo generateIdReturn = methodInfo.newLabel("generateIdReturn");
        Class<? extends Item> argClass = compiler.compileToItem(fnc.getArg(0));
        treatAsEmptyIfFirstArgumentIsEmpty(fnc.getArg(0), ga, methodInfo, generateIdReturn, 0, onEmpty);
        argClass = ga.downCast(argClass, NodeInfo.class);
        ga.newInstance(Type.getType(StringBuilder.class));
        ga.dup();
        ga.push(16);
        ga.invokeConstructor(StringBuilder.class, Integer.TYPE);
        int bufferVar = methodInfo.allocateLocal(StringBuilder.class);
        ga.dup();
        ga.storeLocal(bufferVar);
        ga.invokeInstanceMethod(argClass, "generateId", StringBuilder.class);
        ga.loadLocal(bufferVar);
        ga.invokeInstanceMethod(StringBuilder.class, "trimToSize");
        ga.loadLocal(bufferVar);
        methodInfo.releaseLocal(bufferVar);
        ga.invokeInstanceMethod(StringBuilder.class, "toString");
        if (requiredClass == UnicodeString.class) {
            ga.invokeStaticMethod(BMPString.class, "of", String.class);
        }
        methodInfo.placeLabel(generateIdReturn);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
