package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.SequenceInstr;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;

import java.util.List;

public class SequenceInstrAdjunct extends StreamingAdjunct {

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
        Expression select = ((SequenceInstr)getExpression()).getBaseExpression();
        return StreamingAdjunct.makeStreamingAdjunct(config, select).toStreamingPattern(config);
    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        Expression select = ((SequenceInstr) getExpression()).getBaseExpression();
        return Streamability.getStreamability(select, contextInfo, reasons);
    }

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        Expression select = ((SequenceInstr) getExpression()).getBaseExpression();
        StreamingAdjunct sa = StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
        return sa.getWatchMaker(forGrouping);
    }




}

// Copyright (c) 2018-2022 Saxonica Limited