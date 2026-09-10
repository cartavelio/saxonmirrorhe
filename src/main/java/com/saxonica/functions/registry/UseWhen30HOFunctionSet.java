package com.saxonica.functions.registry;

import net.sf.saxon.functions.registry.UseWhen30FunctionSet;

/**
 * Function signatures (and pointers to implementations) of the functions available for use
 * in static expressions (including use-when expressions) in XSLT 3.0 stylesheets
 */

public class UseWhen30HOFunctionSet extends UseWhen30FunctionSet {

    private static UseWhen30HOFunctionSet THE_INSTANCE = new UseWhen30HOFunctionSet();

    public static UseWhen30HOFunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private UseWhen30HOFunctionSet() {
        init();
    }

    protected void addXPathFunctions() {
        importFunctionSet(XPath31HOFunctionSet.getInstance());
    }


}

// Copyright (c) 2018 Saxonica Limited
