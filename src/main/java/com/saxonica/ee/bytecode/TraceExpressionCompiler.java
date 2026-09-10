package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.UnaryExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ComponentTracer;
import net.sf.saxon.expr.instruct.TraceExpression;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trace.Traceable;
import org.objectweb.asm.commons.GeneratorAdapter;

import java.util.HashMap;
import java.util.Map;

public class TraceExpressionCompiler extends ToIteratorCompiler {

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                             Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getController");
        int controllerVar = methodInfo.allocateLocal(Controller.class);
        ga.storeLocal(controllerVar);

        enterTrace(compiler, ga, controllerVar, expression);
        Expression expr0 = getTracedExpression(expression);
        IteratorDetails argIterator = compiler.compileToIterator(expr0);
        int itrVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(itrVar);
        visitLineNumber(compiler, ga, expression);
        leaveTrace(compiler, ga, controllerVar, expression);

        ga.loadLocal(itrVar);

        methodInfo.releaseLocal(controllerVar);
        methodInfo.releaseLocal(itrVar);

        return new IteratorDetails(SequenceIterator.class, argIterator.itemClass);

    }

    private static Expression getTracedExpression(Expression wrapper) throws CannotCompileException {
        if (wrapper instanceof ComponentTracer) {
            return ((ComponentTracer)wrapper).getChild();
        } else if (wrapper instanceof TraceExpression) {
            return ((TraceExpression)wrapper).getChild();
        } else if (wrapper instanceof UnaryExpression) {
            return ((UnaryExpression)wrapper).getBaseExpression();
        } else {
            throw new CannotCompileException();
        }
    }

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler,
                                               Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getController");
        int controllerVar = methodInfo.allocateLocal(Controller.class);
        ga.storeLocal(controllerVar);
        enterTrace(compiler, ga, controllerVar, expression);
        Expression expr0 = getTracedExpression(expression);
        Class<? extends Item> operandClass = compiler.compileToItem(expr0);
        int itemVar = methodInfo.allocateLocal(operandClass);
        ga.storeLocal(itemVar);
        visitLineNumber(compiler, ga, expression);
        leaveTrace(compiler, ga, controllerVar, expression);

        ga.loadLocal(itemVar);

        methodInfo.releaseLocal(itemVar);
        methodInfo.releaseLocal(controllerVar);
        return operandClass;
    }

    @Override
    public void compileToPush(CompilerService compiler,
                              Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getController");
        int controllerVar = methodInfo.allocateLocal(Controller.class);
        ga.storeLocal(controllerVar);

        enterTrace(compiler, ga, controllerVar, expression);
        Expression expr0 = getTracedExpression(expression);
        compiler.compileToPush(expr0);
        visitLineNumber(compiler, ga, expression);
        leaveTrace(compiler, ga, controllerVar, expression);

        methodInfo.releaseLocal(controllerVar);
    }

    public void enterTrace(CompilerService compiler, Generator ga, int controllerVar, Traceable instrInfo) throws CannotCompileException {

        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo notTracing1 = methodInfo.newLabel("notTracing1");
        ga.loadLocal(controllerVar);
        ga.invokeInstanceMethod(Controller.class, "isTracing");
        ga.ifZCmp(GeneratorAdapter.EQ, notTracing1.label());
        ga.loadLocal(controllerVar);
        ga.invokeInstanceMethod(Controller.class, "getTraceListener");
        allocateStatic(compiler, instrInfo);
        allocateStatic(compiler, new HashMap()); // TODO: put something useful here
        compiler.generateGetContext();
        ga.invokeInstanceMethod(TraceListener.class, "enter", Traceable.class, Map.class, XPathContext.class);

        methodInfo.placeLabel(notTracing1);
    }

    public void leaveTrace(CompilerService compiler, Generator ga, int controllerVar, Traceable instrInfo) throws CannotCompileException {

        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo notTracing2 = methodInfo.newLabel("notTracing2");
        ga.loadLocal(controllerVar);
        ga.invokeInstanceMethod(Controller.class, "isTracing");
        ga.ifZCmp(GeneratorAdapter.EQ, notTracing2.label());
        ga.loadLocal(controllerVar);
        ga.invokeInstanceMethod(Controller.class, "getTraceListener");
        allocateStatic(compiler, instrInfo);
        ga.invokeInstanceMethod(TraceListener.class, "leave", Traceable.class);

        methodInfo.placeLabel(notTracing2);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited