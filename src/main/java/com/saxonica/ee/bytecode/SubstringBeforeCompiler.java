package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.SubstringBefore;
import net.sf.saxon.lib.SubstringMatcher;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for evaluation of a call to the substring-before() function
 */
public class SubstringBeforeCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        final SubstringBefore fn = (SubstringBefore) fnc.getTargetFunction();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "SubstringBefore-str");

        LabelInfo substringReturn = methodInfo.newLabel("substringReturn");
        LabelInfo found = methodInfo.newLabel("found");

        SubstringMatcher stringCollator = (SubstringMatcher)fn.getStringCollator();

        if (stringCollator instanceof CodepointCollator) {

            compiler.compileToPrimitive(fnc.getArg(0), String.class, OnEmpty.RETURN_EMPTY_STRING);
            ga.dup();
            // assert stack arg0 arg0
            compiler.compileToPrimitive(fnc.getArg(1), String.class, OnEmpty.RETURN_EMPTY_STRING);
            // assert stack arg0 arg0 arg1
            ga.invokeInstanceMethod(String.class, "indexOf", String.class);
            // assert stack arg0 start
            ga.dup();
            ga.ifZCmp(GeneratorAdapter.GT, found.label());
            // assert stack arg0 start
            ga.pop();
            ga.pop();
            ga.push("");
            ga.goTo(substringReturn);

            methodInfo.placeLabel(found);
            // assert stack arg0 start
            ga.push(0);
            ga.swap();
            // assert stack arg0 0 start
            ga.invokeInstanceMethod(String.class, "substring", Integer.TYPE, Integer.TYPE);

        } else {

            // Either a non-standard collator was specified statically, or the collator is determined dynamically
            allocateStatic(compiler, stringCollator);

            compiler.compileToPrimitive(fnc.getArg(0), String.class, OnEmpty.RETURN_EMPTY_STRING);
            compiler.compileToPrimitive(fnc.getArg(1), String.class, OnEmpty.RETURN_EMPTY_STRING);

            ga.invokeInstanceMethod(SubstringMatcher.class, "substringBefore", String.class, String.class);
        }

        methodInfo.placeLabel(substringReturn);

    }

}

// Copyright (c) 2011-2020 Saxonica Limited
