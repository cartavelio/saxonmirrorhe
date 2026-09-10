package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CompareToConstant;
import net.sf.saxon.expr.CompareToStringConstant;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.StringValue;


/**
 * Generate code for a CompareToStringConstant
 */
public class CompareToStringConstantCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, /*@NotNull*/ Expression expression)
            throws CannotCompileException {

        Expression exp = ((CompareToConstant) expression).getBaseExpression();
        int operator = ((CompareToConstant) expression).getComparisonOperator();
        UnicodeString comparand = ((CompareToStringConstant) expression).getComparand();

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "CompareToStringCompiler");
        visitLineNumber(compiler, ga, expression);

        LabelInfo returnTrue = methodInfo.newLabel("returnTrue");
        LabelInfo returnFalse = methodInfo.newLabel("returnFalse");
        LabelInfo end = methodInfo.newLabel("endCmpToString");

        ga.invokeStaticMethod(CodepointCollator.class, "getInstance");
        Class<? extends Item> operandClass = compiler.compileToItem(exp);
        operandClass = ga.downCast(operandClass, StringValue.class);
        ga.invokeInstanceMethod(operandClass, "getUnicodeStringValue");
        allocateStatic(compiler, comparand);
        ga.invokeInstanceMethod(CodepointCollator.class, "compareStrings", UnicodeString.class, UnicodeString.class);
        ga.ifZCmp(ValueComparisonCompiler.translateOperator(operator), returnTrue.label());

        methodInfo.placeLabel(returnFalse);
        ga.push(false);
        ga.goTo(end);
        methodInfo.placeLabel(returnTrue);
        ga.push(true);
        methodInfo.placeLabel(end);
    }

}

// Copyright (c) 2018-2022 Saxonica Limited
