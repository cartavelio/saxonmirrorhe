package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.instruct.Choose;
import net.sf.saxon.tree.iter.EmptyIterator;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate code for a conditional expression (if-then-else, xsl:if, xsl:choose, typeswitch...)
 */
public class ChooseCompiler extends ToIteratorCompiler {

    /**
     * Generate bytecode to evaluate the expression as a SequenceIterator
     * Precondition: none.
     * Postcondition: the stack contains a SequenceIterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        Choose choose = (Choose) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ChooseCompiler-Itr");
        LabelInfo end = methodInfo.newLabel("endChoose");
        boolean needOtherwise = true;
        for (int i = 0; i < choose.size(); i++) {
            Expression condition = choose.getCondition(i);
            LabelInfo notTrue = null;
            if (i == choose.size() - 1 && Literal.hasEffectiveBooleanValue(condition, true)) {
                needOtherwise = false;
            } else {
                notTrue = methodInfo.newLabel("notTrue" + i);
                compiler.compileToBoolean(condition);
                ga.ifZCmp(GeneratorAdapter.EQ, notTrue.label());
            }
            visitAnnotation(compiler, "when " + condition.toString());
            compiler.compileToIterator(choose.getAction(i));
            ga.goTo(end);
            if (notTrue != null) {
                methodInfo.placeLabel(notTrue);
                ga.visitLineNumber(condition.getLocation().getLineNumber(), notTrue.label());
                visitAnnotation(compiler, "when not " + condition.toString());
            }
        }
        if (needOtherwise) {
            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
        }
        methodInfo.placeLabel(end);
    }

    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        Choose choose = (Choose) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ChooseCompiler-Item");
        LabelInfo end = methodInfo.newLabel("endChoose");
        boolean needOtherwise = true;
        for (int i = 0; i < choose.size(); i++) {
            Expression condition = choose.getCondition(i);
            LabelInfo notTrue = null;
            if (i == choose.size() - 1 && Literal.hasEffectiveBooleanValue(condition, true)) {
                needOtherwise = false;
            } else {
                notTrue = methodInfo.newLabel("notTrue" + i);
                compiler.compileToBoolean(condition);
                ga.ifFalse(notTrue);
            }
            compiler.compileToItem(choose.getAction(i));
            if (i != choose.size() - 1 || needOtherwise) {
                ga.goTo(end);
            }
            if (notTrue != null) {
                methodInfo.placeLabel(notTrue);
            }
        }
        if (needOtherwise) {
            ga.pushNull();
        }
        methodInfo.placeLabel(end);
    }

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        Choose choose = (Choose) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ChooseCompiler-Boolean");
        LabelInfo end = methodInfo.newLabel("endChoose");
        boolean needOtherwise = true;
        for (int i = 0; i < choose.size(); i++) {
            Expression condition = choose.getCondition(i);
            LabelInfo notTrue = null;
            if (i == choose.size() - 1 && Literal.hasEffectiveBooleanValue(condition, true)) {
                needOtherwise = false;
            } else {
                notTrue = methodInfo.newLabel("notTrue" + i);
                compiler.compileToBoolean(condition);
                ga.ifZCmp(GeneratorAdapter.EQ, notTrue.label());
            }
            compiler.compileToBoolean(choose.getAction(i));
            ga.goTo(end);
            if (notTrue != null) {
                methodInfo.placeLabel(notTrue);
            }
        }
        if (needOtherwise) {
            ga.push(false);
        }
        methodInfo.placeLabel(end);
    }


    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        Choose choose = (Choose) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ChooseCompiler-Boolean");
        LabelInfo end = methodInfo.newLabel("endChoose");
        boolean needOtherwise = true;
        for (int i = 0; i < choose.size(); i++) {
            Expression condition = choose.getCondition(i);
            LabelInfo notTrue = null;
            if (i == choose.size() - 1 && Literal.hasEffectiveBooleanValue(condition, true)) {
                needOtherwise = false;
            } else {
                notTrue = methodInfo.newLabel("notTrue" + i);
                compiler.compileToBoolean(condition);
                ga.ifZCmp(GeneratorAdapter.EQ, notTrue.label());
            }
            compiler.compileToPrimitive(choose.getAction(i), requiredClass, onEmpty);
            ga.goTo(end);
            if (notTrue != null) {
                methodInfo.placeLabel(notTrue);
            }
        }
        if (needOtherwise) {
            onEmpty.generate(ga);
        }
        methodInfo.placeLabel(end);
    }

    @Override
    public void compileToPush(/*@NotNull*/ CompilerService compiler, Expression expression) throws CannotCompileException {
        Choose choose = (Choose) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ChooseCompiler-Push");
        LabelInfo end = methodInfo.newLabel("endChoose");
        for (int i = 0; i < choose.size(); i++) {
            Expression condition = choose.getCondition(i);
            LabelInfo notTrue = null;
            if (i == choose.size() - 1 && Literal.hasEffectiveBooleanValue(condition, true)) {
                // no action
            } else {
                notTrue = methodInfo.newLabel("notTrue" + i);
                compiler.compileToBoolean(condition);
                ga.ifZCmp(GeneratorAdapter.EQ, notTrue.label());
            }
            compiler.compileToPush(choose.getAction(i));
            ga.goTo(end);
            if (notTrue != null) {
                methodInfo.placeLabel(notTrue);
            }
        }
        methodInfo.placeLabel(end);
    }
}

// Copyright (c) 2011-2020 Saxonica Limited




