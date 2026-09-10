using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class ApplyTemplatesAction : Saxon.Eej.ee.stream.feed.ItemFeed {
        private Saxon.Hej.expr.Component.M mode;
        private Saxon.Hej.expr.XPathContextMajor localContext;
        private Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private Saxon.Hej.expr.instruct.ParameterSet actualParams = null;
        private Saxon.Hej.expr.instruct.ParameterSet tunnelParams = null;
        internal bool tracing;
        public ApplyTemplatesAction(Saxon.Hej.expr.Expression expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expression, result, context) {
            tracing = context.getController().isTracing();
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            Saxon.Hej.expr.instruct.ITemplateCall exp = (Saxon.Hej.expr.instruct.ITemplateCall)(getExpression());
            if ((exp != null)) {
                computeActualParams(exp.getActualParams());
                computeTunnelParams(exp.getTunnelParams());
            }
            localContext = getContext().newContext();
            Saxon.Hej.tree.iter.ManualIterator iter = new Saxon.Hej.tree.iter.ManualIterator();
            localContext.setCurrentIterator(iter);
            localContext.setCurrentComponent(mode);
            localContext.setLocalParameters(actualParams);
            localContext.setTunnelParameters(tunnelParams);
            localContext.setCurrentMode(mode);
            localContext.setCurrentComponent(mode);
            localContext.setCurrentGroupIterator(null);
            localContext.setCurrentMergeGroupIterator(null);
        }
        public void computeActualParams(Saxon.Hej.expr.instruct.WithParam[] @params) {
            this.actualParams = Saxon.Hej.expr.instruct.Instruction.assembleParams(getContext(), @params);
        }
        public void computeTunnelParams(Saxon.Hej.expr.instruct.WithParam[] @params) {
            this.tunnelParams = Saxon.Hej.expr.instruct.Instruction.assembleTunnelParams(getContext(), @params);
        }
        public void setActualParams(Saxon.Hej.expr.instruct.ParameterSet @params) {
            this.actualParams = @params;
        }
        public void setTunnelParams(Saxon.Hej.expr.instruct.ParameterSet @params) {
            this.tunnelParams = @params;
        }
        protected internal Saxon.Hej.expr.XPathContext getLocalContext() {
            return localContext;
        }
        public void setMode(Saxon.Hej.expr.Component.M mode) {
            this.mode = mode;
        }
        public Saxon.Hej.expr.Component getMode() {
            return mode;
        }
        public void setWatchManager(Saxon.Eej.ee.stream.watch.WatchManager watchManager) {
            this.watchManager = watchManager;
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            if (tracing) {
                Saxon.Hej.lib.TraceListener listener = getContext().getController().getTraceListener();
                listener.startCurrentItem(node);
            }
            Saxon.Hej.tree.iter.ManualIterator iter = (Saxon.Hej.tree.iter.ManualIterator)(localContext.getCurrentIterator());
            iter.setContextItem(node);
            iter.incrementPosition();
            Saxon.Eej.ee.stream.ComponentInversion inversion = null;
            Saxon.Hej.trans.rules.Rule rule = getRule(node);
            Saxon.Eej.ee.stream.StreamingSequenceWriter streamingSequenceWriter = new Saxon.Eej.ee.stream.StreamingSequenceWriter(getPipelineConfiguration(), getNextOutputter());
            if ((rule == null)) {
                Saxon.Hej.trans.rules.BuiltInRuleSet builtInRuleSet = mode.getActor().getBuiltInRuleSet();
                if ((builtInRuleSet is Saxon.Hej.trans.rules.RuleSetWithWarnings)) {
                    ((Saxon.Hej.trans.rules.RuleSetWithWarnings)(builtInRuleSet)).outputWarning(node, localContext);
                }
                Saxon.Hej.trans.rules.BuiltInRules[] action = builtInRuleSet.getActionForParentNodes(node.getNodeKind());
                switch (action[0]) {
                  case Saxon.Hej.trans.rules.BuiltInRules.DEEP_COPY:
                    {
                        Saxon.Eej.ee.stream.feed.ItemFeed @out = new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(getResultFeed(), getContext());
                        Saxon.Eej.ee.stream.watch.Watch copyWatch = /*134*/Saxon.Eej.ee.stream.feed.CopyOfFeed.getWatch(@out, Saxon.Hej.pattern.AnchorPattern.getInstance(), watchManager, getContext());
                        watchManager.addWatch(copyWatch, true);
                        return null;
                    }
                  case Saxon.Hej.trans.rules.BuiltInRules.FAIL:
                    {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("No user-defined template rule matches the node "+Saxon.Hej.tree.util.Navigator.getPath(node)), "XTDE0555");
                        err.setLocator(locationId);
                        throw err;
                    }
                  case Saxon.Hej.trans.rules.BuiltInRules.DEEP_SKIP:
                    {
                        return null;
                    }
                  case Saxon.Hej.trans.rules.BuiltInRules.SHALLOW_COPY:
                    {
                        Saxon.Hej.expr.XPathContext context = localContext;
                        Saxon.Hej.@event.Outputter cco = getNextOutputter();
                        Saxon.Eej.ee.stream.feed.ShallowCopyEventFeed copyFeed = new Saxon.Eej.ee.stream.feed.ShallowCopyEventFeed(mode.getActor(), new Saxon.Eej.ee.stream.feed.DecomposingFeed(cco, context), context);
                        Saxon.Eej.ee.stream.watch.ApplyTemplatesAction actor = new Saxon.Eej.ee.stream.watch.ApplyTemplatesAction(null, copyFeed, context);
                        actor.setMode(mode);
                        actor.setWatchManager(watchManager);
                        actor.setActualParams(actualParams);
                        actor.setTunnelParams(tunnelParams);
                        watchManager.addWatch(new Saxon.Eej.ee.stream.watch.Trigger(CHILD_NODE_SELECTION, actor, context), true);
                        return null;
                    }
                  default:
                    {
                        bool applyTemplatesToAttributes = false;
                        bool applyTemplatesToChildren = false;
                        foreach (Saxon.Hej.trans.rules.BuiltInRules i in action) {
                            if ((i == Saxon.Hej.trans.rules.BuiltInRules.APPLY_TEMPLATES_TO_ATTRIBUTES)) {
                                applyTemplatesToAttributes = true;
                            } else {
                                if ((i == Saxon.Hej.trans.rules.BuiltInRules.APPLY_TEMPLATES_TO_CHILDREN)) {
                                    applyTemplatesToChildren = true;
                                }
                            }
                        }
                        if ((applyTemplatesToAttributes && (node.getNodeKind() == Saxon.Hej.type.Type.ELEMENT))) {
                            Saxon.Hej.tree.iter.AxisIterator atts = node.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                            Saxon.Hej.expr.XPathContextMajor c2 = localContext.newContext();
                            c2.trackFocus(atts);
                            mode.getActor().applyTemplates(actualParams, tunnelParams, null, new Saxon.Hej.@event.ComplexContentOutputter(streamingSequenceWriter), c2, locationId);
                        }
                        if (applyTemplatesToChildren) {
                            Saxon.Eej.ee.stream.feed.ItemFeed @out = new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(getResultFeed(), localContext);
                            Saxon.Eej.ee.stream.watch.ApplyTemplatesAction actor = new Saxon.Eej.ee.stream.watch.ApplyTemplatesAction(null, @out, getContext());
                            actor.setMode(mode);
                            actor.setWatchManager(watchManager);
                            actor.setActualParams(actualParams);
                            actor.setTunnelParams(tunnelParams);
                            Saxon.Eej.ee.stream.watch.Watch watch = new Saxon.Eej.ee.stream.watch.Trigger(CHILD_NODE_SELECTION, actor, localContext);
                            watchManager.addWatch(watch, true);
                        }
                        break ;
                    }
                }
            } else {
                Saxon.Eej.ee.trans.TemplateRuleEE template = (Saxon.Eej.ee.trans.TemplateRuleEE)(rule.getAction());
                template.initialize();
                Saxon.Hej.expr.Expression body = template.getBody();
                if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(body) != Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                    localContext.openStackFrame(template.getStackFrameMap());
                    localContext.setLocalParameters(actualParams);
                    localContext.setTunnelParameters(tunnelParams);
                    localContext.setCurrentTemplateRule(rule);
                    localContext.setCurrentMode(mode);
                    localContext.setCurrentComponent(mode);
                    localContext.setCurrentMergeGroupIterator(null);
                    body.process(new Saxon.Hej.@event.ComplexContentOutputter(streamingSequenceWriter), localContext);
                } else {
                    inversion = template.getInversion();
                    localContext.openStackFrame(inversion.getStackFrame());
                    localContext.setLocalParameters(actualParams);
                    localContext.setTunnelParameters(tunnelParams);
                    localContext.setCurrentTemplateRule(rule);
                    localContext.setCurrentMode(mode);
                    localContext.setCurrentComponent(mode);
                    localContext.setCurrentMergeGroupIterator(null);
                }
            }
            if ((inversion != null)) {
                Saxon.Eej.ee.stream.watch.Watch watch = inversion.getWatch(watchManager, new Saxon.Eej.ee.stream.feed.NoOpenOrCloseFeed(getResultFeed(), getContext()), localContext);
                watch.setAnchorNode(node);
                watchManager.addWatch(watch, true);
            }
            return null;
        }
        protected internal virtual Saxon.Hej.trans.rules.Rule getRule(Saxon.Eej.ee.stream.om.FleetingNode node) {
            return mode.getActor().getRule(node, localContext);
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (!(hasFailed())) {
                if (tracing) {
                    Saxon.Hej.lib.TraceListener listener = getContext().getController().getTraceListener();
                    listener.startCurrentItem(item);
                }
                Saxon.Hej.tree.iter.ManualIterator iter = (Saxon.Hej.tree.iter.ManualIterator)(localContext.getCurrentIterator());
                iter.setContextItem(item);
                iter.incrementPosition();
                Saxon.Hej.trans.rules.Rule rule = mode.getActor().getRule(item, localContext);
                localContext.setCurrentTemplateRule(rule);
                localContext.setCurrentComponent(mode);
                localContext.setCurrentMode(mode);
                Saxon.Eej.ee.stream.StreamingSequenceWriter streamingSequenceWriter = new Saxon.Eej.ee.stream.StreamingSequenceWriter(getPipelineConfiguration(), getNextOutputter());
                Saxon.Hej.@event.ComplexContentOutputter output = new Saxon.Hej.@event.ComplexContentOutputter(streamingSequenceWriter);
                if ((rule == null)) {
                    Saxon.Hej.trans.rules.BuiltInRuleSet rules = mode.getActor().getBuiltInRuleSet();
                    try {
                        rules.process(item, actualParams, tunnelParams, output, localContext, Saxon.Hej.expr.parser.Loc.NONE);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        dynamicError(e);
                        return ;
                    }
                } else {
                    Saxon.Hej.expr.instruct.TemplateRule template = (Saxon.Hej.expr.instruct.TemplateRule)(rule.getAction());
                    template.initialize();
                    localContext.openStackFrame(template.getStackFrameMap());
                    try {
                        template.apply(output, localContext);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        dynamicError(e);
                        return ;
                    }
                }
                if (tracing) {
                    Saxon.Hej.lib.TraceListener listener = getContext().getController().getTraceListener();
                    listener.endCurrentItem(item);
                }
            }
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            if (tracing) {
                Saxon.Hej.lib.TraceListener listener = getContext().getController().getTraceListener();
                listener.endCurrentItem(getContext().getContextItem());
            }
        }
        public override void close() {
            base.close();
        }
        private static readonly Saxon.Hej.pattern.AncestorQualifiedPattern CHILD_NODE_SELECTION = new Saxon.Hej.pattern.AncestorQualifiedPattern(new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.MultipleNodeKindTest.CHILD_NODE), Saxon.Hej.pattern.AnchorPattern.getInstance(), Saxon.Hej.om.AxisInfo.PARENT);
    
    }
}