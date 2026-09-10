package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.functions.Parts;

/**
 * This class implements the saxon:analyze-uri() function which returns a map containing the components
 * of a URI
 */

public class AnalyzeUri extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context. Not used in this case
     * @param arguments The values of the arguments to the function call. In this case there are none.
     * @return the result of the function. This will have a timezone, specifically, the platform
     * default timezone. The returned value will be an instance of the built-in
     * subtype {@code xs:dateTimeStamp}
     */

    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        return Parts.uriComponents(arguments[0].head().getStringValue());
    }


}

// Copyright (c) 2020-2022 Saxonica Limited
