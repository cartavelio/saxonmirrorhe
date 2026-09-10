package com.saxonica.ee.trans;

import net.sf.saxon.expr.CompareToIntegerConstant;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.pattern.BasePatternWithPredicate;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.pattern.PatternWithPredicate;

import java.util.List;
import java.util.Map;

/**
 * A PatternOptimizer is an agent responsible for optimizing a particular class
 * of patterns. An <code>IntEqualityPatternOptimizer</code> is used for patterns of the
 * form <code>match="XXX[f(.) = 23]"</code>, that is, patterns containing a predicate
 * that compares some value with an integer constant.
 */
public class IntEqualityPatternOptimizer extends PatternOptimizer {

    private static final IntEqualityPatternOptimizer THE_INSTANCE = new IntEqualityPatternOptimizer();

    public static IntEqualityPatternOptimizer getInstance() {
        return THE_INSTANCE;
    }

    @Override
    public boolean appliesTo(Pattern pattern) {
        if (pattern instanceof PatternWithPredicate) {
            Expression exp = ((PatternWithPredicate) pattern).getPredicate();
            return exp instanceof CompareToIntegerConstant;
        }
        return false;
    }

    @Override
    public void process(RuleEE rule, Pattern pattern, Map<Precondition, List<RuleEE>> preconditions, Map<Characteristic, Characteristic> characteristics) {
        Expression exp = ((PatternWithPredicate) pattern).getPredicate();
        Expression lhs = ((CompareToIntegerConstant) exp).getLhsExpression();
        NumericComparisonOperand chic = new NumericComparisonOperand(lhs);
        chic = (NumericComparisonOperand) allocateCharacteristic(characteristics, chic);
        long key = ((CompareToIntegerConstant) exp).getComparand();
        Precondition.NumericComparisonPrecondition p = new Precondition.NumericComparisonPrecondition(chic, key);
        addPrecondition(rule, preconditions, p);

        if (pattern instanceof BasePatternWithPredicate) {
            Pattern base = ((BasePatternWithPredicate) pattern).getBasePattern();
            GeneralPatternOptimizer.getInstance().process(rule, base, preconditions, characteristics);
        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
