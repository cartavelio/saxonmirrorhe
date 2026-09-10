package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.AxisExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.TypeHierarchy;

/**
 * Generate bytecode for evaluation of an AxisExpression
 */
public class AxisExpressionCompiler extends ToIteratorCompiler {

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler, final Expression expression) throws CannotCompileException {
        final int axis = ((AxisExpression) expression).getAxis();
        NodeTest test = ((AxisExpression) expression).getNodeTest();

        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "AxisExpressionCompiler - iterate");
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
        LabelInfo end = null;
        if (((AxisExpression) expression).isContextPossiblyUndefined()) {
            LabelInfo lab = methodInfo.newLabel("lab");
            end = methodInfo.newLabel("endAxis");
            ga.dup();
            ga.ifNonNull(lab.label());
            // Throw error - context item is undefined
            compiler.generateDynamicError("The context item for axis step " + expression.toString() + " is absent", "XPDY0002", expression.getLocation(), false);
            methodInfo.placeLabel(lab);
        }
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        ItemType contextItemType = ((AxisExpression) expression).getContextItemType();
        if (contextItemType == null || !th.isSubType(((AxisExpression) expression).getContextItemType(), AnyNodeTest.getInstance())) {
            LabelInfo OK = methodInfo.newLabel("endTryAxisExpr");
            LabelInfo L0 = methodInfo.newLabel("L0");
            LabelInfo L1 = methodInfo.newLabel("L1");
            LabelInfo L2 = methodInfo.newLabel("L2");
            ga.visitTryCatchBlock(L0, L1, L2, "java/lang/ClassCastException");
            /* try block*/
            methodInfo.placeLabel(L0);
            ga.checkClass(NodeInfo.class);
            iterate(compiler, axis, test);
            methodInfo.placeLabel(L1);
            ga.goTo(OK.label());
            /*catch block*/
            methodInfo.placeLabel(L2);
            visitAnnotation(compiler, "Catch");
            compiler.generateDynamicError("The context item for axis step " + expression.toString() + " is not a node", "XPTY0004", expression.getLocation(), true);
            methodInfo.placeLabel(OK);
        } else {
            ga.checkClass(NodeInfo.class);
            iterate(compiler, axis, test);
        }
        if (end != null) {
            methodInfo.placeLabel(end);
        }
        return new IteratorDetails(AxisIterator.class, NodeInfo.class);
    }

    /**
     * Generate code to iterate over the selected axis.
     * Entry condition: the node forming the origin for the iteration is on the bytecode stack
     * Exit condition: the iterator for the axisExpression is on the bytecode stack
     *
     * @param compiler the compiler service
     * @param axis the axis to be iterated over
     * @param test the node test to be satisfied; possibly null if all nodes are selected
     */

    public static void iterate(CompilerService compiler, final int axis, NodeTest test) {
        Generator ga = compiler.getCurrentGenerator();

        visitAnnotation(compiler, "AxisExpressionCompiler - static method: iterate");
        if (test == null || test == AnyNodeTest.getInstance()) {
            ga.push(axis);
            ga.invokeInstanceMethod(NodeInfo.class, "iterateAxis", Integer.TYPE);

        } else if (test instanceof NodeKindTest) {
            int kind = test.getPrimitiveType();
            ga.push(axis);
            switch (kind) {
                case Type.DOCUMENT:
                    ga.getStaticField(NodeKindTest.class, "DOCUMENT", NodeKindTest.class);
                    break;
                case Type.ELEMENT:
                    ga.getStaticField(NodeKindTest.class, "ELEMENT", NodeKindTest.class);
                    break;
                case Type.ATTRIBUTE:
                    ga.getStaticField(NodeKindTest.class, "ATTRIBUTE", NodeKindTest.class);
                    break;
                case Type.TEXT:
                    ga.getStaticField(NodeKindTest.class, "TEXT", NodeKindTest.class);
                    break;
                case Type.COMMENT:
                    ga.getStaticField(NodeKindTest.class, "COMMENT", NodeKindTest.class);
                    break;
                case Type.PROCESSING_INSTRUCTION:
                    ga.getStaticField(NodeKindTest.class, "PROCESSING_INSTRUCTION", NodeKindTest.class);
                    break;
                case Type.NAMESPACE:
                    ga.getStaticField(NodeKindTest.class, "NAMESPACE", NodeKindTest.class);
                    break;
                default:
                    throw new AssertionError();
            }
            ga.invokeInstanceMethod(NodeInfo.class, "iterateAxis", Integer.TYPE, NodePredicate.class);

        } else {

            ga.push(axis);
            allocateStatic(compiler, test);
            ga.invokeInstanceMethod(NodeInfo.class, "iterateAxis", Integer.TYPE, NodePredicate.class);
        }
    }

}

// Copyright (c) 2011-2022 Saxonica Limited


