package com.saxonica.ee.bytecode;

/**
 * Generate bytecode for evaluation of functions such as name() and local-name()
 * which return a string property of a node
 */
public class LocalNameCompiler extends NodePropertyCompiler {

    public LocalNameCompiler() {
        this.methodName = "getLocalPart";
    }

}

// Copyright (c) 2018-2022 Saxonica Limited
