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
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.tree.iter.ListIterator;

import java.util.ArrayList;
import java.util.List;

/**
 * This class is used when a streaming xsl:for-each requests processing of a grounded consuming
 * sequence with a static dependency on last()
 */


public class BufferingApplyTemplatesFeed extends ItemFeed {

    private final List<Item> inputValue = new ArrayList<>(4);
    private final Component.M targetMode;

    public BufferingApplyTemplatesFeed(ApplyTemplates expr, ItemFeed result, XPathContext context) {
        super(expr, result, context);
        targetMode = expr.getTargetMode(context);
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws XPathException
     *          to report any failure
     */

    @Override
    public void append(Item item) throws XPathException {
        inputValue.add(item);
    }

    @Override
    public void close() throws XPathException {
        XPathContextMajor c2 = getContext().newContext();
        c2.trackFocus(new ListIterator.Of<>(inputValue));

        // handle parameters if any

        ParameterSet actualParams =
                Instruction.assembleParams(getContext(), ((ApplyTemplates) getExpression()).getActualParams());
        ParameterSet tunnelParams =
                Instruction.assembleTunnelParams(getContext(), ((ApplyTemplates) getExpression()).getTunnelParams());

        c2.setCurrentMode(targetMode);
        c2.setCurrentComponent(targetMode);
        c2.setLocalParameters(actualParams);
        c2.setTunnelParameters(tunnelParams);
        c2.setCurrentMergeGroupIterator(null);
        SequenceWriter writer = new StreamingSequenceWriter(getPipelineConfiguration(), getNextOutputter());
        Outputter output = new ComplexContentOutputter(writer);

        Mode mode = targetMode.getActor();

        Item item;
        FocusIterator iter = c2.getCurrentIterator();

        // TODO: following code is shared with ApplyTemplatesFeed...
        while ((item = iter.next()) != null) {
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
}

// Copyright (c) 2018-2022 Saxonica Limited