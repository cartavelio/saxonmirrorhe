package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.value.DateTimeValue;
import net.sf.saxon.value.DateValue;
import net.sf.saxon.value.TimeValue;

/**
 * Compile the fn:dateTime() function to Java bytecode
 */
public class DateTimeConstructorCompiler extends ToItemCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "DateTimeConstructor-Item");
//
//        AtomicValue arg0 = (AtomicValue)argument[0].evaluateItem(context);
//        AtomicValue arg1 = (AtomicValue)argument[1].evaluateItem(context);
//        return DateTimeValue.makeDateTimeValue((DateValue)arg0, (TimeValue)arg1);

        Class<? extends Item> op0class = compiler.compileToItem(fnc.getArg(0));
        ga.downCast(op0class, DateValue.class);
        Class<? extends Item> op1class = compiler.compileToItem(fnc.getArg(1));
        ga.downCast(op1class, TimeValue.class);
        ga.invokeStaticMethod(DateTimeValue.class, "makeDateTimeValue", DateValue.class, TimeValue.class);
        return DateTimeValue.class;
    }

}

// Copyright (c) 2011-2022 Saxonica Limited


