using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class SimpleMode : Saxon.Hej.trans.Mode {
        protected internal readonly Saxon.Hej.trans.rules.RuleChain genericRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.trans.rules.RuleChain atomicValueRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.trans.rules.RuleChain functionItemRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.trans.rules.RuleChain documentRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.trans.rules.RuleChain textRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.trans.rules.RuleChain commentRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.trans.rules.RuleChain processingInstructionRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.trans.rules.RuleChain namespaceRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.trans.rules.RuleChain unnamedElementRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.trans.rules.RuleChain unnamedAttributeRuleChain = new Saxon.Hej.trans.rules.RuleChain();
        protected internal Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.rules.RuleChain> namedElementRuleChains = new Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.rules.RuleChain>(32);
        protected internal Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.rules.RuleChain> namedAttributeRuleChains = new Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.rules.RuleChain>(8);
        protected internal System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.trans.rules.RuleChain> qNamedElementRuleChains;
        protected internal System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.trans.rules.RuleChain> qNamedAttributeRuleChains;
        private Saxon.Hej.trans.rules.BuiltInRuleSet builtInRuleSet = Saxon.Hej.trans.rules.TextOnlyCopyRuleSet.getInstance();
        private Saxon.Hej.trans.rules.Rule mostRecentRule;
        private int mostRecentModuleHash;
        private int stackFrameSlotsNeeded = 0;
        private int highestRank;
        private readonly System.Collections.Generic.IDictionary<string,int> explicitPropertyPrecedences = new System.Collections.Generic.Dictionary<string, int>();
        private readonly System.Collections.Generic.IDictionary<string,string> explicitPropertyValues = new System.Collections.Generic.Dictionary<string, string>();
        public SimpleMode(Saxon.Hej.om.StructuredQName modeName) : base (modeName) {
        }
        public void setBuiltInRuleSet(Saxon.Hej.trans.rules.BuiltInRuleSet defaultRules) {
            this.builtInRuleSet = defaultRules;
            hasRules = true;
        }
        public override Saxon.Hej.trans.rules.BuiltInRuleSet getBuiltInRuleSet() {
            return this.builtInRuleSet;
        }
        public override Saxon.Hej.trans.SimpleMode getActivePart() {
            return this;
        }
        public void checkForConflictingProperties() {
            foreach (System.Collections.Generic.KeyValuePair<string,string> entry in getActivePart().explicitPropertyValues) {
                string prop = entry.Key;
                string value = entry.Value;
                if (value.Equals("##conflict##")) {
                    throw new Saxon.Hej.trans.XPathException((((("For "+getLabel())+", there are conflicting values for xsl:mode/@")+prop)+" at the same import precedence"), "XTSE0545");
                }
                if (prop.Equals("typed")) {
                    mustBeTyped = (("yes".Equals(value) || "strict".Equals(value)) || "lax".Equals(value));
                    mustBeUntyped = "no".Equals(value);
                } else {
                    if (prop.Equals("on-no-match")) {
                        Saxon.Hej.trans.rules.BuiltInRuleSet @base = null;
                        switch (value) {
                          case "text-only-copy":
                            @base = Saxon.Hej.trans.rules.TextOnlyCopyRuleSet.getInstance();
                            break ;
                          case "shallow-copy":
                            @base = Saxon.Hej.trans.rules.ShallowCopyRuleSet.getInstance();
                            break ;
                          case "deep-copy":
                            @base = Saxon.Hej.trans.rules.DeepCopyRuleSet.getInstance();
                            break ;
                          case "shallow-skip":
                            @base = Saxon.Hej.trans.rules.ShallowSkipRuleSet.getInstance();
                            break ;
                          case "deep-skip":
                            @base = Saxon.Hej.trans.rules.DeepSkipRuleSet.getInstance();
                            break ;
                          case "fail":
                            @base = Saxon.Hej.trans.rules.FailRuleSet.getInstance();
                            break ;
                          default:
                            break ;
                        }
                        if ("yes".Equals(Saxon.Impl.Helpers.MapUtils.Get(explicitPropertyValues, "warning-on-no-match"))) {
                            @base = new Saxon.Hej.trans.rules.RuleSetWithWarnings(@base);
                        }
                        setBuiltInRuleSet(@base);
                    }
                }
            }
        }
        public string getLabel() {
            return (isUnnamedMode() ? "the unnamed mode" : ("mode "+modeName.getDisplayName()));
        }
        public static void copyRules(Saxon.Hej.trans.SimpleMode from, Saxon.Hej.trans.SimpleMode to) {
            try {
                from.processRules(r => {
                    Saxon.Hej.trans.rules.Rule r2 = r.copy(false);
                    to.addRule(r2.getPattern(), r2);
                } );
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
            to.mostRecentRule = from.mostRecentRule;
            to.mostRecentModuleHash = from.mostRecentModuleHash;
        }
        protected internal virtual Saxon.Hej.trans.rules.RuleSearchState makeRuleSearchState(Saxon.Hej.trans.rules.RuleChain chain, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.trans.rules.RuleSearchState();
        }
        public override bool isEmpty() {
            return !(hasRules);
        }
        public void setExplicitProperty(string name, string value, int precedence) {
            int p = Saxon.Impl.Helpers.MapUtils.GetOrDefault(explicitPropertyPrecedences, name, System.Int32.MinValue);
            if ((p != System.Int32.MinValue)) {
                if ((p<precedence)) {
                    Saxon.Impl.Helpers.MapUtils.Put(explicitPropertyPrecedences, name, precedence);
                    Saxon.Impl.Helpers.MapUtils.Put(explicitPropertyValues, name, value);
                } else {
                    if ((p == precedence)) {
                        string v = Saxon.Impl.Helpers.MapUtils.Get(explicitPropertyValues, name);
                        if (((v != null)&!(v.Equals(value)))) {
                            Saxon.Impl.Helpers.MapUtils.Put(explicitPropertyValues, name, "##conflict##");
                        }
                    } else {
                    }
                }
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(explicitPropertyPrecedences, name, precedence);
                Saxon.Impl.Helpers.MapUtils.Put(explicitPropertyValues, name, value);
            }
            string typed = Saxon.Impl.Helpers.MapUtils.Get(explicitPropertyValues, "typed");
            mustBeTyped = (("yes".Equals(typed) || "strict".Equals(typed)) || "lax".Equals(typed));
            mustBeUntyped = "no".Equals(typed);
        }
        public string getPropertyValue(string name) {
            return Saxon.Impl.Helpers.MapUtils.Get(explicitPropertyValues, name);
        }
        public override System.Collections.Generic.ISet<string> getExplicitNamespaces(Saxon.Hej.om.NamePool pool) {
            System.Collections.Generic.ISet<string> namespaces = new System.Collections.Generic.HashSet<string>();
            Saxon.Hej.z.IntIterator ii = namedElementRuleChains.keyIterator();
            while (ii.hasNext()) {
                int fp = ii.next();
                namespaces.Add(pool.getURI(fp));
            }
            return namespaces;
        }
        public void addRule(Saxon.Hej.pattern.Pattern pattern, Saxon.Hej.trans.rules.RuleTarget action, Saxon.Hej.style.StylesheetModule module, int precedence, double priority, int position, int part) {
            hasRules = true;
            if ((pattern.getItemType() is Saxon.Hej.type.ErrorType)) {
                return ;
            }
            int moduleHash = module.GetHashCode();
            int minImportPrecedence = module.getMinImportPrecedence();
            Saxon.Hej.trans.rules.Rule newRule = makeRule(pattern, action, precedence, minImportPrecedence, priority, position, part);
            if ((pattern is Saxon.Hej.pattern.NodeTestPattern)) {
                Saxon.Hej.type.ItemType test = pattern.getItemType();
                if ((test is Saxon.Hej.pattern.AnyNodeTest)) {
                    newRule.setAlwaysMatches(true);
                } else {
                    if ((test is Saxon.Hej.pattern.NodeKindTest)) {
                        newRule.setAlwaysMatches(true);
                    } else {
                        if ((test is Saxon.Hej.pattern.NameTest)) {
                            int kind = test.getPrimitiveType();
                            if (((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.ATTRIBUTE))) {
                                newRule.setAlwaysMatches(true);
                            }
                        }
                    }
                }
            }
            mostRecentRule = newRule;
            mostRecentModuleHash = moduleHash;
            addRule(pattern, newRule);
        }
        public virtual Saxon.Hej.trans.rules.Rule makeRule(Saxon.Hej.pattern.Pattern pattern, Saxon.Hej.trans.rules.RuleTarget action, int precedence, int minImportPrecedence, double priority, int sequence, int part) {
            return new Saxon.Hej.trans.rules.Rule(pattern, action, precedence, minImportPrecedence, priority, sequence, part);
        }
        public void addRule(Saxon.Hej.pattern.Pattern pattern, Saxon.Hej.trans.rules.Rule newRule) {
            Saxon.Hej.type.UType uType = pattern.getUType();
            if (uType.Equals(Saxon.Hej.type.UType.ELEMENT)) {
                int fp = pattern.getFingerprint();
                addRuleToNamedOrUnnamedChain(newRule, fp, unnamedElementRuleChain, namedElementRuleChains);
            } else {
                if (uType.Equals(Saxon.Hej.type.UType.ATTRIBUTE)) {
                    int fp = pattern.getFingerprint();
                    addRuleToNamedOrUnnamedChain(newRule, fp, unnamedAttributeRuleChain, namedAttributeRuleChains);
                } else {
                    if (uType.Equals(Saxon.Hej.type.UType.DOCUMENT)) {
                        addRuleToList(newRule, documentRuleChain);
                    } else {
                        if (uType.Equals(Saxon.Hej.type.UType.TEXT)) {
                            addRuleToList(newRule, textRuleChain);
                        } else {
                            if (uType.Equals(Saxon.Hej.type.UType.COMMENT)) {
                                addRuleToList(newRule, commentRuleChain);
                            } else {
                                if (uType.Equals(Saxon.Hej.type.UType.PI)) {
                                    addRuleToList(newRule, processingInstructionRuleChain);
                                } else {
                                    if (uType.Equals(Saxon.Hej.type.UType.NAMESPACE)) {
                                        addRuleToList(newRule, namespaceRuleChain);
                                    } else {
                                        if (Saxon.Hej.type.UType.ANY_ATOMIC.subsumes(uType)) {
                                            addRuleToList(newRule, atomicValueRuleChain);
                                        } else {
                                            if (Saxon.Hej.type.UType.FUNCTION.subsumes(uType)) {
                                                addRuleToList(newRule, functionItemRuleChain);
                                            } else {
                                                addRuleToList(newRule, genericRuleChain);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        protected internal void addRuleToNamedOrUnnamedChain(Saxon.Hej.trans.rules.Rule newRule, int fp, Saxon.Hej.trans.rules.RuleChain unnamedRuleChain, Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.rules.RuleChain> namedRuleChains) {
            if ((fp == -1)) {
                addRuleToList(newRule, unnamedRuleChain);
            } else {
                Saxon.Hej.trans.rules.RuleChain chain = namedRuleChains.get(fp);
                if ((chain == null)) {
                    chain = new Saxon.Hej.trans.rules.RuleChain(newRule);
                    namedRuleChains.put(fp, chain);
                } else {
                    addRuleToList(newRule, chain);
                }
            }
        }
        private void addRuleToList(Saxon.Hej.trans.rules.Rule newRule, Saxon.Hej.trans.rules.RuleChain list) {
            if ((list.head() == null)) {
                list.setHead(newRule);
            } else {
                int precedence = newRule.getPrecedence();
                double priority = newRule.getPriority();
                Saxon.Hej.trans.rules.Rule rule = list.head();
                Saxon.Hej.trans.rules.Rule prev = null;
                while ((rule != null)) {
                    if ((((rule.getPrecedence()<precedence)) || (((rule.getPrecedence() == precedence) && (rule.getPriority()<=priority))))) {
                        newRule.setNext(rule);
                        if ((prev == null)) {
                            list.setHead(newRule);
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
                    System.Diagnostics.Debug.Assert((prev != null));
                    prev.setNext(newRule);
                    newRule.setNext(null);
                }
            }
        }
        public void allocatePatternSlots(int slots) {
            stackFrameSlotsNeeded = System.Math.Max(stackFrameSlotsNeeded, slots);
        }
        public override Saxon.Hej.trans.rules.Rule getRule(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            if ((stackFrameSlotsNeeded>0)) {
                context = makeNewContext(context);
            }
            Saxon.Hej.trans.rules.RuleChain unnamedNodeChain;
            Saxon.Hej.trans.rules.Rule bestRule = null;
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                switch (node.getNodeKind()) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                    unnamedNodeChain = documentRuleChain;
                    break ;
                  case Saxon.Hej.type.Type.ELEMENT:
                    {
                        unnamedNodeChain = unnamedElementRuleChain;
                        Saxon.Hej.trans.rules.RuleChain namedNodeChain;
                        if (node.hasFingerprint()) {
                            namedNodeChain = namedElementRuleChains.get(node.getFingerprint());
                        } else {
                            namedNodeChain = getNamedRuleChain(context, Saxon.Hej.type.Type.ELEMENT, node.getURI(), node.getLocalPart());
                        }
                        if ((namedNodeChain != null)) {
                            bestRule = searchRuleChain(node, context, null, namedNodeChain);
                        }
                        break ;
                    }
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                    {
                        unnamedNodeChain = unnamedAttributeRuleChain;
                        Saxon.Hej.trans.rules.RuleChain namedNodeChain;
                        if (node.hasFingerprint()) {
                            namedNodeChain = namedAttributeRuleChains.get(node.getFingerprint());
                        } else {
                            namedNodeChain = getNamedRuleChain(context, Saxon.Hej.type.Type.ATTRIBUTE, node.getURI(), node.getLocalPart());
                        }
                        if ((namedNodeChain != null)) {
                            bestRule = searchRuleChain(node, context, null, namedNodeChain);
                        }
                        break ;
                    }
                  case Saxon.Hej.type.Type.TEXT:
                    unnamedNodeChain = textRuleChain;
                    break ;
                  case Saxon.Hej.type.Type.COMMENT:
                    unnamedNodeChain = commentRuleChain;
                    break ;
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    unnamedNodeChain = processingInstructionRuleChain;
                    break ;
                  case Saxon.Hej.type.Type.NAMESPACE:
                    unnamedNodeChain = namespaceRuleChain;
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.AssertionError("Unknown node kind");
                }
                if ((unnamedNodeChain != null)) {
                    bestRule = searchRuleChain(node, context, bestRule, unnamedNodeChain);
                }
                bestRule = searchRuleChain(node, context, bestRule, genericRuleChain);
            } else {
                if ((item is Saxon.Hej.value.AtomicValue)) {
                    if ((atomicValueRuleChain != null)) {
                        bestRule = searchRuleChain(item, context, bestRule, atomicValueRuleChain);
                    }
                    bestRule = searchRuleChain(item, context, bestRule, genericRuleChain);
                } else {
                    if ((item is Saxon.Hej.om.Function)) {
                        if ((functionItemRuleChain != null)) {
                            bestRule = searchRuleChain(item, context, bestRule, functionItemRuleChain);
                        }
                        bestRule = searchRuleChain(item, context, bestRule, genericRuleChain);
                    }
                }
            }
            return bestRule;
        }
        protected internal Saxon.Hej.trans.rules.RuleChain getNamedRuleChain(Saxon.Hej.expr.XPathContext c, int kind, string uri, string local) {
            lock (this) {
                if ((qNamedElementRuleChains == null)) {
                    qNamedElementRuleChains = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.trans.rules.RuleChain>(namedElementRuleChains.size());
                    qNamedAttributeRuleChains = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.trans.rules.RuleChain>(namedAttributeRuleChains.size());
                    Saxon.Hej.om.NamePool pool = c.getNamePool();
                    indexByQName(pool, namedElementRuleChains, qNamedElementRuleChains);
                    indexByQName(pool, namedAttributeRuleChains, qNamedAttributeRuleChains);
                }
            }
            return Saxon.Impl.Helpers.MapUtils.Get((((kind == Saxon.Hej.type.Type.ELEMENT) ? qNamedElementRuleChains : qNamedAttributeRuleChains)), new Saxon.Hej.om.StructuredQName("", uri, local));
        }
        private static void indexByQName(Saxon.Hej.om.NamePool pool, Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.rules.RuleChain> indexByFP, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.trans.rules.RuleChain> indexByQN) {
            Saxon.Hej.z.IntIterator ii = indexByFP.keyIterator();
            while (ii.hasNext()) {
                int fp = ii.next();
                Saxon.Hej.trans.rules.RuleChain eChain = indexByFP.get(fp);
                Saxon.Hej.om.StructuredQName name = pool.getStructuredQName(fp);
                Saxon.Impl.Helpers.MapUtils.Put(indexByQN, name, eChain);
            }
        }
        protected internal Saxon.Hej.trans.rules.Rule searchRuleChain(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context, Saxon.Hej.trans.rules.Rule bestRule, Saxon.Hej.trans.rules.RuleChain chain) {
            while (!(((context is Saxon.Hej.expr.XPathContextMajor)))) {
                context = context.getCaller();
            }
            Saxon.Hej.trans.rules.RuleSearchState ruleSearchState = makeRuleSearchState(chain, context);
            Saxon.Hej.trans.rules.Rule head = ((chain == null) ? null : chain.head());
            while ((head != null)) {
                if ((bestRule != null)) {
                    int rank = head.compareRank(bestRule);
                    if ((rank<0)) {
                        break ;
                    } else {
                        if ((rank == 0)) {
                            if (ruleMatches(head, item, (Saxon.Hej.expr.XPathContextMajor)(context), ruleSearchState)) {
                                if ((head.getSequence() != bestRule.getSequence())) {
                                    reportAmbiguity(item, bestRule, head, context);
                                }
                                int seqComp = bestRule.getSequence().CompareTo(head.getSequence());
                                if ((seqComp>0)) {
                                    return bestRule;
                                } else {
                                    if ((seqComp<0)) {
                                        return head;
                                    } else {
                                        bestRule = ((bestRule.getPartNumber()>head.getPartNumber()) ? bestRule : head);
                                    }
                                }
                                break ;
                            } else {
                            }
                        } else {
                            if (ruleMatches(head, item, (Saxon.Hej.expr.XPathContextMajor)(context), ruleSearchState)) {
                                bestRule = head;
                            }
                        }
                    }
                } else {
                    if (ruleMatches(head, item, (Saxon.Hej.expr.XPathContextMajor)(context), ruleSearchState)) {
                        bestRule = head;
                        if ((getRecoveryPolicy() == Saxon.Hej.trans.RecoveryPolicy.RECOVER_SILENTLY)) {
                            break ;
                        }
                    }
                }
                head = head.getNext();
            }
            return bestRule;
        }
        protected internal virtual bool ruleMatches(Saxon.Hej.trans.rules.Rule r, Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContextMajor context, Saxon.Hej.trans.rules.RuleSearchState pre) {
            return (r.isAlwaysMatches() || r.matches(item, context));
        }
        public override Saxon.Hej.trans.rules.Rule getRule(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context, System.Predicate<Saxon.Hej.trans.rules.Rule> filter) {
            if ((stackFrameSlotsNeeded>0)) {
                context = makeNewContext(context);
            }
            Saxon.Hej.trans.rules.RuleSearchState ruleSearchState;
            Saxon.Hej.trans.rules.Rule bestRule = null;
            Saxon.Hej.trans.rules.RuleChain unnamedNodeChain;
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                switch (node.getNodeKind()) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                    unnamedNodeChain = documentRuleChain;
                    break ;
                  case Saxon.Hej.type.Type.ELEMENT:
                    {
                        unnamedNodeChain = unnamedElementRuleChain;
                        Saxon.Hej.trans.rules.RuleChain namedNodeChain;
                        if (node.hasFingerprint()) {
                            namedNodeChain = namedElementRuleChains.get(node.getFingerprint());
                        } else {
                            namedNodeChain = getNamedRuleChain(context, Saxon.Hej.type.Type.ELEMENT, node.getURI(), node.getLocalPart());
                        }
                        if ((namedNodeChain != null)) {
                            ruleSearchState = makeRuleSearchState(namedNodeChain, context);
                            bestRule = searchRuleChain(item, context, null, namedNodeChain, ruleSearchState, filter);
                        }
                        break ;
                    }
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                    {
                        unnamedNodeChain = unnamedAttributeRuleChain;
                        Saxon.Hej.trans.rules.RuleChain namedNodeChain;
                        if (node.hasFingerprint()) {
                            namedNodeChain = namedAttributeRuleChains.get(node.getFingerprint());
                        } else {
                            namedNodeChain = getNamedRuleChain(context, Saxon.Hej.type.Type.ATTRIBUTE, node.getURI(), node.getLocalPart());
                        }
                        if ((namedNodeChain != null)) {
                            ruleSearchState = makeRuleSearchState(namedNodeChain, context);
                            bestRule = searchRuleChain(item, context, null, namedNodeChain, ruleSearchState, filter);
                        }
                        break ;
                    }
                  case Saxon.Hej.type.Type.TEXT:
                    unnamedNodeChain = textRuleChain;
                    break ;
                  case Saxon.Hej.type.Type.COMMENT:
                    unnamedNodeChain = commentRuleChain;
                    break ;
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    unnamedNodeChain = processingInstructionRuleChain;
                    break ;
                  case Saxon.Hej.type.Type.NAMESPACE:
                    unnamedNodeChain = namespaceRuleChain;
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.AssertionError("Unknown node kind");
                }
                ruleSearchState = makeRuleSearchState(unnamedNodeChain, context);
                bestRule = searchRuleChain(item, context, bestRule, unnamedNodeChain, ruleSearchState, filter);
                ruleSearchState = makeRuleSearchState(genericRuleChain, context);
                return searchRuleChain(item, context, bestRule, genericRuleChain, ruleSearchState, filter);
            } else {
                if ((item is Saxon.Hej.value.AtomicValue)) {
                    if ((atomicValueRuleChain != null)) {
                        ruleSearchState = makeRuleSearchState(atomicValueRuleChain, context);
                        bestRule = searchRuleChain(item, context, bestRule, atomicValueRuleChain, ruleSearchState, filter);
                    }
                    ruleSearchState = makeRuleSearchState(genericRuleChain, context);
                    bestRule = searchRuleChain(item, context, bestRule, genericRuleChain, ruleSearchState, filter);
                    return bestRule;
                } else {
                    if ((item is Saxon.Hej.om.Function)) {
                        if ((functionItemRuleChain != null)) {
                            ruleSearchState = makeRuleSearchState(functionItemRuleChain, context);
                            bestRule = searchRuleChain(item, context, bestRule, functionItemRuleChain, ruleSearchState, filter);
                        }
                        ruleSearchState = makeRuleSearchState(genericRuleChain, context);
                        bestRule = searchRuleChain(item, context, bestRule, genericRuleChain, ruleSearchState, filter);
                        return bestRule;
                    } else {
                        return null;
                    }
                }
            }
        }
        protected internal Saxon.Hej.trans.rules.Rule searchRuleChain(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context, Saxon.Hej.trans.rules.Rule bestRule, Saxon.Hej.trans.rules.RuleChain chain, Saxon.Hej.trans.rules.RuleSearchState ruleSearchState, System.Predicate<Saxon.Hej.trans.rules.Rule> filter) {
            Saxon.Hej.trans.rules.Rule head = ((chain == null) ? null : chain.head());
            while (!(((context is Saxon.Hej.expr.XPathContextMajor)))) {
                context = context.getCaller();
            }
            while ((head != null)) {
                if (((filter == null) || filter(head))) {
                    if ((bestRule != null)) {
                        int rank = head.compareRank(bestRule);
                        if ((rank<0)) {
                            break ;
                        } else {
                            if ((rank == 0)) {
                                if (ruleMatches(head, item, (Saxon.Hej.expr.XPathContextMajor)(context), ruleSearchState)) {
                                    reportAmbiguity(item, bestRule, head, context);
                                    bestRule = ((bestRule.getSequence()>head.getSequence()) ? bestRule : head);
                                    break ;
                                } else {
                                }
                            } else {
                                if (ruleMatches(head, item, (Saxon.Hej.expr.XPathContextMajor)(context), ruleSearchState)) {
                                    bestRule = head;
                                }
                            }
                        }
                    } else {
                        if (ruleMatches(head, item, (Saxon.Hej.expr.XPathContextMajor)(context), ruleSearchState)) {
                            bestRule = head;
                            if ((getRecoveryPolicy() == Saxon.Hej.trans.RecoveryPolicy.RECOVER_SILENTLY)) {
                                break ;
                            }
                        }
                    }
                }
                head = head.getNext();
            }
            return bestRule;
        }
        protected internal virtual void reportAmbiguity(Saxon.Hej.om.Item item, Saxon.Hej.trans.rules.Rule r1, Saxon.Hej.trans.rules.Rule r2, Saxon.Hej.expr.XPathContext c) {
            if ((getRecoveryPolicy() == Saxon.Hej.trans.RecoveryPolicy.RECOVER_SILENTLY)) {
                return ;
            }
            if (((r1.getAction() == r2.getAction()) && (r1.getSequence() == r2.getSequence()))) {
                return ;
            }
            string path;
            string errorCode = "XTDE0540";
            if ((item is Saxon.Hej.om.NodeInfo)) {
                path = Saxon.Hej.tree.util.Navigator.getPath((Saxon.Hej.om.NodeInfo)(item));
            } else {
                path = item.toShortString();
            }
            Saxon.Hej.pattern.Pattern pat1 = r1.getPattern();
            Saxon.Hej.pattern.Pattern pat2 = r2.getPattern();
            string message;
            if ((r1.getAction() == r2.getAction())) {
                message = ((((((((("Ambiguous rule match for "+path)+". ")+"Matches \"")+showPattern(pat1))+"\" on line ")+pat1.getLocation().getLineNumber())+" of ")+pat1.getLocation().getSystemId())+", a rule which appears in the stylesheet more than once, because the containing module was included more than once");
            } else {
                message = (((((((((((((("Ambiguous rule match for "+path)+'\n')+"Matches both \"")+showPattern(pat1))+"\" on line ")+pat1.getLocation().getLineNumber())+" of ")+pat1.getLocation().getSystemId())+"\nand \"")+showPattern(pat2))+"\" on line ")+pat2.getLocation().getLineNumber())+" of ")+pat2.getLocation().getSystemId());
            }
            switch (getRecoveryPolicy()) {
              case Saxon.Hej.trans.RecoveryPolicy.DO_NOT_RECOVER:
                throw new Saxon.Hej.trans.XPathException(message, errorCode, getLocation());
              case Saxon.Hej.trans.RecoveryPolicy.RECOVER_WITH_WARNINGS:
                c.getController().warning(message, errorCode, getLocation());
                break ;
              case Saxon.Hej.trans.RecoveryPolicy.RECOVER_SILENTLY:
              default:
                break;
            }
        }
        private static string showPattern(Saxon.Hej.pattern.Pattern p) {
            return Saxon.Hej.value.Whitespace.collapseWhitespace(Saxon.Hej.str.StringView.of(p.toShortString()).tidy()).ToString();
        }
        public virtual void prepareStreamability() {
        }
        public override void allocateAllBindingSlots(Saxon.Hej.style.StylesheetPackage pack) {
            if (((getDeclaringComponent().getDeclaringPackage() == pack) && !(bindingSlotsAllocated))) {
                forceAllocateAllBindingSlots(pack, this, getDeclaringComponent().getComponentBindings());
                bindingSlotsAllocated = true;
            }
        }
        public static void forceAllocateAllBindingSlots(Saxon.Hej.style.StylesheetPackage pack, Saxon.Hej.trans.SimpleMode mode, System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> bindings) {
            System.Collections.Generic.ISet<Saxon.Hej.expr.instruct.TemplateRule> rulesProcessed = new System.Collections.Generic.HashSet<Saxon.Hej.expr.instruct.TemplateRule>();
            Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.pattern.Pattern,bool> patternsProcessed = new Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.pattern.Pattern, bool>();
            try {
                mode.processRules(r => {
                    Saxon.Hej.pattern.Pattern pattern = r.getPattern();
                    if (!(patternsProcessed.ContainsKey(pattern))) {
                        allocateBindingSlotsRecursive(pack, mode, pattern, bindings);
                        Saxon.Impl.Helpers.MapUtils.Put(patternsProcessed, pattern, true);
                    }
                    Saxon.Hej.expr.instruct.TemplateRule tr = (Saxon.Hej.expr.instruct.TemplateRule)(r.getAction());
                    if (((tr.getBody() != null) && !(rulesProcessed.Contains(tr)))) {
                        allocateBindingSlotsRecursive(pack, mode, tr.getBody(), bindings);
                        rulesProcessed.Add(tr);
                    }
                } );
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
        public virtual void computeStreamability() {
        }
        public virtual void invertStreamableTemplates() {
        }
        public override void explainTemplateRules(Saxon.Hej.trace.ExpressionPresenter @out) {
            Saxon.Hej.trans.Mode.RuleAction action = r => r.export(@out, isDeclaredStreamable());
            try {
                processRules(action, new Saxon.Hej.trans.SimpleMode.RuleGroupExplainAction(@out));
            } catch (Saxon.Hej.trans.XPathException err) {
            }
        }
        private class RuleGroupExplainAction : Saxon.Hej.trans.SimpleMode.RuleGroupAction {
            private string type;
            private readonly Saxon.Hej.trace.ExpressionPresenter presenter;
            public RuleGroupExplainAction(Saxon.Hej.trace.ExpressionPresenter presenter)  {
                this.presenter = presenter;
            }
            public void start() {
                presenter.startElement("ruleSet");
                presenter.emitAttribute("type", type);
            }
            public void setLabel(string type) {
                this.type = type;
            }
            public void start(int i) {
                presenter.startElement("ruleChain");
                presenter.emitAttribute("key", presenter.getNamePool().getClarkName(i));
            }
            public void end() {
                presenter.endElement();
            }
        
        }
        public override void exportTemplateRules(Saxon.Hej.trace.ExpressionPresenter @out) {
            processRules(r => r.export(@out, isDeclaredStreamable()));
        }
        public override void processRules(Saxon.Hej.trans.Mode.RuleAction action) {
            processRules(action, null);
        }
        public void processRules(Saxon.Hej.trans.Mode.RuleAction action, Saxon.Hej.trans.SimpleMode.RuleGroupAction group) {
            processRuleChain(documentRuleChain, action, setGroup(group, "document-node()"));
            processRuleChain(unnamedElementRuleChain, action, setGroup(group, "element()"));
            processRuleChains(namedElementRuleChains, action, setGroup(group, "namedElements"));
            processRuleChain(unnamedAttributeRuleChain, action, setGroup(group, "attribute()"));
            processRuleChains(namedAttributeRuleChains, action, setGroup(group, "namedAttributes"));
            processRuleChain(textRuleChain, action, setGroup(group, "text()"));
            processRuleChain(commentRuleChain, action, setGroup(group, "comment()"));
            processRuleChain(processingInstructionRuleChain, action, setGroup(group, "processing-instruction()"));
            processRuleChain(namespaceRuleChain, action, setGroup(group, "namespace()"));
            processRuleChain(genericRuleChain, action, setGroup(group, "node()"));
            processRuleChain(atomicValueRuleChain, action, setGroup(group, "atomicValue"));
            processRuleChain(functionItemRuleChain, action, setGroup(group, "function()"));
        }
        protected internal Saxon.Hej.trans.SimpleMode.RuleGroupAction setGroup(Saxon.Hej.trans.SimpleMode.RuleGroupAction group, string type) {
            if ((group != null)) {
                group.setLabel(type);
            }
            return group;
        }
        public void processRuleChains(Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.rules.RuleChain> chains, Saxon.Hej.trans.Mode.RuleAction action, Saxon.Hej.trans.SimpleMode.RuleGroupAction group) {
            if ((chains.size()>0)) {
                if ((group != null)) {
                    group.start();
                }
                Saxon.Hej.z.IntIterator ii = chains.keyIterator();
                while (ii.hasNext()) {
                    int i = ii.next();
                    if ((group != null)) {
                        group.start(i);
                    }
                    Saxon.Hej.trans.rules.RuleChain r = chains.get(i);
                    processRuleChain(r, action, null);
                    if ((group != null)) {
                        group.end();
                    }
                }
                if ((group != null)) {
                    group.end();
                }
            }
        }
        public void processRuleChain(Saxon.Hej.trans.rules.RuleChain chain, Saxon.Hej.trans.Mode.RuleAction action) {
            Saxon.Hej.trans.rules.Rule r = ((chain == null) ? null : chain.head());
            while ((r != null)) {
                /*Delegate.processRule()*/action(r);
                r = r.getNext();
            }
        }
        public void processRuleChain(Saxon.Hej.trans.rules.RuleChain chain, Saxon.Hej.trans.Mode.RuleAction action, Saxon.Hej.trans.SimpleMode.RuleGroupAction group) {
            Saxon.Hej.trans.rules.Rule r = ((chain == null) ? null : chain.head());
            if ((r != null)) {
                if ((group != null)) {
                    group.start();
                }
                while ((r != null)) {
                    /*Delegate.processRule()*/action(r);
                    r = r.getNext();
                }
                if ((group != null)) {
                    group.end();
                }
            }
        }
        public virtual void optimizeRules() {
        }
        public override int getMaxPrecedence() {
            try {
                System.Collections.Generic.IList<int> capturedPrecedence = new System.Collections.Generic.List<int>(1);
                capturedPrecedence.Add(0);
                processRules(r => {
                    if ((r.getPrecedence()>capturedPrecedence[0])) {
                        capturedPrecedence[0] = r.getPrecedence();
                    }
                } );
                return capturedPrecedence[0];
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
        public override void computeRankings(int start) {
            Saxon.Hej.trans.SimpleMode.RuleSorter sorter = new Saxon.Hej.trans.SimpleMode.RuleSorter(start);
            processRules(it => sorter.addRule(it) /* converted from instance method reference */);
            sorter.allocateRanks();
            highestRank = (start+sorter.getNumberOfRules());
        }
        public override int getMaxRank() {
            return highestRank;
        }
        private class RuleSorter {
            public System.Collections.Generic.List<Saxon.Hej.trans.rules.Rule> rules = new System.Collections.Generic.List<Saxon.Hej.trans.rules.Rule>(100);
            private readonly int start;
            public RuleSorter(int start)  {
                this.start = start;
            }
            public void addRule(Saxon.Hej.trans.rules.Rule rule) {
                rules.Add(rule);
            }
            public void allocateRanks() {
                rules.Sort((x, y) => x.compareComputedRank(y));
                int rank = start;
                for (int i = 0; (i<rules.Count); i++) {
                    if (((i>0) && (rules[(i-1)].compareComputedRank(rules[i]) != 0))) {
                        rank++;
                    }
                    rules[i].setRank(rank);
                }
            }
            public int getNumberOfRules() {
                return rules.Count;
            }
        
        }
        public interface RuleGroupAction {
            void setLabel(string s);
            void start();
            void start(int i);
            void end();
        
        }
        public void allocateAllPatternSlots() {
            System.Collections.Generic.IList<int> count = new System.Collections.Generic.List<int>(1);
            count.Add(0);
            Saxon.Hej.expr.instruct.SlotManager slotManager = new Saxon.Hej.expr.instruct.SlotManager();
            try {
                processRules(r => {
                    int slots = r.getPattern().allocateSlots(slotManager, 0);
                    int max = System.Math.Max(count[0], slots);
                    count[0] = max;
                } );
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
            stackFrameSlotsNeeded = count[0];
        }
        public override int getStackFrameSlotsNeeded() {
            return stackFrameSlotsNeeded;
        }
        public void setStackFrameSlotsNeeded(int slots) {
            this.stackFrameSlotsNeeded = slots;
        }
    
    }
}