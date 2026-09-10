package com.saxonica.ee.stream;

import net.sf.saxon.event.SequenceWriter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;

/**
 * A sequence writer that passes on only those items that satisfy a filter predicate.
 * The filter will always be non-positional.
 */
public class SequenceFilter extends SequenceWriter {

    private SequenceWriter writer;
    private XPathContext context;
    private Expression filter;

    public SequenceFilter(SequenceWriter out, XPathContext context, Expression filter) {
        super(out.getPipelineConfiguration());
        this.writer = out;
        this.context = context;
        this.filter = filter;
    }

    public void write(Item item) throws XPathException {
        XPathContext c2 = context.newContext();
        ManualIterator iter = new ManualIterator(item);
        c2.setCurrentIterator(iter);
        if (filter.effectiveBooleanValue(c2)) {
            writer.write(item);
        }
    }
}

// Copyright (c) 2006-2007 Saxonica Limited. All Rights Reserved