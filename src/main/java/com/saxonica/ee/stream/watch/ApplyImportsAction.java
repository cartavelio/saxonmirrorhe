package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingNode;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.trans.XPathException;

/**
 * Watch for an xsl:apply-imports instruction. This doesn't really need to be a Watch, because it always
 * processes the context node; but it means we can reuse a lot of code from the ApplyTemplatesWatch
 */
public class ApplyImportsAction extends ApplyTemplatesAction {

    public ApplyImportsAction(Expression expression, ItemFeed result, XPathContext context)
            throws XPathException {
        super(expression, result, context);
    }

    @Override
    protected Rule getRule(FleetingNode node) throws XPathException {
        Rule currentRule = getLocalContext().getCurrentTemplateRule();
        int min = currentRule.getMinImportPrecedence();
        int max = currentRule.getPrecedence() - 1;
        return ((Mode)getMode().getActor()).getRule(node, min, max, getContext());
    }
}

// Copyright (c) 2009-2022 Saxonica Limited