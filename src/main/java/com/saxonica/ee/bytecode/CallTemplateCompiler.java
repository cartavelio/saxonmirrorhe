package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.*;

/**
 * Compiler for the XSLT apply-templates instruction
 */
public class CallTemplateCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(/*@NotNull*/ CompilerService compiler, Expression expression) throws CannotCompileException {
        CallTemplate call = (CallTemplate) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        visitLineNumber(compiler, ga, expression);

        compiler.generateGetContext();
        ga.push(call.getBindingSlot());
        ga.invokeInstanceMethod(XPathContext.class, "getTargetComponent", Integer.TYPE);
        ga.dup();
        ga.invokeInstanceMethod(Component.class, "isHiddenAbstractComponent");
        LabelInfo notAbstract = methodInfo.newLabel("notAbstract");
        ga.ifFalse(notAbstract);

        compiler.generateDynamicError("Cannot call an abstract template (" +
                                              call.getCalledTemplateName() +
                                              ") with no implementation", "XTDE3052", expression.getLocation(), false);
        ga.throwException();

        methodInfo.placeLabel(notAbstract);
        ga.dup();
        int targetComponentVar = methodInfo.allocateLocal(NamedTemplate.class);
        ga.storeLocal(targetComponentVar);
        ga.invokeInstanceMethod(Component.class, "getActor");
        ga.checkClass(NamedTemplate.class);

        //allocateStatic(compiler, call.getTargetTemplate());

        int templateVar = methodInfo.allocateLocal(NamedTemplate.class);
        ga.storeLocal(templateVar);

        if (call.usesTailRecursion()) {
            // This shouldn't happen: we avoid injecting a bytecode candidate for tail-recursive templates
            throw new CannotCompileException("tail recursive call-template", true);
        }

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newContext");
        int context2Var = methodInfo.allocateLocal(XPathContextMajor.class);
        visitAnnotation(compiler, "CallTemplateCompiler - newMajorContext");
        ga.storeLocal(context2Var);

        ga.loadLocal(context2Var);
        ga.loadLocal(targetComponentVar);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentComponent", Component.class);

        ga.loadLocal(context2Var);
        ga.loadLocal(templateVar);
        ga.invokeInstanceMethod(NamedTemplate.class, "getStackFrameMap");
        ga.invokeInstanceMethod(XPathContextMajor.class, "openStackFrame", SlotManager.class);

        ga.loadLocal(context2Var);
        ApplyTemplatesCompiler.compileParameterSet(compiler, this, call.getActualParams());
        ga.invokeInstanceMethod(XPathContextMajor.class, "setLocalParameters", ParameterSet.class);

        ga.loadLocal(context2Var);
        ApplyTemplatesCompiler.compileTunnelParameterSet(compiler, this, call.getTunnelParams());
        ga.invokeInstanceMethod(XPathContextMajor.class, "setTunnelParameters", ParameterSet.class);

        ga.loadLocal(context2Var);
        allocateStatic(compiler, expression);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setOrigin", ContextOriginator.class);

//            TailCall tc = t.expand(c2);

        ga.loadLocal(templateVar);
        compiler.generateGetOutputter();
        ga.loadLocal(context2Var);
        ga.invokeInstanceMethod(NamedTemplate.class, "expand", Outputter.class, XPathContext.class);

//            while (tc != null) {
//                tc = tc.processLeavingTail();
//            }        

        LabelInfo callEnd = methodInfo.newLabel("callEnd");
        LabelInfo tailCallLoop = methodInfo.placeNewLabel("loop");
        ga.dup();
        ga.ifNull(callEnd.label());
        ga.invokeInstanceMethod(TailCall.class, "processLeavingTail");
        ga.goTo(tailCallLoop);

        methodInfo.placeLabel(callEnd);
        ga.pop();
        methodInfo.releaseLocal(context2Var);
        methodInfo.releaseLocal(templateVar);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited



