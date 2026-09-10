package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.NamespaceUri_1;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for evaluation of a namespace-uri() function
 */
public class NamespaceUriFnCompiler extends ToItemCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        LabelInfo namespaceUriReturn = methodInfo.newLabel("namespaceUriReturn");
        final NamespaceUri_1 fn = (NamespaceUri_1) fnc.getTargetFunction();
        Class<? extends Item> argClass = compiler.compileToItem(fnc.getArg(0));
        argClass = ga.downCast(argClass, NodeInfo.class);
        if (Cardinality.allowsZero(fnc.getArg(0).getCardinality())) {
            LabelInfo namespaceNotNull = methodInfo.newLabel("namespaceNotNull");
            ga.dup();
            ga.ifNonNull(namespaceNotNull.label());
            ga.pop();
            ga.getStaticField(AnyURIValue.class, "EMPTY_URI", AnyURIValue.class);
            ga.goTo(namespaceUriReturn);
            methodInfo.placeLabel(namespaceNotNull);
        }
        ga.invokeInstanceMethod(argClass, "getURI");
        int uriVar = methodInfo.allocateLocal(String.class);
        ga.storeLocal(uriVar);
        ga.newInstance(Type.getType(AnyURIValue.class));
        ga.dup();
        ga.loadLocal(uriVar);
        ga.invokeConstructor(AnyURIValue.class, String.class);
        methodInfo.placeLabel(namespaceUriReturn);

        methodInfo.releaseLocal(uriVar);
        return AnyURIValue.class;
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
