package com.saxonica.functions.registry;

import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XSLT30FunctionSet;

/**
 * Function signatures (and pointers to implementations) of the functions defined in XSLT 4.0.
 * This includes the functions defined in XPath 4.0 by reference.
 */

public class XSLT40FunctionSet extends XSLT30FunctionSet {

    private static final XSLT40FunctionSet THE_INSTANCE = new XSLT40FunctionSet();

    public static XSLT40FunctionSet getInstance() {
        return THE_INSTANCE;
    }

    @Override
    protected BuiltInFunctionSet correspondingXPathFunctionSet() {
        return XPath40FunctionSet.getInstance();
    }

    private XSLT40FunctionSet() {

    }


}

// Copyright (c) 2020-2023 Saxonica Limited.

