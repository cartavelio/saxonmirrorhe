
package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.AxisExpression;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SlashExpression;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;

import java.util.ArrayList;
import java.util.List;

/**
 * Compile an axis expression to streaming code.
 */

public class AxisExpressionAdjunct extends StreamingAdjunct {

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
        AxisExpression ae = (AxisExpression)getExpression();
        int axis = ae.getAxis();
        try {
            if (AxisInfo.isSubtreeAxis[axis]) {
                // Note, ae.copy() rather than ae, to prevent its parent pointer being corrupted...
                SlashExpression s = new SlashExpression(new ContextItemExpression(), ae.copy(new RebindingMap()));
                return s.toPattern(config);
            } else {
                return null;
            }
        } catch (XPathException e) {
            return null;
        }
    }


    /*@Nullable*/
    @Override
    public WatchMaker getWatchMaker(final boolean forGrouping) throws XPathException {

        // If the expression in its entirety is a streamable pattern, then return a CopyWatch on this expression. (This is more
        // likely to apply to a path expression a/b/c than to an actual xsl:for-each instruction, and it happens when the path
        // appears in an unusual context - see for example strmode089

        Configuration config = getConfiguration();
        List<String> failures = new ArrayList<>();
        final Pattern pattern = StreamingPatternMaker.makeStreamingPattern(getExpression(), config, failures);
        if (pattern != null) {
            return (watchManager, out, context) -> {
                if (forGrouping) {
                    out = new FilteringFeed(
                            out,
                            context,
                            new FilteringFeed.FilterLambda((item, position)
                                   -> position == 1 ? FilteringFeed.FilterResult.MATCHES : FilteringFeed.FilterResult.DONE));
                }
                return new Trigger(pattern, out, context);
            };
        }

        return null;
    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        AxisExpression exp = (AxisExpression)getExpression();
        Posture contextPosture = contextInfo.getContextItemPosture();
        int axis = exp.getAxis();
        NodeTest test = exp.getNodeTest();

        // See XSLT 3.0 19.8.7.7

        switch (contextPosture) {
            case GROUNDED:
                return PostureAndSweep.GROUNDED_AND_MOTIONLESS;

            case ROAMING:
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;

            case CLIMBING:
                switch (axis) {
                    case AxisInfo.SELF:
                    case AxisInfo.PARENT:
                    case AxisInfo.ANCESTOR_OR_SELF:
                    case AxisInfo.ANCESTOR:
                        return new PostureAndSweep(Posture.CLIMBING, Sweep.MOTIONLESS);

                    case AxisInfo.ATTRIBUTE:
                    case AxisInfo.NAMESPACE:
                        return new PostureAndSweep(Posture.STRIDING, Sweep.MOTIONLESS);

                    default:
                        addFreeRangingMessage(exp, axis, contextPosture, reasons);
                        return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }

            case STRIDING:
                switch (axis) {
                    case AxisInfo.PARENT:
                    case AxisInfo.ANCESTOR_OR_SELF:
                    case AxisInfo.ANCESTOR:
                        return new PostureAndSweep(Posture.CLIMBING, Sweep.MOTIONLESS);

                    case AxisInfo.ATTRIBUTE:
                    case AxisInfo.NAMESPACE:
                    case AxisInfo.SELF:
                        return new PostureAndSweep(Posture.STRIDING, Sweep.MOTIONLESS);

                    case AxisInfo.CHILD:
                        return new PostureAndSweep(Posture.STRIDING, Sweep.CONSUMING);

                    case AxisInfo.DESCENDANT:
                    case AxisInfo.DESCENDANT_OR_SELF:
                        if (Streamability.isChildlessNodeKind(test)) {
                            return new PostureAndSweep(Posture.STRIDING, Sweep.CONSUMING);
                        } else {
                            return new PostureAndSweep(Posture.CRAWLING, Sweep.CONSUMING);
                        }

                    default:
                        addFreeRangingMessage(exp, axis, contextPosture, reasons);
                        return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }

            case CRAWLING:
                switch (axis) {
                    case AxisInfo.PARENT:
                    case AxisInfo.ANCESTOR_OR_SELF:
                    case AxisInfo.ANCESTOR:
                        return new PostureAndSweep(Posture.CLIMBING, Sweep.MOTIONLESS);

                    case AxisInfo.ATTRIBUTE:
                    case AxisInfo.NAMESPACE:
                        return new PostureAndSweep(Posture.STRIDING, Sweep.MOTIONLESS);

                    case AxisInfo.SELF:
                        return new PostureAndSweep(Posture.CRAWLING, Sweep.MOTIONLESS);

                    case AxisInfo.CHILD:
                    case AxisInfo.DESCENDANT:
                    case AxisInfo.DESCENDANT_OR_SELF:
                        if (Streamability.isChildlessNodeKind(test)) {
                            return new PostureAndSweep(Posture.STRIDING, Sweep.CONSUMING);
                        }
                        // drop through
                        CSharp.emitCode("goto default;");
                    default:
                        addFreeRangingMessage(exp, axis, contextPosture, reasons);
                        return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }

            default:
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;

        }
    }

    private static void addFreeRangingMessage(Expression exp, int axis, Posture contextPosture, List<String> reasons) {
        if (reasons != null) {
            reasons.add("Cannot use the " + AxisInfo.axisName[axis] + " axis when context posture is "
                    + contextPosture + " (line " + exp.getLocation().getLineNumber() + ")");
        }
    }



}

// Copyright (c) 2013-2022 Saxonica Limited