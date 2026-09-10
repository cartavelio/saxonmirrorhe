package com.saxonica.ee.trans;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.Contains;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.pattern.BasePatternWithPredicate;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.pattern.PatternWithPredicate;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Whitespace;

import java.util.List;
import java.util.Map;

/**
 * A PatternOptimizer is an agent responsible for optimizing a particular class
 * of patterns. It has two parts: a meta-pattern which determines which kinds of
 * pattern it recognizes, and an action part which extracts characteristics and
 * preconditions whose evaluation can be shared by all the rules in a rule chain
 */
public class ContainsTokenPatternOptimizer extends PatternOptimizer {

    private static final ContainsTokenPatternOptimizer THE_INSTANCE = new ContainsTokenPatternOptimizer();

    public static ContainsTokenPatternOptimizer getInstance() {
        return THE_INSTANCE;
    }

    @Override
    public boolean appliesTo(Pattern pattern) {
        if (pattern instanceof PatternWithPredicate) {
            Expression exp = ((PatternWithPredicate) pattern).getPredicate();
            if (exp.isCallOn(Contains.class)) {
                // Optimize the DITA scenario which uses rules of the form *[contains(@class, " topic/li ")]
                SystemFunctionCall call = (SystemFunctionCall) exp;
                if (call.getArity() == 2 && call.getArg(1) instanceof StringLiteral &&
                        exp.getRetainedStaticContext().getDefaultCollationName().equals(NamespaceConstant.CODEPOINT_COLLATION_URI)) {
                    String contained = ((StringLiteral) call.getArg(1)).stringify();
                    return contained.matches(" [^ ]+ ");
                }
            }
        }
        return false;
    }

    @Override
    public void process(RuleEE rule, Pattern pattern, Map<Precondition, List<RuleEE>> preconditions, Map<Characteristic, Characteristic> characteristics) {
        Expression exp = ((BasePatternWithPredicate) pattern).getPredicate();
        SystemFunctionCall call = (SystemFunctionCall) exp;
        UnicodeString contained = ((StringLiteral) call.getArg(1)).getString();
        UnicodeString token = Whitespace.trim(contained);
        TokenMatchOperand chic = new TokenMatchOperand(call.getArg(0));
        chic = (TokenMatchOperand) allocateCharacteristic(characteristics, chic);
        StringCollator collator = CodepointCollator.getInstance();
        AtomicMatchKey key = collator.getCollationKey(token);
        Precondition.StringHashPrecondition p = new Precondition.StringHashPrecondition(chic, key, collator);
        addPrecondition(rule, preconditions, p);
    }
}

// Copyright (c) 2018-2023 Saxonica Limited

