package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.*;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.Mode;
import org.objectweb.asm.Type;

/**
 * Compiler for the XSLT apply-templates instruction
 */
public class ApplyTemplatesCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        if (((ApplyTemplates) expression).getSeparatorExpression() != null) {
            throw new CannotCompileException("Separator attribute", true);
        }
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ApplyTemplates-push");
        visitLineNumber(compiler, ga, expression);
        final ApplyTemplates apply = (ApplyTemplates) expression;

        int modeVar = methodInfo.allocateLocal(Component.M.class);
        if (apply.usesCurrentMode()) {
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getCurrentMode");
            ga.storeLocal(modeVar);
        } else {
            int slot = apply.getBindingSlot();
            compiler.generateGetContext();
            ga.push(slot);
            ga.invokeInstanceMethod(XPathContext.class, "getTargetComponent", Integer.TYPE);
            ga.storeLocal(modeVar);
        }

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newContext");
        int context2Var = methodInfo.allocateLocal(XPathContextMajor.class);
        visitAnnotation(compiler, "ApplyTemplatesCompiler - newMajorContext");
        ga.storeLocal(context2Var);

        ga.loadLocal(context2Var);
        ga.newInstance(FocusTrackingIterator.class);
        ga.dup();
        compiler.compileToIterator(apply.getSelectExpression());
        ga.invokeConstructor(FocusTrackingIterator.class, SequenceIterator.class);
        ga.invokeInstanceMethod(XPathContext.class, "setCurrentIterator", FocusIterator.class);

        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "ApplyTemplatesCompiler - setCurrentIterator");

        ga.loadLocal(context2Var);
        ga.checkClass(XPathContextMajor.class);
        allocateStatic(compiler, expression);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setOrigin", ContextOriginator.class);

        ga.loadLocal(context2Var);
        ga.loadLocal(modeVar);
        ga.checkClass(Component.M.class);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentMode", Component.M.class);

        ga.loadLocal(context2Var);
        ga.loadLocal(modeVar);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentComponent", Component.class);


        // params are evaluated with the old context

        boolean hasNonTunnelParams = apply.getActualParams() != null;
        int paramsVar = 0;
        if (hasNonTunnelParams) {
            paramsVar = methodInfo.allocateLocal(ParameterSet.class);
            compileParameterSet(compiler, this, apply.getActualParams());
            ga.storeLocal(paramsVar);
        }

        int tunnelsVar = methodInfo.allocateLocal(ParameterSet.class);
        compileTunnelParameterSet(compiler, this, apply.getTunnelParams());
        ga.storeLocal(tunnelsVar);

        // now change the context

        methodInfo.pushContextVariableInfo(context2Var, false);

        ga.loadLocal(modeVar);
        ga.invokeInstanceMethod(Component.class, "getActor");
        ga.checkClass(Mode.class);
        if (hasNonTunnelParams) {
            ga.loadLocal(paramsVar);
        } else {
            ga.pushNull();
        }
        ga.loadLocal(tunnelsVar);
        ga.pushNull();
        ga.loadArgOrLocal(methodInfo.getOutputterPosition());
        ga.loadLocal(context2Var);
        allocateStatic(compiler, apply.getLocation());
