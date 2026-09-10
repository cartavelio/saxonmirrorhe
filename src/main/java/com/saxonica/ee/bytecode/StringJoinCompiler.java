package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.PushableFunction;
import net.sf.saxon.functions.StringJoin;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import org.objectweb.asm.Type;

public class StringJoinCompiler extends ToStringCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        // Generate call on ((PushableFunction)target).process(output, context, actualArgs);

        ga.push(2);
        ga.newArray(Type.getType(Sequence.class));
        int arrVar = methodInfo.allocateLocal(Sequence[].class);
        ga.storeLocal(arrVar);

        ga.loadLocal(arrVar);
        ga.push(0);
        compiler.compileToIterator(fnc.getArg(0));
        ga.invokeStaticMethod(SequenceTool.class, "toLazySequence", SequenceIterator.class);
        ga.arrayStore(Type.getType(Sequence.class));

        ga.loadLocal(arrVar);
        ga.push(1);
        compiler.compileToItem(fnc.getArg(1));
        ga.arrayStore(Type.getType(Sequence.class));

        allocateStatic(compiler, fnc.getTargetFunction());
        ga.checkClass(PushableFunction.class);
        compiler.generateGetOutputter();
        compiler.generateGetContext();
        ga.loadLocal(arrVar);

        ga.invokeInstanceMethod(PushableFunction.class, "process",
                                Outputter.class, XPathContext.class, Sequence[].class);

        methodInfo.releaseLocal(arrVar);

    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {
        assert requiredClass == String.class;
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
        ga.invokeInstanceMethod(Item.class, "getStringValue");
        int firstVar = methodInfo.allocateLocal(String.class);
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
        ga.newInstance(StringBuilder.class);
        ga.dup();
        ga.push(64);
        ga.invokeConstructor(StringBuilder.class, Integer.TYPE);
        int sbVar = methodInfo.allocateLocal(StringBuilder.class);
        ga.storeLocal(sbVar);
        ga.loadLocal(sbVar);
        ga.loadLocal(firstVar);
        ga.invokeInstanceMethod(StringBuilder.class, "append", String.class);

        LabelInfo done = methodInfo.newLabel("doneSJ");
        int sepVar = -1;
        if (fnc.getArity() == 1) {
            ga.loadLocal(sbVar);
            ga.swap();
            ga.invokeInstanceMethod(Item.class, "getStringValueCS");
            ga.invokeInstanceMethod(StringBuilder.class, "append", String.class);
        } else {
            compiler.compileToPrimitive(((SystemFunctionCall) expression).getArg(1), String.class, OnEmpty.RETURN_EMPTY_STRING);
            sepVar = methodInfo.allocateLocal(String.class);
            ga.storeLocal(sepVar);
            ga.loadLocal(sbVar);
            ga.loadLocal(sepVar);
            ga.invokeInstanceMethod(StringBuilder.class, "append", String.class);
            ga.loadLocal(sbVar);
            ga.swap();
            ga.invokeInstanceMethod(Item.class, "getStringValue");
            ga.invokeInstanceMethod(StringBuilder.class, "append", String.class);
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
            ga.invokeInstanceMethod(StringBuilder.class, "append", String.class);
            ga.loadLocal(sbVar);
            // assert stack : item buffer
        }
        ga.swap();
        // assert stack : buffer item
        ga.invokeInstanceMethod(Item.class, "getStringValue");
        // assert stack : buffer string
        ga.invokeInstanceMethod(StringBuilder.class, "append", String.class);
        // assert stack : (empty)
        ga.goTo(loop);

        methodInfo.placeLabel(done);
        ga.pop();
        ga.loadLocal(sbVar);
        ga.invokeInstanceMethod(StringBuilder.class, "toString");
        //ga.invokeStaticMethod(StringValue.class, "makeStringValue", CharSequence.class);

        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(iterVar);
        //methodInfo.releaseLocal(itVar);
        methodInfo.releaseLocal(sbVar);
        methodInfo.releaseLocal(sepVar);

    }

}

// Copyright (c) 2011-2020 Saxonica Limited
