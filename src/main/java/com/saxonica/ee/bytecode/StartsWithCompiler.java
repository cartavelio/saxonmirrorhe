package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.CollatingFunctionFixed;
import net.sf.saxon.functions.StartsWith;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.commons.GeneratorAdapter;

import java.util.Stack;

/**
 * Generate bytecode for a call on fn:starts-with or fn:ends-with, with a statically known collation
 */

public class StartsWithCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        CollatingFunctionFixed function = (CollatingFunctionFixed) fnc.getTargetFunction();
        boolean isStartsWith = function instanceof StartsWith;
        Expression arg0 = fnc.getArg(0);
        Expression arg1 = fnc.getArg(1);
        SubstringMatcher stringCollator = (SubstringMatcher)function.getStringCollator();
        Generator ga = compiler.getCurrentGenerator();


        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, isStartsWith ? "StartsWithCompiler" : "EndsWithCompiler");
        visitLineNumber(compiler, ga, expression);
        LabelInfo end = methodInfo.newLabel(isStartsWith ? "endStartsWith" : "endEndsWith");
        LabelInfo returnTrue = methodInfo.newLabel("returnTrue");
        LabelInfo returnFalse = methodInfo.newLabel("returnFalse");
        LabelInfo returnTrue1 = methodInfo.newLabel("returnTrue1");
        LabelInfo returnFalse1 = methodInfo.newLabel("returnFalse1");
        Stack<Class> unwindStack = new Stack<>();

        if (!(stringCollator instanceof CodepointCollator)) {
            allocateStatic(compiler, stringCollator);
            unwindStack.push(StringCollator.class);
            //Stack: SubstringMatcher
        }

        compiler.compileToPrimitive(arg1, String.class, new OnEmpty.UnwindAndJump(unwindStack, returnTrue));
        unwindStack.push(String.class);
        if (Cardinality.allowsZero(arg1.getCardinality())) {
            ga.dup();
            //Stack: SubstringMatcher? String String
            ga.ifNull(returnTrue1.label());
        }

        if (!(arg1 instanceof StringLiteral && !((StringLiteral) arg1).getStringValue().isEmpty())) {
            ga.dup();
            //Stack: SubstringMatcher? String String
            ga.invokeInstanceMethod(String.class, "length");
            //Stack: SubstringMatcher? String int
            ga.ifZCmp(GeneratorAdapter.EQ, returnTrue1.label());
        }
        compiler.compileToPrimitive(arg0, String.class, new OnEmpty.UnwindAndJump(unwindStack, returnFalse));
        unwindStack.push(String.class);
        if (Cardinality.allowsZero(arg1.getCardinality())) {
            ga.dup();
            //Stack: SubstringMatcher? String String String
            ga.ifNull(returnFalse1.label());
        }
        if (!(arg1 instanceof StringLiteral && !((StringLiteral) arg1).getStringValue().isEmpty())) {
            ga.dup();
            //Stack: SubstringMatcher? String String String
            ga.invokeInstanceMethod(String.class, "length");
            //Stack: SubstringMatcher? String String int
            ga.ifZCmp(GeneratorAdapter.EQ, returnFalse1.label());
        }
        ga.swap();

        // assert stack StringCollator[optional], String(arg0), String(arg1)

        if (stringCollator instanceof CodepointCollator) {

            if (isStartsWith) {
                // call s0.startsWith(s1, 0) because that's exactly what s0.startsWith(s1) does
                ga.push(0);
                ga.invokeInstanceMethod(String.class, "startsWith", String.class, Integer.TYPE);
            } else {
                ga.invokeInstanceMethod(String.class, "endsWith", String.class);
            }
            ga.goTo(end);

        } else {

            // Either a non-standard collator was specified statically, or the collator is determined dynamically
            // The StringCollator was added to the stack earlier
            // StringCollator collator = getCollator(2, context);

            ga.invokeInstanceMethod(SubstringMatcher.class, isStartsWith ? "startsWith" : "endsWith", String.class, String.class);
            ga.goTo(end);
        }
        methodInfo.placeLabel(returnTrue1);
        //Stack: SubstringMatcher? String
        if (!(stringCollator instanceof CodepointCollator)) {
            ga.pop();
        }
        ga.pop();
        ga.goTo(returnTrue);
        methodInfo.placeLabel(returnFalse1);
        ga.pop();
        if (!(stringCollator instanceof CodepointCollator)) {
            ga.pop();
        }
        ga.pop();
        methodInfo.placeLabel(returnFalse);
        ga.push(false);
        ga.goTo(end);
        methodInfo.placeLabel(returnTrue);
        ga.push(true);
        methodInfo.placeLabel(end);
    }

}


// Copyright (c) 2011-2020 Saxonica Limited