//        applyTemplates(
//                ParameterSet parameters,
//                ParameterSet tunnelParameters,
//                NodeInfo separator,
//                Outputter output,
//                XPathContextMajor context,
//                Location locationId)
        ga.invokeInstanceMethod(Mode.class, "applyTemplates", ParameterSet.class, ParameterSet.class,
                                NodeInfo.class,
                                Outputter.class, XPathContextMajor.class, Location.class);

        LabelInfo applyEnd = methodInfo.newLabel("applyEnd");
        LabelInfo tailCallLoop = methodInfo.placeNewLabel("loop");
        ga.dup();
        ga.ifNull(applyEnd.label());
        //Note: the processLeavingTail() call reverts to the interpreter
        ga.invokeInstanceMethod(TailCall.class, "processLeavingTail");
        ga.goTo(tailCallLoop);

        methodInfo.placeLabel(applyEnd);
        ga.pop();
        methodInfo.popContextVariableInfo();
        methodInfo.releaseLocal(modeVar);
        methodInfo.releaseLocal(tunnelsVar);
        methodInfo.releaseLocal(context2Var);
        methodInfo.releaseLocal(paramsVar);
    }


    public static void compileParameterSet(CompilerService compiler, ExpressionCompiler ec, WithParam[] params)
            throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "compileParameterSet");
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        if (params == null || params.length == 0) {
            ga.pushNull();
        } else {
//            ParameterSet params = new ParameterSet(actualParams.length);
//            for (int i=0; i<actualParams.length; i++) {
//                params.put(actualParams[i].getParameterId(),
//                           actualParams[i].getSelectValue(context),
//                           actualParams[i].isTypeChecked());
//            }
//            return params;
            ga.newInstance(Type.getType(ParameterSet.class));
            ga.dup();
            ga.push(params.length);
            ga.invokeConstructor(ParameterSet.class, Integer.TYPE);
            for (WithParam param : params) {
                ga.dup();
                compiler.generateGetContext();
                ga.dup();
                ga.invokeInstanceMethod(XPathContext.class, "getTemporaryOutputState");
                int savedOutputStateVar = methodInfo.allocateLocal(Integer.TYPE);
                ga.storeLocal(savedOutputStateVar);
                ga.push(StandardNames.XSL_WITH_PARAM);
                ga.invokeInstanceMethod(XPathContext.class, "setTemporaryOutputState", Integer.TYPE);
                allocateStatic(compiler, param.getVariableQName());
                LetExpressionCompiler.compileCommonExpr(
                        compiler,
                        param.getSelectExpression(),
                        param.getEvaluator(),
                        10,
                        false);
                ga.push(param.isTypeChecked());
                ga.invokeInstanceMethod(ParameterSet.class, "put", StructuredQName.class, Sequence.class, Boolean.TYPE);
                compiler.generateGetContext();
                ga.loadLocal(savedOutputStateVar);
                ga.invokeInstanceMethod(XPathContext.class, "setTemporaryOutputState", Integer.TYPE);

            }
        }
    }

    public static void compileTunnelParameterSet(CompilerService compiler, ExpressionCompiler ec, WithParam[] params)
            throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
//        ParameterSet existingParams = context.getTunnelParameters();
//        if (existingParams == null) {
//            return assembleParams(context, actualParams);
//        }
        int existingParamsVar = methodInfo.allocateLocal(ParameterSet.class);
        LabelInfo endTunnel = methodInfo.newLabel("endTunnel");
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getTunnelParameters");
        ga.storeLocal(existingParamsVar);
        LabelInfo nonNullExisting = methodInfo.newLabel("nonNullExisting");
        ga.loadLocal(existingParamsVar);
        ga.ifNonNull(nonNullExisting.label());
        compileParameterSet(compiler, ec, params);
        ga.goTo(endTunnel);
        methodInfo.placeLabel(nonNullExisting);

//        ParameterSet newParams = new ParameterSet(existingParams, (actualParams==null ? 0 : actualParams.length));

        ga.newInstance(ParameterSet.class);
        ga.dup();
        ga.loadLocal(existingParamsVar);
        ga.push(params == null ? 0 : params.length);
        ga.invokeConstructor(ParameterSet.class, ParameterSet.class, Integer.TYPE);
//        if (actualParams == null || actualParams.length == 0) {
//            return newParams;
//        }
//        for (int i=0; i<actualParams.length; i++) {

//            newParams.put(actualParams[i].getParameterId(),
//                          actualParams[i].getSelectValue(context),
//                          false);
//        }
        if (params != null && params.length != 0) {
            for (WithParam param : params) {
                ga.dup();
                allocateStatic(compiler, param.getVariableQName());
                LetExpressionCompiler.compileCommonExpr(
                        compiler,
                        param.getSelectExpression(),
                        param.getEvaluator(),
                        10,
                        false);
                ga.push(false);
                ga.invokeInstanceMethod(ParameterSet.class, "put", StructuredQName.class, Sequence.class, Boolean.TYPE);
            }
        }
        methodInfo.placeLabel(endTunnel);
        methodInfo.releaseLocal(existingParamsVar);
    }
//        return newParams;


}

// Copyright (c) 2011-2022 Saxonica Limited



