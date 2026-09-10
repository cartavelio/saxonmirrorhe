package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import net.sf.saxon.Configuration;
import net.sf.saxon.pattern.Pattern;

import java.util.List;

public class IsLastExprAdjunct extends StreamingAdjunct {

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
        return null;
    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        Posture cp = contextInfo.getContextItemPosture();
        if (cp == Posture.STRIDING || cp == Posture.CRAWLING || cp == Posture.ROAMING) {
            if (reasons != null) {
                reasons.add("Call to last() is not streamable when processing streamed nodes");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
    }



}

// Copyright (c) 2018-2022 Saxonica Limited