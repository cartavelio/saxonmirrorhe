package com.saxonica.ee.trans;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.OperatorSymbol;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.pattern.BasePatternWithPredicate;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.pattern.PatternWithPredicate;

import java.util.List;
import java.util.Map;

/**
 * A PatternOptimizer is an agent responsible for optimizing a particular class
 * of patterns. It has two parts: a meta-pattern which determines which kinds of
 * pattern it recognizes, and an action part which extracts characteristics and
 * preconditions whose evaluation can be shared by all the rules in a rule chain
 */
public class EqualityPatternOptimizer extends PatternOptimizer {

    private static final EqualityPatternOptimizer THE_INSTANCE = new EqualityPatternOptimizer();

    public static EqualityPatternOptimizer getInstance() {
        return THE_INSTANCE;
    }

    @Override
    public boolean appliesTo(Pattern pattern) {
        if (pattern instanceof PatternWithPredicate) {
            Expression exp = ((PatternWithPredicate) pattern).getPredicate();
            if (exp instanceof ComparisonExpression) {
                if (exp instanceof GeneralComparison && ((ComparisonExpression) exp).getRhsExpression() instanceof StringLiteral
                        && ((GeneralComparison) exp).getSingletonOperator() == OperatorSymbol.FEQ) {
                    return true;
                }
                if (exp instanceof ValueComparison && ((ComparisonExpression) exp).getRhsExpression() instanceof StringLiteral
                        && ((ValueComparison) exp).getSingletonOperator() == OperatorSymbol.FEQ) {
                    return true;
                }
            }
        }
        return false;
    }

    @Override
    public void process(RuleEE rule, Pattern pattern, Map<Precondition, List<RuleEE>> preconditions, Map<Characteristic, Characteristic> characteristics) {
        Expression exp = ((PatternWithPredicate) pattern).getPredicate();

        Expression lhs = ((ComparisonExpression) exp).getLhsExpression();
        StringCollator collator = ((ComparisonExpression) exp).getStringCollator();
        GeneralComparisonOperand chic = new GeneralComparisonOperand(collator, lhs);
        chic = (GeneralComparisonOperand) allocateCharacteristic(characteristics, chic);
        AtomicMatchKey key = collator.getCollationKey(
                ((StringLiteral) ((ComparisonExpression) exp).getRhsExpression()).getString());
        Precondition.StringHashPrecondition p = new Precondition.StringHashPrecondition(chic, key, collator);
        addPrecondition(rule, preconditions, p);

        if (pattern instanceof BasePatternWithPredicate) {
            Pattern base = ((BasePatternWithPredicate) pattern).getBasePattern();
            GeneralPatternOptimizer.getInstance().process(rule, base, preconditions, characteristics);
        }
    }
}

// Copyright (c) 2018-2023 Saxonica Limited