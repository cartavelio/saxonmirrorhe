package com.saxonica.ee.trans;

import net.sf.saxon.pattern.Pattern;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * A PatternOptimizer is an agent responsible for optimizing a particular class
 * of patterns. It has two parts: a meta-pattern which determines which kinds of
 * pattern it recognizes, and an action part which extracts characteristics and
 * preconditions whose evaluation can be shared by all the rules in a rule chain
 */
public abstract class PatternOptimizer {

    /**
     * Ask whether this pattern optimizer is applicable to a particular pattern
     * @param pattern the pattern in question
     * @return true if this pattern optimizer is applicable to this pattern
     */

    public abstract boolean appliesTo(Pattern pattern);

    /**
     * Process a particular pattern, potentially adding to the list of preconditions and characteristics
     * maintained when processing a set of rules
     * @param rule the rule to be processed
     * @param pattern the pattern used by this rule
     * @param preconditions a collection of preconditions maintained for a set of rules
     * @param characteristics a collection of characteristics maintained for a set of rules.
     *   This is maintained as a map from characteristics
     *   to characteristics in which the key and the value are always the same; the reason for this
     *   curious structure is that when an equivalent characteristic is already present in the map,
     *   we want to return the existing characteristic to replace the new one; a Java <code>Set</code>
     *   does not have any (efficient) operation to "find the existing value in the set that is equal
     *   to X".
     */

    public abstract void process(RuleEE rule,
                                     Pattern pattern,
                                     Map<Precondition, List<RuleEE>> preconditions,
                                     Map<Characteristic, Characteristic> characteristics);

    /**
     * Allocate a new characteristic, that is, a property of nodes or other items that may potentially
     * be used in evaluating the preconditions of more than one pattern.
     * @param characteristics The set of characteristics to be added to. This maintained as a map from characteristics
     *                        to characteristics for reasons explained at {@link #process(RuleEE, Pattern, Map, Map)}.
     * @param newChic a new characteristic to be added to the set.
     * @return the supplied characteristic unless there was already an equivalent characteristic in the set, in which
     * case an equivalent characteristic that should be used in place of the new one.
     */
    protected Characteristic allocateCharacteristic(Map<Characteristic, Characteristic> characteristics,
                                                    Characteristic newChic) {
        //Set<Characteristic<T>> characteristicSet;
        Characteristic chic = characteristics.get(newChic);
        if (chic == null) {
            characteristics.put(newChic, newChic);
            return newChic;
        } else {
            return chic;
        }
    }

    protected void addPrecondition(RuleEE rule, Map<Precondition, List<RuleEE>> preconditions, Precondition p) {
        if (preconditions.containsKey(p)) {
            preconditions.get(p).add(rule);
        } else {
            List<RuleEE> applicableRules = new ArrayList<>();
            applicableRules.add(rule);
            preconditions.put(p, applicableRules);
        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited

