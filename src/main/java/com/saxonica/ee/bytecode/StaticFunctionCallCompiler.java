package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.StaticFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Sequence;

import java.util.ArrayList;
import java.util.List;

/**
 * Compiler for a static function call. Not used for calls to system functions if a more
 * specific compiler is available for the function in question.
 */

public class StaticFunctionCallCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler,
                                  /*@NotNull*/ Expression expression) throws CannotCompileException {
        StaticFunctionCall call = (StaticFunctionCall)expression;
        Function function = call.getTargetFunction();
        List<Expression> subExpressions = new ArrayList<Expression>(5);
        for (Operand o: expression.operands()) {
            subExpressions.add(o.getChildExpression());
        }
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "StaticFunctionCallCompiler");
        visitLineNumber(compiler, ga, expression);

        int arrVar = CallableExpressionCompiler.buildArgumentArray(compiler, ga, methodInfo, subExpressions);

        allocateStatic(compiler, function);
        compiler.generateGetContext();
        ga.loadLocal(arrVar);
        ga.invokeInstanceMethod(Function.class, "call", XPathContext.class, Sequence[].class);
        ga.invokeInstanceMethod(Sequence.class, "iterate");
        visitAnnotation(compiler, "StaticFunctionCallCompiler-Done");
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
