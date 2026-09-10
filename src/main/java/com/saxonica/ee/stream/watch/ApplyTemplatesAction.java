package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.ComponentInversion;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.StreamingSequenceWriter;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.*;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.trans.TemplateRuleEE;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Component;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.instruct.*;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.AncestorQualifiedPattern;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.pattern.MultipleNodeKindTest;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.rules.BuiltInRuleSet;
import net.sf.saxon.trans.rules.BuiltInRules;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.trans.rules.RuleSetWithWarnings;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.Type;

/**
 * This watch implements xsl:apply-templates in streaming mode. It watches for the nodes selected by the
 * xsl:apply-templates instruction; when such a node is encountered, it locates the relevant (inverted)
 * template, and activates it.
 */


public class ApplyTemplatesAction extends ItemFeed {

    private Component.M mode;

    // localContext contains the iterator over the nodes selected by the apply-templates instruction,
    // and is the context in which the matched template rules are evaluated
    private XPathContextMajor localContext;
    private WatchManager watchManager;

    private ParameterSet actualParams = null;
    private ParameterSet tunnelParams = null;

    boolean tracing;


    public ApplyTemplatesAction(Expression expression, ItemFeed result, XPathContext context) {
        super(expression, result, context);
        tracing = context.getController().isTracing();
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        // TODO: make tracing work
//        if (tracing && getExpression() != null) {
//            getContext().getController().getTraceListener().enter(getExpression(), getContext());
//        }
        ITemplateCall exp = (ITemplateCall) getExpression();
        if (exp != null) {
            // Not a built-in template
            computeActualParams(exp.getActualParams());
            computeTunnelParams(exp.getTunnelParams());
        }
        localContext = getContext().newContext();
        ManualIterator iter = new ManualIterator();
        localContext.setCurrentIterator(iter);
        localContext.setCurrentComponent(mode);
        localContext.setLocalParameters(actualParams);
        localContext.setTunnelParameters(tunnelParams);
        localContext.setCurrentMode(mode);
        localContext.setCurrentComponent(mode);
        localContext.setCurrentGroupIterator(null);
        localContext.setCurrentMergeGroupIterator(null);
    }

    public void computeActualParams(WithParam[] params) throws XPathException {
        this.actualParams = Instruction.assembleParams(getContext(), params);
    }

    public void computeTunnelParams(WithParam[] params) throws XPathException {
        this.tunnelParams = Instruction.assembleTunnelParams(getContext(), params);
    }

    public void setActualParams(ParameterSet params) {
        this.actualParams = params;
    }

    public void setTunnelParams(ParameterSet params) {
        this.tunnelParams = params;
    }

    protected XPathContext getLocalContext() {
        return localContext;
    }

    public void setMode(Component.M mode) {
        this.mode = mode;
    }

    public Component getMode() {
        return mode;
    }

    public void setWatchManager(WatchManager watchManager) {
        this.watchManager = watchManager;
    }

    /**
     * Signal that an element or document node has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement or startDocument event that matches the selection.
     *
     *
     * @param node       the element or document node whose start event has been matched
     * @param locationId the location associated with the element or document node (may be
     *                   the location of the instruction that created it)
     * @return a Receiver to be notified of all events starting with the startElement/startDocument event
     *         for the matched element, and ending with the endElement event for that element; or null if this
     *         feature is not required.
     * @throws XPathException May be raised if a dynamic error occurs
     */

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (tracing) {
            TraceListener listener = getContext().getController().getTraceListener();
            listener.startCurrentItem(node);
        }
        ManualIterator iter = (ManualIterator) localContext.getCurrentIterator();
        iter.setContextItem(node);
        iter.incrementPosition();

