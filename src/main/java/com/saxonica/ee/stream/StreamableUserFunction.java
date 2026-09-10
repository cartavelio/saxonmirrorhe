package com.saxonica.ee.stream;

import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;

/**
 * Represents a used-defined XSLT function whose declared streamability is other than "unclassified"
 */
public class StreamableUserFunction extends UserFunction {

    private ComponentInversion inversion;

    /**
     * Generate the inversion of the expression comprising the body of the function.
     * Supported in Saxon-EE only
     */


    public void makeInversion() throws XPathException {
        // Create the Jackson inversion of the function
        if (inversion == null) {
//            if (getBody() instanceof TraceExpression) {
//                throw new XPathException(
//                        "Saxon restriction: code that uses streaming cannot be compiled with tracing enabled", SaxonErrorCode.SXST0065, this);
//            }
            ComponentInversion inv = new ComponentInversion(getBody());
            inv.setStackFrame(getStackFrameMap());
            inv.invert(true);
            inversion = inv;
        }
    }

    public ComponentInversion getInversion() {
        return inversion;
    }

    @Override
    public void prepareForStreaming() throws XPathException {
        // Done on reloading from export file, so shouldn't fail
        if (getDeclaredStreamability().isStreaming()) {
            Streamability.getStreamability(getBody(), ContextItemStaticInfoEE.ABSENT, new ArrayList<>());
            makeInversion();
        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited