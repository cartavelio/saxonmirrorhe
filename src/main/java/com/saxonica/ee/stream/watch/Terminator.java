package com.saxonica.ee.stream.watch;

import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.transpile.CSharpModifiers;

/**
 * A Terminator provides a method that can be called to cause early termination of a streaming pass
 * over an input document, in the case where no more information from the document is needed.
 *
 * <p>The default implementation does nothing when the terminate() method is called</p>
 */
public class Terminator {

    private static final Terminator THE_INSTANCE = new Terminator();

    public static Terminator getInstance() {
        return THE_INSTANCE;
    }

    @CSharpModifiers(code={"public", "virtual"})
    public void terminate() throws QuitParsingException {
        // do nothing
    }
}

// Copyright (c) 2018-2023 Saxonica Limited