        ComponentInversion inversion = null;
        Rule rule = getRule(node);
        final StreamingSequenceWriter streamingSequenceWriter = new StreamingSequenceWriter(getPipelineConfiguration(), getNextOutputter());
        if (rule == null) {
            BuiltInRuleSet builtInRuleSet = mode.getActor().getBuiltInRuleSet();
            if (builtInRuleSet instanceof RuleSetWithWarnings) {
                ((RuleSetWithWarnings)builtInRuleSet).outputWarning(node, localContext);
            }
            BuiltInRules[] action = builtInRuleSet.getActionForParentNodes(node.getNodeKind());
            switch (action[0]) {
                case DEEP_COPY: {
                    //currentActivation.isSkipped = true;
                    ItemFeed out = new NoOpenOrCloseFeed(getResultFeed(), getContext());
                    Watch copyWatch = CopyOfFeed.getWatch(out, AnchorPattern.getInstance(), watchManager, getContext());
                    watchManager.addWatch(copyWatch, true);
                    return null;
                }
                case FAIL: {
                    XPathException err = new XPathException("No user-defined template rule matches the node " +
                        Navigator.getPath(node), "XTDE0555");
                    err.setLocator(locationId);
                    throw err;
                }
                case DEEP_SKIP: {
                    //currentActivation.isSkipped = true;
                    return null;
                }
                case SHALLOW_COPY: {
                    //currentActivation.shallowCopiedNode = node;
                    XPathContext context = localContext;
                    //currentActivation.isSkipped = false;
                    //activation.template = inversion;
                    //currentActivation.context = context;
                    //currentActivation.isDocumentNode = (node instanceof FleetingDocumentNode);
                    Outputter cco = getNextOutputter();
                    ShallowCopyEventFeed copyFeed = new ShallowCopyEventFeed(mode.getActor(), new DecomposingFeed(cco, context), context);
                    ApplyTemplatesAction actor = new ApplyTemplatesAction(null, copyFeed, context);
                    actor.setMode(mode);
                    actor.setWatchManager(watchManager);
                    actor.setActualParams(actualParams);
                    actor.setTunnelParams(tunnelParams);
                    watchManager.addWatch(new Trigger(CHILD_NODE_SELECTION, actor, context), true);
                    return null;
                }
                default: {
                    boolean applyTemplatesToAttributes = false;
                    boolean applyTemplatesToChildren = false;
                    for (BuiltInRules i : action) {
                        if (i == BuiltInRules.APPLY_TEMPLATES_TO_ATTRIBUTES) {
                            applyTemplatesToAttributes = true;
                        } else if (i == BuiltInRules.APPLY_TEMPLATES_TO_CHILDREN) {
                            applyTemplatesToChildren = true;
                        }
                    }
                    if (applyTemplatesToAttributes && node.getNodeKind() == Type.ELEMENT) {
                        AxisIterator atts = node.iterateAxis(AxisInfo.ATTRIBUTE);  // See bug 2874
                        XPathContextMajor c2 = localContext.newContext();
                        c2.trackFocus(atts);
                        mode.getActor().applyTemplates(actualParams, tunnelParams,
                                                       null, new ComplexContentOutputter(streamingSequenceWriter), c2, locationId);
                    }
                    if (applyTemplatesToChildren) {
                        ItemFeed out = new NoOpenOrCloseFeed(getResultFeed(), localContext);
                        ApplyTemplatesAction actor = new ApplyTemplatesAction(null, out, getContext());
                        actor.setMode(mode);
                        actor.setWatchManager(watchManager);
                        actor.setActualParams(actualParams);
                        actor.setTunnelParams(tunnelParams);
                        Watch watch = new Trigger(CHILD_NODE_SELECTION, actor, localContext);
                        watchManager.addWatch(watch, true);
                    }
                    break;
                }
            }

        } else {
            TemplateRuleEE template = (TemplateRuleEE)rule.getAction();
            template.initialize();
            Expression body = template.getBody();
            if (Streamability.getSweep(body) != Sweep.CONSUMING) {
                // a non-consuming template body
                localContext.openStackFrame(template.getStackFrameMap());
                localContext.setLocalParameters(actualParams);
                localContext.setTunnelParameters(tunnelParams);
                localContext.setCurrentTemplateRule(rule);
                localContext.setCurrentMode(mode);
                localContext.setCurrentComponent(mode);
                localContext.setCurrentMergeGroupIterator(null);
                body.process(new ComplexContentOutputter(streamingSequenceWriter), localContext);
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

        if (inversion != null) {
            //Watch watch = inversion.getWatch(watchManager, new DecomposingFeed(context.getReceiver()), activation.context);
            Watch watch = inversion.getWatch(watchManager, new NoOpenOrCloseFeed(getResultFeed(), getContext()), localContext);
            watch.setAnchorNode(node);
            watchManager.addWatch(watch, true);
        }
        return null;
    }

    /**
     * Get the template rule matching a specific node
     *
     * @param node the node (in a streamed input document) to be matched
     * @return the best-fit template rule used to process this node
     * @throws XPathException if a dynamic error occurs
     */

    protected Rule getRule(FleetingNode node) throws XPathException {
        return mode.getActor().getRule(node, localContext);
    }

    /**
     * Signal that a item (other than a streamed element) that matches the selection
     * has been found.
     *
     * @param item       the item that was matched.
     *
     */

    @Override
    public void append(Item item) throws XPathException {
        if (!hasFailed()) {
            if (tracing) {
                TraceListener listener = getContext().getController().getTraceListener();
                listener.startCurrentItem(item);
            }

            ManualIterator iter = (ManualIterator) localContext.getCurrentIterator();
            iter.setContextItem(item);
            iter.incrementPosition();

            Rule rule = mode.getActor().getRule(item, localContext);
            localContext.setCurrentTemplateRule(rule);
            localContext.setCurrentComponent(mode);
            localContext.setCurrentMode(mode);


            final StreamingSequenceWriter streamingSequenceWriter =
                    new StreamingSequenceWriter(getPipelineConfiguration(), getNextOutputter());
            ComplexContentOutputter output = new ComplexContentOutputter(streamingSequenceWriter);
            if (rule == null) {
                BuiltInRuleSet rules = mode.getActor().getBuiltInRuleSet();
                try {
                    rules.process(item, actualParams, tunnelParams, output, localContext, Loc.NONE);
                } catch (XPathException e) {
                    dynamicError(e);
                    return;
                }

            } else {
                TemplateRule template = (TemplateRule) rule.getAction();
                template.initialize();
                localContext.openStackFrame(template.getStackFrameMap());
                try {
                    template.apply(output, localContext);
                } catch (XPathException e) {
                    dynamicError(e);
                    return;
                }
            }
            if (tracing) {
                TraceListener listener = getContext().getController().getTraceListener();
                listener.endCurrentItem(item);
            }
        }
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @throws net.sf.saxon.type.ValidationException
     *          May be raised if the constraint implemented by this
     *          Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     * @param locationId the location of the node in the source, or of the instruction that created it
     */

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (tracing) {
            TraceListener listener = getContext().getController().getTraceListener();
            listener.endCurrentItem(getContext().getContextItem());
        }

    }

    @Override
    public void close() throws XPathException {
        // TODO: make tracing work
//        if (tracing && getExpression() != null) {
//            getContext().getController().getTraceListener().leave(getExpression());
//        }
        super.close();
    }

    private static final AncestorQualifiedPattern CHILD_NODE_SELECTION =
            new AncestorQualifiedPattern(
                    new NodeTestPattern(MultipleNodeKindTest.CHILD_NODE), AnchorPattern.getInstance(), AxisInfo.PARENT);

}

// Copyright (c) 2009-2022 Saxonica Limited