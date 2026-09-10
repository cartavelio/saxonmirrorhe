package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.TraceExpression;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trace.InstructionInfo;
import org.objectweb.asm.commons.GeneratorAdapter;

public class TraceExpressionCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getController");
        int controllerVar = methodInfo.allocateLocal(Controller.class);
        ga.storeLocal(controllerVar);
        InstructionInfo instrInfo = (TraceExpression) expression;

        enterTrace(compiler, ga, controllerVar, instrInfo);
        Expression expr0 = ((TraceExpression) expression).getChild();
        compiler.compileToIterator(expr0);
        int itrVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(itrVar);
        visitLineNumber(compiler, ga, expression);
        leaveTrace(compiler, ga, controllerVar, instrInfo);

        ga.loadLocal(itrVar);

        methodInfo.releaseLocal(controllerVar);
        methodInfo.releaseLocal(itrVar);

    }

    @Override
    public void compileToItem(CompilerService compiler,
                              Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getController");
        int controllerVar = methodInfo.allocateLocal(Controller.class);
        ga.storeLocal(controllerVar);
        InstructionInfo instrInfo = (TraceExpression) expression;

        enterTrace(compiler, ga, controllerVar, instrInfo);
        Expression expr0 = ((TraceExpression) expression).getChild();
        compiler.compileToItem(expr0);
        int itemVar = methodInfo.allocateLocal(Item.class);
        ga.storeLocal(itemVar);
        visitLineNumber(compiler, ga, expression);
        leaveTrace(compiler, ga, controllerVar, instrInfo);

        ga.loadLocal(itemVar);

        methodInfo.releaseLocal(itemVar);
        methodInfo.releaseLocal(controllerVar);
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
        InstructionInfo instrInfo = (TraceExpression) expression;

        enterTrace(compiler, ga, controllerVar, instrInfo);
        Expression expr0 = ((TraceExpression) expression).getChild();
        compiler.compileToPush(expr0);
        visitLineNumber(compiler, ga, expression);
        leaveTrace(compiler, ga, controllerVar, instrInfo);

        methodInfo.releaseLocal(controllerVar);
    }

    public void enterTrace(CompilerService compiler, Generator ga, int controllerVar, InstructionInfo instrInfo) throws CannotCompileException {

        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo notTracing1 = methodInfo.newLabel("notTracing1");
        ga.loadLocal(controllerVar);
        ga.invokeInstanceMethod(Controller.class, "isTracing");
        ga.ifZCmp(GeneratorAdapter.EQ, notTracing1.label());
        ga.loadLocal(controllerVar);
        ga.invokeInstanceMethod(Controller.class, "getTraceListener");
        allocateStatic(compiler, instrInfo);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(TraceListener.class, "enter", InstructionInfo.class, XPathContext.class);

        methodInfo.placeLabel(notTracing1);
    }

    public void leaveTrace(CompilerService compiler, Generator ga, int controllerVar, InstructionInfo instrInfo) throws CannotCompileException {

        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo notTracing2 = methodInfo.newLabel("notTracing2");
        ga.loadLocal(controllerVar);
        ga.invokeInstanceMethod(Controller.class, "isTracing");
        ga.ifZCmp(GeneratorAdapter.EQ, notTracing2.label());
        ga.loadLocal(controllerVar);
        ga.invokeInstanceMethod(Controller.class, "getTraceListener");
        allocateStatic(compiler, instrInfo);
        ga.invokeInstanceMethod(TraceListener.class, "leave", InstructionInfo.class);

        methodInfo.placeLabel(notTracing2);
    }
}

// Copyright (c) 2011 Saxonica Limited.