package com.saxonica.ee.stream;

import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Watch;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;

/**
 * This class represents the inversion of a template or function body
 */
public class ComponentInversion {

    private final Expression body;
    private Inversion inversion;
    private SlotManager stackFrame = null;

    /**
     * Create the Jackson inversion of a template or function,
     * allowing the template or function to execute on a streamed input source.
     *
     * @param body the body of the template to be inverted
     *
     */

    public ComponentInversion(Expression body) {
        this.body = body;
    }

    /**
     * Get a Watch associated with this template, if any. This creates a watch to implement
     * the "drill-down" instruction of the inverted template, for example if the drill-down instruction
     * is <code>&lt;xsl:copy-of select="a/b/c"&gt;</code> then it returns a
     * {@link com.saxonica.ee.stream.feed.CopyOfFeed} with that expression as its selection.
     *
     * @param wm           the WatchManager which keeps track of all active Watches
     * @param result the destination for the output of this Watch
     * @param context the stack of dynamic context objects for the active Watches
     * @return the Watch if there is one, or null otherwise.
     * @throws XPathException if an error occurs
     */

    public Watch getWatch(WatchManager wm, ItemFeed result, XPathContext context) throws XPathException {
        return inversion.getWatch(wm, result, context);
    }

    /**
     * Invert the body of the template.
     *
     * @throws XPathException if the template is non-streamable
     */

    public void invert(boolean isW3CStreamable) throws XPathException {
        try {
            inversion = Inversion.invertExpression(body, false);
        } catch (XPathException e) {
            String frag = isW3CStreamable ? "although it is" : "and is not";
            XPathException e2 = new XPathException(
                    "Component cannot be streamed, " + frag + " guaranteed streamable according to W3C rules. " +
                            e.getMessage(), SaxonErrorCode.SXST0062);
            e2.setErrorCodeQName(e.getErrorCodeQName());
            e2.setLocator(body.getLocation());
            e2.setXPathContext(e.getXPathContext());
            throw e2;
        }
    }

    /**
     * Set the stackframe identifying the slots used for local variables
     *
     * @param frame the stack frame skeleton
     */

    public void setStackFrame(SlotManager frame) {
        this.stackFrame = frame;
    }

    /**
     * Get the stackframe identifying the slots used for local variables
     *
     * @return the stack frame skeleton
     */

    /*@Nullable*/
    public SlotManager getStackFrame() {
        return stackFrame;
    }

    public Inversion getInversion() {
        return inversion;
    }

    /**
     * Output diagnostic explanation to an ExpressionPresenter
     * @param presenter the destination for the output
     */

    public void explain(ExpressionPresenter presenter) throws XPathException {
        Expression exp = body;
        presenter.emitAttribute("line", exp.getLocation().getLineNumber() + "");
        presenter.emitAttribute("module", exp.getLocation().getSystemId());
        presenter.emitAttribute("streaming", "true");
        inversion.explain(presenter);
        exp.export(presenter);
    }


}

// Copyright (c) 2009-2023 Saxonica Limited


