package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.IdentityComparison;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for IdentityComparison expression
 */
public class IdentityComparisonCompiler extends ToItemCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        Expression operand0 = ((IdentityComparison) expression).getLhsExpression();
        Expression operand1 = ((IdentityComparison) expression).getRhsExpression();
        boolean generateIdEmulation = ((IdentityComparison) expression).isGenerateIdEmulation();

        int operator = ((IdentityComparison) expression).getOperator();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "IdentityComparison");

        compiler.compileToItem(operand0);
        ga.checkClass(NodeInfo.class);
        int operand0Var = methodInfo.allocateLocal(NodeInfo.class);
        ga.storeLocal(operand0Var);

        LabelInfo notNull0 = methodInfo.newLabel("notNull0");
        LabelInfo notNull1 = methodInfo.newLabel("notNull1");
        LabelInfo end = methodInfo.newLabel("endIdCmp");
        LabelInfo trueLab = methodInfo.newLabel("trueLab");

        if (Cardinality.allowsZero(operand0.getCardinality()) && !generateIdEmulation) {
            ga.loadLocal(operand0Var);
            ga.ifNonNull(notNull0.label());
            ga.pushNull();
            ga.goTo(end);
            methodInfo.placeLabel(notNull0);
        }

        compiler.compileToItem(operand1);
        ga.checkClass(NodeInfo.class);
        int operand1Var = methodInfo.allocateLocal(NodeInfo.class);
        ga.storeLocal(operand1Var);
        visitLineNumber(compiler, ga, expression);
        if (Cardinality.allowsZero(operand1.getCardinality()) && !generateIdEmulation) {
            ga.loadLocal(operand1Var);
            ga.ifNonNull(notNull1.label());
            ga.pushNull();
            ga.goTo(end);
            methodInfo.placeLabel(notNull1);
        }

        if (generateIdEmulation) {
            // if both operands are empty sequences, return true; if one is an empty sequence, return false
            LabelInfo firstIsNull = methodInfo.newLabel("firstIsNull");
            LabelInfo continueLab = methodInfo.newLabel("continueLab");
            ga.loadLocal(operand0Var);
            ga.ifNull(firstIsNull.label());

            ga.loadLocal(operand1Var);
            ga.ifNonNull(continueLab.label());
            ga.getStaticField(BooleanValue.class, "FALSE", BooleanValue.class);
            ga.goTo(end);

            methodInfo.placeLabel(firstIsNull);
            ga.loadLocal(operand1Var);
            ga.ifNull(trueLab.label());

            ga.getStaticField(BooleanValue.class, "FALSE", BooleanValue.class);
            ga.goTo(end);

            methodInfo.placeLabel(continueLab);
        }
        switch (operator) {
            case Token.IS:
                //return BooleanValue.get(compareIdentity(node1, node2));
                ga.loadLocal(operand0Var);
                ga.loadLocal(operand1Var);
                ga.invokeInstanceMethod(NodeInfo.class, "isSameNodeInfo", NodeInfo.class);
                ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
                ga.goTo(end);
                break;
            case Token.PRECEDES:
                //return GlobalOrderComparer.getInstance().compare(node1, node2) < 0;
                ga.invokeStaticMethod(GlobalOrderComparer.class, "getInstance");
                ga.loadLocal(operand0Var);
                ga.loadLocal(operand1Var);
                ga.invokeInstanceMethod(GlobalOrderComparer.class, "compare", NodeInfo.class, NodeInfo.class);
                ga.ifZCmp(GeneratorAdapter.LT, trueLab.label());
                ga.getStaticField(BooleanValue.class, "FALSE", BooleanValue.class);
                ga.goTo(end);
                break;
            case Token.FOLLOWS:
                //return GlobalOrderComparer.getInstance().compare(node1, node2) > 0
                ga.invokeStaticMethod(GlobalOrderComparer.class, "getInstance");
                ga.loadLocal(operand0Var);
                ga.loadLocal(operand1Var);
                ga.invokeInstanceMethod(GlobalOrderComparer.class, "compare", NodeInfo.class, NodeInfo.class);
                ga.ifZCmp(GeneratorAdapter.GT, trueLab.label());
                ga.getStaticField(BooleanValue.class, "FALSE", BooleanValue.class);
                ga.goTo(end);
                break;
            default:
                throw new UnsupportedOperationException("Unknown node identity test");
        }
        methodInfo.placeLabel(trueLab);
        ga.getStaticField(BooleanValue.class, "TRUE", BooleanValue.class);

        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(operand0Var);
        methodInfo.releaseLocal(operand1Var);
        return BooleanValue.class;
    }

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        Expression operand0 = ((IdentityComparison) expression).getLhsExpression();
        Expression operand1 = ((IdentityComparison) expression).getRhsExpression();
        boolean generateIdEmulation = ((IdentityComparison) expression).isGenerateIdEmulation();

        int operator = ((IdentityComparison) expression).getOperator();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "IdentityComparison-Boolean");

        compiler.compileToItem(operand0);
        ga.checkClass(NodeInfo.class);
        int operand0Var = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(operand0Var);

        LabelInfo notNull0 = methodInfo.newLabel("notNull0");
        LabelInfo notNull1 = methodInfo.newLabel("notNull1");
        LabelInfo end = methodInfo.newLabel("endIdCmp");
        LabelInfo trueLab = methodInfo.newLabel("trueLab");

        if (Cardinality.allowsZero(operand0.getCardinality()) && !generateIdEmulation) {
            ga.loadLocal(operand0Var);
            ga.ifNonNull(notNull0.label());
            ga.push(false);
            ga.goTo(end);
            methodInfo.placeLabel(notNull0);
        }

        compiler.compileToItem(operand1);
        ga.checkClass(NodeInfo.class);
        int operand1Var = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(operand1Var);

        if (Cardinality.allowsZero(operand1.getCardinality()) && !generateIdEmulation) {
            ga.loadLocal(operand1Var);
            ga.ifNonNull(notNull1.label());
            ga.push(false);
            ga.goTo(end);
            methodInfo.placeLabel(notNull1);
        }

        if (generateIdEmulation) {
            // if both operands are empty sequences, return true; if one is an empty sequence, return false
            LabelInfo firstIsNull = methodInfo.newLabel("firstIsNull");
            LabelInfo continueLab = methodInfo.newLabel("continueLab");
            ga.loadLocal(operand0Var);
            ga.ifNull(firstIsNull.label());

            ga.loadLocal(operand1Var);
            ga.ifNonNull(continueLab.label());
            ga.push(false);
            ga.goTo(end);

            methodInfo.placeLabel(firstIsNull);
            ga.loadLocal(operand1Var);
            ga.ifNull(trueLab.label());
            ga.push(false);
            ga.goTo(end);

            methodInfo.placeLabel(continueLab);
        }

        switch (operator) {
            case Token.IS:
                // return node0.isSameNodeInfo(node1);
                ga.loadLocal(operand0Var);
                ga.loadLocal(operand1Var);
                ga.invokeInstanceMethod(NodeInfo.class, "isSameNodeInfo", NodeInfo.class);
                ga.goTo(end);
                break;
            case Token.PRECEDES:
                //return GlobalOrderComparer.getInstance().compare(node0, node1) < 0;
                ga.invokeStaticMethod(GlobalOrderComparer.class, "getInstance");
                ga.loadLocal(operand0Var);
                ga.loadLocal(operand1Var);
                ga.invokeInstanceMethod(GlobalOrderComparer.class, "compare", NodeInfo.class, NodeInfo.class);
                ga.ifZCmp(GeneratorAdapter.LT, trueLab.label());
                ga.push(false);
                ga.goTo(end);
                break;
            case Token.FOLLOWS:
                //return GlobalOrderComparer.getInstance().compare(node0, node1) > 0
                ga.invokeStaticMethod(GlobalOrderComparer.class, "getInstance");
                ga.loadLocal(operand0Var);
                ga.loadLocal(operand1Var);
                ga.invokeInstanceMethod(GlobalOrderComparer.class, "compare", NodeInfo.class, NodeInfo.class);
                ga.ifZCmp(GeneratorAdapter.GT, trueLab.label());
                ga.push(false);
                ga.goTo(end);
                break;
            default:
                throw new UnsupportedOperationException("Unknown node identity test");
        }
        methodInfo.placeLabel(trueLab);
        ga.push(true);

        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(operand0Var);
        methodInfo.releaseLocal(operand1Var);
    }


}

// Copyright (c) 2011-2022 Saxonica Limited
