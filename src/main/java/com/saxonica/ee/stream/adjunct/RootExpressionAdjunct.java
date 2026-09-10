package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.watch.WatchMaker;
import net.sf.saxon.Configuration;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.UType;

import java.util.List;


/**
 * Streaming code for an expression that does not support streaming
 */
public class RootExpressionAdjunct extends StreamingAdjunct {

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
        return new NodeTestPattern(NodeKindTest.DOCUMENT);
    }

    /**
     * Get the posture and sweep of this expression as defined in the W3C streamability specifications.
     * This provides an assessment of stylesheet code against the W3C criteria for guaranteed
     * streamability, and is implemented to allow these criteria to be tested. It is not the
     * case that all expression that emerge as streamable from this analysis are currently
     * capable of being streamed by Saxon
     * @param contextInfo      Information about the context item for the expression
     * @param reasons          the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        Posture contextItemPosture = ((ContextItemStaticInfoEE) contextInfo).getContextItemPosture();
        if (contextItemPosture == Posture.GROUNDED) {
            return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        }
        if (contextInfo.getItemType().getUType().equals(UType.DOCUMENT)) {
            return new PostureAndSweep(contextItemPosture, Sweep.MOTIONLESS);
        }
        if (reasons != null) {
            reasons.add("Expression " + getExpression().toShortString() + " is free-ranging");
        }
        return PostureAndSweep.CLIMBING_AND_MOTIONLESS;
    }


    /**
     * Get a WatchMaker, a factory class that creates a Watch to evaluate this expression
     * in streaming mode. This method will only be called if the getImplementationMethod()
     * method of the expression includes the value {@link net.sf.saxon.expr.Expression#WATCH_METHOD}
     *
     *
     * @return a Watchmaker to evaluate the expression, or null if it is not streamable with respect
     *         to the specificed argument.
     * @param forGrouping true if this is for for-each-group grouping
     */
    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        throw new XPathException("Streaming is not supported for expression " + getExpression().toShortString())
                .withErrorCode(SaxonErrorCode.SXST0060)
                .withLocation(getExpression().getLocation());
    }

}

// Copyright (c) 2014-2023 Saxonica Limited