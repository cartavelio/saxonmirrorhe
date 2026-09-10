package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.NumericType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.*;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Abstract superclass for expression compilers that handle expressions returning a single item (or nothing).
 * The compilers for such expressions must implement the compileToItem() method; they may optionally implement
 * other methods such as compileToIterator().
 */
public abstract class ToItemCompiler extends ExpressionCompiler {

    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileToItem(compiler, expression);
        Generator ga = compiler.getCurrentGenerator();
        ga.invokeStaticMethod(SingletonIterator.class, "makeIterator", Item.class);
    }

    @Override
    public void compileToBoolean(CompilerService compiler, final Expression expression) throws CannotCompileException {
        visitAnnotation(compiler, "ToItemCompiler-Boolean");
        compileToItem(compiler, expression);
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        int itemVar = methodInfo.allocateLocal(Item.class);
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        ga.storeLocal(itemVar);
        LabelInfo lab = methodInfo.newLabel("lab");
        LabelInfo end = methodInfo.newLabel("endToitem");
        if (Cardinality.allowsZero(expression.getCardinality())) {

            ga.loadLocal(itemVar);
            ga.ifNonNull(lab.label());
            ga.push(0);
            ga.goTo(end);
            methodInfo.placeLabel(lab);
        }

        if (th.relationship(expression.getItemType(), BuiltInAtomicType.BOOLEAN) != Affinity.DISJOINT) {
            ga.loadLocal(itemVar);
            ga.instanceOf(org.objectweb.asm.Type.getType(BooleanValue.class));
            LabelInfo notBoolean = methodInfo.newLabel("notBoolean");
            ga.ifZCmp(GeneratorAdapter.EQ, notBoolean.label());
            ga.loadLocal(itemVar);
            ga.checkClass(BooleanValue.class);
            ga.invokeInstanceMethod(BooleanValue.class, "getBooleanValue");
            ga.goTo(end);
            methodInfo.placeLabel(notBoolean);
        }

        if (th.relationship(expression.getItemType(), NumericType.getInstance()) != Affinity.DISJOINT) {
            ga.loadLocal(itemVar);
            ga.instanceOf(org.objectweb.asm.Type.getType(NumericValue.class));

            LabelInfo notNumeric = methodInfo.newLabel("notNumeric");
            ga.ifFalse(notNumeric);

            ga.loadLocal(itemVar);
            ga.checkClass(AtomicValue.class);
            ga.invokeInstanceMethod(AtomicValue.class, "effectiveBooleanValue");
            ga.goTo(end);
            methodInfo.placeLabel(notNumeric);
        }

        if (th.relationship(expression.getItemType(), BuiltInAtomicType.STRING) != Affinity.DISJOINT
                || th.relationship(expression.getItemType(), BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT
                || th.relationship(expression.getItemType(), BuiltInAtomicType.ANY_URI) != Affinity.DISJOINT) {
            ga.loadLocal(itemVar);
            ga.instanceOf(org.objectweb.asm.Type.getType(StringValue.class));
            LabelInfo notString = methodInfo.newLabel("notString");
            ga.ifZCmp(GeneratorAdapter.EQ, notString.label());
            ga.loadLocal(itemVar);
            ga.checkClass(StringValue.class);
            ga.invokeInstanceMethod(StringValue.class, "isEmpty");
            ga.not();
            ga.goTo(end);
            methodInfo.placeLabel(notString);
        }

        if (th.relationship(expression.getItemType(), AnyNodeTest.getInstance()) != Affinity.DISJOINT) {
            ga.loadLocal(itemVar);
            ga.instanceOf(org.objectweb.asm.Type.getType(NodeInfo.class));
            LabelInfo notNode = methodInfo.newLabel("notNode");
            ga.ifZCmp(GeneratorAdapter.EQ, notNode.label());
            ga.push(true);
            ga.goTo(end);
            methodInfo.placeLabel(notNode);
        }
        compiler.generateDynamicError(
                "Effective boolean value is not defined for an item other than a boolean, number, string, or URI",
            "FORG0006", expression.getLocation(), false);
        //ga.push(false);
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(itemVar);

    }


    @Override
    public void compileToPush(CompilerService compiler, final Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "ToItemCompilerPush");
        compiler.generateGetOutputter();
        compileToItem(compiler, expression);
        ga.invokeInstanceMethod(Outputter.class, "append", Item.class);

    }

    /**
     * Utility method for use by subclasses to generate code that causes an empty string to be returned
     * if the first argument is an empty sequence. The actual return value depends on the onEmpty
     * argument.
     * Precondition: the value of the first argument is on the stack, either as a string or as an item
     * Postcondition: either the control has jumped to the return label with a zero-length string on the
     * stack, or control drops through with the value of the first argument still on the stack (as supplied).
     *  @param arg         the first argument
     * @param ga          the Generator
     * @param methodInfo  the method information
     * @param returnLabel the label to jump to on completion
     * @param extraPops   items (other than the string being tested) that need to be popped of the stack if jumping to
     * @param onEmpty     the way in which the returned empty string is to be represented
     */

    protected void treatAsEmptyIfFirstArgumentIsEmpty(Expression arg, Generator ga, GeneratedMethodInfo methodInfo, LabelInfo returnLabel, int extraPops, OnEmpty onEmpty) {
        if (Cardinality.allowsZero(arg.getCardinality())) {
            LabelInfo stringArgNonNull = methodInfo.newLabel("stringArgNonNull");
            ga.dup();
            ga.ifNonNull(stringArgNonNull.label());
            for (int i = 0; i < extraPops + 1; i++) {
                ga.pop();
            }
            onEmpty.generate(ga);
            ga.goTo(returnLabel);
            methodInfo.placeLabel(stringArgNonNull);
        }
    }

}

// Copyright (c) 2011-2022 Saxonica Limited



