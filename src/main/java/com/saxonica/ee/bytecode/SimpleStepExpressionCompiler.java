package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.AxisExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SlashExpression;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.Cardinality;


/**
 * Generate bytecode for a call to the SimpleStepExpression, which is a SlashExpression in which the
 * lhs operand is known to return a singleton and the rhs operand is known to be an AxisExpression
 */
public class SimpleStepExpressionCompiler extends ToIteratorCompiler {

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                             Expression expression) throws CannotCompileException {

        Expression start = ((SlashExpression) expression).getSelectExpression();
        Expression step = ((SlashExpression) expression).getActionExpression();
        final int axis = ((AxisExpression) step).getAxis();
        NodeTest test = ((AxisExpression) step).getNodeTest();

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "SimpleStepExpression-Itr");
        visitLineNumber(compiler, ga, expression);

        compiler.compileToItem(start);

        if (Cardinality.allowsZero(start.getCardinality())) {
            LabelInfo end = methodInfo.newLabel("endStep");

            LabelInfo notNull = methodInfo.newLabel("notnull");
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
            ga.goTo(end);

            methodInfo.placeLabel(notNull);
            ga.checkClass(NodeInfo.class);
            AxisExpressionCompiler.iterate(compiler, axis, test);

            methodInfo.placeLabel(end);

        } else {
            ga.checkClass(NodeInfo.class);
            AxisExpressionCompiler.iterate(compiler, axis, test);
        }
        return new IteratorDetails(SequenceIterator.class, NodeInfo.class);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
