package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.instruct.IterateInstr;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.om.FocusTrackingIterator;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.rules.Rule;

/**
 * Bytecode generator for the xsl:iterate instruction
 */
public class IterateInstrCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        final IterateInstr instr = (IterateInstr) expression;

        visitAnnotation(compiler, "IterateInstrCompiler-Push");
        visitLineNumber(compiler, ga, expression);

//        SequenceIterator iter = select.iterate(context);

        int iterVar = methodInfo.allocateLocal(FocusIterator.class);
        ga.newInstance(FocusTrackingIterator.class);
        ga.dup();
        compiler.compileToIterator(instr.getSelectExpression());
        ga.invokeConstructor(FocusTrackingIterator.class, SequenceIterator.class);
        ga.storeLocal(iterVar);
//
//        XPathContextMajor c2 = context.newContext();
//        c2.setOrigin(this);
//        c2.setCurrentIterator(iter);
//        c2.setCurrentTemplateRule(null);
//

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newContext");
        int context2Var = methodInfo.allocateLocal(XPathContextMajor.class);
        ga.storeLocal(context2Var);

        ga.loadLocal(context2Var);
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(XPathContext.class, "setCurrentIterator", FocusIterator.class);

        ga.loadLocal(context2Var);
        ga.pushNull();
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentTemplateRule", Rule.class);

//        boolean tracing = context.getController().isTracing();
//        TraceListener listener = (tracing ? context.getController().getTraceListener() : null);
//
//        initiallyExp.process(context);

        compiler.compileToPush(instr.getInitiallyExp());

        // (not done earlier because the params are evaluated with the old context)
        methodInfo.pushContextVariableInfo(context2Var, false);

        LabelInfo loop = methodInfo.placeNewLabel("iterLoop");
        methodInfo.getXslIterateLoopLabelStack().push(loop);
        LabelInfo iterBreak = methodInfo.newLabel("iterBreak");
        methodInfo.getXslIterateBreakLabelStack().push(iterBreak);
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        LabelInfo completion = methodInfo.newLabel("onCompletion");
        ga.ifNull(completion.label());

        compiler.compileToPush(instr.getActionExpression());

        ga.goTo(loop);

        methodInfo.placeLabel(completion);
        compiler.compileToPush(instr.getOnCompletion());
        LabelInfo end = methodInfo.newLabel("iterateEnd");
        ga.goTo(end);

        methodInfo.placeLabel(iterBreak);
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "close");

        methodInfo.placeLabel(end);

        methodInfo.popContextVariableInfo();

        methodInfo.getXslIterateLoopLabelStack().pop();
        methodInfo.getXslIterateBreakLabelStack().pop();
        methodInfo.releaseLocal(context2Var);
        methodInfo.releaseLocal(iterVar);

    }

}

//
// Copyright (c) 2011-2022 Saxonica Limited