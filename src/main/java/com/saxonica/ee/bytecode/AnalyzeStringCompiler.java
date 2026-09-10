package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.map.AnalyzeMappingFunction;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.AnalyzeString;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.om.FocusTrackingIterator;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.regex.RegexIterator;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.UnicodeString;

import java.util.List;

/**
 * Generate bytecode for a call to the xsl:analyze-string expression.
 */
public class AnalyzeStringCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(/*@NotNull*/ CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo mi = compiler.getCurrentMethod();
        AnalyzeString analyzeStr = (AnalyzeString) expression;

        RegularExpression re = analyzeStr.getPatternExpression();
        Expression nonMatching = analyzeStr.getNonMatching();
        Expression matching = analyzeStr.getMatching();


        compileRegex(compiler, ga, mi, analyzeStr, re);
        compiler.compileToPrimitive(analyzeStr.getSelect(), UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
        ga.invokeInstanceMethod(RegularExpression.class, "analyze", UnicodeString.class);
        //RegexIterator on stack
        int iterVar = ga.newLocal(RegexIterator.class);
        ga.storeLocal(iterVar);

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newContext");
        int c2Var = mi.allocateLocal(XPathContextMajor.class);
        ga.storeLocal(c2Var);
        mi.pushContextVariableInfo(c2Var, false);
        ga.loadLocal(c2Var);
        allocateStatic(compiler, analyzeStr);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setOrigin", ContextOriginator.class);

        ga.loadLocal(c2Var);
        ga.newInstance(FocusTrackingIterator.class);
        ga.dup();
        ga.loadLocal(iterVar);
        ga.invokeConstructor(FocusTrackingIterator.class, SequenceIterator.class);
        ga.invokeInstanceMethod(XPathContext.class, "setCurrentIterator", FocusIterator.class);

        ga.loadLocal(c2Var);
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentRegexIterator", RegexIterator.class);


        ga.newInstance(ContextMappingIterator.class);
        ga.dup();
        ga.newInstance(AnalyzeMappingFunction.class);
        ga.dup();
        ga.loadLocal(iterVar);
        ga.loadLocal(c2Var);
        if (nonMatching == null) {
            allocateStatic(compiler, Literal.makeEmptySequence());
        } else {
            CompiledExpression exp = compiler.compileToByteCode(nonMatching, "EE_NonMatchingExpr", Expression.ITERATE_METHOD);
            if (exp == null) {
                throw new CannotCompileException(nonMatching);
            }
            allocateStatic(compiler, exp);
        }
        if (matching == null) {
            allocateStatic(compiler, Literal.makeEmptySequence());
        } else {
            CompiledExpression exp = compiler.compileToByteCode(matching, "EE_MatchingExpr", Expression.ITERATE_METHOD);
            if (exp == null) {
                throw new CannotCompileException(nonMatching);
            }
            allocateStatic(compiler, exp);
        }
        ga.invokeConstructor(AnalyzeMappingFunction.class, RegexIterator.class, XPathContext.class, Expression.class, Expression.class);
        ga.loadLocal(c2Var);
        ga.invokeConstructor(ContextMappingIterator.class, ContextMappingFunction.class, XPathContext.class);
        mi.popContextVariableInfo();
    }

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
//        RegexIterator iter = getRegexIterator(context);
//        FocusIterator focusIter = new FocusTrackingIterator(iter);

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo mi = compiler.getCurrentMethod();
        AnalyzeString analyzeStr = (AnalyzeString) expression;

        RegularExpression re = analyzeStr.getPatternExpression();
//        Expression nonMatching = analyzeStr.getNonMatching();
//        Expression matching = analyzeStr.getMatching();


        compileRegex(compiler, ga, mi, analyzeStr, re);
        compiler.compileToPrimitive(analyzeStr.getSelect(), UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
        ga.invokeInstanceMethod(RegularExpression.class, "analyze", UnicodeString.class);
        //RegexIterator on stack
        int iterVar = ga.newLocal(RegexIterator.class);
        ga.storeLocal(iterVar);

//        XPathContextMajor c2 = context.newContext();
//        c2.setOrigin(this);
//        c2.setCurrentIterator(focusIter);
//        c2.setCurrentRegexIterator(iter);

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newContext");
        int c2Var = mi.allocateLocal(XPathContextMajor.class);
        ga.storeLocal(c2Var);
        mi.pushContextVariableInfo(c2Var, false);
        ga.loadLocal(c2Var);
        allocateStatic(compiler, analyzeStr);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setOrigin", ContextOriginator.class);

        ga.loadLocal(c2Var);
        ga.newInstance(FocusTrackingIterator.class);
        ga.dup();
        ga.loadLocal(iterVar);
        ga.invokeConstructor(FocusTrackingIterator.class, SequenceIterator.class);
        int focusIterVar = ga.newLocal(FocusTrackingIterator.class);
        ga.dup();
        ga.storeLocal(focusIterVar);
        ga.invokeInstanceMethod(XPathContext.class, "setCurrentIterator", FocusIterator.class);

        ga.loadLocal(c2Var);
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentRegexIterator", RegexIterator.class);

//
//        while (true) {

        LabelInfo loop = mi.newLabel("loop");
        LabelInfo breakLoop = mi.newLabel("breakLoop");
        LabelInfo nonMatchingLab = mi.newLabel("nonMatching");
        mi.placeLabel(loop);

//            Item it = focusIter.next();
//            if (it == null) {
//                break;
//            }

        ga.loadLocal(focusIterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.ifNull(breakLoop.label());

//            if (iter.isMatching()) {

        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(RegexIterator.class, "isMatching");
        ga.ifFalse(analyzeStr.getNonMatching() == null ? loop : nonMatchingLab);
//                if (getMatching() != null) {
//                    getMatching().process(c2);
//                }
        if (analyzeStr.getMatching() != null) {
            compiler.compileToPush(analyzeStr.getMatching());
        }

        ga.goTo(loop);
        mi.placeLabel(nonMatchingLab);
//            } else {
//                if (getNonMatching() != null) {
//                    getNonMatching().process(c2);
//                }
        if (analyzeStr.getNonMatching() != null) {
            compiler.compileToPush(analyzeStr.getNonMatching());
        }

        ga.goTo(loop);
//            }
//        }

        mi.placeLabel(breakLoop);
        mi.popContextVariableInfo();

    }

    private void compileRegex(CompilerService compiler, Generator ga, GeneratedMethodInfo mi, AnalyzeString analyzeStr, RegularExpression re) throws CannotCompileException {
        if (re == null) {
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
            compiler.compileToPrimitive(analyzeStr.getRegex(), UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            compiler.compileToPrimitive(analyzeStr.getFlags(), String.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            ga.push("XP30");
            ga.pushNull();
            ga.invokeInstanceMethod(Configuration.class, "compileRegularExpression", UnicodeString.class, String.class, String.class, List.class);

        } else {
            allocateStatic(compiler, re);
        }
    }
}

// Copyright (c) 2011-2020 Saxonica Limited
