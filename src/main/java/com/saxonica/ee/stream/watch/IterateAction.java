package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.expr.instruct.BreakInstr;
import net.sf.saxon.expr.instruct.IterateInstr;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.TailCallLoop;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.XPathException;

/**
 * This watch implements xsl:iterate in streaming mode. It watches for the nodes selected by the
 * xsl:iterate instruction; when such a node is encountered, it locates the relevant (inverted)
 * template, and activates it.
 * <p>The watch is capable of handling a select expression that selects non-peer nodes (that is, one selected node
 * that is a descendant of another). A top-level selected node is processed directly to the output receiver;
 * a nested node is processed to create a temporary result tree, which is then appended to the output sequence
 * when the processing of the outer selected node is complete.</p>
 */


public class IterateAction extends ForEachAction {

    boolean broken = false;
    Terminator terminator;


    public IterateAction(WatchManager watchManager, Expression expression, Feed result, XPathContext context) {
        super(watchManager, expression, result, context);
    }


    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        this.terminator = terminator;
        XPathContext outerContext = getContext();
        ((IterateInstr) getExpression()).getInitiallyExp().process(outerContext);
        makeBodyContext();
    }


    /**
     * Signal that an element has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement event that matches the selection. For this purpose,
     * any attributes selected by the last step in the selection are ignored
     *
     * @throws net.sf.saxon.type.ValidationException
     *          May be raised if the constraint implemented by this
     *          Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     */

    /*@Nullable*/
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (broken) {
            // xsl:break has been executed
            return null;
        }
        return super.startSelectedParentNode(node, locationId);
    }

    /**
     * Signal that a node (other than an element) that matches the selection
     * has been found.
     *
     * @param it       the node that was matched.
     *
     */

    public void processItem(Item<?> it) throws XPathException {
        if (broken) {
            // xsl:break has been executed
            return;
        }
        super.processItem(it);
        checkForBreakInstruction();
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

    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (broken) {
            // xsl:break has been executed
            return;
        }
        super.endSelectedParentNode(locationId);
        checkForBreakInstruction();
    }

    private void checkForBreakInstruction() throws QuitParsingException {
        XPathContext c = getContext();
        while (!(c instanceof XPathContextMajor)) {
            c = c.getCaller();
        }
        XPathContextMajor cm = (XPathContextMajor)c;
        TailCallLoop.TailCallInfo tail = cm.getTailCallInfo();
        if (tail != null && tail instanceof BreakInstr) {
            // Indicates a saxon:break instruction was encountered: break the loop
            // If we are iterating over the whole document, abandon parsing as soon as xsl:break is executed
            broken = true;
            terminator.terminate();
        }
    }

    public void close() throws XPathException {
        if (!broken) {
            // handle xsl:on-completion
            Expression onCompletion = ((IterateInstr) getExpression()).getOnCompletion();
            SequenceIterator iter = onCompletion.iterate(getContext());
            processItems(iter, getResult());
        }
        broken = false;
        super.close();
    }

}

// Copyright (c) 2009-2013 Saxonica Limited.