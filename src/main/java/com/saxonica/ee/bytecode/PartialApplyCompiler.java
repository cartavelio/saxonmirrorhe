package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.hof.CurriedFunction;
import net.sf.saxon.functions.hof.PartialApply;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for a call to a PartialApply expression
 */
public class PartialApplyCompiler extends ToItemCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "PartialApplyCompiler - " + expression.getExpressionName());
        visitLineNumber(compiler, ga, expression);
//        LabelInfo notNullLab = methodInfo.newLabel("notNullLab_PartialApply");
        PartialApply pa = (PartialApply) expression;

        compiler.compileToItem(pa.getBaseExpression());
        ga.checkClass(FunctionItem.class);
//        ga.dup();
//        // assert f != null;
//        ga.ifNonNull(notNullLab.label());
//        ga.invokeDefaultConstructor(AssertionError.class);
//        ga.throwException();
//        methodInfo.placeLabel(notNullLab);

        ga.push(pa.getNumberOfArguments());
        ga.newArray(Type.getType(Sequence.class));
        int valuesVar = methodInfo.allocateLocal(Sequence[].class);
        ga.storeLocal(valuesVar);
        visitAnnotation(compiler, "ArrayCreated - ");

        for (int i=0; i<pa.getNumberOfArguments(); i++) {
            Expression expr = pa.getArgument(i);
            if (expr == null) {
                ga.loadLocal(valuesVar);
                ga.push(i);
                ga.push((String) null);
                ga.arrayStore(Type.getType(Sequence.class));
            } else {
                ga.loadLocal(valuesVar);
                ga.push(i);
                compiler.compileToIterator(expr);
                ga.invokeStaticMethod(SequenceTool.class, "toGroundedValue", SequenceIterator.class);
                ga.arrayStore(Type.getType(Sequence.class));
            }
        }
        LabelInfo end = methodInfo.newLabel("endPartialApply");
        LabelInfo L0 = methodInfo.newLabel("L0");
        LabelInfo L1 = methodInfo.newLabel("L1");
        LabelInfo L2 = methodInfo.newLabel("L2");
        ga.visitTryCatchBlock(L0, L1, L2, "net/sf/saxon/trans/XPathException");
        /* try block*/
        methodInfo.placeLabel(L0);
        // stack: functionItem
        ga.newInstance(CurriedFunction.class);
        ga.dup();
        // stack: functionItem CurriedFunction CurriedFunction
        ga.dup2X1();
        ga.pop2();
        // stack: CurriedFunction CurriedFunction functionItem
        ga.loadLocal(valuesVar);
        // stack: CurriedFunction CurriedFunction functionItem sequence[]
        ga.invokeConstructor(CurriedFunction.class, FunctionItem.class, Sequence[].class);
        // stack: CurriedFunction
        methodInfo.placeLabel(L1);
        ga.goTo(end.label());

        /*catch block*/
        methodInfo.placeLabel(L2);
        visitAnnotation(compiler, "PartialApply-Catch");
        ga.dup();
        allocateStatic(compiler, pa.getLocation());
        ga.invokeInstanceMethod(XPathException.class, "maybeSetLocation", Location.class);
        ga.dup();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathException.class, "maybeSetContext", XPathContext.class);
        ga.throwException();
        methodInfo.placeLabel(end);
        methodInfo.releaseLocal(valuesVar);
        return FunctionItem.class;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
