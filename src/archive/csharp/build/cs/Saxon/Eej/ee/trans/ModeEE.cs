using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class ModeEE : Saxon.Eej.trans.ModePE {
        public ModeEE(Saxon.Hej.om.StructuredQName modeName, Saxon.Hej.trans.CompilerInfo compilerInfo) : base (modeName) {
            init();
        }
        private void init() {
        }
        protected internal override void exportUseAccumulators(Saxon.Hej.trace.ExpressionPresenter presenter) {
            System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators = getAccumulators();
            if (((accumulators != null) && !(accumulators.Count==0))) {
                Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
                foreach (Saxon.Hej.expr.accum.Accumulator acc in accumulators) {
                    string name = acc.getAccumulatorName().getEQName();
                    if ((fsb.length() != 0)) {
                        fsb.append(" ");
                    }
                    fsb.append(name);
                }
                presenter.emitAttribute("useAcc", fsb.ToString());
            }
        }
        public override Saxon.Hej.trans.rules.Rule makeRule(Saxon.Hej.pattern.Pattern pattern, Saxon.Hej.trans.rules.RuleTarget action, int precedence, int minImportPrecedence, double priority, int sequence, int part) {
            return new Saxon.Eej.ee.trans.RuleEE(pattern, action, precedence, minImportPrecedence, priority, sequence, part);
        }
        protected internal override Saxon.Eej.ee.trans.PreconditionMatcher makeRuleSearchState(Saxon.Hej.trans.rules.RuleChain chain, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.trans.PreconditionMatcher(chain, context);
        }
        public Saxon.Hej.trans.rules.RuleChain getNamedNodeChain(Saxon.Hej.om.NodeInfo node, Saxon.Hej.expr.XPathContext context) {
            if (node.hasFingerprint()) {
                switch (node.getNodeKind()) {
                  case Saxon.Hej.type.Type.ELEMENT:
                    return namedElementRuleChains.get(node.getFingerprint());
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                    return namedAttributeRuleChains.get(node.getFingerprint());
                }
            } else {
                switch (node.getNodeKind()) {
                  case Saxon.Hej.type.Type.ELEMENT:
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                    return getNamedRuleChain(context, node.getNodeKind(), node.getURI(), node.getLocalPart());
                }
            }
            return null;
        }
        public Saxon.Hej.trans.rules.RuleChain getUnnamedNodeChain(Saxon.Hej.om.NodeInfo node) {
            switch (node.getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return documentRuleChain;
              case Saxon.Hej.type.Type.ELEMENT:
                return unnamedElementRuleChain;
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return unnamedAttributeRuleChain;
              case Saxon.Hej.type.Type.TEXT:
                return textRuleChain;
              case Saxon.Hej.type.Type.COMMENT:
                return commentRuleChain;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return processingInstructionRuleChain;
              case Saxon.Hej.type.Type.NAMESPACE:
                return namespaceRuleChain;
              default:
                throw new Saxon.Ejava.lang.AssertionError("Unknown node kind");
            }
        }
        protected internal override bool ruleMatches(Saxon.Hej.trans.rules.Rule r, Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContextMajor context, Saxon.Hej.trans.rules.RuleSearchState ruleSearchState) {
            return (r.isAlwaysMatches() || (((Saxon.Eej.ee.trans.RuleEE)(r)).checkPreconditions((Saxon.Eej.ee.trans.PreconditionMatcher)(ruleSearchState)) && r.matches(item, context)));
        }
        public override void computeStreamability() {
            processRules(r => {
                Saxon.Eej.ee.trans.TemplateRuleEE t = (Saxon.Eej.ee.trans.TemplateRuleEE)(r.getAction());
                System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
                Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(t.getBody(), Saxon.Eej.ee.trans.ContextItemStaticInfoEE.DEFAULT, reasons);
                if (((ps.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING) || (ps.getPosture() == Saxon.Eej.ee.stream.Posture.ROAMING))) {
                    Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder((("Stylesheet packaging problem: reloaded template with match = "+t.getMatchPattern().toShortString())+" is not streamable. "));
                    foreach (string reason in reasons) {
                        message.append(reason).append(". ");
                    }
                    throw new Saxon.Hej.trans.XPathException(message.ToString());
                }
            } );
        }
        public override void invertStreamableTemplates() {
            if (isDeclaredStreamable()) {
                processRules(r => {
                    Saxon.Eej.ee.trans.TemplateRuleEE t = (Saxon.Eej.ee.trans.TemplateRuleEE)(r.getAction());
                    if ((t.getBody() != null)) {
                        if ((/*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(t.getBody()) == null)) {
                            if (!(t.isActuallyStreamable(new System.Collections.Generic.List<string>()))) {
                                throw new Saxon.Hej.trans.XPathException("Template rule is not streamable", "XTSE3430", t);
                            }
                        }
                        if (!(t.isDeclaredStreamable())) {
                            setStreamable(false);
                        }
                    }
                } );
            }
            if (isDeclaredStreamable()) {
                processRules(r => {
                    Saxon.Eej.ee.trans.TemplateRuleEE t = (Saxon.Eej.ee.trans.TemplateRuleEE)(r.getAction());
                    if ((t.getBody() != null)) {
                        if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(t.getBody()) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                            t.makeInversion();
                        }
                    }
                } );
            }
        }
        public override void optimizeRules() {
            optimizeRuleChain(documentRuleChain);
            optimizeRuleChain(unnamedElementRuleChain);
            foreach (Saxon.Hej.trans.rules.RuleChain chain in namedElementRuleChains.valueSet()) {
                optimizeRuleChain(chain);
            }
            optimizeRuleChain(unnamedAttributeRuleChain);
            foreach (Saxon.Hej.trans.rules.RuleChain chain in namedAttributeRuleChains.valueSet()) {
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
        public void optimizeRuleChain(Saxon.Hej.trans.rules.RuleChain chain) {
            if ((chain.getLength()>=10)) {
                System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions = new System.Collections.Generic.Dictionary<Saxon.Eej.ee.trans.Precondition, System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>>();
                System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics = new System.Collections.Generic.Dictionary<Saxon.Eej.ee.trans.Characteristic, Saxon.Eej.ee.trans.Characteristic>();
                Saxon.Hej.trans.rules.Rule r = chain.head();
                while ((r != null)) {
                    computeCandidatePreconditions((Saxon.Eej.ee.trans.RuleEE)(r), preconditions, characteristics);
                    r = r.getNext();
                }
                preconditions = filterPreconditions(chain, preconditions);
                if (((preconditions.Count>4) || (characteristics.Count>4))) {
                    allocatePreconditionSlots(preconditions);
                    allocateCharacteristicSlots(characteristics);
                    Saxon.Eej.ee.trans.ModeEE.RuleChainOptimizationData data = new Saxon.Eej.ee.trans.ModeEE.RuleChainOptimizationData();
                    data.preconditions = preconditions;
                    data.characteristics = Saxon.Impl.Helpers.MapUtils.KeySet(characteristics);
                    chain.optimizationData = data;
                }
            }
        }
        public class RuleChainOptimizationData {
            public System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions;
            public System.Collections.Generic.ISet<Saxon.Eej.ee.trans.Characteristic> characteristics;
        
        }
        private void computeCandidatePreconditions(Saxon.Eej.ee.trans.RuleEE rule, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics) {
            Saxon.Hej.pattern.Pattern pattern = rule.getPattern();
            /*134*/Saxon.Eej.ee.trans.GeneralPatternOptimizer.getInstance().process(rule, pattern, preconditions, characteristics);
        }
        private System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> filterPreconditions(Saxon.Hej.trans.rules.RuleChain chain, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> candidates) {
            System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> result = new System.Collections.Generic.Dictionary<Saxon.Eej.ee.trans.Precondition, System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>>();
            int chainLength = chain.getLength();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> entry in candidates) {
                int numRules = entry.Value.Count;
                if (((numRules>=3) && (numRules<=(chainLength-3)))) {
                    Saxon.Impl.Helpers.MapUtils.Put(result, entry.Key, entry.Value);
                }
            }
            return result;
        }
        private void allocatePreconditionSlots(System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> conditions) {
            int slot = 0;
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> entry in conditions) {
                entry.Key.setSlotNumber(slot++);
                foreach (Saxon.Eej.ee.trans.RuleEE rule in entry.Value) {
                    rule.addPrecondition(entry.Key);
                }
            }
        }
        private void allocateCharacteristicSlots(System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics) {
            int slot = 0;
            foreach (Saxon.Eej.ee.trans.Characteristic chic in Saxon.Impl.Helpers.MapUtils.KeySet(characteristics)) {
                chic.setSlotNumber(slot++);
            }
        }
        public static System.Collections.Generic.IList<Saxon.Hej.trans.rules.Rule> listRules(Saxon.Hej.trans.rules.RuleChain chain) {
            Saxon.Hej.trans.rules.Rule r = ((chain == null) ? null : chain.head());
            System.Collections.Generic.List<Saxon.Hej.trans.rules.Rule> result = new System.Collections.Generic.List<Saxon.Hej.trans.rules.Rule>();
            while ((r != null)) {
                result.Add(r);
                r = r.getNext();
            }
            return result;
        }
        public static System.Collections.Generic.IList<Saxon.Hej.trans.rules.Rule> orderRules(System.Collections.Generic.IList<Saxon.Hej.trans.rules.Rule> r1, System.Collections.Generic.IList<Saxon.Hej.trans.rules.Rule> r2) {
            System.Collections.Generic.List<Saxon.Hej.trans.rules.Rule> result = new System.Collections.Generic.List<Saxon.Hej.trans.rules.Rule>();
            System.Collections.Generic.IEnumerator<Saxon.Hej.trans.rules.Rule> ir1 = r1.GetEnumerator();
            System.Collections.Generic.IEnumerator<Saxon.Hej.trans.rules.Rule> ir2 = r2.GetEnumerator();
            Saxon.Hej.trans.rules.Rule chain1 = (ir1.MoveNext() /*UNSAFE CONVERSION*/ ? ir1.Current : null);
            Saxon.Hej.trans.rules.Rule chain2 = (ir2.MoveNext() /*UNSAFE CONVERSION*/ ? ir2.Current : null);
            while (((chain1 != null) || (chain2 != null))) {
                if (useFirst(chain1, chain2)) {
                    result.Add(chain1);
                    chain1 = (ir1.MoveNext() /*UNSAFE CONVERSION*/ ? ir1.Current : null);
                } else {
                    result.Add(chain2);
                    chain2 = (ir2.MoveNext() /*UNSAFE CONVERSION*/ ? ir2.Current : null);
                }
            }
            return result;
        }
        protected internal static bool useFirst(Saxon.Hej.trans.rules.Rule chain1, Saxon.Hej.trans.rules.Rule chain2) {
            if ((chain1 == null)) {
                return false;
            } else {
                if ((chain2 == null)) {
                    return true;
                } else {
                    int rank = chain1.compareRank(chain2);
                    if ((rank>0)) {
                        return true;
                    } else {
                        if ((rank == 0)) {
                            return (chain1.getSequence()>=chain2.getSequence());
                        } else {
                            return false;
                        }
                    }
                }
            }
        }
        public static System.Collections.Generic.IList<Saxon.Hej.trans.rules.Rule> orderRules(Saxon.Hej.trans.rules.RuleChain chain1, Saxon.Hej.trans.rules.RuleChain chain2) {
            System.Collections.Generic.List<Saxon.Hej.trans.rules.Rule> result = new System.Collections.Generic.List<Saxon.Hej.trans.rules.Rule>();
            Saxon.Hej.trans.rules.Rule r1 = chain1.head();
            Saxon.Hej.trans.rules.Rule r2 = chain2.head();
            while (((r1 != null) || (r2 != null))) {
                if (useFirst(r1, r2)) {
                    result.Add(r1);
                    r1 = r1.getNext();
                } else {
                    result.Add(r2);
                    r2 = r2.getNext();
                }
            }
            return result;
        }
        public override void explainTemplateRules(Saxon.Hej.trace.ExpressionPresenter @out) {
            base.explainTemplateRules(@out);
        }
        protected internal override void reportAmbiguity(Saxon.Hej.om.Item item, Saxon.Hej.trans.rules.Rule r1, Saxon.Hej.trans.rules.Rule r2, Saxon.Hej.expr.XPathContext c) {
            base.reportAmbiguity(item, r1, r2, c);
        }
        public static string typeName(int type) {
            switch (type) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return "document-node()";
              case Saxon.Hej.type.Type.ELEMENT:
                return "element()";
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return "attribute()";
              case Saxon.Hej.type.Type.TEXT:
                return "text()";
              case Saxon.Hej.type.Type.COMMENT:
                return "comment()";
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return "processing-instruction()";
              case Saxon.Hej.type.Type.NAMESPACE:
                return "namespace-node()";
            }
            return "";
        }
    
    }
}