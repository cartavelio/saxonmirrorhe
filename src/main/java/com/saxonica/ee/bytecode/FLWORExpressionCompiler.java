package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.flwor.WindowClause;
import net.sf.saxon.Controller;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.SequenceOutputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.flwor.*;

import java.util.List;

/**
 * Generate bytecode for a call to the FLWORExpression
 */
public class FLWORExpressionCompiler extends ToIteratorCompiler {
    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {


        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "FLWORExpressionCompiler-Iter");
        FLWORExpression flworExpr = (FLWORExpression) expression;
        List<Clause> clauses = flworExpr.getClauseList();

        //force push mode if there is a window clause that is not the first clause
        for (int i = 1; i < clauses.size(); i++) {
            if (clauses.get(i) instanceof WindowClause) {
                compiler.generateGetContext();
                ga.invokeInstanceMethod(XPathContext.class, "getController");
                ga.invokeInstanceMethod(Controller.class, "allocateSequenceOutputter");
                ga.dup();
                ga.invokeInstanceMethod(SequenceOutputter.class, "getPipelineConfiguration");
                ga.push(expression.getPackageData().getHostLanguage());
                ga.invokeInstanceMethod(PipelineConfiguration.class, "setHostLanguage", Integer.TYPE);
                compiler.pushNewReceiverInfo(ga);
                compiler.compileToPush(expression);
                compiler.generateGetReceiver();
                compiler.popReceiverInfo();
                ga.dup();
                ga.invokeInstanceMethod(SequenceOutputter.class, "close");
                ga.invokeInstanceMethod(SequenceOutputter.class, "iterate");
                return;
            }

        }


        ga.newInstance(ReturnClauseIterator.class);
        ga.dup();
        ga.invokeDefaultConstructor(SingularityPull.class);

        for (Clause c : clauses) {
            ExpressionCompiler.allocateStatic(compiler, c);
            ga.swap();
            compiler.generateGetContext();
            ga.invokeInstanceMethod(Clause.class, "getPullStream", TuplePull.class, XPathContext.class);
        }
        allocateStatic(compiler, flworExpr);
        compiler.generateGetContext();
        ga.invokeConstructor(ReturnClauseIterator.class, TuplePull.class, FLWORExpression.class, XPathContext.class);
    }


    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "FLWORExpressionCompiler-Push");
        FLWORExpression flworExpr = (FLWORExpression) expression;
        List<Clause> clauses = flworExpr.getClauseList();

        ga.newInstance(ReturnClausePush.class);
        ga.dup();
        allocateStatic(compiler, flworExpr.getReturnClause());
        ga.invokeConstructor(ReturnClausePush.class, Expression.class);

        for (int i = clauses.size() - 1; i >= 0; i--) {
            ExpressionCompiler.allocateStatic(compiler, clauses.get(i));
            ga.swap();
            compiler.generateGetContext();
            ga.invokeInstanceMethod(Clause.class, "getPushStream", TuplePush.class, XPathContext.class);
        }
        ga.dup();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(TuplePush.class, "processTuple", XPathContext.class);
        ga.invokeInstanceMethod(TuplePush.class, "close");
    }

}

// Copyright (c) 2011 Saxonica Limited.

