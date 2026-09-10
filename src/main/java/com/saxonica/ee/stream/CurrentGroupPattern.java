package com.saxonica.ee.stream;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;

/**
 * This is a special pattern that matches the "anchor node". It is used for the selectors
 * that arise when evaluating XPath expressions in streaming mode; the anchor
 * node is the context node for the streamed XPath evaluation.
 *
 * Given a streamed evaluation of an expression such as ./BOOKS/BOOK/PRICE, the way we evaluate
 * this is to turn it into a pattern, which is then tested against all descendant nodes.
 * Conceptually the pattern is $A/BOOKS/BOOK/PRICE, where $A is referred to as the anchor
 * node. When we evaluate the pattern against (say) a PRICE element, the match will only succeed
 * if the name of the element is "PRICE" and its ancestors are, in order, a BOOK element, a
 * BOOKS element, and the anchor node $A.
 */
public class CurrentGroupPattern extends AnchorPattern {

    private static final CurrentGroupPattern THE_INSTANCE = new CurrentGroupPattern();
    
    public static CurrentGroupPattern getInstance() {
        return THE_INSTANCE;
    }

    protected CurrentGroupPattern() {
    }

    /**
     * Ask whether the pattern is anchored on a call on current-group()
     *
     * @return true if calls on matchesBeneathAnchor should test with all nodes in the
     * current group as anchor nodes. If false, only the first node in a group is
     * treated as the anchor node
     */

    @Override
    public boolean matchesCurrentGroup() {
        return true;
    }

    @Override
    public boolean matchesBeneathAnchor(NodeInfo node, NodeInfo anchor, XPathContext context) throws XPathException {
        return ((ManualGroupIterator)context.getCurrentGroupIterator()).currentGroupContains(node);
    }

    @Override
    public void export(ExpressionPresenter presenter) throws XPathException {
        presenter.startElement("p.currentGroup");
        presenter.endElement();
    }



}

// Copyright (c) 2018-2023 Saxonica Limited

