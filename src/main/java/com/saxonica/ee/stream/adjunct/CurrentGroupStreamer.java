package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.instruct.ForEachGroup;
import net.sf.saxon.functions.CurrentGroupCall;
import com.saxonica.ee.stream.CurrentGroupPattern;
import net.sf.saxon.pattern.Pattern;

import java.util.List;

/**
 * Provides streaming code for a call on current-group().
 */
public class CurrentGroupStreamer extends StreamingAdjunct {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        CurrentGroupCall exp = (CurrentGroupCall) getExpression();
        ForEachGroup feg = exp.getControllingInstruction();
        if (feg == null) {
            if (reasons != null) {
                reasons.add("The current-group() function has no containing xsl:for-each-group instruction");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        PostureAndSweep ps = Streamability.getStreamability(feg.getSelectExpression(), contextInfo, reasons);
        if (ps.getPosture() == Posture.GROUNDED) {
            // spec bug 24455, test case si-group-026
            return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        } else if (exp.isInHigherOrderOperand()) {
            if (reasons != null) {
                reasons.add("The current-group() function is evaluated repeatedly");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        } else {
            return ps;
        }
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
        return CurrentGroupPattern.getInstance();
    }


}

// Copyright (c) 2012-2022 Saxonica Limited