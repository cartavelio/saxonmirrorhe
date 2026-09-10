using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class SelectedElementsSpaceStrippingRule : Saxon.Hej.om.SpaceStrippingRule {
        private Saxon.Hej.trans.rules.Rule anyElementRule = null;
        private Saxon.Hej.trans.rules.Rule unnamedElementRuleChain = null;
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName,Saxon.Hej.trans.rules.Rule> namedElementRules = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName,Saxon.Hej.trans.rules.Rule>(32);
        private int sequence = 0;
        private readonly bool rejectDuplicates;
        public SelectedElementsSpaceStrippingRule(bool rejectDuplicates)  {
            this.rejectDuplicates = rejectDuplicates;
        }
        public int isSpacePreserving(Saxon.Hej.om.NodeName fingerprint, Saxon.Hej.type.SchemaType schemaType) {
            Saxon.Hej.trans.rules.Rule rule = getRule(fingerprint);
            if ((rule == null)) {
                return Saxon.Hej.@event.Stripper.ALWAYS_PRESERVE;
            }
            return ((rule.getAction() == Saxon.Hej.@event.Stripper.PRESERVE) ? Saxon.Hej.@event.Stripper.ALWAYS_PRESERVE : Saxon.Hej.@event.Stripper.STRIP_DEFAULT);
        }
        public void addRule(Saxon.Hej.pattern.NodeTest test, Saxon.Hej.@event.Stripper.StripRuleTarget action, Saxon.Hej.style.StylesheetModule module, int lineNumber) {
            int precedence = module.getPrecedence();
            int minImportPrecedence = module.getMinImportPrecedence();
            Saxon.Hej.pattern.NodeTestPattern pattern = new Saxon.Hej.pattern.NodeTestPattern(test);
            addRule(pattern, action, precedence, minImportPrecedence);
        }
        public void addRule(Saxon.Hej.pattern.NodeTestPattern pattern, Saxon.Hej.@event.Stripper.StripRuleTarget action, int precedence, int minImportPrecedence) {
            Saxon.Hej.pattern.NodeTest test = pattern.getNodeTest();
            double priority = test.getDefaultPriority();
            Saxon.Hej.trans.rules.Rule newRule = new Saxon.Hej.trans.rules.Rule(pattern, action, precedence, minImportPrecedence, priority, sequence++, 0);
            int prio = ((priority == 0) ? 2 : ((priority == -(0.25)) ? 1 : 0));
            newRule.setRank(((((precedence<<18))+((prio<<16)))+sequence));
            if ((test is Saxon.Hej.pattern.NodeKindTest)) {
                newRule.setAlwaysMatches(true);
                anyElementRule = addRuleToList(newRule, anyElementRule, true);
            } else {
                if ((test is Saxon.Hej.pattern.NameTest)) {
                    newRule.setAlwaysMatches(true);
                    int fp = test.getFingerprint();
                    Saxon.Hej.om.NamePool pool = ((Saxon.Hej.pattern.NameTest)(test)).getNamePool();
                    Saxon.Hej.om.FingerprintedQName key = new Saxon.Hej.om.FingerprintedQName(pool.getUnprefixedQName(fp), pool);
                    Saxon.Hej.trans.rules.Rule chain = Saxon.Impl.Helpers.MapUtils.Get(namedElementRules, key);
                    Saxon.Impl.Helpers.MapUtils.Put(namedElementRules, key, addRuleToList(newRule, chain, true));
                } else {
                    unnamedElementRuleChain = addRuleToList(newRule, unnamedElementRuleChain, false);
                }
            }
        }
        private Saxon.Hej.trans.rules.Rule addRuleToList(Saxon.Hej.trans.rules.Rule newRule, Saxon.Hej.trans.rules.Rule list, bool dropRemainder) {
            if ((list == null)) {
                return newRule;
            }
            int precedence = newRule.getPrecedence();
            Saxon.Hej.trans.rules.Rule rule = list;
            Saxon.Hej.trans.rules.Rule prev = null;
            while ((rule != null)) {
                if ((rule.getPrecedence()<=precedence)) {
                    if (((rejectDuplicates && (rule.getPrecedence() == precedence)) && !(rule.getAction().Equals(newRule.getAction())))) {
                        throw new Saxon.Hej.trans.XPathException((("There are conflicting xsl:strip-space and xsl:preserve-space declarations for "+rule.getPattern())+" at the same import precedence"), "XTSE0270");
                    }
                    newRule.setNext((dropRemainder ? null : rule));
                    if ((prev == null)) {
                        return newRule;
                    } else {
                        prev.setNext(newRule);
                    }
                    break ;
                } else {
                    prev = rule;
                    rule = rule.getNext();
                }
            }
            if ((rule == null)) {
                prev.setNext(newRule);
                newRule.setNext(null);
            }
            return list;
        }
        public Saxon.Hej.trans.rules.Rule getRule(Saxon.Hej.om.NodeName nodeName) {
            Saxon.Hej.trans.rules.Rule bestRule = Saxon.Impl.Helpers.MapUtils.Get(namedElementRules, nodeName);
            if ((unnamedElementRuleChain != null)) {
                bestRule = searchRuleChain(nodeName, bestRule, unnamedElementRuleChain);
            }
            if ((anyElementRule != null)) {
                bestRule = searchRuleChain(nodeName, bestRule, anyElementRule);
            }
            return bestRule;
        }
        private Saxon.Hej.trans.rules.Rule searchRuleChain(Saxon.Hej.om.NodeName nodeName, Saxon.Hej.trans.rules.Rule bestRule, Saxon.Hej.trans.rules.Rule head) {
            while ((head != null)) {
                if ((bestRule != null)) {
                    int rank = head.compareRank(bestRule);
                    if ((rank<0)) {
                        break ;
                    } else {
                        if ((rank == 0)) {
                            if ((head.isAlwaysMatches() || ((Saxon.Hej.pattern.NodeTest)(head.getPattern().getItemType())).matches(Saxon.Hej.type.Type.ELEMENT, nodeName, null))) {
                                bestRule = head;
                                break ;
                            } else {
                            }
                        } else {
                            if ((head.isAlwaysMatches() || ((Saxon.Hej.pattern.NodeTest)(head.getPattern().getItemType())).matches(Saxon.Hej.type.Type.ELEMENT, nodeName, null))) {
                                bestRule = head;
                            }
                        }
                    }
                } else {
                    if ((head.isAlwaysMatches() || ((Saxon.Hej.pattern.NodeTest)(head.getPattern().getItemType())).matches(Saxon.Hej.type.Type.ELEMENT, nodeName, null))) {
                        bestRule = head;
                        break ;
                    }
                }
                head = head.getNext();
            }
            return bestRule;
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.trans.rules.Rule> getRankedRules() {
            System.Collections.Generic.SortedDictionary<int,Saxon.Hej.trans.rules.Rule> treeMap = new System.Collections.Generic.SortedDictionary<int,Saxon.Hej.trans.rules.Rule>();
            Saxon.Hej.trans.rules.Rule rule = anyElementRule;
            while ((rule != null)) {
                Saxon.Impl.Helpers.MapUtils.Put(treeMap, -(rule.getRank()), rule);
                rule = rule.getNext();
            }
            rule = unnamedElementRuleChain;
            while ((rule != null)) {
                Saxon.Impl.Helpers.MapUtils.Put(treeMap, -(rule.getRank()), rule);
                rule = rule.getNext();
            }
            foreach (Saxon.Hej.trans.rules.Rule r in namedElementRules.Values) {
                Saxon.Impl.Helpers.MapUtils.Put(treeMap, -(r.getRank()), r);
            }
            return treeMap.Values.GetEnumerator();
        }
        public Saxon.Hej.@event.ProxyReceiver makeStripper(Saxon.Hej.@event.Receiver next) {
            return new Saxon.Hej.@event.Stripper(this, next);
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("strip");
            Saxon.Hej.trans.rules.Rule rule = anyElementRule;
            while ((rule != null)) {
                exportRule(rule, presenter);
                rule = rule.getNext();
            }
            rule = unnamedElementRuleChain;
            while ((rule != null)) {
                exportRule(rule, presenter);
                rule = rule.getNext();
            }
            foreach (Saxon.Hej.trans.rules.Rule r in namedElementRules.Values) {
                exportRule(r, presenter);
            }
            presenter.endElement();
        }
        private static void exportRule(Saxon.Hej.trans.rules.Rule rule, Saxon.Hej.trace.ExpressionPresenter presenter) {
            string which = ((rule.getAction() == Saxon.Hej.@event.Stripper.STRIP) ? "s" : "p");
            presenter.startElement(which);
            presenter.emitAttribute("test", Saxon.Hej.type.AlphaCode.fromItemType(rule.getPattern().getItemType()));
            presenter.emitAttribute("prec", (rule.getPrecedence()+""));
            presenter.endElement();
        }
    
    }
}