package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.type.UType;

import java.util.List;

public class ContextItemExprAdjunct extends StreamingAdjunct {

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
        return AnchorPattern.getInstance();
    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        // Extra rule not in XSLT 3.0 specification: if the context item type doesn't allow nodes, return GROUNDED/MOTIONLESS
        if (!contextInfo.getItemType().getUType().overlaps(UType.ANY_NODE)) {
            return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        }
        // See XSLT 3.0 19.8.7.11
        return new PostureAndSweep(contextInfo.getContextItemPosture(), Sweep.MOTIONLESS);
    }


}

// Copyright (c) 2018-2023 Saxonica Limited