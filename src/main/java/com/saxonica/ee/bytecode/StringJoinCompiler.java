package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.StringJoin;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.value.StringValue;

public class StringJoinCompiler extends ToStringCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        Expression expr0 = fnc.getArg(0);
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "StringJoin-push");
        LabelInfo notInstanceOf = methodInfo.newLabel("notInstanceOf");
        LabelInfo end = methodInfo.newLabel("endSJ");
        LabelInfo notNull = methodInfo.newLabel("strJnotNull");
        //if (out instanceOf ComplexContentOutputter)
        compiler.generateGetOutputter();
        ga.ifNotInstance(ComplexContentOutputter.class, notInstanceOf);
        compiler.generateGetOutputter();
        ga.getStaticField(StringValue.class, "EMPTY_STRING", StringValue.class);
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "append", Item.class, Location.class, Integer.TYPE);
        visitAnnotation(compiler, "StringJoin-iterate-first-arg-enter");
        compiler.compileToIterator(expr0);
        visitAnnotation(compiler, "StringJoin-iterate-first-arg-exit");
        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iterVar);
        visitLineNumber(compiler, ga, expression);
        ga.loadLocal(iterVar);
        ga.showObject(compiler, "SequenceIterator (SJ arg 0) A: ");
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        int itVar = methodInfo.allocateLocal(Item.class);
        ga.storeLocal(itVar);
        ga.loadLocal(itVar);
        ga.ifNull(end.label());
        ga.loadLocal(itVar);

        visitAnnotation(compiler, "StringJoin-process-first");
        ga.invokeInstanceMethod(Item.class, "getStringValueCS");
        int firstVar = methodInfo.allocateLocal(CharSequence.class);
        ga.storeLocal(firstVar);
        compiler.generateGetOutputter();
        ga.loadLocal(firstVar);
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "characters", CharSequence.class, Location.class, Integer.TYPE);

        ga.loadLocal(iterVar);
        ga.showObject(compiler, "SequenceIterator (SJ arg 0) B: ");
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.storeLocal(itVar);
        ga.loadLocal(itVar);
        ga.ifNonNull(notNull.label());
        visitAnnotation(compiler, "StringJoin-second-item-is-null");
        compiler.generateGetOutputter();
        ga.getStaticField(StringValue.class, "EMPTY_STRING", StringValue.class);
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "append", Item.class, Location.class, Integer.TYPE);
        ga.goTo(end);

        methodInfo.placeLabel(notNull);
        visitAnnotation(compiler, "StringJoin-process-second");
        if (fnc.getArity() == 1) {
            ga.loadLocal(itVar);
            ga.invokeInstanceMethod(Item.class, "getStringValueCS");
            compiler.generateGetOutputter();
            ga.swap();
            allocateStatic(compiler, expression.getLocation());
            ga.push(ReceiverOption.NONE);
            ga.invokeInstanceMethod(Outputter.class, "characters", CharSequence.class, Location.class, Integer.TYPE);

            LabelInfo loop = methodInfo.placeNewLabel("loop");
            ga.loadLocal(iterVar);
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            ga.storeLocal(itVar);
            ga.loadLocal(itVar);
            ga.ifNull(end.label());
            visitAnnotation(compiler, "StringJoin-process-next");
            ga.loadLocal(itVar);
            ga.invokeInstanceMethod(Item.class, "getStringValueCS");
            compiler.generateGetOutputter();
            ga.swap();
            allocateStatic(compiler, expression.getLocation());
            ga.push(ReceiverOption.NONE);
            ga.invokeInstanceMethod(Outputter.class, "characters", CharSequence.class, Location.class, Integer.TYPE);
            ga.goTo(loop);
        } else {
            compiler.compileToPrimitive(fnc.getArg(1), CharSequence.class, OnEmpty.RETURN_EMPTY_STRING);
            visitAnnotation(compiler, "StringJoin-push-else");
            int sepVar = methodInfo.allocateLocal(CharSequence.class);
            ga.storeLocal(sepVar);
            compiler.generateGetOutputter();
            ga.loadLocal(sepVar);
            allocateStatic(compiler, expression.getLocation());
            ga.push(ReceiverOption.NONE);
            ga.invokeInstanceMethod(Outputter.class, "characters", CharSequence.class, Location.class, Integer.TYPE);
            compiler.generateGetOutputter();
            ga.loadLocal(itVar);
            ga.invokeInstanceMethod(Item.class, "getStringValueCS");
            allocateStatic(compiler, expression.getLocation());
            ga.push(ReceiverOption.NONE);
            ga.invokeInstanceMethod(Outputter.class, "characters", CharSequence.class, Location.class, Integer.TYPE);


            LabelInfo loop = methodInfo.placeNewLabel("loop");
            ga.loadLocal(iterVar);
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            ga.storeLocal(itVar);
            ga.loadLocal(itVar);
            ga.ifNull(end.label());
            compiler.generateGetOutputter();
            ga.loadLocal(sepVar);
            allocateStatic(compiler, expression.getLocation());
            ga.push(ReceiverOption.NONE);
            ga.invokeInstanceMethod(Outputter.class, "characters", CharSequence.class, Location.class, Integer.TYPE);
            compiler.generateGetOutputter();
            ga.loadLocal(itVar);
            ga.invokeInstanceMethod(Item.class, "getStringValueCS");
            allocateStatic(compiler, expression.getLocation());
            ga.push(ReceiverOption.NONE);
            ga.invokeInstanceMethod(Outputter.class, "characters", CharSequence.class, Location.class, Integer.TYPE);
            ga.goTo(loop);

            methodInfo.releaseLocal(sepVar);

        }
        methodInfo.placeLabel(notInstanceOf);
        compiler.generateGetOutputter();
        compileToItem(compiler, expression);
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "append", Item.class, Location.class, Integer.TYPE);

        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(iterVar);
        methodInfo.releaseLocal(itVar);
        methodInfo.releaseLocal(firstVar);

    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        Expression expr0 = fnc.getArg(0);
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "StringJoin-Item");
        LabelInfo end = methodInfo.newLabel("endSJ");
        LabelInfo notNull = methodInfo.newLabel("strJnotNull");
        LabelInfo notNull2 = methodInfo.newLabel("strJnotNull2");

        compiler.compileToIterator(expr0);
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "StringJoin-Item-1");
        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iterVar);
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        ga.ifNonNull(notNull.label());
        visitAnnotation(compiler, "StringJoin-Item-1-null");
        ga.pop();
        if (((StringJoin)fnc.getTargetFunction()).isReturnEmptyIfEmpty()) {
            onEmpty.generate(ga);
        } else {
            ga.push("");
        }
        ga.goTo(end);

        methodInfo.placeLabel(notNull);
        ga.invokeInstanceMethod(Item.class, "getStringValueCS");
        int firstVar = methodInfo.allocateLocal(CharSequence.class);
        ga.storeLocal(firstVar);

        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        ga.ifNonNull(notNull2.label());
        ga.pop();
        ga.loadLocal(firstVar);
        //invokeStatic(ga,StringValue.class, "makeStringValue", CharSequence.class);
        ga.goTo(end);

        methodInfo.placeLabel(notNull2);
        ga.newInstance(FastStringBuffer.class);
        ga.dup();
        ga.push(FastStringBuffer.C64);
        ga.invokeConstructor(FastStringBuffer.class, Integer.TYPE);
        int sbVar = methodInfo.allocateLocal(FastStringBuffer.class);
        ga.storeLocal(sbVar);
        ga.loadLocal(sbVar);
        ga.loadLocal(firstVar);
        ga.invokeInstanceMethod(FastStringBuffer.class, "append", CharSequence.class);

        LabelInfo done = methodInfo.newLabel("doneSJ");
        int sepVar = -1;
        if (fnc.getArity() == 1) {
            ga.loadLocal(sbVar);
            ga.swap();
            ga.invokeInstanceMethod(Item.class, "getStringValueCS");
            ga.invokeInstanceMethod(FastStringBuffer.class, "append", CharSequence.class);
        } else {
            compiler.compileToPrimitive(((SystemFunctionCall) expression).getArg(1), CharSequence.class, OnEmpty.RETURN_EMPTY_STRING);
            sepVar = methodInfo.allocateLocal(CharSequence.class);
            ga.storeLocal(sepVar);
            ga.loadLocal(sbVar);
            ga.loadLocal(sepVar);
            ga.invokeInstanceMethod(FastStringBuffer.class, "append", CharSequence.class);
            ga.loadLocal(sbVar);
            ga.swap();
            ga.invokeInstanceMethod(Item.class, "getStringValueCS");
            ga.invokeInstanceMethod(FastStringBuffer.class, "append", CharSequence.class);
        }
        //int itVar = methodInfo.allocateLocal(Item.class);
        LabelInfo loop = methodInfo.placeNewLabel("loop");
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        // assert stack : item
        //ga.storeLocal(itVar);
        //ga.loadLocal(itVar);
        ga.dup();
        ga.ifNull(done.label());
        // assert stack : item
        ga.loadLocal(sbVar);
        // assert stack : item buffer
        if (sepVar >= 0) {
            ga.loadLocal(sepVar);
            // assert stack : item buffer separator
            ga.invokeInstanceMethod(FastStringBuffer.class, "append", CharSequence.class);
            ga.loadLocal(sbVar);
            // assert stack : item buffer
        }
        ga.swap();
        // assert stack : buffer item
        ga.invokeInstanceMethod(Item.class, "getStringValueCS");
        // assert stack : buffer string
        ga.invokeInstanceMethod(FastStringBuffer.class, "append", CharSequence.class);
        // assert stack : (empty)
        ga.goTo(loop);

        methodInfo.placeLabel(done);
        ga.pop();
        ga.loadLocal(sbVar);
        ga.invokeInstanceMethod(FastStringBuffer.class, "condense");
        //ga.invokeStaticMethod(StringValue.class, "makeStringValue", CharSequence.class);

        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(iterVar);
        //methodInfo.releaseLocal(itVar);
        methodInfo.releaseLocal(sbVar);
        methodInfo.releaseLocal(sepVar);

        if (requiredClass == String.class) {
            ga.invokeInstanceMethod(Object.class, "toString");
        }
    }

}

// Copyright (c) 2011-2020 Saxonica Limited
