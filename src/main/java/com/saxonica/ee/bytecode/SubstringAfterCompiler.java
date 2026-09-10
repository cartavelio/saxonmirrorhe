package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.SubstringAfter;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.UnicodeString;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for evaluation of a call to the substring-after() function
 */
public class SubstringAfterCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        final SubstringAfter fn = (SubstringAfter) fnc.getTargetFunction();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "SubstringAfter-str");

        LabelInfo substringReturn = methodInfo.newLabel("substringReturn");
        LabelInfo found = methodInfo.newLabel("found");

        SubstringMatcher stringCollator = (SubstringMatcher)fn.getStringCollator();

        if (stringCollator instanceof CodepointCollator) {
            compiler.compileToPrimitive(fnc.getArg(0), UnicodeString.class, OnEmpty.RETURN_EMPTY_STRING);
            // assert stack arg0
            ga.dup();
            // assert stack arg0 arg0
            compiler.compileToPrimitive(fnc.getArg(1), UnicodeString.class, OnEmpty.RETURN_EMPTY_STRING);
            // assert stack arg0 arg0 arg1
            int arg1Var = methodInfo.allocateLocal(UnicodeString.class);
            ga.storeLocal(arg1Var);
            ga.loadLocal(arg1Var);
            ga.push(0L);
            // assert stack arg0 arg0 arg1 0L
            ga.invokeInstanceMethod(UnicodeString.class, "indexOf", UnicodeString.class, Long.TYPE);
            // assert stack arg0 index
            ga.dup();
            // assert stack arg0 index index
            ga.ifZCmp(GeneratorAdapter.GE, found.label());
            // assert stack arg0 start
            ga.pop();
            ga.pop();
            ga.push("");
            ga.goTo(substringReturn);

            methodInfo.placeLabel(found);
            // assert stack arg0 start
            ga.loadLocal(arg1Var);
            ga.invokeInstanceMethod(UnicodeString.class, "length");
            // assert stack arg0 start length
            ga.math(GeneratorAdapter.ADD, Type.LONG_TYPE);
            // assert stack arg0 start end
            ga.invokeInstanceMethod(UnicodeString.class, "substring", Long.TYPE);

            methodInfo.releaseLocal(arg1Var);
        } else {
            // get the collator and place it on the stack
            allocateStatic(compiler, stringCollator);

            compiler.compileToPrimitive(fnc.getArg(0), UnicodeString.class, OnEmpty.RETURN_EMPTY_STRING);
            compiler.compileToPrimitive(fnc.getArg(1), UnicodeString.class, OnEmpty.RETURN_EMPTY_STRING);
            ga.invokeInstanceMethod(SubstringMatcher.class, "substringAfter", UnicodeString.class, UnicodeString.class);

        }

        methodInfo.placeLabel(substringReturn);

    }

}

// Copyright (c) 2011-2020 Saxonica Limited
