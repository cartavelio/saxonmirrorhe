package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.StreamingSequenceWriter;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.SequenceWriter;
import net.sf.saxon.expr.Component;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.instruct.ApplyTemplates;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.instruct.ParameterSet;
import net.sf.saxon.expr.instruct.TemplateRule;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.tree.iter.ManualIterator;

/**
 * This class is used when a streaming template applies xsl:apply-templates to composed items, typically
 * to the items in the current group established using xsl:for-each-group
 */


public class ApplyTemplatesFeed extends ItemFeed {

    private final XPathContext localContext;
    private final ManualIterator manual;
    private final Component.M targetMode;

    public ApplyTemplatesFeed(ApplyTemplates expr, ItemFeed result, XPathContext context) {
        super(expr, result, context);
        localContext = context.newMinorContext();
        manual = new ManualIterator();
        localContext.setCurrentIterator(manual);
        targetMode = expr.getTargetMode(context);
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    @Override
    public void append(Item item) throws XPathException {
        manual.setContextItem(item);
        manual.incrementPosition();

        // handle parameters if any

        // TODO: reuse these for each item in the input sequence?
        ParameterSet actualParams =
                Instruction.assembleParams(localContext, ((ApplyTemplates) getExpression()).getActualParams());
        ParameterSet tunnelParams =
                Instruction.assembleTunnelParams(localContext, ((ApplyTemplates) getExpression()).getTunnelParams());

        XPathContextMajor c2 = localContext.newContext();
        c2.setCurrentIterator(manual);
        c2.setCurrentMode(targetMode);
        c2.setCurrentComponent(targetMode);
        c2.setLocalParameters(actualParams);
        c2.setTunnelParameters(tunnelParams);
        c2.setCurrentMergeGroupIterator(null);
        SequenceWriter writer = new StreamingSequenceWriter(getPipelineConfiguration(), getNextOutputter());
        Outputter output = new ComplexContentOutputter(writer);

        Mode mode = targetMode.getActor();
        Rule rule = mode.getRule(item, c2);

        if (rule == null) {
            mode.getBuiltInRuleSet().process(item, actualParams, tunnelParams, output, c2, Loc.NONE);
        } else {
            TemplateRule template = (TemplateRule) rule.getAction();
            template.initialize();
            c2.openStackFrame(template.getStackFrameMap());
            c2.setCurrentTemplateRule(rule);
            template.apply(output, c2);
        }

    }

}

// Copyright (c) 2018-2023 Saxonica Limited