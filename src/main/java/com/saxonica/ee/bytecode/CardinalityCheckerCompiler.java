package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.TypeCheckingFilter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.DocumentNodeTest;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.type.ItemType;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

public class CardinalityCheckerCompiler extends ToIteratorCompiler {


    @Override
    public void compileToItem(CompilerService compiler,
                              Expression expression) throws CannotCompileException {

        Expression operand = ((CardinalityChecker) expression).getBaseExpression();
        int requiredCardinality = ((CardinalityChecker) expression).getRequiredCardinality();
        RoleDiagnostic role = ((CardinalityChecker) expression).getRoleLocator();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "CardinalityCheckerCompiler-Item");

        compiler.compileToIterator(operand);
        ga.checkClass(SequenceIterator.class);
        if (requiredCardinality == StaticProperty.EMPTY) {
            LabelInfo returnNull = methodInfo.newLabel("returnNull00");
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            ga.ifNull(returnNull.label());
            compiler.generateDynamicError("An empty sequence is the only allowed value for the " + role.getMessage(),
                    role.getErrorCode(), expression.getLocation(), true);
            methodInfo.placeLabel(returnNull);
            ga.pushNull();

        } else if (requiredCardinality == StaticProperty.ALLOWS_ZERO_OR_ONE) {
            LabelInfo returnFirst = methodInfo.newLabel("returnFirst01");
            ga.dup();
            // assert stack: iterator iterator
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            // assert stack: iterator firstItem
            ga.swap();
            // assert stack: firstItem iterator
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            // assert stack: firstItem secondItem
            ga.ifNull(returnFirst.label());
            // report error: multiple items found
            allocateStatic(compiler, expression);
            compiler.generateGetContext();
            ga.invokeStaticMethod(Callback.class, "makeMultipleItemsException", CardinalityChecker.class, XPathContext.class);
            ga.throwException();
            methodInfo.placeLabel(returnFirst);
            // assert stack: firstItem

        } else if (requiredCardinality == StaticProperty.ALLOWS_ONE) {
            LabelInfo notEmpty = methodInfo.newLabel("notEmpty11");
            LabelInfo returnFirst = methodInfo.newLabel("returnFirst11");
            ga.dup();
            // assert stack: iterator iterator
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            // assert stack: iterator firstItem
            ga.dup();
            // assert stack: iterator firstItem firstItem
            ga.ifNonNull(notEmpty.label());
            compiler.generateDynamicError("An empty sequence is not allowed as the " + role.getMessage(),
                    role.getErrorCode(), expression.getLocation(), true);
            methodInfo.placeLabel(notEmpty);
            // assert stack: iterator firstItem
            ga.swap();
            // assert stack: firstItem iterator
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            // assert stack: firstItem secondItem
            ga.ifNull(returnFirst.label());
            // report error: multiple items found
            allocateStatic(compiler, expression);
            compiler.generateGetContext();
            ga.invokeStaticMethod(Callback.class, "makeMultipleItemsException", CardinalityChecker.class, XPathContext.class);
            ga.throwException();
            methodInfo.placeLabel(returnFirst);
            // assert stack: firstItem

        } else {
            throw new IllegalStateException("CardinalityChecker: compileToItem called when multiple items allowed");
        }

    }

    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        Expression base = ((CardinalityChecker) expression).getBaseExpression();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        int requiredCardinality = ((CardinalityChecker) expression).getRequiredCardinality();
        RoleDiagnostic role = ((CardinalityChecker) expression).getRoleLocator();

        visitAnnotation(compiler, "CardinalityCheckerCompiler-Itr");
        visitLineNumber(compiler, ga, expression);

        // If the base iterator knows how many items there are, then check it now rather than wasting time

