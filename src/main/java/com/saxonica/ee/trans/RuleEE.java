package com.saxonica.ee.trans;

import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.trans.rules.RuleTarget;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * A Rule with associated preconditions
 */
public class RuleEE extends Rule {

    // A list of preconditions that must be satisfied for this rule to match
    private List<Precondition> preconditions = Collections.emptyList();

    public RuleEE() {
    }

    public RuleEE(Pattern p, RuleTarget o, int prec, int min, double prio, int seq, int part) {
        super(p, o, prec, min, prio, seq, part);
    }

    @Override
    public Rule copy(boolean copyChain) {
        RuleEE r2 = new RuleEE();
        r2.copyFrom(this, copyChain);
        return r2;
    }

    /**
     * Get the preconditions for this rule
     * @return a list of preconditions that must be satisfied if this rule is to match
     */

    public List<Precondition> getPreconditions() {
        return preconditions;
    }

    /**
     * Add a precondition for this rule
     * @param condition a precondition that must be satisfied if this rule is to match
     */

    public void addPrecondition(Precondition condition) {
        if (preconditions.isEmpty()) {
            preconditions = new ArrayList<>();
        }
        preconditions.add(condition);
    }

    /**
     * Check that all preconditions for this rule are satisfied
     * @param state holds the state for matching all rules on a rule-chain, including values
     *              of common subexpressions
     * @return true if all preconditions are satisfied
     * @throws XPathException if evaluation of a precondition fails
     */

    public boolean checkPreconditions(PreconditionMatcher state) throws XPathException {
        for (Precondition condition : preconditions) {
            if (!condition.isSatisfied(state)) {
                return false;
            }
        }
        return true;
    }



    @Override
    public void exportOtherProperties(ExpressionPresenter out) throws XPathException {
//        int[] preconditionIndexes = getPreconditionIndexes();
//        if (preconditionIndexes != null && preconditionIndexes.length > 0) {
//            boolean first = true;
//            String indexes = "";
//            for (int i : preconditionIndexes) {
//                indexes = indexes + (!first ? " " : "") + i;
//                first = false;
//            }
//            out.emitAttribute("conditions", indexes);
//        }
//        Set<Expression> preconditions = getPreconditions();
//        if (preconditions != null && preconditions.size() > 0) {
//            out.startSubsidiaryElement("c.pre");
//            for (Expression e : preconditions) {
//                e.export(out);
//            }
//            out.endSubsidiaryElement();
//        }
    }


}

// Copyright (c) 2018-2022 Saxonica Limited
