package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.EquivalenceComparison;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;

/**
 * Generate bytecode for an equivalence expression, which is the equality test used in an XQuery 3.0 switch expression
 */
public class EquivalenceComparisonCompiler extends ToBooleanCompiler {

    /**
     * Generate bytecode to evaluate the expression as a boolean
     * Precondition: none.
     * Postcondition: at execution time, the stack contains an integer (0=false, 1=true) representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {

        EquivalenceComparison test = (EquivalenceComparison) expression;

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        visitAnnotation(compiler, "EquivalenceComparisonCompiler");
        visitLineNumber(compiler, ga, expression);
        LabelInfo done = methodInfo.newLabel("EqCdone");
        LabelInfo returnFalse = methodInfo.newLabel("EqFalse");
        LabelInfo returnTrue = methodInfo.newLabel("EqTrue");


        int op0Var = methodInfo.allocateLocal(AtomicValue.class);
        int op1Var = methodInfo.allocateLocal(AtomicValue.class);

        compiler.compileToItem(test.getLhsExpression());
        ga.checkClass(AtomicValue.class);
        ga.storeLocal(op0Var);

        compiler.compileToItem(test.getRhsExpression());
        ga.checkClass(AtomicValue.class);
        ga.storeLocal(op1Var);

        if (Cardinality.allowsZero(test.getLhsExpression().getCardinality()) || Cardinality.allowsZero(test.getLhsExpression().getCardinality())) {
            LabelInfo op0Null = methodInfo.newLabel("op0Null");
            LabelInfo doneNullCheck = methodInfo.newLabel("doneNullCheck");
            ga.loadLocal(op0Var);
            ga.ifNull(op0Null.label());
            ga.loadLocal(op1Var);
            ga.ifNull(returnFalse.label());  // lhs non-null, rhs null
            ga.goTo(doneNullCheck);
            methodInfo.placeLabel(op0Null);
            ga.loadLocal(op1Var);
            ga.ifNull(returnTrue.label());   // lhs null, rhs null
            ga.goTo(returnFalse.label());    // lhs null, rhs non-null
            methodInfo.placeLabel(doneNullCheck);
        }

        if (!test.isKnownToBeComparable()) {
            // Type.isComparable(v0.getPrimitiveType(), v1.getPrimitiveType(), false)
            ga.loadLocal(op0Var);
            ga.invokeInstanceMethod(AtomicValue.class, "getPrimitiveType");
            ga.loadLocal(op1Var);
            ga.invokeInstanceMethod(AtomicValue.class, "getPrimitiveType");
            ga.push(false);
            ga.invokeStaticMethod(Type.class, "isGuaranteedComparable", BuiltInAtomicType.class, BuiltInAtomicType.class, Boolean.TYPE);
            ga.ifFalse(returnFalse);
        }

        allocateStatic(compiler, test.getComparer());
        compiler.generateGetContext();
        ga.invokeInstanceMethod(AtomicComparer.class, "provideContext", XPathContext.class);
        ga.loadLocal(op0Var);
        ga.loadLocal(op1Var);
        ga.invokeInstanceMethod(AtomicComparer.class, "comparesEqual", AtomicValue.class, AtomicValue.class);
        ga.ifFalse(returnFalse);
        methodInfo.placeLabel(returnTrue);
        ga.push(true);
        ga.goTo(done);
        methodInfo.placeLabel(returnFalse);
        ga.push(false);
        methodInfo.placeLabel(done);

        methodInfo.releaseLocal(op0Var);
        methodInfo.releaseLocal(op1Var);

    }
}

//
// Copyright (c) 2011-2022 Saxonica Limited