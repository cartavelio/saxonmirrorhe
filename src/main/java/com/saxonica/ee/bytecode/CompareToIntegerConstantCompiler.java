package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CompareToIntegerConstant;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.om.Item;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.NumericValue;
import org.objectweb.asm.Type;

import java.math.BigDecimal;


/**
 * Generate code for a CompareToIntegerConstant
 */
public class CompareToIntegerConstantCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, /*@NotNull*/ Expression expression)
            throws CannotCompileException {

        Expression exp = ((CompareToIntegerConstant) expression).getBaseExpression();
        int operator = ((CompareToIntegerConstant) expression).getComparisonOperator();
        long comparand = ((CompareToIntegerConstant) expression).getComparand();

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "CompareToIntegerCompiler");
        visitLineNumber(compiler, ga, expression);

        LabelInfo returnTrue = methodInfo.newLabel("returnTrue");
        LabelInfo returnFalse = methodInfo.newLabel("returnFalse");
        LabelInfo end = methodInfo.newLabel("endCmpToInt");

        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        ItemType operandType = exp.getItemType();

        boolean isDouble = th.isSubType(operandType, BuiltInAtomicType.DOUBLE);
        boolean isFloat = th.isSubType(operandType, BuiltInAtomicType.FLOAT);
        boolean isInteger = th.isSubType(operandType, BuiltInAtomicType.INTEGER);
        boolean isDecimal = (!isInteger) && th.isSubType(operandType, BuiltInAtomicType.DECIMAL);
        boolean isUnknownNumeric = !(isDouble || isFloat || isInteger || isDecimal);

        if (isInteger && compiler.isInRangeForInt(exp) && Math.abs(comparand) < Integer.MAX_VALUE) {

            // Fast path where the operand can be compiled directly to an unboxed int

            compiler.compileToPrimitive(exp, Integer.TYPE, new OnEmpty.UnwindAndJump(returnFalse));
            ga.push((int) comparand);
            ga.ifCmp(Type.INT_TYPE, ValueComparisonCompiler.translateOperator(operator), returnTrue.label());

        } else if (isInteger && compiler.isInRangeForLong(exp)) {

            // Fast path where the operand can be compiled directly to an unboxed long

            compiler.compileToPrimitive(exp, Long.TYPE, new OnEmpty.UnwindAndJump(returnFalse));
            ga.push(comparand);
            ga.ifCmp(Type.LONG_TYPE, ValueComparisonCompiler.translateOperator(operator), returnTrue.label());

        } else {

            Class<? extends Item> operandClass = compiler.compileToItem(exp);
            operandClass = ga.downCast(operandClass, NumericValue.class);
            int numericVar = methodInfo.allocateLocal(operandClass);
            ga.storeLocal(numericVar);

            if (isUnknownNumeric || isDouble || isFloat) {
                ga.loadLocal(numericVar);
                ga.invokeInstanceMethod(operandClass, "isNaN");
                ga.ifTrue((operator == Token.FNE ? returnTrue : returnFalse));
                //ga.ifZCmp((operator == Token.FNE ? GeneratorAdapter.EQ : GeneratorAdapter.NE), returnFalse.label());
            }


            // If the type of comparison is known statically, the convert the constant operand to the type of the
            // variable operand in advance, to avoid run-time conversion

            if (isDouble || isFloat) {
                double value = (double) comparand;
                ga.loadLocal(numericVar);
                ga.invokeInstanceMethod(NumericValue.class, "getDoubleValue");
                ga.push(value);
                ga.ifCmp(Type.getType(Double.TYPE), ValueComparisonCompiler.translateOperator(operator), returnTrue.label());
            } else if (isInteger) {
                if (comparand == 0) {
                    ga.loadLocal(numericVar);
                    ga.invokeInstanceMethod(NumericValue.class, "signum");
                    ga.ifZCmp(ValueComparisonCompiler.translateOperator(operator), returnTrue.label());
                } else {
                    ga.loadLocal(numericVar);
                    ga.push(comparand);
                    ga.invokeInstanceMethod(NumericValue.class, "compareTo", Long.TYPE);
                    ga.ifZCmp(ValueComparisonCompiler.translateOperator(operator), returnTrue.label());
                }
            } else if (th.isSubType(operandType, BuiltInAtomicType.DECIMAL)) {
                ga.loadLocal(numericVar);
                ga.invokeInstanceMethod(NumericValue.class, "getDecimalValue");
                ga.newInstance(BigDecimal.class);
                ga.dup();
                ga.push(comparand);
                ga.invokeConstructor(BigDecimal.class, Long.TYPE);
                ga.invokeInstanceMethod(BigDecimal.class, "compareTo", BigDecimal.class);
                ga.ifZCmp(ValueComparisonCompiler.translateOperator(operator), returnTrue.label());
            } else {
                // type not known statically - generate polymorphic code
                ga.loadLocal(numericVar);
                ga.push(comparand);
                ga.invokeInstanceMethod(NumericValue.class, "compareTo", Long.TYPE);
                ga.ifZCmp(ValueComparisonCompiler.translateOperator(operator), returnTrue.label());
            }
            methodInfo.placeLabel(returnFalse);
            methodInfo.releaseLocal(numericVar);
        }
        ga.push(false);
        ga.goTo(end);
        methodInfo.placeLabel(returnTrue);
        ga.push(true);
        methodInfo.placeLabel(end);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
