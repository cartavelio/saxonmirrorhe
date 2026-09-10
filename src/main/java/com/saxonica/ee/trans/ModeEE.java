package com.saxonica.ee.trans;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.trans.ModePE;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.accum.Accumulator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.trans.rules.RuleChain;
import net.sf.saxon.trans.rules.RuleSearchState;
import net.sf.saxon.trans.rules.RuleTarget;
import net.sf.saxon.type.Type;

import java.util.*;

/**
 * A version of class Mode that optimizes various aspects of the template patterns,
 * most notably supporting rule preconditions and indexation of unnamed elements.
 */
public class ModeEE extends ModePE {


    public ModeEE(StructuredQName modeName, CompilerInfo compilerInfo) {
        super(modeName);
        init();
    }

    private void init() {

    }


    @Override
    protected void exportUseAccumulators(ExpressionPresenter presenter) {
        Set<? extends Accumulator> accumulators = getAccumulators();
        if (accumulators != null && !accumulators.isEmpty()) {
            StringBuilder fsb = new StringBuilder(256);
            for (Accumulator acc : accumulators) {
                String name = acc.getAccumulatorName().getEQName();
                if (fsb.length() != 0) {
                    fsb.append(" ");
                }
                fsb.append(name);
            }
            presenter.emitAttribute("useAcc", fsb.toString());
        } else if ("JS".equals(((ExpressionPresenter.ExportOptions) presenter.getOptions()).target)) {
            presenter.emitAttribute("useAcc", "");
        }
    }

    @Override
    public Rule makeRule(/*@NotNull*/ Pattern pattern, /*@NotNull*/ RuleTarget action,
                                      int precedence, int minImportPrecedence, double priority, int sequence, int part) {
        return new RuleEE(pattern, action, precedence, minImportPrecedence, priority, sequence, part);
    }

    @Override
    protected PreconditionMatcher makeRuleSearchState(RuleChain chain, XPathContext context) {
        return new PreconditionMatcher(chain, context);
    }


    /**
     * Find the named node rule chain for a given node
     *
     * @param node    the node to be matched
     * @param context local context
     * @return the rule chain for nodes with this name, or null if none
     */
    public RuleChain getNamedNodeChain(NodeInfo node, XPathContext context) {
        if (node.hasFingerprint()) {
            switch (node.getNodeKind()) {
                case Type.ELEMENT:
                    return namedElementRuleChains.get(node.getFingerprint());
                case Type.ATTRIBUTE:
                    return namedAttributeRuleChains.get(node.getFingerprint());
            }
        } else {
            switch (node.getNodeKind()) {
                case Type.ELEMENT:
                case Type.ATTRIBUTE:
                    return getNamedRuleChain(context, node.getNodeKind(), node.getURI(), node.getLocalPart());
            }
        }
        return null;
    }

    /**
     * Find the unnamed node rule chain for a given node
     *
     * @param node the node to be matched
     * @return the rule chain for nodes of this type,or XPath error if illegal type
     */
    public RuleChain getUnnamedNodeChain(NodeInfo node) {
        switch (node.getNodeKind()) {
            case Type.DOCUMENT:
                return documentRuleChain;
            case Type.ELEMENT:
                return unnamedElementRuleChain;
            case Type.ATTRIBUTE:
                return unnamedAttributeRuleChain;
            case Type.TEXT:
                return textRuleChain;
            case Type.COMMENT:
                return commentRuleChain;
            case Type.PROCESSING_INSTRUCTION:
                return processingInstructionRuleChain;
            case Type.NAMESPACE:
                return namespaceRuleChain;
            default:
                throw new AssertionError("Unknown node kind");
        }
    }

    /**
     * Does this rule match the given item? Can be overridden
     *
     * @param r               the rule to check
     * @param item            the context item
     * @param context         the static context for evaluation
     * @param ruleSearchState An appropriate matcher for preconditions in this mode
     * @return true if this rule does match
     * @throws XPathException if a dynamic error occurs while matching the pattern
     */
    @Override
    protected boolean ruleMatches(Rule r, Item item, XPathContextMajor context, RuleSearchState ruleSearchState) throws XPathException {
        return r.isAlwaysMatches() || ((RuleEE) r).checkPreconditions((PreconditionMatcher) ruleSearchState) && r.matches(item, context);
    }

    /**
     * Compute the streamability of all template rules
     */

    @Override
    public void computeStreamability() throws XPathException {
        processRules(r -> {
            TemplateRuleEE t = (TemplateRuleEE) r.getAction();
            List<String> reasons = new ArrayList<>();
            PostureAndSweep ps = Streamability.getStreamability(t.getBody(), ContextItemStaticInfoEE.DEFAULT, reasons);
            if (ps.getSweep() == Sweep.FREE_RANGING || ps.getPosture() == Posture.ROAMING) {
                // this should not happen
                StringBuilder message = new StringBuilder("Stylesheet packaging problem: reloaded template with match = "
                                                                  + t.getMatchPattern().toShortString() + " is not streamable. ");
                for (String reason : reasons) {
                    message.append(reason).append(". ");
                }
                throw new XPathException(message.toString());
            }
        });
    }

