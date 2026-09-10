package com.saxonica.functions.registry;

import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XSLT30FunctionSet;

/**
 * Function signatures (and pointers to implementations) of the functions available in the
 * standard function namespace in XSLT 3.0, including by reference functions defined in
 * XPath 3.1, including the higher-order functions
 */

public class XSLT30HOFunctionSet extends BuiltInFunctionSet {

    private static XSLT30HOFunctionSet THE_INSTANCE = new XSLT30HOFunctionSet();

    public static XSLT30HOFunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private XSLT30HOFunctionSet() {
        init();
    }

    private void init() {

        importFunctionSet(XPath31HOFunctionSet.getInstance());
        importFunctionSet(XSLT30FunctionSet.getInstance());

    }


}

// Copyright (c) 2018 Saxonica Limited
