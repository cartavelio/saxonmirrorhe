package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.NumericType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.*;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Abstract superclass for expression compilers of expressions that return a general sequence.
 * Such compilers must implement the method compileToIterator(). They may optionally implement
 * other methods such as compileToItem().
 */
public abstract class ToIteratorCompiler extends ExpressionCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        // This will only be called if type checking has established that the result will be a singleton.
        // Therefore this method does not need to insert any type-checking code; it simply returns the first
        // item (or null if there is no first item)
        Generator ga = compiler.getCurrentGenerator();
        IteratorDetails details = compiler.compileToIterator(expression);
        ga.invokeInstanceMethod(details.iteratorClass, "next");
        return details.itemClass;
    }

    @Override
    public void compileToPush(CompilerService compiler, final Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ToIteratorCompilerPush");
        compileToIterator(compiler, expression);
        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iterVar);

        LabelInfo done = methodInfo.newLabel("doneToItr");

        LabelInfo loop = methodInfo.placeNewLabel("loop");
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        ga.ifNull(done.label());
        compiler.generateGetOutputter();
        ga.swap();
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.ALL_NAMESPACES);
        ga.invokeInstanceMethod(Outputter.class, "append", Item.class, Location.class, Integer.TYPE);
        ga.goTo(loop);

        methodInfo.placeLabel(done);
        ga.pop();

        methodInfo.releaseLocal(iterVar);

    }

    @Override
    public void compileToBoolean(CompilerService compiler, final Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();

        LabelInfo end = methodInfo.newLabel("endToIterB");
        LabelInfo checkNoMoreItems = methodInfo.newLabel("checkNoMoreItems");
        LabelInfo error = methodInfo.newLabel("error");
        LabelInfo returnFalse = methodInfo.newLabel("false");
        boolean needToCheckForMore = false;

        visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(" + expression + ")");

        // iterVar = expression.iterate()

        compileToIterator(compiler, expression);
        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.dup();
        ga.storeLocal(iterVar);

        // itemVar = iterVar.next();

        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        int itemVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(itemVar);

        // if (itemVar == null) return false

        if (Cardinality.allowsZero(expression.getCardinality())) {
            // assert stack - (empty)
            visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(): test if empty");
            LabelInfo notEmptySequence = methodInfo.newLabel("notEmptySequence");
            ga.loadLocal(itemVar);
            ga.ifNonNull(notEmptySequence.label());
            ga.push(false);
            // assert stack - boolean
            ga.goTo(end);
            // assert stack - (empty)
            methodInfo.placeLabel(notEmptySequence);
        }

        // if (itemVar is boolean) {checkNoMore(); return itemVar.asBoolean()}

        if (th.relationship(expression.getItemType(), BuiltInAtomicType.BOOLEAN) != Affinity.DISJOINT) {
            visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(): test boolean value");
            ga.loadLocal(itemVar);
            LabelInfo notBoolean = methodInfo.newLabel("notBoolean");
            ga.ifNotInstance(BooleanValue.class, notBoolean);
            // assert stack - (empty)
            ga.loadLocal(itemVar);
            ga.checkClass(BooleanValue.class);
            ga.invokeInstanceMethod(BooleanValue.class, "getBooleanValue");
            // assert stack - boolean
            if (Cardinality.allowsMany(expression.getCardinality())) {
                needToCheckForMore = true;
                ga.goTo(checkNoMoreItems);
            } else {
                ga.goTo(end);
            }
            // assert stack - (empty)
            methodInfo.placeLabel(notBoolean);
        }

        // if (itemVar is numeric) {checkNoMore(); return (!itemVar.isNaN() && itemVar != 0}

        if (th.relationship(expression.getItemType(), NumericType.getInstance()) != Affinity.DISJOINT) {
            visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(): test numeric value");
            // assert stack - (empty)
            ga.loadLocal(itemVar);
            LabelInfo notNumeric = methodInfo.newLabel("notNumeric");
            ga.ifNotInstance(NumericValue.class, notNumeric);

            if (th.relationship(expression.getItemType(), BuiltInAtomicType.DOUBLE) != Affinity.DISJOINT ||
                    th.relationship(expression.getItemType(), BuiltInAtomicType.FLOAT) != Affinity.DISJOINT) {

                // assert stack - (empty)
                // check for NaN
                visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(): test for NaN");
                ga.loadLocal(itemVar);
                ga.checkClass(AtomicValue.class);
                ga.invokeInstanceMethod(AtomicValue.class, "isNaN");
                LabelInfo notNaN = methodInfo.newLabel("notNaN");
                ga.ifFalse(notNaN);
                ga.push(false);
                // assert stack - boolean
                if (Cardinality.allowsMany(expression.getCardinality())) {
                    needToCheckForMore = true;
                    ga.goTo(checkNoMoreItems);
                } else {
                    ga.goTo(end);
                }
                methodInfo.placeLabel(notNaN);
            }

            // assert stack - (empty)
            ga.loadLocal(itemVar);
            ga.checkClass(NumericValue.class);
            ga.push(0L);
            ga.invokeInstanceMethod(NumericValue.class, "compareTo", Long.TYPE);
            LabelInfo numericZero = methodInfo.newLabel("numericZero");
            ga.ifZCmp(GeneratorAdapter.EQ, numericZero.label());
            ga.push(true);
            // assert stack - boolean
            if (Cardinality.allowsMany(expression.getCardinality())) {
                needToCheckForMore = true;
                ga.goTo(checkNoMoreItems);
            } else {
                ga.goTo(end);
            }
            // assert stack - (empty)
            methodInfo.placeLabel(numericZero);
            ga.push(false);
            // assert stack - boolean
            if (Cardinality.allowsMany(expression.getCardinality())) {
                needToCheckForMore = true;
                ga.goTo(checkNoMoreItems);
            } else {
                ga.goTo(end);
            }
            // assert stack - (empty)
            methodInfo.placeLabel(notNumeric);
        }

        // if (itemVar is string) {checkNoMore(); return (itemVar.asString().length() != 0}

        if (th.relationship(expression.getItemType(), BuiltInAtomicType.STRING) != Affinity.DISJOINT ||
                th.relationship(expression.getItemType(), BuiltInAtomicType.ANY_URI) != Affinity.DISJOINT ||
                th.relationship(expression.getItemType(), BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT) {
            visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(): test string value");
            ga.loadLocal(itemVar);
            LabelInfo notString = methodInfo.newLabel("notString");
            ga.ifNotInstance(StringValue.class, notString);
            ga.loadLocal(itemVar);
            ga.checkClass(StringValue.class);
            ga.invokeInstanceMethod(StringValue.class, "isEmpty");
            ga.not();
            // assert stack - boolean
            if (Cardinality.allowsMany(expression.getCardinality())) {
                needToCheckForMore = true;
                ga.goTo(checkNoMoreItems);
            } else {
                ga.goTo(end);
            }
            // assert stack - (empty)
            methodInfo.placeLabel(notString);
        }

        // if (itemVar is node) return true;

        Affinity relToNode = th.relationship(expression.getItemType(), AnyNodeTest.getInstance());
        if (relToNode == Affinity.SAME_TYPE || relToNode == Affinity.SUBSUMED_BY) {
            visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(): node exists");
            // assert stack - boolean
            ga.push(true);
            ga.goTo(end);
        } else if (relToNode != Affinity.DISJOINT) {
            visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(): test node existence");
            ga.loadLocal(itemVar);
            ga.ifNotInstance(NodeInfo.class, error);
            ga.push(true);
            // assert stack - boolean
            ga.goTo(end);
        } else {
            // assert stack - (empty)
            ga.goTo(error);
        }

        // checkNoMore: if (iterVar.next() != null) error;

        if (needToCheckForMore) {
            visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(): check no more items");
            // assert stack - boolean
            methodInfo.placeLabel(checkNoMoreItems);
            ga.loadLocal(iterVar);
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            // assert stack - boolean Item
            ga.ifNull(end.label());
            // assert stack - boolean
            ga.pop();
        }
        // assert stack - (empty)
        methodInfo.placeLabel(error);
        compiler.generateDynamicError("Effective boolean value is not defined", "FORG0006", expression.getLocation(), false);

        methodInfo.placeLabel(returnFalse);
        ga.push(false);

        visitAnnotation(compiler, "ToIteratorCompiler.compileToBoolean(): return");
        // assert stack - boolean
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(iterVar);
        methodInfo.releaseLocal(itemVar);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited



