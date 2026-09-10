package com.saxonica.ee.trans;

import net.sf.saxon.pattern.Pattern;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * A PatternOptimizer is an agent responsible for optimizing a particular class
 * of patterns. The <code>GeneralPatternOptimizer</code> is an umbrella implementation
 * that delegates to a number of more specific pattern optimizers.
 */
public class GeneralPatternOptimizer extends PatternOptimizer {

    private static final List<PatternOptimizer> patternOptimizers = new ArrayList<PatternOptimizer>();

    static {
        patternOptimizers.add(EqualityPatternOptimizer.getInstance());
        patternOptimizers.add(ContainsTokenPatternOptimizer.getInstance());
        patternOptimizers.add(IntEqualityPatternOptimizer.getInstance());
        patternOptimizers.add(AncestorPatternOptimizer.getInstance());
    }

    private static final GeneralPatternOptimizer THE_INSTANCE = new GeneralPatternOptimizer();

    public static GeneralPatternOptimizer getInstance() {
        return THE_INSTANCE;
    }

    @Override
    public boolean appliesTo(Pattern pattern) {
        return true;
    }

    @Override
    public void process(RuleEE rule, Pattern pattern, Map<Precondition, List<RuleEE>> preconditions, Map<Characteristic, Characteristic> characteristics) {
        for (PatternOptimizer optimizer : patternOptimizers) {
            if (optimizer.appliesTo(pattern)) {
                optimizer.process(rule, pattern, preconditions, characteristics);
                break;
            }
        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited

