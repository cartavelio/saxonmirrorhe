package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.pattern.Pattern;

import java.util.List;

public class RootStreamer extends StreamingAdjunct {

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
        SystemFunctionCall exp = (SystemFunctionCall)getExpression();
        PostureAndSweep ps = Streamability.getStreamability(exp.getArg(0), contextInfo, reasons);
        switch (Streamability.getPosture(exp.getArg(0))) {
            case GROUNDED:
            case ROAMING:
                return ps;
            default:
                return new PostureAndSweep(Posture.CLIMBING, ps.getSweep());
        }
    }




}

// Copyright (c) 2018-2022 Saxonica Limited