    /**
     * For a streamable mode, invert all the templates to generate streamable code
     *
     * @throws net.sf.saxon.trans.XPathException if there is a non-streamable template in the mode
     */
    @Override
    public void invertStreamableTemplates() throws XPathException {
        if (isDeclaredStreamable()) {
            processRules(r -> {
                TemplateRuleEE t = (TemplateRuleEE) r.getAction();
                if (t.getBody() != null) {
                    if (Streamability.getPostureAndSweepIfKnown(t.getBody()) == null) {
                        if (!t.isActuallyStreamable(new ArrayList<>())) {
                            throw new XPathException("Template rule is not streamable", "XTSE3430", t);
                        }
                    }
                    if (!t.isDeclaredStreamable()) {
                        // means that the template was found to be non-streamable and fallback processing was requested
                        setStreamable(false);
                    }
                }
            });
        }
        if (isDeclaredStreamable()) {
            processRules(r -> {
                TemplateRuleEE t = (TemplateRuleEE) r.getAction();
                if (t.getBody() != null) {
                    if (Streamability.getSweep(t.getBody()) == Sweep.CONSUMING) {
                        t.makeInversion();
                    }
                }
            });
        }
    }

//    public interface RuleTypeAction extends RuleAction {
//        void setType(ItemType type);
//    }
//
//    protected RuleTypeAction setType(RuleTypeAction action, ItemType type) {
//        action.setType(type);
//        return action;
//    }

    /**
     * Perform optimization on the complete set of rules comprising this Mode. This is done by optimizing
     * each rule chain in turn.
     */

    @Override
    public void optimizeRules() {

        optimizeRuleChain(documentRuleChain);
        optimizeRuleChain(unnamedElementRuleChain);
        for (RuleChain chain : namedElementRuleChains.valueSet()) {
            optimizeRuleChain(chain);
        }
        optimizeRuleChain(unnamedAttributeRuleChain);
        for (RuleChain chain : namedAttributeRuleChains.valueSet()) {
            optimizeRuleChain(chain);
        }
        optimizeRuleChain(textRuleChain);
        optimizeRuleChain(commentRuleChain);
        optimizeRuleChain(processingInstructionRuleChain);
        optimizeRuleChain(namespaceRuleChain);
        optimizeRuleChain(genericRuleChain);
        optimizeRuleChain(atomicValueRuleChain);
        optimizeRuleChain(functionItemRuleChain);
    }

    /**
     * Perform optimization on a chain of rules. This is done by examining the rules in the chain
     * to see if there are common subexpressions (preconditions) that apply to more than one rule;
     * once a precondition has been evaluated for one rule, it can be used to eliminate other rules
     * quickly without full evaluation of the pattern
     * @param chain the chain of rules
     */

    public void optimizeRuleChain(RuleChain chain) {
        if (chain.getLength() >= 10) {
            // No action on short rule chains
            Map<Precondition, List<RuleEE>> preconditions = new HashMap<>();
            Map<Characteristic, Characteristic> characteristics = new HashMap<>();
            Rule r = chain.head();
            while (r != null) {
                computeCandidatePreconditions((RuleEE)r, preconditions, characteristics);
                r = r.getNext();
            }
            preconditions = filterPreconditions(chain, preconditions);
            if (preconditions.size() > 4 || characteristics.size() > 4) {
                allocatePreconditionSlots(preconditions);
                allocateCharacteristicSlots(characteristics);
                RuleChainOptimizationData data = new RuleChainOptimizationData();
                data.preconditions = preconditions;
                data.characteristics = characteristics.keySet();
                chain.optimizationData = data;
            }
        }
    }

    public static class RuleChainOptimizationData {
        public Map<Precondition, List<RuleEE>> preconditions;
        public Set<Characteristic> characteristics;

    }


    /**
     * Given a (template) rule, examine it to look for preconditions and characteristics that might be shared
     * with other rules in the same rule chain
     *
     * @param rule            the rule to be examined
     * @param preconditions   A collection of all the preconditions for the rules in a rule set, expressed
     *                        as a map from preconditions to rules. A precondition is essentially a boolean
     *                        expression that gets applied to an item and that must be true if the item is to
     *                        match; the idea is to identify preconditions that several rules have in common.
     * @param characteristics A collection of the characteristics computed for the rules in a rule set.
     *                        Characteristics are properties of an item that may be used by one or more
     *                        of the rule preconditions. This maintained as a map from characteristics
     *                        to characteristics for reasons explained at
     *                        {@link PatternOptimizer#process(RuleEE, Pattern, Map, Map)}.
     */


    private void computeCandidatePreconditions(RuleEE rule, Map<Precondition, List<RuleEE>> preconditions, Map<Characteristic, Characteristic> characteristics) {
        Pattern pattern = rule.getPattern();
        GeneralPatternOptimizer.getInstance().process(rule, pattern, preconditions, characteristics);
    }

