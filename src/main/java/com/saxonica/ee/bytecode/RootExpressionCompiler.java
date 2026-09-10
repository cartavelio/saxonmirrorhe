package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.RootExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.type.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for a call to the RootExpression()
 */
public class RootExpressionCompiler extends ToItemCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "RootExpressionCompiler");
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
        //int contextItemVar = methodInfo.allocateLocal(Item.class);
        //ga.storeLocal(contextItemVar);

        LabelInfo end = methodInfo.newLabel("endRoot");
        LabelInfo notInstanceLab = methodInfo.newLabel("notInstanceLab");

        if (((RootExpression) expression).isContextPossiblyUndefined()) {
            LabelInfo notNull = methodInfo.newLabel("contextNotNull");
            ga.dup();
            ga.ifNonNull(notNull.label());
            // Throw error - root of the tree
            compiler.generateDynamicError("Finding root of tree: the context item is absent", "XPDY0002", expression.getLocation(), false);

//			ga.push((String)null);
//			ga.goTo(end.label());
            methodInfo.placeLabel(notNull);
        }

        //ga.loadLocal(contextItemVar);
        ga.dup();
        ga.ifNotInstance(NodeInfo.class, notInstanceLab);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getRoot");
        ga.dup();

        ga.invokeInstanceMethod(NodeInfo.class, "getNodeKind");
        ga.push(Type.DOCUMENT);
        ga.ifICmp(GeneratorAdapter.EQ, end.label());

        // Throw error - DocumentRoot is null
        compiler.generateDynamicError("The root of the tree containing the context item is not a document node", "XPDY0050", expression.getLocation(), false);

        methodInfo.placeLabel(notInstanceLab);
        // Throw error - Type error - context item isn't a node
        compiler.generateDynamicError("Finding root of tree: the context item is not a node", "XPTY0020", expression.getLocation(), true);

        methodInfo.placeLabel(end);

        //methodInfo.releaseLocal(contextItemVar);
        return NodeInfo.class;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
