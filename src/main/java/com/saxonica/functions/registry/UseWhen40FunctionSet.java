package com.saxonica.functions.registry;

import net.sf.saxon.functions.registry.UseWhen30FunctionSet;
import net.sf.saxon.functions.registry.XPath31FunctionSet;

public class UseWhen40FunctionSet extends UseWhen30FunctionSet {

    private static final UseWhen40FunctionSet THE_INSTANCE_31 = new UseWhen40FunctionSet(31);
    private static final UseWhen40FunctionSet THE_INSTANCE_40 = new UseWhen40FunctionSet(40);

    public static UseWhen30FunctionSet getInstance(int version) {
        return version == 40 ? THE_INSTANCE_40 : THE_INSTANCE_31;
    }

    protected UseWhen40FunctionSet(int version) {
        super(version);
    }

    @Override
    protected void addXPathFunctions(int version) {
        importFunctionSet(version == 40 ? XPath40FunctionSet.getInstance() : XPath31FunctionSet.getInstance());
    }
}

// Copyright (c) 2020-2023 Saxonica Limited.