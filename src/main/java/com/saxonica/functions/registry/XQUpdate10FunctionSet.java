package com.saxonica.functions.registry;

import net.sf.saxon.functions.Put;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XPath31FunctionSet;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Type;

/**
 * Function signatures (and pointers to implementations) of the functions defined in XQuery Update 1.0
 */

public class XQUpdate10FunctionSet extends BuiltInFunctionSet {

    public static XQUpdate10FunctionSet THE_INSTANCE = new XQUpdate10FunctionSet();

    public static XQUpdate10FunctionSet getInstance() {
        return THE_INSTANCE;
    }

    public XQUpdate10FunctionSet() {
        init();
    }

    private void init() {

        importFunctionSet(XPath31FunctionSet.getInstance());

        register("put", 2, Put.class, AnyItemType.getInstance(), OPT, LATE)
                .arg(0, Type.NODE_TYPE, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null);
    }


}

// Copyright (c) 2018-2022 Saxonica Limited
