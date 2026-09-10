package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.tree.util.FastStringBuffer;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for evaluation of a generate-id() function
 */
public class GenerateIdCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        final SystemFunctionCall fnc = (SystemFunctionCall) expression;
        visitAnnotation(compiler, "GenerateId-String");
        LabelInfo generateIdReturn = methodInfo.newLabel("generateIdReturn");
        compiler.compileToItem(fnc.getArg(0));
        returnZeroLengthStringIfNull(fnc.getArg(0), ga, methodInfo, generateIdReturn, 0);
        ga.checkClass(NodeInfo.class);
        ga.newInstance(Type.getType(FastStringBuffer.class));
        ga.dup();
        ga.push(FastStringBuffer.C16);
        ga.invokeConstructor(FastStringBuffer.class, Integer.TYPE);
        int bufferVar = methodInfo.allocateLocal(FastStringBuffer.class);
        ga.dup();
        ga.storeLocal(bufferVar);
        ga.invokeInstanceMethod(NodeInfo.class, "generateId", FastStringBuffer.class);
        ga.loadLocal(bufferVar);
        ga.invokeInstanceMethod(FastStringBuffer.class, "condense");
        methodInfo.placeLabel(generateIdReturn);
        methodInfo.releaseLocal(bufferVar);
        if (requiredClass == String.class) {
            ga.invokeInstanceMethod(Object.class, "toString");
        }
    }
}

// Copyright (c) 2011-2020 Saxonica Limited
