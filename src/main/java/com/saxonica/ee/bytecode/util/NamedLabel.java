package com.saxonica.ee.bytecode.util;

import org.objectweb.asm.Label;

/**
 * Subclass of the ASM Label class that adds a diagnostic name to the label; this enables more intelligible bytecode
 * to be output for diagnostics
 */
public class NamedLabel extends Label {

    private String name;

    public void setName(String name) {
        this.name = name;
    }

    public String getName() {
        return name;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited

