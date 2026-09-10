package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import com.saxonica.ee.optim.SwitchExpression;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * compiler for switch expression in XQuery 3.0, whether explicitly written as such, or the result of
 * optimizing an xsl:choose or a sequence of if/then/elseif/then expressions
 */
public class SwitchExpressionCompiler extends ToIteratorCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileGeneric(compiler, expression, Expression.EVALUATE_METHOD);
        return Item.class;
    }

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileGeneric(compiler, expression, Expression.PROCESS_METHOD);
    }

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileGeneric(compiler, expression, Expression.ITERATE_METHOD);
        return new IteratorDetails(SequenceIterator.class, Item.class);
    }

    /**
     * Generic code to compile the switch expression in any of the three modes evaluate, iterate, or process
     *
     * @param compiler       the compiler service
     * @param expression     the switch expression
     * @param evaluationMode the evaluation mode
     * @throws CannotCompileException if the expression cannot be compiled
     */

    private void compileGeneric(CompilerService compiler, Expression expression, int evaluationMode) throws CannotCompileException {
        SwitchExpression switcher = (SwitchExpression) expression;

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        String modeName = "";
        switch (evaluationMode) {
            case Expression.EVALUATE_METHOD:
                modeName = "Item";
                break;
            case Expression.ITERATE_METHOD:
                modeName = "Itr";
                break;
            case Expression.PROCESS_METHOD:
                modeName = "Push";
                break;
        }

        visitAnnotation(compiler, "SwitchExpressionCompiler-" + modeName);
        visitLineNumber(compiler, ga, expression);
        LabelInfo done = methodInfo.newLabel("SwitchDone");
        LabelInfo otherwise = methodInfo.newLabel("SwitchOtherwise");
        LabelInfo foundCase = methodInfo.newLabel("SwitchFoundCase");

//        AtomicValue subjectValue = (AtomicValue)subject.evaluateItem(context);
//        if (subjectValue == null) {
//            return defaultAction.evaluateItem(context);
//        }

        Expression subject = switcher.getSubjectExpression();
        compiler.compileToItem(subject);

        if (Cardinality.allowsZero(subject.getCardinality())) {
            LabelInfo notNull = methodInfo.newLabel("SwitchNotNull");
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            ga.goTo(otherwise);
            methodInfo.placeLabel(notNull);
        }

//        Object key = subjectValue.getXPathComparable(false, collation, context.getImplicitTimezone);
//        Expression action = map.get(key);
//        if (action == null) {
//            return defaultAction.evaluateItem(context);
//        } else {
//            return action.evaluateItem(context);
//        }

        ga.checkClass(AtomicValue.class);
        allocateStatic(compiler, switcher.getCollation());
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getImplicitTimezone");
        ga.invokeInstanceMethod(AtomicValue.class, "getXPathMatchKey", StringCollator.class, Integer.TYPE);

        // Compile the branches (cases) of the switch expression as separate free-standing compiled expression objects,
        // holding these in a map, which we then use at run-time to identify the appropriate expression to evaluate

        allocateStatic(compiler, compileMap(compiler, switcher, evaluationMode));
        ga.swap();
        ga.invokeInstanceMethod(Map.class, "get", Object.class);
        ga.dup();
        ga.ifNonNull(foundCase.label());
        ga.pop();
        ga.goTo(otherwise);

        methodInfo.placeLabel(foundCase);
        ga.checkClass(Expression.class);
        if (evaluationMode == Expression.PROCESS_METHOD) {
            compiler.generateGetOutputter();
        }
        compiler.generateGetContext();
        switch (evaluationMode) {
            case Expression.EVALUATE_METHOD:
                ga.invokeInstanceMethod(Expression.class, "evaluateItem", XPathContext.class);
                break;
            case Expression.ITERATE_METHOD:
                ga.invokeInstanceMethod(Expression.class, "iterate", XPathContext.class);
                break;
            case Expression.PROCESS_METHOD:
                ga.invokeInstanceMethod(Expression.class, "process", Outputter.class, XPathContext.class);
                break;
        }

        ga.goTo(done);

        methodInfo.placeLabel(otherwise);
        switch (evaluationMode) {
            case Expression.EVALUATE_METHOD:
                compiler.compileToItem(switcher.getOtherwiseExpression());
                break;
            case Expression.ITERATE_METHOD:
                compiler.compileToIterator(switcher.getOtherwiseExpression());
                break;
            case Expression.PROCESS_METHOD:
                compiler.compileToPush(switcher.getOtherwiseExpression());
                break;
        }

        methodInfo.placeLabel(done);
    }

    /**
     * Internal method to generate a map containing the compiled "case" expressions
     *
     * @param compiler       the compiler service
     * @param switcher       the switch expression
     * @param evaluationMode the expression evaluation mode (e.g. compile as item, compile as iterator, compile push)
     * @return a new map containing compiled expressions in place of interpreted expressions
     * @throws CannotCompileException if any of the expressions cannot be compiled
     */

    private Map<AtomicMatchKey, Expression> compileMap(CompilerService compiler, SwitchExpression switcher, int evaluationMode) throws CannotCompileException {
        Map<AtomicMatchKey, Integer> cases = switcher.getCaseMap();
        Map<AtomicMatchKey, Expression> out = new HashMap<>(cases.size());
        List<Operand> actions = switcher.getActions();
        List<CompiledExpression> compiledActions = new ArrayList<>();
        for (int i=0; i<actions.size(); i++) {
            CompiledExpression ce = compiler.compileToByteCode(
                    actions.get(i).getChildExpression(), "case" + i, evaluationMode);
            if (ce == null) {
                throw new CannotCompileException(actions.get(i).getChildExpression());
            }
            compiledActions.add(ce);
        }

        for (Map.Entry<AtomicMatchKey, Integer> e : cases.entrySet()) {
            out.put(e.getKey(), compiledActions.get(e.getValue()));
        }
        return out;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited