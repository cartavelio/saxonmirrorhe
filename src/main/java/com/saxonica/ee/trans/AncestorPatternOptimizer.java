package com.saxonica.ee.trans;

import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.pattern.AncestorQualifiedPattern;
import net.sf.saxon.pattern.Pattern;

import java.util.List;
import java.util.Map;

/**
 * A PatternOptimizer is an agent responsible for optimizing a particular class
 * of patterns. It has two parts: a meta-pattern which determines which kinds of
 * pattern it recognizes, and an action part which extracts characteristics and
 * preconditions whose evaluation can be shared by all the rules in a rule chain
 */
public class AncestorPatternOptimizer extends PatternOptimizer {

    private static final AncestorPatternOptimizer THE_INSTANCE = new AncestorPatternOptimizer();

    public static AncestorPatternOptimizer getInstance() {
        return THE_INSTANCE;
    }

    @Override
    public boolean appliesTo(Pattern pattern) {
        return pattern instanceof AncestorQualifiedPattern;
    }

    public void process(RuleEE rule,
                        Pattern pattern,
                        Map<Precondition, List<RuleEE>> preconditions,
                        Map<Characteristic, Characteristic> characteristics) {
        Pattern anc = ((AncestorQualifiedPattern) pattern).getUpperPattern();
        boolean isParent = ((AncestorQualifiedPattern) pattern).getUpwardsAxis() == AxisInfo.PARENT;
        AncestorCharacteristic chic = new AncestorCharacteristic(anc, isParent);
        AncestorCharacteristic chic2 = (AncestorCharacteristic)allocateCharacteristic(characteristics, chic);

        Precondition.BooleanPrecondition p = new Precondition.BooleanPrecondition(chic2);
        addPrecondition(rule, preconditions, p);

        Pattern base = ((AncestorQualifiedPattern) pattern).getBasePattern();
        GeneralPatternOptimizer.getInstance().process(rule, base, preconditions, characteristics);
    }
}

// Copyright (c) 2018-2023 Saxonica Limited
