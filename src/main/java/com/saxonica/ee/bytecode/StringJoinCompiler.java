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
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.StringValue;
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
        if (fnc.getArity() > 1) {
            compiler.compileToItem(fnc.getArg(1));
        } else {
            allocateStatic(compiler, StringValue.EMPTY_STRING);
        }
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
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {

        // The logic here is designed to avoid creating a string builder, and evaluating the separator argument,
        // in the common case where the first argument turns out to be a singleton. For example this happens
        // with <xsl:value-of select="$x"/> if the type of $x is undeclared and cannot be inferred.

        // The code is generic, relying on the fact that StringBuilder and UnicodeBuilder have very similar methods.

        // Rewritten in v11 to use StringBuilder rather than FastStringBuffer. This simplifies the logic because
        // after an append() operation, the StringBuilder stays on the stack.

        assert requiredClass == String.class || requiredClass == UnicodeString.class;
        boolean unicode = requiredClass == UnicodeString.class;
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        Expression expr0 = fnc.getArg(0);
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "StringJoin-Item");
        LabelInfo end = methodInfo.newLabel("endSJ");
        LabelInfo nonEmptyInput = methodInfo.newLabel("strJnotNull");

        compiler.compileToIterator(expr0);
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "StringJoin-Item-1");
        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iterVar);
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        ga.ifNonNull(nonEmptyInput.label());
        visitAnnotation(compiler, "StringJoin-Item-1-null");
        ga.pop();
        if (((StringJoin)fnc.getTargetFunction()).isReturnEmptyIfEmpty()) {
            onEmpty.generate(ga);
        } else if (unicode) {
            ga.invokeStaticMethod(EmptyUnicodeString.class, "getInstance");
        } else {
            ga.push("");
        }
        ga.goTo(end);

        methodInfo.placeLabel(nonEmptyInput);
        // Stack: firstItem

        String stringValueMethod = unicode ? "getUnicodeStringValue" : "getStringValue";
        Class<?> stringClass = unicode ? UnicodeString.class : String.class;
        Class<?> builderClass = unicode ? UnicodeBuilder.class : StringBuilder.class;
        String builderToStringMethod = unicode ? "toUnicodeString" : "toString";

        ga.invokeInstanceMethod(Item.class, stringValueMethod);
        // Stack: firstUString

        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        // Stack: firstUString secondItem secondItem

        LabelInfo moreThanOne = methodInfo.newLabel("moreThanOne");
        ga.ifNonNull(moreThanOne.label());
        // Stack: firstUString null
        ga.pop();
        ga.goTo(end);

        methodInfo.placeLabel(moreThanOne);
        // Stack: firstUString secondItem
        ga.swap();
        // Stack: secondItem firstUString

        ga.newInstance(builderClass);
        ga.dup();
        ga.push(64);
        ga.invokeConstructor(builderClass, Integer.TYPE);

        // Stack: secondItem firstUString builder
        ga.swap();
        // Stack: secondItem builder firstUString
        ga.invokeInstanceMethod(builderClass, "append", stringClass);
        // Stack: secondItem builder

        if (fnc.getArity() == 2) {
            // there is a separator
            int sepVar = methodInfo.allocateLocal(stringClass);
            compiler.compileToPrimitive(fnc.getArg(1), stringClass, unicode ? OnEmpty.RETURN_EMPTY_UNICODE_STRING : OnEmpty.RETURN_EMPTY_STRING);
            ga.storeLocal(sepVar);
            // Stack: secondItem builder

            LabelInfo separatorLoop = methodInfo.placeNewLabel("separatorLoop");
            // Stack: nextItem builder
            ga.loadLocal(sepVar);
            // Stack: nextItem builder sepUString
            ga.invokeInstanceMethod(builderClass, "append", stringClass);
            // Stack: nextItem builder
            ga.swap();
            // Stack: builder nextItem
            ga.invokeInstanceMethod(Item.class, stringValueMethod);
            // Stack: builder nextUString
            ga.invokeInstanceMethod(builderClass, "append", stringClass);
            // Stack: builder
            ga.loadLocal(iterVar);
            // Stack: builder iterator
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            // Stack: builder nextItem
            ga.dupX1();
            // Stack: nextItem builder nextItem
            ga.ifNonNull(separatorLoop.label());
            // Stack: null builder
            ga.swap();
            ga.pop();
            ga.invokeInstanceMethod(builderClass, builderToStringMethod);

        } else {
            // there is no separator
            // Stack: secondItem builder
            ga.swap();
            // Stack: builder secondItem

            LabelInfo simpleLoop = methodInfo.placeNewLabel("simpleLoop");
            // Stack: builder nextItem
            ga.invokeInstanceMethod(Item.class, stringValueMethod);
            // Stack: builder nextUString
            ga.invokeInstanceMethod(builderClass, "append", stringClass);
            // Stack: builder
            ga.loadLocal(iterVar);
            ga.invokeInstanceMethod(SequenceIterator.class, "next");
            // Stack: builder nextItem
            ga.dup();
            ga.ifNonNull(simpleLoop.label());

            // Stack: builder null
            ga.pop();
            ga.invokeInstanceMethod(builderClass, builderToStringMethod);

        }
        // Stack: UString
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(iterVar);
        //methodInfo.releaseLocal(itVar);


    }


}

// Copyright (c) 2011-2022 Saxonica Limited
