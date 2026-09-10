package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.SubstringAfter;
import net.sf.saxon.lib.SubstringMatcher;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for evaluation of a call to the substring-after() function
 */
public class SubstringAfterCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        final SubstringAfter fn = (SubstringAfter) fnc.getTargetFunction();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "SubstringAfter-str");

        LabelInfo substringReturn = methodInfo.newLabel("substringReturn");
        LabelInfo found = methodInfo.newLabel("found");

        SubstringMatcher stringCollator = (SubstringMatcher)fn.getStringCollator();

        if (stringCollator instanceof CodepointCollator) {
            compiler.compileToPrimitive(fnc.getArg(0), String.class, OnEmpty.RETURN_EMPTY_STRING);
            // assert stack arg0
            ga.dup();
            // assert stack arg0 arg0
            compiler.compileToPrimitive(fnc.getArg(1), String.class, OnEmpty.RETURN_EMPTY_STRING);
            // assert stack arg0 arg0 arg1
            int arg1Var = methodInfo.allocateLocal(String.class);
            ga.storeLocal(arg1Var);
            ga.loadLocal(arg1Var);
            // assert stack arg0 arg0 arg1
            ga.invokeInstanceMethod(String.class, "indexOf", String.class);
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
            ga.invokeInstanceMethod(CharSequence.class, "length");
            // assert stack arg0 start length
            ga.math(GeneratorAdapter.ADD, Type.INT_TYPE);
            // assert stack arg0 start end
            ga.invokeInstanceMethod(String.class, "substring", Integer.TYPE);

            methodInfo.releaseLocal(arg1Var);
        } else {
            // get the collator and place it on the stack
            allocateStatic(compiler, stringCollator);

            compiler.compileToPrimitive(fnc.getArg(0), String.class, OnEmpty.RETURN_EMPTY_STRING);
            compiler.compileToPrimitive(fnc.getArg(1), String.class, OnEmpty.RETURN_EMPTY_STRING);
            ga.invokeInstanceMethod(SubstringMatcher.class, "substringAfter", String.class, String.class);

        }

        methodInfo.placeLabel(substringReturn);

    }

}

//Copyright (c) 2011-2017 Saxonica Limited.
