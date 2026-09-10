package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.DocumentSorter;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.z.IntHashMap;

import java.util.ArrayList;
import java.util.List;
import java.util.Stack;


/**
 * Generate streaming code for a document sorting expression node. This will only be used
 * to support streaming of a path expression of the form child::* / ancestor::*, where the
 * only operation needed is filtering to remove duplicate ancestors (and returning of ancestors
 * in document order).
 *
 * In addition, if the expression a mixture of striding and grounded nodes, then
 * we can sort it because the streamed nodes retain their order (bug 29507)
 */
public class DocumentSorterAdjunct extends TransmissionAdjunct implements FeedMaker {

    /**
     * Compute the posture and sweep of this expression as defined in the W3C streamability specifications.
     * This must be implemented in all subclasses that don't use the general streamability rules.
     * @param contextInfo     Information about the context item type and posture
     * @param reasons         the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        // Normally a sort into document order is not streamable. However, in the special case of a path expression such as
        // child::*/ancestor::*, the path expression is permitted by the streamability rules, and is streamable using a
        // special de-duplication filter.  Furthermore, if the expression a mixture of striding/crawling and grounded nodes, then
        // we can sort it because the streamed nodes retain their order (bug 29507)
        DocumentSorter exp = (DocumentSorter)getExpression();
        Expression base = exp.getBaseExpression();
        PostureAndSweep ps = Streamability.getStreamability(base, contextInfo, reasons);
        if (ps.getSweep() == Sweep.CONSUMING && ps.getPosture() == Posture.CRAWLING) {
            // This case is streamable only if the base expression is in the form of a "scanning expression"
            Pattern p = toStreamingPattern(getConfiguration());
            if (p == null) {
                reasons.add(needsSorting());
                reasons.add("The expression " + base +
                                    " cannot be used as a scanning expression, because it is not valid as a pattern");
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (p.isMotionless()) {
                base.setExtraProperty("scanningPattern", p);
                return ps;
            } else {
                reasons.add(needsSorting());
                reasons.add("The expression " + exp.getBaseExpression() +
                                    " cannot be used as a scanning expression, because as a pattern, it is not motionless");
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        if (ps.getSweep() == Sweep.MOTIONLESS || ps.getPosture() == Posture.GROUNDED || ps.getPosture() == Posture.CLIMBING
                || ps.getPosture() == Posture.STRIDING || ps.getPosture() == Posture.CRAWLING) {
            return ps;
        } else {
            if (reasons != null) {
                reasons.add("Expression " + exp.getBaseExpression().toShortString() + " requires sorting nodes into document order");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
    }

    private String needsSorting() {
        return "Expression " + ((DocumentSorter) getExpression()).getBaseExpression().toShortString() + " requires sorting nodes into document order";
    }


    /**
     * Convert this expression to a streaming pattern (a pattern used internally to match nodes during
     * push processing of an event stream)
     *
     *
     * @param config           the Saxon configuration
     * @return the equivalent pattern if conversion succeeds; otherwise null
     */

    @Override
    public Pattern toStreamingPattern(Configuration config) {
        return Streamability.toStreamingPattern(((DocumentSorter) getExpression()).getBaseExpression(), config);
    }

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        Expression base = ((DocumentSorter) getExpression()).getBaseExpression();
        if (base.getExtraProperty("scanningPattern") != null) {
            return StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), base)
                    .getWatchMaker(false);
        } else {
            return null;
        }
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        if (Streamability.getPosture(((DocumentSorter) getExpression()).getBaseExpression()) == Posture.STRIDING) {
            return new MixedPostureSorter(out, context);
        } else {
            return new AncestorArrangingFeed(out, context);
        }
    }


    @Override
    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : super.getFeedMaker(arg);
    }

    /**
     * This class takes as input the nodes selected by an expression such as XXX/ancestor::*, and outputs
     * them in document order, eliminating duplicates. It is known that XXX delivers nodes in document order
     * without duplicates. This involves detection of whether a node is
     * an ancestor of the most recent node; if it is, then we stack it; otherwise we process the current
     * stack in reverse order, outputting only those nodes that were not present on the previous stack.
     */

    private static class AncestorArrangingFeed extends ItemFeed {

        public IntHashMap<NodeInfo> mostRecentAtDepth = new IntHashMap<>();
        public Stack<NodeInfo> stack = new Stack<>();

        public AncestorArrangingFeed(ItemFeed out, XPathContext context) {
            super(out, context);
        }

        @Override
        public void append(Item item) throws XPathException {
            NodeInfo node = (NodeInfo)item;
            if (!stack.isEmpty() && Navigator.isAncestorOrSelf(node, stack.peek())) {
                // this node is an ancestor of the previous node; add it to the stack
                stack.push(node);
            } else {
                // we have a new leaf node. We now output the content of the stack representing the previous leaf node
                // and its ancestors, in document order (top of stack first), except for any nodes that were already
                // output while processing the previous stack, which are therefore duplicates.
                flush();
                stack = new Stack<>();
                stack.push(node);
            }
        }

        @Override
        public void close() throws XPathException {
            flush();
            super.close();
        }

        private void flush() throws XPathException {
            Outputter out = getNextOutputter();
            while (!stack.isEmpty()) {
                NodeInfo n = stack.pop();
                int depth = depthOfNode(n);
                NodeInfo prev = mostRecentAtDepth.get(depth);
                if (prev == null || prev != n) {
                    out.append(n);
                    mostRecentAtDepth.put(depth, n);
                }
            }
        }

        private int depthOfNode(NodeInfo n) {
            int depth = 0;
            while ((n = n.getParent()) != null) {
                depth++;
            }
            return depth;
        }

    }

    /**
     * This class takes as input the nodes selected by an expression such as (/books/book, $moreBooks), and outputs
     * them in document order, eliminating duplicates. It is known that the streamed nodes are in document order
     * without duplicates; furthermore, we adopt the convention that nodes from streamed documents always come
     * before grounded nodes in document order. So the result of the sort is to output the streamed nodes as they
     * arrive, followed by the grounded nodes, which must themselves be sorted into document order.
     */

    private static class MixedPostureSorter extends ItemFeed {

        public List<Item> groundedItems = new ArrayList<>();

        public MixedPostureSorter(ItemFeed out, XPathContext context) {
            super(out, context);
        }

        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            return getResultFeed().startSelectedParentNode(node, locationId);
        }

        @Override
        public void append(Item item) throws XPathException {
            if (item instanceof FleetingNode) {
                getNextOutputter().append(item);
            } else {
                groundedItems.add(item);
            }
        }

        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            getResultFeed().endSelectedParentNode(locationId);
        }

        @Override
        public void close() throws XPathException {
            SequenceIterator grounded = new ListIterator.Of<>(groundedItems);
            SequenceIterator sorted = new DocumentOrderIterator(grounded, GlobalOrderComparer.getInstance());
            Outputter out = getNextOutputter();
            SequenceTool.supply(sorted, (ItemConsumer<? super Item>) out::append);
            super.close();
        }

    }

}

// Copyright (c) 2013-2022 Saxonica Limited