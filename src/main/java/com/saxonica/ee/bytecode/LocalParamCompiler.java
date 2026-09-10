package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.LocalParam;
import net.sf.saxon.expr.instruct.ParameterSet;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.TypeHierarchy;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Compiler for LocalParam instructions in XSLT (representing an xsl:param in a template)
 */
public class LocalParamCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        LocalParam param = (LocalParam) expression;
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "LocalParam-push");

        // int wasSupplied = context.useLocalParameter(parameterId, slotNumber, isTunnelParam());

        LabelInfo paramDone = methodInfo.newLabel("paramDone");

        compiler.generateGetContext();
        allocateStatic(compiler, param.getVariableQName());
        ga.push(param.getSlotNumber());
        ga.push(param.isTunnelParam());
        ga.invokeInstanceMethod(XPathContext.class, "useLocalParameter", StructuredQName.class, Integer.TYPE, Boolean.TYPE);

//        case ParameterSet.SUPPLIED:
//            if (conversion != null) {
//                context.setLocalVariable(slotNumber,
//                        ExpressionTool.evaluate(conversion, conversionEvaluationMode, context, 10));
//            }

        if (param.getConversion() != null) {
            LabelInfo notSupplied = methodInfo.newLabel("notSupplied");
            ga.dup();
            ga.push(ParameterSet.SUPPLIED);
            ga.ifCmp(Type.INT_TYPE, GeneratorAdapter.NE, notSupplied.label());
            ga.pop();
            compiler.generateGetContext();
            ga.push(param.getSlotNumber());
            compiler.compileToSequence(param.getConversion());
            ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
            ga.goTo(paramDone);
            methodInfo.placeLabel(notSupplied);
        }

//        case ParameterSet.NOT_SUPPLIED:
//            if (isImplicitlyRequiredParam()) {
//                throw new XPathException("A value must be supplied for parameter " ...
//
//            } else if (isRequiredParam()) {
//                throw new XPathException("No value supplied for required parameter " ...
//            }
//            context.setLocalVariable(slotNumber, getSelectValue(context));

        ga.push(ParameterSet.NOT_SUPPLIED);
        ga.ifCmp(Type.INT_TYPE, GeneratorAdapter.NE, paramDone.label());
        if (param.isImplicitlyRequiredParam()) {
            String name = "$" + param.getVariableQName().getDisplayName();
            String message = "A value must be supplied for parameter "
                    + name + " because " +
                    "the default value is not a valid instance of the required type";
            compiler.generateDynamicError(message, "XTDE0700", param.getLocation(), false);
        } else if (param.isRequiredParam()) {
            String name = "$" + param.getVariableQName().getDisplayName();
            String message = "No value supplied for required parameter " + name;
            compiler.generateDynamicError(message, "XTDE0700", param.getLocation(), false);
        } else {
            compiler.generateGetContext();
            ga.push(param.getSlotNumber());
            compiler.generateGetContext();
            ga.dup();
            ga.invokeInstanceMethod(XPathContext.class, "getTemporaryOutputState");
            int savedOutputStateVar = methodInfo.allocateLocal(Integer.TYPE);
            ga.storeLocal(savedOutputStateVar);
            ga.push(StandardNames.XSL_WITH_PARAM);
            ga.invokeInstanceMethod(XPathContext.class, "setTemporaryOutputState", Integer.TYPE);
            compiler.compileToSequence(param.getSelectExpression());
            compiler.generateGetContext();
            ga.loadLocal(savedOutputStateVar);
            ga.invokeInstanceMethod(XPathContext.class, "setTemporaryOutputState", Integer.TYPE);
            ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        }

        methodInfo.placeLabel(paramDone);

    }
}

// Copyright (c) 2011-2022 Saxonica Limited