    private Map<Precondition, List<RuleEE>> filterPreconditions(RuleChain chain, Map<Precondition, List<RuleEE>> candidates) {
        Map<Precondition, List<RuleEE>> result = new HashMap<>();
        int chainLength = chain.getLength();
        for (Map.Entry<Precondition, List<RuleEE>> entry : candidates.entrySet()) {
            // A precondition is only worth evaluating if there are at least 3 rules to which it applies
            // and at least 3 to which it does not apply; in other cases, drop it.
            int numRules = entry.getValue().size();
            if (numRules >= 3 && numRules <= chainLength - 3) {
                result.put(entry.getKey(), entry.getValue());
            }
        }
        return result;
    }

    private void allocatePreconditionSlots(Map<Precondition, List<RuleEE>> conditions) {
        int slot = 0;
        for (Map.Entry<Precondition, List<RuleEE>> entry : conditions.entrySet()) {
            entry.getKey().setSlotNumber(slot++);
            for (RuleEE rule : entry.getValue()) {
                rule.addPrecondition(entry.getKey());
            }
        }
    }

    private void allocateCharacteristicSlots(Map<Characteristic, Characteristic> characteristics) {
        int slot = 0;
        for (Characteristic chic : characteristics.keySet()) {
            chic.setSlotNumber(slot++);
        }
    }

    /**
     * Work with a rule chain as a list of rules
     *
     * @param chain the chained set of rules
     * @return the rules as an ordered list, in the same order as the chain
     */
    public static List<Rule> listRules(RuleChain chain) {
        Rule r = chain == null ? null : chain.head();
        ArrayList<Rule> result = new ArrayList<>();
        while (r != null) {
            result.add(r);
            r = r.getNext();
        }
        return result;
    }

    /**
     * Combine two rule lists in rank order
     * <p>This does not detect duplicates within the list</p>
     *
     * @param r1 one list of rules, assumed in rank order
     * @param r2 the other list of rules, assumed in rank order
     * @return the combined list of rules
     */
    public static List<Rule> orderRules(List<Rule> r1, List<Rule> r2) {
        ArrayList<Rule> result = new ArrayList<>();
        Iterator<Rule> ir1 = r1.iterator();
        Iterator<Rule> ir2 = r2.iterator();
        Rule chain1 = ir1.hasNext() ? ir1.next() : null;
        Rule chain2 = ir2.hasNext() ? ir2.next() : null;
        while (chain1 != null || chain2 != null) {
            if (useFirst(chain1, chain2)) {
                result.add(chain1);
                chain1 = ir1.hasNext() ? ir1.next() : null;
            } else {
                result.add(chain2);
                chain2 = ir2.hasNext() ? ir2.next() : null;
            }
        }
        return result;
    }

    protected static boolean useFirst(Rule chain1, Rule chain2) {
        if (chain1 == null) {
            return false;
        } else if (chain2 == null) {
            return true;
        } else {
            int rank = chain1.compareRank(chain2);
            if (rank > 0) {
                return true;
            } else if (rank == 0) {                // Around here we could eliminate duplicates
                return chain1.getSequence() >= chain2.getSequence();
            } else {
                return false;
            }
        }
    }


    /**
     * Combine two rule chains into a rank-ordered combined list
     * <p>This could be simplified as orderRules(listRules(chain1),listRules(chain2))</p>
     *
     * @param chain1 the first rule chain
     * @param chain2 the second rule chain
     * @return a list containing all the rules in the two chains, ordered by rank
     */
    public static List<Rule> orderRules(RuleChain chain1, RuleChain chain2) {
        ArrayList<Rule> result = new ArrayList<>();
        Rule r1 = chain1.head();
        Rule r2 = chain2.head();
        while (r1 != null || r2 != null) {
            if (useFirst(r1, r2)) {
                result.add(r1);
                r1 = r1.getNext();
            } else {
                result.add(r2);
                r2 = r2.getNext();
            }
        }
        return result;
    }

    @Override
    public void explainTemplateRules(final ExpressionPresenter out) throws XPathException {
//        if (patternOptimization != null) {
//            patternOptimization.export(out);
//        }
//        if (rulePreconditions != null) {
//            rulePreconditions.export(out);
//        }
        super.explainTemplateRules(out);
    }


    @Override
    protected void reportAmbiguity(Item item, Rule r1, Rule r2, XPathContext c)
            throws XPathException {
        super.reportAmbiguity(item, r1, r2, c);
    }

    /**
     * Find the name for a node type
     *
     * @param type the code for the type
     * @return a suitable string or the empty string if unknown
     */
    public static String typeName(int type) {
        switch (type) {
            case Type.DOCUMENT:
                return "document-node()";
            case Type.ELEMENT:
                return "element()";
            case Type.ATTRIBUTE:
                return "attribute()";
            case Type.TEXT:
                return "text()";
            case Type.COMMENT:
                return "comment()";
            case Type.PROCESSING_INSTRUCTION:
                return "processing-instruction()";
            case Type.NAMESPACE:
                return "namespace-node()";
        }
        return "";
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
