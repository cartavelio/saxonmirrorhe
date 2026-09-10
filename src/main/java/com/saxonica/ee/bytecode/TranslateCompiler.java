package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.z.IntToIntMap;
import net.sf.saxon.functions.Translate;
import net.sf.saxon.value.StringItem;

/**
 * Generate bytecode for evaluation of a call to the translate() function
 */
public class TranslateCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        final Translate fn = (Translate) fnc.getTargetFunction();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Translate-str");

        LabelInfo translateReturn = methodInfo.newLabel("translateReturn");

        compiler.compileToItem(fnc.getArg(0));
        returnZeroLengthStringIfNull(fnc.getArg(0), ga, methodInfo, translateReturn, 0);
        ga.checkClass(StringItem.class);

        IntToIntMap staticMap = fn.getStaticMap();
        if (staticMap != null) {
            allocateStatic(compiler, staticMap);
            ga.invokeStaticMethod(Translate.class, "translateUsingMap", StringItem.class, IntToIntMap.class);
        } else {
            compiler.compileToItem(fnc.getArg(1));
            ga.checkClass(StringItem.class);
            compiler.compileToItem(fnc.getArg(2));
            ga.checkClass(StringItem.class);
            ga.invokeStaticMethod(Translate.class, "translate", StringItem.class, StringItem.class, StringItem.class);
        }
        if (requiredClass == String.class) {
            ga.invokeInstanceMethod(CharSequence.class, "toString");
        }

        methodInfo.placeLabel(translateReturn);
    }

}

// Copyright (c) 2011-2020 Saxonica Limited
