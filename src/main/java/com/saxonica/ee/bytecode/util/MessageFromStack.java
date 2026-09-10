package com.saxonica.ee.bytecode.util;

/**
 * Helper class to generate a run-time exception using a message that is present at the top of the bytecode stack
 */
public class MessageFromStack implements MessageBuilder {

    private static final MessageFromStack THE_INSTANCE = new MessageFromStack();

    public static MessageFromStack getInstance() {
        return THE_INSTANCE;
    }

    private MessageFromStack() {
    }

    @Override
    public void generateErrorMessage() {
        // null action: the message is already on the stack
    }

}

// Copyright (c) 2011-2022 Saxonica Limited