package com.saxonica.ee.bytecode;

/**
 * Generate bytecode for evaluation of functions such as name() and local-name()
 * which return a string property of a node
 */
public class NameCompiler extends NodePropertyCompiler {

    public NameCompiler() {
        this.methodName = "getDisplayName";
    }

}

// Copyright (c) 2018-2022 Saxonica Limited
