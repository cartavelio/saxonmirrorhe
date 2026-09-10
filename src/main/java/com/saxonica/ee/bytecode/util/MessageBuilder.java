package com.saxonica.ee.bytecode.util;


/**
 * Helper interface MeassageBuilder. This is used to construct error messages in the generated code at run-time.
 * Various subclasses are defined to represent messages of varying complexity.
 */
public interface MessageBuilder {

    /**
     * Generate code to construct the error message and leave it on the run-time stack
     */

    void generateErrorMessage();

}

// Copyright (c) 2011-2022 Saxonica Limited