//        if ((base.getProperties() & SequenceIterator.LAST_POSITION_FINDER) != 0) {
//            int count = ((LastPositionFinder)base).getLength();
//            if (count == 0 && !Cardinality.allowsZero(requiredCardinality)) {
//                typeError("An empty sequence is not allowed as the " +
//                             role.getMessage(), role.getErrorCode(), context);
//            } else if (count == 1 && requiredCardinality == StaticProperty.EMPTY) {
//                typeError("The only value allowed for the " +
//                             role.getMessage() + " is an empty sequence", role.getErrorCode(), context);
//            } else if (count > 1 && !Cardinality.allowsMany(requiredCardinality)) {
//                typeError("A sequence of more than one item is not allowed as the " +
//                                role.getMessage() + depictSequenceStart(base.getAnother(), 2),
//                           role.getErrorCode(), context);
//            }
//            return base;
//        }

        LabelInfo notLastFinder = methodInfo.newLabel("notLastFinder");
        LabelInfo ok = methodInfo.newLabel("cardinalityOK");

        compiler.compileToIterator(base);
        // assert stack : iterator
        if (CountCompiler.maybeLastPositionFinder(base)) {
            ga.dup();
            // assert stack : iterator iterator
            ga.ifNotInstance(LastPositionFinder.class, notLastFinder);
            // assert stack : iterator iterator
            ga.dup();
            ga.checkClass(LastPositionFinder.class);
            // assert stack : iterator iterator
            ga.invokeInstanceMethod(LastPositionFinder.class, "supportsGetLength");
            ga.ifFalse(notLastFinder);
            // assert stack : iterator
            
            ga.dup();
            // assert stack : iterator iterator
            ga.checkClass(LastPositionFinder.class);
            ga.invokeInstanceMethod(LastPositionFinder.class, "getLength");
            // assert stack : iterator length
            if (requiredCardinality == StaticProperty.EMPTY) {
                ga.ifZCmp(GeneratorAdapter.EQ, ok.label());
                compiler.generateDynamicError("An empty sequence is the only allowed value for the " + role.getMessage(),
                        role.getErrorCode(), expression.getLocation(), true);
            } else if (requiredCardinality == StaticProperty.ALLOWS_ZERO_OR_ONE) {
                ga.push(1);
                ga.ifICmp(GeneratorAdapter.LE, ok.label());
                // report error: multiple items found
                allocateStatic(compiler, expression);
                compiler.generateGetContext();
                ga.invokeStaticMethod(Callback.class, "makeMultipleItemsException", CardinalityChecker.class, XPathContext.class);
                ga.throwException();
            } else if (requiredCardinality == StaticProperty.ALLOWS_ONE) {
                LabelInfo notEmpty = methodInfo.newLabel("cardinalityNotEmpty");
                LabelInfo notOk = methodInfo.newLabel("cardinalityNotOK");
                ga.dup();
                // assert stack : iterator length length
                ga.push(1);
                // assert stack : iterator length length 1
                ga.ifICmp(GeneratorAdapter.NE, notOk.label());
                // assert stack : iterator length
                ga.pop();
                // assert stack : iterator
                ga.goTo(ok);
                methodInfo.placeLabel(notOk);
                // assert stack : iterator length
                ga.ifZCmp(GeneratorAdapter.NE, notEmpty.label());
                compiler.generateDynamicError("An empty sequence is not allowed as the " + role.getMessage(),
                        role.getErrorCode(), expression.getLocation(), true);
                methodInfo.placeLabel(notEmpty);
                // report error: multiple items found
                allocateStatic(compiler, expression);
                compiler.generateGetContext();
                ga.invokeStaticMethod(Callback.class, "makeMultipleItemsException", CardinalityChecker.class, XPathContext.class);
                ga.throwException();
            } else if (requiredCardinality == StaticProperty.ALLOWS_ONE_OR_MORE) {
                ga.ifZCmp(GeneratorAdapter.NE, ok.label());
                compiler.generateDynamicError("An empty sequence is not allowed as the " + role.getMessage(),
                        role.getErrorCode(), expression.getLocation(), true);
            } else {
                throw new IllegalStateException("Unexpected required cardinality " + requiredCardinality);
            }
            methodInfo.placeLabel(notLastFinder);
        }

        // Otherwise return an iterator that does the checking on the fly

        // assert stack : baseIterator
        ga.newInstance(Type.getType(CardinalityCheckingIterator.class));
        // assert stack : baseIterator checkingIterator
        ga.swap();
        // assert stack : checkingIterator baseIterator
        ga.dup2();
        // assert stack : checkingIterator baseIterator checkingIterator baseIterator

        ga.push(requiredCardinality);
        allocateStatic(compiler, role);
        allocateStatic(compiler, expression.getLocation());
        ga.invokeConstructor(CardinalityCheckingIterator.class, SequenceIterator.class, Integer.TYPE, RoleDiagnostic.class, Location.class);

        // assert stack : checkingIterator baseIterator
        ga.pop();

        // assert stack : checkingIterator
        methodInfo.placeLabel(ok);
        ga.checkClass(SequenceIterator.class);

    }

    @Override
    public void compileToPush(/*@NotNull*/ CompilerService compiler, Expression expression) throws CannotCompileException {
        CardinalityChecker cc = (CardinalityChecker) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Expression next = cc.getBaseExpression();
        ItemType type = net.sf.saxon.type.Type.ITEM_TYPE;
        if (next instanceof ItemChecker) {
            type = ((ItemChecker) next).getRequiredType();
            next = ((ItemChecker) next).getBaseExpression();
        }
        if ((next.getImplementationMethod() & Expression.PROCESS_METHOD) != 0 && !(type instanceof DocumentNodeTest)) {
            ga.newInstance(TypeCheckingFilter.class);
            ga.dup();
            //Stack: TypeCheckingFilter TypeCheckingFilter
            compiler.generateGetOutputter();
            //Stack: TypeCheckingFilter TypeCheckingFilter Receiver
            ga.invokeConstructor(TypeCheckingFilter.class, Outputter.class);
            //Stack: TypeCheckingFilter
            int filterVar = methodInfo.allocateLocal(TypeCheckingFilter.class);
            ga.storeLocal(filterVar);
            ga.loadLocal(filterVar);
            //Stack: TypeCheckingFilter
            allocateStatic(compiler, type);
            ga.push(cc.getRequiredCardinality());
            allocateStatic(compiler, cc.getRoleLocator());
            allocateStatic(compiler, cc.getLocation());
            ga.invokeInstanceMethod(TypeCheckingFilter.class, "setRequiredType", ItemType.class, Integer.TYPE, RoleDiagnostic.class, Location.class);
            ga.loadLocal(filterVar);
            //Stack: Receiver
            compiler.pushNewOutputterInfo(ga);

            compiler.compileToPush(next);
            ga.loadLocal(filterVar);
            ga.invokeInstanceMethod(TypeCheckingFilter.class, "finalCheck");
            compiler.popOutputterInfo();

        } else {
            super.compileToPush(compiler, expression);
        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
