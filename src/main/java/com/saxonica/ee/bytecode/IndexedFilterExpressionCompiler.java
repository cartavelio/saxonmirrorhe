package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import com.saxonica.ee.optim.IndexedFilterExpression;
import com.saxonica.ee.optim.IndexedValue;
import com.saxonica.ee.optim.SearchableValue;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Actor;
import net.sf.saxon.expr.instruct.Bindery;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.EmptySequence;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

public class IndexedFilterExpressionCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        IndexedFilterExpression indexedFilter = (IndexedFilterExpression) expression;
        Expression base = indexedFilter.getBaseExpression();
        if (!(base instanceof VariableReference)) {
            throw new CannotCompileException("IndexedFilterExpression has been rewritten", false);
        }
        VariableReference start = (VariableReference)base;
        VariableReferenceCompiler.genEvaluateVariable(compiler, ga, start);
//        visitAnnotation(compiler, "IndexedFilterExpressionCompiler-Itr");
//        ExpressionCompiler.allocateStatic(compiler, start);
//        compiler.generateGetContext();
//        ga.invokeInstanceMethod(VariableReference.class, "evaluateVariable", XPathContext.class);
        int baseVar = methodInfo.allocateLocal(GroundedValue.class);
        ga.storeLocal(baseVar);
        LabelInfo notEmpty = methodInfo.newLabel("IXFnotEmpty");
        LabelInfo isSearchableValue = methodInfo.newLabel("isIdxVal");
        LabelInfo end = methodInfo.newLabel("endIxFlt");
        LabelInfo notZero = methodInfo.newLabel("notZero");
        ga.loadLocal(baseVar);
        ga.ifNotInstance(EmptySequence.class, notEmpty);
        ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
        ga.checkClass(SequenceIterator.class);
        ga.goTo(end);

        methodInfo.placeLabel(notEmpty);
        ga.loadLocal(baseVar);
        ga.ifInstance(SearchableValue.class, isSearchableValue);

        //indexedValue = SearchableValue.makeSearchableValue(base.iterate());

        ga.loadLocal(baseVar);
        ga.invokeInstanceMethod(Sequence.class, "iterate");
        ga.invokeStaticMethod(SearchableValue.class, "makeSearchableValue", SequenceIterator.class);
        ga.storeLocal(baseVar);

        Binding binding = start.getBinding();
        if (binding instanceof LocalBinding) {
            compiler.generateGetContext();
            ga.push(((LocalBinding)binding).getLocalSlotNumber());
            ga.loadLocal(baseVar);
            ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);

        } else if (binding instanceof GlobalVariable) {
//            context.getController().getBindery(((GlobalVariable) binding).getPackageData())
//                    .setGlobalVariable((GlobalVariable) binding, indexedValue);
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getController");
            allocateStatic(compiler, binding);
            ga.invokeInstanceMethod(Actor.class, "getPackageData");
            ga.invokeInstanceMethod(Controller.class, "getBindery", PackageData.class);
            allocateStatic(compiler, binding);
            ga.checkClass(GlobalVariable.class);
            ga.loadLocal(baseVar);
            ga.invokeInstanceMethod(Bindery.class, "setGlobalVariable", GlobalVariable.class, GroundedValue.class);

        } else {
            ga.newInstance(Type.getType(IndexedValue.class));
            ga.dup();
            ga.loadLocal(baseVar);
            ga.invokeInstanceMethod(Sequence.class, "iterate");
            ga.invokeConstructor(IndexedValue.class, SequenceIterator.class);
            ga.storeLocal(baseVar);
        }
        methodInfo.placeLabel(isSearchableValue);
        ga.loadLocal(baseVar);
        ga.checkClass(SearchableValue.class);
//        int startValueVar = methodInfo.allocateLocal(IndexedValue.class);
//        ga.storeLocal(startValueVar);
//        ga.loadLocal(baseVar);
        ga.invokeInstanceMethod(SearchableValue.class, "getLength");
        ga.push(0);
        ga.ifICmp(GeneratorAdapter.NE, notZero.label());
        ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
        ga.goTo(end);

        methodInfo.placeLabel(notZero);
        Expression use = indexedFilter.getUseExpression();
        Expression valExpr = indexedFilter.getSearchExpression();
//        return indexedValue.findItems(getUseExpression(), values, convertUntypedToOther,
//                                      false, comparer, context);
        ga.loadLocal(baseVar);
        ga.checkClass(SearchableValue.class);
        allocateStatic(compiler, use);
        compiler.compileToIterator(valExpr);
        ga.push(indexedFilter.isConvertUntypedToOther());
        ga.push(false);
        AtomicComparer comparer = indexedFilter.getComparer();
        allocateStatic(compiler, comparer);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(AtomicComparer.class, "provideContext", XPathContext.class);
        //		int comparer_var =  ga.newLocal(Type.getType(AtomicComparer.class));
        //		ga.storeLocal(comparer_var);
        compiler.generateGetContext();
        // abstract SequenceIterator findItems(Expression use, SequenceIterator<? extends AtomicValue> values,
        //                                               boolean convertUntypedToOther,
        //                                               boolean firstOnly, AtomicComparer comparer, XPathContext context) throws XPathException;
        ga.invokeInstanceMethod(SearchableValue.class, "findItems", Expression.class, SequenceIterator.class, Boolean.TYPE, Boolean.TYPE, AtomicComparer.class, XPathContext.class);
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(baseVar);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited

