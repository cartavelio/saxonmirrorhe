package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.SubscriptExpression;
import net.sf.saxon.expr.TailIterator;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.MemoSequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.GroundedIterator;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.NumericValue;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Bytecode generator for the saxon:item-at() function
 */
public class SubscriptExpressionCompiler extends ToItemCompiler {

    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {

        Expression sequence = ((SubscriptExpression) expression).getBaseExpression();
        Expression index = ((SubscriptExpression) expression).getSubscriptExpression();

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        visitAnnotation(compiler, "itemAt");
        visitLineNumber(compiler, ga, expression);

        if (index instanceof Literal) {
            GroundedValue value = ((Literal) index).getValue();
            if (value instanceof NumericValue && ((NumericValue) value).isWholeNumber()) {
                LabelInfo grounded = methodInfo.newLabel("grounded");
                LabelInfo memo = methodInfo.newLabel("memo");
                LabelInfo end = methodInfo.newLabel("end");
                int i;
                try {
                    i = (int) ((NumericValue) value).longValue();
                } catch (XPathException e) {
                    // we should have dealt with out-of-range numbers earlier
                    throw new AssertionError();
                }
                compiler.compileToIterator(sequence);
                // assert stack SequenceIterator
                ga.dup();
                // assert stack SequenceIterator SequenceIterator
                ga.ifInstance(MemoSequence.ProgressiveIterator.class, memo);
                // assert stack SequenceIterator
                ga.dup();
                // assert stack SequenceIterator SequenceIterator
                ga.invokeInstanceMethod(SequenceIterator.class, "getProperties");
                // assert stack SequenceIterator int
                ga.push(SequenceIterator.GROUNDED);
                // assert stack SequenceIterator int int
                ga.math(GeneratorAdapter.AND, Type.INT_TYPE);
                // assert stack SequenceIterator int
                ga.ifZCmp(GeneratorAdapter.NE, grounded.label());
                // assert stack SequenceIterator
                ga.push(i);
                // assert stack SequenceIterator int
                ga.invokeStaticMethod(TailIterator.class, "make", SequenceIterator.class, Integer.TYPE);
                // assert stack SequenceIterator
                ga.dup();
                // assert stack SequenceIterator SequenceIterator
                ga.invokeInstanceMethod(SequenceIterator.class, "next");
                // assert stack SequenceIterator Item
                ga.swap();
                // assert stack Item SequenceIterator
                ga.invokeInstanceMethod(SequenceIterator.class, "close");
                // assert stack Item
                ga.goTo(end);

                methodInfo.placeLabel(memo);
                // assert stack SequenceIterator
                ga.checkClass(MemoSequence.ProgressiveIterator.class);
                ga.invokeInstanceMethod(MemoSequence.ProgressiveIterator.class, "getMemoSequence");
                // assert stack MemoSequence
                ga.push(i - 1);
                // assert stack MemoSequence int
                ga.invokeInstanceMethod(MemoSequence.class, "itemAt", Integer.TYPE);
                // assert stack Item
                ga.goTo(end);

                methodInfo.placeLabel(grounded);
                ga.checkClass(GroundedIterator.class);
                ga.invokeInstanceMethod(GroundedIterator.class, "materialize");
                ga.push(i - 1);
                ga.invokeInstanceMethod(GroundedValue.class, "itemAt", Integer.TYPE);

                methodInfo.placeLabel(end);
                return;
            }
        }

        LabelInfo returnEmpty = methodInfo.newLabel("returnEmpty");
        LabelInfo popAndReturnEmpty = methodInfo.newLabel("popAndReturnEmpty");
        LabelInfo end = methodInfo.newLabel("end");

        if (th.isSubType(index.getItemType(), BuiltInAtomicType.INTEGER)) {
            IntegerValue[] bounds = index.getIntegerBounds();
            if (bounds != null && bounds[1].compareTo(Integer.MAX_VALUE) <= 0) {
                compiler.compileToPrimitive(index, Integer.TYPE, new OnEmpty.UnwindAndJump(returnEmpty));
            } else {
                compiler.compileToItem(index);
                // Stack: Item
                if (Cardinality.allowsZero(index.getCardinality())) {
                    ga.dup();
                    // Stack: Item Item
                    ga.ifNull(end.label());
                }
                // Stack: Item
                ga.checkClass(NumericValue.class);
                // Stack: NumericValue
                ga.dup();
                // Stack: NumericValue NumericValue
                ga.push((long) Integer.MAX_VALUE);
                // Stack: NumericValue NumericValue long
                ga.invokeInstanceMethod(NumericValue.class, "compareTo", Long.TYPE);
                // Stack: NumericValue int
                ga.ifZCmp(GeneratorAdapter.GT, popAndReturnEmpty.label());
                // Stack: NumericValue
                ga.invokeInstanceMethod(NumericValue.class, "longValue");
                // Stack: long
                ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
                // Stack: int

            }
        } else {
            compiler.compileToItem(index);
            // Stack: Item
            if (Cardinality.allowsZero(index.getCardinality())) {
                ga.dup();
                // Stack: Item Item
                ga.ifNull(end.label());
            }
            // Stack: Item
            ga.checkClass(NumericValue.class);
            // Stack: NumericValue
            ga.dup();
            // Stack: NumericValue NumericValue
            ga.push((long) Integer.MAX_VALUE);
            // Stack: NumericValue NumericValue long
            ga.invokeInstanceMethod(NumericValue.class, "compareTo", Long.TYPE);
            // Stack: NumericValue int
            ga.ifZCmp(GeneratorAdapter.GT, popAndReturnEmpty.label());
            // Stack: NumericValue
            ga.dup();
            // Stack: NumericValue NumericValue
            ga.invokeInstanceMethod(NumericValue.class, "isWholeNumber");
            // Stack: NumericValue int
            ga.ifFalse(popAndReturnEmpty);
            // Stack: NumericValue
            ga.invokeInstanceMethod(NumericValue.class, "longValue");
            // Stack: long
            ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
            // Stack: int
        }

        // We now have an int on the stack containing the evaluated subscript

        LabelInfo nonZero = methodInfo.newLabel("nonZero");
        LabelInfo memo = methodInfo.newLabel("memo");
        LabelInfo grounded = methodInfo.newLabel("grounded");

        ga.push(1);
        ga.math(Generator.SUB, Type.INT_TYPE);
        int indexVar = methodInfo.allocateLocal(Integer.TYPE);
        ga.storeLocal(indexVar);

        compiler.compileToIterator(sequence);

        // Stack: baseIter

        ga.loadLocal(indexVar);
        ga.ifZCmp(GeneratorAdapter.NE, nonZero.label());
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.goTo(end);

        methodInfo.placeLabel(nonZero);

        // Stack: baseIter

        ga.dup();
        // Stack: baseIter baseIter
        ga.ifInstance(MemoSequence.ProgressiveIterator.class, memo);
        // Stack: baseIter
        ga.dup();
        // Stack: baseIter baseIter
        ga.invokeInstanceMethod(SequenceIterator.class, "getProperties");
        // Stack: baseIter int
        ga.push(SequenceIterator.GROUNDED);
        ga.math(GeneratorAdapter.AND, Type.INT_TYPE);
        // Stack: baseIter int
        ga.ifZCmp(GeneratorAdapter.NE, grounded.label());
        // Stack: baseIter
        ga.loadLocal(indexVar);
        ga.push(1);
        ga.math(GeneratorAdapter.ADD, Type.INT_TYPE);
        // Stack: baseIter int
        ga.invokeStaticMethod(TailIterator.class, "make", SequenceIterator.class, Integer.TYPE);
        // Stack: tailIter
        ga.dup();
        // Stack: tailIter tailIter
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        // Stack: tailIter item
        ga.swap();
        // Stack: item tailIter
        ga.invokeInstanceMethod(SequenceIterator.class, "close");
        // Stack: item
        ga.goTo(end);

        methodInfo.placeLabel(popAndReturnEmpty);
        // Stack: NumericValue
        ga.pop();
        methodInfo.placeLabel(returnEmpty);
        // Stack: <empty>
        ga.pushNull();
        // Stack: item(null)
        ga.goTo(end);

        methodInfo.placeLabel(memo);
        // Stack: baseIter
        ga.checkClass(MemoSequence.ProgressiveIterator.class);
        ga.invokeInstanceMethod(MemoSequence.ProgressiveIterator.class, "getMemoSequence");
        // assert stack MemoClosure
        ga.loadLocal(indexVar);
        // Stack: baseIter int
        ga.invokeInstanceMethod(MemoSequence.class, "itemAt", Integer.TYPE);
        // Stack: item
        ga.goTo(end);

        methodInfo.placeLabel(grounded);
        // Stack: baseIter
        ga.checkClass(GroundedIterator.class);
        ga.invokeInstanceMethod(GroundedIterator.class, "materialize");
        // Stack: GroundedValue
        ga.loadLocal(indexVar);
        // Stack: GroundedValue int
        ga.invokeInstanceMethod(GroundedValue.class, "itemAt", Integer.TYPE);
        // Stack: item

        methodInfo.placeLabel(end);
        methodInfo.releaseLocal(indexVar);
    }
}

// Copyright (c) 2018 Saxonica Limited



