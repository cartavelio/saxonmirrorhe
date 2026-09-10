package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;

/**
 * A Trigger is a Watch that contains a pattern used for matching streamed nodes, and
 * a WatchAction which is invoked when a matching node is found.
 */

public class Trigger extends PatternWatch {

    private final ItemFeed action;
    private final XPathContext selectionContext;
    private boolean matchCurrentGroup = false;


    public Trigger(Pattern selection, ItemFeed action, XPathContext selectionContext) {
        this.action = action;
        this.selectionContext = selectionContext;
        setSelection(selection);
    }

    @Override
    public boolean matchesNode(FleetingNode node, XPathContext context) throws XPathException {
        // We use the context associated with the Trigger in preference to the context associated
        // with the WatchManager. This is because the WatchManager's context does not have a useful
        // value for currentComponent, which is needed to resolve any references in the selection
        // pattern to global variables or named functions. See bug 2821 and test case si-for-each-801.
        return super.matchesNode(node, selectionContext);
    }

    /**
     * Get the Feed representing the pipeline of operations performed by this Trigger
     * @return the Feed through which matched items are processed
     */

    public ItemFeed getAction() {
        return action;
    }

    /**
     * Open the watch. This is done at the point where it first starts watching for nodes
     * that match the selection.
     * @param terminator the callback to be used to signal early exit
     */

    @Override
    public void open(Terminator terminator) throws XPathException {
        final FleetingParentNode anchor = getAnchorNode();
        if (anchor != null && anchor.getNodeKind() == Type.DOCUMENT) {
            terminator = new Terminator() {
                @Override
                public void terminate() throws QuitParsingException {
                    throw new QuitParsingException(false);
                }
            };
        }
        action.open(terminator);
    }

    /**
     * Signal that an element or document node has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement or startDocument event that matches the selection.
     *
     * @param node       the element or document node whose start event has been matched
     * @param locationId the location associated with the element or document node (may be
     *                   the location of the instruction that created it)
     * @return a Receiver to be notified of all events starting with the startElement/startDocument event
     *         for the matched element, and ending with the endElement event for that element; or null if this
     *         feature is not required.
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a dynamic error occurs
     */
    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        return action.startSelectedParentNode(node, locationId);
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the element
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a constraint implemented by this
     *          Watch is violated
     */
    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        action.endSelectedParentNode(locationId);
    }

    /**
     * Signal that a node (other than an element) that matches the selection
     * has been found.
     *
     * @param item       the node that was matched.
     *
     */

    @Override
    public void processItem(Item item) throws XPathException {
        action.append(item);
    }

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope. The default implementation does nothing.
     */

    @Override
    public void close() throws XPathException {
        action.close();
    }

    /**
     * Ask whether or not the pattern should use all nodes in the current group as
     * anchor nodes (by default only the first is matched)
     *
     * @return true if every node in the current group is to act
     *                          as an anchor node
     */

    public boolean isMatchCurrentGroup() {
        return matchCurrentGroup;
    }

    /**
     * Set the selection (that is, the streamable XPath expression) that defines
     * the nodes that this Watch is looking for
     *
     * @param selection the streamable XPath expression
     */

    @Override
    public void setSelection(Pattern selection) {
        super.setSelection(selection);
        if (selection.matchesCurrentGroup()) {
            setMatchCurrentGroup(true);
        }
    }

    /**
     * Set whether or not the pattern should use all nodes in the current group as
     * anchor nodes (by default only the first is matched)
     * @param matchCurrentGroup true if every node in the current group is to act
     *                          as an anchor node
     */

    public void setMatchCurrentGroup(boolean matchCurrentGroup) {
        this.matchCurrentGroup = matchCurrentGroup;
    }
}

// Copyright (c) 2018-2022 Saxonica Limited No warranty.
