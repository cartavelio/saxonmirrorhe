package com.saxonica.ee.bytecode.util;

/**
 * Helper class to build messages in which the value of run-time bytecode variables can be inserted.
 */
public class MessageTemplate implements MessageBuilder {

    /*@NotNull*/
    Generator ga;
    String messageTemplate = null;
    int instanceVar;

    /**
     * Create a message template with a single parameter
     * @param ga the Generator
     * @param messageTemplate a message with a placeholder for the dynamic insertion in the form "{$1}"
     * @param param0 the slot number on the bytecode stack of the variable whose value is to be inserted
     * into the message; or -1 to indicate that it is to be taken from the top of the bytecode stack
     */

    public MessageTemplate(Generator ga, String messageTemplate, int param0) {
        this.messageTemplate = messageTemplate;
        this.instanceVar = param0;
        this.ga = ga;
    }

    @Override
    public void generateErrorMessage() {
        ga.push(messageTemplate);
        if (instanceVar == -1) {
            ga.swap();
        } else {
            ga.loadLocal(instanceVar);
        }
        ga.invokeStaticMethod(Callback.class, "makeMessageFromTemplate", String.class, Object.class);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited