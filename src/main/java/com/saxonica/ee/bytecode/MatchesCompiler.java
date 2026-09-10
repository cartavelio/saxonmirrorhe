package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.Matches;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

/**
 * Generate code for the fn:matches function
 */
public class MatchesCompiler extends ToBooleanCompiler {

    /**
     * Generate bytecode to evaluate the expression as a boolean
     * Precondition: none.
     * Postcondition: at execution time, the stack contains an integer (0=false, 1=true) representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {

        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        Matches fn = (Matches) fnc.getTargetFunction();
        Expression arg0 = fnc.getArg(0);
        Expression arg1 = fnc.getArg(1);
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        RegularExpression re = fn.getStaticRegex();
        if (re == null) {
            // return fn.evalMatches(sv0, pat, flags, c)
            LabelInfo end = methodInfo.newLabel("endMatches");
            LabelInfo notNull = methodInfo.newLabel("notNull");
            LabelInfo notNull1 = methodInfo.newLabel("notNull1");
            allocateStatic(compiler, fn);
            compiler.compileToItem(arg0);
            ga.checkClass(AtomicValue.class);
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            allocateStatic(compiler, StringValue.EMPTY_STRING);
            methodInfo.placeLabel(notNull);
            compiler.compileToItem(arg1);
            ga.dup();
            ga.ifNonNull(notNull1.label());
            ga.pop();
            ga.pop();
            ga.pop();
            ga.push(false);
            ga.goTo(end);
            methodInfo.placeLabel(notNull1);
            ga.checkClass(AtomicValue.class);
            if (fn.getArity() == 2) {
                ga.push("");
            } else {
                compiler.compileToPrimitive(fnc.getArg(2), UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            }
            compiler.generateGetContext();
            ga.invokeInstanceMethod(Matches.class, "evalMatches", AtomicValue.class, AtomicValue.class, UnicodeString.class, XPathContext.class);
            methodInfo.placeLabel(end);
        } else {
            // return re.containsMatch(sv0.getStringValueCS())
            allocateStatic(compiler, re);
            compiler.compileToPrimitive(arg0, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            ga.invokeInstanceMethod(RegularExpression.class, "containsMatch", UnicodeString.class);
        }

    }
}

// Copyright (c) 2011-2020 Saxonica Limited