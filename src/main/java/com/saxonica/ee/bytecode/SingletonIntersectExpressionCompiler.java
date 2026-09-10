package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SingletonIntersectExpression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;

/**
 * Bytecode generation for an expression of the form (A intersect B) where A is a singleton
 */
@SuppressWarnings("ALL")
public class SingletonIntersectExpressionCompiler extends ToIteratorCompiler {

    /**
     * Generate bytecode to evaluate the expression as a {@link net.sf.saxon.om.SequenceIterator}
     * Precondition: none.
     * Postcondition: at execution time, the stack contains a SequenceIterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        final SingletonIntersectExpression sie = (SingletonIntersectExpression) expression;

        visitAnnotation(compiler, "SingletonIntersectionCompiler-Itr");
        visitLineNumber(compiler, ga, expression);
//        NodeInfo m = (NodeInfo)operand0.evaluateItem(c);
//        if (m == null) {
//            return EmptyIterator.getInstance();
//        }
//        SequenceIterator iter = operand1.iterate(c);
//        while (true) {
//            NodeInfo n = (NodeInfo)iter.next();
//            if (n == null) {
//                return EmptyIterator.getInstance();
//            }
//            if (n.isSameNodeInfo(m)) {
//                return SingletonIterator.makeIterator(m);
//            }
//        }

        compiler.compileToItem(sie.getLhsExpression());
        LabelInfo end = methodInfo.newLabel("SIEend");
        if (Cardinality.allowsZero(sie.getLhsExpression().getCardinality())) {
            LabelInfo notNull = methodInfo.newLabel("SIEnotNull");
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
            ga.goTo(end);
            methodInfo.placeLabel(notNull);
        }
        int mVar = methodInfo.allocateLocal(NodeInfo.class);
        ga.checkClass(NodeInfo.class);
        ga.storeLocal(mVar);

        compiler.compileToIterator(sie.getRhsExpression());
        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iterVar);

        LabelInfo loop = methodInfo.placeNewLabel("SIEloop");
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        LabelInfo notNull2 = methodInfo.newLabel("SIEnotNull2");
        ga.ifNonNull(notNull2.label());
        ga.pop();
        ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
        ga.goTo(end);

        methodInfo.placeLabel(notNull2);
        ga.loadLocal(mVar);
        ga.invokeInstanceMethod(NodeInfo.class, "isSameNodeInfo", NodeInfo.class);
        ga.ifFalse(loop);

        ga.loadLocal(mVar);
        ga.invokeStaticMethod(SingletonIterator.class, "makeIterator", Item.class);
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(mVar);
        methodInfo.releaseLocal(iterVar);
    }


}


// Copyright (c) 2011-2022 Saxonica Limited