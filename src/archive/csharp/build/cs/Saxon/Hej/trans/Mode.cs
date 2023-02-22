using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public abstract class Mode : Saxon.Hej.expr.instruct.Actor {
        public static readonly Saxon.Hej.om.StructuredQName OMNI_MODE = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.om.NamespaceUri.SAXON, "_omniMode");
        public static readonly Saxon.Hej.om.StructuredQName UNNAMED_MODE_NAME = new Saxon.Hej.om.StructuredQName("xsl", Saxon.Hej.om.NamespaceUri.XSLT, "unnamed");
        public static readonly Saxon.Hej.om.StructuredQName DEFAULT_MODE_NAME = new Saxon.Hej.om.StructuredQName("xsl", Saxon.Hej.om.NamespaceUri.XSLT, "default");
        protected internal Saxon.Hej.om.StructuredQName modeName;
        private bool streamable;
        public const int RECOVER_WITH_WARNINGS = 1;
        private Saxon.Hej.trans.RecoveryPolicy recoveryPolicy = Saxon.Hej.trans.RecoveryPolicy.RECOVER_WITH_WARNINGS;
        public bool mustBeTyped = false;
        public bool mustBeUntyped = false;
        internal bool hasRules = false;
        internal bool bindingSlotsAllocated = false;
        internal bool modeTracing = false;
        internal Saxon.Hej.value.SequenceType defaultResultType = null;
        private System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators;
        public Mode(Saxon.Hej.om.StructuredQName modeName)  {
            this.modeName = modeName;
        }
        public override Saxon.Hej.expr.Component.M getDeclaringComponent() {
            return (Saxon.Hej.expr.Component.M)(base.getDeclaringComponent());
        }
        public abstract Saxon.Hej.trans.rules.BuiltInRuleSet getBuiltInRuleSet();
        public bool isUnnamedMode() {
            return modeName.Equals(UNNAMED_MODE_NAME);
        }
        public Saxon.Hej.om.StructuredQName getModeName() {
            return modeName;
        }
        public abstract Saxon.Hej.trans.SimpleMode getActivePart();
        public abstract int getMaxPrecedence();
        public abstract int getMaxRank();
        public abstract void computeRankings(int start);
        public string getModeTitle(bool initialCaps) {
            if (initialCaps) {
                return (isUnnamedMode() ? "The unnamed mode" : ("Mode "+getModeName().getDisplayName()));
            } else {
                return (isUnnamedMode() ? "the unnamed mode" : ("mode "+getModeName().getDisplayName()));
            }
        }
        public void setModeTracing(bool tracing) {
            this.modeTracing = tracing;
        }
        public bool isModeTracing() {
            return modeTracing;
        }
        public System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> getAccumulators() {
            if ((accumulators == null)) {
                return new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
            } else {
                return accumulators;
            }
        }
        public void setAccumulators(System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators) {
            this.accumulators = accumulators;
        }
        public override Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_MODE, getModeName());
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return getModeName();
        }
        public abstract bool isEmpty();
        public void setRecoveryPolicy(Saxon.Hej.trans.RecoveryPolicy policy) {
            recoveryPolicy = policy;
        }
        public void setHasRules(bool hasRules) {
            this.hasRules = hasRules;
        }
        public Saxon.Hej.trans.RecoveryPolicy getRecoveryPolicy() {
            return recoveryPolicy;
        }
        public void setStreamable(bool streamable) {
            this.streamable = streamable;
        }
        public bool isDeclaredStreamable() {
            return streamable;
        }
        public abstract System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> getExplicitNamespaces(Saxon.Hej.om.NamePool pool);
        public void setDefaultResultType(Saxon.Hej.value.SequenceType type) {
            defaultResultType = type;
        }
        public Saxon.Hej.value.SequenceType getDefaultResultType() {
            return defaultResultType;
        }
        public abstract void processRules(Saxon.Hej.trans.Mode.RuleAction action);
        public Saxon.Hej.expr.XPathContext makeNewContext(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            c2.setOrigin(context.getController());
            c2.openStackFrame(getStackFrameSlotsNeeded());
            if (!(((context.getCurrentComponent().getActor() is Saxon.Hej.expr.accum.Accumulator)))) {
                c2.setCurrentComponent(context.getCurrentMode());
            }
            return c2;
        }
        public abstract Saxon.Hej.trans.rules.Rule getRule(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context);
        public abstract Saxon.Hej.trans.rules.Rule getRule(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context, System.Predicate<Saxon.Hej.trans.rules.Rule> filter);
        public Saxon.Hej.trans.rules.Rule getRule(Saxon.Hej.om.Item item, int min, int max, Saxon.Hej.expr.XPathContext context) {
            return getRule(item, context, r => {
                int p = r.getPrecedence();
                return ((p>=min) && (p<=max));
            } );
        }
        public Saxon.Hej.trans.rules.Rule getNextMatchRule(Saxon.Hej.om.Item item, Saxon.Hej.trans.rules.Rule currentRule, Saxon.Hej.expr.XPathContext context) {
            return getRule(item, context, r => {
                int comp = r.compareRank(currentRule);
                if ((comp<0)) {
                    return true;
                } else {
                    if ((comp == 0)) {
                        int seqComp = r.getSequence().CompareTo(currentRule.getSequence());
                        if ((seqComp<0)) {
                            return true;
                        } else {
                            if ((seqComp == 0)) {
                                return (r.getPartNumber()<currentRule.getPartNumber());
                            }
                        }
                    }
                }
                return false;
            } );
        }
        public abstract void exportTemplateRules(Saxon.Hej.trace.ExpressionPresenter @out);
        public abstract void explainTemplateRules(Saxon.Hej.trace.ExpressionPresenter @out);
        public Saxon.Hej.expr.instruct.TailCall applyTemplates(Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParameters, Saxon.Hej.om.NodeInfo separator, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContextMajor context, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.om.SequenceIterator iterator = context.getCurrentIterator();
            Saxon.Hej.expr.instruct.TailCall tc = null;
            Saxon.Hej.lib.TraceListener traceListener = null;
            if (controller.isTracing()) {
                traceListener = controller.getTraceListener();
            }
            bool lookahead = ((iterator is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(iterator)).supportsHasNext());
            Saxon.Hej.expr.instruct.TemplateRule previousTemplate = null;
            bool first = true;
            while (true) {
                if ((tc != null)) {
                    if ((lookahead && !(((Saxon.Hej.tree.iter.LookaheadIterator)(iterator)).hasNext()))) {
                        break ;
                    }
                    do {
                        tc = tc.processLeavingTail();
                    } while ((tc != null));
                }
                Saxon.Hej.om.Item item = iterator.next();
                if ((item == null)) {
                    break ;
                }
                if ((separator != null)) {
                    if (first) {
                        first = false;
                    } else {
                        output.append(separator);
                    }
                }
                if (mustBeTyped) {
                    checkMustBeTyped(item);
                } else {
                    if (mustBeUntyped) {
                        checkMustByUntyped(item);
                    }
                }
                if ((traceListener != null)) {
                    traceListener.startRuleSearch();
                }
                Saxon.Hej.trans.rules.Rule rule = getRule(item, context);
                if ((traceListener != null)) {
                    handleTraceListener(rule, item, traceListener);
                }
                Saxon.Hej.trace.TemplateRuleTraceListener ruleTraceListener = null;
                if (modeTracing) {
                    ruleTraceListener = handleRuleTraceListener(ruleTraceListener, controller, locationId, item, rule);
                }
                if ((rule == null)) {
                    getBuiltInRuleSet().process(item, parameters, tunnelParameters, output, context, locationId);
                } else {
                    System.Object[] result = handleRuleNotNull(rule, traceListener, context, item, previousTemplate, parameters, tunnelParameters, output);
                    tc = (Saxon.Hej.expr.instruct.TailCall)(result[0]);
                    previousTemplate = (Saxon.Hej.expr.instruct.TemplateRule)(result[1]);
                }
                if (modeTracing) {
                    ruleTraceListener.leave();
                }
            }
            return tc;
        }
        private void checkMustBeTyped(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                int kind = ((Saxon.Hej.om.NodeInfo)(item)).getNodeKind();
                if (((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.ATTRIBUTE))) {
                    Saxon.Hej.type.SchemaType annotation = ((Saxon.Hej.om.NodeInfo)(item)).getSchemaType();
                    if (((annotation == Saxon.Hej.type.Untyped.getInstance()) || (annotation == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                        throw new Saxon.Hej.trans.XPathException((getModeTitle(true)+" requires typed nodes, but the input is untyped"), "XTTE3100");
                    }
                }
            }
        }
        private void checkMustByUntyped(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                int kind = ((Saxon.Hej.om.NodeInfo)(item)).getNodeKind();
                if (((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.ATTRIBUTE))) {
                    Saxon.Hej.type.SchemaType annotation = ((Saxon.Hej.om.NodeInfo)(item)).getSchemaType();
                    if (!((((annotation == Saxon.Hej.type.Untyped.getInstance()) || (annotation == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))))) {
                        throw new Saxon.Hej.trans.XPathException((getModeTitle(true)+" requires untyped nodes, but the input is typed"), "XTTE3110");
                    }
                }
            }
        }
        private System.Object[] handleRuleNotNull(Saxon.Hej.trans.rules.Rule rule, Saxon.Hej.lib.TraceListener traceListener, Saxon.Hej.expr.XPathContextMajor context, Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.TemplateRule previousTemplate, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParameters, Saxon.Hej.@event.Outputter output) {
            Saxon.Hej.expr.instruct.TemplateRule template = (Saxon.Hej.expr.instruct.TemplateRule)(rule.getAction());
            if ((template != previousTemplate)) {
                previousTemplate = template;
                template.initialize();
                context.openStackFrame(template.getStackFrameMap());
                context.setLocalParameters(parameters);
                context.setTunnelParameters(tunnelParameters);
                context.setCurrentMergeGroupIterator(null);
            }
            context.setCurrentTemplateRule(rule);
            Saxon.Hej.expr.instruct.TailCall tc;
            if ((traceListener != null)) {
                traceListener.startCurrentItem(item);
                tc = template.applyLeavingTail(output, context);
                if ((tc != null)) {
                    do {
                        tc = tc.processLeavingTail();
                    } while ((tc != null));
                }
                traceListener.endCurrentItem(item);
            } else {
                tc = template.applyLeavingTail(output, context);
            }
            return new System.Object[]{tc, previousTemplate};
        }
        private Saxon.Hej.trace.TemplateRuleTraceListener handleRuleTraceListener(Saxon.Hej.trace.TemplateRuleTraceListener ruleTraceListener, Saxon.Hej.Controller controller, Saxon.Hej.s9api.Location locationId, Saxon.Hej.om.Item item, Saxon.Hej.trans.rules.Rule rule) {
            ruleTraceListener = ((Saxon.Hej.trans.XsltController)(controller)).getTemplateRuleTraceListener();
            if ((ruleTraceListener == null)) {
                ruleTraceListener = new Saxon.Hej.trace.TemplateRuleTraceListener(controller.getConfiguration().getLogger());
                ((Saxon.Hej.trans.XsltController)(controller)).setTemplateRuleTraceListener(ruleTraceListener);
            }
            ruleTraceListener.enter("apply-templates", locationId, item, ((rule == null) ? null : (Saxon.Hej.expr.instruct.TemplateRule)(rule.getAction())));
            return ruleTraceListener;
        }
        private void handleTraceListener(Saxon.Hej.trans.rules.Rule rule, Saxon.Hej.om.Item item, Saxon.Hej.lib.TraceListener traceListener) {
            if ((rule == null)) {
                traceListener.endRuleSearch(getBuiltInRuleSet(), this, item);
            } else {
                traceListener.endRuleSearch(rule, this, item);
            }
        }
        public abstract int getStackFrameSlotsNeeded();
        public string getCodeForBuiltInRuleSet(Saxon.Hej.trans.rules.BuiltInRuleSet builtInRuleSet) {
            if ((builtInRuleSet is Saxon.Hej.trans.rules.ShallowCopyRuleSet)) {
                return "SC";
            } else {
                if ((builtInRuleSet is Saxon.Hej.trans.rules.ShallowSkipRuleSet)) {
                    return "SS";
                } else {
                    if ((builtInRuleSet is Saxon.Hej.trans.rules.DeepCopyRuleSet)) {
                        return "DC";
                    } else {
                        if ((builtInRuleSet is Saxon.Hej.trans.rules.DeepSkipRuleSet)) {
                            return "DS";
                        } else {
                            if ((builtInRuleSet is Saxon.Hej.trans.rules.FailRuleSet)) {
                                return "FF";
                            } else {
                                if ((builtInRuleSet is Saxon.Hej.trans.rules.TextOnlyCopyRuleSet)) {
                                    return "TC";
                                } else {
                                    if ((builtInRuleSet is Saxon.Hej.trans.rules.RuleSetWithWarnings)) {
                                        return (getCodeForBuiltInRuleSet(((Saxon.Hej.trans.rules.RuleSetWithWarnings)(builtInRuleSet)).getBaseRuleSet())+"+W");
                                    } else {
                                        return "???";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public Saxon.Hej.trans.rules.BuiltInRuleSet getBuiltInRuleSetForCode(string code) {
            Saxon.Hej.trans.rules.BuiltInRuleSet @base;
            if (code.StartsWith("SC")) {
                @base = Saxon.Hej.trans.rules.ShallowCopyRuleSet.getInstance();
            } else {
                if (code.StartsWith("SS")) {
                    @base = Saxon.Hej.trans.rules.ShallowSkipRuleSet.getInstance();
                } else {
                    if (code.StartsWith("DC")) {
                        @base = Saxon.Hej.trans.rules.DeepCopyRuleSet.getInstance();
                    } else {
                        if (code.StartsWith("DS")) {
                            @base = Saxon.Hej.trans.rules.DeepSkipRuleSet.getInstance();
                        } else {
                            if (code.StartsWith("FF")) {
                                @base = Saxon.Hej.trans.rules.FailRuleSet.getInstance();
                            } else {
                                if (code.StartsWith("TC")) {
                                    @base = Saxon.Hej.trans.rules.TextOnlyCopyRuleSet.getInstance();
                                } else {
                                    throw new Saxon.Ejava.lang.IllegalArgumentException(code);
                                }
                            }
                        }
                    }
                }
            }
            if (code.EndsWith("+W")) {
                @base = new Saxon.Hej.trans.rules.RuleSetWithWarnings(@base);
            }
            return @base;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            int s = presenter.startElement("mode");
            if (!(isUnnamedMode())) {
                presenter.emitAttribute("name", getModeName());
            }
            presenter.emitAttribute("onNo", getCodeForBuiltInRuleSet(getBuiltInRuleSet()));
            string flags = "";
            if (isDeclaredStreamable()) {
                flags += "s";
            }
            if (isUnnamedMode()) {
                flags += "d";
            }
            if (mustBeTyped) {
                flags += "t";
            }
            if (mustBeUntyped) {
                flags += "u";
            }
            if ((recoveryPolicy == Saxon.Hej.trans.RecoveryPolicy.DO_NOT_RECOVER)) {
                flags += "F";
            } else {
                if ((recoveryPolicy == Saxon.Hej.trans.RecoveryPolicy.RECOVER_WITH_WARNINGS)) {
                    flags += "W";
                }
            }
            if (!(hasRules)) {
                flags += "e";
            }
            if (!(flags.Length==0)) {
                presenter.emitAttribute("flags", flags);
            }
            exportUseAccumulators(presenter);
            presenter.emitAttribute("patternSlots", (getStackFrameSlotsNeeded()+""));
            exportTemplateRules(presenter);
            int e = presenter.endElement();
            if ((s != e)) {
                throw new Saxon.Ejava.lang.IllegalStateException(("Export tree unbalanced for mode "+getModeName()));
            }
        }
        protected internal virtual void exportUseAccumulators(Saxon.Hej.trace.ExpressionPresenter presenter) {
        }
        public bool isMustBeTyped() {
            return mustBeTyped;
        }
        public void explain(Saxon.Hej.trace.ExpressionPresenter presenter) {
            int s = presenter.startElement("mode");
            if (!(isUnnamedMode())) {
                presenter.emitAttribute("name", getModeName());
            }
            presenter.emitAttribute("onNo", getCodeForBuiltInRuleSet(getBuiltInRuleSet()));
            string flags = "";
            if (isDeclaredStreamable()) {
                flags += "s";
            }
            if (isUnnamedMode()) {
                flags += "d";
            }
            if (mustBeTyped) {
                flags += "t";
            }
            if (mustBeUntyped) {
                flags += "u";
            }
            if ((recoveryPolicy == Saxon.Hej.trans.RecoveryPolicy.DO_NOT_RECOVER)) {
                flags += "F";
            } else {
                if ((recoveryPolicy == Saxon.Hej.trans.RecoveryPolicy.RECOVER_WITH_WARNINGS)) {
                    flags += "W";
                }
            }
            if (!(flags.Length==0)) {
                presenter.emitAttribute("flags", flags);
            }
            presenter.emitAttribute("patternSlots", (getStackFrameSlotsNeeded()+""));
            explainTemplateRules(presenter);
            int e = presenter.endElement();
            if ((s != e)) {
                throw new Saxon.Ejava.lang.IllegalStateException("tree unbalanced");
            }
        }
        public interface RuleFilter {
            bool testRule(Saxon.Hej.trans.rules.Rule r);
        
        }
        public delegate void RuleAction (Saxon.Hej.trans.rules.Rule r);
    
    }
}