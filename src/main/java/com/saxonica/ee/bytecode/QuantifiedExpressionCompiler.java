package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.QuantifiedExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;

/**
 * Compile the QuantifiedExpression to Java bytecode
 */
public class QuantifiedExpressionCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        /*
           *   // First create an iteration of the base sequence.

          SequenceIterator base = sequence.iterate(context);

          // Now test to see if some or all of the tests are true. The same
          // logic is used for the SOME and EVERY operators

          final boolean some = (operator==Token.SOME);
          int slot = getLocalSlotNumber();
          while (true) {
              final Item it = base.next();
              if (it == null) {
                  break;
              }
              context.setLocalVariable(slot, it);
              if (some == action.effectiveBooleanValue(context)) {
                  base.close();
                  return some;
              }
          }
          return !some;
           * */
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "QuantifiedExpression-Boolean");
        QuantifiedExpression quanExpr = (QuantifiedExpression) expression;
        Expression base = quanExpr.getSequence();
        Expression action = quanExpr.getAction();
        boolean some = (quanExpr.getOperator() == Token.SOME);
        int slotNum = quanExpr.getLocalSlotNumber();
        compiler.compileToIterator(base);
        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iterVar);
        LabelInfo end = methodInfo.newLabel("endQF");
        LabelInfo notSatisfied = methodInfo.newLabel("notSatisfied");
        LabelInfo done = methodInfo.newLabel("doneQF");

        LabelInfo loop = methodInfo.placeNewLabel("loop");
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        ga.ifNull(done.label());
        compiler.generateGetContext();
        ga.swap();
        ga.push(slotNum);
        ga.swap();
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        compiler.compileToBoolean(action);
        if (some) {
            ga.ifFalse(notSatisfied);
        } else {
            ga.ifTrue(notSatisfied);
        }
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "close");
        ga.push(some);
        ga.goTo(end);

        methodInfo.placeLabel(notSatisfied);
        ga.goTo(loop);

        methodInfo.placeLabel(done);
        ga.pop();
        ga.push(!some);
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(iterVar);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
