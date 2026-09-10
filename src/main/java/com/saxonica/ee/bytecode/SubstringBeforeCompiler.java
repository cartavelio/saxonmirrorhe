package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.SubstringBefore;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for evaluation of a call to the substring-before() function
 */
public class SubstringBeforeCompiler extends ToStringCompiler {

    @Override
    public void compileToUnicodeString(CompilerService compiler, Expression expression, OnEmpty onEmpty)
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

            compiler.compileToPrimitive(fnc.getArg(0), UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            ga.dup();
            // assert stack arg0 arg0
            compiler.compileToPrimitive(fnc.getArg(1), UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            // assert stack arg0 arg0 arg1

            ga.push(0L);
            ga.invokeInstanceMethod(UnicodeString.class, "indexOf", UnicodeString.class, Long.TYPE);
            // assert stack arg0 start(long)

            ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
            // assert stack arg0 start(int)
            ga.dup();
            // assert stack arg0 start(int) start(int)
            ga.ifZCmp(GeneratorAdapter.GT, found.label());
            // assert stack arg0 start(int)
            ga.pop();
            ga.pop();
            ga.invokeStaticMethod(EmptyUnicodeString.class, "getInstance");
            ga.goTo(substringReturn);

            methodInfo.placeLabel(found);
            // assert stack arg0 start(int)
            ga.cast(Type.INT_TYPE, Type.LONG_TYPE);
            ga.invokeInstanceMethod(UnicodeString.class, "prefix", Long.TYPE);
            // assert stack substring (as UnicodeString)

        } else {

            // Either a non-standard collator was specified statically, or the collator is determined dynamically
            allocateStatic(compiler, stringCollator);

            compiler.compileToPrimitive(fnc.getArg(0), UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            compiler.compileToPrimitive(fnc.getArg(1), UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);

            ga.invokeInstanceMethod(SubstringMatcher.class, "substringBefore", UnicodeString.class, UnicodeString.class);

        }

        methodInfo.placeLabel(substringReturn);

    }

}

// Copyright (c) 2011-2022 Saxonica Limited
