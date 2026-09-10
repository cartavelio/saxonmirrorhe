package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.NodeInfo;

public class RootFunctionCompiler extends ToItemCompiler {

    @Override
    public void compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        /*
           * NodeInfo start = (NodeInfo)argument[0].evaluateItem(c);
          if (start==null) {
              return null;
          }
          return start.getRoot();
           * */

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Expression arg = ((SystemFunctionCall)expression).getArg(0);

        compiler.compileToItem(arg);
        visitLineNumber(compiler, ga, expression);
        LabelInfo end = methodInfo.newLabel("endRootF");

        LabelInfo notNull = methodInfo.newLabel("rootNotNull");
        ga.dup();
        ga.ifNonNull(notNull.label());
        ga.pop();
        ga.pushNull();
        ga.goTo(end);

        methodInfo.placeLabel(notNull);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getRoot");

        methodInfo.placeLabel(end);

    }

}

// Copyright (c) 2011-2022 Saxonica Limited
